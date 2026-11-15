"""插件冒烟测试：解析 / 规则判定 / 审计 / 配置 schema 校验（不需要真实 AstrBot 运行）。"""

from __future__ import annotations

import importlib.util
import json
import os
import sys
from pathlib import Path

PLUGIN_DIR = Path(__file__).resolve().parent.parent
INSTANCE_CORE = Path(
    r"C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\core"
)

os.environ.setdefault("ASTRBOT_ROOT", str(INSTANCE_CORE))
sys.path.insert(0, str(INSTANCE_CORE))

spec = importlib.util.spec_from_file_location("ai_approval_under_test", PLUGIN_DIR / "main.py")
assert spec and spec.loader
mod = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = mod
spec.loader.exec_module(mod)

failures: list[str] = []
checks = 0


def check(name: str, condition: bool, detail: str = "") -> None:
    global checks
    checks += 1
    if condition:
        print(f"  PASS  {name}")
    else:
        print(f"  FAIL  {name}  {detail}")
        failures.append(f"{name} {detail}")


print("== 数据目录 ==")
print("  data root:", mod._data_root())
check(
    "默认审计路径指向实例 core/data",
    str(mod._data_root()).lower() == str(INSTANCE_CORE / "data").lower(),
    str(mod._data_root()),
)

print("\n== looks_like_application（应识别为申请） ==")
positive = [
    "申请请假 3 天，事由：家里有事",
    "报销 1200 元，打车费",
    "申请 2026-11-20 外出 2 小时",
    "#审批 请假 3 天 家里有事",
    "#审批 其他 采购服务器 金额 3000 元",
    "请两天假，家里有点事",
    "申请调休 1 天 去体检",
    "出差三天，上海客户",
    "加班 4 小时 版本发布",
]
for text in positive:
    check(text, mod.looks_like_application(text) is True)

print("\n== looks_like_application（普通聊天应安静忽略） ==")
negative = [
    "今天天气不错",
    "这个请假流程怎么走？",
    "我报销完了吗",
    "群公告：明天开会",
    "",
    "审批系统上线了吗",
]
for text in negative:
    check(repr(text), mod.looks_like_application(text) is False)

print("\n== parse_application ==")
app = mod.parse_application("申请请假 3 天，事由：家里有事")
assert app is not None
check("类型=请假", app.type == "请假", app.type)
check("天数=3", app.days == 3.0, str(app.days))
check("事由=家里有事", app.reason == "家里有事", app.reason)

app = mod.parse_application("报销 1200 元，打车费")
assert app is not None
check("报销金额=1200", app.amount == 1200.0, str(app.amount))
check("报销事由=打车费", app.reason == "打车费", app.reason)

app = mod.parse_application("申请 2026-11-20 外出 2 小时")
assert app is not None
check("外出时间=2026-11-20", app.time_desc == "2026-11-20", app.time_desc)
check("外出 2 小时=0.25 天", app.days == 0.25, str(app.days))

app = mod.parse_application("#审批 其他 采购服务器 金额 3000 元")
assert app is not None
check("结构化类型=其他", app.type == "其他", app.type)
check("结构化金额=3000", app.amount == 3000.0, str(app.amount))
check("事由无残留「元」", "元" not in app.reason, app.reason)

app = mod.parse_application("请两天假，家里有点事")
assert app is not None
check("请两天假类型=请假", app.type == "请假", app.type)
check("请两天假天数=2", app.days == 2.0, str(app.days))

print("\n== evaluate_rules ==")
auto_types = ["请假", "外出"]
cases = [
    ("申请请假 3 天，事由：家里有事", False),  # 超过 2 天
    ("申请请假 1 天，事由：家里有事", True),
    ("申请请假 2 天，事由：家里有事", True),  # 边界
    ("申请 2026-11-20 外出 2 小时", True),
    ("报销 120 元，打车费", False),  # 报销不在默认自动类型
    ("申请请假，家里有事", False),  # 缺时长
]
for text, expected in cases:
    parsed = mod.parse_application(text)
    assert parsed is not None
    passed, reason = mod.evaluate_rules(parsed, auto_types, 2.0, 500.0)
    check(f"{text} -> {expected}", passed == expected, reason)

# 金额上限：报销类型临时加入自动类型，验证金额规则
parsed = mod.parse_application("报销 1200 元，打车费")
assert parsed is not None
passed, reason = mod.evaluate_rules(parsed, ["报销"], 2.0, 500.0)
check("报销 1200 超金额上限 -> False", passed is False, reason)
parsed = mod.parse_application("报销 300 元，打车费")
assert parsed is not None
passed, reason = mod.evaluate_rules(parsed, ["报销"], 2.0, 500.0)
check("报销 300 在金额上限内 -> True", passed is True, reason)

