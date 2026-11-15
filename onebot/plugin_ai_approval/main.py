"""AstrBot 插件：AI 审批（中文）。

员工在审批群或私聊中提交请假 / 报销 / 外出 / 加班等申请，插件按配置规则
自动通过；不满足规则时转管理员群提醒，并把每一笔申请写入 JSONL 审计日志。

适配 AstrBot 4.29（插件 API：Star + filter.command + filter.custom_filter）。
"""

from __future__ import annotations

import asyncio
import inspect
import json
import os
import random
import re
from dataclasses import dataclass
from datetime import datetime, timedelta
from pathlib import Path
from typing import Any

from astrbot.api import logger
from astrbot.api.event import AstrMessageEvent, MessageChain, filter
from astrbot.api.event.filter import CustomFilter, EventMessageType
from astrbot.api.message_components import Plain
from astrbot.api.star import Context, Star, StarTools

try:  # 4.29 数据目录工具；旧版本可能不存在，做降级处理
    from astrbot.core.utils.astrbot_path import get_astrbot_data_path
except Exception:  # pragma: no cover - 仅用于兼容
    get_astrbot_data_path = None  # type: ignore[assignment]


PLUGIN_NAME = "astrbot_plugin_ai_approval"
AUDIT_DIR_NAME = "astrbot_plugin_ai_approval"

DEFAULT_AUTO_TYPES = ["请假", "外出"]
TYPE_ORDER = ["请假", "报销", "外出", "加班", "其他"]

# 类型别名 -> 标准类型
TYPE_ALIASES: dict[str, str] = {
    "请假": "请假",
    "休假": "请假",
    "调休": "请假",
    "病假": "请假",
    "年假": "请假",
    "事假": "请假",
    "报销": "报销",
    "报账": "报销",
    "差旅报销": "报销",
    "外出": "外出",
    "出差": "外出",
    "外勤": "外出",
    "加班": "加班",
    "其他": "其他",
}
_TYPE_ALIAS_SORTED = sorted(TYPE_ALIASES.items(), key=lambda kv: -len(kv[0]))

APPLY_MARKERS = (
    "申请",
    "审批",
    "报批",
    "请批",
    "提交",
    "我要",
    "我想",
    "需要",
    "麻烦",
    "帮我",
)

NUM_PATTERN = r"(?:\d+(?:\.\d+)?|[零一二两三四五六七八九十]+)"
DAY_RE = re.compile(rf"({NUM_PATTERN})\s*(?:个)?天")
HOUR_RE = re.compile(rf"({NUM_PATTERN})\s*(?:个)?(?:小时|钟头)")
HALF_DAY_RE = re.compile(r"半天")
HALF_HOUR_RE = re.compile(r"半\s*(?:个)?小时")
AMOUNT_UNIT_RE = re.compile(r"(\d+(?:\.\d+)?)\s*(?:元|块|人民币|rmb|RMB)")
AMOUNT_PREFIX_RE = re.compile(
    rf"(?:金额|费用|花费|报销金额|报销费用)\s*[：:]?\s*(?:¥|￥|人民币)?\s*(\d+(?:\.\d+)?)\s*(?:元|块|人民币)?",
)
AMOUNT_SYMBOL_RE = re.compile(r"(?:¥|￥)\s*(\d+(?:\.\d+)?)")

ASK_LEAVE_RE = re.compile(
    rf"请\s*(?:半天|{NUM_PATTERN}\s*天|{NUM_PATTERN}\s*个?(?:小时|钟头))\s*假",
)

# 事由清理时丢弃的“噪声词”
NOISE_WORDS = {
    "元",
    "块",
    "人民币",
    "金额",
    "费用",
    "花费",
    "的",
    "了",
    "吧",
    "呀",
    "啊",
    "哦",
    "呢",
}


DATE_YMD_RE = re.compile(r"(\d{4})\s*[-/年.]\s*(\d{1,2})\s*[-/月.]\s*(\d{1,2})\s*日?")
DATE_MD_RE = re.compile(r"(\d{1,2})\s*月\s*(\d{1,2})\s*[日号]")
DATE_DAY_RE = re.compile(r"(\d{1,2})\s*[日号]")
RELATIVE_DAYS = (("大后天", 3), ("后天", 2), ("明天", 1), ("今天", 0))

REASON_EXPLICIT_RE = re.compile(r"(?:事由|原因|理由|说明)\s*[：:]\s*([^\n]+)")
STRUCTURED_PREFIXES = ("#审批", "#審批", "#approval", "#审批单")

_PLUGIN_INSTANCE: "AIApprovalPlugin | None" = None


# --------------------------------------------------------------------------- #
# 基础工具函数（模块级，方便单独测试）
# --------------------------------------------------------------------------- #


