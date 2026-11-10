# 微灵协议说明（PROTOCOL）

适配客户端：**企业微信 5.0.0.6008 x86（PC）**。程序集 `WeComSever`，界面版本 v1.0.3 / 文件版本 1.0.1.0。

## 1. 总览

微灵启动后：

1. 以管理员权限运行（否则注入失败）；
2. `LoadLibrary("Loader.dll")`，初始化 socket，遍历并注入所有 `WXWork.exe` 进程；
3. 启动三个对外通道；
4. 每个被注入的客户端实例分配一个 `client` 编号（从 1 开始），所有指令与事件都带 `client`。

| 通道 | 默认地址 | 方向 | 说明 |
| --- | --- | --- | --- |
| HTTP API | `http://0.0.0.0:8060/`（`Port`） | 下行指令 | ASP.NET Web API SelfHost，路由模板 `api/{controller}/{action}/{id}` |
| WebSocket | `http://*:8061/message/`（固定） | 双向 | 发送 JSON 指令；广播所有事件 |
| TCP | `0.0.0.0:9090`（`TcpPort`） | 上行事件 / 下行指令 | 纯 TCP，UTF-8 JSON，无分帧协议 |
| HTTP 回调 | `config.ini` 的 `CallbackUrl` | 上行事件 | 每个事件 POST 一条 JSON |

- 指令统一格式：`{"client": 1, "type": <指令号>, "data": {...}}`
- 事件统一格式：`{"client": 1, "type": <事件号>, "data": {...}}`（部分早期事件没有 `client`）
- 非 ASCII 字符在传给原生 DLL 前会被转成 `\uXXXX` 转义，对外无感知。
- 所有指令均无鉴权，只监听在本机/内网，**不要暴露到公网**。

## 2. HTTP API（8060）

### 2.1 下发指令

```
POST http://127.0.0.1:8060/api/Client/ExecCommand
Content-Type: application/json

{"client": 1, "type": 11029,
 "data": {"conversation_id": "S:1688851111111111_1688855837869858", "content": "hello"}}
```

- 请求体必须包含 `client` 字段（缺省会抛异常/全局异常过滤器返回错误）。
- `data` 可以省略，但对应指令就取不到参数。
- 可加 `"getData": true`：**同步等待**本次指令的原生返回（最长 60 秒），成功时 `data` 为原生返回值。
  同步响应只回到这次 HTTP 调用，**不会再广播**到 TCP/WS/回调。
- 不加 `getData` 时是异步的，HTTP 立即返回“指令已发送”。
- `model.ToString()` 是被原样传给原生 DLL 的 JSON 字符串（`client`/`type` 都在里面），
  `ExecCommand` 本身只解析 `client`、`type`、`getData` 三个键用于路由。

返回结构：

```json
{"code": 200, "msg": "请求成功！", "data": null, "time": "2026-11-01 12:00:00"}
```

| code | 含义 |
| --- | --- |
| 200 | 成功（`msg` 默认「请求成功！」） |
| 500 / 其它 | 失败，`msg` 为异常信息 |

### 2.2 其它动作

| 方法 | 路径 | 说明 |
| --- | --- | --- |
| GET | `/api/Client/OpenWeCom` | 拉起企业微信客户端 |
| GET | `/api/Client/InjectAllWeCom` | 对当前所有 `WXWork.exe` 进程重新注入 |
| GET | `/api/Client/ReInitSocket` | 重新初始化 DLL socket（掉线自救） |
| POST | `/api/Client/ExecCommand` | 下发指令（见上） |
> CDN 下载不是独立路由，而是 `POST /api/Client/ExecCommand` 的 `type=11170 / 11171` 分支：
> 命中时**不返回 JSON，而是返回文件流**（`Content-Disposition: attachment`），内部流程是
> 同步下载到 `temp_cdn/` 再读回字节。需要提供完整参数：
>
> - `11170`（C2cCDN）：`data` 里必须有 `aes_key`、`file_id`、`file_size`、`file_type`
> - `11171`（WxCDN）：`data` 里必须有 `url`、`auth_key`、`aes_key`、`size`、`download_type`
>
> 扩展名/Content-Type 映射：`file_type` 1/2/3→`.jpg`(image/jpeg)、4→`.mp4`(video/mp4)、5→`.dat`；
> `download_type` 1→`.jpg`、2→`.mp4`、3→`.amr`、其它→`.dat`。

