# -*- coding: utf-8 -*-
"""第 7 章：动作接口。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "7", "动作接口", page_break=True)
    K.add_para(doc, "OneBot 实现通过动作（action）请求微灵执行操作。本章列出改造版支持的全部动作、"
                    "参数与返回结构，以及失败时的错误码。总原则：**查询类动作尽力返回可用数据；"
                    "发送类动作支持文本、@ 与图片/视频/文件/动图，语音除外**。"
                    "消息能力与降级规则的速查见第 6 章。")

    K.add_h2(doc, "7.1", "支持的动作总览")
    K.add_table(
        doc,
        ["action", "数据来源", "说明"],
        [
            ["`send_private_msg`", "微灵指令 11029", "发送私聊文本/文本+@（@ 在私聊无实际意义，按文本处理）"],
            ["`send_group_msg`", "微灵指令 11069 或 11029", "群聊：带 @ 用 11069，否则用 11029；含媒体时先发媒体"],
            ["`send_msg`", "同上", "按 `message_type` 分发到群/私聊，缺省按私聊处理"],
            ["`get_login_info`", "内存（登录事件/探测）", "返回 `user_id`、`nickname`"],
            ["`get_status`", "连接状态", "返回 `online`、`good`"],
            ["`get_version_info`", "静态信息", "`app_name=wecom-ling-onebot`、`app_version=1.11.0`"],
            ["`get_group_list`", "微灵指令 11038", "返回 `[{group_id, group_name}]`"],
            ["`get_friend_list`", "微灵指令 11036", "返回 `[{user_id, nickname, remark}]`"],
            ["`get_group_info`", "11038 结果过滤", "从群列表中匹配，取不到时 `group_name` 为空"],
            ["`get_group_member_info`", "微灵指令 11040", "按 `group_id` + `user_id` 查询单个成员"],
            ["`get_group_member_list`", "微灵指令 11040（带 `getData:true`）", "返回整个成员数组"],
            ["`get_stranger_info`", "微灵指令 11039", "按 `user_id` 查询陌生人信息"],
            ["`get_msg`", "本地消息缓存（500 条）", "只能查到本进程缓存过的消息"],
            ["`get_group_msg_history`", "本地实现", "固定返回空数组 `[]`（微灵无历史消息接口）"],
            ["`can_send_image`", "固定能力", "返回 `{yes: true}`"],
            ["`can_send_record`", "固定能力", "返回 `{yes: true}`（协议层能力查询，实际发送语音仍不支持）"],
            ["`get_image` / `get_record` / `get_file`", "媒体缓存 + 按需下载", "命中 CDN 直链时下载并返回 `base64://`，见 7.6 节"],
            ["`get_group_file_url` / `get_private_file_url`", "媒体缓存（CDN 直链）", "返回微灵 CDN 原始 URL，见 7.6 节"],
        ],
        widths=[6.2, 4.6, 5.2],
        caption="表 7-1　支持的动作",
        mono_cols=[0],
    )
    K.add_callout(doc, "info", "动作请求**不需要**传 `client` 参数，也不需要传 `getData`。微灵使用事件中记录的 "
                               "client token（默认 1）与微灵通信；查询类指令（11038/11036/11040/11039）由 shim "
                               "自动附加 `getData:true`。对端只需按标准 OneBot v11 传业务参数。", title="无需 client/getData 参数")

    K.add_h2(doc, "7.2", "发送消息")
    K.add_h3(doc, "7.2.1", "参数与消息解析")
    K.add_bullet(doc, "`send_group_msg` 必填 `group_id`；`send_private_msg` 必填 `user_id`；二者均必填 `message`。")
    K.add_bullet(doc, "`message` 支持字符串（按纯文本处理）与消息段数组。")
    K.add_bullet(doc, "识别的段类型：`text`、`at`、`image`、`video`、`file`、GIF（`image` 且 `type`/`sub_type` 含 gif，"
                       "或文件扩展名为 `.gif`）。")
    K.add_bullet(doc, "`text` 拼接为正文；`at` 收集 `qq` 值（数字 ID 会以数字形式写入 `at_list`）；"
                       "`at` 的 `qq` 为 `all` 时无法 @ 全体，退化为纯文本；`reply` 与 `face` 段被静默忽略。")
    K.add_bullet(doc, "`record` 段不支持（微灵没有语音发送指令）；`json`/`xml`/`forward`/`node` 等其余类型返回失败。")
    K.add_bullet(doc, "正文为空且没有任何可发送段时返回失败 `empty message`；只有 @（无文本）也是合法消息。")
    K.add_bullet(doc, "群消息带至少一个 `at` 时走指令 11069；否则走 11029。")
    K.add_bullet(doc, "私聊会话 ID 为 `S:<user_id>_<self_id>`，群会话 ID 为 `R:<group_id>`。")
    K.add_bullet(doc, "含媒体的消息会拆成多条微灵命令：媒体先发（图片 11030、视频 11067、文件 11031、动图 11070），"
                       "文本/@ 后发；媒体文件落在微灵程序目录的 `onebot_media_tmp` 临时目录，详见 8.2 节。")
    K.add_bullet(doc, "`onebot_media_tmp` 会随发送量增长并占用磁盘（单文件最大约 32MB，后台清理 1 小时前的文件）；"
                       "高频发送媒体时请关注该目录体积。")

    K.add_h3(doc, "7.2.2", "请求与响应示例")
    K.add_code(doc, [
        "{",
        "  \"action\": \"send_group_msg\",",
        "  \"params\": {",
        "    \"group_id\": 24680001,",
        "    \"message\": [",
        "      { \"type\": \"at\", \"data\": { \"qq\": \"168800001\" } },",
        "      { \"type\": \"text\", \"data\": { \"text\": \" 收到了\" } },",
        "      { \"type\": \"image\", \"data\": { \"file\": \"C:\\\\media\\\\ok.png\" } }",
        "    ]",
        "  },",
        "  \"echo\": \"send-1\"",
        "}",
    ], caption="代码 7-1　发送群消息（带 @ 与图片）")
    K.add_code(doc, [
        "{",
        "  \"status\": \"ok\",",
        "  \"retcode\": 0,",
        "  \"msg\": \"\",",
        "  \"data\": { \"message_id\": 88776602 },",
        "  \"echo\": \"send-1\"",
        "}",
    ], caption="代码 7-2　发送成功响应")
    K.add_table(
        doc,
        ["失败场景", "`retcode`", "`msg` 示例"],
        [
            ["缺少群号/用户号", "500", "`missing group_id` / `missing user_id`"],
            ["没有任何可发送内容", "500", "`empty message`"],
            ["发送语音", "500", "`暂不支持发送语音：微灵未提供语音发送指令（可改用文本或文件）`"],
            ["包含其它不支持的段", "500", "`当前版本不支持发送 json 消息段（支持：文本、@、图片、视频、文件、动图）`"],
            ["媒体源不可用", "500", "`媒体下载失败: <url>` / `媒体文件不存在` / `媒体文件路径必须是绝对路径`"],
            ["媒体超过大小限制", "500", "`媒体内容超过 32MB 限制`"],
            ["微灵侧发送失败", "500", "`发送媒体失败(image): ...` / `微灵未返回结果（超时或未登录）`"],
            ["未知动作", "1404", "`unsupported action: <action>`"],
        ],
        widths=[4.4, 2.0, 9.6],
        caption="表 7-2　发送类动作失败信息",
    )
    K.add_bullet(doc, "返回的 `message_id` 优先取微灵结果中的 `server_id`/`message_id`/`local_id`/`msg_id`；"
                       "都缺失时返回微灵合成的自增 ID（从 100001 起），不能用于反查。含媒体时 `message_id` 为最后一条命令的结果。")
    K.add_bullet(doc, "发送动作在接收循环内串行执行，单次最长等待微灵 60s。")

    K.add_h2(doc, "7.3", "账号与状态")
    K.add_table(
        doc,
        ["action", "返回 data", "说明"],
        [
            ["`get_login_info`", "`{ user_id, nickname }`", "昵称取登录事件中的 `username`/`name`，缺失时用微灵客户端名"],
            ["`get_status`", "`{ online, good }`", "`online` 为“已连接且 self_id 非空”，`good` 恒为 true"],
            ["`get_version_info`", "`{ app_name, app_version, protocol_version, app_full_name }`",
             "`wecom-ling-onebot` / `1.11.0` / `v11` / `微灵 OneBot v11 改造版`"],
            ["`get_group_msg_history`", "`[]`（空数组）", "接口存在但微灵没有历史消息查询能力"],
        ],
        widths=[4.6, 5.6, 5.8],
        caption="表 7-3　账号与状态动作",
        mono_cols=[0],
    )
    K.add_code(doc, [
        "{",
        "  \"status\": \"ok\",",
        "  \"retcode\": 0,",
        "  \"msg\": \"\",",
        "  \"data\": {",
        "    \"app_name\": \"wecom-ling-onebot\",",
        "    \"app_version\": \"1.11.0\",",
        "    \"protocol_version\": \"v11\",",
        "    \"app_full_name\": \"微灵 OneBot v11 改造版\"",
        "  }",
        "}",
    ], caption="代码 7-3　get_version_info 响应")

    K.add_h2(doc, "7.4", "群与好友查询")
    K.add_table(
        doc,
        ["action", "必填参数", "微灵指令", "返回字段", "备注"],
        [
            ["`get_group_list`", "无", "11038", "`[{group_id, group_name}]`", "失败或个微通道返回 `[]`（`status` 仍为 ok）"],
            ["`get_friend_list`", "无", "11036", "`[{user_id, nickname, remark}]`", "`remark` 恒为空字符串；个微通道返回 `[]`"],
            ["`get_group_info`", "`group_id`", "复用 11038", "`{group_id, group_name}`", "遍历群列表匹配，取不到时 `group_name` 为空"],
            ["`get_group_member_info`", "`group_id`, `user_id`", "11040", "`user_id, nickname, card, sex, age, role`",
             "`sex` 为 `unknown`、`age` 为 0、`role` 为 `member`；个微通道返回默认值"],
            ["`get_group_member_list`", "`group_id`", "11040 + `getData:true`", "`[{user_id, nickname, card, sex, age, role}]`",
             "一次返回全部成员；个微通道返回 `[]`"],
            ["`get_stranger_info`", "`user_id`", "11039", "`user_id, nickname, sex, age`", "字段多由微灵返回结果映射，缺失时给默认值"],
        ],
        widths=[3.5, 2.3, 2.1, 3.9, 4.2],
        caption="表 7-4　群与好友查询动作",
        mono_cols=[0, 2],
    )
    K.add_code(doc, [
        "{",
        "  \"action\": \"get_group_list\",",
        "  \"params\": {},",
        "  \"echo\": \"g1\"",
        "}",
        "",
        "// 响应",
        "{",
        "  \"status\": \"ok\",",
        "  \"retcode\": 0,",
        "  \"msg\": \"\",",
        "  \"data\": [",
        "    { \"group_id\": 24680001, \"group_name\": \"测试群\" },",
        "    { \"group_id\": 24680002, \"group_name\": \"另一个群\" }",
        "  ],",
        "  \"echo\": \"g1\"",
        "}",
    ], caption="代码 7-4　获取群列表")
    K.add_bullet(doc, "群号统一为去 `R:` 前缀后的数字字符串/数字，对端可直接使用。")
    K.add_bullet(doc, "`get_group_info`、`get_group_member_info`、`get_group_member_list` 依赖微灵群列表/成员列表接口，"
                       "首次调用可能较慢（微灵需向企业微信拉取）。")

    K.add_h2(doc, "7.5", "消息查询与能力")
    K.add_bullet(doc, "`get_msg`：按 `message_id` 查询**本进程缓存**的消息事件（最多 500 条，先进先出）；"
                       "命中返回完整的 message 事件 JSON（含 `ling`），未命中返回失败 `message not found: <id>`。")
    K.add_bullet(doc, "`get_group_msg_history`：接口兼容存在，但微灵不提供历史消息，固定返回空数组 `[]`。"
                       "缓存只包含改造版上报过的消息，重启微灵后清空。")
    K.add_bullet(doc, "`can_send_image` / `can_send_record` 固定返回 `{ \"yes\": true }`，表示协议层面的能力查询结果；"
                       "实际出站图片/视频/文件/动图已支持（见 8.2 节），语音（`record`）仍不支持。")

    K.add_h2(doc, "7.6", "媒体与文件动作")
    K.add_callout(doc, "info", "微灵会把入站消息自带的 CDN 直链登记到媒体缓存；下列动作在命中缓存（或调用方直接"
                                "传入 http(s) URL）时按需下载并返回 `base64://`，或直接返回 CDN 原始地址。"
                                "无直链、下载失败或超出限制时返回失败。出站媒体发送见 8.2 节。", title="说明")
    K.add_table(
        doc,
        ["action", "入参", "成功返回", "失败场景"],
        [
            ["`get_image`", "`file`（兼容 `file_id`）", "`{file: \"base64://...\", file_name}`", "failed / 500：`media not available: <file>`"],
            ["`get_record`", "`file`（兼容 `file_id`）", "`{file: \"base64://...\", file_name}`", "failed / 500：`media not available: <file>`（语音事件无直链）"],
            ["`get_file`", "`file`（兼容 `file_id`）", "`{file: \"base64://...\", file_name}`", "failed / 500：`media not available: <file>`"],
            ["`get_group_file_url`", "`file_id`（兼容 `file`）", "`{url, file_name}`，`url` 为 CDN 直链", "failed / 500：`file url not available ...`"],
            ["`get_private_file_url`", "`file_id`（兼容 `file`）", "`{url, file_name}`，`url` 为 CDN 直链", "failed / 500：`file url not available ...`"],
        ],
        widths=[4.2, 2.8, 4.4, 4.6],
        caption="表 7-5　媒体动作与行为",
        mono_cols=[0, 1],
    )
    K.add_bullet(doc, "`get_image`/`get_record`/`get_file` 优先读参数 `file`，缺失时读 `file_id`；命中缓存后按需 HTTP "
                       "下载（15s 超时，单文件最多约 32MB，超出会截断），返回 `base64://` 内容与 `file_name`。")
    K.add_bullet(doc, "缓存键来自入站消息段：图片为 `cdn.md5`/`cdn.file_name`，动图为 `name`，视频为 "
                       "`cdn.file_name`/`cdn.md5`，文件为 `server_id`/`local_id`；对端应原样使用事件消息段里的 `file` 值。")
    K.add_bullet(doc, "`get_group_file_url`/`get_private_file_url` 优先读 `file_id`，缺失时读 `file`；命中时直接返回"
                       "微灵 CDN 原始 URL（见 8.3 节），不依赖本地媒体服务。")
    K.add_bullet(doc, "`get_record` 最特殊：11044 语音事件只带 `file_id`、不带 CDN 地址，因此只有调用方把 `file` "
                       "传成 http(s) 直链时才可能成功。")
    K.add_bullet(doc, "本地媒体服务（`127.0.0.1:18890–18920`）仅在缓存到**字节**时兜底启动；当前版本没有写入字节"
                       "缓存的调用路径，正常路径不会启动。")

    K.add_h2(doc, "7.7", "不支持的动作与错误处理")
    K.add_para(doc, "对不支持的动作，微灵返回失败响应而不是关闭连接。两类错误的区别如下：")
    K.add_table(
        doc,
        ["类别", "`retcode`", "`msg`", "示例 action"],
        [
            ["显式不支持（已知但未实现）", "500", "`action not supported by 微灵 OneBot shim: <action>`",
             "`set_group_name`、`set_group_card`、`set_group_ban`、`set_friend_add_request`、`set_group_add_request`"],
            ["未知动作", "1404", "`unsupported action: <action>`", "其他任何未列入表 7-1 的 action"],
        ],
        widths=[4.6, 2.0, 5.2, 4.2],
        caption="表 7-6　不支持动作的两种返回",
        mono_cols=[1],
    )
    K.add_bullet(doc, "对端应按 `retcode` 区分：1404 通常是客户端写错动作名，500 多为能力/参数问题。")
    K.add_bullet(doc, "返回 `failed` 时 `data` 为 null；`echo` 仍按请求原样返回，便于对端关联。")
    K.add_bullet(doc, "动作执行异常不会导致连接断开；但长时间阻塞会推迟后续事件与动作的处理。")
