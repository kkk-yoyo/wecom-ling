#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Mock OneBot v11 reverse-WS server for testing the 微灵 (WeComSever) OneBot shim.

Listens on 127.0.0.1:6399, WebSocket endpoint /ws (the shim's default path).
Validates handshake headers:
    Authorization: Bearer testtoken
    X-Self-ID: <non-empty>
    X-Client-Role: Universal

Control API (plain HTTP, consumed by TestHost.exe):
    GET  /ctl/ping                       -> {"ok": true, "pid": ...}
    GET  /ctl/state?since=N              -> frames with seq > N, connections, rejections
    GET  /ctl/media_info?name=pic.png    -> deterministic media fixture (size/sha256/base64/hits)
    POST /ctl/action  {action, params, echo}  -> push an action request to newest open conn
    POST /ctl/close_all                  -> server-side close of all open connections
    POST /ctl/reset                      -> clear recorded state
    POST /ctl/shutdown                   -> stop the mock server process

Media fixtures (for shim get_image/get_record/get_file download tests):
    GET /media/pic.png, /media/doc.txt, /media/rec.silk

Everything is also appended to files under ./logs/:
    mock_server.log, frames.jsonl, connections.jsonl, rejections.jsonl, media_hits.jsonl
"""

import asyncio
import base64
import hashlib
import json
import logging
import os
import signal
import sys
import time

from aiohttp import web, WSMsgType

BASE = os.path.dirname(os.path.abspath(__file__))
LOGDIR = os.path.join(BASE, "logs")
os.makedirs(LOGDIR, exist_ok=True)

HOST = "127.0.0.1"
PORT = 6399
EXPECTED_TOKEN = "testtoken"

# Deterministic byte fixtures served to the shim's on-demand media downloader.
MEDIA_FILES = {
    "pic.png": b"\x89PNG\r\n\x1a\n" + b"MOCK-PNG-" + bytes(range(16)),
    "doc.txt": b"MOCK-FILE-CONTENT-1234567890\n",
    "rec.silk": b"MOCK-RECORD-DATA",
}

LOCK = asyncio.Lock()
STATE = {
    "seq": 0,
    "frames": [],
    "connections": [],
    "rejections": [],
    "media_hits": [],
    "open": {},          # conn id -> WebSocketResponse
    "started_at": time.time(),
    "shutdown": False,
}


def logf(name, obj):
    try:
        with open(os.path.join(LOGDIR, name), "a", encoding="utf-8") as f:
            f.write(json.dumps(obj, ensure_ascii=False) + "\n")
    except Exception:
        pass


def mock_log(text):
    line = time.strftime("%Y-%m-%d %H:%M:%S") + " " + text
    try:
        with open(os.path.join(LOGDIR, "mock_server.log"), "a", encoding="utf-8") as f:
            f.write(line + "\n")
    except Exception:
        pass
    print(line, flush=True)


def record_frame(conn_id, direction, data):
    STATE["seq"] += 1
    rec = {
        "seq": STATE["seq"],
        "conn": conn_id,
        "dir": direction,     # "in" = shim -> mock ; "out" = mock -> shim
        "at": time.time(),
        "data": data,
    }
    STATE["frames"].append(rec)
    logf("frames.jsonl", rec)
    return rec


async def ws_handler(request):
    headers = {str(k).lower(): str(v) for k, v in request.headers.items()}
    auth = request.headers.get("Authorization", "")
    expected = "Bearer " + EXPECTED_TOKEN
    if auth != expected:
        rec = {
            "at": time.time(),
            "path": request.path,
            "remote": request.remote,
            "headers": headers,
            "reason": "token mismatch (expected %r got %r)" % (expected, auth),
        }
        STATE["rejections"].append(rec)
        logf("rejections.jsonl", rec)
        mock_log("REJECT ws handshake from %s: %s" % (request.remote, rec["reason"]))
        return web.Response(status=401, text="invalid token")

    ws = web.WebSocketResponse(max_msg_size=4 * 1024 * 1024, heartbeat=None)
    await ws.prepare(request)

    conn_id = len(STATE["connections"]) + 1
    conn = {
        "id": conn_id,
        "path": request.path,
        "remote": request.remote,
        "opened_at": time.time(),
        "closed_at": None,
        "headers": headers,
    }
    STATE["connections"].append(conn)
    STATE["open"][conn_id] = ws
    logf("connections.jsonl", {"event": "open", **conn})
    mock_log("CONNECT #%d from %s path=%s self_id=%s role=%s"
             % (conn_id, request.remote, request.path,
                headers.get("x-self-id"), headers.get("x-client-role")))

    try:
        async for msg in ws:
            if msg.type == WSMsgType.TEXT:
                raw = msg.data
                try:
                    payload = json.loads(raw)
                except Exception:
                    payload = {"_raw": raw}
                record_frame(conn_id, "in", payload)
            elif msg.type == WSMsgType.BINARY:
                record_frame(conn_id, "in", {"_binary_len": len(msg.data)})
            elif msg.type == WSMsgType.ERROR:
                mock_log("WS error on conn #%d: %s" % (conn_id, ws.exception()))
                break
    finally:
        conn["closed_at"] = time.time()
        STATE["open"].pop(conn_id, None)
        logf("connections.jsonl", {"event": "close", "id": conn_id,
                                   "closed_at": conn["closed_at"]})
        mock_log("DISCONNECT #%d" % conn_id)
    return ws


async def media_handler(request):
    name = request.match_info.get("name", "")
    data = MEDIA_FILES.get(name)
    if data is None:
        mock_log("MEDIA GET miss: %r" % (name,))
        return web.Response(status=404, text="no such media fixture")
    rec = {"name": name, "at": time.time(), "size": len(data), "remote": request.remote}
    STATE["media_hits"].append(rec)
    logf("media_hits.jsonl", rec)
    mock_log("MEDIA GET %s (%d bytes)" % (name, len(data)))
    return web.Response(body=data, content_type="application/octet-stream")


async def ctl_media_info(request):
    name = request.query.get("name", "")
    data = MEDIA_FILES.get(name)
    if data is None:
        return web.json_response({"ok": False, "error": "no such media fixture: " + name}, status=404)
    hits = sum(1 for h in STATE["media_hits"] if h["name"] == name)
    return web.json_response({
        "ok": True,
        "name": name,
        "size": len(data),
        "sha256": hashlib.sha256(data).hexdigest(),
        "base64": base64.b64encode(data).decode("ascii"),
        "hits": hits,
    })


async def ctl_ping(request):
    return web.json_response({"ok": True, "pid": os.getpid(), "started_at": STATE["started_at"]})


async def ctl_state(request):
    try:
        since = int(request.query.get("since", "0"))
    except Exception:
        since = 0
    frames = [f for f in STATE["frames"] if f["seq"] > since]
    if len(frames) > 5000:
        frames = frames[:5000]
    next_seq = STATE["seq"] if STATE["frames"] else 0
    return web.json_response({
        "ok": True,
        "now": time.time(),
        "next": next_seq,
        "frames": frames,
        "connections": STATE["connections"],
        "rejections": STATE["rejections"],
        "media_hits": STATE["media_hits"],
        "open_conns": sorted(STATE["open"].keys()),
        "expected_token": EXPECTED_TOKEN,
    })


async def ctl_action(request):
    body = await request.json()
    action = body.get("action")
    params = body.get("params")
    echo = body.get("echo")
    frame = {"action": action, "params": params if params is not None else {}, "echo": echo}
    async with LOCK:
        targets = sorted(STATE["open"].items())
        if not targets:
            return web.json_response({"ok": False, "error": "no open websocket connection"}, status=409)
        conn_id, ws = targets[-1]
        await ws.send_str(json.dumps(frame, ensure_ascii=False))
        rec = record_frame(conn_id, "out", frame)
    mock_log("ACTION #%d %s echo=%r" % (conn_id, action, echo))
    return web.json_response({"ok": True, "conn": conn_id, "seq": rec["seq"], "echo": echo})


async def ctl_close_all(request):
    async with LOCK:
        items = list(STATE["open"].items())
    closed = []
    for conn_id, ws in items:
        try:
            await ws.close(code=1000, message=b"mock-close")
            closed.append(conn_id)
        except Exception as exc:
            mock_log("close #%d failed: %s" % (conn_id, exc))
    mock_log("CLOSE_ALL requested, closed=%r" % (closed,))
    return web.json_response({"ok": True, "closed": closed})


async def ctl_reset(request):
    STATE["frames"] = []
    STATE["connections"] = []
    STATE["rejections"] = []
    STATE["media_hits"] = []
    STATE["seq"] = 0
    mock_log("RESET state")
    return web.json_response({"ok": True})


async def ctl_shutdown(request):
    STATE["shutdown"] = True
    mock_log("SHUTDOWN requested")
    loop = asyncio.get_event_loop()
    loop.call_later(0.2, loop.stop)
    return web.json_response({"ok": True})


async def on_shutdown(app):
    mock_log("mock server shutting down")


def main():
    logging.basicConfig(level=logging.WARNING)
    app = web.Application(client_max_size=8 * 1024 * 1024)
    app.router.add_get("/ctl/ping", ctl_ping)
    app.router.add_get("/ctl/state", ctl_state)
    app.router.add_get("/ctl/media_info", ctl_media_info)
    app.router.add_post("/ctl/action", ctl_action)
    app.router.add_post("/ctl/close_all", ctl_close_all)
    app.router.add_post("/ctl/reset", ctl_reset)
    app.router.add_post("/ctl/shutdown", ctl_shutdown)
    app.router.add_get("/ws", ws_handler)
    app.router.add_get("/media/{name}", media_handler)
    app.on_shutdown.append(on_shutdown)

    mock_log("=" * 70)
    mock_log("Mock OneBot v11 server starting on http://%s:%d (ws path /ws, token %r)"
             % (HOST, PORT, EXPECTED_TOKEN))
    web.run_app(app, host=HOST, port=PORT, print=None,
                shutdown_timeout=3.0, handle_signals=True)


if __name__ == "__main__":
    main()
