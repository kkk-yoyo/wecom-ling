# 微灵 OneBot v11 shim 黑盒测试报告

- 任务：`task-2`（Mock OneBot v11 环境 + C# TestHost 黑盒验证 shim；本轮含扩展能力验收：回复链/出站媒体/入站新段/新动作）
- 生成时间：2026-11-13 13:37:17（本机时间）
- 实际加载的被测对象：`D:\wecom-ling\onebot\tests\bin\OneBotShim.dll`（53760 字节，SHA256 `4f0d7ea2372b0bf7109c6f6aec87d5d6e41a523313b8264b0d93ecc6f46fd714`）
- 最新编译产物：`D:\wecom-ling\onebot\output\OneBotShim.dll`（53760 字节，SHA256 `4f0d7ea2372b0bf7109c6f6aec87d5d6e41a523313b8264b0d93ecc6f46fd714`），与实际加载副本 **一致**
- shim 源码：`D:\wecom-ling\onebot\src\WxSocketClient.cs`（3183 行）
- 原程序集：`D:\wecom-ling\onebot\tests\lib\WeComSever.exe`（SHA256 `4f2954a7149b318b4a69f7d407302eaed67a2911f27084c289dd35bf0f5ba24a`）
- Mock 服务器：`mock_server.py`（aiohttp，`http://127.0.0.1:6399`，WS 路径 `/ws`，token `testtoken`，媒体夹具 `/media/{name}`）
- TestHost：`bin\TestHost.exe`（`/platform:x86`，csc 4.0，C# 5）
- 运行环境：Windows-10-10.0.19045-SP0；Python 3.12.13
- 一键复现：`powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\wecom-ling\onebot\tests\run_all.ps1"`

## 总览

| 指标 | 值 |
| --- | --- |
| 用例总数 | 62 |
| 通过 | 62 |
| 失败 | 0 |
| 通过率 | 100.0% |
| P0（核心链路） | 54/54 通过 |
| P1（补充/边界） | 8/8 通过 |

### 分组结果

| 分组 | 用例数 | 通过 | 失败 |
| --- | --- | --- | --- |
| 握手 / 生命周期 (`handshake`) | 4 | 4 | 0 |
| 事件注入与翻译 (`events`) | 16 | 16 | 0 |
| OneBot 动作处理 (`actions`) | 37 | 37 | 0 |
| 负向 / 断线重连 (`resilience`) | 2 | 2 | 0 |
| 生命周期 / 状态 (`lifecycle`) | 2 | 2 | 0 |
| 日志证据 (`log`) | 1 | 1 | 0 |

### 结论

**全部 62 个用例（P0 54 + P1 8）通过。** shim 的握手、生命周期、心跳、事件翻译、媒体登记与按需下载（`get_image/get_record/get_file` → `base64://`、`get_*_file_url` → 原始 CDN URL）、动作处理、错误分支与断线重连行为均与源码实现一致；本轮新增能力（出站 reply/face/@all 处理，出站图片/视频/文件/动图段 → 11030/11067/11031/11070 落盘发送（base64:// / http(s) / file:// / 绝对路径四种来源），入站 location/share/contact/11045 新结构，11041 保留 self at，11200 未映射消息透传与 read_receipt 过滤，`get_group_member_list`/`get_group_msg_history`）亦全部通过，未发现与任务验收标准不符的缺陷。

## 用例结果一览

| 用例 | 优先级 | 分组 | 名称 | 结果 | 耗时(ms) |
| --- | --- | --- | --- | --- | --- |
| T01 | P0 | handshake | 启动前无 self_id 不连接；注入 11026 登录后建立连接并带上 self_id | PASS | 5834 |
| T02 | P0 | handshake | 握手头：X-Client-Role / Authorization / X-Self-ID / 路径 | PASS | 1 |
| T03 | P0 | handshake | 连接建立后发送 lifecycle.connect 元事件 | PASS | 10 |
| T04 | P0 | handshake | 心跳 meta_event（interval=30s）+ 心跳周期内无周期性重连（看门狗已删除） | PASS | 18686 |
| T05 | P0 | events | 11041 群文本 → group 消息（group_id/text 段/raw_message/ling） | PASS | 101 |
| T06 | P0 | events | 11041 私聊文本 S:67890_<self> → private 消息 user_id=67890 | PASS | 89 |
| T07 | P0 | events | is_self 消息不推送（is_self=true / sender==self_id） | PASS | 3133 |
| T08 | P0 | events | 11041 at_list → at 段（mixed / 纯 at / 保留 self at） | PASS | 93 |
| T09 | P0 | events | 11042 图片 → image 段（cdn.url / md5 作为 file） | PASS | 4 |
| T10 | P0 | events | 11045 文件 → file 段 | PASS | 2 |
| T11 | P0 | events | 11123 撤回 → notice（group_recall / friend_recall） | PASS | 84 |
| T12 | P0 | events | 11072 群成员增加 → notice group_increase | PASS | 4 |
| T13 | P0 | events | 未知事件类型 99999 被忽略（不推送 ws 帧） | PASS | 1528 |
| T14 | P0 | actions | send_private_msg 文本 → 微灵命令 {client:1,type:11029,conversation_id:'S:67890_<self>'} + ok/echo/message_id | PASS | 95 |
| T15 | P0 | actions | send_group_msg 无 at → 11029 R:12345 | PASS | 8 |
| T16 | P0 | actions | send_group_msg 含 at → 11069 + data.at_list | PASS | 87 |
| T17 | P0 | actions | send_msg message_type=private → 11029 | PASS | 145 |
| T18 | P0 | actions | send_msg message_type=group → 11029 | PASS | 8 |
| T19 | P0 | actions | get_login_info → user_id/nickname | PASS | 93 |
| T20 | P0 | actions | get_status → online/good | PASS | 9 |
| T21 | P0 | actions | get_version_info → 实现信息 | PASS | 94 |
| T22 | P0 | actions | get_group_list → 11038 数据转 [{group_id,group_name}] | PASS | 91 |
| T23 | P0 | actions | get_friend_list → 11036 数据转 [{user_id,nickname}] | PASS | 93 |
| T24 | P1 | actions | get_group_info → 复用群列表 | PASS | 92 |
| T25 | P0 | actions | get_group_member_info → 11040 数据 | PASS | 11 |
| T26 | P1 | actions | get_stranger_info → 11039 数据 | PASS | 84 |
| T27 | P0 | actions | get_msg → 命中消息缓存（由 T05 注入的消息） | PASS | 37 |
| T28 | P1 | actions | get_msg 未知 message_id → failed/500 + message not found | PASS | 8 |
| T29 | P0 | actions | can_send_image → yes:true | PASS | 90 |
| T30 | P0 | actions | 已知但不支持的动作 → failed/500；未知动作 → failed/1404 | PASS | 21 |
| T31 | P0 | actions | 出站 image 段（base64 PNG）→ 先 11030 落盘（绝对路径/.png）再 11029 文本，返回 ok | PASS | 104 |
| T32 | P1 | actions | 空消息 → failed/500 empty message | PASS | 8 |
| T33 | P0 | actions | get_image 命中已登记媒体 → 按需 HTTP 下载并返回 base64:// | PASS | 179 |
| T34 | P1 | actions | 出站 face 段被忽略，文本正常发送 | PASS | 7 |
| T35 | P0 | resilience | token 不符 → Mock 401 拒绝；shim 重试不崩溃；改正后连上 | PASS | 2244 |
| T36 | P0 | resilience | Mock 主动断开 → shim 约 1s 后自动重连 | PASS | 1724 |
| T37 | P0 | lifecycle | Stop 后不再重连、状态回到未连接 | PASS | 1955 |
| T38 | P0 | log | onebot_v11.log 证据：连接/生命周期/重连/忽略事件/动作日志 | PASS | 382 |
| T39 | P1 | lifecycle | StatusChanged 事件序列：未连接/连接中/已连接/未连接 与背景色 | PASS | 0 |
| T40 | P1 | actions | 发送缺字段的负向分支：missing group_id / missing user_id | PASS | 103 |
| T41 | P1 | actions | send_group_msg 仅 at 段（无文本）→ 11069 且 content 为空 | PASS | 123 |
| T42 | P0 | actions | get_file → base64；get_group_file_url/get_private_file_url → 原始 CDN URL | PASS | 116 |
| T43 | P0 | actions | get_record 直传 HTTP file → 按需下载 base64（未登记媒体分支） | PASS | 189 |
| T44 | P0 | actions | 媒体未命中 → failed/500：media not available / file url not available | PASS | 97 |
| T45 | P0 | events | 11173 群成员邀请 → notice group_increase/invite（新分派） | PASS | 91 |
| T46 | P0 | actions | 出站 send_group_msg reply+at+text → 11069（reply 忽略、文本与 at 正常） | PASS | 45 |
| T47 | P0 | actions | 出站 at qq=all 与 face 段被忽略 → 11029 无 at_list | PASS | 6 |
| T48 | P0 | actions | 出站 video 段（base64 mp4）→ 11067（落盘绝对路径、扩展名 .mp4） | PASS | 105 |
| T49 | P0 | actions | 出站 file 段（绝对路径）→ 11031（保留 data.file_name） | PASS | 82 |
| T50 | P0 | actions | 出站 image 段 gif 变体（data.type=gif / file 后缀 .gif）→ 11070 | PASS | 89 |
| T51 | P0 | actions | 出站 record 段 → failed/500（暂不支持发送语音），且不下发微灵命令 | PASS | 80 |
| T52 | P0 | events | 入站 11046 位置 → location 段（lat/lon/title/content） | PASS | 77 |
| T53 | P0 | events | 入站 11047 链接 → share 段（url/title/content/image） | PASS | 93 |
| T54 | P0 | events | 入站 11050 名片 → contact 段（type=qq，id 为 JSON 数字） | PASS | 95 |
| T55 | P0 | events | 入站 11048 动图 → image 段（file=name，url=cdn url） | PASS | 88 |
| T56 | P0 | actions | get_group_member_list → 11040 查询带顶层 getData:true，返回成员数组 | PASS | 11 |
| T57 | P0 | actions | get_group_msg_history → ok 且返回空数组（不回调微灵 handler） | PASS | 82 |
| T58 | P0 | events | 入站 11200 未映射消息 → message 事件（content/at_list 透传） | PASS | 4 |
| T59 | P0 | events | 入站 11200 已读上报 → 不推送（message_type / event_desc 两种判定） | PASS | 3066 |
| T60 | P0 | actions | 出站 image 段（http(s) URL）→ 经 Mock 下载落盘后发 11030 | PASS | 119 |
| T61 | P0 | actions | 出站 image 段（file:// 前缀 + 绝对路径）→ 11030 | PASS | 90 |
| T62 | P0 | actions | 出站未知消息段（json）→ failed/500 提示不支持，不下发微灵命令 | PASS | 12 |