print("\n== 申请 ID ==")
ids = {mod._new_application_id() for _ in range(50)}
check("ID 唯一", len(ids) == 50)
check("ID 形如 AP+14位时间+4位HEX", all(__import__("re").fullmatch(r"AP\d{14}[0-9A-F]{4}", i) for i in ids))

print("\n== _conf_schema.json 校验 ==")
schema_path = PLUGIN_DIR / "_conf_schema.json"
schema = json.loads(schema_path.read_text(encoding="utf-8-sig"))
required_keys = [
    "approval_group_ids",
    "admin_group_ids",
    "admin_user_ids",
    "auto_approve_types",
    "auto_approve_max_leave_days",
    "auto_approve_max_amount",
    "use_llm",
    "llm_provider_id",
    "audit_log_path",
]
for key in required_keys:
    check(f"schema 含 {key}", key in schema)
defaults = {key: value.get("default") for key, value in schema.items()}
check("默认自动类型=请假/外出", defaults["auto_approve_types"] == ["请假", "外出"], str(defaults["auto_approve_types"]))
check("默认天数上限=2", defaults["auto_approve_max_leave_days"] == 2, str(defaults["auto_approve_max_leave_days"]))
check("默认金额上限=500", defaults["auto_approve_max_amount"] == 500, str(defaults["auto_approve_max_amount"]))
check("默认 use_llm=False", defaults["use_llm"] is False)
check("schema 类型合法", all(v.get("type") in {"int", "float", "bool", "string", "text", "list", "file", "object", "dict", "template_list"} for v in schema.values()))

print("\n== 审计 JSONL 写入/读取（真实插件方法） ==")
import asyncio

audit_tmp = PLUGIN_DIR / "_smoke_audit.jsonl"
if audit_tmp.exists():
    audit_tmp.unlink()

cfg = dict(defaults)
cfg["audit_log_path"] = str(audit_tmp)
plugin = mod.AIApprovalPlugin(None, cfg)
check("默认配置可实例化插件", plugin is not None)
check("审计路径按配置解析", plugin._get_audit_path() == audit_tmp, str(plugin._get_audit_path()))

records = [
    {"ts": "2026-11-13T13:00:00+08:00", "applicant_id": "u1", "applicant_name": "张三", "type": "请假", "reason": "家里有事", "days": 1.0, "amount": None, "decision": "auto_approved", "decider": "rule_engine", "reason_text": "符合自动通过规则"},
    {"ts": "2026-11-13T13:01:00+08:00", "applicant_id": "u2", "applicant_name": "李四", "type": "报销", "reason": "打车费", "days": None, "amount": 1200.0, "decision": "pending", "decider": "admin_group", "reason_text": "金额超限"},
]
for rec in records:
    asyncio.run(plugin._write_audit(rec))
loaded = plugin._read_recent_audit(10)
check("审计行数正确", len(loaded) == 2, str(len(loaded)))
check(
    "审计字段完整",
    set(loaded[0]) == {"ts", "applicant_id", "applicant_name", "type", "reason", "days", "amount", "decision", "decider", "reason_text"},
    str(sorted(loaded[0])),
)
check("中文不转义", "张三" in audit_tmp.read_text(encoding="utf-8"))
check("最近 1 条返回最后一条", len(plugin._read_recent_audit(1)) == 1 and plugin._read_recent_audit(1)[0]["applicant_name"] == "李四")
audit_tmp.unlink()

print("\n== 文本模板 ==")
help_text = plugin._format_help()
rules_text = plugin._format_rules()
check("帮助含示例", "申请请假 3 天" in help_text and "/ai审批帮助" in help_text)
check("规则含自动类型", "请假" in rules_text and "天数上限" in rules_text)
check("提示含结构化格式", "#审批" in plugin._format_hint())

print("\n== metadata.yaml ==")
try:
    import yaml

    meta = yaml.safe_load((PLUGIN_DIR / "metadata.yaml").read_text(encoding="utf-8"))
    check("name 合法且匹配目录", meta.get("name") == "astrbot_plugin_ai_approval", str(meta.get("name")))
    check("含 author/desc/version", all(meta.get(k) for k in ("author", "desc", "version")))
    check("astrbot_version 合法 PEP440 区间", meta.get("astrbot_version") == ">=4.16,<5", str(meta.get("astrbot_version")))
    check("support_platforms 含 aiocqhttp", "aiocqhttp" in (meta.get("support_platforms") or []))
except ImportError:
    print("  SKIP  yaml 不可用")

print(f"\n== 结果：{checks - len(failures)}/{checks} 通过 ==")
if failures:
    print("\n失败项：")
    for item in failures:
        print("  -", item)
    sys.exit(1)
print("全部通过")
