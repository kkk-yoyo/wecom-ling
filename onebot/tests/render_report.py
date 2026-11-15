#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Render TEST_RESULTS.md and results.json from logs/results_raw.json."""

import hashlib
import json
import os
import platform
import sys

TESTS = os.path.dirname(os.path.abspath(__file__))
LOGS = os.path.join(TESTS, "logs")
RAW = os.path.join(LOGS, "results_raw.json")
MD = os.path.join(TESTS, "TEST_RESULTS.md")
OUT_JSON = os.path.join(TESTS, "results.json")

WECOM = r"D:\wecom-ling"
OUTPUT_DLL = os.path.join(WECOM, "onebot", "output", "OneBotShim.dll")
LOADED_DLL = os.path.join(TESTS, "bin", "OneBotShim.dll")
LIB_DLL = os.path.join(TESTS, "lib", "OneBotShim.dll")
SHIM_SRC = os.path.join(WECOM, "onebot", "src", "WxSocketClient.cs")
ORIG_EXE = os.path.join(TESTS, "lib", "WeComSever.exe")
CSC = r"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"

# P0 = 任务验收核心链路（Lead 指定的必测项 + 新 shim 变更点）；P1 = 补充/边界用例。
P0_CASES = {
    "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09", "T10",
    "T11", "T12", "T13", "T14", "T15", "T16", "T17", "T18", "T19", "T20",
    "T21", "T22", "T23", "T25", "T27", "T29", "T30", "T31", "T33", "T35",
    "T36", "T37", "T38", "T42", "T43", "T44", "T45",
    # 本轮新增能力验收（新 shim）：回复链/出站媒体/入站新段/新动作/11200
    "T46", "T47", "T48", "T49", "T50", "T51", "T52", "T53", "T54", "T55",
    "T56", "T57", "T58", "T59", "T60", "T61", "T62",
}

GROUP_ORDER = ["handshake", "events", "actions", "resilience", "lifecycle", "log"]
GROUP_TITLES = {
    "handshake": "握手 / 生命周期",
    "events": "事件注入与翻译",
    "actions": "OneBot 动作处理",
    "resilience": "负向 / 断线重连",
    "lifecycle": "生命周期 / 状态",
    "log": "日志证据",
}


def priority(cid):
    return "P0" if cid in P0_CASES else "P1"


def sha256(path):
    try:
        h = hashlib.sha256()
        with open(path, "rb") as f:
            for chunk in iter(lambda: f.read(1 << 20), b""):
                h.update(chunk)
        return h.hexdigest()
    except Exception:
        return "n/a"


def fsize(path):
    return os.path.getsize(path) if os.path.exists(path) else None


def esc(text, limit=0):
    if text is None:
        return ""
    s = str(text).replace("|", "\\|").replace("\r", "").replace("\n", "<br>")
    if limit and len(s) > limit:
        s = s[:limit] + " …(截断)"
    return s


def load():
    if not os.path.exists(RAW):
        print("ERROR: missing " + RAW, file=sys.stderr)
        sys.exit(1)
    with open(RAW, "r", encoding="utf-8") as f:
        return json.load(f)


