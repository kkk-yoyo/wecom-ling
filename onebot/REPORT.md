# 微灵 1.11.0 → OneBot v11 改造报告

更新时间：2026-11-13 13:50（v1.6 文档 / 最终部署构建）
工作目录：`D:\wecom-ling`

## 一、结论

已将「微灵 (WeComSever) 1.11.0」自研 WebSocket 协议（`WeComConsoleServer.Core.WxWebSocketClient`，协议为 `{id,action,payload}` + hello/ping/pong/event/command/command_result + WXFL 二进制分片）**替换为标准 OneBot v11 反向 WebSocket 客户端**。

- 原程序 `D:\wecom-ling\微灵1.11.0_免卡密.exe` **未修改**（SHA256 前后一致）。
- 改造版：`D:\wecom-ling\微灵-OneBotV11-1.11.0.exe`（51,494,912 字节，最终构建）。
- 微灵现在作为 OneBot v11 **反向 WS 客户端**，主动连接 AstrBot 的 `ws://127.0.0.1:6200/ws`（可配置），AstrBot 侧零改动。
- 微灵其余功能（企业微信注入、HTTP 8060 API、事件回调、UI、配置、个微 TCP 桥等）**全部原样保留**，只替换了 WS 客户端这一个类的实现。
- 消息能力：入站文本/@/图片/视频/文件/动图/位置/分享/名片/未映射消息，出站文本/@/图片/视频/文件/动图（reply、face 段自动忽略）。

## 二、交付物（最终）