## 证据详情（按用例）

### T01 [P0/handshake] 启动前无 self_id 不连接；注入 11026 登录后建立连接并带上 self_id — PASS

- 断言/证据（13 条）：
  - OK  无 self_id 时 StatusText=连接中 == 连接中
  - OK  无 self_id 时 StatusBackground=#E6A23C == #E6A23C
  - OK  self_id 为空时 1.2s 内不建立 WS 连接（NetLoop 等待登录）
  - OK  无 self_id 时 IsConnected 仍为 false
  - OK  构造后 StatusText == 未连接
  - OK  构造后 StatusBackground == #909399
  - OK  构造后 IsConnected==false
  - 注入微灵事件 type=11026 TrySendEvent=false
  - 启动前注入 11026 设置 self_id（未连接时 TrySendEvent 返回 false，但 CaptureState 已生效）seq=0
  - OK  连接后 StatusText=已连接 == 已连接
  - OK  连接后 StatusBackground=#22AA22 == #22AA22
  - OK  Mock 记录到连接
  - OK  11026 后连接的 X-Self-ID 使用 robot_id/data.user_id == 77880001

### T02 [P0/handshake] 握手头：X-Client-Role / Authorization / X-Self-ID / 路径 — PASS

- 断言/证据（5 条）：
  - OK  存在连接记录
  - OK  WS 路径 == /ws
  - OK  X-Client-Role 头 == Universal
  - OK  Authorization 头 == Bearer testtoken
  - OK  X-Self-ID 头 == 77880001

### T03 [P0/handshake] 连接建立后发送 lifecycle.connect 元事件 — PASS

- 断言/证据（3 条）：
  - OK  lifecycle.sub_type == connect
  - OK  lifecycle.self_id == 77880001 (整型)
  - OK  lifecycle 含 time (存在 time)

### T04 [P0/handshake] 心跳 meta_event（interval=30s）+ 心跳周期内无周期性重连（看门狗已删除） — PASS

- 断言/证据（6 条）：
  - OK  heartbeat.interval == 30000 (整型)
  - OK  heartbeat.status.online == true
  - OK  heartbeat.status.good == true
  - OK  heartbeat.self_id == 77880001 (整型)
  - OK  心跳等待期间没有周期性重连（总连接数仍为 1）
  - OK  心跳周期后连接仍存活

### T05 [P0/events] 11041 群文本 → group 消息（group_id/text 段/raw_message/ling） — PASS

- 断言/证据（21 条）：
  - 注入微灵事件 type=11041 TrySendEvent=true
  - OK  post_type == message
  - OK  message_type == group
  - OK  sub_type == normal
  - OK  group_id == 12345 (整型)
  - OK  群消息无顶层 user_id (无 user_id)
  - OK  message_id（server_id 转数字） == 55550001 (整型)
  - OK  self_id == 77880001 (整型)
  - OK  time（秒值透传） == 1710000000 (整型)
  - OK  sender.nickname == 张三
  - OK  sender.user_id == 11111 (整型)
  - OK  sender.role == member
  - OK  font == 0 (整型)
  - OK  message 仅 1 个段
  - OK  段类型 text == text
  - OK  段文本 == 你好，世界
  - OK  raw_message == 你好，世界
  - OK  顶层 ling 存在
  - OK  ling.type 保留原始事件类型 == 11041 (整型)
  - OK  ling.data.conversation_id == R:12345
  - OK  ling 中剥离 data.payload_base64 (无 payload_base64)

### T06 [P0/events] 11041 私聊文本 S:67890_<self> → private 消息 user_id=67890 — PASS

