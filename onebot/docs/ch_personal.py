# -*- coding: utf-8 -*-
"""第 9 章：个人微信通道（实验性）。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "9", "个人微信通道（实验性）", page_break=True)
    K.add_callout(doc, "limit", "个人微信通道的 OneBot v11 支持是**实验性**的：事件映射依赖启发式字段，"
                                 "能力远少于企业微信通道，且默认不启用。生产环境请优先使用企业微信通道。",
                  title="实验性能力")
    K.add_para(doc, "改造版为个人微信通道提供了独立的 OneBot v11 连接（与企微通道分开配置、分开连接）。"
                    "本章说明其启用方式、事件映射与能力边界。")

    K.add_h2(doc, "9.1", "启用方式")
    K.add_step(doc, 1, "在微灵界面切换到「个人微信」通道，完成个人微信登录。")
    K.add_step(doc, 2, "通信方式选择 **WebSocket**，并在该通道的地址栏填写 OneBot 实现的反向 WS 地址。")
    K.add_step(doc, 3, "填写 Token（与 OneBot 实现一致；可为空）。")
    K.add_step(doc, 4, "保存并启用。与企微通道不同，个微通道**没有默认地址**：地址为空时不会发起连接。")
    K.add_step(doc, 5, "在 OneBot 实现侧确认连接到来，并检查 `self_id` 是否正确（个微通道依赖事件携带 `robot_id` 或登录信息）。")
    K.add_bullet(doc, "个微通道与企微通道可以同时连接同一个 OneBot 实现，但建议使用不同适配器或通过 `X-Self-ID` 区分。")
    K.add_bullet(doc, "个微通道不做 HTTP `self_id` 探测，因此不会访问 8060 端口；`self_id` 完全依赖事件。")

    K.add_h2(doc, "9.2", "事件映射")
    K.add_para(doc, "个微通道的事件结构与企业微信不同，改造版采用启发式字段匹配：先看常见字段名，"
                    "取到即用。事件类型不是整数（或缺少可识别字段）时按消息处理。")
    K.add_table(
        doc,
        ["OneBot 字段", "个微事件字段（按优先级尝试）", "说明"],
        [
            ["发送者 `user_id`", "`from_user`、`from_wxid`、`wxid`、`sender`", "取首个非空值"],
            ["`group_id`", "`room_id`、`chatroom`、`room_wxid`", "非空即视为群聊"],
            ["消息文本", "`content`、`msg`、`text`、`message`", "取首个非空值，生成单个 `text` 段"],
            ["`message_id`", "`msg_id`、`message_id`、`MsgId`", "缺失时合成自增值"],
            ["`self_id`", "事件顶层 `robot_id`，或登录类事件的账号字段", "解析不到时不连接/不上报"],
        ],
        widths=[3.6, 7.0, 5.4],
        caption="表 9-1　个微事件字段映射（启发式）",
    )
    K.add_bullet(doc, "消息事件固定为 `message`，`message` 只含一个 `text` 段；`sub_type` 恒为 `friend`。")
    K.add_bullet(doc, "`room_id` 非空时 `message_type` 为 `group` 并带 `group_id`；否则为 `private` 并带 `user_id`。")
    K.add_bullet(doc, "事件同样附带 `ling`，保留个微原始事件，便于对端自行解析媒体等缺失信息。")
    K.add_bullet(doc, "由于字段是启发式的，不同微灵小版本或不同消息类型可能无法映射；此时事件被忽略。")

    K.add_h2(doc, "9.3", "动作支持")
    K.add_table(
        doc,
        ["能力", "行为", "说明"],
        [
            ["发送文本", "调用个微 `send_text_msg`，`wxid` 为私聊对象或群 ID", "仅文本；`at` 段会被丢弃（正文必须非空）"],
            ["群/好友列表", "返回空数组 `[]`", "个微通道不提供列表接口映射"],
            ["群信息", "返回 `{group_id, group_name: \"\"}`", "名称恒为空"],
            ["成员/陌生人信息", "返回默认值（`nickname` 为空等）", "不做真实查询"],
            ["`get_login_info` / `get_status` / `get_version_info`", "与企业微信通道一致", "账号信息取决于 self_id 与登录事件"],
            ["`get_msg`", "查本地消息缓存", "与企业微信通道共用缓存机制"],
            ["媒体与文件动作", "需传 http(s) 直链；个微事件不登记媒体缓存", "见 6.6、8.4 节"],
            ["不支持的动作", "与企业微信通道相同（500/1404）", "见 7.7 节"],
        ],
        widths=[5.0, 6.6, 4.4],
        caption="表 9-2　个微通道动作能力",
    )
    K.add_code(doc, [
        "{",
        "  \"action\": \"send_private_msg\",",
        "  \"params\": {",
        "    \"user_id\": \"wxid_abc123\",",
        "    \"message\": \"你好\"",
        "  },",
        "  \"echo\": \"p1\"",
        "}",
    ], caption="代码 9-1　向个微好友发送文本（内部转为 send_text_msg）")
    K.add_bullet(doc, "个微通道的标识符是 wxid 字符串，不是数字 QQ；对端不要对其做数字运算。")
    K.add_bullet(doc, "发送时群聊的 `group_id` 会被直接作为 `wxid` 传给 `send_text_msg`。")

    K.add_h2(doc, "9.4", "个微通道限制")
    K.add_table(
        doc,
        ["限制", "说明"],
        [
            ["实验性、默认不启用", "需要手动选择个人微信通道并填写地址；官方推荐企微通道"],
            ["事件映射启发式", "字段名变化即可能失效；媒体、引用、撤回等事件覆盖有限"],
            ["无列表/资料查询", "群列表、好友列表、成员信息均为空或默认值"],
            ["仅文本发送", "无法发送图片、语音、文件等"],
            ["依赖 self_id 事件", "事件中缺少 `robot_id`/账号字段时不会连接"],
            ["能力随版本变化", "个微协议本身不稳定，升级微灵后需重新验证"],
        ],
        widths=[5.0, 11.0],
        caption="表 9-3　个微通道限制",
    )
    K.add_para(doc, "若业务必须使用个人微信，建议把个微通道定位为“文本通知 + 兜底”，"
                    "并在对端做好字段缺失与动作失败的兼容处理。")