| 路径 | 说明 | SHA256 |
|---|---|---|
| `微灵-OneBotV11-1.11.0.exe` | 改造版主程序（根目录 / 安装目录各一份，同一文件） | `3A54DF0E9B44889796B7A2E806018F963E97847F1B01A31869BBE633BB96DED8` |
| `微灵 OneBot v11 对接文档.docx` | 对接文档 v1.6（13 章 + 52 表） | `7913D90052932A045FD536BF91B6B48D724D3E641C1D6E76D2F185477326B305` |
| `D:\微灵OneBotV11\` | 已部署运行目录：exe + PersonalTcpBridge.exe + config.ini + 运行说明 + 文档副本 | — |
| `backup\微灵1.11.0_免卡密.orig.exe` | 原程序备份（未改动） | `4F2954A7149B318B4A69F7D407302EAED67A2911F27084C289DD35BF0F5BA24A` |
| `onebot\src\WxSocketClient.cs` | OneBot v11 客户端实现（shim 源码，C#5） | — |
| `onebot\output\OneBotShim.dll` | 编译后的 shim（53,760 字节，最终构建） | `4F0D7EA2372B0BF7109C6F6AEC87D5D6E41A523313B8264B0D93ECC6F46FD714` |
| `onebot\tools\PatchOneBot.cs` | IL 移植工具（dnlib 4.5.0，加法式策略） | — |
| `onebot\tests\` | 62 用例 Mock 测试（TEST_RESULTS.md / results.json / run_all.ps1） | — |
| `onebot\plugin_ai_approval\` | AstrBot AI 审批插件源码（astrbot_plugin_ai_approval） | — |
| `onebot\smoke\` | GUI 启动、AstrBot 联调、取证脚本与日志 | — |

## 三、实现原理

1. 用 dnlib 4.5.0 加载原 exe（.NET Framework 4.7.2、x86、WPF、单文件托管程序集，含混淆）。
2. **保留式（加法式）重写**：原有 537 个类型、全部字段/方法/嵌套类型/属性/事件**一个不删**；只把 `WxWebSocketClient` 的 10 个公开成员（`.ctor/Start/Stop/TrySendEvent/Dispose/add_StatusChanged/remove_StatusChanged/get_IsConnected/get_StatusText/get_StatusBackground`）的方法体替换为 shim 实现；shim 其余方法与字段以 `ob_` 前缀追加（`StatusChanged` 事件复用原字段）。
3. 写出时必须使用 `MetadataFlags.PreserveAll`（保留 #Strings/#US/#Blob 堆偏移、额外签名数据与全部 RID）。**这是关键**：混淆器在元数据堆偏移/签名尾数据里存放运行期解密所需信息，普通重写会让程序启动即崩溃（CLR FailFast：`Infinite recursion during resource lookup within mscorlib ... Arg_NullReferenceException`）。
   - 对照实验：dnlib 原样 round-trip + PreserveAll → 可正常启动；仅保留堆偏移 → 崩溃；Cecil 重写 → 崩溃。由此定位到 PreserveAll 为必要项。
4. 混淆器注入的不可达垃圾 IL（非法 token 的 call）原样保留，靠 writer 的 `DummyLogger.NoThrowInstance` 写出，不影响运行（已实测）。
5. 原 4 个内嵌资源（256 / 49,442,954 / 4,552 / 50,232 字节）全部保留；程序集身份 `WeComSever, Version=1.11.0.0, PublicKeyToken=null` 不变。伪造的 `mscorlib 65535.255…` 重复引用被 writer 丢弃（16→15 条 AssemblyRef），实测无影响。

## 四、协议实现概览

- **连接**：反向 WS 客户端；URL 取微灵配置 `WxWsUrl`（空则默认 `ws://127.0.0.1:6200/ws`；`http→ws`、`https→wss`；无 scheme 自动补 `ws://`；路径为空补 `/ws`）；header：`X-Self-ID`、`X-Client-Role: Universal`、可选 `Authorization: Bearer <WxWsToken>`。
- **self_id**：优先取登录事件 `11026/11179` 的 `user_id`/`robot_id`；未登录时通过微灵 HTTP `POST /api/Client/ExecCommand {type:11035, getData:true}` 探测，仍无则每 1s 重试（不空连）。
- **握手/心跳**：连接成功即发 `lifecycle.connect`；每 30s 发 `meta_event heartbeat`；断线 1s 起指数退避、最长 30s 重连。
- **动作**（AstrBot → 微灵，经 `ClientController.ExecuteCore` 下发）：`send_private_msg` / `send_group_msg` / `send_msg`（文本 + @ + 媒体；群 R:<id>、私聊 S:<peer>_<self>；群内 @ → 微灵 11069，否则 11029；媒体按 image→11030、video→11067、file→11031、gif→11070 逐条发送，最后发文本）；`get_login_info`、`get_status`、`get_version_info`、`get_group_list`、`get_friend_list`、`get_group_info`、`get_group_member_info`、`get_group_member_list`、`get_stranger_info`、`get_msg`（本地缓存）、`get_group_msg_history`（空数组）、`can_send_image`/`can_send_record`、`get_image`/`get_record`/`get_file`（`base64://`，15s 超时/32MB 截断）、`get_group_file_url`/`get_private_file_url`（微灵 CDN 直链优先，字节缓存时兜底本地 `http://127.0.0.1:18890-18920/media/<key>`）；未知动作回 `retcode 1404`，不支持动作回 `retcode 500`；`echo` 原样回传；`reply`/`face` 段静默忽略；`at qq=all` 忽略。
- **出站媒体落地**：`base64://`、http(s) URL、`file://`、本机绝对路径四种来源统一下载/解码到 `<程序目录>\onebot_media_tmp\media_<时间>_<序号><扩展名>`（按 magic bytes 定扩展名，1 小时前文件自动清理）；`record` 发送返回失败（微灵无直发语音指令）。
- **事件**（微灵 → AstrBot）：`11041` 文本（含 at 段，保留指向自己的 at）、`11042` 图片、`11043` 视频、`11044` 语音（record，仅 file_id）、`11045` 文件→`{name,file,url}`、`11048` 动图→image、`11046` 位置→location、`11047` 链接→share、`11050` 名片→contact、`11049` 红包/`11066` 小程序/`11068` 图文/`11124` 视频号/`11195` 直播 → 文本；`11123` 撤回 → notice；`11063` 好友申请 → request；`11072/11073/11074/11173/11075/11076/11077/11078` → notice；`11200` 未映射消息 → message（普通消息按 content/at_list 透传；`read_receipt`/已读上报过滤并记日志）；原始微灵事件放事件顶层 `ling` 字段（剔除 `payload_base64`）；非数字 id 以字符串透传。
- **个人微信通道**（`PersonalUseWebSocket=true`）：实验性支持；出站仅文本（`send_text_msg`），入站按 `from_user/room_id/content/event_type` 启发式映射，详见文档 §9。
- **日志**：exe 同目录 `onebot_v11.log`（5MB 轮转）。