_CN_DIGITS = {
    "零": 0,
    "一": 1,
    "二": 2,
    "两": 2,
    "三": 3,
    "四": 4,
    "五": 5,
    "六": 6,
    "七": 7,
    "八": 8,
    "九": 9,
    "十": 10,
}


def _cn_to_number(text: str) -> float | None:
    """把简单中文数字（含十位，如 十二 / 二十 / 二十一）转成数字。"""
    text = text.strip()
    if not text:
        return None
    if text.isdigit():
        return float(text)
    if text in _CN_DIGITS:
        return float(_CN_DIGITS[text])
    if "十" in text:
        head, _, tail = text.partition("十")
        tens = _CN_DIGITS.get(head, 1) if head else 1
        ones = _CN_DIGITS.get(tail, 0) if tail else 0
        if head and head not in _CN_DIGITS:
            return None
        if tail and tail not in _CN_DIGITS:
            return None
        return float(tens * 10 + ones)
    return None


def _to_number(text: str) -> float | None:
    text = (text or "").strip()
    if not text:
        return None
    try:
        if re.fullmatch(r"\d+(?:\.\d+)?", text):
            return float(text)
        return _cn_to_number(text)
    except Exception:
        return None


def _as_float(value: Any, default: float) -> float:
    try:
        if value is None or value == "":
            return default
        return float(value)
    except (TypeError, ValueError):
        return default


def _as_bool(value: Any, default: bool = False) -> bool:
    if isinstance(value, bool):
        return value
    if isinstance(value, (int, float)):
        return bool(value)
    if isinstance(value, str):
        return value.strip().lower() in ("1", "true", "yes", "on", "是", "开启")
    return default


def _as_str_list(value: Any) -> list[str]:
    """把配置值统一成字符串列表（兼容字符串 / 列表 / 其它可迭代对象）。"""
    if value is None:
        return []
    if isinstance(value, str):
        parts = re.split(r"[\s,，;；]+", value)
        return [p.strip() for p in parts if p.strip()]
    if isinstance(value, (list, tuple, set)):
        result: list[str] = []
        for item in value:
            if isinstance(item, dict):
                continue
            text = str(item).strip()
            if text:
                result.append(text)
        return result
    text = str(value).strip()
    return [text] if text else []


def _canonical_type(value: str) -> str:
    value = (value or "").strip()
    return TYPE_ALIASES.get(value, value)


def _fmt_number(value: float | None) -> str:
    if value is None:
        return "-"
    if float(value).is_integer():
        return str(int(value))
    return f"{value:g}"


def _strip_spans(text: str, spans: list[tuple[int, int]]) -> str:
    for start, end in sorted(spans, key=lambda x: -x[0]):
        text = text[:start] + " " + text[end:]
    return text


def _clean_fragment(text: str) -> str:
    text = re.sub(r"\s+", " ", text or "")
    text = text.strip(" \t\r\n，,。.；;：:！!？?、-—~～|/\\")
    return text.strip()


def _detect_type(text: str) -> tuple[str | None, int, int]:
    """返回 (标准类型, 起始位置, 结束位置)。"""
    candidates: list[tuple[int, int, str]] = []
    for alias, canonical in _TYPE_ALIAS_SORTED:
        idx = text.find(alias)
        if idx >= 0:
            candidates.append((idx, -len(alias), canonical))

    match = ASK_LEAVE_RE.search(text)
    if match:
        candidates.append((match.start(), -(match.end() - match.start()), "请假"))

    if not candidates:
        return None, -1, -1
    start, neg_len, canonical = min(candidates)
    return canonical, start, start - neg_len


def _is_structured(text: str) -> bool:
    lowered = text.strip().lower()
    return any(lowered.startswith(p.lower()) for p in STRUCTURED_PREFIXES)


def looks_like_application(text: str) -> bool:
    """判断文本是否像一条审批申请（用于唤醒过滤，尽量避免误触发闲聊）。"""
    raw = (text or "").strip()
    if not raw or len(raw) > 500:
        return False
    if _is_structured(raw):
        return True
    if ASK_LEAVE_RE.search(raw):
        return True

    if not any(kw in raw for kw in TYPE_ALIASES):
        return False
    if any(marker in raw for marker in APPLY_MARKERS):
        return True
    if DAY_RE.search(raw) or HOUR_RE.search(raw) or HALF_DAY_RE.search(raw):
        return True
    if HOUR_RE.search(raw) or HALF_HOUR_RE.search(raw) or AMOUNT_UNIT_RE.search(raw):
        return True
    if AMOUNT_PREFIX_RE.search(raw) or AMOUNT_SYMBOL_RE.search(raw):
        return True
    return False


