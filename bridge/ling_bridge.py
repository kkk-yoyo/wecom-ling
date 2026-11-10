#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""微灵 (WeComSever) ⇄ OneBot v11 桥接层.

- 接收微灵事件（HTTP CallbackUrl / 可选 TCP 9090），转换成 OneBot v11 事件；
- 以反向 WebSocket 客户端身份连到 OneBot 实现（AstrBot aiocqhttp / NapCat / LLOneBot 等），
  接收并应答 send_* / get_* 等 action。

依赖：Python 3.9+，aiohttp
用法：
    pip install aiohttp
    python ling_bridge.py --config config.json
    python ling_bridge.py --selftest        # 不需要微灵，打印转换结果自检
"""

from __future__ import annotations

import argparse
import asyncio
import codecs
import json
import logging
import signal
import sys
import time
import traceback
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

try:
    import aiohttp
    from aiohttp import web
except ImportError:  # pragma: no cover
    print("需要 aiohttp：pip install aiohttp", file=sys.stderr)
    raise

VERSION = "0.1.0"
LOG = logging.getLogger("ling-bridge")

# ---------------------------------------------------------------------------
# 微灵事件类型
# ---------------------------------------------------------------------------
EV_CLIENT_ONLINE = 1000
EV_CLIENT_OFFLINE = -1000
EV_READY = 11024
EV_LOGIN = 11026
EV_LOGIN_CORP = 11179
EV_LOGOUT = 11027
EV_QRCODE = 11028

MSG_TEXT = 11041
MSG_IMAGE = 11042
MSG_LINK = 11047
MSG_GIF = 11048
MSG_FILE = 11045
MSG_VIDEO = 11043
MSG_VOICE = 11044
MSG_POSITION = 11046
MSG_CARD = 11050
MSG_MINIAPP = 11066
MSG_VIDEO_ACCOUNT = 11124
MSG_REDPACKET = 11049
MSG_RICH = 11068
MSG_RECALL = 11123

MESSAGE_TYPES = {
    MSG_TEXT: ("text", None),
    MSG_IMAGE: ("image", "[图片]"),
    MSG_LINK: ("link", "[链接]"),
    MSG_GIF: ("gif", "[动图]"),
    MSG_FILE: ("file", "[文件]"),
    MSG_VIDEO: ("video", "[视频]"),
    MSG_VOICE: ("voice", "[语音]"),
    MSG_POSITION: ("position", "[位置]"),
    MSG_CARD: ("card", "[名片]"),
    MSG_MINIAPP: ("miniapp", "[小程序]"),
    MSG_VIDEO_ACCOUNT: ("videoaccount", "[视频号]"),
    MSG_REDPACKET: ("redpacket", "[红包]"),
    MSG_RICH: ("rich", "[图文]"),
    MSG_RECALL: ("recall", None),
}

# 微灵指令
CMD_SEND_TEXT = 11029
CMD_SEND_AT = 11069

DEFAULT_CONFIG: Dict[str, Any] = {
    "http": {"host": "127.0.0.1", "port": 8899},
    "ling": {
        "api": "http://127.0.0.1:8060",
        "client": 1,
        "tcp": "",  # 例如 "127.0.0.1:9090"；留空则不连
    },
    "onebot": {
        "ws": "ws://127.0.0.1:6200/ws",
        "access_token": "",
        "self_id": "",
    },
    "filter": {
        "ignore_self": True,
        "ignore_recall": True,
    },
}


def deep_merge(base: Dict[str, Any], override: Dict[str, Any]) -> Dict[str, Any]:
    for key, value in override.items():
        if isinstance(value, dict) and isinstance(base.get(key), dict):
            deep_merge(base[key], value)
        else:
            base[key] = value
    return base


def load_config(path: Optional[str]) -> Dict[str, Any]:
    cfg = json.loads(json.dumps(DEFAULT_CONFIG))
    if path:
        p = Path(path)
        if p.exists():
            user_cfg = json.loads(p.read_text(encoding="utf-8-sig"))
            deep_merge(cfg, user_cfg)
            LOG.info("已加载配置: %s", p.resolve())
        else:
            LOG.warning("配置文件不存在，使用默认配置: %s", p)
    return cfg


def to_int(value: Any) -> Any:
    if isinstance(value, bool):
        return value
    if isinstance(value, int):
        return value
    if isinstance(value, str) and value.lstrip("-").isdigit():
        try:
            return int(value)
        except ValueError:
            return value
    return value


def now_ts() -> int:
    return int(time.time())


def parse_conv(conversation_id: str) -> Tuple[str, str]:
    """把微灵 conversation_id 拆成 (kind, id)。

    群聊: "R:<room_id>"          -> ("group", "<room_id>")
    私聊: "S:<peer>_<self>"      -> ("private", "<peer>")
    """
    cid = (conversation_id or "").strip()
    if cid.startswith("R:"):
        return "group", cid[2:]
    if cid.startswith("S:"):
        body = cid[2:]
        peer = body.split("_", 1)[0]
        return "private", peer
    return "private", cid


def build_conv_id(kind: str, peer: str, self_id: str) -> str:
    """构造微灵 conversation_id。"""
    if kind == "group":
        return "R:" + str(peer)
    return "S:%s_%s" % (peer, self_id)


class ActionFailed(Exception):
    """OneBot action 执行失败。"""


class Bridge:
    def __init__(self, cfg: Dict[str, Any]) -> None:
        self.cfg = cfg
        self.http_cfg = cfg["http"]
        self.ling_cfg = cfg["ling"]
        self.ob_cfg = cfg["onebot"]
        self.filter_cfg = cfg["filter"]

        self.client_id: int = int(self.ling_cfg.get("client") or 1)
        self.self_id: str = str(self.ob_cfg.get("self_id") or "")
        self.self_name: str = ""

        self.session: Optional[aiohttp.ClientSession] = None
        self.ws: Optional[aiohttp.ClientWebSocketResponse] = None
        self.ws_task: Optional[asyncio.Task] = None
        self.tcp_task: Optional[asyncio.Task] = None
        self.stopping = False
        self.started_at = time.time()

        self.stats = {
            "events": 0,
            "messages": 0,
            "actions": 0,
            "sent": 0,
            "errors": 0,
        }
        self._seen: Dict[str, float] = {}

    # ------------------------------------------------------------------ 启动
    async def start(self) -> None:
        self.session = aiohttp.ClientSession()

        app = web.Application()
        app.router.add_post("/{tail:.*}", self.handle_callback)
        app.router.add_get("/status", self.handle_status)
        app.router.add_get("/", self.handle_status)

        runner = web.AppRunner(app)
        await runner.setup()
        site = web.TCPSite(runner, self.http_cfg["host"], int(self.http_cfg["port"]))
        await site.start()
        LOG.info(
            "事件回调服务已启动: http://%s:%s/（把这个地址填进微灵 config.ini 的 CallbackUrl）",
            self.http_cfg["host"],
            self.http_cfg["port"],
        )

        if self.self_id:
            LOG.info("配置里已有 self_id=%s，直接建立 OneBot 连接", self.self_id)
            self.ensure_ws_task()

        tcp_addr = str(self.ling_cfg.get("tcp") or "").strip()
        if tcp_addr:
            self.tcp_task = asyncio.create_task(self.tcp_loop())

        LOG.info("桥接层就绪 (version=%s)", VERSION)
        await asyncio.Event().wait()

    # ------------------------------------------------------------- 事件接收
    async def handle_callback(self, request: "web.Request") -> "web.Response":
        raw = await request.read()
        if not raw:
            return web.Response(status=400, text="empty body")
        text = raw.decode("utf-8", "replace")
        payload = self.decode_event(text)
        if payload is None:
            LOG.warning("无法解析回调: %s", text[:300])
            return web.Response(status=400, text="invalid json")
        self.dispatch(payload)
        return web.Response(text="ok")

    @staticmethod
    def decode_event(text: str) -> Optional[Dict[str, Any]]:
        """兼容微灵回调的“二次编码字符串”形式。"""
        text = (text or "").strip()
        if not text:
            return None
        try:
            payload: Any = json.loads(text)
        except json.JSONDecodeError:
            return None
        for _ in range(2):
            if isinstance(payload, str):
                try:
                    payload = json.loads(payload)
                except json.JSONDecodeError:
                    return None
            else:
                break
        return payload if isinstance(payload, dict) else None

    async def handle_status(self, request: "web.Request") -> "web.Response":
        data = {
            "version": VERSION,
            "uptime": int(time.time() - self.started_at),
            "client_id": self.client_id,
            "self_id": self.self_id,
            "self_name": self.self_name,
            "onebot_ws_connected": self.ws is not None and not self.ws.closed,
            "onebot_ws_url": self.ob_cfg.get("ws"),
            "stats": self.stats,
        }
        return web.json_response(data, dumps=lambda o: json.dumps(o, ensure_ascii=False))

    def dispatch(self, payload: Dict[str, Any]) -> None:
        event_type = payload.get("type")
        data = payload.get("data") or {}
        if not isinstance(data, dict):
            data = {}
        client = to_int(payload.get("client") or self.client_id)
        self.stats["events"] += 1

        try:
            if event_type == EV_CLIENT_ONLINE:
                self.client_id = int(to_int(data.get("clientId") or client))
                LOG.info("微灵客户端上线: client=%s", self.client_id)
            elif event_type == EV_CLIENT_OFFLINE:
                LOG.warning("微灵客户端下线: client=%s", data.get("clientId"))
            elif event_type == EV_READY:
                LOG.info("微灵接口就绪: pid=%s", data.get("pid"))
            elif event_type == EV_QRCODE:
                LOG.info("登录二维码: file=%s key=%s", data.get("file"), data.get("key"))
            elif event_type in (EV_LOGIN, EV_LOGIN_CORP):
                self.on_login(data, client)
            elif event_type == EV_LOGOUT:
                self.on_logout(data)
            elif event_type in MESSAGE_TYPES:
                self.on_message(int(event_type), data, client)
            else:
                LOG.debug("未处理事件: type=%s data=%s", event_type, str(data)[:200])
        except Exception:  # noqa: BLE001
            self.stats["errors"] += 1
            LOG.error("处理事件失败: %s\n%s", payload, traceback.format_exc())

    # ------------------------------------------------------------- 登录状态
    def on_login(self, data: Dict[str, Any], client: int) -> None:
        user_id = str(data.get("user_id") or "")
        if not user_id:
            return
        changed = user_id != self.self_id
        self.self_id = user_id
        self.client_id = int(client)
        self.self_name = str(data.get("username") or data.get("nickname") or "")
        LOG.info(
            "微灵登录成功: user_id=%s username=%s client=%s corp=%s",
            self.self_id,
            self.self_name,
            self.client_id,
            data.get("corp_short_name") or data.get("corp_name") or data.get("corp_id"),
        )
        if changed or self.ws is None or self.ws.closed:
            self.ensure_ws_task(force_restart=True)

    def on_logout(self, data: Dict[str, Any]) -> None:
        LOG.warning("微灵账号退出: user_id=%s，断开 OneBot 连接", data.get("user_id"))
        self.self_id = ""
        self.self_name = ""
        if self.ws is not None and not self.ws.closed:
            asyncio.create_task(self._close_ws())

    async def _close_ws(self) -> None:
        try:
            if self.ws is not None:
                await self.ws.close()
        except Exception:  # noqa: BLE001
            pass

    # ------------------------------------------------------------- 消息事件
    def on_message(self, msg_type: int, data: Dict[str, Any], client: int) -> None:
        if self.filter_cfg.get("ignore_recall") and msg_type == MSG_RECALL:
            return

        conversation_id = str(data.get("conversation_id") or "")
        sender = str(data.get("sender") or "")
        if not conversation_id or not sender:
            LOG.debug("消息缺少 conversation_id/sender: %s", data)
            return

        self_id = self.self_id or str(data.get("receiver") or "")
        if self.filter_cfg.get("ignore_self", True) and self_id and sender == self_id:
            return

        if self._is_duplicate(msg_type, data):
            return

        kind, peer = parse_conv(conversation_id)
        if not peer:
            return

        segments: List[Dict[str, Any]] = []
        seg_kind, placeholder = MESSAGE_TYPES.get(msg_type, ("unknown", "[消息]"))

        if msg_type == MSG_TEXT:
            for at in data.get("at_list") or []:
                if isinstance(at, dict) and at.get("user_id"):
                    segments.append({"type": "at", "data": {"qq": to_int(at["user_id"])}})
            content = data.get("content") or ""
            if content:
                segments.append({"type": "text", "data": {"text": str(content)}})
        elif msg_type == MSG_RECALL:
            recall_id = data.get("server_id") or data.get("local_id")
            event = {
                "time": now_ts(),
                "self_id": to_int(self_id),
                "post_type": "notice",
                "notice_type": "group_recall" if kind == "group" else "friend_recall",
                "user_id": to_int(sender),
                "message_id": to_int(recall_id),
            }
            if kind == "group":
                event["group_id"] = to_int(peer)
            self.emit(event)
            return
        else:
            if placeholder:
                segments.append({"type": "text", "data": {"text": placeholder}})

        if not segments:
            return

        message_id = data.get("server_id") or data.get("local_id") or ""
        event: Dict[str, Any] = {
            "time": int(data.get("send_time") or now_ts()),
            "self_id": to_int(self_id),
            "post_type": "message",
            "message_type": kind,
            "sub_type": "normal" if kind == "group" else "friend",
            "message_id": to_int(message_id),
            "user_id": to_int(sender),
            "message": segments,
            "raw_message": str(data.get("content") or "") or placeholder or "",
            "font": 0,
            "sender": {
                "user_id": to_int(sender),
                "nickname": str(data.get("sender_name") or ""),
                "card": str(data.get("sender_name") or ""),
                "role": "member",
            },
            # 额外保留微灵原始信息，便于下游插件使用
            "ling": {
                "client": client,
                "type": msg_type,
                "conversation_id": conversation_id,
                "data": data,
            },
        }
        if kind == "group":
            event["group_id"] = to_int(peer)
        else:
            event["user_id"] = to_int(peer)
            event["sender"]["user_id"] = to_int(peer)

        self.stats["messages"] += 1
        LOG.info(
            "消息 %s/%s from %s: %s",
            kind,
            peer,
            data.get("sender_name") or sender,
            (str(data.get("content") or ""))[:60] or placeholder,
        )
        self.emit(event)

    def _is_duplicate(self, msg_type: int, data: Dict[str, Any]) -> bool:
        """CallbackUrl 与 TCP 可能同时收到同一条消息，做一次去重。"""
        marker = data.get("server_id") or data.get("local_id")
        if not marker:
            return False
        key = "%s:%s:%s" % (msg_type, marker, data.get("send_time"))
        now = time.time()
        if key in self._seen and now - self._seen[key] < 30:
            return True
        self._seen[key] = now
        if len(self._seen) > 4096:
            cutoff = now - 60
            self._seen = {k: v for k, v in self._seen.items() if v > cutoff}
        return False

    def emit(self, event: Dict[str, Any]) -> None:
        if self.ws is None or self.ws.closed:
            LOG.warning("OneBot 连接未建立，事件被丢弃: %s", event.get("post_type"))
            return
        try:
            self.ws.send_json(event)
        except Exception:  # noqa: BLE001
            self.stats["errors"] += 1
            LOG.warning("发送事件失败: %s", traceback.format_exc())

    # ------------------------------------------------------------- OneBot 连接
    def ensure_ws_task(self, force_restart: bool = False) -> None:
        if force_restart and self.ws_task and not self.ws_task.done():
            self.ws_task.cancel()
        if self.ws_task is None or self.ws_task.done():
            self.ws_task = asyncio.create_task(self.onebot_loop())

    async def onebot_loop(self) -> None:
        url = self.ob_cfg.get("ws")
        if not url:
            LOG.error("onebot.ws 未配置")
            return
        delay = 2
        while not self.stopping:
            self_id = self.self_id
            if not self_id:
                await asyncio.sleep(1)
                continue
            headers = {
                "X-Client-Role": "universal",
                "X-Self-ID": str(self_id),
            }
            token = str(self.ob_cfg.get("access_token") or "")
            if token:
                headers["Authorization"] = "Bearer " + token
            try:
                LOG.info("连接 OneBot 反向 WS: %s (self_id=%s)", url, self_id)
                async with self.session.ws_connect(url, headers=headers, heartbeat=30) as ws:
                    self.ws = ws
                    delay = 2
                    LOG.info("OneBot 连接已建立")
                    async for msg in ws:
                        if msg.type == aiohttp.WSMsgType.TEXT:
                            await self.on_onebot_packet(msg.data)
                        elif msg.type in (aiohttp.WSMsgType.CLOSED, aiohttp.WSMsgType.ERROR):
                            break
            except asyncio.CancelledError:
                break
            except Exception as exc:  # noqa: BLE001
                LOG.warning("OneBot 连接失败: %s", exc)
            finally:
                self.ws = None
            if self.stopping:
                break
            if self.self_id == self_id:
                await asyncio.sleep(delay)
                delay = min(delay * 2, 30)
            else:
                await asyncio.sleep(0.5)

    async def on_onebot_packet(self, text: str) -> None:
        try:
            packet = json.loads(text)
        except json.JSONDecodeError:
            LOG.warning("收到非 JSON OneBot 包: %s", text[:200])
            return
        action = packet.get("action")
        params = packet.get("params") or {}
        echo = packet.get("echo")
        self.stats["actions"] += 1
        try:
            result = await self.call_action(str(action), params)
            response = {"status": "ok", "retcode": 0, "data": result, "echo": echo}
        except ActionFailed as exc:
            response = {
                "status": "failed",
                "retcode": 100,
                "data": None,
                "msg": str(exc),
                "echo": echo,
            }
        except Exception as exc:  # noqa: BLE001
            self.stats["errors"] += 1
            LOG.error("action 处理异常: %s\n%s", action, traceback.format_exc())
            response = {
                "status": "failed",
                "retcode": 120,
                "data": None,
                "msg": str(exc),
                "echo": echo,
            }
        if self.ws is not None and not self.ws.closed:
            try:
                await self.ws.send_json(response)
            except Exception:  # noqa: BLE001
                LOG.warning("回复 OneBot action 失败")

    async def call_action(self, action: str, params: Dict[str, Any]) -> Any:
        if action in ("send_private_msg", "send_group_msg", "send_msg"):
            return await self.action_send(action, params)
        if action == "get_login_info":
            return {"user_id": to_int(self.self_id), "nickname": self.self_name}
        if action == "get_status":
            return {"online": bool(self.ws and not self.ws.closed), "good": True}
        if action == "get_version_info":
            return {"app_name": "ling-bridge", "app_version": VERSION, "protocol_version": "v11"}
        if action == "get_group_info":
            return {
                "group_id": to_int(params.get("group_id")),
                "group_name": "",
                "member_count": 0,
            }
        if action == "get_group_member_info":
            return {
                "user_id": to_int(params.get("user_id")),
                "nickname": "",
                "card": "",
                "role": "member",
            }
        if action == "get_stranger_info":
            return {"user_id": to_int(params.get("user_id")), "nickname": "", "sex": "unknown"}
        if action == "get_group_member_list":
            return []
        if action == "get_group_list":
            return []
        if action == "get_friend_list":
            return []
        if action == "get_msg":
            raise ActionFailed("微灵协议不支持 get_msg")
        if action in ("can_send_image", "can_send_record"):
            return {"yes": True}
        if action in ("set_group_name", "send_like", "delete_msg", "set_restart"):
            raise ActionFailed("不支持该 action: %s" % action)
        raise ActionFailed("未知 action: %s" % action)

    async def action_send(self, action: str, params: Dict[str, Any]) -> Dict[str, Any]:
        message = params.get("message")
        text, at_list = segments_to_text(message)

        if action == "send_group_msg":
            kind = "group"
        elif action == "send_private_msg":
            kind = "private"
        else:
            kind = params.get("message_type") or ("group" if params.get("group_id") else "private")

        if kind == "group":
            peer = params.get("group_id")
            if peer is None:
                raise ActionFailed("send_group_msg 缺少 group_id")
        else:
            peer = params.get("user_id")
            if peer is None:
                raise ActionFailed("send_private_msg 缺少 user_id")

        self_id = self.self_id
        if not self_id:
            raise ActionFailed("微灵尚未登录，无法发送")
        conv = build_conv_id(kind, str(peer), self_id)

        if kind == "group" and at_list:
            cmd = CMD_SEND_AT
            data: Dict[str, Any] = {"conversation_id": conv, "content": text, "at_list": at_list}
        else:
            cmd = CMD_SEND_TEXT
            data = {"conversation_id": conv, "content": text}

        ok = await self.send_ling(cmd, data)
        if not ok:
            raise ActionFailed("微灵发送失败")
        self.stats["sent"] += 1
        return {"message_id": int(time.time() * 1000) % (10**12)}

    async def send_ling(self, cmd_type: int, data: Dict[str, Any]) -> bool:
        url = str(self.ling_cfg.get("api") or "").rstrip("/") + "/api/Client/ExecCommand"
        body = {"client": self.client_id, "type": cmd_type, "data": data}
        try:
            async with self.session.post(url, json=body, timeout=aiohttp.ClientTimeout(total=20)) as resp:
                raw = await resp.text()
        except Exception as exc:  # noqa: BLE001
            LOG.warning("请求微灵接口失败: %s", exc)
            return False
        try:
            result = json.loads(raw)
        except json.JSONDecodeError:
            LOG.warning("微灵接口返回非 JSON: %s", raw[:300])
            return False
        if result.get("code") != 200:
            LOG.warning("微灵接口返回失败: %s", raw[:300])
            return False
        LOG.info("已下发微灵指令 type=%s data=%s", cmd_type, json.dumps(data, ensure_ascii=False)[:200])
        return True

    # ------------------------------------------------------------- TCP 事件
    async def tcp_loop(self) -> None:
        addr = str(self.ling_cfg.get("tcp") or "")
        try:
            host, port = addr.rsplit(":", 1)
            port_i = int(port)
        except ValueError:
            LOG.error("ling.tcp 配置格式错误（应为 host:port）: %s", addr)
            return

        decoder = json.JSONDecoder()
        while not self.stopping:
            writer = None
            try:
                reader, writer = await asyncio.open_connection(host, port_i)
                LOG.info("已连接微灵 TCP 事件通道: %s", addr)
                text_buffer = ""
                utf8 = codecs.getincrementaldecoder("utf-8")("replace")
                while not self.stopping:
                    chunk = await reader.read(65536)
                    if not chunk:
                        break
                    text_buffer += utf8.decode(chunk)
                    while True:
                        idx = text_buffer.find("{")
                        if idx < 0:
                            text_buffer = ""
                            break
                        text_buffer = text_buffer[idx:]
                        try:
                            payload, end = decoder.raw_decode(text_buffer)
                        except json.JSONDecodeError:
                            break
                        text_buffer = text_buffer[end:]
                        if isinstance(payload, dict):
                            self.dispatch(payload)
            except asyncio.CancelledError:
                break
            except Exception as exc:  # noqa: BLE001
                LOG.warning("TCP 事件通道连接失败: %s", exc)
            finally:
                if writer is not None:
                    writer.close()
                    try:
                        await writer.wait_closed()
                    except Exception:  # noqa: BLE001
                        pass
            await asyncio.sleep(3)


def normalize_message(message: Any) -> List[Dict[str, Any]]:
    """OneBot message（string 或 segment 数组）→ segment 数组。"""
    if isinstance(message, str):
        return [{"type": "text", "data": {"text": message}}]
    if isinstance(message, list):
        result = []
        for seg in message:
            if isinstance(seg, dict) and seg.get("type"):
                result.append(seg)
            elif isinstance(seg, str):
                result.append({"type": "text", "data": {"text": seg}})
        return result
    if isinstance(message, dict):
        return [message]
    return []


def segments_to_text(message: Any) -> Tuple[str, List[Any]]:
    """把 OneBot 消息段转成（纯文本, at user_id 列表）。"""
    segments = normalize_message(message)
    parts: List[str] = []
    at_list: List[Any] = []
    for seg in segments:
        seg_type = seg.get("type")
        data = seg.get("data") or {}
        if seg_type == "text":
            parts.append(str(data.get("text") or ""))
        elif seg_type == "at":
            qq = data.get("qq")
            if qq in (None, "", "all"):
                continue
            at_list.append(to_int(qq))
        elif seg_type in ("face", "reply", "node"):
            continue
        elif seg_type == "image":
            raise ActionFailed(
                "微灵不支持直接发送 OneBot 图片段（需要本地文件 + type 11030）；"
                "请在插件侧先落地文件再用 /api/Client/ExecCommand 发送"
            )
        elif seg_type in ("record", "video", "file"):
            raise ActionFailed("微灵暂不支持通过本 bridge 发送 %s 段" % seg_type)
        else:
            parts.append("[%s]" % seg_type)
    return "".join(parts), at_list


# --------------------------------------------------------------------------- 主入口
def setup_logging(level: str = "INFO", log_file: Optional[str] = None) -> None:
    LOG.setLevel(getattr(logging, level.upper(), logging.INFO))
    # Windows 控制台/重定向时 stdout 可能是 GBK，中文会变成 '?'，这里统一成 UTF-8
    if hasattr(sys.stdout, "reconfigure"):
        try:
            sys.stdout.reconfigure(encoding="utf-8", errors="replace")  # type: ignore[attr-defined]
        except Exception:  # noqa: BLE001
            pass
    # 幂等：重复调用时先清掉旧 handler（main 与 selftest 都会调用）
    for handler in list(LOG.handlers):
        LOG.removeHandler(handler)
        try:
            handler.close()
        except Exception:  # noqa: BLE001
            pass
    fmt = logging.Formatter("%(asctime)s [%(levelname)s] %(message)s")
    console = logging.StreamHandler(sys.stdout)
    console.setFormatter(fmt)
    LOG.addHandler(console)
    if log_file:
        Path(log_file).parent.mkdir(parents=True, exist_ok=True)
        file_handler = logging.FileHandler(log_file, encoding="utf-8")
        file_handler.setFormatter(fmt)
        LOG.addHandler(file_handler)


def selftest() -> int:
    """不用微灵也能验证转换逻辑。"""
    setup_logging("DEBUG")
    bridge = Bridge(json.loads(json.dumps(DEFAULT_CONFIG)))
    bridge.self_id = "1688855837869858"
    bridge.self_name = "HVIBOT"
    events = [
        {
            "client": 1,
            "type": 11041,
            "data": {
                "at_list": [{"nickname": "王金彪", "user_id": "1688857075806028"}],
                "content": "@王金彪 你好",
                "content_type": 2,
                "conversation_id": "R:10855764690307218",
                "sender": "1688856554766685",
                "sender_name": "小雨",
                "send_time": "1690000000",
                "server_id": "1099017",
            },
        },
        {
            "client": 1,
            "type": 11041,
            "data": {
                "at_list": [],
                "content": "1+1 等于几？",
                "content_type": 2,
                "conversation_id": "S:1688851111111111_1688855837869858",
                "sender": "1688851111111111",
                "sender_name": "测试好友",
                "send_time": "1690000001",
                "server_id": "1099018",
            },
        },
        {
            "client": 1,
            "type": 11042,
            "data": {
                "conversation_id": "R:10855764690307218",
                "sender": "1688856554766685",
                "sender_name": "小雨",
                "send_time": "1690000002",
                "server_id": "1099019",
            },
        },
    ]

    collected: List[Dict[str, Any]] = []

    # emit 是同步方法，这里直接替换后收集
    original_emit = bridge.emit
    bridge.emit = lambda event: collected.append(event)  # type: ignore[assignment]
    try:
        for ev in events:
            bridge.dispatch(ev)
    finally:
        bridge.emit = original_emit  # type: ignore[assignment]

    print(json.dumps(collected, ensure_ascii=False, indent=2))

    # 验证发送侧拼装
    text, ats = segments_to_text(
        [
            {"type": "at", "data": {"qq": "1688857075806028"}},
            {"type": "text", "data": {"text": " 收到"}},
        ]
    )
    print("\n发送示例: text=%r at_list=%r" % (text, ats))
    print("私聊 conversation_id:", build_conv_id("private", "1688851111111111", bridge.self_id))
    print("群聊 conversation_id:", build_conv_id("group", "10855764690307218", bridge.self_id))
    return 0


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="微灵 ⇄ OneBot v11 桥接层")
    parser.add_argument("--config", default="config.json", help="配置文件路径")
    parser.add_argument("--log-level", default="INFO", help="日志级别")
    parser.add_argument("--log-file", default="logs/bridge.log", help="日志文件（空字符串=不写文件）")
    parser.add_argument("--create-config", action="store_true", help="生成默认配置文件后退出")
    parser.add_argument("--selftest", action="store_true", help="离线自检（不需要微灵/OneBot）")
    args = parser.parse_args(argv)

    setup_logging(args.log_level, args.log_file or None)

    if args.create_config:
        path = Path(args.config)
        path.write_text(json.dumps(DEFAULT_CONFIG, ensure_ascii=False, indent=2), encoding="utf-8")
        print("已生成配置: %s" % path.resolve())
        return 0

    if args.selftest:
        return selftest()

    cfg = load_config(args.config)
    bridge = Bridge(cfg)

    async def runner() -> None:
        loop = asyncio.get_running_loop()
        for sig in (signal.SIGINT, signal.SIGTERM):
            try:
                loop.add_signal_handler(sig, lambda: asyncio.create_task(shutdown(bridge)))
            except NotImplementedError:  # Windows
                pass
        await bridge.start()

    async def shutdown(b: Bridge) -> None:
        LOG.info("正在退出...")
        b.stopping = True
        for task in (b.ws_task, b.tcp_task):
            if task and not task.done():
                task.cancel()
        if b.session:
            await b.session.close()

    try:
        asyncio.run(runner())
    except KeyboardInterrupt:
        pass
    return 0


if __name__ == "__main__":
    sys.exit(main())