- 断言/证据（9 条）：
  - 注入微灵事件 type=11041 TrySendEvent=true
  - OK  message_type == private
  - OK  sub_type == friend
  - OK  user_id（取最后一个 _ 之前部分） == 67890 (整型)
  - OK  私聊消息无 group_id (无 group_id)
  - OK  毫秒 send_time 被 /1000 == 1710000123 (整型)
  - OK  message 仅 1 个段
  - OK  段文本 == 私聊内容
  - OK  sender.nickname == 李四

### T07 [P0/events] is_self 消息不推送（is_self=true / sender==self_id） — PASS

- 断言/证据（4 条）：
  - 注入微灵事件 type=11041 TrySendEvent=false
  - OK  1500ms 内未收到 is_self=true 的消息事件
  - 注入微灵事件 type=11041 TrySendEvent=false
  - OK  1500ms 内未收到 sender==self_id 的消息事件

### T08 [P0/events] 11041 at_list → at 段（mixed / 纯 at / 保留 self at） — PASS

- 断言/证据（17 条）：
  - 注入微灵事件 type=11041 TrySendEvent=true
  - OK  mixed: 3 个 at + 1 个 text 段，共 4 段
  - OK  段0 类型 at == at
  - OK  段0 qq (字符串条目) == 22222
  - OK  段0 qq 的 JSON 类型为 string == String
  - OK  段1 类型 at == at
  - OK  段1 qq (JObject user_id) == 33333
  - OK  段2 类型 at (self) == at
  - OK  段2 qq (self 也保留) == 77880001
  - OK  段3 类型 text == text
  - OK  段3 文本 == 大家好
  - OK  raw_message 拼接 @ 与文本 == @22222@33333@77880001大家好
  - 注入微灵事件 type=11041 TrySendEvent=true
  - OK  纯 at: 仅 1 个 at 段（空 content 不生成 text 段）
  - OK  纯 at 段类型 == at
  - OK  纯 at qq == 44444
  - OK  纯 at raw_message == @44444

### T09 [P0/events] 11042 图片 → image 段（cdn.url / md5 作为 file） — PASS

- 断言/证据（5 条）：
  - 注入微灵事件 type=11042 TrySendEvent=true
  - OK  图片消息 1 个段
  - OK  段类型 image == image
  - OK  image.data.file 取 cdn.md5 == abc123
  - OK  image.data.url 取 cdn.url == https://cdn.example.com/img/a.png

### T10 [P0/events] 11045 文件 → file 段 — PASS

- 断言/证据（7 条）：
  - 注入微灵事件 type=11045 TrySendEvent=true
  - OK  文件消息 1 个段
  - OK  段类型 file == file
  - OK  file.data.name 取 cdn.file_name（AstrBot File 需要） == 报告.pdf
  - OK  file.data.file 取 cdn.url == https://cdn.example.com/f/报告.pdf
  - OK  file.data.url == https://cdn.example.com/f/报告.pdf
  - OK  file 段不再输出 file_id（新结构 {name,file,url}） (无 file_id)

### T11 [P0/events] 11123 撤回 → notice（group_recall / friend_recall） — PASS

- 断言/证据（9 条）：
  - 注入微灵事件 type=11123 TrySendEvent=true
  - OK  群撤回 group_id == 12345 (整型)
  - OK  群撤回 message_id == 55550001 (整型)
  - OK  群撤回 operator_id == 11111 (整型)
  - OK  群撤回含 ling
  - 注入微灵事件 type=11123 TrySendEvent=true
  - OK  私聊撤回 user_id == 67890 (整型)
  - OK  私聊撤回 message_id == 55550002 (整型)
  - OK  私聊撤回含 ling

### T12 [P0/events] 11072 群成员增加 → notice group_increase — PASS

- 断言/证据（6 条）：
  - 注入微灵事件 type=11072 TrySendEvent=true
  - OK  sub_type invite == invite
  - OK  group_id == 12345 (整型)
  - OK  user_id=member_list[0].user_id == 99999 (整型)
  - OK  operator_id == 11111 (整型)
  - OK  notice 含 ling

### T13 [P0/events] 未知事件类型 99999 被忽略（不推送 ws 帧） — PASS

- 断言/证据（2 条）：
  - 注入微灵事件 type=99999 TrySendEvent=false
  - OK  1500ms 内未收到 未知事件产生的任何非心跳帧

### T14 [P0/actions] send_private_msg 文本 → 微灵命令 {client:1,type:11029,conversation_id:'S:67890_<self>'} + ok/echo/message_id — PASS

- 断言/证据（10 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":"私聊回复"} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11029-1"},"echo":"echo-T14-1"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  echo 原样返回
  - OK  含 data (存在 data)
  - OK  message_id 来自 handler 的 server_id: srv-11029-1
  - OK  命令 client=1（数字） == 1 (整型)
  - OK  命令 type == 11029 (整型)
  - OK  命令 conversation_id == S:67890_77880001
  - OK  命令 content == 私聊回复

### T15 [P0/actions] send_group_msg 无 at → 11029 R:12345 — PASS

- 断言/证据（5 条）：
  - ACTION send_group_msg params={"group_id":"12345","message":"群回复"} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11029-2"},"echo":"echo-T15-2"}
  - OK  status == ok
  - OK  命令 conversation_id == R:12345
  - OK  命令 content == 群回复
  - OK  无 at 时不带 at_list (无 at_list)

### T16 [P0/actions] send_group_msg 含 at → 11069 + data.at_list — PASS