@dataclass
class Application:
    raw: str
    type: str
    reason: str = "未填写事由"
    days: float | None = None
    hours: float | None = None
    amount: float | None = None
    time_desc: str = ""
    structured: bool = False

    @property
    def effective_days(self) -> float | None:
        return self.days


def _extract_date(text: str) -> tuple[str, list[tuple[int, int]]]:
    """提取时间描述，返回 (time_desc, 需要从原文剔除的 span 列表)。"""
    spans: list[tuple[int, int]] = []

    match = DATE_YMD_RE.search(text)
    if match:
        try:
            year, month, day = int(match.group(1)), int(match.group(2)), int(match.group(3))
            desc = f"{year:04d}-{month:02d}-{day:02d}"
        except ValueError:
            desc = match.group(0)
        spans.append(match.span())
        return desc, spans

    match = DATE_MD_RE.search(text)
    if match:
        try:
            month, day = int(match.group(1)), int(match.group(2))
            desc = f"{datetime.now().year:04d}-{month:02d}-{day:02d}"
        except ValueError:
            desc = match.group(0)
        spans.append(match.span())
        return desc, spans

    for word, offset in RELATIVE_DAYS:
        idx = text.find(word)
        if idx >= 0:
            target = (datetime.now() + timedelta(days=offset)).strftime("%Y-%m-%d")
            spans.append((idx, idx + len(word)))
            return f"{word}({target})", spans

    match = DATE_DAY_RE.search(text)
    if match:
        try:
            day = int(match.group(1))
            now = datetime.now()
            target = datetime(now.year, now.month, day)
            if target.date() < now.date():
                target = target.replace(month=now.month % 12 + 1)
            desc = target.strftime("%Y-%m-%d")
        except ValueError:
            desc = match.group(0)
        spans.append(match.span())
        return desc, spans

    return "", spans


def _extract_reason(text: str, leftover: str) -> str:
    match = REASON_EXPLICIT_RE.search(text)
    if match:
        reason = _clean_fragment(match.group(1))
        if reason:
            return reason

    cleaned = leftover
    for prefix in STRUCTURED_PREFIXES:
        cleaned = re.sub(re.escape(prefix), " ", cleaned, flags=re.IGNORECASE)
    for alias in TYPE_ALIASES:
        cleaned = cleaned.replace(alias, " ")
    for marker in APPLY_MARKERS:
        cleaned = cleaned.replace(marker, " ")
    cleaned = re.sub(r"[，,。.；;：:！!？?、\s]+", " ", cleaned)
    tokens = [
        tok
        for tok in cleaned.split()
        if tok and tok not in NOISE_WORDS and not tok.startswith("@")
    ]
    cleaned = " ".join(tokens)
    cleaned = _clean_fragment(cleaned)
    return cleaned or "未填写事由"


def parse_application(text: str) -> Application | None:
    """解析自然语言或结构化（#审批 ...）审批申请。"""
    raw = (text or "").strip()
    if not raw:
        return None

    structured = _is_structured(raw)
    body = raw
    if structured:
        for prefix in STRUCTURED_PREFIXES:
            if body.lower().startswith(prefix.lower()):
                body = body[len(prefix):].strip()
                break
        if not body:
            return None

    type_name, type_start, type_end = _detect_type(body)
    spans: list[tuple[int, int]] = []
    if type_start >= 0:
        spans.append((type_start, type_end))
    if type_name is None:
        if not structured:
            return None
        type_name = "其他"

    days: float | None = None
    hours: float | None = None
    amount: float | None = None

    match = DAY_RE.search(body)
    if match:
        days = _to_number(match.group(1))
        spans.append(match.span())
    else:
        if HALF_DAY_RE.search(body):
            days = 0.5
            spans.append(HALF_DAY_RE.search(body).span())  # type: ignore[union-attr]

    match = HOUR_RE.search(body)
    if match:
        hours = _to_number(match.group(1))
        spans.append(match.span())
    elif HALF_HOUR_RE.search(body):
        hours = 0.5
        spans.append(HALF_HOUR_RE.search(body).span())  # type: ignore[union-attr]

    if hours is not None:
        converted = round(hours / 8.0, 2)
        if days is None:
            days = converted
        else:
            days = round(days + converted, 2)

    for pattern in (AMOUNT_PREFIX_RE, AMOUNT_UNIT_RE, AMOUNT_SYMBOL_RE):
        match = pattern.search(body)
        if match:
            amount = _to_number(match.group(1)) if match.groups() else None
            if amount is not None:
                spans.append(match.span())
                break

    time_desc, date_spans = _extract_date(body)
    spans.extend(date_spans)

    leftover = _strip_spans(body, spans)
    reason = _extract_reason(body, leftover)

    application = Application(
        raw=raw,
        type=type_name,
        reason=reason,
        days=days,
        hours=hours,
        amount=amount,
        time_desc=time_desc,
        structured=structured,
    )
    return application


