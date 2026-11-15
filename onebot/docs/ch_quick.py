# -*- coding: utf-8 -*-
"""第 2 章：快速开始。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "2", "快速开始", page_break=True)
    K.add_para(doc, "本章以 **AstrBot 4.29.0 + 微灵 1.11.0 改造版（企微通道）** 为例，"
                    "完成从零到“收发一条消息”的最小闭环。全流程约 10 分钟。"
                    "读者只需照做并在最后完成验证，即可确认 OneBot v11 通道工作正常。")

    K.add_h2(doc, "2.1", "准备工作")
    K.add_table(
        doc,
        ["项目", "要求", "说明"],
        [
            ["微灵版本", "1.11.0 改造版", "内置 OneBotShim（OneBot v11 反向 WS 客户端）"],
            ["OneBot 实现", "AstrBot 4.29.0（aiocqhttp 平台）", "留空 Token 也可，但建议设置；本文使用 114514"],
            ["微灵登录状态", "企业微信已登录成功", "未登录无法解析 self_id，微灵不会发起连接（见 4.6 节）"],
            ["网络", "微灵与 AstrBot 网络可达", "同机部署时使用 127.0.0.1；跨机时使用 AstrBot 所在主机 IP"],
            ["端口", "AstrBot 反向 WS 端口 6200 空闲", "示例端口，可改为其他值，但两侧必须一致"],
            ["微灵 HTTP 端口", "默认 8060 未被修改", "shim 通过它探测 self_id；修改后需依赖登录事件（见 12.1 节）"],
            ["运行目录", "普通本地目录（如 `D:\\微灵OneBotV11\\`）", "不要放在被安全软件/终端策略管控的路径，否则 8060 不监听、企微注入失败（见 3.1 节）"],
        ],
        widths=[3.0, 5.4, 7.6],
        caption="表 2-1　快速开始前置条件",
    )
    K.add_callout(doc, "info", "“反向 WS”意味着连接由微灵发起、AstrBot 监听。"
                               "因此 **必须先启动 AstrBot**，再启动/登录微灵并启用 WebSocket 通道。", title="顺序很重要")

    K.add_h2(doc, "2.2", "AstrBot 侧配置")
    K.add_step(doc, 1, "启动 AstrBot 4.29.0，打开其 WebUI（默认端口 6185，以实际部署为准）。")
    K.add_step(doc, 2, "进入「消息平台 / 平台配置」，新增一个 **aiocqhttp**（OneBot v11）适配器。")
    K.add_step(doc, 3, "按表 2-2 填写反向 WS 参数；反向 WS 路径固定为 `/ws`。")
    K.add_step(doc, 4, "保存并启用该适配器，确认 AstrBot 日志中出现反向 WS 服务已监听的记录。")

    K.add_table(
        doc,
        ["配置字段", "填写值", "说明"],
        [
            ["ws_reverse_host", "0.0.0.0", "监听地址。仅本机连接时可填 127.0.0.1；跨机部署需 0.0.0.0 并放行防火墙"],
            ["ws_reverse_port", "6200", "监听端口，须与微灵侧填入的地址端口一致"],
            ["ws_reverse_token", "114514", "鉴权 Token，须与微灵侧完全一致；生产环境请换成强口令"],
            ["反向 WS 路径", "/ws", "由 aiocqhttp 提供，微灵连接地址中需包含该路径"],
        ],
        widths=[3.4, 3.4, 9.2],
        caption="表 2-2　AstrBot aiocqhttp 反向 WS 配置",
        mono_cols=[0, 2],
    )
    K.add_para(doc, "如果使用 AstrBot 的配置文件方式，可参考以下片段（字段名与 WebUI 一致，实际以 4.29.0 版本为准）：")
    K.add_code(doc, [
        "# AstrBot aiocqhttp（OneBot v11）平台配置示意",
        "platform: aiocqhttp",
        "ws_reverse_host: 0.0.0.0",
        "ws_reverse_port: 6200",
        "ws_reverse_token: \"114514\"",
    ], caption="代码 2-1　AstrBot aiocqhttp 配置片段")
    K.add_callout(doc, "warn", "`ws_reverse_token` 是唯一鉴权手段，请勿使用示例中的 114514 直接上线；"
                               "`ws_reverse_host` 设为 0.0.0.0 时，AstrBot 的 6200 端口会暴露在内网，"
                               "请配合防火墙或仅监听 127.0.0.1。")

    K.add_h2(doc, "2.3", "微灵侧配置")
    K.add_step(doc, 1, "启动微灵 1.11.0 改造版，完成企业微信登录，等待界面显示账号在线。")
    K.add_step(doc, 2, "进入微灵的连接/通信设置，把「通信方式」选择为 **WebSocket**。")
    K.add_step(doc, 3, "「WebSocket 地址」填写 `ws://127.0.0.1:6200/ws`（AstrBot 与微灵同机）。"
                       "跨机部署时把 127.0.0.1 换成 AstrBot 所在主机 IP。")
    K.add_step(doc, 4, "「Token」填写 `114514`，与 AstrBot 的 `ws_reverse_token` 完全一致。")
    K.add_step(doc, 5, "保存配置并启用。微灵将作为客户端连接 AstrBot，连接状态显示为“已连接”（或同义状态）。")
    K.add_para(doc, "个人微信通道的填写方式相同，但需要在微灵中选择「个人微信 + WebSocket」通道，"
                    "并填入该通道对应的 OneBot 实现地址；个微通道**没有默认地址**，地址为空时不会连接。"
                    "个微通道的能力与限制见第 9 章。")
    K.add_table(
        doc,
        ["微灵配置项", "示例值", "说明"],
        [
            ["通信方式", "WebSocket", "改造后 OneBot v11 通道即为 WebSocket 模式"],
            ["WebSocket 地址", "`ws://127.0.0.1:6200/ws`", "包含协议、主机、端口与路径；也可只填主机，路径为空时自动补 `/ws`"],
            ["Token", "`114514`", "以 `Authorization: Bearer` 请求头发送；留空则不发鉴权头"],
            ["通道类型", "企业微信", "个微通道需单独选择，属实验性能力"],
        ],
        widths=[3.2, 5.2, 7.6],
        caption="表 2-3　微灵侧配置项",
    )
    K.add_callout(doc, "info", "企业微信通道的地址留空时，微灵会自动连接默认地址 `ws://127.0.0.1:6200/ws`，"
                               "适合“AstrBot 与微灵同机 + 使用默认端口”的零配置场景。"
                               "地址规则详见 3.3 节。")

    K.add_h2(doc, "2.4", "启动与验证")
    K.add_para(doc, "按以下顺序启动，避免微灵先启动、找不到监听端而反复重连：")
    K.add_step(doc, 1, "启动 AstrBot，确认 aiocqhttp 适配器已启用、6200 端口处于监听状态。")
    K.add_step(doc, 2, "启动微灵并登录企业微信；登录成功后微灵才会解析出 self_id 并发起连接（见 4.6 节）。")
    K.add_step(doc, 3, "在微灵中启用 WebSocket 通信方式，地址与 Token 按表 2-3 填写。")
    K.add_step(doc, 4, "观察两侧状态：AstrBot 日志应显示 aiocqhttp 客户端接入/连接成功；"
                       "微灵界面应显示已连接，微灵程序目录下 `onebot_v11.log` 中可见连接成功记录。")
    K.add_step(doc, 5, "发消息验证：用企业微信给机器人账号发一条私聊消息，AstrBot 应收到 message 事件；"
                       "再让 AstrBot 向该会话回复一条文本，企业微信侧应能收到。")
    K.add_para(doc, "建议按下表逐项确认，任何一项失败都可直接定位到对应章节。")
    K.add_table(
        doc,
        ["检查项", "预期结果", "不通过时"],
        [
            ["AstrBot 反向 WS 监听", "适配器日志显示监听 0.0.0.0:6200（或对应配置）", "检查端口占用、适配器是否启用（3.7 节）"],
            ["微灵连接状态", "界面显示“已连接”，日志出现连接成功记录", "检查地址/路径、Token、网络（3.3/3.4 节）"],
            ["AstrBot 收到事件", "收到 `message` 事件，`self_id` 为微灵登录账号", "微灵是否正确登录、是否发送了非自身消息（5.1 节）"],
            ["AstrBot 发送文本", "企业微信成功收到消息，动作返回 `status: ok`", "查看失败 `retcode` 与 `msg`（7.2 节、第 12 章）"],
            ["AstrBot 发送图片/文件", "企业微信收到对应媒体", "检查临时目录 `onebot_media_tmp`、源文件/URL 与大小限制（8.2 节）"],
            ["心跳", "连接期间每 30s 收到 `meta_event.heartbeat`", "连接异常或发送队列阻塞（4.3/4.6 节）"],
        ],
        widths=[3.6, 7.0, 5.4],
        caption="表 2-4　首次接入验证清单",
    )

    K.add_h2(doc, "2.5", "首次接入常见问题")
    K.add_bullet(doc, "**微灵一直不连接**：多为微灵未登录成功、或 8060 端口不可用导致取不到 self_id。"
                       "先确认微灵账号在线，再查看 `onebot_v11.log` 中是否有等待 self_id 的记录。")
    K.add_bullet(doc, "**连接被拒绝（401/403）**：两侧 Token 不一致或微灵 Token 为空。Token 走请求头，"
                       "URL 里不需要也不支持 `?token=`。")
    K.add_bullet(doc, "**连接 404 或握手失败**：地址缺少 `/ws` 路径，或 AstrBot 的反向 WS 路径被修改。"
                       "按 3.3 节规则补全路径。")
    K.add_bullet(doc, "**连上但收不到事件**：检查消息是否为账号自己发出（会被过滤）、事件类型是否属于未映射类型，"
                       "或 AstrBot 适配器未启用；详见 5.7 节与 12.2 节。")
    K.add_bullet(doc, "**AstrBot 回复失败**：文本/@/图片/视频/文件/动图已支持发送（图片等媒体先落盘到 `onebot_media_tmp`）；"
                       "语音（`record`）与 `json`/`xml`/`forward` 等段不支持，会返回 failed。若 `msg` 为"
                       "“微灵未返回结果（超时或未登录）”，说明微灵侧登录或指令通道异常（见 8.2 节）。")
