# -*- coding: utf-8 -*-
"""端到端测试 2：通过 Dashboard Chat 私聊验证 /ai审批帮助 /ai审批规则 /ai审批记录 命令。"""
from __future__ import annotations

import json
import sys
import time
import urllib.request

import jwt

INSTANCE_CORE = r"C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\core"
BASE = "http://127.0.0.1:5090"
CFG = json.load(open(INSTANCE_CORE + r"\data\cmd_config.json", "r", encoding="utf-8-sig"))
SECRET = CFG["dashboard"]["jwt_secret"]
USER = CFG["dashboard"]["username"]


def token() -> str:
    return jwt.encode({"username": USER, "exp": int(time.time()) + 900}, SECRET, algorithm="HS256")


def req(method: str, path: str, body: dict | None = None, stream: bool = False):
    data = json.dumps(body).encode("utf-8") if body is not None else None
    r = urllib.request.Request(
        BASE + path,
        data=data,
        method=method,
        headers={"Authorization": "Bearer " + token(), "Content-Type": "application/json"},
    )
    resp = urllib.request.urlopen(r, timeout=90)
    return resp if stream else json.loads(resp.read().decode("utf-8"))


def send(session_id: str, message: str) -> list[str]:
    texts: list[str] = []
    resp = req("POST", "/api/chat/send", {"session_id": session_id, "message": message}, stream=True)
    for raw in resp:
        line = raw.decode("utf-8", "replace").rstrip("\r\n")
        if not line.startswith("data: "):
            continue
        try:
            evt = json.loads(line[6:])
        except Exception:
            continue
        if evt.get("type") == "plain" and evt.get("data"):
            texts.append(evt["data"])
        if evt.get("type") == "end":
            break
    return texts


def main() -> None:
    s = req("GET", "/api/chat/new_session?platform_id=webchat")
    session_id = s["data"]["session_id"]
    print("session:", session_id)
    for cmd in ["/ai审批帮助", "/ai审批规则", "/ai审批记录"]:
        out = send(session_id, cmd)
        print("=" * 20, cmd)
        for t in out:
            print(t)
        if not out:
            print("(无回复)")
    print("DONE")


if __name__ == "__main__":
    main()
