# -*- coding: utf-8 -*-
"""第 10 章：保留能力与兼容性。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "10", "保留能力与兼容性", page_break=True)
    K.add_para(doc, "改造只替换了 WebSocket 通信层：微灵原有的 HTTP API、HTTP 事件回调、配置体系全部保留。"
                    "本章说明哪些能力不受影响、如何与 OneBot 通道共存，以及升级/回滚方式。")

    K.add_h2(doc, "10.1", "微灵原有 HTTP API 保留")
    K.add_bullet(doc, "企业微信 HTTP 接口（`POST /api/Client/ExecCommand` 等）行为不变，仍可按官方文档调用。")
    K.add_bullet(doc, "个人微信 HTTP 接口（`POST /api/PersonalWeChat/ExecCommand`、`/api/PersonalWeChat/Clients` 等）行为不变。")
    K.add_bullet(doc, "HTTP API 与 OneBot 通道可以同时使用：例如运维脚本通过 HTTP 发消息，机器人通过 OneBot 收发。")
    K.add_bullet(doc, "shim 自身的 `self_id` 探测也走 HTTP（127.0.0.1:8060 的 11035 指令），因此修改 HTTP 端口会影响连接（见 12.1 节）。")

    K.add_h2(doc, "10.2", "HTTP 事件回调")
    K.add_para(doc, "微灵原有的“事件通知 URL”（`config.ini` 中的 `CallbackUrl`）配置仍然有效。"
                    "改造版在事件入口处先尝试投递 OneBot 连接，返回结果告知宿主是否已投递；"
                    "宿主可据此决定是否继续走原有 HTTP 回调。")
    K.add_table(
        doc,
        ["场景", "事件去向", "建议"],
        [
            ["仅配置 OneBot 地址", "事件只走 OneBot 连接", "推荐；对端为 AstrBot 等 OneBot 实现"],
            ["仅配置 HTTP 回调", "事件只走 HTTP 回调", "与未改造版本一致"],
            ["两者都配置", "可能同时收到两份事件", "不要这样配置；若必须共存，请在对端做去重"],
            ["OneBot 未连接", "事件可能被丢弃（通道互斥）", "保持微灵在线、连接稳定（见 12.5 节）"],
        ],
        widths=[3.8, 5.6, 6.6],
        caption="表 10-1　事件通道选择",
    )
    K.add_callout(doc, "warn", "改造版不会把“OneBot 投递失败”自动转为 HTTP 回调重发：是否回落由微灵宿主逻辑决定，"
                               "且未连接期间的事件不会被缓存补发。需要可靠双通道时请自行做幂等与去重。", title="避免双投递")

    K.add_h2(doc, "10.3", "配置与界面")
    K.add_bullet(doc, "不新增配置文件；OneBot 地址与 Token 在微灵界面填写，仍由微灵写入 `config.ini`。")
    K.add_bullet(doc, "登录、二维码、账号面板、日志、更新等原有界面与流程不变。")
    K.add_bullet(doc, "程序目录新增 `onebot_v11.log`（协议日志），不影响原日志文件。")
    K.add_bullet(doc, "部署组件与未改造版本一致，仅多一个 `OneBotShim.dll`；无需安装额外运行时（依赖 .NET Framework 与原程序一致）。")

    K.add_h2(doc, "10.4", "通道互斥与保持在线")
    K.add_para(doc, "微灵 1.11.0 的通信通道存在互斥特性：同一通道同一时间只使用一种通信方式，"
                    "且 **WebSocket 尚未连接成功时，事件可能不会投递**。因此：")
    K.add_bullet(doc, "启动顺序：先启动 OneBot 实现，再启动/登录微灵（见 2.4 节）。")
    K.add_bullet(doc, "保持微灵账号在线：退出登录会清空 `self_id`，连接停止，事件不再产生。")
    K.add_bullet(doc, "避免频繁切换通信方式：切换会重建连接，切换期间的事件可能丢失。")
    K.add_bullet(doc, "网络抖动导致重连时，重连窗口内的事件不补发；需要强可靠性的场景应在微灵侧或对端做补偿。")
    K.add_bullet(doc, "同一账号不要同时连接多个 OneBot 实现（同一微灵实例只维护一条连接）；多实现需求请拆分为多个微灵实例。")

    K.add_h2(doc, "10.5", "升级与回滚")
    K.add_table(
        doc,
        ["操作", "步骤", "注意事项"],
        [
            ["升级 shim", "停止微灵 → 替换 `OneBotShim.dll` → 启动微灵", "保留旧 dll 备份；确认版本号与文档一致"],
            ["回滚到 HTTP", "微灵界面把通信方式切回 HTTP → 配置事件通知 URL", "无需替换文件；OneBot 连接随即断开"],
            ["完全回滚", "恢复备份的原程序/原 dll → 启动", "配置文件中 OneBot 地址可保留，不影响 HTTP 模式"],
            ["修改 Token/地址", "界面修改 → 保存 → 重新启用通道", "保存后需重连才生效；旧连接不会热更新"],
        ],
        widths=[3.2, 6.6, 6.2],
        caption="表 10-2　升级与回滚",
    )
    K.add_bullet(doc, "升级前建议导出一份 `config.ini` 与微灵账号配置，便于快速恢复。")
    K.add_bullet(doc, "回滚后 `onebot_v11.log` 不再增长，可保留作为历史记录。")

    K.add_h2(doc, "10.6", "安全注意事项")
    K.add_bullet(doc, "Token 以请求头传递，不出现在 URL；但仍应使用强口令，避免被内网嗅探。")
    K.add_bullet(doc, "跨公网务必使用 `wss://`，并在反向代理/防火墙层限制来源。")
    K.add_bullet(doc, "`onebot_v11.log` 包含事件内容（含 `ling` 原始事件），可能含昵称、群名、消息文本；请按敏感日志管理并定期清理。")
    K.add_bullet(doc, "对端实现应校验 `Authorization`，不要以“内网”为由留空 Token。")