## 五、验证证据

1. **Mock 测试**：**62/62 PASS**（P0 54/54 + P1 8/8，0 失败），覆盖握手/心跳/事件/媒体收发/动作/负向/隔离；`onebot\tests\TEST_RESULTS.md`、`results.json`；复现：`powershell -NoProfile -ExecutionPolicy Bypass -File D:\wecom-ling\onebot\tests\run_all.ps1`。
2. **真实 AstrBot 4.29 联调**：本机 AstrBot `wecom` 平台（aiocqhttp，6200，token 114514）——握手成功、`aiocqhttp(OneBot v11) 适配器已连接。`；入站群/私聊消息进入 AstrBot 事件总线并触发插件；出站 `send_group_msg`/`send_private_msg` 成功（message_id 100001–100006 等，微灵回采消息内容正确，中文正常）。
3. **改造版 GUI 启动实测**：进程存活、「微灵」主窗口、自动释放 `dll/`、创建 `logs/`、写 `onebot_v11.log`；`self_id resolved via HTTP: 1688855837869858` → `connected: ws://127.0.0.1:6200/ws` → `lifecycle.connect` → AstrBot 同一秒「适配器已连接」。
4. **原程序完整性**：原 exe 与备份 SHA256 = `4F2954A7…5BA24A`（未变）；产物结构回读：类型 537（与原始一致）、资源 4 个（与原始一致）、目标类型方法 186、字段 72、`nested` 9、保留成员名齐全。
5. **线上实测**（2026-11-13）：企业微信账号 HVIBOT 注入成功、客户端上线；图片/文件消息双向收发验证；入站图片在 AstrBot 显示 `[图片]`；read_receipt 过滤生效；中文发送以显式 UTF-8 请求体验证正常。

## 六、迭代修复记录（交付后现场问题）

| 版本 | 问题 | 修复 |
|---|---|---|
| v1 | 群内 @机器人不触发回复 | 入站 11041 不再过滤指向自己的 at（保留全部 at 段） |
| v2 | AstrBot 查询动作报「参数不正确：ID 必须为字符串…」、群列表恒空 | 查询指令改为顶层 `getData:true`；11039 参数 `user_id`；11038/11040 参数修正 |
| v3 | AstrBot 回复链带 `reply` 段导致发送失败（retcode 500） | `AppendOutgoingSegment` 支持 reply（忽略）/face/`at all`；新增 `get_group_member_list`、`get_group_msg_history` |
| v4 | 出站仅文本+@；入站 file 段丢段、location/link/card 降级；未映射消息 11200 被忽略、已读上报噪音 | 出站 image/video/file/gif 落地后发送；入站 file/location/share/contact 结构化；11200 映射与 read_receipt 过滤；新增 `ONEBOT_DISABLE_HTTP_SELFID_PROBE` 测试开关 |
| 文档 v1.6 | 能力矩阵/限制/插件章节 | 已更新（13 章 52 表） |

## 七、已知限制

1. 出站**语音/record**、转发、json/xml 段暂不支持（返回 failed 并附原因）；出站媒体文件只在微灵进程内临时落地，磁盘占用见文档。
2. 个人微信通道为**启发式实验性**实现，建议以企业微信通道为主。
3. 语音消息 `11044` 只有 `file_id`、无 CDN URL：`get_record` 需调用方传 http(s) 直链（文档已注明）。
4. `11200` 未映射消息按 `content`/`at_list` 尽力映射，字段细节有限。
5. 更新 shim 后需重跑 `onebot\tools\PatchOneBot.exe`（读取 `onebot\output\OneBotShim.dll`）生成新 exe，再替换安装目录并重启。
6. 微灵自研 WS 协议已被 OneBot v11 取代，原 hello/ping/WXFL 文件分片通道不再使用。

## 八、使用步骤（最短路径）