def evaluate_rules(
    app: Application,
    auto_types: list[str],
    max_leave_days: float,
    max_amount: float,
) -> tuple[bool, str]:
    """规则判定：返回 (是否自动通过, 说明)。"""
    auto_set = {_canonical_type(t) for t in auto_types if str(t).strip()}
    if not auto_set:
        return False, "未配置自动通过类型，需人工审核"
    if app.type not in auto_set:
        shown = "、".join(sorted(auto_set))
        return False, f"类型「{app.type}」不在自动通过范围（{shown}）"

    if app.type in ("请假", "外出", "加班"):
        if app.days is None:
            return False, "未提供请假/外出天数或时长，需人工确认"
        if app.days > max_leave_days:
            return False, f"天数 {_fmt_number(app.days)} 超过自动通过上限 {_fmt_number(max_leave_days)} 天"

    if app.type == "报销":
        if app.amount is None:
            return False, "未提供报销金额，需人工确认"
    if app.amount is not None and app.amount > max_amount:
        return False, f"金额 {_fmt_number(app.amount)} 元超过自动通过上限 {_fmt_number(max_amount)} 元"

    return True, "符合自动通过规则"


def _format_duration(app: Application) -> str:
    parts: list[str] = []
    if app.days is not None:
        parts.append(f"{_fmt_number(app.days)} 天")
    if app.hours is not None:
        parts.append(f"{_fmt_number(app.hours)} 小时")
    return " / ".join(parts) if parts else "-"


def _format_application_lines(app: Application, applicant_id: str, applicant_name: str) -> list[str]:
    applicant = applicant_name or applicant_id or "未知"
    if applicant_id:
        applicant = f"{applicant}（ID: {applicant_id}）"
    lines = [
        f"申请人：{applicant}",
        f"类型：{app.type}",
        f"事由：{app.reason}",
        f"时长：{_format_duration(app)}",
        f"金额：{_fmt_number(app.amount) + ' 元' if app.amount is not None else '-'}",
    ]
    if app.time_desc:
        lines.append(f"时间：{app.time_desc}")
    return lines


def _new_application_id() -> str:
    return "AP" + datetime.now().strftime("%Y%m%d%H%M%S") + f"{random.randint(0, 0xFFFF):04X}"


def _data_root() -> Path:
    if get_astrbot_data_path is not None:
        try:
            return Path(get_astrbot_data_path())
        except Exception:
            pass
    try:
        return StarTools.get_data_dir(PLUGIN_NAME).parent.parent
    except Exception:
        return Path("data")


# --------------------------------------------------------------------------- #
# 消息唤醒过滤器：只在审批群 / 私聊且消息像申请时才激活处理器
# --------------------------------------------------------------------------- #


class ApprovalMessageFilter(CustomFilter):
    """自定义过滤器：把审批群/私聊中的“像审批申请”的消息激活出来。

    群聊中默认消息不会唤醒插件，这里通过 CustomFilter 让满足条件的消息
    进入本插件的处理器；普通聊天、其它群的消息直接返回 False（安静忽略）。
    """

    def filter(self, event: AstrMessageEvent, cfg: Any) -> bool:  # noqa: D102
        try:
            plugin = _PLUGIN_INSTANCE
            if plugin is None:
                return False
            text = event.get_message_str()
            if not looks_like_application(text):
                return False
            if event.is_private_chat():
                return True
            group_id = str(event.get_group_id() or "")
            return plugin.is_approval_group(group_id)
        except Exception as exc:  # filter 内绝不能抛异常
            logger.debug(f"[{PLUGIN_NAME}] 审批唤醒过滤器异常，已忽略：{exc}")
            return False


# --------------------------------------------------------------------------- #
# 插件主体
# --------------------------------------------------------------------------- #


