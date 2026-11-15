# -*- coding: utf-8 -*-
"""第 8 章：媒体与文件。"""
import docx_kit as K


def build(doc):
    K.add_h1(doc, "8", "媒体与文件", page_break=True)
    K.add_para(doc, "本章说明消息中的媒体与文件如何表示、如何取回，以及出站媒体如何发送。"
                    "总览：**入站媒体依赖事件自带的 CDN 直链；出站媒体已支持图片、视频、文件、动图**，"
                    "语音（`record`）仅有入站能力。")

    K.add_h2(doc, "8.1", "入站媒体")
    K.add_para(doc, "微灵的事件会在消息段中携带 CDN 信息。改造版按以下规则生成 OneBot 消息段，"
                    "并同时把可用的 CDN 直链登记到媒体缓存，供后续动作查询。")
    K.add_table(
        doc,
        ["消息类型", "OneBot 消息段", "事件中的媒体字段", "媒体缓存键"],
        [
            ["11042 图片", "`image`", "`file` 取 `cdn.md5`/`file_name`/`url`，`url` 取 `cdn.url`（回退 `md_url`/`ld_url`）", "`cdn.md5` / `file_name` / 原始 URL"],
            ["11048 动图", "`image`（GIF）", "`url` 取事件 `url`，`file` 取 `name`", "`name`"],
            ["11043 视频", "`video`", "`file` 取 `cdn.file_name`/`md5`，`url` 取 `cdn.url`", "`cdn.file_name` / `md5`"],
            ["11044 语音", "`record`", "只有 `file` = `c2c_cdn.file_id`，事件不带下载 URL", "不登记"],
            ["11045 文件", "`file`", "`name` 取 `cdn.file_name`；`file` 优先 `cdn.url`（无直链时为文件名）；有 `cdn.url` 时附加 `url`", "`server_id` / `local_id`"],
        ],
        widths=[2.6, 2.4, 6.8, 4.2],
        caption="表 8-1　入站媒体映射",
        mono_cols=[1],
    )
    K.add_bullet(doc, "有直链的媒体会登记到媒体缓存；`get_image`/`get_record`/`get_file` 可凭消息段里的 `file` 值取回"
                       "（按需下载后返回 `base64://`），`get_*_file_url` 可拿到原始 CDN 地址（见 7.6 节）。")
    K.add_bullet(doc, "无 `cdn.url`（且没有回退地址）时无法取回，消息段会降级为文本 `[图片]`/`[视频]`/`[动图]`。")
    K.add_bullet(doc, "`record` 只有 `file_id`，没有直链；`get_record` 只有在调用方传入 http(s) URL 时才可能成功。")
    K.add_bullet(doc, "位置、链接、名片分别映射为 `location`、`share`、`contact` 段，不涉及媒体缓存，详见 5.3.3 节与第 6 章。")
    K.add_callout(doc, "info", "CDN 地址通常有时效。收到消息后应尽快处理媒体；需要长期保存的内容请在业务流程里"
                                "自行转存，不要依赖事件里的 `url` 永久有效。", title="CDN 时效")

    K.add_h2(doc, "8.2", "出站媒体")
    K.add_para(doc, "出站媒体已支持。OneBot 消息中的媒体段由 shim 统一落地为本地临时文件，再以微灵发送指令发出；"
                    "一条含媒体与文本的消息会被拆成多条微灵命令（媒体在前、文本在后）。")
    K.add_h3(doc, "8.2.1", "支持的媒体与微灵指令")
    K.add_table(
        doc,
        ["OneBot 段", "微灵指令", "传给微灵的字段", "说明"],
        [
            ["`image`", "11030", "`{conversation_id, file: <本地绝对路径>}`", "普通图片"],
            ["`video`", "11067", "`{conversation_id, file: <本地绝对路径>}`", "视频文件"],
            ["`file`", "11031", "`{conversation_id, file: <本地绝对路径>, file_name}`", "`file_name` 取段内 `name`/`file_name`"],
            ["GIF（`image` + gif 标识）", "11070", "`{conversation_id, file: <本地绝对路径>}`", "`type`/`sub_type` 含 `gif` 或扩展名 `.gif`"],
            ["`record`", "—", "—", "不支持：微灵未提供语音发送指令"],
        ],
        widths=[3.6, 2.4, 5.6, 4.4],
        caption="表 8-2　出站媒体与微灵指令",
        mono_cols=[0, 1],
    )
    K.add_h3(doc, "8.2.2", "媒体来源与临时文件")
    K.add_bullet(doc, "`file` 字段支持：`base64://` 内联数据、http(s) URL、`file://` URI、本地绝对路径。")
    K.add_bullet(doc, "http(s) 源由 shim 下载（15s 超时），单文件超过约 32MB 或下载失败直接返回失败。")
    K.add_bullet(doc, "`base64://` 内容解码失败、本地路径不是绝对路径、文件不存在，都会返回明确错误（见表 7-2）。")
    K.add_bullet(doc, "下载/解码后的内容写入**微灵程序目录**的 `onebot_media_tmp` 临时目录，文件名形如 "
                       "`media_<时间戳>_<自增ID>.<扩展名>`；扩展名优先按文件头判断（png/jpg/gif/webp/mp4），"
                       "否则取源文件名扩展。")
    K.add_bullet(doc, "后台会清理该目录中 1 小时前的旧文件；磁盘紧张时可手工清空，不影响运行中的发送。")
    K.add_bullet(doc, "临时文件随发送量增长而占用磁盘（单文件最大约 32MB）：高频发图/发文件的实例应定期检查 "
                       "`onebot_media_tmp` 体积，避免系统盘被占满。")
    K.add_bullet(doc, "媒体命令执行失败时整条动作返回失败（`发送媒体失败(<类型>): ...`），已发出的媒体不会撤回。")
    K.add_code(doc, [
        "{",
        "  \"action\": \"send_group_msg\",",
        "  \"params\": {",
        "    \"group_id\": 24680001,",
        "    \"message\": [",
        "      { \"type\": \"image\", \"data\": { \"file\": \"base64://iVBORw0KGgo...\" } },",
        "      { \"type\": \"text\", \"data\": { \"text\": \" 请看这张图\" } }",
        "    ]",
        "  }",
        "}",
        "",
        "// shim 侧大致行为",
        "// 1. 解码 base64 -> onebot_media_tmp\\media_20261113...png",
        "// 2. 调用微灵 11030 {conversation_id: \"R:24680001\", file: \"<程序目录>\\onebot_media_tmp\\media_...png\"}",
        "// 3. 调用微灵 11029 {conversation_id: \"R:24680001\", content: \" 请看这张图\"}",
    ], caption="代码 8-1　发送带图片的群消息")
    K.add_h3(doc, "8.2.3", "已知约束")
    K.add_bullet(doc, "每条媒体都会先落盘，微灵进程需要对程序目录有写权限；建议使用可信目录运行（见 3.1 节）。")
    K.add_bullet(doc, "媒体与文本是两条微灵命令，群成员会看到媒体消息与文本消息各一条。")
    K.add_bullet(doc, "微灵侧命令执行失败不会自动重试；对端可根据 `retcode`/`msg` 决定是否重发。")

    K.add_h2(doc, "8.3", "媒体缓存与本地媒体服务")
    K.add_table(
        doc,
        ["机制", "设计用途", "当前状态"],
        [
            ["媒体登记（CDN 直链）", "记录入站消息自带的 `cdn.url`，供 7.6 节的媒体动作查询", "已启用"],
            ["按需 HTTP 下载", "`get_image`/`get_record`/`get_file` 命中缓存后下载并返回 `base64://`", "已启用（15s 超时、32MB 截断）"],
            ["本地媒体服务", "缓存到字节内容时启动 `http://127.0.0.1:18890–18920/media/<key>`", "正常路径不会启动（无写入字节缓存的调用点）"],
        ],
        widths=[3.2, 7.6, 5.2],
        caption="表 8-3　媒体缓存与本地媒体服务",
    )
    K.add_bullet(doc, "本地媒体服务绑定的端口范围为 `18890–18920`，仅在存在字节缓存且 `get_*_file_url` 命中时才会启动。")
    K.add_bullet(doc, "按需下载不落盘：`base64://` 内容只在动作响应中返回，不保存到本地。")
    K.add_bullet(doc, "下载失败会记录 `media download failed: <url> <原因>`；超过大小上限记录 `media too large, truncated: <url>`。")
    K.add_bullet(doc, "不要依赖本地媒体服务做代理：正常部署下它不会启动，直接使用 CDN 直链或 `base64://` 内容。")

    K.add_h2(doc, "8.4", "媒体相关限制汇总")
    K.add_table(
        doc,
        ["限制", "说明", "影响"],
        [
            ["入站依赖 CDN 直链", "无 `cdn.url` 的媒体无法取回", "消息段降级为文本占位；媒体动作失败"],
            ["语音只有 `file_id`", "11044 事件不带下载 URL", "`get_record` 基本不可用"],
            ["CDN 有时效", "`url` 可能过期", "需及时处理或自行转存"],
            ["出站需本地化", "base64/URL/file/绝对路径都会先落盘", "需要程序目录写权限与临时空间"],
            ["出站有大小上限", "约 32MB，超过截断或失败", "大文件建议改用文件链接/网盘"],
            ["语音不能发送", "微灵无语音发送指令", "`record` 段返回失败"],
            ["媒体与文本分多条", "一条 OneBot 消息可能产生多条微灵消息", "群成员看到多条记录"],
        ],
        widths=[3.6, 7.0, 5.4],
        caption="表 8-4　媒体限制汇总",
    )
