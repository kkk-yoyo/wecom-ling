# -*- coding: utf-8 -*-
"""微灵 HTTP 8060 stub：为 AstrBotProbe / 联调提供 self_id 查询与最小 ExecCommand 响应。

用法: python http_stub_8060.py [--self-id 1688855837869858] [--port 8060]
仅监听 127.0.0.1。收到 POST /api/Client/ExecCommand 时解析 {client,type,data}：
  type=11035（自己信息） -> {"code":200,"msg":"请求成功！","data":{"user_id":...,"account":...,"username":"ProbeBot"}}
  其它 type             -> {"code":200,"msg":"请求成功！","data":{}}
"""
import argparse
import json
import sys
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer


class Handler(BaseHTTPRequestHandler):
    server_version = "WeComStub/1.0"
    self_id = "1688855837869858"

    def log_message(self, fmt, *args):
        sys.stdout.write("[stub] %s %s\n" % (self.command, (args[0] if args else "")))
        sys.stdout.flush()

    def _send_json(self, obj, code=200):
        body = json.dumps(obj, ensure_ascii=False).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path.startswith("/api/Client/") or self.path == "/status":
            self._send_json({"code": 200, "msg": "请求成功！", "data": {}, "time": int(time.time())})
            return
        self._send_json({"code": 404, "msg": "not found", "data": None}, 404)

    def do_POST(self):
        length = int(self.headers.get("Content-Length") or 0)
        raw = self.rfile.read(length) if length else b""
        try:
            req = json.loads(raw.decode("utf-8"))
        except Exception:
            req = {}
        client = req.get("client", 1)
        typ = req.get("type", 0)
        if isinstance(typ, str):
            try:
                typ = int(typ)
            except Exception:
                typ = 0
        sys.stdout.write("[stub] POST type=%s client=%s\n" % (typ, client))
        sys.stdout.flush()
        data = {}
        if typ == 11035:
            data = {
                "user_id": self.self_id,
                "account": self.self_id,
                "username": "ProbeBot",
                "name": "ProbeBot",
                "corp_id": "wwprobe",
                "pid": 4242,
            }
        self._send_json({"code": 200, "msg": "请求成功！", "data": data, "time": int(time.time())})


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--self-id", default="1688855837869858")
    parser.add_argument("--port", type=int, default=8060)
    args = parser.parse_args()
    Handler.self_id = args.self_id
    server = ThreadingHTTPServer(("127.0.0.1", args.port), Handler)
    sys.stdout.write("[stub] listening on 127.0.0.1:%d self_id=%s\n" % (args.port, args.self_id))
    sys.stdout.flush()
    server.serve_forever()


if __name__ == "__main__":
    main()