class AIApprovalPlugin(Star):
    """AI 审批插件。"""

    def __init__(self, context: Context, config: dict | None = None) -> None:
        super().__init__(context, config)
        self.config = config or {}
        self._audit_lock = asyncio.Lock()
        self._audit_path_cache: Path | None = None
        self._audit_init_error: str | None = None
        global _PLUGIN_INSTANCE
        _PLUGIN_INSTANCE = self
        logger.info(f"[{PLUGIN_NAME}] 插件实例已创建")

    # -- 生命周期 ---------------------------------------------------------- #

    async def initialize(self) -> None:  # noqa: D102
        global _PLUGIN_INSTANCE
        _PLUGIN_INSTANCE = self
        try:
            path = self._get_audit_path()
            path.parent.mkdir(parents=True, exist_ok=True)
            logger.info(f"[{PLUGIN_NAME}] 审计日志路径：{path}")
        except Exception as exc:
            self._audit_init_error = str(exc)
            logger.error(f"[{PLUGIN_NAME}] 审计日志目录初始化失败：{exc}")
        logger.info(
            f"[{PLUGIN_NAME}] 已加载：自动通过类型={self._auto_types()}，"
            f"天数上限={self._max_leave_days()}，金额上限={self._max_amount()}，"
            f"use_llm={self._use_llm()}"
        )

    async def terminate(self) -> None:  # noqa: D102
        global _PLUGIN_INSTANCE
        if _PLUGIN_INSTANCE is self:
            _PLUGIN_INSTANCE = None
        logger.info(f"[{PLUGIN_NAME}] 插件已卸载")

    # -- 配置读取 ---------------------------------------------------------- #

    def _cfg(self, key: str, default: Any = None) -> Any:
        try:
            if self.config is None:
                return default
            return self.config.get(key, default)
        except Exception:
            return default

    def _auto_types(self) -> list[str]:
        values = _as_str_list(self._cfg("auto_approve_types", DEFAULT_AUTO_TYPES))
        if not values:
            values = list(DEFAULT_AUTO_TYPES)
        return [_canonical_type(v) for v in values]

    def _max_leave_days(self) -> float:
        return _as_float(self._cfg("auto_approve_max_leave_days", 2), 2.0)

    def _max_amount(self) -> float:
        return _as_float(self._cfg("auto_approve_max_amount", 500), 500.0)

    def _use_llm(self) -> bool:
        return _as_bool(self._cfg("use_llm", False), False)

    def _approval_group_ids(self) -> list[str]:
        return _as_str_list(self._cfg("approval_group_ids", []))

    def _admin_group_ids(self) -> list[str]:
        return _as_str_list(self._cfg("admin_group_ids", []))

    def _admin_user_ids(self) -> list[str]:
        return _as_str_list(self._cfg("admin_user_ids", []))

    def is_approval_group(self, group_id: str) -> bool:
        group_id = str(group_id or "").strip()
        if not group_id:
            return False
        return group_id in self._approval_group_ids()

    # -- 审计 -------------------------------------------------------------- #

    def _get_audit_path(self) -> Path:
        if self._audit_path_cache is not None:
            return self._audit_path_cache
        configured = str(self._cfg("audit_log_path", "") or "").strip()
        if configured:
            path = Path(os.path.expanduser(configured))
            if not path.is_absolute():
                path = _data_root() / path
        else:
            path = _data_root() / AUDIT_DIR_NAME / "audit.jsonl"
        self._audit_path_cache = path
        return path

    def _append_audit_line(self, record: dict[str, Any]) -> None:
        path = self._get_audit_path()
        path.parent.mkdir(parents=True, exist_ok=True)
        line = json.dumps(record, ensure_ascii=False)
        with path.open("a", encoding="utf-8") as fp:
            fp.write(line + "\n")

    async def _write_audit(self, record: dict[str, Any]) -> None:
        try:
            async with self._audit_lock:
                await asyncio.to_thread(self._append_audit_line, record)
        except Exception as exc:
            logger.error(f"[{PLUGIN_NAME}] 写入审计日志失败：{exc}")

    def _read_recent_audit(self, limit: int = 10) -> list[dict[str, Any]]:
        path = self._get_audit_path()
        if not path.exists():
            return []
        block = max(8192, limit * 1024)
        try:
            with path.open("rb") as fp:
                fp.seek(0, os.SEEK_END)
                size = fp.tell()
                fp.seek(max(0, size - block))
                raw = fp.read().decode("utf-8", errors="replace")
        except Exception as exc:
            logger.error(f"[{PLUGIN_NAME}] 读取审计日志失败：{exc}")
            return []
        lines = [line for line in raw.splitlines() if line.strip()]
        if size > block and lines:
            lines = lines[1:]  # 丢弃可能被截断的首行
        records: list[dict[str, Any]] = []
        for line in lines[-limit:]:
            try:
                records.append(json.loads(line))
            except Exception:
                continue
        return records

    # -- 权限 -------------------------------------------------------------- #

    def _sender_is_configured_admin(self, event: AstrMessageEvent) -> bool:
        sender_id = str(event.get_sender_id() or "")
        if not sender_id:
            return False
        if sender_id in self._admin_user_ids():
            return True
        try:
            return bool(event.is_admin())
        except Exception:
            return False

    async def _sender_is_admin(self, event: AstrMessageEvent) -> bool:
        if self._sender_is_configured_admin(event):
            return True
        sender_id = str(event.get_sender_id() or "")
        if not sender_id:
            return False
        try:
            cfg = self.context.get_config()
            if inspect.isawaitable(cfg):
                cfg = await cfg
            admins = _as_str_list(cfg.get("admins_id", []) if hasattr(cfg, "get") else [])
            return sender_id in admins
        except Exception as exc:
            logger.debug(f"[{PLUGIN_NAME}] 读取全局管理员列表失败：{exc}")
            return False

    # -- 发送 -------------------------------------------------------------- #

    @staticmethod
    def _platform_id(event: AstrMessageEvent) -> str:
        try:
            platform_id = event.get_platform_id()
            if platform_id:
                return str(platform_id)
        except Exception:
            pass
        try:
            return str(event.session.platform_name)
        except Exception:
            return ""

    async def _send_to_session(self, session: str, text: str) -> bool:
        try:
            return bool(
                await self.context.send_message(session, MessageChain([Plain(text)]))
            )
        except Exception as exc:
            logger.warning(f"[{PLUGIN_NAME}] 发送消息到 {session} 失败：{exc}")
            return False

    async def _notify_admins(
        self,
        event: AstrMessageEvent,
        app_id: str,
        app: Application,
        decision_reason: str,
    ) -> tuple[bool, list[str]]:
        """向管理员群（或管理员私聊）发送提醒，返回 (是否送达, 失败目标)。"""
        platform_id = self._platform_id(event)
        lines = [
            "🔔 新的审批申请待处理",
            f"申请ID：{app_id}",
            *_format_application_lines(app, str(event.get_sender_id() or ""), event.get_sender_name() or ""),
            f"规则判定：{decision_reason}",
            "请管理员按内部流程处理（可用 /ai审批记录 查看最近记录）。",
        ]
        text = "\n".join(lines)

        failures: list[str] = []
        sent = False
        for group_id in self._admin_group_ids():
            session = f"{platform_id}:GroupMessage:{group_id}"
            if await self._send_to_session(session, text):
                sent = True
            else:
                failures.append(session)

        if not sent:
            for user_id in self._admin_user_ids():
                session = f"{platform_id}:FriendMessage:{user_id}"
                if await self._send_to_session(session, text):
                    sent = True
                else:
                    failures.append(session)

        if not sent:
            logger.warning(
                f"[{PLUGIN_NAME}] 管理员提醒未送达（申请 {app_id}）："
                f"admin_group_ids={self._admin_group_ids()}，"
                f"admin_user_ids={self._admin_user_ids()}"
            )
        return sent, failures

    # -- LLM 合理性审查（可选） -------------------------------------------- #

    async def _call_llm(
        self,
        event: AstrMessageEvent,
        prompt: str,
        system_prompt: str,
    ) -> str:
        provider_id = str(self._cfg("llm_provider_id", "") or "").strip()
        if provider_id:
            response = await self.context.llm_generate(
                chat_provider_id=provider_id,
                prompt=prompt,
                system_prompt=system_prompt,
            )
            return str(getattr(response, "completion_text", "") or "")

        provider = None
        getter = getattr(self.context, "get_using_provider_async", None)
        if callable(getter):
            provider = await getter(event.unified_msg_origin)
        if provider is None:
            fallback = getattr(self.context, "get_using_provider", None)
            if callable(fallback):
                provider = fallback(event.unified_msg_origin)
                if inspect.isawaitable(provider):
                    provider = await provider
        if provider is None:
            raise RuntimeError("没有可用的 LLM Provider")

        response = await provider.text_chat(
            prompt=prompt,
            system_prompt=system_prompt,
            contexts=[],
        )
        return str(getattr(response, "completion_text", "") or "")

    async def _llm_review(
        self,
        event: AstrMessageEvent,
        app: Application,
    ) -> tuple[str, str]:
        """返回 (verdict, reason)，verdict ∈ {approve, reject, unavailable}。"""
        system_prompt = (
            "你是企业审批合规审查助手。请根据申请信息判断该申请是否合理，"
            "是否存在明显异常（例如事由与类型不符、金额异常、时间冲突等）。"
            '只输出 JSON，格式：{"decision": "approve" 或 "reject", "reason": "一句话理由"}。'
        )
        prompt_lines = [
            f"申请人：{event.get_sender_name() or event.get_sender_id()}",
            f"申请类型：{app.type}",
            f"申请事由：{app.reason}",
            f"申请时长：{_format_duration(app)}",
            f"申请金额：{_fmt_number(app.amount) if app.amount is not None else '-'}",
            f"申请时间：{app.time_desc or '-'}",
            "请给出审查意见。",
        ]
        prompt = "\n".join(prompt_lines)

        try:
            text = await asyncio.wait_for(
                self._call_llm(event, prompt, system_prompt),
                timeout=25,
            )
        except Exception as exc:
            logger.warning(f"[{PLUGIN_NAME}] LLM 合理性审查不可用，回退规则判定：{exc}")
            return "unavailable", f"LLM 不可用：{exc}"

        data = self._parse_llm_json(text)
        if not data:
            return "unavailable", "LLM 输出无法解析"

        decision = str(data.get("decision", "")).strip().lower()
        reason = str(data.get("reason", "")).strip() or "（未提供理由）"
        if decision in ("approve", "approved", "pass", "通过", "同意"):
            return "approve", reason
        if decision in ("reject", "rejected", "deny", "驳回", "拒绝"):
            return "reject", reason
        return "unavailable", f"LLM 输出无法识别：{decision or text[:60]}"

    @staticmethod
    def _parse_llm_json(text: str) -> dict[str, Any] | None:
        cleaned = (text or "").strip()
        if cleaned.startswith("```"):
            cleaned = re.sub(r"^```[a-zA-Z]*\s*", "", cleaned)
            cleaned = re.sub(r"\s*```$", "", cleaned)
        try:
            data = json.loads(cleaned)
            return data if isinstance(data, dict) else None
        except Exception:
            pass
        match = re.search(r"\{.*\}", cleaned, re.DOTALL)
        if match:
            try:
                data = json.loads(match.group(0))
                return data if isinstance(data, dict) else None
            except Exception:
                return None
        return None

    # -- 主流程 ------------------------------------------------------------ #

    async def _process_application(self, event: AstrMessageEvent, app: Application) -> str:
        app_id = _new_application_id()
        applicant_id = str(event.get_sender_id() or "")
        applicant_name = str(event.get_sender_name() or "")

        passed, rule_reason = evaluate_rules(
            app,
            self._auto_types(),
            self._max_leave_days(),
            self._max_amount(),
        )

        applicant_is_admin = self._sender_is_configured_admin(event)
        if applicant_is_admin and passed:
            passed = False
            rule_reason = "申请人为管理员，按配置强制人工审核"

        decision: str
        decider: str
        decision_reason: str

        if passed and self._use_llm():
            verdict, llm_reason = await self._llm_review(event, app)
            if verdict == "approve":
                decision, decider = "auto_approved", "rule_engine+llm"
                decision_reason = f"{rule_reason}；AI 合理性审查通过：{llm_reason}"
            elif verdict == "reject":
                decision, decider = "rejected", "llm_review"
                decision_reason = f"{rule_reason}；AI 合理性审查建议人工复核：{llm_reason}"
                rule_reason = decision_reason
            else:
                decision, decider = "auto_approved", "rule_engine"
                decision_reason = f"{rule_reason}（AI 审查不可用，已回退规则判定：{llm_reason}）"
        elif passed:
            decision, decider = "auto_approved", "rule_engine"
            decision_reason = rule_reason
        else:
            decision, decider = "pending", "admin_group"
            decision_reason = rule_reason

        notified = False
        if decision != "auto_approved":
            notified, _failures = await self._notify_admins(
                event,
                app_id,
                app,
                decision_reason,
            )
            if not notified:
                decision_reason += "（管理员提醒未送达：请检查 admin_group_ids / admin_user_ids 配置）"

        record = {
            "ts": datetime.now().astimezone().isoformat(timespec="seconds"),
            "applicant_id": applicant_id,
            "applicant_name": applicant_name,
            "type": app.type,
            "reason": app.reason,
            "days": app.days,
            "amount": app.amount,
            "decision": decision,
            "decider": decider,
            "reason_text": decision_reason,
        }
        await self._write_audit(record)

        lines = [
            "✅ 审批已自动通过" if decision == "auto_approved" else "📩 已提交待管理员审批",
            "────────────────",
            f"申请ID：{app_id}",
            *_format_application_lines(app, applicant_id, applicant_name),
            f"规则判定：{decision_reason}",
        ]
        if decision == "auto_approved":
            lines.append("备注：该申请由规则引擎自动通过，已写入审计日志。")
        elif decision == "rejected":
            lines.append("备注：AI 审查建议人工复核，已通知管理员，请等待处理结果。")
        else:
            lines.append("备注：管理员将尽快处理，请留意通知。")
        return "\n".join(lines)

    # -- 消息处理器 -------------------------------------------------------- #

    @filter.custom_filter(ApprovalMessageFilter)
    @filter.event_message_type(EventMessageType.ALL)
    async def handle_application(self, event: AstrMessageEvent):  # noqa: D102
        """处理审批群 / 私聊中的审批申请消息。"""
        try:
            text = event.get_message_str()
            app = parse_application(text)
            if app is None:
                if event.is_at_or_wake_command:
                    yield event.plain_result(self._format_hint())
                else:
                    event.stop_event()
                return
            reply = await self._process_application(event, app)
            yield event.plain_result(reply)
        except Exception as exc:
            logger.error(f"[{PLUGIN_NAME}] 处理审批申请失败：{exc}", exc_info=True)
            try:
                if event.is_at_or_wake_command:
                    yield event.plain_result(
                        "处理审批申请时出现异常，请稍后重试或联系管理员。"
                    )
                else:
                    event.stop_event()
            except Exception:
                pass

    @filter.command("ai审批帮助")
    async def cmd_help(self, event: AstrMessageEvent):  # noqa: D102
        yield event.plain_result(self._format_help())

    @filter.command("ai审批规则")
    async def cmd_rules(self, event: AstrMessageEvent):  # noqa: D102
        yield event.plain_result(self._format_rules())

    @filter.command("ai审批记录")
    async def cmd_records(self, event: AstrMessageEvent):  # noqa: D102
        try:
            if not await self._sender_is_admin(event):
                yield event.plain_result("🔒 该命令仅管理员可用。")
                return
            records = await asyncio.to_thread(self._read_recent_audit, 10)
        except Exception as exc:
            logger.error(f"[{PLUGIN_NAME}] 读取审批记录失败：{exc}")
            yield event.plain_result("读取审计记录失败，请查看 AstrBot 日志。")
            return

        if not records:
            yield event.plain_result("📭 暂无审批记录。")
            return

        lines = ["📚 最近 10 条审批记录", "────────────────"]
        decision_map = {
            "auto_approved": "自动通过",
            "pending": "待审批",
            "rejected": "建议人工复核",
        }
        for record in records:
            ts = str(record.get("ts", ""))[:19].replace("T", " ")
            name = record.get("applicant_name") or record.get("applicant_id") or "未知"
            type_name = record.get("type", "-")
            decision = decision_map.get(str(record.get("decision", "")), str(record.get("decision", "-")))
            reason = str(record.get("reason", ""))[:20]
            lines.append(f"{ts} | {name} | {type_name} | {decision} | {reason}")
        yield event.plain_result("\n".join(lines))

    # -- 文本模板 ---------------------------------------------------------- #

    def _format_hint(self) -> str:
        return (
            "这条消息看起来像审批申请，但没有解析出发起类型。\n"
            "可以参考以下格式提交：\n"
            "· 申请请假 3 天，事由：家里有事\n"
            "· 报销 1200 元，打车费\n"
            "· 申请 2026-11-20 外出 2 小时\n"
            "· #审批 请假 3 天 家里有事\n"
            "发送 /ai审批帮助 查看完整说明。"
        )

    def _format_help(self) -> str:
        rule_lines = self._rule_summary_lines()
        return "\n".join(
            [
                "📋 AI 审批插件使用说明",
                "────────────────",
                "【提交申请】在审批群或私聊中直接发送（无需唤醒词）：",
                "1. 申请请假 3 天，事由：家里有事",
                "2. 报销 1200 元，打车费",
                "3. 申请 2026-11-20 外出 2 小时",
                "4. #审批 请假 3 天 家里有事",
                "",
                "【当前自动通过规则】",
                *rule_lines,
                "",
                "【命令】",
                "/ai审批帮助 - 显示本帮助",
                "/ai审批规则 - 查看当前规则",
                "/ai审批记录 - 查看最近 10 条审计记录（仅管理员）",
                "",
                "不满足自动通过条件的申请会转交管理员群提醒；每笔申请都会写入审计日志。",
            ]
        )

    def _rule_summary_lines(self) -> list[str]:
        auto_types = "、".join(self._auto_types()) or "（未配置）"
        return [
            f"· 可自动通过的类型：{auto_types}",
            f"· 单次天数上限：{_fmt_number(self._max_leave_days())} 天",
            f"· 单次金额上限：{_fmt_number(self._max_amount())} 元",
            f"· LLM 合理性审查：{'开启' if self._use_llm() else '关闭'}",
            "· 管理员提交的申请：强制人工审核",
        ]

    def _format_rules(self) -> str:
        approval_groups = "、".join(self._approval_group_ids()) or "（未配置，群聊不触发）"
        admin_groups = "、".join(self._admin_group_ids()) or "（未配置，改为私聊管理员）"
        admin_users = "、".join(self._admin_user_ids()) or "（未配置）"
        return "\n".join(
            [
                "⚙️ 当前审批规则",
                "────────────────",
                f"· 审批群：{approval_groups}",
                f"· 管理员群：{admin_groups}",
                f"· 管理员用户：{admin_users}",
                *self._rule_summary_lines(),
                f"· 审计日志：{self._get_audit_path()}",
            ]
        )
