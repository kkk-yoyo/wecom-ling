# -*- coding: utf-8 -*-
"""第 4 章：连接与协议。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "4", "连接与协议", page_break=True)
    K.add_para(doc, "本章描述微灵作为 OneBot v11 反向 WS 客户端时的完整链路：如何取到 `self_id`、"
                    "握手携带哪些信息、连接后交换哪些帧、失败时如何重连。所有行为均为改造版实际实现，"
                    "可直接用于自研 OneBot 实现或抓包排障。")

    K.add_h2(doc, "4.1", "连接流程")
    K.add_para(doc, "微灵的连接是一个后台网络循环，与登录状态解耦：账号未登录时循环仍在运行，但不会发起连接。"
                    "完整流程如下：")
    K.add_step(doc, 1, "解析 `self_id`：内存中已有则直接使用；否则每 15s 通过微灵 HTTP API 查询一次"
                       "（企微通道，指令 11035），直到取得 `user_id`。个微通道跳过此查询，依赖事件中的 `robot_id` 或登录事件。")
    K.add_step(doc, 2, "生成连接地址：按 3.3 节规则规范化；若地址无效或为空（个微），本次不连接，等待下一轮。")
    K.add_step(doc, 3, "发起 WebSocket 握手，携带 `X-Self-ID`、`X-Client-Role: Universal` 与 `Authorization` 头，"
                       "最长等待 15s。")
    K.add_step(doc, 4, "握手成功后立即发送 `meta_event.lifecycle.connect`，并启动接收循环与发送循环。")
    K.add_step(doc, 5, "运行期：每 30s 发送心跳；把微灵事件翻译为 OneBot 事件后发送；执行对端下发的动作。")
    K.add_step(doc, 6, "连接断开（对端关闭、网络错误或发送超时）后清理状态，按 1s 起、上限 30s 的指数退避重连。")
    K.add_callout(doc, "info", "连接状态变化会通知微灵界面（已连接/未连接）。排查时优先看微灵程序目录下的 "
                               "`onebot_v11.log`，其中记录了地址、self_id、连接结果与失败原因。", title="排障入口")

    K.add_h2(doc, "4.2", "握手请求头")
    K.add_para(doc, "微灵在 WebSocket 握手（HTTP Upgrade）请求中固定携带以下头：")
    K.add_table(
        doc,
        ["请求头", "值", "说明"],
        [
            ["`X-Self-ID`", "当前登录账号 ID", "OneBot v11 反向 WS 约定的机器人账号标识；未解析到 self_id 不会连接"],
            ["`X-Client-Role`", "`Universal`", "表示同时接收事件与下发动作；对端可据此区分角色"],
            ["`Authorization`", "`Bearer <token>`", "仅 Token 非空时发送；Token 为空则不带此头"],
            ["`User-Agent`", "系统默认值", "微灵不额外设置，抓包时看到的是 .NET 默认 UA"],
        ],
        widths=[3.4, 3.6, 9.0],
        caption="表 4-1　握手请求头",
    )
    K.add_bullet(doc, "连接超时为 15s；`ClientWebSocket.KeepAliveInterval` 设为 20s，用于保持底层 TCP 连接活跃。")
    K.add_bullet(doc, "Token 只出现在请求头中，**不会**出现在 URL、日志正文或事件里。")
    K.add_bullet(doc, "握手被拒绝（401/403/404 等）时不会无限快速重试，而是进入指数退避。")

    K.add_h2(doc, "4.3", "元事件：连接与心跳")
    K.add_para(doc, "连接建立后，微灵主动发送两类 `meta_event`。对端可据此确认连接可用，并刷新账号信息。")
    K.add_h3(doc, "4.3.1", "连接事件")
    K.add_para(doc, "握手成功后立即发送一次，`sub_type` 固定为 `connect`：")
    K.add_code(doc, [
        "{",
        "  \"time\": 1731452400,",
        "  \"self_id\": 168800001,",
        "  \"post_type\": \"meta_event\",",
        "  \"meta_event_type\": \"lifecycle\",",
        "  \"sub_type\": \"connect\"",
        "}",
    ], caption="代码 4-1　lifecycle.connect")
    K.add_h3(doc, "4.3.2", "心跳事件")
    K.add_para(doc, "连接期间每 30s 发送一次；`status` 固定为在线且正常，`interval` 固定 30000（毫秒）：")
    K.add_code(doc, [
        "{",
        "  \"time\": 1731452430,",
        "  \"self_id\": 168800001,",
        "  \"post_type\": \"meta_event\",",
        "  \"meta_event_type\": \"heartbeat\",",
        "  \"status\": { \"online\": true, \"good\": true },",
        "  \"interval\": 30000",
        "}",
    ], caption="代码 4-2　heartbeat")
    K.add_callout(doc, "limit", "心跳是**单向**的：微灵只发不收，也不要求对端回应。接收循环按秒轮询 socket 状态、"
                                 "不使用固定读超时；发送线程也已移除 120s 入站静默看门狗。对端长时间不发帧不会导致微灵"
                                 "主动断连，连接有效性由 WebSocket/TCP KeepAlive 与发送失败共同判断（见 4.6 节）。", title="注意")

    K.add_h2(doc, "4.4", "事件帧格式")
    K.add_para(doc, "所有事件都是单行 JSON 文本帧，顶层包含通用字段，再按 `post_type` 追加类型字段。"
                    "以一条群消息为例：")
    K.add_code(doc, [
        "{",
        "  \"time\": 1731452500,",
        "  \"self_id\": 168800001,",
        "  \"post_type\": \"message\",",
        "  \"message_type\": \"group\",",
        "  \"sub_type\": \"normal\",",
        "  \"message_id\": 88776601,",
        "  \"group_id\": 24680001,",
        "  \"message\": [",
        "    { \"type\": \"at\", \"data\": { \"qq\": \"168800001\" } },",
        "    { \"type\": \"text\", \"data\": { \"text\": \" 在吗\" } }",
        "  ],",
        "  \"raw_message\": \"@168800001 在吗\",",
        "  \"font\": 0,",
        "  \"sender\": { \"user_id\": 168800002, \"nickname\": \"张三\", \"card\": \"张三\", \"role\": \"member\" },",
        "  \"ling\": { \"type\": 11041, \"client\": 1, \"data\": { \"...\": \"原始微灵事件\" } }",
        "}",
    ], caption="代码 4-3　message 事件示例")
    K.add_table(
        doc,
        ["字段", "类型", "说明"],
        [
            ["`time`", "number", "秒级 Unix 时间戳。微灵事件时间为毫秒时自动换算；缺失时取当前时间"],
            ["`self_id`", "number/string", "机器人账号 ID，来自登录事件或 HTTP 探测"],
            ["`post_type`", "string", "`message` / `notice` / `request` / `meta_event`"],
            ["`message_id`", "number/string", "优先取微灵 `server_id`，缺失时可能为微灵合成的自增值"],
            ["`ling`", "object", "原始微灵事件（已剔除 `data.payload_base64`），供需要底层字段的场景使用"],
        ],
        widths=[3.0, 3.0, 10.0],
        caption="表 4-2　事件公共字段",
        mono_cols=[0],
    )
    K.add_bullet(doc, "`message` 为消息段数组，段类型与字段见第 5 章；`raw_message` 为文本与 `@<QQ>` 的拼接结果。")
    K.add_bullet(doc, "`sender.card` 与 `nickname` 相同，`sender.role` 固定为 `member`（微灵事件不提供真实群角色）。")
    K.add_bullet(doc, "对端发来的、不含 `action` 字段的入站帧会被忽略并记录日志，不影响连接。")

    K.add_h2(doc, "4.5", "动作请求与响应")
    K.add_para(doc, "对端通过同一 WebSocket 向微灵下发动作。请求必须包含 `action`，可选 `params` 与 `echo`：")
    K.add_code(doc, [
        "{",
        "  \"action\": \"send_group_msg\",",
        "  \"params\": {",
        "    \"group_id\": 24680001,",
        "    \"message\": [ { \"type\": \"text\", \"data\": { \"text\": \"收到\" } } ]",
        "  },",
        "  \"echo\": \"req-001\"",
        "}",
    ], caption="代码 4-4　动作请求")
    K.add_para(doc, "微灵处理后在原连接返回响应；字段顺序为 `status`、`retcode`、`msg`、`data`、`echo`"
                    "（`echo` 仅在请求携带时出现，原样返回）：")
    K.add_code(doc, [
        "{",
        "  \"status\": \"ok\",",
        "  \"retcode\": 0,",
        "  \"msg\": \"\",",
        "  \"data\": { \"message_id\": 88776602 },",
        "  \"echo\": \"req-001\"",
        "}",
    ], caption="代码 4-5　动作成功响应")
    K.add_table(
        doc,
        ["retcode", "status", "含义", "常见原因"],
        [
            ["0", "`ok`", "成功", "—"],
            ["1404", "`failed`", "未知动作", "`action` 不在支持列表中，`msg` 为 `unsupported action: <action>`"],
            ["500", "`failed`", "动作执行失败", "参数错误、微灵未返回结果、媒体无直链或下载失败、显式不支持的动作等；`msg` 给出原因"],
        ],
        widths=[2.0, 2.0, 3.4, 8.6],
        caption="表 4-3　动作响应码",
    )
    K.add_bullet(doc, "`params` 缺失或为 `null` 时按空对象处理，由具体动作校验必填参数。")
    K.add_bullet(doc, "动作在接收循环中**串行**执行；单个动作最长等待微灵 60s，期间后续事件与动作不会并发处理。")
    K.add_bullet(doc, "失败响应的 `data` 为 `null`；`msg` 为便于阅读的英文/中文错误描述，可用于日志与提示。")
    K.add_bullet(doc, "对端应自行按 `echo` 关联请求与响应；微灵不保证 `echo` 的类型，原样回传。")

    K.add_h2(doc, "4.6", "重连、心跳与链路检测")
    K.add_para(doc, "微灵内置多层保护机制，理解它们有助于区分“真故障”与“正常重连”：")
    K.add_table(
        doc,
        ["机制", "参数", "行为"],
        [
            ["连接超时", "15s", "握手超过 15s 未完成即放弃，进入重连退避"],
            ["重连退避", "1s 起，×2 递增，上限 30s", "失败后等待当前延迟，成功后重置为 1s"],
            ["心跳", "30s", "发送线程每 500ms 检查一次，距上次心跳 ≥30s 发心跳帧"],
            ["链路检测", "KeepAlive 20s", "由 WebSocket/TCP 层保活探测断链；对端空闲不发帧不会导致主动断开"],
            ["发送超时", "10s/帧", "单帧发送超过 10s 视为连接异常，断开重连"],
            ["发送队列", "2000 帧", "队列满时丢弃最旧帧并记日志，避免内存无限增长"],
        ],
        widths=[3.2, 4.0, 8.8],
        caption="表 4-4　连接保护机制",
    )
    K.add_para(doc, "重连间隔序列（毫秒）示意：")
    K.add_table(
        doc,
        ["第 1 次", "第 2 次", "第 3 次", "第 4 次", "第 5 次", "第 6 次及以后"],
        [["1000", "2000", "4000", "8000", "16000", "30000（封顶）"]],
        widths=[2.5, 2.5, 2.5, 2.5, 2.5, 3.5],
        caption="表 4-5　重连退避序列",
        mono_cols=[0, 1, 2, 3, 4, 5],
        header_fill="2E74B5",
    )
    K.add_callout(doc, "info", "接收侧固定读超时已移除；发送线程的 120s 入站静默看门狗也已移除（源码常量已删除），"
                               "**不存在因空闲静默而周期性重连**。AstrBot 等 OneBot 实现空闲时通常不发帧，"
                               "连接有效性依赖 `ClientWebSocket.KeepAliveInterval`（20s）的保活探测与发送失败处理；"
                               "链路中断会在下一次发送失败或 KeepAlive 探测失败时进入退避重连。", title="不再有静默看门狗")
    K.add_bullet(doc, "重连不会清空消息缓存（500 条）与 self_id；连接恢复后动作与事件立即恢复。")
    K.add_bullet(doc, "未连接期间产生的事件不会排队补发，直接丢弃；需要可靠接收时请保持连接稳定。")
    K.add_bullet(doc, "日志文件 `onebot_v11.log` 位于微灵程序目录，单文件 5MB，超出后轮转为 `onebot_v11.log.1`。")
