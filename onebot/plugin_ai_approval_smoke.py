import importlib.util
import sys

CORE = r"C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\core"
sys.path.insert(0, CORE)

spec = importlib.util.spec_from_file_location(
    "ai_approval_under_test", r"D:\wecom-ling\onebot\plugin_ai_approval\main.py"
)
mod = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = mod
spec.loader.exec_module(mod)

print("== data root:", mod._data_root())
print("== default audit path:", mod._data_root() / mod.AUDIT_DIR_NAME / "audit.jsonl")

samples = [
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
print("\n== looks_like_application ==")
for s in samples:
    print(f"{mod.looks_like_application(s)!s:5} | {s}")

print("\n== parse_application ==")
for s in samples:
    app = mod.parse_application(s, "u1", "张三")
    print(s)
    print("   ->", None if app is None else {
        "type": app.type, "reason": app.reason, "days": app.days,
        "hours": app.hours, "amount": app.amount, "time": app.time_desc,
        "structured": app.structured,
    })

print("\n== neighbors should NOT look like applications ==")
negatives = [
    "今天天气不错",
    "这个请假流程怎么走？",  # 提到请假但没有申请要素
    "我报销完了吗",
    "群公告：明天开会",
    "",
]
for s in negatives:
    print(f"{mod.looks_like_application(s)!s:5} | {s}")

print("\n== evaluate_rules ==")
cases = [
    ("申请请假 3 天，事由：家里有事", False, ""),
    ("申请请假 1 天，事由：家里有事", True, ""),
    ("申请 2026-11-20 外出 2 小时", True, ""),
    ("报销 1200 元，打车费", False, ""),
    ("报销 120 元，打车费", False, "default auto types 不含报销"),
    ("申请请假，家里有事", False, "缺时长"),
    ("#审批 其他 采购服务器 金额 3000 元", False, "其他类型"),
]
for s, expected, note in cases:
    app = mod.parse_application(s)
    ok, reason = mod.evaluate_rules(app, ["请假", "外出"], 2.0, 500.0)
    status = "OK" if ok == expected else "MISMATCH"
    print(f"{status:8} expected={expected!s:5} got={ok!s:5} | {s} | {reason} {note}")

print("\n== structured alias type ==")
app = mod.parse_application("#审批 休假 2 天 事假")
print(app)

print("\n== ID sample:", mod._new_application_id())