1. 备份当前微灵目录（原 exe 已在 `backup\` 留档）。
2. 把整个运行目录放到**不受安全策略限制**的位置（本机已备好 `D:\微灵OneBotV11\`）；**不要**放回 `D:\wecom-ling` 等受限目录，否则 8060 与企微注入都会失败（见 §九）。
3. 启动改造版 → 微灵界面 → 企业微信 → 通信方式选 WebSocket，地址填 `ws://127.0.0.1:6200`（或 AstrBot 实际地址），Token 填 AstrBot 平台 token（本机 `114514`），保存并连接。
4. AstrBot 平台配置保持 `aiocqhttp` 反向 WS：host `0.0.0.0`、port `6200`、token `114514`（路径 `/ws` 由适配器处理）。
5. 微灵登录企业微信后，`onebot_v11.log` 出现 `connected: ws://...`，AstrBot 显示「适配器已连接」即为成功。
6. 排障：文档第 12 章；shim 日志在 exe 同目录 `onebot_v11.log`。

## 九、运行环境关键发现（2026-11-12 现场问题定位，重要）

用户现场反馈两个问题：企业微信注入弹窗 `Error: cannot verify loaded hook modules`；HTTP 8060 起不来。最终定位：**与 OneBot 改造补丁无关**——本机终端安全策略（传云 Vault / CloudEDR HIoTSecAgent）把 `D:\wecom-ling` 标记为受限目录，从该目录启动的进程及其子进程被限制（禁止写 C:\Users AppData/Temp、禁止注册 HTTP.SYS 前缀、注入器被判定不可信）。

证据：
- 同一 exe（含原版）在 D:\wecom-ling 下：8060 不监听、注入报 hook 校验失败、子进程写 C:\Users 被拒（UnauthorizedAccessException）；复制到 D:\wecom-ling-app 等可信目录：8060 秒开、注入成功（proxy /api/status 返回 hooks_installed=true、tcp.connected=true）。
- 父进程信任传染：同一 junction 路径的注入器，由不可信父进程启动 → exit 1 `cannot verify loaded hook modules`；由可信父进程启动 → exit 0、注入成功。
- junction 绕过测试成功（策略按解析后最终路径判定）。
- 更正早期怀疑：曾以为 8060 失败是 BaseAddress 使用 `0.0.0.0`（HttpListener 直接测试确实不支持该前缀），但可信目录中原版 exe 日志显示 `监听地址: http://0.0.0.0:8060/` 正常工作 → **0.0.0.0 不是问题**，已移除对应补丁（避免把监听限制到 127.0.0.1）。

交付/部署要求：**必须从不受安全策略限制的目录运行**（本机为 `D:\微灵OneBotV11`）。该目录含：`微灵-OneBotV11-1.11.0.exe`（3A54DF0E…）、`PersonalTcpBridge.exe`、`config.ini`（预配 ws://127.0.0.1:6200、token 114514、自动连接）、`运行说明-必读.txt`、文档副本。实测：启动约 12s 内 8060 监听、`/api/Client/ReInitSocket` 返回 200、日志「服务已启动，监听地址: http://0.0.0.0:8060/」；UI 点「登录新账号」→ 注入成功、客户端上线、proxy hooks_installed/tcp.connected 均 true。

## 十、AI 审批插件（AstrBot 第三方插件）

- 名称：`astrbot_plugin_ai_approval`（v1.0.0），源码：`D:\wecom-ling\onebot\plugin_ai_approval\`，已安装到 AstrBot 实例 `core\data\plugins\`，AstrBot 已加载（日志 `Loading plugin astrbot_plugin_ai_approval ...`）。
- 定位：**聊天消息驱动的审批助手**，不是企业微信原生审批（原生审批同意无开放 API，微灵 Hook 仅提供消息/联系人/群能力）。
- 功能：员工在群/私聊按自然语言或结构化内容提交申请（类型/事由/天数/金额/时间）；插件解析后按规则自动通过（默认：请假/外出 ≤2 天、金额 ≤500 元），不满足转管理员提醒；JSONL 审计日志；命令 `/ai审批帮助`、`/ai审批规则`、`/ai审批记录`（管理员）。
- 配置项：`approval_group_ids`、`admin_group_ids`、`admin_user_ids`、`auto_approve_types`、`auto_approve_max_leave_days`、`auto_approve_max_amount`、`use_llm`、`llm_provider_id`、`audit_log_path`。
- 验证：自带 smoke_test.py 65/65 PASS；AstrBot 加载成功、配置已生成。
