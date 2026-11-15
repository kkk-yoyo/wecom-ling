# -*- coding: utf-8 -*-
"""用 dashboard JWT 触发热重载 astrbot_plugin_ai_approval（不重启 AstrBot）。"""
import json
import sys
import time
import urllib.request

import jwt

SECRET = "92b1dbbc6e519d1de75e09603920f5a8f2a4dce3cc8d39ad1cc5970388b406e1"
BASE = "http://127.0.0.1:5090"

payload = {
    "username": "admin",
    "exp": int(time.time()) + 1800,
    "iat": int(time.time()),
}
token = jwt.encode(payload, SECRET, algorithm="HS256")


def call(path, body):
    req = urllib.request.Request(
        BASE + path,
        data=json.dumps(body, ensure_ascii=False).encode("utf-8"),
        headers={
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token,
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            print(path, "->", r.status)
            print(r.read().decode("utf-8", "replace")[:800])
    except Exception as e:
        print(path, "ERR", type(e).__name__, str(e)[:400])
        try:
            print(e.read().decode("utf-8", "replace")[:800])  # type: ignore[attr-defined]
        except Exception:
            pass


if __name__ == "__main__":
    plugin = "astrbot_plugin_ai_approval"
    call("/api/v1/plugins/reload", {"name": plugin})
    call("/api/plugin/reload", {"name": plugin})
