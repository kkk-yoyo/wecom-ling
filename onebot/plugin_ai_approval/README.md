# astrbot_plugin_ai_approval（AI 审批）

适用于 **AstrBot 4.29** 的中文审批插件：员工在企业微信 / QQ 等聊天中直接提交请假、报销、外出、加班申请，插件按配置规则**自动通过**；不满足规则时**转管理员群提醒**，并把每一笔申请追加写入 **JSONL 审计日志**。

> 适配平台：`aiocqhttp`（企业微信桥接通道、OneBot v11）；其它平台走同一套通用发消息接口，但仅在 `metadata.yaml` 声明的平台上保证可用。

---

## 一、功能

- **自然语言解析**：支持「申请请假 3 天，事由：家里有事」「报销 1200 元，打车费」「申请 2026-11-20 外出 2 小时」等说法，也识别中文数字（三天、两天、两个半小时）。
- **结构化格式**：`#审批 请假 3 天 家里有事`，适合表单/快捷输入。
- **规则自动通过**：类型在 `auto_approve_types` 内、天数 ≤ `auto_approve_max_leave_days`、金额 ≤ `auto_approve_max_amount`，且申请人不是管理员时自动通过。
- **转人工**：不满足规则时把「申请摘要 + 申请ID」发到 `admin_group_ids`（未配置则私聊 `admin_user_ids`），同时回复申请人「已提交待管理员审批」。
- **可选 LLM 合理性审查**：`use_llm=true` 时调用 AstrBot 已配置的 LLM 复核；LLM 不可用、超时、输出无法解析时**自动回退规则判定**，不会崩溃、不会阻塞审批。
- **审计日志**：每笔申请一行 JSON，追加写入 `audit.jsonl`，字段固定：`ts / applicant_id / applicant_name / type / reason / days / amount / decision / decider / reason_text`。
- **安静忽略**：审批群/私聊中不像审批申请的消息不会触发插件，也不会乱回复普通聊天。
- **命令**：`/ai审批帮助`、`/ai审批规则`、`/ai审批记录`（管理员）。

---

## 二、安装

