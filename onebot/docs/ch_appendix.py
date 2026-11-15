# -*- coding: utf-8 -*-
"""第 13 章：附录。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "13", "附录", page_break=True)
    K.add_para(doc, "本附录提供事件号、指令号、术语与版本信息，便于查阅。事件与指令编号来自微灵 1.11.0；"
                    "“改造版处理”一列为 OneBot v11 shim 的实际行为。")

    K.add_h2(doc, "13.1", "微灵事件号速查")
    K.add_table(
        doc,
        ["事件号", "含义", "改造版处理", "备注"],
        [
            ["1000 / -1000", "框架控制/错误事件", "忽略", "非业务事件"],
            ["11024", "接口就绪", "忽略", "内部状态"],
            ["11026", "登录成功", "更新 self_id，不上报", "企微通道"],
            ["11027", "退出登录", "清空 self_id，不上报", "连接停止"],
            ["11028", "登录二维码", "忽略", "登录过程"],
            ["11041", "文本消息", "message", "at 段 + text 段"],
            ["11042", "图片消息", "message + image 段", "依赖 cdn.url"],
            ["11043", "视频消息", "message + video 段", "依赖 cdn.url"],
            ["11044", "语音消息", "message + record 段", "仅 file_id"],
            ["11045", "文件消息", "message + file 段", "`name`/`file`/可选 `url`"],
            ["11046", "位置消息", "message + location 段", "lat/lon/title/content"],
            ["11047", "链接消息", "message + share 段", "url/title/content/image"],
            ["11048", "动图", "message + image 段", "依赖 url"],
            ["11049", "红包", "message + text 段", "占位文本"],
            ["11050", "名片", "message + contact 段", "数字 user_id 时；否则降级文本"],
            ["11063", "好友申请", "request.friend", "不可自动通过"],
            ["11066", "小程序", "message + text 段", "占位文本"],
            ["11068", "图文", "message + text 段", "占位文本"],
            ["11072", "群成员增加", "notice.group_increase", "sub_type=invite"],
            ["11073", "群成员减少", "notice.group_decrease", "leave/kick"],
            ["11074", "新增群", "notice.group_increase", "按成员增加处理"],
            ["11075", "主动退群", "notice.group_decrease", "sub_type=leave"],
            ["11076", "好友新增", "notice.friend_add", "—"],
            ["11077", "好友删除", "notice.notify", "sub_type=friend_delete"],
            ["11078", "群名称变化", "notice.notify", "sub_type=group_name_change"],
            ["11122", "自己的二维码", "忽略", "登录过程"],
            ["11123", "撤回", "notice.group_recall / friend_recall", "—"],
            ["11124", "视频号", "message + text 段", "占位文本"],
            ["11153", "刷新二维码", "忽略", "登录过程"],
            ["11173", "新增群通知", "notice.group_increase", "sub_type=invite，规则同 11072/11074"],
            ["11174", "二维码状态", "忽略", "登录过程"],
            ["11179", "登录成功（含公司名）", "更新 self_id，不上报", "企微通道"],
            ["11195", "视频号直播", "message + text 段", "占位文本"],
            ["11200", "未映射消息", "message（尽力透传 text + at）", "`read_receipt`/已读上报被过滤"],
            ["11201", "连接断开", "忽略", "由 WS 状态体现"],
            ["11202", "连接恢复", "忽略", "由 WS 状态体现"],
        ],
        widths=[2.6, 4.0, 6.2, 3.2],
        caption="表 13-1　微灵事件号速查",
        mono_cols=[0],
        size=8.5,
    )

    K.add_h2(doc, "13.2", "微灵指令号速查")
    K.add_para(doc, "下表列出改造版实际使用的指令（含出站媒体发送）：")
    K.add_table(
        doc,
        ["指令号", "功能", "改造版用途"],
        [
            ["11029", "发送文本消息", "私聊/群聊纯文本；群聊无有效 @ 时使用"],
            ["11035", "当前登录账号信息", "self_id 探测（HTTP）"],
            ["11036", "内部联系人列表", "`get_friend_list`"],
            ["11038", "群聊列表", "`get_group_list` / `get_group_info`"],
            ["11039", "联系人信息", "`get_stranger_info`"],
            ["11040", "群成员列表", "`get_group_member_info` / `get_group_member_list`（带 `getData:true`）"],
            ["11069", "群 @ 消息", "群聊带有效 @ 时使用；`at_list` 为 user_id 数组（数字 ID 以数字形式写入）"],
            ["11030", "发送图片", "出站 `image` 段（传本地绝对路径）"],
            ["11031", "发送文件", "出站 `file` 段（传本地绝对路径）"],
            ["11067", "发送视频", "出站 `video` 段（传本地绝对路径）"],
            ["11070", "发送动图", "出站 GIF `image` 段（传本地绝对路径）"],
            ["11115", "C2cCDN 上传", "未使用（出站媒体直接传本地文件路径）"],
            ["11170 / 11171", "CDN 文件下载", "未实现（入站媒体按需走 HTTP 直链下载）"],
        ],
        widths=[3.4, 5.2, 7.4],
        caption="表 13-2　微灵指令号速查",
        mono_cols=[0],
        size=8.5,
    )

    K.add_h2(doc, "13.3", "术语表")
    K.add_table(
        doc,
        ["术语", "含义"],
        [
            ["微灵 / WeComSever", "本项目改造的宿主程序，负责登录企业微信/个人微信并进行协议转换"],
            ["OneBot v11", "机器人应用与实现之间的标准协议，定义事件、动作与消息段格式"],
            ["反向 WebSocket（反向 WS）", "由 OneBot 实现作为服务端监听、机器人宿主作为客户端主动连接的部署方式"],
            ["`self_id`", "机器人账号 ID；微灵通过登录事件或 HTTP 探测获得"],
            ["`action` / `params` / `echo`", "动作请求的动作名、参数与回显标识；响应按 `echo` 关联"],
            ["`retcode`", "动作结果码：0 成功、1404 未知动作、500 执行失败"],
            ["`ling`", "改造版附在事件上的原始微灵事件字段，便于访问底层数据"],
            ["`conversation_id`", "微灵会话标识：`R:<群ID>` 为群、`S:<对方>_<自己>` 为私聊"],
            ["CDN 直链", "微灵事件中 `cdn.url` 等可直接下载的媒体地址"],
            ["KeepAlive（保活）", "`ClientWebSocket.KeepAliveInterval` 为 20s；断链由 WebSocket/TCP 层保活探测与发送失败发现，本版无固定静默断连"],
            ["指数退避", "重连间隔从 1s 开始逐次翻倍、上限 30s 的策略"],
            ["AstrBot / aiocqhttp", "常用的 OneBot v11 机器人框架及其适配器；本文以其为例"],
            ["消息能力矩阵", "第 6 章给出的「接收/发送支持哪些消息段、如何降级」总表"],
            ["`onebot_media_tmp`", "出站媒体落盘目录（位于微灵程序目录），后台清理 1 小时前的旧文件"],
            ["AI 审批插件", "AstrBot 侧第三方插件 astrbot_plugin_ai_approval，非桥接层内置功能（见第 11 章）"],
        ],
        widths=[4.2, 11.8],
        caption="表 13-3　术语表",
    )

    K.add_h2(doc, "13.4", "相关文件与生成信息")
    K.add_table(
        doc,
        ["文件/路径", "说明"],
        [
            ["`D:\\wecom-ling\\onebot\\src\\WxSocketClient.cs`", "OneBot v11 shim 源码（本文行为依据）"],
            ["`D:\\wecom-ling\\onebot\\output\\OneBotShim.dll`", "编译产物，部署时置于微灵程序目录；本文以 **2026-11-13 13:34:40** 最终部署构建为基线（53,760 字节）"],
            ["微灵程序目录 `onebot_v11.log`", "协议日志（5MB 轮转）"],
            ["微灵程序目录 `config.ini`", "微灵原配置，由界面维护"],
            ["`D:\\wecom-ling\\onebot\\docs\\`", "本文档的生成脚本与中间产物（python-docx）"],
        ],
        widths=[9.2, 6.8],
        caption="表 13-4　相关文件",
        mono_cols=[0],
        size=9,
    )
    K.add_bullet(doc, "部署基线（**最终部署构建**，2026-11-13 13:34 构建）：`微灵-OneBotV11-1.11.0.exe`（51,494,912 字节，13:34:46，SHA256 `3A54DF0E9B44889796B7A2E806018F963E97847F1B01A31869BBE633BB96DED8`）；`OneBotShim.dll`（53,760 字节，13:34:40，SHA256 `4F0D7EA2372B0BF7109C6F6AEC87D5D6E41A523313B8264B0D93ECC6F46FD714`）。")

    K.add_h2(doc, "13.5", "版本记录")
    K.add_table(
        doc,
        ["版本", "日期", "说明"],
        [
            ["v1.0", "2026-11-12", "初版：基于微灵 1.11.0 改造版与 OneBotShim 实际实现编写"],
            ["v1.1", "2026-11-12", "同步 13:12:49 构建：媒体按需下载与 CDN 直链返回、11173 上报、"
                                    "接收循环改为 socket 轮询（发送线程静默看门狗保留）"],
            ["v1.2", "2026-11-12", "同步 13:21:15 构建：彻底移除入站静默看门狗（`ReceiveWatchdogMs` 常量删除），"
                                    "空闲连接不再周期性重连；4.6 改述为链路检测"],
            ["v1.3", "2026-11-12", "新增 3.1「运行环境要求（重要）」：安全软件/终端管控限制运行目录时，"
                                    "会出现 8060 不监听、企微注入失败等与补丁无关的故障；给出症状判别、正确目录与本机实测证据"],
            ["v1.4", "2026-11-12", "实现基线（19:34 构建）：出站图片/视频/文件/动图发送、位置/链接/名片结构化段、`get_group_member_list` 与 `get_group_msg_history`、reply 忽略与 @全体降级"],
            ["v1.5", "2026-11-13", "本文档：新增第 6 章消息能力矩阵、第 11 章 AI 审批插件；同步 11200 透传、"
                                    "位置/链接/名片结构化、出站媒体与临时目录说明；保留 v1.3 运行环境内容"],
            ["v1.6", "2026-11-13", "修订部署基线为最终部署构建（`OneBotShim.dll` 4F0D7EA2…、exe 3A54DF0E…）；"
                                    "补充第 11 章插件源码位置、实现状态与安装路径说明"],
        ],
        widths=[2.4, 3.0, 10.6],
        caption="表 13-5　版本记录",
    )
    K.add_caption_table_note(doc, "本文档由脚本生成，内容以 `WxSocketClient.cs` 实际行为为准；"
                                  "实现更新后应同步修订第 5、6、7、8、12 章。")
