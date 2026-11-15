# -*- coding: utf-8 -*-
"""查询 webchat 私聊中 /ai审批规则 输出，用于判断配置文件修改是否即时生效。"""

from __future__ import annotations

import json
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


def req(method, path, body=None, stream=False):
    data = json.dumps(body).encode("utf-8") if body is not None else None
    r = urllib.request.Request(
        BASE + path,
        data=data,
        method=method,
        headers={"Authorization": "Bearer " + token(), "Content-Type": "application/json"},
    )
    resp = urllib.request.urlopen(r, timeout=90)
    return resp if stream else json.loads(resp.read().decode("utf-8"))


def send(session_id, message):
    texts, items = [], []
    resp = req("POST", "/api/chat/send", {"session_id": session_id, "message": message}, stream=True)
    for raw in resp:
        line = raw.decode("utf-8", "replace").rstrip("\r\n")
        if not line.startswith("data: "):
            continue
        try:
            evt = json.loads(line[6:])
        except Exception:
            continue
        items.append(evt)
        if evt.get("type") == "plain" and evt.get("data"):
            texts.append(evt["data"])
        if evt.get("type") == "end":
            break
    return texts, items


def main() -> None:
    s = req("GET", "/api/chat/new_session?platform_id=webchat")
    sid = s["data"]["session_id"]
    texts, items = send(sid, "/ai审批规则")
    print("SESSION:", sid)
    for t in texts:
        print(t)
    print("EVENTS:", json.dumps([i.get("type") for i in items], ensure_ascii=False))


if __name__ == "__main__":
    main()