### 2.3 同步指令示例

```json
{"client": 1, "type": 11035, "getData": true}
```

```json
{"code": 200, "msg": "请求成功！",
 "data": {"user_id": "1688855837869858", "username": "HVIBOT", "corp_id": "...", "...": "..."},
 "time": "..."}
```

> 同步锁的 key 是 `{client}_{type}_{ticks}`，下载类按 `save_path` 匹配，因此同一 `client + type` 下不要并发发起多个同步请求。

## 3. WebSocket（8061）

- 地址固定：`ws://127.0.0.1:8061/message/`
- 连接后服务端会向该连接广播所有客户端事件。
- 客户端发送文本帧必须是带 `client` 的 JSON 指令，等价于 HTTP `ExecCommand`：
  `{"client": 1, "type": 11029, "data": {...}}`
- 非 WebSocket 的普通 HTTP 请求访问该地址会返回 `400 Bad Request`。
- 日志里 `Received: {...}` 就是 WS 收到指令时打印的。
- 注意：WS **不会**给指令返回响应（下行结果要看事件或 `CallbackUrl`），需要返回值请用 HTTP `getData=true`。

## 4. TCP（9090）

- 监听 `0.0.0.0:9090`（`config.ini` 的 `TcpPort`，改为 0 可关闭）。
- 服务端把事件以 UTF-8 JSON 逐条写给所有已连接客户端（**没有长度前缀/分隔符**，按读到的 JSON 边界解析）。
- 客户端向 TCP 发送含 `client` 的 JSON 指令，效果同 WS。

## 5. 事件推送（CallbackUrl / WS / TCP）

```json
POST http://<CallbackUrl>
{"client": 1, "type": 11041, "data": {...}}
```

> ⚠️ 回调实现细节（来自 `WeComHelper.PostJsonAsync`）：主程序把已经是字符串的事件
> `JsonConvert.SerializeObject(string)` 后再 POST，因此 **CallbackUrl 实际收到的是一个被二次编码的
> JSON 字符串**（body 形如 `"{\"type\":11041,...}"`）。接收端要 `json.loads` 两次。
> `bridge/ling_bridge.py` 已兼容两种形式。
>
> 事件 JSON 在广播前会被主程序补充字段：`client`（接收回调的 client id）追加到顶层，
> 若 `data.receiver` 缺失则补成本机登录 user_id；`client` 上线/下线事件（1000 / -1000）
> 没有 `client` 字段，只有 `data.clientId` 和 `message`。

### 5.1 生命周期 / 登录

| type | 事件 | data 关键字段 |
| --- | --- | --- |
| 1000 | 客户端上线（socket 接入） | `clientId`，另有 `message:"客户端上线"` |
| -1000 | 客户端下线（socket 断开） | `clientId`，另有 `message:"客户端下线"` |
| 11024 | 接口就绪（最早的事件） | `pid` |
| 11028 | 登录二维码 | `code`（二维码内容 URL）、`file`（本地 qr.png 路径）、`key` |
| 11174 | 登录二维码状态 | `status`（0 待扫 / 1 已扫待确认 / …）、`vid`（用户 id）、`corp_id`、`nick_name`、`icon_url`、`logo`、`qrcode_key` 等 |
| 11026 | 登录成功（无公司名，扫码或已登录都会触发） | `user_id`、`username`、`avatar`、`mobile`、`corp_id`、`acctid`、`pid`、`document_root`、`account`、`email`、`sex`、`nickname`、`position`、`job_name` |
| 11179 | 登录成功（含公司名，11026 之后延迟触发） | 同 11026，另加 `corp_name`、`corp_short_name` |
| 11027 | 用户退出（手机端退出或切换企业） | `user_id` |

### 5.2 消息事件

