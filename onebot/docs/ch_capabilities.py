# -*- coding: utf-8 -*-
"""第 6 章：消息能力矩阵。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "6", "消息能力矩阵", page_break=True)
    K.add_para(doc, "本章用矩阵形式回答一个实际问题：**哪些微灵消息能变成 OneBot 消息段，哪些 OneBot 段能发到微灵**。"
                    "字段级细节见第 5 章与第 7 章，媒体落盘细节见第 8 章。"
                    "✅ 表示完整支持，⚠️ 表示有限支持，— 表示不支持。")

    K.add_h2(doc, "6.1", "接收方向：微灵 → OneBot")
    K.add_para(doc, "微灵事件进入 shim 后，先按事件号映射为 OneBot 事件，再按消息字段生成消息段数组：")
    K.add_table(
        doc,
        ["微灵事件", "OneBot 表现", "关键字段 / 降级规则", "支持"],
        [
            ["11041 文本", "message + `text` 段（可含 `at` 段）", "`at_list` 映射为 `at` 段（含 @自己）；正文取 `content`/`text`；"
                                                                  "`rich_text` 也有 `at_list` 与 `content`", "✅"],
            ["11042 图片", "message + `image` 段", "`file` 取 `cdn.md5`/`file_name`/`url`；`url` 取 `cdn.url`（回退 `md_url`/`ld_url`）", "✅"],
            ["11043 视频", "message + `video` 段", "`file` 取 `cdn.file_name`/`md5`；`url` 取 `cdn.url`", "✅"],
            ["11044 语音", "message + `record` 段", "只有 `file` = `c2c_cdn.file_id`，无下载 URL", "⚠️ 有限"],
            ["11045 文件", "message + `file` 段", "`name`/`file`/可选 `url`（CDN 直链）；无 `file_id` 字段", "✅"],
            ["11046 位置", "message + `location` 段", "`lat`/`lon`/`title`/`content`（`content` 取 `address`）", "✅"],
            ["11047 链接", "message + `share` 段", "`url`/`title`/`content`（取 `desc`）/`image`（取 `image_url`）", "✅"],
            ["11048 动图", "message + `image` 段（GIF）", "`file` 取 `name`；`url` 取事件 `url`", "✅"],
            ["11050 名片", "message + `contact` 段", "`type=qq`、`id=user_id`（仅当 `user_id` 为数字）；否则降级文本", "✅"],
            ["11049 红包", "message + `text` 段", "占位文本 `[红包] <描述>`", "✅ 降级"],
            ["11066 小程序", "message + `text` 段", "占位文本 `[小程序] <标题>`", "✅ 降级"],
            ["11068 图文", "message + `text` 段", "占位文本 `[图文] <文本内容>`", "✅ 降级"],
            ["11124 视频号", "message + `text` 段", "占位文本 `[视频号] <描述> <链接>`", "✅ 降级"],
            ["11195 视频号直播", "message + `text` 段", "占位文本 `[视频号直播] <描述>`", "✅ 降级"],
            ["11123 撤回", "notice（`group_recall` / `friend_recall`）", "`message_id`、`operator_id`、群聊带 `group_id`", "✅"],
            ["11063 好友申请", "request（`request_type=friend`）", "`user_id`、`comment`、`flag`", "✅"],
            ["11072 / 11073 / 11074 / 11075 / 11076 / 11077 / 11078 / 11173", "notice（群成员增删、好友增删、群名变化）",
             "详见 5.5 节；11173/11074 映射为 `group_increase`（`sub_type=invite`）", "✅"],
            ["11200 未映射消息", "message（尽力透传 `text` + `at`）", "`read_receipt`/已读上报被过滤；正文取 `content`/`text`/`content_summary`，否则占位 `[未映射消息] <类型>`", "✅ 降级"],
            ["11026 / 11179 / 1000 等", "不上报 OneBot 事件", "仅内部状态（`self_id`/client 等），不进入事件流", "—"],
        ],
        widths=[3.5, 3.6, 7.4, 1.5],
        caption="表 6-1　接收方向能力矩阵",
        mono_cols=[],
        center_cols=[3],
    )

    K.add_h2(doc, "6.2", "发送方向：OneBot → 微灵")
    K.add_para(doc, "动作与消息段进入 shim 后，按微灵可执行的指令发送；媒体会先落地为本地临时文件（见 6.3 节）：")
    K.add_table(
        doc,
        ["OneBot 消息段 / 行为", "微灵指令或处理", "说明", "支持"],
        [
            ["`text`", "11029（群/私聊）", "`content` 拼接为正文", "✅"],
            ["`at`", "群内 11069（`at_list`）", "`qq` 非空且非 `all` 时加入 `at_list`（数字以数字形式、非数字保留字符串）；私聊中的 `at` 不单独发送", "✅"],
            ["`at` + `qq=all`（@全体）", "降级为纯文本", "微灵 `at_list` 无法表达 @全体，退化为普通文本（11029）", "✅ 降级"],
            ["`reply`（引用回复）", "静默忽略", "AstrBot 默认回复带 `reply` 段；不报错，正文照常发送", "✅ 忽略"],
            ["`face`（表情）", "静默忽略", "不写入微灵内容，其余段照常发送", "✅ 忽略"],
            ["`image`", "11030", "先落地为 `onebot_media_tmp` 临时文件，传绝对路径", "✅"],
            ["`video`", "11067", "同上", "✅"],
            ["`file`", "11031", "同上；额外传 `file_name`", "✅"],
            ["GIF（`image` gif）", "11070", "同上", "✅"],
            ["`record`（语音）", "—", "微灵没有语音发送指令，返回 failed", "—"],
            ["`json`/`xml`/`forward`/`node` 等", "—", "返回 failed（`当前版本不支持发送 ... 消息段`）", "—"],
            ["个人微信通道", "仅 `send_text_msg`", "个微通道只支持文本；`at`、`reply`、`face` 与媒体均不发送", "⚠️ 文本"],
        ],
        widths=[3.8, 3.4, 7.3, 1.5],
        caption="表 6-2　发送方向能力矩阵",
        mono_cols=[],
        center_cols=[3],
    )
    K.add_callout(doc, "info", "一条 OneBot 消息里同时包含媒体与文本时，shim 会先发媒体（每条一个微灵指令），"
                                "再发文本/@；微灵侧会看到多条消息。详见 8.2 节。", title="媒体与文本分多条发送")

    K.add_h2(doc, "6.3", "媒体落盘与发送顺序")
    K.add_bullet(doc, "媒体来源支持 `base64://`、http(s) URL、`file://`、本地绝对路径；http(s) 源由 shim 下载。")
    K.add_bullet(doc, "内容统一写入微灵程序目录的 `onebot_media_tmp`，单文件上限约 32MB，下载超时 15s；"
                       "后台清理 1 小时前的旧文件。")
    K.add_bullet(doc, "临时文件会随发送量增长并占用磁盘：高频发图/发文件时请关注该目录体积，必要时手工清空。")
    K.add_bullet(doc, "同一会话的发送顺序为：媒体段按出现顺序逐条发送，随后发送文本/@ 段。")
    K.add_bullet(doc, "任一条媒体命令失败，动作整体返回 failed；已发出的媒体不会撤回。")

    K.add_h2(doc, "6.4", "能力边界速览")
    K.add_table(
        doc,
        ["能力", "结论", "备注"],
        [
            ["接收文本/@/图片/视频/文件/位置/链接/名片", "支持", "见 6.1 节"],
            ["接收语音", "有限", "只有 `file_id`，通常取不回内容"],
            ["接收红包/小程序/图文/视频号/直播", "降级为文本", "内容保留在占位文本与 `ling` 中"],
            ["发送文本/@/图片/视频/文件/动图", "支持", "见 6.2 节；媒体需可访问的源"],
            ["发送语音", "不支持", "微灵无对应指令"],
            ["引用回复、撤回、群管理、请求处理", "不支持", "动作返回 500/1404，见 7.7 节"],
            ["历史消息查询", "不支持", "`get_group_msg_history` 返回空数组"],
            ["图文混排精细解析", "不逐段解析", "`rich_text` 只取 `content`/`text` 与 `at_list`"],
        ],
        widths=[5.6, 2.6, 7.8],
        caption="表 6-3　能力边界速览",
    )