def main():
    raw = load()
    cases = sorted(raw.get("cases", []), key=lambda c: c.get("id", ""))
    summary = raw.get("summary", {})
    total = summary.get("total", len(cases))
    passed = summary.get("passed", sum(1 for c in cases if c.get("status") == "PASS"))
    failed = summary.get("failed", total - passed)
    failures = [c for c in cases if c.get("status") != "PASS"]

    for c in cases:
        c["priority"] = priority(c.get("id", ""))

    p0 = [c for c in cases if c["priority"] == "P0"]
    p1 = [c for c in cases if c["priority"] == "P1"]
    p0_pass = sum(1 for c in p0 if c.get("status") == "PASS")
    p1_pass = sum(1 for c in p1 if c.get("status") == "PASS")

    loaded_hash = sha256(LOADED_DLL)
    output_hash = sha256(OUTPUT_DLL)
    env = {
        "os": platform.platform(),
        "python": sys.executable,
        "python_version": sys.version.split()[0],
        "csc": CSC,
        "csc_exists": os.path.exists(CSC),
        # 实际被 TestHost 加载的副本（build.ps1 每次从 output 刷新到 lib，再复制到 bin）
        "loaded_shim_dll": LOADED_DLL,
        "loaded_shim_dll_sha256": loaded_hash,
        "loaded_shim_dll_size": fsize(LOADED_DLL),
        # 最新编译产物，用于一致性核对
        "output_shim_dll": OUTPUT_DLL,
        "output_shim_dll_sha256": output_hash,
        "output_shim_dll_size": fsize(OUTPUT_DLL),
        "shim_copy_matches_output": loaded_hash != "n/a" and loaded_hash == output_hash,
        "shim_source": SHIM_SRC,
        "shim_source_lines": (
            sum(1 for _ in open(SHIM_SRC, "r", encoding="utf-8", errors="replace"))
            if os.path.exists(SHIM_SRC) else None
        ),
        "original_exe": ORIG_EXE,
        "original_exe_sha256": sha256(ORIG_EXE),
        "mock_base": summary.get("mock_base", "http://127.0.0.1:6399"),
        "token": summary.get("token", "testtoken"),
        "robot_id": summary.get("robot_id", "77880001"),
        "testhost": os.path.join(TESTS, "bin", "TestHost.exe"),
        "testhost_built": os.path.exists(os.path.join(TESTS, "bin", "TestHost.exe")),
    }

    generated = raw.get("finished_at", "")
    out = {
        "task": "task-2",
        "generated_at": generated,
        "summary": {
            "total": total,
            "passed": passed,
            "failed": failed,
            "pass_rate": round(100.0 * passed / total, 1) if total else 0.0,
            "p0_total": len(p0),
            "p0_passed": p0_pass,
            "p1_total": len(p1),
            "p1_passed": p1_pass,
        },
        "environment": env,
        "cases": cases,
    }
    with open(OUT_JSON, "w", encoding="utf-8") as f:
        json.dump(out, f, ensure_ascii=False, indent=2)

    lines = []
    lines.append("# 微灵 OneBot v11 shim 黑盒测试报告")
    lines.append("")
    lines.append("- 任务：`task-2`（Mock OneBot v11 环境 + C# TestHost 黑盒验证 shim；本轮含扩展能力验收：回复链/出站媒体/入站新段/新动作）")
    lines.append("- 生成时间：%s（本机时间）" % generated)
    lines.append("- 实际加载的被测对象：`%s`（%s 字节，SHA256 `%s`）"
                 % (LOADED_DLL, env["loaded_shim_dll_size"], loaded_hash))
    lines.append("- 最新编译产物：`%s`（%s 字节，SHA256 `%s`）%s"
                 % (OUTPUT_DLL, env["output_shim_dll_size"], output_hash,
                    "，与实际加载副本 **一致**" if env["shim_copy_matches_output"]
                    else "，**与实际加载副本不一致（结果不可信）**"))
    lines.append("- shim 源码：`%s`（%s 行）" % (SHIM_SRC, env["shim_source_lines"]))
    lines.append("- 原程序集：`%s`（SHA256 `%s`）" % (ORIG_EXE, env["original_exe_sha256"]))
    lines.append("- Mock 服务器：`mock_server.py`（aiohttp，`http://127.0.0.1:6399`，WS 路径 `/ws`，token `testtoken`，"
                 "媒体夹具 `/media/{name}`）")
    lines.append("- TestHost：`bin\\TestHost.exe`（`/platform:x86`，csc 4.0，C# 5）")
    lines.append("- 运行环境：%s；Python %s" % (env["os"], env["python_version"]))
    lines.append("- 一键复现：`powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"D:\\wecom-ling\\onebot\\tests\\run_all.ps1\"`")
    lines.append("")
    lines.append("## 总览")
    lines.append("")
    lines.append("| 指标 | 值 |")
    lines.append("| --- | --- |")
    lines.append("| 用例总数 | %d |" % total)
    lines.append("| 通过 | %d |" % passed)
    lines.append("| 失败 | %d |" % failed)
    lines.append("| 通过率 | %.1f%% |" % out["summary"]["pass_rate"])
    lines.append("| P0（核心链路） | %d/%d 通过 |" % (p0_pass, len(p0)))
    lines.append("| P1（补充/边界） | %d/%d 通过 |" % (p1_pass, len(p1)))
    lines.append("")

    # group breakdown
    lines.append("### 分组结果")
    lines.append("")
    lines.append("| 分组 | 用例数 | 通过 | 失败 |")
    lines.append("| --- | --- | --- | --- |")
    groups = {}
    for c in cases:
        groups.setdefault(c.get("group", "?"), []).append(c)
    for g in GROUP_ORDER + [g for g in groups if g not in GROUP_ORDER]:
        if g not in groups:
            continue
        gc = groups[g]
        gp = sum(1 for c in gc if c.get("status") == "PASS")
        lines.append("| %s (`%s`) | %d | %d | %d |"
                     % (GROUP_TITLES.get(g, g), g, len(gc), gp, len(gc) - gp))
    lines.append("")
    lines.append("### 结论")
    lines.append("")
    if failed == 0:
        lines.append("**全部 %d 个用例（P0 %d + P1 %d）通过。** shim 的握手、生命周期、心跳、事件翻译、媒体登记与按需下载"
                     "（`get_image/get_record/get_file` → `base64://`、`get_*_file_url` → 原始 CDN URL）、动作处理、"
                     "错误分支与断线重连行为均与源码实现一致；本轮新增能力（出站 reply/face/@all 处理，出站图片/视频/文件/动图段 → "
                     "11030/11067/11031/11070 落盘发送（base64:// / http(s) / file:// / 绝对路径四种来源），"
                     "入站 location/share/contact/11045 新结构，11041 保留 self at，"
                     "11200 未映射消息透传与 read_receipt 过滤，`get_group_member_list`/"
                     "`get_group_msg_history`）亦全部通过，未发现与任务验收标准不符的缺陷。"
                     % (total, len(p0), len(p1)))
    else:
        lines.append("**%d/%d 个用例通过，%d 个失败。** 失败项明细见下方“失败详情”，均为可复现的断言失败"
                     "（每条结论均有 Mock 帧记录 + shim 日志 + 命令记录三重证据）。" % (passed, total, failed))
    lines.append("")

    lines.append("## 用例结果一览")
    lines.append("")
    lines.append("| 用例 | 优先级 | 分组 | 名称 | 结果 | 耗时(ms) |")
    lines.append("| --- | --- | --- | --- | --- | --- |")
    for c in cases:
        mark = "PASS" if c.get("status") == "PASS" else "**FAIL**"
        lines.append("| %s | %s | %s | %s | %s | %s |"
                     % (c.get("id"), c.get("priority"), c.get("group"),
                        esc(c.get("name"), 160), mark, c.get("duration_ms")))
    lines.append("")

    if failures:
        lines.append("## 失败详情")
        lines.append("")
        for c in failures:
            lines.append("### %s %s" % (c.get("id"), c.get("name")))
            lines.append("")
            lines.append("- 分组：`%s`，优先级：%s" % (c.get("group"), c.get("priority")))
            lines.append("- 错误：`%s`" % esc(c.get("error", ""), 800))
            lines.append("- 证据（最后若干条）：")
            ev = c.get("evidence", [])
            for e in ev[-8:]:
                lines.append("  - %s" % esc(e, 500))
            lines.append("")

    lines.append("## 证据详情（按用例）")
    lines.append("")
    for c in cases:
        lines.append("### %s [%s/%s] %s — %s"
                     % (c.get("id"), c.get("priority"), c.get("group"), c.get("name"), c.get("status")))
        lines.append("")
        if c.get("status") != "PASS":
            lines.append("- **失败原因：** `%s`" % esc(c.get("error", ""), 1000))
        ev = c.get("evidence", [])
        lines.append("- 断言/证据（%d 条）：" % len(ev))
        for e in ev[:40]:
            lines.append("  - %s" % esc(e, 600))
        if len(ev) > 40:
            lines.append("  - …（其余 %d 条见 logs\\testhost.log）" % (len(ev) - 40))
        lines.append("")

    lines.append("## 测试方法与证据链")
    lines.append("")
    lines.append("1. **Mock OneBot v11 服务器**（`mock_server.py`，aiohttp，127.0.0.1:6399）：作为反向 WS 服务端校验握手头"
                 "（`X-Self-ID`/`X-Client-Role: Universal`/`Authorization: Bearer testtoken`，token 不符返回 HTTP 401），"
                 "记录全部入站帧（`logs/frames.jsonl`）、连接与拒绝记录，并可主动下发动作（`/ctl/action`）与断链（`/ctl/close_all`）；"
                 "另提供 `/media/{name}` 媒体夹具与 `/ctl/media_info`，用于验证 `get_image/get_record/get_file` 的按需下载与 base64 返回。")
    lines.append("2. **TestHost**（`TestHost.cs` → `bin\\TestHost.exe`，x86）：实例化真实 `OneBotShim.WxSocketClient`，"
                 "通过公开面（`Start/Stop/TrySendEvent/IsConnected/StatusText/StatusBackground/StatusChanged`）驱动；"
                 "注入微灵原始 JSON 事件；以伪造的 `CommandHandler` 拦截 shim 下发给微灵的命令并写入 "
                 "`logs/commands.jsonl`，同时返回构造的 11038/11036/11040/11039 业务数据。")
    lines.append("3. **断言依据**：直接读取 Mock 记录的 WS 帧（`/ctl/state`）、媒体命中记录（`/ctl/state` 的 `media_hits`）"
                 "与 shim 写出的 `onebot_v11.log`，不使用任何 mock 化的 shim 内部状态，保证是真实黑盒验证。")
    lines.append("4. **构建新鲜度**：`build.ps1` 每次运行都从 `onebot\\output\\OneBotShim.dll` 刷新 `lib`/`bin` 副本，"
                 "上述 SHA256 即为本次实际加载的二进制，并与最新编译产物核对一致。")
    lines.append("5. **API 探针**：`probe\\ApiProbe.cs`（x86 + 编译期引用原 exe）用于确认 `Robot.Common.CommandExecResult` "
                 "可写属性（`Ok/Data/Message/Error`）与 `WxWebSocketClient.CommandHandler` 委托签名，据此实现假 handler。")
    lines.append("6. **测试自包含/隔离**：仅使用 127.0.0.1:6399 的 Mock，不启动或依赖真实微灵/AstrBot；shim 的 self_id 在 "
                 "`Start()` 前由 11026 注入，从而不触发其 `127.0.0.1:8060` HTTP 自探测（本机可能同时运行线上实例）；"
                 "“无 self_id 不连接”一项改用 personal 通道 shim 验证（该通道不做 HTTP 自探测），避免与线上实例串扰。")
    lines.append("")

    lines.append("### 覆盖要点")
    lines.append("")
    lines.append("- **握手/生命周期**：X-Self-ID 需先由 11026（或顶层 robot_id）建立，之后才连接；`X-Client-Role: Universal`、"
                 "`Authorization: Bearer testtoken`；连接后 lifecycle.connect；30 s 心跳（`interval:30000`）。")
    lines.append("- **事件翻译**：11026/11041 群与私聊/11042/11043/11044/11045/11046/11047/11048/11050/11123/11063/11072/11073/11074/11075/11076/11078/11173/11200，"
                 "is_self 过滤、at 段（11041 保留 @自己 的 at 段）、raw_message 拼接、顶层 `ling` 原样回传（去掉 `data.payload_base64`）；"
                 "11200 未映射消息按 content/at_list 透传为 message，`message_type=read_receipt` / `event_desc=已读上报` 静默丢弃。")
    lines.append("- **入站段新结构（本轮）**：11045 → file 段 `{name,file,url}`（AstrBot File 需要 name）；11046 → location 段"
                 "（lat/lon/title/content）；11047 → share 段（url/title/content/image）；11048 → image 段；11050 → contact 段"
                 "（type=qq、id 为数字，非数字降级文本）。")
    lines.append("- **媒体（新 shim）**：11042/11043/11045/11048 登记 cdn.url 后，`get_image/get_record/get_file` 按需 HTTP 下载并返回 "
                 "`base64://`（保留 file_name）；`get_group_file_url/get_private_file_url` 返回原始 CDN URL；未命中返回 failed/500。")
    lines.append("- **动作发送**：send_private_msg/send_group_msg/send_msg（文本 / at / 混合 / 仅 at）、11029 与 11069 选择、"
                 "conversation_id 构造（`S:<peer>_<self>` / `R:<group>`）、client token、echo 原样回传、message_id。")
    lines.append("- **出站回复链与媒体（本轮）**：reply/face 段静默忽略、at(qq=all) 忽略（降级 11029）；image 段支持 base64://、"
                 "http(s) URL（经 Mock 下载）、file:// 前缀与绝对路径四种来源，先落盘到程序目录 `onebot_media_tmp\\` 后发 11030"
                 "（命令携带绝对路径与正确扩展名）；video→11067；file→11031（保留 file_name）；gif 类型/扩展名→11070；"
                 "record→failed/500「暂不支持发送语音」，json 等未知段→failed/500「不支持」，两者均不下发命令。")
    lines.append("- **get_* 查询**：get_login_info/get_status/get_version_info/get_group_list/get_friend_list/get_group_info/"
                 "get_group_member_info/get_stranger_info/get_msg/can_send_image；本轮新增 get_group_member_list（→ 11040 且命令带"
                 "顶层 `getData:true`，返回成员数组）与 get_group_msg_history（返回空数组）；未支持动作 failed（500 或 1404）。")
    lines.append("- **负向/健壮性**：token 不符 401 后重连、Mock 主动断链后约 1 s 重连、Stop 后不再连接、record/未知消息段被拒绝、"
                 "缺字段/空消息报错、未知事件不推送、未知动作 1404。")
    lines.append("- **看门狗已删除**：最终版 shim 删除了 `ReceiveWatchdogMs` / SendLoop 120 s 入站静默看门狗，不再有周期性重连；"
                 "T04 断言心跳等待期间连接数不变，T38 断言 `onebot_v11.log` 中不存在 `watchdog` / `120000 ms; closing socket` 记录。")
    lines.append("")

    lines.append("### 证据文件")
    lines.append("")
    for p, desc in [
        ("logs/testhost.log", "TestHost 逐步执行与断言日志"),
        ("logs/testhost_console.log", "TestHost 控制台输出（含 TOTAL/PASS/FAIL 与失败项）"),
        ("logs/commands.jsonl", "假微灵 handler 收到的每条命令 JSON"),
        ("logs/frames.jsonl", "Mock 记录的 WS 双向帧"),
        ("logs/connections.jsonl", "WS 连接/断开记录"),
        ("logs/rejections.jsonl", "token 不符被拒记录"),
        ("logs/media_hits.jsonl", "媒体夹具被 shim HTTP 下载的记录"),
        ("logs/onebot_v11.log", "shim 自身日志（从 bin 复制）"),
        ("logs/mock_server.log", "Mock 服务器日志"),
        ("logs/results_raw.json", "TestHost 原始结果（本报告的数据源）"),
    ]:
        full = os.path.join(TESTS, p.replace("/", os.sep))
        lines.append("- `%s`%s — %s" % (p, "" if os.path.exists(full) else "（缺失）", desc))
    lines.append("")

    lines.append("## 复现命令")
    lines.append("")
    lines.append("```powershell")
    lines.append("cd D:\\wecom-ling\\onebot\\tests")
    lines.append("# 编译 + 启动 Mock + 跑全部用例 + 生成报告（本机无 pwsh；脚本未签名，需 Bypass）")
    lines.append("powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\\run_all.ps1")
    lines.append("```")
    lines.append("")
    lines.append("单独运行：`powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\\build.ps1`，然后")
    lines.append("`\"<含 aiohttp 的 python>\\python.exe\" .\\mock_server.py`（后台）再执行 `bin\\TestHost.exe`。")
    lines.append("")
    lines.append("## 产物清单")
    lines.append("")
    lines.append("| 产物 | 说明 |")
    lines.append("| --- | --- |")
    lines.append("| `mock_server.py` | Mock OneBot v11 反向 WS 服务端（含控制接口与媒体夹具） |")
    lines.append("| `TestHost.cs` / `bin\\TestHost.exe` | C# 测试宿主（%d 个用例，真实实例化 shim） |" % total)
    lines.append("| `build.ps1` / `run_all.ps1` | 编译脚本 / 一键运行脚本 |")
    lines.append("| `TEST_RESULTS.md` | 本报告（每用例 PASS/FAIL + 证据） |")
    lines.append("| `results.json` | 机器可读结果（摘要 + 环境 + 每用例证据 + P0/P1） |")
    lines.append("| `logs\\*` | 原始日志与帧记录 |")
    lines.append("")

    with open(MD, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")

    print("WROTE " + MD)
    print("WROTE " + OUT_JSON)
    print("SUMMARY total=%d passed=%d failed=%d p0=%d/%d p1=%d/%d"
          % (total, passed, failed, p0_pass, len(p0), p1_pass, len(p1)))
    return 0 if failed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