1. 把整个插件目录放到 AstrBot 实例的插件目录：
   - 本实例路径：`C:\Users\Administrator\.astrbot_launcher\instances\38c868af-ee29-4389-9c27-c69e148f7200\core\data\plugins\astrbot_plugin_ai_approval\`
   - 目录名必须是 `astrbot_plugin_ai_approval`（与 `metadata.yaml` 的 `name` 一致）。
2. 在 AstrBot WebUI →「插件管理」中点击**重载/启用**该插件（或重启 AstrBot）。
3. 首次加载会自动生成配置文件 `core\data\config\astrbot_plugin_ai_approval_config.json`（内容取自 `_conf_schema.json` 默认值）。

> **配置生效方式**：在 WebUI 插件配置页保存时会**自动热重载**该插件、配置即时生效；如果直接手工编辑上述 JSON 文件（或命令行改文件），插件内存里仍是旧配置，需要到 WebUI「插件管理 → 重载」点一下，或调用 `POST /api/plugin/reload`（body 可带 `{"name": "astrbot_plugin_ai_approval"}`）重载后生效。插件已在运行时不需重启 AstrBot 本体。

> 插件无第三方依赖，仅使用 AstrBot 自带 API 与 Python 标准库。

---

## 三、配置（`_conf_schema.json`）

| 键 | 类型 | 默认 | 说明 |
| --- | --- | --- | --- |
| `approval_group_ids` | list[str] | `[]` | 审批群群号。**只有这些群会触发**；留空则群聊完全不触发，仅私聊可用。 |
| `admin_group_ids` | list[str] | `[]` | 管理员群群号，转人工申请会发到这里。留空则改为私聊提醒 `admin_user_ids`。 |
| `admin_user_ids` | list[str] | `[]` | 管理员用户 ID 列表：申请人若在内则强制人工审核；`/ai审批记录` 也允许这些 ID 使用。 |
| `auto_approve_types` | list[str] | `["请假","外出"]` | 可自动通过的类型，候选：`请假 / 报销 / 外出 / 加班 / 其他`（支持 `休假/调休/病假/报账/出差` 等别名）。 |
| `auto_approve_max_leave_days` | float | `2` | 天数上限。小时按 **8 小时 = 1 天**折算（如 2 小时 = 0.25 天）。 |
| `auto_approve_max_amount` | float | `500` | 金额上限（元）。 |
| `use_llm` | bool | `false` | 是否启用 LLM 合理性审查。 |
| `llm_provider_id` | str | `""` | 审查使用的 Provider ID；留空 = 按会话偏好使用默认对话模型。 |
| `audit_log_path` | str | `""` | JSONL 路径。留空 = `core\data\astrbot_plugin_ai_approval\audit.jsonl`；相对路径基于 AstrBot data 目录。 |

配置示例（`core\data\config\astrbot_plugin_ai_approval_config.json`）：

```json
{
  "approval_group_ids": ["1688855837869858"],
  "admin_group_ids": ["1688855837869275"],
  "admin_user_ids": ["1688855837869275"],
  "auto_approve_types": ["请假", "外出"],
  "auto_approve_max_leave_days": 2.0,
  "auto_approve_max_amount": 500.0,
  "use_llm": false,
  "llm_provider_id": "",
  "audit_log_path": ""
}
```

> **重要**：企业微信场景下 `approval_group_ids` / `admin_group_ids` 要填**群 ID**（群聊会话 ID，可在 AstrBot 日志或 WebUI 会话列表中查看），不是群名称。
>
> **本实例当前配置**：`approval_group_ids = ["10758409533113150", "1970326573995963"]`（测试专用、华威网络科技有限公司两个群），`admin_group_ids` / `admin_user_ids` 暂未配置；因此自动通过的申请只回复申请人、不打扰任何人，超出规则的申请只回复申请人「已提交待管理员审批」并在日志中提示未送达管理员——要真正提醒管理员，请在 WebUI 把管理员群 ID 填进 `admin_group_ids`（或管理员用户 ID 填进 `admin_user_ids`）后保存（会自动热重载）。

---

## 四、命令

群聊中命令需要唤醒前缀（本实例默认为 `/`）；私聊中直接发送即可。

| 命令 | 权限 | 说明 |
| --- | --- | --- |
| `/ai审批帮助` | 所有人 | 用法示例 + 当前自动通过规则摘要 |
| `/ai审批规则` | 所有人 | 显示审批群、管理员群、类型/天数/金额上限、LLM 开关、审计路径 |
| `/ai审批记录` | 管理员 | 显示最近 10 条审计记录（时间 / 申请人 / 类型 / 结论 / 事由） |

管理员判定：`admin_user_ids` 中的用户、群主/群管理员（`event.is_admin()`）、或 AstrBot 全局配置 `admins_id` 中的账号。

---

## 五、申请示例

在审批群（或私聊）中直接发送，**不需要唤醒词**：

```
申请请假 3 天，事由：家里有事
报销 1200 元，打车费
申请 2026-11-20 外出 2 小时
#审批 请假 3 天 家里有事
#审批 其他 采购服务器 金额 3000 元
```

默认规则（`请假/外出` 自动通过、上限 2 天 / 500 元）下的结果：

- 「申请 2026-11-20 外出 2 小时」→ **自动通过**（0.25 天 ≤ 2 天）
- 「申请请假 3 天，事由：家里有事」→ 超出 2 天 → **转管理员群**，回复「已提交待管理员审批」
- 「报销 1200 元，打车费」→ 类型不在自动通过列表且金额超 500 → **转管理员群**

---

## 六、判定逻辑

1. **解析**：识别类型（请假/报销/外出/加班/其他）、事由、天数（含小时折算）、金额、时间。
2. **规则判定**：
   - 类型 ∈ `auto_approve_types`；
   - 天数 ≤ `auto_approve_max_leave_days`（请假/外出/加班必须能解析出时长，否则转人工）；
   - 金额 ≤ `auto_approve_max_amount`（报销必须能解析出金额，否则转人工）；
   - 申请人不在管理员名单（管理员申请强制人工审核）。
3. **可选 LLM 审查**（`use_llm=true` 且规则通过）：LLM 说 approve → 自动通过；说 reject → 记为 `rejected` 并仍然转管理员群；不可用 → 回退规则。
4. **落库**：写入审计 JSONL；`decision` 取值 `auto_approved` / `pending` / `rejected`，`decider` 取值 `rule_engine` / `rule_engine+llm` / `llm_review` / `admin_group`。

---

## 七、审计日志

默认路径：`core\data\astrbot_plugin_ai_approval\audit.jsonl`（每行一条 JSON，UTF-8，`ensure_ascii=False`）：

```json
{"ts": "2026-11-13T13:40:00+08:00", "applicant_id": "1688855837869858", "applicant_name": "张三", "type": "请假", "reason": "家里有事", "days": 3.0, "amount": null, "decision": "pending", "decider": "admin_group", "reason_text": "天数 3 超过自动通过上限 2 天"}
```

写日志失败只记录错误日志，不影响审批回复与提醒。

---

## 八、限制与说明

- **本插件是「聊天消息驱动」的审批**：解析聊天文本 → 规则判定 → 回复申请人 / 提醒管理员群 → 写 JSONL 审计。它**不是企业微信原生审批流**：不调用企业微信审批接口、不生成/流转企微审批单，企微「审批」应用里看不到这些记录；如需原生审批请另行对接企业微信审批 API。审计与状态仅存于本插件的 JSONL 文件。
- **群聊触发必须先配置审批群**：`approval_group_ids` 为空时，群消息一律不处理（避免误触发其它群）；私聊不受影响。
- **群聊申请不需要唤醒词**（插件用自定义 filter 激活）；群聊命令（`/ai审批帮助` 等）仍按 AstrBot 规则需要唤醒前缀或 @机器人。
- 企业微信 / OneBot 的群 ID、用户 ID 都是平台侧 ID，换平台后需要重新配置。
- 审批申请**只支持文本**；图片、语音申请不会触发（LLM 解析图片不在本插件范围）。
- 「非管理员强制审核」实现为：`admin_user_ids` 中的账号或群管理员提交的申请强制转人工，不自动通过。
- 时间解析支持 `YYYY-MM-DD`、`YYYY年M月D日`、`M月D日`、`M号`、`今天/明天/后天`；复杂时间（如「下周三」「工作日上午」）会保留原文但不参与规则判定。
- 可选 LLM 审查默认关闭；开启后单次最多等待 25 秒，失败/超时自动回退规则。

---

## 九、目录结构

```
astrbot_plugin_ai_approval/
├── main.py            # 插件主逻辑（解析 / 判定 / 提醒 / 审计 / 命令）
├── _conf_schema.json  # WebUI 配置 schema
├── metadata.yaml      # 插件元数据（name/version/author/...）
├── README.md          # 本说明
└── tests/             # 冒烟 / 端到端测试脚本（不参与插件加载）
    ├── smoke_test.py       # 解析/规则/审计/schema 冒烟测试（65 项）
    ├── e2e_fake_event.py   # 假 event/context 驱动完整处理器（自动通过 + 转管理员群）
    ├── e2e_test.py         # 通过 Dashboard Chat 私聊发真实申请的端到端测试
    ├── e2e_commands.py     # 通过 Dashboard Chat 验证三条命令
    ├── reload_plugin.py    # 自签 JWT 调 Dashboard API 热重载本插件
    ├── query_rules.py      # 查询 /ai审批规则 当前生效配置
    └── check_api.py        # 检查插件是否被 AstrBot 识别（activated）
```

工作区源码镜像：`D:\wecom-ling\onebot\plugin_ai_approval\`（安装时只需复制 `main.py`、`_conf_schema.json`、`metadata.yaml`、`README.md`）。
