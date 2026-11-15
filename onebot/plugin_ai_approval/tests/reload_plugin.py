# -*- coding: utf-8 -*-
"""重载 astrbot_plugin_ai_approval 插件并查询 /ai审批规则 验证配置生效。"""

from __future__ import annotations

import json
import time
import urllib.request

import jwt

INSTANCE_CORE = r"C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\core"
BASE = "http://127.0.0.1:5090"
CFG = json.load(open(INSTANCE_CORE + r"\data\cmd_config.json", "r", encoding="utf-8-sig"))


def token() -> str:
    return jwt.encode(
        {"username": CFG["dashboard"]["username"], "exp": int(time.time()) + 900},
        CFG["dashboard"]["jwt_secret"],
        algorithm="HS256",
    )


def req(method, path, body=None, stream=False):
    data = json.dumps(body).encode("utf-8") if body is not None else None
    r = urllib.request.Request(
        BASE + path,
        data=data,
        method=method,
        headers={"Authorization": "Bearer " + token(), "Content-Type": "application/json"},
    )
    resp = urllib.request.urlopen(r, timeout=120)
    return resp if stream else json.loads(resp.read().decode("utf-8"))


def send(session_id, message):
    texts = []
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
    print("== reload plugin ==")
    r = req("POST", "/api/plugin/reload", {"name": "astrbot_plugin_ai_approval"})
    print(json.dumps(r, ensure_ascii=False))

    time.sleep(2)
    print("\n== plugins list check ==")
    r = req("GET", "/api/v1/plugins")
    items = r.get("data") or []
    rows = [p for p in items if p.get("name") == "astrbot_plugin_ai_approval"]
    for p in rows:
        print(
            "name=%s activated=%s version=%s"
            % (p.get("name"), p.get("activated"), p.get("version"))
        )

    print("\n== rules after reload ==")
    s = req("GET", "/api/chat/new_session?platform_id=webchat")
    sid = s["data"]["session_id"]
    for t in send(sid, "/ai审批规则"):
        print(t)


if __name__ == "__main__":
    main()
