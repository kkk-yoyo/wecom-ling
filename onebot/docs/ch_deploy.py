# -*- coding: utf-8 -*-
"""第 3 章：部署与配置。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "3", "部署与配置", page_break=True)
    K.add_para(doc, "本章说明运行环境要求、改造版的部署形态、地址与 Token 规则、微灵可配置项，以及对接 AstrBot 之外其他 "
                    "OneBot v11 实现时的要求。核心结论是：**微灵只负责主动连接，配置只有地址与 Token 两项**，"
                    "其余参数均由 OneBot 实现侧决定。")

    K.add_h2(doc, "3.1", "运行环境要求（重要）")
    K.add_para(doc, "部署前请先确认运行目录：**微灵必须放在不被本机安全软件或终端管控策略限制的普通目录中运行**。"
                    "现场已定位：若程序目录被列入受限路径，会出现一组与 OneBot 改造补丁无关的故障——"
                    "企业微信注入失败、HTTP 8060 不监听、无法写入 `C:\\Users` 目录等；"
                    "同一份 exe 复制到普通目录后立即恢复正常，说明问题在运行环境而不在程序或补丁本身。")
    K.add_callout(doc, "warn", "**推荐目录：`D:\\微灵OneBotV11\\`**（本机已验证）。"
                               "不要放在 `D:\\wecom-ling\\`、`C:\\Users\\...\\Downloads`、VDI 同步盘等"
                               "可能被安全策略/沙箱管控的路径下运行。移动时把整个程序目录一起复制，不要只移动 exe。",
                   title="结论先行")
    K.add_para(doc, "受限路径的典型症状如下，出现任意一条都建议先换目录验证：")
    K.add_table(
        doc,
        ["症状", "典型表现", "判别方法"],
        [
            ["企业微信注入失败", "注入器/代理日志报 `Error: cannot verify loaded hook modules`",
             "把整个程序目录复制到其它盘或其它目录后重试"],
            ["HTTP 8060 不监听", "日志只有「正在启动Web服务...」，没有「服务已启动，监听地址: http://0.0.0.0:8060/」",
             "换目录后观察 8060 是否立即监听"],
            ["无法写入用户目录", "配置或日志写入 `C:\\Users\\...`（AppData 等）失败", "改用普通本地目录并确认目录可写"],
            ["行为随目录变化", "同一 exe 在 A 目录异常、在 B 目录正常", "对比两个目录的运行日志与注入结果"],
        ],
        widths=[3.2, 7.0, 5.8],
        caption="表 3-1　受限运行路径的典型症状与判别",
    )
    K.add_para(doc, "判别方法很简单：**把整个程序目录（含 exe、`OneBotShim.dll`、`PersonalTcpBridge.exe` 及依赖）"
                    "复制到其它盘或其它目录后重试**。同一份 exe 在新目录恢复正常，即可判定原目录被安全策略限制，"
                    "无需修改配置或调整补丁。")
    K.add_table(
        doc,
        ["目录", "建议", "说明"],
        [
            ["`D:\\微灵OneBotV11\\`", "**推荐**", "本机已验证：内含 `微灵-OneBotV11-1.11.0.exe` 与 `PersonalTcpBridge.exe`"],
            ["`D:\\wecom-ling\\`", "不要使用", "本机该目录被安全软件列入受限路径，出现 8060 不监听、注入失败"],
            ["`C:\\Users\\...\\Downloads`、VDI/同步盘", "避免", "可能被终端安全、虚拟化或同步策略管控"],
            ["其它普通本地盘目录（如 `D:\\wecom-ling-app\\`）", "可用", "按上面的判别方法验证后再投入运行"],
        ],
        widths=[4.8, 2.4, 8.8],
        caption="表 3-2　运行目录选择建议",
        mono_cols=[0],
    )
    K.add_para(doc, "换到可信目录后的实测现象，可作为验证基线：")
    K.add_bullet(doc, "HTTP 8060 秒开，日志出现「服务已启动，监听地址: http://0.0.0.0:8060/」。")
    K.add_bullet(doc, "企业微信注入成功：proxy `/api/status` 返回 `hooks_installed=true`、`tcp.connected=true`，"
                       "客户端上线；日志出现「已接入 5.0.13.6005 pid=… client=1 http=19065 tcp=19167」。")
    K.add_bullet(doc, "**父进程信任会传染**：注入器由受限目录中的进程启动时同样失败，改由可信目录中的进程启动即成功。")
    K.add_callout(doc, "info", "该限制来自终端安全/沙箱策略（本机为传云 Vault + CloudEDR / HIoTSecAgent），"
                               "**与 OneBot 改造补丁无关**。换到普通目录即可，不需要改配置、重装或调整补丁；"
                               "若换目录后仍异常，再按第 12 章排查。")

    K.add_h2(doc, "3.2", "部署形态")
    K.add_bullet(doc, "**单实例单连接**：一个微灵进程的企微通道建立一条 OneBot v11 WebSocket 连接；"
                       "个微通道（实验性）单独建立一条连接。两条通道互不影响。")
    K.add_bullet(doc, "**OneBot 实现在对端**：AstrBot / NapCat / LLOneBot 以“反向 WS 服务端”身份监听端口，"
                       "微灵以客户端身份连接；不需要在微灵侧开放任何入站端口。")
    K.add_bullet(doc, "**无需额外进程**：协议翻译在微灵进程内完成；部署组件与未改造版本完全一致，"
                       "仅多一个 `OneBotShim.dll`。")
    K.add_bullet(doc, "**多账号/多实例**：每个微灵实例各自填写自己的 OneBot 地址；"
                       "OneBot 实现侧可通过 `X-Self-ID` 区分不同账号，或为每个账号配置独立适配器。")
    K.add_para(doc, "同机部署与跨机部署的差异只体现在地址上：")
    K.add_table(
        doc,
        ["部署方式", "微灵 WebSocket 地址", "注意事项"],
        [
            ["同机部署（推荐）", "`ws://127.0.0.1:6200/ws`", "最简单；AstrBot 可监听 127.0.0.1"],
            ["跨机部署", "`ws://<AstrBot-IP>:6200/ws`", "AstrBot 监听 0.0.0.0 并放行 6200；建议内网或 TLS"],
            ["跨公网 / 内网穿透", "`wss://<域名>/ws`", "使用 TLS 终止；Token 必须为强口令"],
        ],
        widths=[3.6, 6.4, 6.0],
        caption="表 3-3　部署方式与地址",
    )

    K.add_h2(doc, "3.3", "连接地址规则")
    K.add_para(doc, "微灵在连接前会对界面中填写的地址做一次规范化，规则如下（按顺序执行）：")
    K.add_step(doc, 1, "地址为 **空** 时：企业微信通道自动使用默认地址 `ws://127.0.0.1:6200/ws`；"
                       "个人微信通道不连接。")
    K.add_step(doc, 2, "去掉首尾空白后，按前缀替换协议：`http://` → `ws://`，`https://` → `wss://`；"
                       "没有任何协议前缀时自动补 `ws://`。")
    K.add_step(doc, 3, "解析 URL；解析失败则放弃本次连接并按退避策略重试。")
    K.add_step(doc, 4, "路径为空（或仅为 `/`）时补 `/ws`；已有路径保持不变。")
    K.add_step(doc, 5, "保留查询参数（`?a=b`）；连接时不额外附加参数。")
    K.add_table(
        doc,
        ["填写的地址", "实际连接地址", "说明"],
        [
            ["（空，企微通道）", "`ws://127.0.0.1:6200/ws`", "默认地址，日志会提示使用了默认值"],
            ["（空，个微通道）", "不连接", "个人微信通道必须显式配置地址"],
            ["`http://192.168.1.20:6200/ws`", "`ws://192.168.1.20:6200/ws`", "http 自动映射为 ws"],
            ["`https://bot.example.com/ws`", "`wss://bot.example.com/ws`", "https 自动映射为 wss"],
            ["`192.168.1.20:6200`", "`ws://192.168.1.20:6200/ws`", "无协议补 ws://，无路径补 /ws"],
            ["`ws://192.168.1.20:6200`", "`ws://192.168.1.20:6200/ws`", "空路径补 /ws"],
            ["`ws://192.168.1.20:6200/onebot`", "`ws://192.168.1.20:6200/onebot`", "自定义路径原样保留"],
            ["`ws://host:6200/ws?x=1`", "`ws://host:6200/ws?x=1`", "查询参数保留"],
        ],
        widths=[6.2, 6.2, 3.6],
        caption="表 3-4　地址规范化规则",
        mono_cols=[0, 1],
    )
    K.add_callout(doc, "warn", "Token **不要写进地址**（如 `?token=xxx`）。改造版通过 `Authorization: Bearer` "
                               "请求头传递 Token；把 Token 放在 URL 中会出现在日志与代理记录里，且可能被服务端忽略。")

    K.add_h2(doc, "3.4", "Token 与鉴权")
    K.add_bullet(doc, "Token 不为空时，微灵在 WebSocket 握手请求中携带请求头 "
                       "`Authorization: Bearer <token>`。")
    K.add_bullet(doc, "Token 为空时，不发送 `Authorization` 头；若 OneBot 实现配置了 Token，握手会返回 401/403。")
    K.add_bullet(doc, "鉴权失败不会停止重试：微灵按 1s～30s 退避持续重连，因此配置错误时会周期性出现握手失败日志。")
    K.add_bullet(doc, "Token 只用于握手鉴权，连接建立后不再校验；更换 Token 后需要断开重连（重启微灵或切换通信方式）才生效。")
    K.add_bullet(doc, "若某个 OneBot 实现仅支持 `?token=` 查询参数、不支持请求头，可在微灵地址中显式带上查询参数"
                       "（查询参数会被保留）作为兼容手段，但不推荐用于生产。")

    K.add_h2(doc, "3.5", "配置项说明")
    K.add_para(doc, "改造版**不新增配置文件**。OneBot 通道的地址与 Token 由微灵界面维护，"
                    "并写入微灵原有的 `config.ini`。下表列出与对接相关的配置项。")
    K.add_table(
        doc,
        ["配置位置", "配置项", "说明"],
        [
            ["微灵界面", "通信方式", "选择 WebSocket 即启用 OneBot v11 通道；选择 HTTP 则回到原 HTTP 回调方式"],
            ["微灵界面", "WebSocket 地址", "OneBot 实现的反向 WS 地址，规则见 3.3 节"],
            ["微灵界面", "Token", "与 OneBot 实现一致；以 `Authorization: Bearer` 发送"],
            ["config.ini", "`Port`", "微灵 HTTP API 端口，默认 8060。shim 的 self_id 探测固定访问 127.0.0.1:8060（见 12.1 节）"],
            ["config.ini", "`CallbackUrl`", "原 HTTP 事件回调地址；改造后仍可用，建议与 OneBot 通道二选一"],
            ["config.ini", "`WeComUseWebSocket`", "企微通道通信方式开关，由界面写入，无需手工修改"],
            ["config.ini", "`PersonalUseWebSocket`", "个微通道通信方式开关，由界面写入，无需手工修改"],
        ],
        widths=[2.8, 4.2, 9.0],
        caption="表 3-5　与 OneBot 对接相关的配置项",
    )
    K.add_callout(doc, "info", "`config.ini` 由微灵自身读写，改造版不解析它。请通过微灵界面修改配置，"
                               "手工编辑可能被界面覆盖，也容易与内存状态不一致。其余原有配置项保持默认即可。")

    K.add_h2(doc, "3.6", "对接其他 OneBot v11 实现")
    K.add_para(doc, "改造版使用标准 OneBot v11 反向 WS 帧，不依赖特定实现。对接任何实现前，请确认它满足以下条件：")
    K.add_table(
        doc,
        ["要求", "说明"],
        [
            ["提供反向 WS **服务端**", "该实现必须能监听端口、接受客户端连接，而不是去连接别人的 WS 服务端"],
            ["支持 `Authorization: Bearer` 校验", "与微灵的 Token 机制一致；若实现只支持 query token，见 3.4 节兼容手段"],
            ["可配置监听地址/端口/路径", "微灵地址需要与之匹配，路径通常为 `/ws`"],
            ["能忽略未知请求头", "微灵固定发送 `X-Self-ID`、`X-Client-Role: Universal`，实现可读取也可忽略"],
            ["支持 OneBot v11 事件与动作 JSON", "推荐实现：AstrBot 4.29.0（aiocqhttp）、NapCat、LLOneBot 等"],
        ],
        widths=[5.0, 11.0],
        caption="表 3-6　对 OneBot v11 实现的要求",
    )
    K.add_bullet(doc, "**AstrBot**：使用 aiocqhttp 平台，按第 2 章配置，已验证可用。")
    K.add_bullet(doc, "**NapCat / LLOneBot**：使用它们的“反向 WebSocket 服务端”模式，把微灵地址指向其监听地址；"
                       "Token 在对应实现中配置，保持与微灵一致。")
    K.add_bullet(doc, "**自研实现**：按第 4 章的帧格式实现连接、事件接收与动作发送即可，无需实现微灵私有协议。")

    K.add_h2(doc, "3.7", "网络与端口")
    K.add_table(
        doc,
        ["端口", "所属", "用途", "说明"],
        [
            ["6200", "OneBot 实现", "反向 WS 监听（示例）", "微灵主动连接；可更换端口，两侧保持一致"],
            ["8060", "微灵", "HTTP API / self_id 探测", "shim 硬编码访问 `127.0.0.1:8060`，仅本机；请勿随意改端口"],
            ["18890–18920", "微灵", "本地媒体服务", "仅缓存到字节时兜底；正常路径直接返回 CDN 直链，见 8.3 节"],
        ],
        widths=[2.6, 3.0, 4.6, 5.8],
        caption="表 3-7　端口一览",
    )
    K.add_bullet(doc, "同机部署时建议 OneBot 实现监听 127.0.0.1，减少暴露面；跨机部署监听 0.0.0.0 并限制来源 IP。")
    K.add_bullet(doc, "跨公网时建议用 `wss://`（TLS），避免 Token 与消息内容明文传输；可在反向代理上终止 TLS。")
    K.add_bullet(doc, "若 AstrBot 与微灵不在同一台机器，微灵配置中的 127.0.0.1 必须改为 AstrBot 主机可达的 IP。")
