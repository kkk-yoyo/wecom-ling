# 微灵 ⇄ OneBot v11 桥接层

把微灵（WeComSever）hook 的事件流转换成 **OneBot v11** 协议，接入任意 OneBot v11 实现
（AstrBot 的 `aiocqhttp` 平台、NapCat、LLOneBot 等）。

```
微灵.exe (8060 HTTP + 9090 TCP + CallbackUrl)
        │  事件：{"type":11041,"data":{...},"client":1}
        ▼
   ling_bridge.py  ── 反向 WebSocket（X-Client-Role: universal）──▶  OneBot 实现
        │  指令：POST http://127.0.0.1:8060/api/Client/ExecCommand
        ▼
微灵 → 企业微信客户端
```

## 1. 启动微灵

1. 企业微信客户端必须是 **5.0.0.6008**（微灵只适配这个版本）；
2. 以管理员身份运行 `微灵.exe`，等日志出现 `服务已启动，监听地址: http://0.0.0.0:8060/`；
3. 编辑微灵目录下的 `config.ini`：

```ini
Port=8060
CallbackUrl=http://127.0.0.1:8899/wecom/callback
TcpPort=9090
```

4. 重启微灵（或点 GUI 的「保存」「停止」后重启）。

## 2. 启动桥接层

```bash
pip install aiohttp
python ling_bridge.py --config config.json
```

- `--config`：配置文件，参考 `config.example.json`（首次可直接 `--create-config` 生成）；
- `--selftest`：离线自检，不需要微灵和 OneBot，会把转换结果打印出来；
- `--log-file logs/bridge.log`：写日志文件。

启动后访问 <http://127.0.0.1:8899/status> 查看状态：

```json
{"version":"0.1.0","client_id":1,"self_id":"168885...","self_name":"HVIBOT",
 "onebot_ws_connected":true,"stats":{"events":12,"messages":8,"actions":9,"sent":6,"errors":0}}
```

## 3. 配置项

| 键 | 默认 | 说明 |
| --- | --- | --- |
| `http.host` / `http.port` | 127.0.0.1 / 8899 | 接收微灵 CallbackUrl 的监听地址 |
| `ling.api` | http://127.0.0.1:8060 | 微灵 HTTP 服务地址 |
| `ling.client` | 1 | 微灵客户端 ID（GUI 里「客户端 N」；多开时对应不同实例） |
| `ling.tcp` | 空 | 可选：`host:port` 连微灵 TCP 9090 事件通道（与回调二选一，重复的会自动去重） |
| `onebot.ws` | ws://127.0.0.1:6200/ws | OneBot 反向 WS 地址（AstrBot 企微平台端口） |
| `onebot.access_token` | 空 | OneBot 侧 token（AstrBot `ws_reverse_token`） |
| `onebot.self_id` | 空 | 账号 ID；留空则由微灵登录事件自动获取（推荐留空） |

> AstrBot 侧：平台类型 `aiocqhttp`，`ws_reverse_host`/`ws_reverse_port` 指向桥接层所连的
> 那个反向 WS 端口（历史配置中企微单独用 6200，QQ 用 6199，互不干扰）。

## 4. 支持的 OneBot action

| action | 实现 |
| --- | --- |
| `send_private_msg` / `send_group_msg` / `send_msg` | ✅ 合成为微灵 11029（私聊）/ 11069（群聊带 @ 时） |
| `get_login_info` | ✅ 微灵登录事件里的 user_id / username |
| `get_status` / `get_version_info` | ✅ 本地状态 |
| `get_group_info` / `get_group_member_info` / `get_stranger_info` / `get_group_member_list` / `get_group_list` / `get_friend_list` | ⚠️ 最小实现（返回空壳；群成员接口需后续用微灵指令补全） |
| `get_msg` / `delete_msg` / `send_like` 等 | ❌ 返回 failed |

图/视频/文件/语音 **发送** 暂不支持（微灵发送图片需要本地文件路径 + 11030 指令，
且 hook 端的 CDN 上行不稳定）；接收侧会转成 `[图片]`、`[文件]` 等文本占位。
详见 `../docs/PROTOCOL.md`。

## 5. 事件映射

| 微灵 type | OneBot 事件 |
| --- | --- |
| 11041 文本 | `message`（group/private，含 at 段） |
| 11042/11043/11044/11045/… 媒体 | `message` + `[图片]`/`[视频]` 等文本占位 |
| 11123 撤回 | `notice`（group_recall / friend_recall，尽力而为） |
| 11026 / 11179 登录成功 | 内部状态：设置 self_id，建立 OneBot 连接 |
| 11027 退出 | 断开 OneBot 连接 |
| 1000 / -1000 客户端上线/下线 | 内部状态：更新 client_id |

## 6. 常见问题

- **启动后 `onebot_ws_connected: false`**：微灵还没登录；桥接层会等 11026/11179 登录事件后自动连接。
- **回调 400 invalid json**：确认微灵 `config.ini` 的 `CallbackUrl` 指向本服务的根路径（任意路径都行），
  并且已经重启微灵。
- **消息重复**：微灵会把事件同时推到 CallbackUrl 和 TCP（若都配了），桥接层按 `server_id+send_time`
  去重；仍重复就只配其中一个通道。
- **端口占用**：8899 被占时改 `http.port`，同步改微灵 `CallbackUrl`。

## 7. 自检

```bash
python ling_bridge.py --selftest
```

不需要微灵/企业微信，仅验证「微灵事件 → OneBot 事件」和「OneBot 消息段 → 微灵指令」的拼装逻辑。
