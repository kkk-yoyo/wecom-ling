# -*- coding: utf-8 -*-
"""v1.3 修订：在第 3 章新增 3.1「运行环境要求（重要）」并顺延 3.x 编号。
对所有替换做 count==1 断言，任一失败即中止，不写盘。"""
import io
import os
import sys

DOCS = os.path.dirname(os.path.abspath(__file__))


def load(name):
    with io.open(os.path.join(DOCS, name), "r", encoding="utf-8") as f:
        return f.read()


def save(name, text):
    with io.open(os.path.join(DOCS, name), "w", encoding="utf-8", newline="\n") as f:
        f.write(text)


def replace(text, old, new, label):
    n = text.count(old)
    if n != 1:
        raise SystemExit("FAIL %s: found %d occurrences of %r" % (label, n, old[:80]))
    return text.replace(old, new, 1)


# ---------------------------------------------------------------- ch_deploy
p = "ch_deploy.py"
t = load(p)

t = replace(t,
    '    K.add_para(doc, "本章说明改造版的部署形态、地址与 Token 规则、微灵可配置项，以及对接 AstrBot 之外其他 "',
    '    K.add_para(doc, "本章说明运行环境要求、改造版的部署形态、地址与 Token 规则、微灵可配置项，以及对接 AstrBot 之外其他 "',
    p + " intro")

NEW_SECTION = '''    K.add_h2(doc, "3.1", "运行环境要求（重要）")
    K.add_para(doc, "部署前请先确认运行目录：**微灵必须放在不被本机安全软件或终端管控策略限制的普通目录中运行**。"
                    "现场已定位：若程序目录被列入受限路径，会出现一组与 OneBot 改造补丁无关的故障——"
                    "企业微信注入失败、HTTP 8060 不监听、无法写入 `C:\\\\Users` 目录等；"
                    "同一份 exe 复制到普通目录后立即恢复正常，说明问题在运行环境而不在程序或补丁本身。")
    K.add_callout(doc, "warn", "**推荐目录：`D:\\\\微灵OneBotV11\\\\`**（本机已验证）。"
                               "不要放在 `D:\\\\wecom-ling\\\\`、`C:\\\\Users\\\\...\\\\Downloads`、VDI 同步盘等"
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
            ["无法写入用户目录", "配置或日志写入 `C:\\\\Users\\\\...`（AppData 等）失败", "改用普通本地目录并确认目录可写"],
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
            ["`D:\\\\微灵OneBotV11\\\\`", "**推荐**", "本机已验证：内含 `微灵-OneBotV11-1.11.0.exe` 与 `PersonalTcpBridge.exe`"],
            ["`D:\\\\wecom-ling\\\\`", "不要使用", "本机该目录被安全软件列入受限路径，出现 8060 不监听、注入失败"],
            ["`C:\\\\Users\\\\...\\\\Downloads`、VDI/同步盘", "避免", "可能被终端安全、虚拟化或同步策略管控"],
            ["其它普通本地盘目录（如 `D:\\\\wecom-ling-app\\\\`）", "可用", "按上面的判别方法验证后再投入运行"],
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
                               "若换目录后仍异常，再按第 10 章排查。")

'''

t = replace(t,
    '    K.add_h2(doc, "3.1", "部署形态")',
    NEW_SECTION + '    K.add_h2(doc, "3.2", "部署形态")',
    p + " new section")

for old, new in [
    ('K.add_h2(doc, "3.2", "连接地址规则")', 'K.add_h2(doc, "3.3", "连接地址规则")'),
    ('K.add_h2(doc, "3.3", "Token 与鉴权")', 'K.add_h2(doc, "3.4", "Token 与鉴权")'),
    ('K.add_h2(doc, "3.4", "配置项说明")', 'K.add_h2(doc, "3.5", "配置项说明")'),
    ('K.add_h2(doc, "3.5", "对接其他 OneBot v11 实现")', 'K.add_h2(doc, "3.6", "对接其他 OneBot v11 实现")'),
    ('K.add_h2(doc, "3.6", "网络与端口")', 'K.add_h2(doc, "3.7", "网络与端口")'),
    ('caption="表 3-1　部署方式与地址"', 'caption="表 3-3　部署方式与地址"'),
    ('caption="表 3-2　地址规范化规则"', 'caption="表 3-4　地址规范化规则"'),
    ('caption="表 3-3　与 OneBot 对接相关的配置项"', 'caption="表 3-5　与 OneBot 对接相关的配置项"'),
    ('caption="表 3-4　对 OneBot v11 实现的要求"', 'caption="表 3-6　对 OneBot v11 实现的要求"'),
    ('caption="表 3-5　端口一览"', 'caption="表 3-7　端口一览"'),
    ('"OneBot 实现的反向 WS 地址，规则见 3.2 节"', '"OneBot 实现的反向 WS 地址，规则见 3.3 节"'),
    ('若实现只支持 query token，见 3.3 节兼容手段', '若实现只支持 query token，见 3.4 节兼容手段'),
]:
    t = replace(t, old, new, p + " " + old[:40])
