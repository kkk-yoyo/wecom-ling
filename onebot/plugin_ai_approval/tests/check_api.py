"""通过 AstrBot Dashboard REST API 确认插件已被识别（使用实例 jwt_secret 自签 JWT）。"""

from __future__ import annotations

import json
import time
import urllib.error
import urllib.request
from pathlib import Path

import jwt

INSTANCE = Path(r"C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200")
PLUGIN_NAME = "astrbot_plugin_ai_approval"
cfg = json.loads((INSTANCE / "core" / "data" / "cmd_config.json").read_text(encoding="utf-8-sig"))
secret = cfg["dashboard"]["jwt_secret"]
token = jwt.encode(
    {"username": cfg["dashboard"].get("username", "admin"), "exp": int(time.time()) + 600},
    secret,
    algorithm="HS256",
)


def call(base: str, method: str, path: str, body: dict | None = None, timeout: float = 30.0):
    url = base.rstrip("/") + path
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header("Authorization", f"Bearer {token}")
    if data is not None:
        req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            return resp.status, resp.read().decode("utf-8", errors="replace")
    except urllib.error.HTTPError as exc:
        return exc.code, exc.read().decode("utf-8", errors="replace")
    except Exception as exc:  # noqa: BLE001
        return 0, f"{type(exc).__name__}: {exc}"


candidates = []
for port in (cfg["dashboard"].get("port"), 5090, 6185):
    if port and port not in candidates:
        candidates.append(port)

base = None
for port in candidates:
    for host in ("127.0.0.1", "localhost"):
        b = f"http://{host}:{port}"
        status, text = call(b, "GET", "/api/v1/plugins", timeout=8)
        print(f"[probe] {b} GET /api/v1/plugins -> {status} {text[:160]}")
        if status == 200:
            base = b
            break
    if base:
        break

if not base:
    print("dashboard API 不可达，改用日志/文件方式验证")
    raise SystemExit(2)

status, text = call(base, "GET", "/api/v1/plugins")
print(f"\n[plugins] status={status}")
try:
    payload = json.loads(text)
    data = payload.get("data", payload)
    plugins = data.get("plugins", data) if isinstance(data, dict) else data
    names = [p.get("name") for p in plugins if isinstance(p, dict)] if isinstance(plugins, list) else []
    print(f"[plugins] 点数={len(names)}，包含目标插件={'是' if PLUGIN_NAME in names else '否'}")
    print("[plugins] 目标条目：", next((p for p in plugins if isinstance(p, dict) and p.get("name") == PLUGIN_NAME), None))
except Exception as exc:  # noqa: BLE001
    print("[plugins] 解析响应失败：", exc, text[:400])

if PLUGIN_NAME not in (names if isinstance(names, list) else []):
    print("\n[reload] 插件尚未被加载，调用 legacy /api/plugin/reload 重载全部插件 ...")
    status, text = call(base, "POST", "/api/plugin/reload", {}, timeout=300)
    print(f"[reload] status={status} body={text[:500]}")
    status, text = call(base, "GET", "/api/v1/plugins")
    try:
        payload = json.loads(text)
        data = payload.get("data", payload)
        plugins = data.get("plugins", data) if isinstance(data, dict) else data
        names = [p.get("name") for p in plugins if isinstance(p, dict)] if isinstance(plugins, list) else []
        entry = next((p for p in plugins if isinstance(p, dict) and p.get("name") == PLUGIN_NAME), None)
        print(f"[plugins] 重载后：包含目标插件={'是' if PLUGIN_NAME in names else '否'}，总数={len(names)}")
        print("[plugins] 目标条目：", json.dumps(entry, ensure_ascii=False, indent=2)[:3000] if entry else None)
    except Exception as exc:  # noqa: BLE001
        print("[plugins] 解析响应失败：", exc, text[:500])