- 断言/证据（7 条）：
  - ACTION send_group_msg params={"group_id":"12345","message":[{"type":"text","data":{"text":"Hi "}},{"type":"at","data":{"qq":"22222"}},{"type":"at","data":{"qq":"33333"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11069-3"},"echo":"echo-T16-3"}
  - OK  status == ok
  - OK  命令 conversation_id == R:12345
  - OK  命令 content（at 段不产生文本） == Hi 
  - OK  at_list 含 2 个用户
  - OK  at_list[0] == 22222 (整型)
  - OK  at_list[1] == 33333 (整型)

### T17 [P0/actions] send_msg message_type=private → 11029 — PASS

- 断言/证据（4 条）：
  - ACTION send_msg params={"message_type":"private","user_id":"67890","message":"send_msg私聊"} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11029-4"},"echo":"echo-T17-4"}
  - OK  status == ok
  - OK  命令 conversation_id == S:67890_77880001
  - OK  命令 content == send_msg私聊

### T18 [P0/actions] send_msg message_type=group → 11029 — PASS

- 断言/证据（3 条）：
  - ACTION send_msg params={"message_type":"group","group_id":"12345","message":"send_msg群"} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11029-5"},"echo":"echo-T18-5"}
  - OK  status == ok
  - OK  命令 conversation_id == R:12345

### T19 [P0/actions] get_login_info → user_id/nickname — PASS

- 断言/证据（4 条）：
  - ACTION get_login_info params={} -> {"status":"ok","retcode":0,"msg":"","data":{"user_id":77880001,"nickname":"机器人昵称"},"echo":"echo-T19-6"}
  - OK  status == ok
  - OK  user_id == 77880001 (整型)
  - OK  nickname 来自 11026 data.username == 机器人昵称

### T20 [P0/actions] get_status → online/good — PASS

- 断言/证据（4 条）：
  - ACTION get_status params={} -> {"status":"ok","retcode":0,"msg":"","data":{"online":true,"good":true},"echo":"echo-T20-7"}
  - OK  status == ok
  - OK  online == true
  - OK  good == true

### T21 [P0/actions] get_version_info → 实现信息 — PASS

- 断言/证据（6 条）：
  - ACTION get_version_info params={} -> {"status":"ok","retcode":0,"msg":"","data":{"app_name":"wecom-ling-onebot","app_version":"1.11.0","protocol_version":"v11","app_full_name":"微灵 OneBot v11 改造版"},"echo":"echo-T21-8"}
  - OK  status == ok
  - OK  app_name == wecom-ling-onebot
  - OK  app_version == 1.11.0
  - OK  protocol_version == v11
  - OK  app_full_name == 微灵 OneBot v11 改造版

### T22 [P0/actions] get_group_list → 11038 数据转 [{group_id,group_name}] — PASS

- 断言/证据（7 条）：
  - ACTION get_group_list params={} -> {"status":"ok","retcode":0,"msg":"","data":[{"group_id":12345,"group_name":"测试群A"},{"group_id":77777,"group_name":"测试群B"}],"echo":"echo-T22-9"}
  - OK  status == ok
  - OK  群列表 2 项
  - OK  群1 group_id（R: 前缀已剥离） == 12345 (整型)
  - OK  群1 group_name == 测试群A
  - OK  群2 group_id == 77777 (整型)
  - OK  群2 group_name == 测试群B

### T23 [P0/actions] get_friend_list → 11036 数据转 [{user_id,nickname}] — PASS

- 断言/证据（7 条）：
  - ACTION get_friend_list params={} -> {"status":"ok","retcode":0,"msg":"","data":[{"user_id":67890,"nickname":"好友甲","remark":""},{"user_id":12345,"nickname":"好友乙","remark":""}],"echo":"echo-T23-10"}
  - OK  status == ok
  - OK  好友列表 2 项
  - OK  好友1 user_id == 67890 (整型)
  - OK  好友1 nickname == 好友甲
  - OK  好友2 user_id 由 conversation_id 解析 == 12345 (整型)
  - OK  好友2 nickname（remark 回退） == 好友乙

### T24 [P1/actions] get_group_info → 复用群列表 — PASS

- 断言/证据（4 条）：
  - ACTION get_group_info params={"group_id":"12345"} -> {"status":"ok","retcode":0,"msg":"","data":{"group_id":12345,"group_name":"测试群A"},"echo":"echo-T24-11"}
  - OK  status == ok
  - OK  group_id == 12345 (整型)
  - OK  group_name == 测试群A

### T25 [P0/actions] get_group_member_info → 11040 数据 — PASS

- 断言/证据（5 条）：
  - ACTION get_group_member_info params={"group_id":"12345","user_id":"22222"} -> {"status":"ok","retcode":0,"msg":"","data":{"user_id":22222,"nickname":"成员甲","card":"成员甲","sex":"unknown","age":0,"role":"member"},"echo":"echo-T25-12"}
  - OK  status == ok
  - OK  11040 查询 conversation_id == R:12345
  - OK  成员 user_id == 22222 (整型)
  - OK  成员 nickname == 成员甲

### T26 [P1/actions] get_stranger_info → 11039 数据 — PASS

- 断言/证据（4 条）：
  - ACTION get_stranger_info params={"user_id":"44444"} -> {"status":"ok","retcode":0,"msg":"","data":{"user_id":44444,"nickname":"陌生人甲","sex":"unknown","age":0},"echo":"echo-T26-13"}
  - OK  status == ok
  - OK  user_id == 44444 (整型)
  - OK  nickname == 陌生人甲

### T27 [P0/actions] get_msg → 命中消息缓存（由 T05 注入的消息） — PASS

- 断言/证据（5 条）：
  - ACTION get_msg params={"message_id":"55550001"} -> {"status":"ok","retcode":0,"msg":"","data":{"time":1710000000,"self_id":77880001,"post_type":"message","message_type":"group","sub_type":"normal","message_id":55550001,"message":[{"type":"text","data":{"text":"你好，世界"}}],"raw_message":"你好，世界","font":0,"sender":{"user_id":11111,"nickname":"张三","card":"张三","role":"member"},"group_id":12345,"ling":{"type":11041,"client":1,"data":{"conversation_id":"R:12345","sender":"11111","sender_name":"张三","receiver":"12345","content":"你好，世界","send_time":1710000000,"server_id":"55550001","local_id":"l1","is_sel …(截断)
  - OK  status == ok
  - OK  缓存消息 message_id == 55550001 (整型)
  - OK  缓存消息 post_type == message
  - OK  缓存消息含 ling

### T28 [P1/actions] get_msg 未知 message_id → failed/500 + message not found — PASS

- 断言/证据（4 条）：
  - ACTION get_msg params={"message_id":"999999999"} -> {"status":"failed","retcode":500,"msg":"message not found: 999999999","data":null,"echo":"echo-T28-15"}
  - OK  status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息 (找到 "message not found: 999999999")

### T29 [P0/actions] can_send_image → yes:true — PASS

- 断言/证据（3 条）：
  - ACTION can_send_image params={} -> {"status":"ok","retcode":0,"msg":"","data":{"yes":true},"echo":"echo-T29-16"}
  - OK  status == ok
  - OK  yes == true

### T30 [P0/actions] 已知但不支持的动作 → failed/500；未知动作 → failed/1404 — PASS

- 断言/证据（8 条）：
  - ACTION set_group_name params={"group_id":"12345","group_name":"x"} -> {"status":"failed","retcode":500,"msg":"action not supported by 微灵 OneBot shim: set_group_name","data":null,"echo":"echo-T30a-17"}
  - OK  set_group_name status == failed
  - OK  set_group_name retcode == 500 (整型)
  - OK  set_group_name 错误信息 (找到 "action not supported by 微灵 OneBot shim: set_group_name")
  - ACTION nonexistent_action_xyz params={} -> {"status":"failed","retcode":1404,"msg":"unsupported action: nonexistent_action_xyz","data":null,"echo":"echo-T30b-18"}
  - OK  未知动作 status == failed
  - OK  未知动作 retcode == 1404 (整型)
  - OK  未知动作错误信息 == unsupported action: nonexistent_action_xyz

### T31 [P0/actions] 出站 image 段（base64 PNG）→ 先 11030 落盘（绝对路径/.png）再 11029 文本，返回 ok — PASS

- 断言/证据（15 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"image","data":{"file":"base64://iVBORw0KGgpNT0NLLVBORy0AAQIDBAUGBwgJCgsMDQ4P"}},{"type":"text","data":{"text":"图片说明"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11029-7"},"echo":"echo-T31-19"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  echo 原样回传 == echo-T31-19
  - OK  11030 conversation_id == S:67890_77880001
  - OK  11030 data.file 为绝对路径
  - OK  11030 data.file 指向已存在文件
  - OK  11030 data.file 扩展名 .png
  - OK  11030 落盘内容与 base64 一致 长度 (33 == 期望 33)
  - OK  11030 落盘内容与 base64 一致 字节完全一致（33 bytes）
  - OK  11029 conversation_id == S:67890_77880001
  - OK  11029 content == 图片说明
  - OK  命令顺序：先 11030 媒体后 11029 文本
  - OK  共 2 条命令（图片 + 文本）
  - OK  message_id 来自最后一条文本命令结果

### T32 [P1/actions] 空消息 → failed/500 empty message — PASS

- 断言/证据（4 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":""} -> {"status":"failed","retcode":500,"msg":"empty message","data":null,"echo":"echo-T32-20"}
  - OK  status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息 (找到 "empty message")

### T33 [P0/actions] get_image 命中已登记媒体 → 按需 HTTP 下载并返回 base64:// — PASS

- 断言/证据（12 条）：
  - 注入微灵事件 type=11042 TrySendEvent=true
  - OK  图片消息 1 个 image 段
  - OK  image 段 file 取 cdn.file_name == pic.png
  - OK  image 段 url == http://127.0.0.1:6399/media/pic.png
  - ACTION get_image params={"file":"pic.png"} -> {"status":"ok","retcode":0,"msg":"","data":{"file":"base64://iVBORw0KGgpNT0NLLVBORy0AAQIDBAUGBwgJCgsMDQ4P","file_name":"pic.png"},"echo":"echo-T33-21"}
  - OK  get_image status == ok
  - OK  retcode == 0 (整型)
  - OK  data.file 形如 base64://...
  - OK  get_image 下载内容 长度 (33 == 期望 33)
  - OK  get_image 下载内容 字节完全一致（33 bytes）
  - OK  data.file_name 保留 cdn.file_name == pic.png
  - OK  Mock 记录到 shim 的按需 HTTP 下载请求

### T34 [P1/actions] 出站 face 段被忽略，文本正常发送 — PASS

- 断言/证据（3 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"text","data":{"text":"表情"}},{"type":"face","data":{"id":"14"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11029-8"},"echo":"echo-T34-28"}
  - OK  status == ok
  - OK  content 仅文本部分 == 表情

### T35 [P0/resilience] token 不符 → Mock 401 拒绝；shim 重试不崩溃；改正后连上 — PASS

- 断言/证据（7 条）：
  - 注入微灵事件 type=11026 TrySendEvent=false
  - OK  被拒绝握手的 Authorization == Bearer badtoken
  - OK  token 错误时 IsConnected==false
  - OK  token 错误时 StatusText=连接中（重试中） == 连接中
  - OK  token 错误时 StatusBackground=#E6A23C
  - OK  改正 token 后 StatusText=已连接 == 已连接
  - OK  Mock 侧存在活跃连接

### T36 [P0/resilience] Mock 主动断开 → shim 约 1s 后自动重连 — PASS

- 断言/证据（6 条）：
  - OK  断开前存在活跃连接
  - OK  mock close_all 成功
  - 断开→重连完成耗时约 1718 ms
  - OK  重连耗时 < 6s（ReconnectMinMs=1000 + 握手）
  - OK  Mock 记录到新的连接（重连发生）
  - OK  重连后 StatusText=已连接 == 已连接

### T37 [P0/lifecycle] Stop 后不再重连、状态回到未连接 — PASS

- 断言/证据（4 条）：
  - OK  Stop 后 IsConnected==false
  - OK  Stop 后 StatusText=未连接 == 未连接
  - OK  Stop 后 StatusBackground=#909399 == #909399
  - OK  Stop 后 1.8s 内没有新的重连连接

### T38 [P0/log] onebot_v11.log 证据：连接/生命周期/重连/忽略事件/动作日志 — PASS

- 断言/证据（13 条）：
  - OK  日志文件存在: D:\wecom-ling\onebot\tests\bin\onebot_v11.log
  - 日志大小 22270 字节
  - OK  channel 日志前缀 (找到 "[OneBot:wecom-test]")
  - OK  连接成功日志 (找到 "connected: ws://127.0.0.1:6399/ws")
  - OK  lifecycle 入队日志 (找到 "lifecycle connect queued")
  - OK  服务端断开日志 (找到 "server closed connection")
  - OK  重连延迟日志 (找到 "reconnect in 1000 ms")
  - OK  入站动作日志 (找到 "<- action send_private_msg")
  - OK  未映射事件日志（99999） (找到 "不映射为 OneBot 事件")
  - OK  坏 token shim 的日志前缀 (找到 "[OneBot:wecom-bad-token]")
  - OK  连接失败日志（token 401） (找到 "connection failed:")
  - OK  日志中没有 watchdog 静默断连记录（新 shim 已彻底删除看门狗）
  - OK  日志中没有 120s 静默强制关链记录

### T39 [P1/lifecycle] StatusChanged 事件序列：未连接/连接中/已连接/未连接 与背景色 — PASS

- 断言/证据（4 条）：
  - 状态序列:<br>连接中\|#E6A23C\|False<br>已连接\|#22AA22\|True<br>连接中\|#E6A23C\|False<br>已连接\|#22AA22\|True<br>未连接\|#909399\|False<br>未连接\|#909399\|False
  - OK  启动时 连接中/#E6A23C (找到 "连接中\|#E6A23C\|False")
  - OK  连接后 已连接/#22AA22 (找到 "已连接\|#22AA22\|True")
  - OK  Stop 后 未连接/#909399 (找到 "未连接\|#909399\|False")

### T40 [P1/actions] 发送缺字段的负向分支：missing group_id / missing user_id — PASS

- 断言/证据（8 条）：
  - ACTION send_group_msg params={"message":"x"} -> {"status":"failed","retcode":500,"msg":"missing group_id","data":null,"echo":"echo-T40a-29"}
  - OK  send_group_msg 缺 group_id status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息 (找到 "missing group_id")
  - ACTION send_private_msg params={"message":"x"} -> {"status":"failed","retcode":500,"msg":"missing user_id","data":null,"echo":"echo-T40b-30"}
  - OK  send_private_msg 缺 user_id status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息 (找到 "missing user_id")

### T41 [P1/actions] send_group_msg 仅 at 段（无文本）→ 11069 且 content 为空 — PASS

- 断言/证据（5 条）：
  - ACTION send_group_msg params={"group_id":"12345","message":[{"type":"at","data":{"qq":"22222"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11069-9"},"echo":"echo-T41-31"}
  - OK  status == ok
  - OK  content 为空字符串 == 
  - OK  at_list 含 1 个用户
  - OK  at_list[0] == 22222 (整型)

### T42 [P0/actions] get_file → base64；get_group_file_url/get_private_file_url → 原始 CDN URL — PASS

- 断言/证据（17 条）：
  - 注入微灵事件 type=11045 TrySendEvent=true
  - OK  文件消息 1 个 file 段
  - OK  file 段 name 取 cdn.file_name == 报告.txt
  - OK  file 段 file 取 cdn.url == http://127.0.0.1:6399/media/doc.txt
  - OK  file 段 url == http://127.0.0.1:6399/media/doc.txt
  - ACTION get_file params={"file_id":"55550201"} -> {"status":"ok","retcode":0,"msg":"","data":{"file":"base64://TU9DSy1GSUxFLUNPTlRFTlQtMTIzNDU2Nzg5MAo=","file_name":"报告.txt"},"echo":"echo-T42get-22"}
  - OK  get_file status == ok
  - OK  get_file 下载内容 长度 (29 == 期望 29)
  - OK  get_file 下载内容 字节完全一致（29 bytes）
  - OK  get_file file_name == 报告.txt
  - ACTION get_group_file_url params={"file_id":"55550201"} -> {"status":"ok","retcode":0,"msg":"","data":{"url":"http://127.0.0.1:6399/media/doc.txt","file_name":"报告.txt"},"echo":"echo-T42groupurl-23"}
  - OK  get_group_file_url status == ok
  - OK  get_group_file_url 返回原始 CDN url（未缓存字节） == http://127.0.0.1:6399/media/doc.txt
  - OK  get_group_file_url file_name == 报告.txt
  - ACTION get_private_file_url params={"file_id":"55550201"} -> {"status":"ok","retcode":0,"msg":"","data":{"url":"http://127.0.0.1:6399/media/doc.txt","file_name":"报告.txt"},"echo":"echo-T42privurl-24"}
  - OK  get_private_file_url status == ok
  - OK  get_private_file_url 返回原始 CDN url == http://127.0.0.1:6399/media/doc.txt

### T43 [P0/actions] get_record 直传 HTTP file → 按需下载 base64（未登记媒体分支） — PASS

- 断言/证据（6 条）：
  - ACTION get_record params={"file":"http://127.0.0.1:6399/media/rec.silk"} -> {"status":"ok","retcode":0,"msg":"","data":{"file":"base64://TU9DSy1SRUNPUkQtREFUQQ==","file_name":"file"},"echo":"echo-T43-25"}
  - OK  get_record status == ok
  - OK  get_record 下载内容 长度 (16 == 期望 16)
  - OK  get_record 下载内容 字节完全一致（16 bytes）
  - OK  未登记媒体的 file_name 默认 file == file
  - OK  Mock 记录到 get_record 的下载请求

### T44 [P0/actions] 媒体未命中 → failed/500：media not available / file url not available — PASS

- 断言/证据（8 条）：
  - ACTION get_image params={"file":"no-such-media"} -> {"status":"failed","retcode":500,"msg":"media not available: no-such-media","data":null,"echo":"echo-T44a-26"}
  - OK  get_image 未命中 status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息 (找到 "media not available: no-such-media")
  - ACTION get_group_file_url params={"file_id":"no-such-file"} -> {"status":"failed","retcode":500,"msg":"file url not available (需要消息内自带 cdn.url): no-such-file","data":null,"echo":"echo-T44b-27"}
  - OK  get_group_file_url 未命中 status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息 (找到 "file url not available (需要消息内自带 cdn.url)")

### T45 [P0/events] 11173 群成员邀请 → notice group_increase/invite（新分派） — PASS

- 断言/证据（7 条）：
  - 注入微灵事件 type=11173 TrySendEvent=true
  - OK  sub_type invite == invite
  - OK  group_id == 12345 (整型)
  - OK  user_id == 55555 (整型)
  - OK  operator_id == 11111 (整型)
  - OK  11173 notice 含 ling
  - OK  ling.type == 11173

### T46 [P0/actions] 出站 send_group_msg reply+at+text → 11069（reply 忽略、文本与 at 正常） — PASS

- 断言/证据（10 条）：
  - ACTION send_group_msg params={"group_id":"12345","message":[{"type":"reply","data":{"id":"srv-11029-1"}},{"type":"at","data":{"qq":"22222"}},{"type":"text","data":{"text":"回复你"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11069-10"},"echo":"echo-T46-32"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  echo 原样回传 == echo-T46-32
  - OK  conversation_id == R:12345
  - OK  content（reply 段被忽略，正文保留） == 回复你
  - OK  at_list 含 1 个用户
  - OK  at_list[0] == 22222 (整型)
  - OK  reply 段不产生额外命令（仅 1 条 11069）
  - OK  message_id 来自 11069 结果

### T47 [P0/actions] 出站 at qq=all 与 face 段被忽略 → 11029 无 at_list — PASS

- 断言/证据（7 条）：
  - ACTION send_group_msg params={"group_id":"12345","message":[{"type":"text","data":{"text":"看@全体"}},{"type":"at","data":{"qq":"all"}},{"type":"face","data":{"id":"14"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11029-11"},"echo":"echo-T47-33"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  conversation_id == R:12345
  - OK  content 仅文本（@全体 不作为 at_list） == 看@全体
  - OK  @all 忽略后降级为 11029，无 at_list (无 at_list)
  - OK  仅 1 条命令（face/@all 不产生命令）

### T48 [P0/actions] 出站 video 段（base64 mp4）→ 11067（落盘绝对路径、扩展名 .mp4） — PASS

- 断言/证据（11 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"video","data":{"file":"base64://AAAAGGZ0eXBtcDQyDQ4KCw=="}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11067-12"},"echo":"echo-T48-34"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  11067 conversation_id == S:67890_77880001
  - OK  11067 data.file 为绝对路径
  - OK  11067 data.file 指向已存在文件
  - OK  落盘扩展名为 .mp4
  - OK  11067 媒体文件内容与 base64 一致 长度 (16 == 期望 16)
  - OK  11067 媒体文件内容与 base64 一致 字节完全一致（16 bytes）
  - OK  仅 1 条媒体命令（无文本段）
  - OK  message_id 来自 11067 结果

### T49 [P0/actions] 出站 file 段（绝对路径）→ 11031（保留 data.file_name） — PASS

- 断言/证据（11 条）：
  - 临时文件: D:\wecom-ling\onebot\tests\bin\tmp_upload_source.txt (30 bytes)
  - ACTION send_group_msg params={"group_id":"12345","message":[{"type":"file","data":{"name":"报告.txt","file":"D:\\wecom-ling\\onebot\\tests\\bin\\tmp_upload_source.txt"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11031-13"},"echo":"echo-T49-35"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  11031 conversation_id == R:12345
  - OK  11031 data.file 为绝对路径
  - OK  11031 data.file 指向已存在文件
  - OK  落盘扩展名为 .txt
  - OK  11031 媒体文件内容与源文件一致 长度 (30 == 期望 30)
  - OK  11031 媒体文件内容与源文件一致 字节完全一致（30 bytes）
  - OK  11031 data.file_name 保留段中的 name == 报告.txt

### T50 [P0/actions] 出站 image 段 gif 变体（data.type=gif / file 后缀 .gif）→ 11070 — PASS

- 断言/证据（10 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"image","data":{"file":"base64://R0lGODlhAAAAAAAAAAAAAAAAAAAAAA==","type":"gif"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11070-14"},"echo":"echo-T50a-36"}
  - OK  data.type=gif status == ok
  - OK  11070 data.file 存在
  - OK  11070 落盘扩展名 .gif
  - OK  11070 媒体文件内容与 base64 一致 长度 (22 == 期望 22)
  - OK  11070 媒体文件内容与 base64 一致 字节完全一致（22 bytes）
  - 临时文件: D:\wecom-ling\onebot\tests\bin\tmp_anim.gif (22 bytes)
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"image","data":{"file":"D:\\wecom-ling\\onebot\\tests\\bin\\tmp_anim.gif"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11070-15"},"echo":"echo-T50b-37"}
  - OK  file 后缀 .gif status == ok
  - OK  11070(.gif 路径) 落盘扩展名 .gif

### T51 [P0/actions] 出站 record 段 → failed/500（暂不支持发送语音），且不下发微灵命令 — PASS

- 断言/证据（5 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"record","data":{"file":"https://cdn.example.com/a.silk"}}]} -> {"status":"failed","retcode":500,"msg":"暂不支持发送语音：微灵未提供语音发送指令（可改用文本或文件）","data":null,"echo":"echo-T51-38"}
  - OK  status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息 (找到 "暂不支持发送语音")
  - OK  record 段在解析阶段被拒绝，未下发微灵命令

### T52 [P0/events] 入站 11046 位置 → location 段（lat/lon/title/content） — PASS

- 断言/证据（7 条）：
  - 注入微灵事件 type=11046 TrySendEvent=true
  - OK  位置消息 1 个段
  - OK  段类型 location == location
  - OK  location.data.lat 取 latitude == 31.2304
  - OK  location.data.lon 取 longitude == 121.4737
  - OK  location.data.title == 公司
  - OK  location.data.content 取 address == 上海市黄浦区某路 1 号

### T53 [P0/events] 入站 11047 链接 → share 段（url/title/content/image） — PASS

- 断言/证据（11 条）：
  - 注入微灵事件 type=11047 TrySendEvent=true
  - OK  链接消息 1 个段
  - OK  段类型 share == share
  - OK  share.data.url == https://news.example.com/a/1
  - OK  share.data.title == 新闻标题
  - OK  share.data.content 取 desc == 新闻摘要
  - OK  share.data.image 取 image_url == https://cdn.example.com/img/thumb.png
  - 注入微灵事件 type=11047 TrySendEvent=true
  - OK  空 url/title 降级为 1 个段
  - OK  降级段类型 text == text
  - OK  降级文本内容取 content == [链接] 链接描述文本

### T54 [P0/events] 入站 11050 名片 → contact 段（type=qq，id 为 JSON 数字） — PASS

- 断言/证据（5 条）：
  - 注入微灵事件 type=11050 TrySendEvent=true
  - OK  名片消息 1 个段
  - OK  段类型 contact == contact
  - OK  contact.data.type=qq == qq
  - OK  contact.data.id 为 JSON 数字 == 12345678 (整型)

### T55 [P0/events] 入站 11048 动图 → image 段（file=name，url=cdn url） — PASS

- 断言/证据（5 条）：
  - 注入微灵事件 type=11048 TrySendEvent=true
  - OK  动图消息 1 个段
  - OK  段类型 image == image
  - OK  image.data.file 取 name == anim.gif
  - OK  image.data.url == http://127.0.0.1:6399/media/anim.gif

### T56 [P0/actions] get_group_member_list → 11040 查询带顶层 getData:true，返回成员数组 — PASS

- 断言/证据（11 条）：
  - ACTION get_group_member_list params={"group_id":"12345"} -> {"status":"ok","retcode":0,"msg":"","data":[{"user_id":22222,"nickname":"成员甲","card":"成员甲","sex":"unknown","age":0,"role":"member"},{"user_id":33333,"nickname":"成员乙","card":"群昵称乙","sex":"unknown","age":0,"role":"member"}],"echo":"echo-T56-39"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  命令顶层 getData=true == true
  - OK  命令 type == 11040 (整型)
  - OK  查询 conversation_id == R:12345
  - OK  返回 2 个成员
  - OK  成员0 user_id == 22222 (整型)
  - OK  成员0 nickname 取 name == 成员甲
  - OK  成员1 user_id == 33333 (整型)
  - OK  成员1 card 取 group_nickname == 群昵称乙

### T57 [P0/actions] get_group_msg_history → ok 且返回空数组（不回调微灵 handler） — PASS

- 断言/证据（5 条）：
  - ACTION get_group_msg_history params={"group_id":"12345","count":20} -> {"status":"ok","retcode":0,"msg":"","data":[],"echo":"echo-T57-40"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  data 为空数组
  - OK  未下发微灵命令

### T58 [P0/events] 入站 11200 未映射消息 → message 事件（content/at_list 透传） — PASS

- 断言/证据（10 条）：
  - 注入微灵事件 type=11200 TrySendEvent=true
  - OK  message_type == group
  - OK  group_id == 12345 (整型)
  - OK  11200 消息含 at + text 共 2 段
  - OK  段0 类型 at == at
  - OK  at 段 qq == 22222
  - OK  段1 类型 text == text
  - OK  text 段内容取 content == 未映射消息内容
  - OK  raw_message 拼接 == @22222未映射消息内容
  - OK  ling.type 保留原始 11200 == 11200 (整型)

### T59 [P0/events] 入站 11200 已读上报 → 不推送（message_type / event_desc 两种判定） — PASS

- 断言/证据（4 条）：
  - 注入微灵事件 type=11200 TrySendEvent=false
  - OK  1500ms 内未收到 message_type=read_receipt 的 11200
  - 注入微灵事件 type=11200 TrySendEvent=false
  - OK  1500ms 内未收到 event_desc=已读上报 的 11200

### T60 [P0/actions] 出站 image 段（http(s) URL）→ 经 Mock 下载落盘后发 11030 — PASS

- 断言/证据（11 条）：
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"image","data":{"file":"http://127.0.0.1:6399/media/pic.png"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11030-16"},"echo":"echo-T60-41"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  11030 conversation_id == S:67890_77880001
  - OK  11030 data.file 为绝对路径
  - OK  11030 data.file 指向已存在文件
  - OK  落盘扩展名 .png
  - OK  落盘内容为 Mock 下载的 PNG 长度 (33 == 期望 33)
  - OK  落盘内容为 Mock 下载的 PNG 字节完全一致（33 bytes）
  - OK  Mock 记录到 shim 的 HTTP 下载请求
  - OK  仅 1 条 11030 命令（无文本段）

### T61 [P0/actions] 出站 image 段（file:// 前缀 + 绝对路径）→ 11030 — PASS

- 断言/证据（10 条）：
  - 临时文件: D:\wecom-ling\onebot\tests\bin\tmp_src_for_fileprefix.png (33 bytes)
  - file:// 源: file:///D:/wecom-ling/onebot/tests/bin/tmp_src_for_fileprefix.png
  - ACTION send_private_msg params={"user_id":"67890","message":[{"type":"image","data":{"file":"file:///D:/wecom-ling/onebot/tests/bin/tmp_src_for_fileprefix.png"}}]} -> {"status":"ok","retcode":0,"msg":"","data":{"message_id":"srv-11030-17"},"echo":"echo-T61-42"}
  - OK  status == ok
  - OK  retcode == 0 (整型)
  - OK  11030 data.file 为绝对路径
  - OK  11030 data.file 指向已存在文件
  - OK  落盘扩展名 .png
  - OK  file:// 源文件内容一致 长度 (33 == 期望 33)
  - OK  file:// 源文件内容一致 字节完全一致（33 bytes）

### T62 [P0/actions] 出站未知消息段（json）→ failed/500 提示不支持，不下发微灵命令 — PASS

- 断言/证据（5 条）：
  - ACTION send_group_msg params={"group_id":"12345","message":[{"type":"json","data":{"data":"{}"}}]} -> {"status":"failed","retcode":500,"msg":"当前版本不支持发送 json 消息段（支持：文本、@、图片、视频、文件、动图）","data":null,"echo":"echo-T62-43"}
  - OK  status == failed
  - OK  retcode == 500 (整型)
  - OK  错误信息提示不支持 (找到 "不支持")
  - OK  未知消息段不产生微灵命令

## 测试方法与证据链

1. **Mock OneBot v11 服务器**（`mock_server.py`，aiohttp，127.0.0.1:6399）：作为反向 WS 服务端校验握手头（`X-Self-ID`/`X-Client-Role: Universal`/`Authorization: Bearer testtoken`，token 不符返回 HTTP 401），记录全部入站帧（`logs/frames.jsonl`）、连接与拒绝记录，并可主动下发动作（`/ctl/action`）与断链（`/ctl/close_all`）；另提供 `/media/{name}` 媒体夹具与 `/ctl/media_info`，用于验证 `get_image/get_record/get_file` 的按需下载与 base64 返回。
2. **TestHost**（`TestHost.cs` → `bin\TestHost.exe`，x86）：实例化真实 `OneBotShim.WxSocketClient`，通过公开面（`Start/Stop/TrySendEvent/IsConnected/StatusText/StatusBackground/StatusChanged`）驱动；注入微灵原始 JSON 事件；以伪造的 `CommandHandler` 拦截 shim 下发给微灵的命令并写入 `logs/commands.jsonl`，同时返回构造的 11038/11036/11040/11039 业务数据。
3. **断言依据**：直接读取 Mock 记录的 WS 帧（`/ctl/state`）、媒体命中记录（`/ctl/state` 的 `media_hits`）与 shim 写出的 `onebot_v11.log`，不使用任何 mock 化的 shim 内部状态，保证是真实黑盒验证。
4. **构建新鲜度**：`build.ps1` 每次运行都从 `onebot\output\OneBotShim.dll` 刷新 `lib`/`bin` 副本，上述 SHA256 即为本次实际加载的二进制，并与最新编译产物核对一致。
5. **API 探针**：`probe\ApiProbe.cs`（x86 + 编译期引用原 exe）用于确认 `Robot.Common.CommandExecResult` 可写属性（`Ok/Data/Message/Error`）与 `WxWebSocketClient.CommandHandler` 委托签名，据此实现假 handler。
6. **测试自包含/隔离**：仅使用 127.0.0.1:6399 的 Mock，不启动或依赖真实微灵/AstrBot；shim 的 self_id 在 `Start()` 前由 11026 注入，从而不触发其 `127.0.0.1:8060` HTTP 自探测（本机可能同时运行线上实例）；“无 self_id 不连接”一项改用 personal 通道 shim 验证（该通道不做 HTTP 自探测），避免与线上实例串扰。

### 覆盖要点

- **握手/生命周期**：X-Self-ID 需先由 11026（或顶层 robot_id）建立，之后才连接；`X-Client-Role: Universal`、`Authorization: Bearer testtoken`；连接后 lifecycle.connect；30 s 心跳（`interval:30000`）。
- **事件翻译**：11026/11041 群与私聊/11042/11043/11044/11045/11046/11047/11048/11050/11123/11063/11072/11073/11074/11075/11076/11078/11173/11200，is_self 过滤、at 段（11041 保留 @自己 的 at 段）、raw_message 拼接、顶层 `ling` 原样回传（去掉 `data.payload_base64`）；11200 未映射消息按 content/at_list 透传为 message，`message_type=read_receipt` / `event_desc=已读上报` 静默丢弃。
- **入站段新结构（本轮）**：11045 → file 段 `{name,file,url}`（AstrBot File 需要 name）；11046 → location 段（lat/lon/title/content）；11047 → share 段（url/title/content/image）；11048 → image 段；11050 → contact 段（type=qq、id 为数字，非数字降级文本）。
- **媒体（新 shim）**：11042/11043/11045/11048 登记 cdn.url 后，`get_image/get_record/get_file` 按需 HTTP 下载并返回 `base64://`（保留 file_name）；`get_group_file_url/get_private_file_url` 返回原始 CDN URL；未命中返回 failed/500。
- **动作发送**：send_private_msg/send_group_msg/send_msg（文本 / at / 混合 / 仅 at）、11029 与 11069 选择、conversation_id 构造（`S:<peer>_<self>` / `R:<group>`）、client token、echo 原样回传、message_id。
- **出站回复链与媒体（本轮）**：reply/face 段静默忽略、at(qq=all) 忽略（降级 11029）；image 段支持 base64://、http(s) URL（经 Mock 下载）、file:// 前缀与绝对路径四种来源，先落盘到程序目录 `onebot_media_tmp\` 后发 11030（命令携带绝对路径与正确扩展名）；video→11067；file→11031（保留 file_name）；gif 类型/扩展名→11070；record→failed/500「暂不支持发送语音」，json 等未知段→failed/500「不支持」，两者均不下发命令。
- **get_* 查询**：get_login_info/get_status/get_version_info/get_group_list/get_friend_list/get_group_info/get_group_member_info/get_stranger_info/get_msg/can_send_image；本轮新增 get_group_member_list（→ 11040 且命令带顶层 `getData:true`，返回成员数组）与 get_group_msg_history（返回空数组）；未支持动作 failed（500 或 1404）。
- **负向/健壮性**：token 不符 401 后重连、Mock 主动断链后约 1 s 重连、Stop 后不再连接、record/未知消息段被拒绝、缺字段/空消息报错、未知事件不推送、未知动作 1404。
- **看门狗已删除**：最终版 shim 删除了 `ReceiveWatchdogMs` / SendLoop 120 s 入站静默看门狗，不再有周期性重连；T04 断言心跳等待期间连接数不变，T38 断言 `onebot_v11.log` 中不存在 `watchdog` / `120000 ms; closing socket` 记录。

### 证据文件

- `logs/testhost.log` — TestHost 逐步执行与断言日志
- `logs/testhost_console.log` — TestHost 控制台输出（含 TOTAL/PASS/FAIL 与失败项）
- `logs/commands.jsonl` — 假微灵 handler 收到的每条命令 JSON
- `logs/frames.jsonl` — Mock 记录的 WS 双向帧
- `logs/connections.jsonl` — WS 连接/断开记录
- `logs/rejections.jsonl` — token 不符被拒记录
- `logs/media_hits.jsonl` — 媒体夹具被 shim HTTP 下载的记录
- `logs/onebot_v11.log` — shim 自身日志（从 bin 复制）
- `logs/mock_server.log` — Mock 服务器日志
- `logs/results_raw.json` — TestHost 原始结果（本报告的数据源）

## 复现命令

```powershell
cd D:\wecom-ling\onebot\tests
# 编译 + 启动 Mock + 跑全部用例 + 生成报告（本机无 pwsh；脚本未签名，需 Bypass）
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\run_all.ps1
```

单独运行：`powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1`，然后
`"<含 aiohttp 的 python>\python.exe" .\mock_server.py`（后台）再执行 `bin\TestHost.exe`。

## 产物清单

| 产物 | 说明 |
| --- | --- |
| `mock_server.py` | Mock OneBot v11 反向 WS 服务端（含控制接口与媒体夹具） |
| `TestHost.cs` / `bin\TestHost.exe` | C# 测试宿主（62 个用例，真实实例化 shim） |
| `build.ps1` / `run_all.ps1` | 编译脚本 / 一键运行脚本 |
| `TEST_RESULTS.md` | 本报告（每用例 PASS/FAIL + 证据） |
| `results.json` | 机器可读结果（摘要 + 环境 + 每用例证据 + P0/P1） |
| `logs\*` | 原始日志与帧记录 |