save(p, t)

# ---------------------------------------------------------------- ch_quick
p = "ch_quick.py"
t = load(p)
t = replace(t,
    '            ["微灵 HTTP 端口", "默认 8060 未被修改", "shim 通过它探测 self_id；修改后需依赖登录事件（见 10.1 节）"],',
    '            ["微灵 HTTP 端口", "默认 8060 未被修改", "shim 通过它探测 self_id；修改后需依赖登录事件（见 10.1 节）"],\n'
    '            ["运行目录", "普通本地目录（如 `D:\\\\微灵OneBotV11\\\\`）", "不要放在被安全软件/终端策略管控的路径，'
    '否则 8060 不监听、企微注入失败（见 3.1 节）"],',
    p + " 2.1 row")
t = replace(t, "地址规则详见 3.2 节。", "地址规则详见 3.3 节。", p + " ref1")
t = replace(t, "检查端口占用、适配器是否启用（3.6 节）", "检查端口占用、适配器是否启用（3.7 节）", p + " ref2")
t = replace(t, "检查地址/路径、Token、网络（3.2/3.3 节）", "检查地址/路径、Token、网络（3.3/3.4 节）", p + " ref3")
t = replace(t, "按 3.2 节规则补全路径。", "按 3.3 节规则补全路径。", p + " ref4")
save(p, t)

# ---------------------------------------------------------------- ch_limits
p = "ch_limits.py"
t = load(p)
t = replace(t, "按 3.2 节补路径", "按 3.3 节补路径", p + " ref")
t = replace(t,
    '            ["握手 404/500", "路径不对（缺 `/ws`）；对端未开反向 WS", "按 3.3 节补路径；确认对端为服务端模式"],',
    '            ["握手 404/500", "路径不对（缺 `/ws`）；对端未开反向 WS", "按 3.3 节补路径；确认对端为服务端模式"],\n'
    '            ["8060 不监听 / 企微注入失败", "程序目录被安全软件或终端管控策略限制（本机为传云 Vault + CloudEDR）",'
    ' "把整个程序目录复制到普通目录（如 `D:\\\\微灵OneBotV11\\\\`）后重启，见 3.1 节"],',
    p + " faq row")
save(p, t)

# ---------------------------------------------------------------- ch_protocol
p = "ch_protocol.py"
t = load(p)
t = replace(t, "按 3.2 节规则规范化", "按 3.3 节规则规范化", p + " ref")
save(p, t)

# ---------------------------------------------------------------- build_doc (TOC)
p = "build_doc.py"
t = load(p)
t = replace(t,
    '''    (1, "3", "部署与配置"),
    (2, "3.1", "部署形态"),
    (2, "3.2", "连接地址规则"),
    (2, "3.3", "Token 与鉴权"),
    (2, "3.4", "配置项说明"),
    (2, "3.5", "对接其他 OneBot v11 实现"),
    (2, "3.6", "网络与端口"),''',
    '''    (1, "3", "部署与配置"),
    (2, "3.1", "运行环境要求（重要）"),
    (2, "3.2", "部署形态"),
    (2, "3.3", "连接地址规则"),
    (2, "3.4", "Token 与鉴权"),
    (2, "3.5", "配置项说明"),
    (2, "3.6", "对接其他 OneBot v11 实现"),
    (2, "3.7", "网络与端口"),''',
    p + " toc")
save(p, t)

# ---------------------------------------------------------------- docx_kit (cover version)
p = "docx_kit.py"
t = load(p)
t = replace(t, '("文档版本", "v1.2")', '("文档版本", "v1.3")', p + " version")
save(p, t)

# ---------------------------------------------------------------- ch_appendix (version rows)
p = "ch_appendix.py"
t = load(p)
t = replace(t,
    '''            ["v1.2", "2026-11-12", "同步 13:21:15 构建：彻底移除入站静默看门狗（`ReceiveWatchdogMs` 常量删除），"
                                    "空闲连接不再周期性重连；4.6 改述为链路检测"],''',
    '''            ["v1.2", "2026-11-12", "同步 13:21:15 构建：彻底移除入站静默看门狗（`ReceiveWatchdogMs` 常量删除），"
                                    "空闲连接不再周期性重连；4.6 改述为链路检测"],
            ["v1.3", "2026-11-12", "新增 3.1「运行环境要求（重要）」：安全软件/终端管控限制运行目录时，"
                                    "会出现 8060 不监听、企微注入失败等与补丁无关的故障；给出症状判别、正确目录与本机实测证据"],''',
    p + " v13")
save(p, t)

print("OK: v1.3 patch applied")