| type | 消息 | 关键字段 |
| --- | --- | --- |
| 11041 | 文本 | `content`、`content_type`(=2)、`conversation_id`、`sender`、`sender_name`、`receiver`、`at_list`、`is_pc`、`local_id`、`server_id`、`send_time` |
| 11042 | 图片 | `content_type`(=101)、`cdn_type`(1 wxCdn / 2 c2cCdn / 3 bigCdn)、`cdn{ aes_key, auth_key, url, size, md_url, md_size, ld_url, ld_size, md5 }` |
| 11047 | 链接 | 卡片标题/描述/缩略图/URL |
| 11048 | GIF | 同图片（cdn） |
| 11045 | 文件 | `file_name`、`file_size`、`cdn` 等 |
| 11043 | 视频 | `cdn`、`duration` 等 |
| 11044 | 语音 | `cdn`、`duration` |
| 11046 | 位置 | `title`、`address`、经纬度等 |
| 11050 | 名片 | 名片用户信息 |
| 11066 | 小程序 | 小程序标题/图标/页面参数 |
| 11124 | 视频号 | 视频号内容信息 |
| 11195 | 视频号直播 | 直播信息 |
| 11068 | 图文 | 图文链接卡片 |
| 11049 | 红包 | 红包信息 |
| 11123 | 撤回通知 | 被撤回的消息标识 |

文本消息示例：

```json
{"client": 1, "type": 11041,
 "data": {"at_list": [{"nickname": "张三", "user_id": "1688857075806028"}],
          "content": "@张三?2万 1次 车评",
          "content_type": 2,
          "conversation_id": "R:10855764690307218",
          "is_pc": 0, "local_id": "2819", "receiver": "1688850535xxx",
          "send_time": "1639407098", "sender": "78813022839xx",
          "sender_name": "小邪", "server_id": "1099017"}}
```

### 5.3 好友 / 群通知

| type | 事件 |
| --- | --- |
| 11063 | 好友申请通知 |
| 11076 | 好友新增 |
| 11077 | 好友删除 |
| 11173 | 新增群（外部） |
| 11074 | 新增群 |
| 11072 | 群成员增加 |
| 11073 | 群成员减少 |
| 11075 | 主动退群 |
| 11078 | 群名称变化 |

## 6. conversation_id 规则（关键）

| 会话类型 | 格式 | 示例 |
| --- | --- | --- |
| 私聊 | `S:<对方 user_id>_<自己 user_id>` | `S:1688851111111111_1688855837869858` |
| 群聊 | `R:<群/room id>` | `R:10855764690307218` |

- 群消息里 `sender` 是群成员，`sender_name` 是群昵称；私聊里 `sender` 是对方。
- 发消息时 `conversation_id` 必须按上表构造；`R:` 后面的数字不是群号而是内部 room id，可从群消息/群列表事件获取。

## 7. 指令表（type → 用途）

“指令”为 `POST /api/Client/ExecCommand` 的 `type`，除非注明为同步。

### 7.1 登录 / 客户

| type | 指令 | data 主要字段 |
| --- | --- | --- |
| 11153 | 刷新登录二维码 | `{}`（结果走 11028） |
| 11112 | 退出登录 | `{}` |
| 11035 | 当前登录帐号信息 | `{}` |
| 11122 | 获取自己的二维码 | `{}` |
| 11039 | 获取联系人信息 | `{"user_id": ...}` |
| 11036 / 11037 / 11038 / 11040 | 列表类（内部联系人 / 外部联系人 / 群等） | `{"page_num":1,"page_size":100}` |
| 11052 | 搜索微信/企微用户 | `{"keyword": ...}` |
| 11053 | 添加搜索到的微信用户 | 搜索结果中的字段 |
| 11088 | 添加搜索到的企微用户 | 同上 |
| 11121 | 添加名片为联系人 | 名片信息 |
| 11152 | 恢复被删除的联系人 | `user_id` 等 |
| 11054 | 修改内/外部好友备注 | `user_id`、`remark` |
| 11055 | 修改好友描述 | `user_id`、`desc` |
| 11056 | 修改外部联系人手机号 | `user_id`、`mobile` |
| 11057 | 修改外部联系人公司名 | `user_id`、`corp_name` |
| 11111 | 删除联系人 | `user_id` |
| 11064 | 接受好友申请 | 申请信息 |
| 11036 | 内部联系人列表 | `page_num`、`page_size` |

### 7.2 群

