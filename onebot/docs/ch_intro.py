# -*- coding: utf-8 -*-
"""前言与第 1 章：改造概述。"""
import docx_kit as K


def build(doc):
    # ------------------------------------------------------------ 前言
    K.add_h1(doc, "", "前言")
    K.add_para(doc, "本手册面向需要把微灵（WeComSever）接入 OneBot v11 机器人框架的开发者与运维人员，"
                    "说明微灵 1.11.0 改造版如何以 **OneBot v11 反向 WebSocket 客户端** 的身份接入 AstrBot、"
                    "NapCat、LLOneBot 等支持 OneBot v11 的机器人实现，并给出配置、协议、事件、动作、排障的完整说明。")

    K.add_h2(doc, "", "文档目的")
    K.add_para(doc, "微灵 1.11.0 改造版在进程内替换了原有的自研 WebSocket 通道实现，"
                    "把「微灵事件 → OneBot v11 事件」和「OneBot 动作 → 微灵指令」两件事在微灵内部完成翻译。"
                    "因此对接方不再需要自行实现微灵私有的 hello / ping / command 帧，也不必处理 11170/11171 的二进制文件流，"
                    "只需部署一个支持 OneBot v11 反向 WS 的机器人框架，在微灵界面里填写地址与 Token 即可完成对接。")

    K.add_h2(doc, "", "读者对象")
    K.add_bullet(doc, "**部署运维人员**：按第 2 章完成 AstrBot 与微灵的两侧配置，按第 12 章排查问题。")
    K.add_bullet(doc, "**机器人开发者**：阅读第 4～7 章，了解连接握手、事件帧、消息能力矩阵、动作请求与错误码约定。")
    K.add_bullet(doc, "**二次开发人员**：阅读第 6 章消息能力矩阵、第 8 章媒体说明、第 10 章兼容性说明与附录中的事件号/指令号速查表。")

    K.add_h2(doc, "", "与官方文档的关系")
    K.add_para(doc, "微灵官方《微灵对接文档》（1.11.0）描述的是 HTTP 接口与**自研** WebSocket 协议，"
                    "这些内容在改造版中仍然有效：HTTP 8060 接口、HTTP 事件回调、微灵指令号均未改变。"
                    "本手册只描述改造后的 **OneBot v11 通道**；当两者表述冲突时，以本手册及实际实现"
                    "（`D:\\wecom-ling\\onebot\\src\\WxSocketClient.cs`）为准。")
    K.add_callout(doc, "info", "本手册的行为描述依据 1.11.0 改造版的实现编写。微灵后续版本可能扩展事件与动作，"
                               "升级后请以对应版本的变更说明为准。")

    K.add_h2(doc, "", "阅读指引")
    K.add_table(
        doc,
        ["我想……", "建议阅读"],
        [
            ["尽快把 AstrBot 和微灵连起来", "第 2 章 快速开始"],
            ["理解改造前后的差异、评估切换风险", "第 1 章 改造概述、第 10 章 保留能力与兼容性"],
            ["调整地址、Token、端口、跨机部署", "第 3 章 部署与配置"],
            ["自己实现一个 OneBot v11 服务端来对接", "第 4 章 连接与协议、第 5～7 章 事件与动作"],
            ["了解收发消息支持哪些类型、如何降级", "第 6 章 消息能力矩阵"],
            ["启用第三方 AI 审批插件", "第 11 章 AI 审批插件"],
            ["处理图片、语音、文件、群文件", "第 8 章 媒体与文件、第 12 章 已知限制"],
            ["查事件号、指令号、动作支持情况", "第 13 章 附录"],
        ],
        widths=[7.0, 9.0],
        caption="表 0-1　阅读指引",
        first_col_center=False,
    )

    K.add_h2(doc, "", "排版约定")
    K.add_bullet(doc, "行内代码（如 `send_group_msg`、`X-Self-ID`）表示协议字段名、动作名或配置键。")
    K.add_bullet(doc, "灰底代码块表示 JSON 帧、配置片段或日志示例，均为示意数据，实际值以运行环境为准。")
    K.add_bullet(doc, "“微灵”指微灵 1.11.0 改造版进程；“OneBot 实现”指接收反向 WS 连接的机器人框架（如 AstrBot）。")
    K.add_bullet(doc, "“企微通道”指企业微信通道；“个微通道”指个人微信通道（实验性，见第 9 章）。")

    # ------------------------------------------------------------ 第 1 章
    K.add_h1(doc, "1", "改造概述", page_break=True)

    K.add_h2(doc, "1.1", "改造背景")
    K.add_para(doc, "微灵原方案通过一套自研 WebSocket 协议把微信事件推送给对接方：微灵作为客户端连接对接方自建的 "
                    "WS 服务端（企微通道路径为 `/wx/ws`，个微通道为 `/personal-wx/ws`，Token 以 `?token=` 形式附加在 URL 上）。"
                    "对接方需要自行实现握手（hello）、心跳应答（ping/pong）、事件帧（event）、指令帧（command）与二进制文件流，"
                    "协议细节多、排障成本高，且无法直接复用已有的 OneBot 生态。")
    K.add_para(doc, "改造版把微灵进程内的 WebSocket 通道替换为 **OneBot v11 反向 WebSocket 客户端**："
                    "微灵仍然主动向外建立 WebSocket 连接，但说的是 OneBot v11 标准协议；"
                    "连接目标从“对接方自建服务端”变成“现成的 OneBot v11 实现”（AstrBot 的 aiocqhttp、NapCat、LLOneBot 等）。"
                    "微灵内部继续使用原有的指令体系与事件采集链路，协议翻译在进程内完成，不改变微灵的登录、收发消息和部署形态。")

    K.add_h2(doc, "1.2", "改造后架构")
    K.add_para(doc, "改造只替换微灵的“WS 出口”，整体链路如下：")
    K.add_code(doc, [
        "企业微信客户端",
        "      │  （微灵原生协议，保持不变）",
        "      ▼",
        "┌────────────────────────────── 微灵进程（1.11.0 改造版）──────────────────────────────┐",
        "│  登录/收消息/发消息/群与联系人管理        原有 HTTP 8060 API 与 HTTP 事件回调（保留）  │",
        "│                        OneBotShim（WxSocketClient）：事件翻译 + 动作执行              │",
        "└───────────────────────────────────┬────────────────────────────────────────────────┘",
        "                                    │ OneBot v11 反向 WebSocket（微灵为客户端）",
        "                                    ▼",
        "                    AstrBot / NapCat / LLOneBot 等 OneBot v11 实现",
        "                    （反向 WS 服务端，示例监听 0.0.0.0:6200，路径 /ws）",
    ], caption="图 1-1　改造后连接拓扑")
    K.add_bullet(doc, "**连接方向不变**：微灵仍是 WebSocket 客户端，主动连接 OneBot 实现提供的反向 WS 服务端。")
    K.add_bullet(doc, "**协议改变**：握手、事件、动作全部使用 OneBot v11 的 JSON 帧；微灵私有帧不再出现在 OneBot 通道上。")
    K.add_bullet(doc, "**翻译在进程内**：事件侧把微灵事件转成 OneBot 事件并保留原始事件（`ling` 字段）；"
                       "动作侧把 OneBot 动作转成微灵指令，通过微灵原有命令通道执行。")
    K.add_bullet(doc, "**零额外进程**：不需要再部署一个 WS 网关；只需在微灵界面填写 OneBot 实现的地址与 Token。")

    K.add_h2(doc, "1.3", "改造前后对比")
    K.add_para(doc, "下表从对接视角汇总两类通道的差异，便于评估迁移工作量。")
    K.add_table(
        doc,
        ["对比项", "原自研 WebSocket 协议", "改造后 OneBot v11"],
        [
            ["协议规范", "微灵私有协议：hello / ping / event / command 帧", "OneBot v11 标准事件与动作 JSON"],
            ["连接角色", "微灵为客户端，连接对接方自建 WS 服务端", "微灵为客户端，连接 OneBot 实现的反向 WS 服务端"],
            ["服务端路径", "企微 `/wx/ws`，个微 `/personal-wx/ws`", "由 OneBot 实现决定，常见为 `/ws`"],
            ["鉴权方式", "URL 查询参数 `?token=<token>`", "请求头 `Authorization: Bearer <token>`"],
            ["事件格式", "`{type, client, robot_id, data}` 私有结构", "`{time, self_id, post_type, ...}`，原始事件保留在 `ling`"],
            ["下行动作", "微灵指令号（11029 等）+ 私有 command 帧", "OneBot 动作（`send_group_msg` 等）+ `action/params/echo`"],
            ["心跳", "微灵每 20s 发 ping，约 40s 无 pong 断开", "微灵每 30s 发 `meta_event.heartbeat`（单向）；无固定静默看门狗，空闲不会周期性重连"],
            ["文件与媒体", "11170/11171 指令 + WXFL 二进制分片流", "事件内 `cdn.url` 直链；入站按需下载，出站支持图片/视频/文件/动图（见第 6、8 章）"],
            ["对接成本", "需自行实现完整私有协议与分片逻辑", "部署现成 OneBot 实现即可，无需写协议代码"],
            ["HTTP 能力", "HTTP 8060 接口、HTTP 事件回调可用", "保持不变，仅 WS 通道被替换（见第 10 章）"],
        ],
        widths=[2.6, 6.7, 6.7],
        caption="表 1-1　原自研协议与 OneBot v11 改造版对比",
    )

    K.add_h2(doc, "1.4", "设计原则与兼容性")
    K.add_bullet(doc, "**最小侵入**：不改变微灵登录、收消息、发消息与群管理的原生链路，只替换 WS 出口；"
                       "部署形态仍是单个微灵进程。")
    K.add_bullet(doc, "**原样保留**：所有 OneBot 事件都附带 `ling` 字段，内含完整微灵原始事件"
                       "（仅剔除体积最大的 `data.payload_base64`），便于高级用户读取 OneBot 未覆盖的字段。")
    K.add_bullet(doc, "**显式失败**：首版未实现的动作返回明确的 `failed` 与错误说明（`retcode` 1404/500），"
                       "不会静默丢帧，便于快速定位。")
    K.add_bullet(doc, "**兼容旧能力**：微灵原有 HTTP 8060 API、HTTP 事件回调、微灵 UI 与 `config.ini` 均保留，"
                       "改造只影响 WebSocket 通道。")
    K.add_bullet(doc, "**能力边界清晰**：出站支持文本、@ 与图片/视频/文件/动图（语音除外），入站媒体取回依赖事件自带 CDN 直链，"
                       "这些限制集中记录在第 8 章与第 12 章。")
    K.add_callout(doc, "limit", "改造版不提供语音发送，也不支持撤回、群管理、请求处理等动作；"
                                "详见 7.6、7.7 节与 12.3 节。若业务强依赖这些能力，请评估后再切换。")
