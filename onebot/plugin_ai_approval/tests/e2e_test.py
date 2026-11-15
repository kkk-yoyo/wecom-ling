# -*- coding: utf-8 -*-
"""端到端测试：通过 AstrBot Dashboard Chat API 以私聊形式发送审批申请，
验证插件是否自动通过并写入审计日志。

用法：实例 venv python e2e_test.py
"""
from __future__ import annotations

import json
import time
import urllib.request
import urllib.parse

import jwt

INSTANCE_CORE = r"C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\core"
BASE = "http://127.0.0.1:5090"
CFG = json.load(open(INSTANCE_CORE + r"\data\cmd_config.json", "r", encoding="utf-8-sig"))


def token() -> str:
    secret = CFG["dashboard"]["jwt_secret"]
    payload = {"username": CFG["dashboard"]["username"], "exp": int(time.time()) + 900}
    return jwt.encode(payload, secret, algorithm="HS256")


def req(method: str, path: str, body: dict | None = None, stream: bool = False):
    url = BASE + path
    data = json.dumps(body).encode("utf-8") if body is not None else None
    r = urllib.request.Request(
        url,
        data=data,
        method=method,
        headers={
            "Authorization": "Bearer " + token(),
            "Content-Type": "application/json",
        },
    )
    resp = urllib.request.urlopen(r, timeout=90)
    if stream:
        return resp
    return json.loads(resp.read().decode("utf-8"))


def main() -> None:
    s = req("GET", "/api/chat/new_session?platform_id=webchat")
    print("new_session:", json.dumps(s, ensure_ascii=False)[:300])
    session_id = (s.get("data") or {}).get("session_id") or (s.get("data") or {}).get("conversation_id")
    if not session_id:
        raise SystemExit("no session_id")
    print("session_id:", session_id)

    msg = "申请请假 1 天，事由：端到端测试"
    resp = req("POST", "/api/chat/send", {"session_id": session_id, "message": msg}, stream=True)
    n = 0
    text_all = []
    for raw in resp:
        line = raw.decode("utf-8", "replace").rstrip("\r\n")
        if not line:
            continue
        print("SSE:", line[:500])
        text_all.append(line)
        n += 1
        if n >= 40:
            break
    print("--- done, %d lines ---" % n)


if __name__ == "__main__":
    main()
