# -*- coding: utf-8 -*-
"""端到端（进程内）测试：用假 event/context 直接驱动插件处理器，验证
自动通过分支、管理员群提醒、审计落盘（不污染实例真实审计文件）。

用法：实例 venv python e2e_fake_event.py
"""

from __future__ import annotations

import asyncio
import importlib.util
import json
import os
import sys
import tempfile
from pathlib import Path
from types import SimpleNamespace

PLUGIN_DIR = Path(__file__).resolve().parent.parent
INSTANCE_CORE = Path(
    r"C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\core"
)

os.environ.setdefault("ASTRBOT_ROOT", str(INSTANCE_CORE))
sys.path.insert(0, str(INSTANCE_CORE))

spec = importlib.util.spec_from_file_location("ai_approval_fake_e2e", PLUGIN_DIR / "main.py")
assert spec and spec.loader
mod = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = mod
spec.loader.exec_module(mod)

TMP = Path(tempfile.mkdtemp(prefix="ai_approval_e2e_"))
AUDIT = TMP / "audit.jsonl"

CFG = {
    "approval_group_ids": ["999001"],
    "admin_group_ids": ["888001"],
    "admin_user_ids": [],
    "auto_approve_types": ["请假", "外出"],
    "auto_approve_max_leave_days": 2.0,
    "auto_approve_max_amount": 500.0,
    "use_llm": False,
    "llm_provider_id": "",
    "audit_log_path": str(AUDIT),
}


class FakeChain:
    def __init__(self, texts):
        self.texts = texts


class FakeContext:
    def __init__(self):
        self.sent: list[tuple[str, str]] = []

    async def send_message(self, session, chain):
        components = getattr(chain, "chain", chain)
        text = "".join(getattr(c, "text", "") for c in components)
        self.sent.append((str(session), text))
        return True


class FakeEvent:
    def __init__(self, text, sender_id="u1001", sender_name="张三", group_id="999001", private=False):
        self.text = text
        self.sender_id = sender_id
        self.sender_name = sender_name
        self.group_id = group_id
        self.private = private
        self.is_at_or_wake_command = True
        self.stopped = False
        self.session = SimpleNamespace(platform_name="wecom")

    def get_message_str(self):
        return self.text

    def get_sender_id(self):
        return self.sender_id

    def get_sender_name(self):
        return self.sender_name

    def get_group_id(self):
        return self.group_id

    def is_private_chat(self):
        return self.private

    def is_admin(self):
        return False

    def get_platform_id(self):
        return "wecom"

    def plain_result(self, text):
        return SimpleNamespace(kind="plain", text=text)

    def stop_event(self):
        self.stopped = True


async def collect(plugin, event):
    out = []
    async for res in plugin.handle_application(event):
        out.append(res)
    return out


async def main() -> None:
    ctx = FakeContext()
    plugin = mod.AIApprovalPlugin(ctx, CFG)
    await plugin.initialize()

    failures = []

    def check(name, cond, detail=""):
        print(("  PASS  " if cond else "  FAIL  ") + name + ("" if cond else f"  {detail}"))
        if not cond:
            failures.append(name)

    print("== 1) 审批群内 请假 1 天 -> 自动通过 ==")
    ev = FakeEvent("申请请假 1 天，事由：感冒发烧")
    out = await collect(plugin, ev)
    text = "\n".join(r.text for r in out)
    print(text)
    check("回复含自动通过", "✅ 审批已自动通过" in text)
    check("回复含申请ID", "AP" in text)
    check("审计文件存在", AUDIT.exists())
    lines = AUDIT.read_text(encoding="utf-8").strip().splitlines()
    check("审计 1 条", len(lines) == 1, str(len(lines)))
    rec = json.loads(lines[-1])
    check("decision=auto_approved", rec["decision"] == "auto_approved", rec["decision"])
    check("decider=rule_engine", rec["decider"] == "rule_engine", rec["decider"])
    check("type=请假", rec["type"] == "请假", rec["type"])
    check("days=1.0", rec["days"] == 1.0, str(rec["days"]))
    check("applicant_name=张三", rec["applicant_name"] == "张三", rec["applicant_name"])
    check("自动通过无管理员通知", ctx.sent == [], str(ctx.sent))

    print("\n== 2) 审批群内 请假 5 天 -> 转管理员群 ==")
    ctx.sent.clear()
    ev = FakeEvent("申请请假 5 天，事由：回老家")
    out = await collect(plugin, ev)
    text = "\n".join(r.text for r in out)
    print(text)
    check("回复含待审批", "📩 已提交待管理员审批" in text)
    app_id = [r.text for r in out][0]
    check("通知发送 1 条", len(ctx.sent) == 1, str(ctx.sent))
    if ctx.sent:
        sess, body = ctx.sent[0]
        check("通知会话为管理员群", sess == "wecom:GroupMessage:888001", sess)
        check("通知含申请摘要", "请假" in body and "回老家" in body)
        check("通知含申请ID", "AP" in body)
    lines = AUDIT.read_text(encoding="utf-8").strip().splitlines()
    rec = json.loads(lines[-1])
    check("decision=pending", rec["decision"] == "pending", rec["decision"])
    check("decider=admin_group", rec["decider"] == "admin_group", rec["decider"])

    print("\n== 3) 普通群（非审批群）消息 -> filter 不激活 ==")
    ev = FakeEvent("申请请假 1 天", group_id="777001")
    active = mod.ApprovalMessageFilter().filter(ev, {})
    check("filter=False", active is False, str(active))
    ev2 = FakeEvent("申请请假 1 天", private=True)
    check("私聊 filter=True", mod.ApprovalMessageFilter().filter(ev2, {}) is True)
    ev3 = FakeEvent("今天天气不错", private=True)
    check("私聊普通聊天 filter=False", mod.ApprovalMessageFilter().filter(ev3, {}) is False)

    print("\n== 4) 报销 1200 元 -> pending（金额超限） ==")
    ctx.sent.clear()
    ev = FakeEvent("报销 1200 元，打车费")
    out = await collect(plugin, ev)
    lines = AUDIT.read_text(encoding="utf-8").strip().splitlines()
    rec = json.loads(lines[-1])
    check("decision=pending", rec["decision"] == "pending", rec["decision"])
    check("amount=1200", rec["amount"] == 1200.0, str(rec["amount"]))
    check("已通知管理员", len(ctx.sent) == 1, str(ctx.sent))

    print("\n== 5) 帮助/规则命令输出 ==")
    help_text = plugin._format_help()
    rules_text = plugin._format_rules()
    check("帮助含命令", "/ai审批帮助" in help_text and "/ai审批记录" in help_text)
    check("规则含审计路径", str(AUDIT) in rules_text, rules_text[-200:])

    print("\n--- 审计文件内容 ---")
    print(AUDIT.read_text(encoding="utf-8"))
    print(f"\nRESULT: {'ALL PASS' if not failures else 'FAILURES: ' + str(failures)}")
    await plugin.terminate()
    if failures:
        sys.exit(1)


if __name__ == "__main__":
    asyncio.run(main())