| type | 指令 | data 主要字段 |
| --- | --- | --- |
| 11058 | 创建内/外部群 | `user_list`、`room_name` |
| 11125 | 创建空外部群 | `room_name` |
| 11059 | 修改群名 | `room_id`、`room_name` |
| 11060 | 批量邀请好友入群 | `room_id`、`user_list` |
| 11061 | 批量踢群成员 | `room_id`、`user_list` |
| 11071 | 添加群成员为联系人 | `room_id`、`user_id` |
| 11082 | 发布群公告 | `room_id`、`content` |
| 11090 | 转让群主 | `room_id`、`user_id` |
| 11089 | 开启/关闭群邀请确认 | `room_id`、开关 |
| 11108 | 开启/关闭禁止修改群名 | `room_id`、开关 |
| 11105 | 退出群聊 | `room_id` |
| 11130 | 解散群 | `room_id` |

### 7.3 发送消息

| type | 消息 | data |
| --- | --- | --- |
| 11029 | 文本 | `{"conversation_id": "...", "content": "..."}` |
| 11069 | 群内 @ | `{"conversation_id": "...", "content": "...", "at_list": ["1688..."]}` |
| 11030 | 图片 | `{"conversation_id": "...", "file": "本地图片路径"}` |
| 11067 | 视频 | `{"conversation_id": "...", "file": "本地视频路径"}` |
| 11070 | GIF | `{"conversation_id": "...", "file": "本地 gif 路径"}` |
| 11031 | 文件 | `{"conversation_id": "...", "file": "本地文件路径"}` |
| 11162 | 小程序 | 小程序信息 |
| 11172 | 视频号 | 视频号内容 |
| 11159 | 卡片消息 | 卡片 JSON |
| 11161 | 名片 | 名片信息 |
| 11196 | 视频号直播消息 | 直播信息 |

发送结果：异步模式下 HTTP 返回「指令已发送」即代表已投递到 DLL；是否真正发出看是否收到对应消息事件（`is_pc`/`sender` 为本人）。

### 7.4 资源上传/下载

| type | 用途 | data |
| --- | --- | --- |
| 11115 | C2cCDN 上传资源（file_type=1 可传图片含 gif） | 文件路径等 |
| 11170 | C2cCDN 下载媒体文件（当收到 `cdn_type=2`） | `save_path`、`url`、`auth_key`、`aes_key`、`size` |
| 11171 | WxCDN 下载文件（当收到 `cdn_type=1`） | 同上 |

- 下载类指令支持同步取值（`getData=true`），返回 `data` 为文件信息；也可以直接用 HTTP 的 `/api/Client/DownloadWxCdnFile` 返回文件流。
- 下载下来的文件默认放在程序目录 `temp_cdn/` 下（按扩展名推断类型）。

## 8. 错误与排错

| 现象 | 原因/处理 |
| --- | --- |
| HTTP 全 404 | 路径写错；正确前缀是 `/api/Client/...` |
| WS 连不上 / 立刻断开 | 地址必须是 `ws://127.0.0.1:8061/message/`（带结尾 `/`） |
| 指令无反应 | `client` 不存在（未注入成功/未登录）；先看日志里「添加企微客户端: 客户端 N」 |
| 登录后事件里没有消息 | hook 没真正挂上：确认客户端版本是 5.0.0.6008、且以管理员运行 |
| 掉线 | 调 `GET /api/Client/ReInitSocket` 或在 GUI 里「启动新实例」 |
| 二维码不出现 | 关注 11028/11174 事件，或点 GUI「刷新二维码」 |

## 9. 与 OneBot v11 的映射（bridge 约定）

桥接层 `bridge/ling_bridge.py` 的转换规则：

| 微灵事件 | OneBot v11 |
| --- | --- |
| 11041 + `conversation_id` 以 `R:` 开头 | `post_type=message, message_type=group, group_id=R 后的数字`，`user_id=sender`，`message=[at..., text]` |
| 11041 + `conversation_id` 以 `S:` 开头 | `message_type=private`，`user_id=` `S:` 后第一段（自己发的会按 self_id 过滤） |
| 11026 / 11179 | 记录 `self_id`，建立/重建反向 WS 连接 |
| 11027 | 清空 `self_id`，断开 WS |

OneBot 动作 → 微灵指令：

| action | 指令 |
| --- | --- |
| `send_private_msg` | `11029`，`conversation_id = S:<user_id>_<self_id>` |
| `send_group_msg` | 无 `at` 时 `11029`；有 `at` 时 `11069`，`at_list` 取 at 段的 `qq`（非 `all`） |
| `send_msg` | 按 `message_type` 分派到上面两种 |
