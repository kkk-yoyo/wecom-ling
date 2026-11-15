# -*- coding: utf-8 -*-
"""第 5 章：事件映射。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "5", "事件映射", page_break=True)
    K.add_para(doc, "微灵把企业微信的原始事件翻译为 OneBot v11 事件后推送给对端。本章逐类给出映射关系、"
                    "字段来源与过滤规则。**未列出的微灵事件一律不上报**，仅在日志中记录一次。")

    K.add_h2(doc, "5.1", "通用规则")
    K.add_bullet(doc, "**会话方向**：优先解析微灵事件的 `conversation_id`。`R:<room_id>` 视为群聊，"
                       "`group_id` 取 `<room_id>`；`S:<peer>_<self>` 视为私聊，`user_id` 取 `<peer>`。"
                       "解析失败时按 `conversation_type` 判断：`group` 为群，其余（含 `customer_service`）为私聊，"
                       "对端用户取 `sender`。")
    K.add_bullet(doc, "**过滤自身消息**：`is_self` 为真，或 `sender` 等于 `self_id` 时，消息事件直接丢弃。")
    K.add_bullet(doc, "**顶层字段**：所有上报的业务事件都带 `time`、`self_id`、`post_type`，以及 `ling`（原始微灵事件，"
                       "见 5.8 节）。")
    K.add_bullet(doc, "**时间**：微灵 `send_time` 为毫秒（大于 1e11）时自动除以 1000；缺失或为 0 时取当前时间。")
    K.add_bullet(doc, "**昵称**：取微灵事件的 `sender_name`；`sender.card` 与 `nickname` 相同，"
                       "`sender.role` 固定为 `member`（微灵事件不提供群角色）。")

    K.add_h2(doc, "5.2", "消息事件总览")
    K.add_para(doc, "下表为消息类事件的一览，详细字段见 5.3 节：")
    K.add_table(
        doc,
        ["微灵事件号", "含义", "OneBot 映射", "关键字段/消息段"],
        [
            ["11041", "文本消息", "message（group/private）", "at 段（群且 `at_list`）+ text 段"],
            ["11042", "图片消息", "message + image 段", "`file`、`url`（cdn.url 优先）"],
            ["11043", "视频消息", "message + video 段", "`file`、`url`"],
            ["11044", "语音消息", "message + record 段", "`file` 取 `c2c_cdn.file_id`"],
            ["11045", "文件消息", "message + file 段", "`name`、`file`、可选 `url`（CDN 直链）"],
            ["11046", "位置消息", "message + location 段", "`lat`、`lon`、`title`、`content`（取 `address`）"],
            ["11047", "链接消息", "message + share 段", "`url`、`title`、`content`（取 `desc`）、可选 `image`"],
            ["11048", "动图（GIF）", "message + image 段", "`file`、`url`"],
            ["11049", "红包", "message + text 段", "占位文本 `[红包] <描述>`"],
            ["11050", "名片", "message + contact 段（user_id 为数字时）", "`type: qq`、`id`（数字 user_id）；否则降级文本"],
            ["11066", "小程序", "message + text 段", "占位文本 `[小程序] <标题>`"],
            ["11068", "图文", "message + text 段", "占位文本 `[图文] <文本内容>`"],
            ["11124", "视频号", "message + text 段", "占位文本 `[视频号] <描述> <URL>`"],
            ["11195", "视频号直播", "message + text 段", "占位文本 `[视频号直播] <描述>`"],
            ["11200", "未映射消息（5.0.13.6005）", "message（尽力透传 text + at）",
             "`read_receipt`/已读上报被过滤；正文取 `content`/`text`/`content_summary`，都为空时用 `[未映射消息] <类型>`"],
        ],
        widths=[2.4, 2.8, 5.0, 5.8],
        caption="表 5-1　消息事件映射",
        mono_cols=[0],
    )
    K.add_callout(doc, "info", "图片、视频、文件只有在微灵事件自带 CDN 直链（`cdn.url`）时才能被对端下载；"
                               "同时微灵会把该直链登记到媒体缓存，`get_image`/`get_record`/`get_file` 可凭消息段中的 "
                               "`file` 值按需取回（见 7.6 节与第 8 章）。位置、链接、名片分别映射为 location / share / contact 段，详见 5.3.3 节与第 6 章。", title="媒体可用性")
    K.add_callout(doc, "info", "11200 是微灵 5.0.13.6005 的「未映射消息」兜底类型：能识别出正文与 `at_list` 的按普通消息透传；"
                               "`read_receipt`/已读上报等非聊天内容会被过滤并记日志，避免污染 OneBot 消息流。", title="未映射消息（11200）")

    K.add_h2(doc, "5.3", "消息段字段详解")
    K.add_h3(doc, "5.3.1", "文本与 @（11041）")
    K.add_para(doc, "文本消息在群聊中若带 `at_list`，先生成 `at` 段，再生成 `text` 段；私聊只生成 `text` 段。"
                    "`at` 段的 `qq` 为数字字符串。`at_list` 中只有 `user_id` 可解析为数字时才生成 `at` 段；@自己也会保留。`rich_text` 不逐段解析：只取 `content`/`text` 与 `at_list`（见第 6 章）。")
    K.add_code(doc, [
        "{",
        "  \"message\": [",
        "    { \"type\": \"at\", \"data\": { \"qq\": \"168800001\" } },",
        "    { \"type\": \"text\", \"data\": { \"text\": \" 请查收\" } }",
        "  ],",
        "  \"raw_message\": \"@168800001 请查收\"",
        "}",
    ], caption="代码 5-1　文本 + @ 的消息段")
    K.add_bullet(doc, "群消息带 `at_list` 时，`at` 段在前；文本内容取 `content`（缺失时取 `text`）。")
    K.add_bullet(doc, "`raw_message` 中 @ 的形式为 `@<数字QQ>`，**不是**昵称；需要昵称请读取 `at` 段或 `sender`。")
    K.add_bullet(doc, "`text` 段内容为空时仍会生成（消息只含 @ 的场景），由对端自行处理。")

    K.add_h3(doc, "5.3.2", "图片、动图、视频、语音、文件")
    K.add_table(
        doc,
        ["事件", "消息段", "data 字段", "取值来源"],
        [
            ["11042 图片", "image", "`file`, `url`", "`url` 优先 `cdn.url`，其次 `md_url`、`ld_url`；`file` 取 `cdn.md5`、`cdn.file_name` 或 `cdn.url`"],
            ["11048 动图", "image", "`file`, `url`", "`file` 取 `name`，`url` 取事件中的 `url`"],
            ["11043 视频", "video", "`file`, `url`", "`url` 取 `cdn.url`；`file` 取 `cdn.file_name` 或 `cdn.md5`"],
            ["11044 语音", "record", "`file`", "取 `c2c_cdn.file_id`；不提供 URL"],
            ["11045 文件", "file", "`name`, `file`, 可选 `url`", "`name` 取 `cdn.file_name`（缺失用 `file`）；`file` 优先 `cdn.url`，无 URL 时为文件名；有 `cdn.url` 时附加 `url` 并登记媒体缓存（键为 `server_id`/`local_id`）"],
        ],
        widths=[2.6, 2.0, 4.4, 7.0],
        caption="表 5-2　媒体消息段字段",
        mono_cols=[1, 2],
    )
    K.add_bullet(doc, "无 URL 时不会生成媒体段：图片降级为文本 `[图片]`，视频为 `[视频]`，动图为 `[动图]`。")
    K.add_bullet(doc, "`record` 段只有 `file`（微灵 CDN `c2c_cdn.file_id`），**事件不带下载 URL**；"
                       "`get_record` 只有在对端直接传入 http(s) 直链时才可能成功，直接传 `file_id` 会返回失败（见 7.6 节）。")

    K.add_h3(doc, "5.3.3", "结构化消息段（位置、链接、名片）")
    K.add_bullet(doc, "**位置（11046）→ `location` 段**：`lat` 取 `latitude`/`lat`，`lon` 取 `longitude`/`lon`/`lng`，"
                       "`title` 取 `title`，`content` 取 `address`。")
    K.add_bullet(doc, "**链接（11047）→ `share` 段**：`url` 取 `url`，`title` 取 `title`，`content` 取 `desc`，"
                       "`image` 取 `image_url`（可选）；`url` 与 `title` 都为空时降级为文本。")
    K.add_bullet(doc, "**名片（11050）→ `contact` 段**：`type` 固定为 `qq`，`id` 为数字 `user_id`；"
                       "`user_id` 不是数字时降级为文本 `[名片] <昵称>`。")
    K.add_bullet(doc, "以上字段缺失时会给出空字符串；对端应做容错，不要把空串当作有效值。")

    K.add_h3(doc, "5.3.4", "消息公共字段")
    K.add_code(doc, [
        "{",
        "  \"time\": 1731452500,",
        "  \"self_id\": 168800001,",
        "  \"post_type\": \"message\",",
        "  \"message_type\": \"group\",",
        "  \"sub_type\": \"normal\",",
        "  \"message_id\": 88776601,",
        "  \"group_id\": 24680001,",
        "  \"user_id\": 168800002,",
        "  \"message\": [ ... ],",
        "  \"raw_message\": \"...\",",
        "  \"font\": 0,",
        "  \"sender\": { \"user_id\": 168800002, \"nickname\": \"张三\", \"card\": \"张三\", \"role\": \"member\" },",
        "  \"ling\": { ... }",
        "}",
    ], caption="代码 5-2　消息事件字段（群聊）")
    K.add_bullet(doc, "私聊事件带 `user_id`（对端账号）、不含 `group_id`；群聊事件带 `group_id`，发送者 ID 在 `sender.user_id`。")
    K.add_bullet(doc, "`message_id` 优先取微灵 `server_id`，其次 `message_id`、`local_id`；都缺失时使用微灵合成的自增值。"
                       "该值可能是数字或字符串，**不保证**能被微灵反查。")
    K.add_bullet(doc, "`sub_type`：群聊固定 `normal`，私聊固定 `friend`（微灵不区分好友来源）。")

    K.add_h2(doc, "5.4", "撤回与好友申请")
    K.add_table(
        doc,
        ["微灵事件号", "OneBot 事件", "关键字段", "说明"],
        [
            ["11123 撤回", "notice.group_recall / notice.friend_recall", "`message_id`、`operator_id`、`group_id`/`user_id`",
             "`room_id` 非空且不为 `0` 为群撤回，否则为私聊撤回"],
            ["11063 好友申请", "request.friend", "`user_id`、`comment`、`flag`",
             "`flag` 固定为 `ling-friend-<user_id>`；当前不自动接受"],
        ],
        widths=[2.6, 5.0, 4.6, 3.8],
        caption="表 5-3　撤回与好友申请",
        mono_cols=[0],
    )
    K.add_callout(doc, "limit", "好友申请只能“通知”对端，不能通过 OneBot 动作自动通过："
                                 "`set_friend_add_request` 与 `set_group_add_request` 均未实现（返回 500）。"
                                 "如需自动处理，请在微灵侧处理。", title="不支持自动通过好友")

    K.add_h2(doc, "5.5", "群事件与通知")
    K.add_table(
        doc,
        ["微灵事件号", "含义", "OneBot 事件", "关键字段与规则"],
        [
            ["11072", "群成员增加", "notice.group_increase（`sub_type: invite`）",
             "`group_id` 去 `R:` 前缀；`operator_id` 取 `op_user_id`；`user_id` 取 `member_list[0].user_id`，缺失用 `self_id`"],
            ["11074", "新增群（入群）", "notice.group_increase（`sub_type: invite`）", "规则同 11072"],
            ["11173", "新增群通知", "notice.group_increase（`sub_type: invite`）", "规则同 11072/11074（按成员增加上报）"],
            ["11073", "群成员减少", "notice.group_decrease",
             "`sub_type`：`operator_id` 等于被移除成员时为 `leave`，否则为 `kick`"],
            ["11075", "主动退群", "notice.group_decrease（`sub_type: leave`）", "`operator_id` 与 `user_id` 均为 `self_id`"],
            ["11076", "好友新增", "notice.friend_add", "`user_id` 为新增好友"],
            ["11077", "好友删除", "notice.notify（`sub_type: friend_delete`）", "`user_id` 为被删除好友"],
            ["11078", "群名称变化", "notice.notify（`sub_type: group_name_change`）", "`group_id`、`group_name`（新群名）"],
        ],
        widths=[2.2, 2.8, 5.2, 5.8],
        caption="表 5-4　群事件与通知映射",
        mono_cols=[0],
    )
    K.add_bullet(doc, "所有 notice 事件均带 `group_id` 或 `user_id`（视场景），以及 `ling` 原始事件。")
    K.add_bullet(doc, "`group_increase` 的 `sub_type` 固定为 `invite`，微灵不提供“主动加入”的区分。")
    K.add_bullet(doc, "群成员事件中操作者昵称、群名等原始信息可从 `ling` 中读取。")

    K.add_h2(doc, "5.6", "登录与状态事件")
    K.add_para(doc, "登录相关事件用于维护 `self_id`，**本身不作为 OneBot 事件上报**（对端通过 lifecycle/heartbeat "
                    "即可感知连接状态）：")
    K.add_table(
        doc,
        ["微灵事件号", "含义", "微灵侧处理", "是否上报"],
        [
            ["11026", "登录成功", "用 `data.user_id`、`username`/`name` 更新 `self_id` 与昵称", "否"],
            ["11179", "登录成功（含公司名）", "规则同 11026", "否"],
            ["11027", "退出登录", "清空 `self_id`，暂停发起连接（等待重新登录）", "否"],
        ],
        widths=[2.4, 3.4, 7.4, 2.8],
        caption="表 5-5　登录事件处理",
        mono_cols=[0],
    )
    K.add_bullet(doc, "若对端需要“账号登录成功”的明确信号，可在收到生命周期连接事件后调用 `get_login_info`。")
    K.add_bullet(doc, "`self_id` 的获取顺序：内存中的登录事件 → HTTP 探测（11035）→ 两者都失败则不连接。")
    K.add_bullet(doc, "重复登录会覆盖 `self_id`；退出登录后连接会因 `self_id` 为空而停止重连。")

    K.add_h2(doc, "5.7", "被忽略的事件")
    K.add_para(doc, "以下事件在改造版中被忽略（不上报、不影响连接），其中部分保留为原始事件供 `ling` 追溯：")
    K.add_table(
        doc,
        ["微灵事件号", "含义", "忽略原因"],
        [
            ["1000 / -1000", "框架内控制/错误事件", "非业务事件"],
            ["11024", "接口就绪", "内部状态事件"],
            ["11027", "退出登录", "仅用于清空 self_id"],
            ["11028", "登录二维码", "登录过程事件，非消息"],
            ["11122", "自己的二维码", "登录过程事件"],
            ["11153", "刷新二维码", "登录过程事件"],
            ["11174", "二维码状态", "登录过程事件"],
            ["11201", "连接断开", "由 WS 连接状态体现"],
            ["11202", "连接恢复", "由 WS 连接状态体现"],
        ],
        widths=[3.0, 5.0, 8.0],
        caption="表 5-6　忽略事件清单",
        mono_cols=[0],
    )
    K.add_bullet(doc, "`11200` 不再忽略：可识别正文与 `at_list` 的按消息透传（见 5.2 节）；其中的 `read_receipt`/已读上报仍会被过滤。")
    K.add_bullet(doc, "每种未知事件类型只在首次出现时记录一条日志，避免刷屏；日志中可见事件号与原始 JSON。")
    K.add_bullet(doc, "若业务确实需要某个被忽略事件，可在 `ling` 机制之外扩展 `TranslateEvent`，"
                       "但改造版首版不做此承诺。")

    K.add_h2(doc, "5.8", "ling：原始微灵事件")
    K.add_para(doc, "每条上报的 OneBot 事件都附带 `ling` 字段，内容为原始微灵事件。它使对端可以访问 "
                    "OneBot v11 字段之外的信息（例如群名、操作者昵称、CDN 鉴权字段），代价是事件体积变大。")
    K.add_code(doc, [
        "\"ling\": {",
        "  \"type\": 11041,",
        "  \"client\": 1,",
        "  \"robot_id\": 168800001,",
        "  \"data\": {",
        "    \"conversation_id\": \"R:24680001\",",
        "    \"sender\": 168800002,",
        "    \"sender_name\": \"张三\",",
        "    \"content\": \"在吗\",",
        "    \"send_time\": 1731452500000,",
        "    \"server_id\": 88776601",
        "  }",
        "}",
    ], caption="代码 5-3　ling 原始事件（示意）")
    K.add_bullet(doc, "`ling` 仅剔除 `data.payload_base64`（大体积 base64 内容），其余字段原样保留。")
    K.add_bullet(doc, "`ling` 的结构随微灵版本变化，对端不应把它作为稳定协议使用；OneBot 字段才是契约。")
    K.add_bullet(doc, "如对端对事件体积敏感，可在消费端忽略 `ling`；微灵侧不提供关闭开关。")
