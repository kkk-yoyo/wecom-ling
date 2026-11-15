# -*- coding: utf-8 -*-
"""第 11 章：AI 审批插件。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "11", "AI 审批插件", page_break=True)
    K.add_para(doc, "本章介绍与改造版配套使用的第三方 AstrBot 插件 `astrbot_plugin_ai_approval`（AI 审批）。"
                    "**它是运行在 AstrBot 侧的独立插件，不是桥接层（OneBotShim）的功能**：桥接层只负责"
                    "企业微信消息与 OneBot v11 之间的转发，审批规则、解析与审计都由插件完成。")

    K.add_h2(doc, "11.1", "功能概览")
    K.add_bullet(doc, "**实现状态与源码位置**：插件源码已生成于 `D:\\wecom-ling\\onebot\\plugin_ai_approval\\`"
                       "（`main.py`、`README.md`、`metadata.yaml`、`_conf_schema.json`），为 AstrBot v4 插件"
                       "（`Star` 基类），聊天消息驱动，不依赖桥接层；安装到 AstrBot 实例后即可使用（见 11.4 节）。")
    K.add_bullet(doc, "员工在审批群或私聊中按自然语言或约定结构提交审批内容（申请人、类型、事由、天数、金额、时间）。")
    K.add_bullet(doc, "插件解析提交内容，并按配置的规则判定是否满足自动通过条件；满足则回复通过回执。")
    K.add_bullet(doc, "不满足规则时，插件把审批内容转发到管理员群提醒人工处理。")
    K.add_bullet(doc, "所有提交、判定与结果写入 JSONL 审计日志，便于追溯。")
    K.add_bullet(doc, "提供管理员命令：`/ai审批帮助`、`/ai审批规则`、`/ai审批记录`。")
    K.add_callout(doc, "warn", "这是**聊天消息驱动**的审批流程，不是企业微信原生审批。企业微信原生审批的“同意/拒绝”"
                                "没有开放接口，微灵 Hook 也不提供该能力，插件只能基于群/私聊消息与规则近似实现。",
                  title="不是原生审批")

    K.add_h2(doc, "11.2", "工作流程")
    K.add_step(doc, 1, "员工提交：在审批群或私聊中发送审批内容。支持自然语言（如“我明天请一天年假，事由……”）"
                       "和结构化写法（申请人/类型/事由/天数/金额/时间）。")
    K.add_step(doc, 2, "解析：插件提取字段；开启 `use_llm` 时调用 AstrBot 侧配置的 LLM 供应商解析自然语言。")
    K.add_step(doc, 3, "规则判定：按 `auto_approve_types`、`auto_approve_max_leave_days`、`auto_approve_max_amount` "
                       "等配置判断是否自动通过。")
    K.add_step(doc, 4, "结果处理：满足条件时回复通过回执；不满足时转发到 `admin_group_ids` 指定的管理员群并 @ 管理员。")
    K.add_step(doc, 5, "审计：把原始提交、解析结果与判定写入 `audit_log_path` 指向的 JSONL 文件。")

    K.add_h2(doc, "11.3", "配置项")
    K.add_table(
        doc,
        ["配置项", "类型", "说明", "示例"],
        [
            ["`approval_group_ids`", "数组", "接收审批提交的群 ID（OneBot `group_id`）", "`[\"24680001\"]`"],
            ["`admin_group_ids`", "数组", "需要人工处理时提醒的管理员群 ID", "`[\"24680002\"]`"],
            ["`admin_user_ids`", "数组", "管理员用户 ID（用于命令权限与 @ 提醒）", "`[\"168800001\"]`"],
            ["`auto_approve_types`", "数组", "允许自动通过的审批类型", "`[\"请假\", \"报销\"]`"],
            ["`auto_approve_max_leave_days`", "数值", "自动通过的最大请假天数", "`1`"],
            ["`auto_approve_max_amount`", "数值", "自动通过的最大金额（报销等）", "`500`"],
            ["`use_llm`", "布尔", "是否启用 LLM 解析自然语言", "`true`"],
            ["`llm_provider_id`", "字符串", "AstrBot 中配置的 LLM 供应商 ID", "`deepseek`"],
            ["`audit_log_path`", "字符串", "JSONL 审计日志路径", "`data/ai_approval_audit.jsonl`"],
        ],
        widths=[4.6, 2.0, 6.2, 3.2],
        caption="表 11-1　AI 审批插件配置项",
        mono_cols=[0, 3],
    )
    K.add_code(doc, [
        "# AstrBot 插件配置示例（在 WebUI 插件配置页填写）",
        "approval_group_ids: [\"24680001\"]",
        "admin_group_ids: [\"24680002\"]",
        "admin_user_ids: [\"168800001\"]",
        "auto_approve_types: [\"请假\", \"报销\"]",
        "auto_approve_max_leave_days: 1",
        "auto_approve_max_amount: 500",
        "use_llm: true",
        "llm_provider_id: \"deepseek\"",
        "audit_log_path: \"data/ai_approval_audit.jsonl\"",
    ], caption="代码 11-1　插件配置示例")

    K.add_h2(doc, "11.4", "部署步骤")
    K.add_step(doc, 1, "确认 AstrBot 已正常运行，且 aiocqhttp 反向 WS 已连上微灵（第 2 章）；"
                       "群/私聊消息能在 AstrBot 日志中看到。")
    K.add_step(doc, 2, "把 `D:\\wecom-ling\\onebot\\plugin_ai_approval` 复制到 AstrBot 实例的 "
                       "`core\\data\\plugins\\astrbot_plugin_ai_approval`（目录名保持 astrbot_plugin_ai_approval；"
                       "不同 AstrBot 版本/发行版的插件目录可能不同，以实际安装目录为准）。")
    K.add_step(doc, 3, "在 AstrBot WebUI 的插件页重载插件（或重启 AstrBot），确认插件出现在列表中并已启用。")
    K.add_step(doc, 4, "打开插件配置页，按表 11-1 填写群 ID、管理员、规则与 LLM 配置并保存。")
    K.add_step(doc, 5, "在审批群发送一条测试审批，确认收到解析/通过回执；不满足规则时应转发到管理员群。")
    K.add_step(doc, 6, "用管理员账号在群里发送 `/ai审批规则`、`/ai审批记录` 验证命令权限与审计日志。")
    K.add_callout(doc, "info", "插件依赖 OneBot 消息事件与 `send_group_msg`/`send_private_msg` 动作，"
                                "因此必须先把桥接层与 AstrBot 连通（第 2 章）；桥接层本身不需要为插件做额外配置。",
                  title="前置条件")

    K.add_h2(doc, "11.5", "边界与说明")
    K.add_bullet(doc, "插件是**第三方**组件：升级 AstrBot 或插件版本前先确认兼容性，停用插件不影响桥接层与 OneBot 连接。")
    K.add_bullet(doc, "自动通过只依据配置规则与解析结果，不替代企业微信原生审批；重要审批建议关闭自动通过，"
                       "统一转管理员群人工处理。")
    K.add_bullet(doc, "LLM 解析存在不确定性：建议要求员工按结构化格式提交，或在提示词中约束字段。")
    K.add_bullet(doc, "审计日志是本地 JSONL 文件，请纳入备份与权限管理；日志内容含申请人、事由等敏感信息。")
    K.add_bullet(doc, "插件默认不启用（未配置 `approval_group_ids` 时不响应任何群消息）。")
    K.add_bullet(doc, "若 AstrBot 的插件规范或安装目录随版本变化，以实际安装目录与版本为准；"
                       "插件本身是 AstrBot v4 插件（`Star` 基类），不修改微灵与桥接层。")
