# -*- coding: utf-8 -*-
"""v1.5：内容修订（交叉引用已由 patch_refs_v15.py 处理）。

覆盖：前言/第 1 章能力表述、第 5 章结构化消息段、第 2 章验证表、第 12 章限制与
FAQ、第 13 章附录（事件/指令/术语/文件/版本）。
"""
import io
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))

EDITS = []  # (file, old, new)


def add(f, old, new):
    EDITS.append((f, old, new))


# ---------------------------------------------------------------- ch_intro.py
add("ch_intro.py",
    "阅读第 4～7 章，了解连接握手、事件帧、动作请求与错误码约定。",
    "阅读第 4～7 章，了解连接握手、事件帧、消息能力矩阵、动作请求与错误码约定。")
add("ch_intro.py",
    "阅读第 8 章媒体说明、第 10 章兼容性说明与附录中的事件号/指令号速查表。",
    "阅读第 6 章消息能力矩阵、第 8 章媒体说明、第 10 章兼容性说明与附录中的事件号/指令号速查表。")
add("ch_intro.py",
    '["自己实现一个 OneBot v11 服务端来对接", "第 4 章 连接与协议、第 5～7 章 事件与动作"],',
    '["自己实现一个 OneBot v11 服务端来对接", "第 4 章 连接与协议、第 5～7 章 事件与动作"],\n'
    '            ["了解收发消息支持哪些类型、如何降级", "第 6 章 消息能力矩阵"],\n'
    '            ["启用第三方 AI 审批插件", "第 11 章 AI 审批插件"],')
add("ch_intro.py",
    "事件内 `cdn.url` 直链；媒体动作可按需下载后返回 base64（见第 8 章）",
    "事件内 `cdn.url` 直链；入站按需下载，出站支持图片/视频/文件/动图（见第 6、8 章）")
add("ch_intro.py",
    "**能力边界清晰**：首版出站仅支持文本与 @，入站媒体取回依赖事件自带 CDN 直链，",
    "**能力边界清晰**：出站支持文本、@ 与图片/视频/文件/动图（语音除外），入站媒体取回依赖事件自带 CDN 直链，")
add("ch_intro.py",
    "首版不提供出站图片/语音/视频/文件发送，也不支持撤回、群管理、请求处理等动作；",
    "改造版不提供语音发送，也不支持撤回、群管理、请求处理等动作；")
add("ch_intro.py", "详见 6.6、7.7 节与 12.3 节。", "详见 7.6、7.7 节与 12.3 节。")

# --------------------------------------------------------------- ch_events.py
add("ch_events.py",
    '["11046", "位置消息", "message + text 段", "占位文本 `[位置] <标题> <地址>`"],',
    '["11046", "位置消息", "message + location 段", "`lat`、`lon`、`title`、`content`（取 `address`）"],')
add("ch_events.py",
    '["11047", "链接消息", "message + text 段", "占位文本 `[链接] <标题> <URL>`"],',
    '["11047", "链接消息", "message + share 段", "`url`、`title`、`content`（取 `desc`）、可选 `image`"],')
add("ch_events.py",
    '["11050", "名片", "message + text 段", "占位文本 `[名片] <昵称>`"],',
    '["11050", "名片", "message + contact 段（user_id 为数字时）", "`type: qq`、`id`（数字 user_id）；否则降级文本"],')
add("ch_events.py",
    "`file` 值按需取回（见 7.6 节与第 8 章）。",
    "`file` 值按需取回（见 7.6 节与第 8 章）。位置、链接、名片分别映射为 location / share / contact 段，"
    "详见 5.3.3 节与第 6 章。")
add("ch_events.py",
    "以下情况会跳过某个 @：`user_id` 不是数字、`user_id` 等于 `self_id`。",
    "`at_list` 中只有 `user_id` 可解析为数字时才生成 `at` 段；@自己也会保留。"
    "`rich_text` 不逐段解析：只取 `content`/`text` 与 `at_list`（见第 6 章）。")
STRUCT = '''    K.add_h3(doc, "5.3.3", "结构化消息段（位置、链接、名片）")
    K.add_bullet(doc, "**位置（11046）→ `location` 段**：`lat` 取 `latitude`/`lat`，`lon` 取 `longitude`/`lon`/`lng`，"
                       "`title` 取 `title`，`content` 取 `address`。")
    K.add_bullet(doc, "**链接（11047）→ `share` 段**：`url` 取 `url`，`title` 取 `title`，`content` 取 `desc`，"
                       "`image` 取 `image_url`（可选）；`url` 与 `title` 都为空时降级为文本。")
    K.add_bullet(doc, "**名片（11050）→ `contact` 段**：`type` 固定为 `qq`，`id` 为数字 `user_id`；"
                       "`user_id` 不是数字时降级为文本 `[名片] <昵称>`。")
    K.add_bullet(doc, "以上字段缺失时会给出空字符串；对端应做容错，不要把空串当作有效值。")

    K.add_h3(doc, "5.3.4", "消息公共字段")'''
add("ch_events.py", 'K.add_h3(doc, "5.3.3", "消息公共字段")', STRUCT)

# ---------------------------------------------------------------- ch_quick.py
add("ch_quick.py",
    '["AstrBot 发送文本", "企业微信成功收到消息，动作返回 `status: ok`", "查看失败 `retcode` 与 `msg`（7.2 节、第 12 章）"],',
    '["AstrBot 发送文本", "企业微信成功收到消息，动作返回 `status: ok`", "查看失败 `retcode` 与 `msg`（7.2 节、第 12 章）"],\n'
    '            ["AstrBot 发送图片/文件", "企业微信收到对应媒体", "检查临时目录 `onebot_media_tmp`、源文件/URL 与大小限制（8.2 节）"],')

# --------------------------------------------------------------- ch_limits.py
add("ch_limits.py",
    '["11074 / 11173 映射为 group_increase", "“新增群 / 新增群通知”被当作“成员增加”上报，`sub_type` 为 `invite`", "对端不要用这两个事件判断建群"],',
    '["11074 / 11173 映射为 group_increase", "“新增群 / 新增群通知”被当作“成员增加”上报，`sub_type` 为 `invite`", "对端不要用这两个事件判断建群"],\n'
    '            ["11046/11047/11050 已结构化", "位置→location、链接→share、名片（数字 user_id）→contact；不满足条件时降级文本", "对端可直接解析结构化段"],')
add("ch_limits.py",
    'K.add_bullet(doc, "**出站仅文本 + @**：图片/语音/视频/文件段返回 500，见 8.2 节。")',
    '''    K.add_bullet(doc, "**出站媒体支持但有约束**：支持 text/at/image/video/file/gif（图片 11030、视频 11067、文件 11031、"
                       "动图 11070）；`record` 不支持（微灵无语音发送指令）。详见 8.2 节。")
    K.add_bullet(doc, "**reply 与 face 段被忽略**：AstrBot 默认回复带的 `reply` 段不会导致失败，正文照常发送；`face` 段忽略。")
    K.add_bullet(doc, "**@全体降级为纯文本**：`at` 的 `qq` 为 `all` 时不写入 `at_list`，群消息退化为普通文本（11029）。")
    K.add_bullet(doc, "**媒体与文本分多条发送**：一条含媒体的 OneBot 消息会先按媒体依次调用 11030/11031/11067/11070，"
                       "再发送文本段（11069/11029），对端会看到多条微灵侧记录。")
    K.add_bullet(doc, "**rich_text 不逐段解析**：文本事件只取 `content`/`text` 与 `at_list`，图文混排请读取 `ling`。")
    K.add_bullet(doc, "**get_group_msg_history 返回空数组**：微灵不提供历史消息查询，只能查本进程 500 条缓存。")''')
add("ch_limits.py",
    "本地媒体服务仅在缓存到字节时兜底、正常不会启动；出站媒体仍只支持文本与 @。",
    "出站媒体支持图片/视频/文件/动图（语音除外），但需要可访问的本地文件或可下载的 URL，"
    "临时文件写入程序目录 `onebot_media_tmp`；本地媒体服务仅在缓存到字节时兜底、正常不会启动。")
add("ch_limits.py",
    '"查事件消息段的 `data.url`；传入与事件一致的 `file` 值；看日志 `media download failed`"],',
    '"查事件消息段的 `data.url`；传入与事件一致的 `file` 值；看日志 `media download failed`"],\n'
    '            ["发送图片/视频/文件失败", "临时目录不可写、源文件/URL 不可访问、超过 32MB 或微灵发送失败", "检查 `onebot_media_tmp` 写权限、源地址与大小；读响应 `msg` 与日志（8.2 节）"],')

# -------------------------------------------------------------- ch_appendix.py
add("ch_appendix.py",
    '["11046", "位置消息", "message + text 段", "占位文本"],',
    '["11046", "位置消息", "message + location 段", "lat/lon/title/content"],')
add("ch_appendix.py",
    '["11047", "链接消息", "message + text 段", "占位文本"],',
    '["11047", "链接消息", "message + share 段", "url/title/content/image"],')
add("ch_appendix.py",
    '["11050", "名片", "message + text 段", "占位文本"],',
    '["11050", "名片", "message + contact 段", "数字 user_id 时；否则降级文本"],')
add("ch_appendix.py",
    '["11040", "群成员列表", "`get_group_member_info`"],',
    '["11040", "群成员列表", "`get_group_member_info` / `get_group_member_list`（带 `getData:true`）"],')
add("ch_appendix.py",
    '["11030", "发送图片", "未实现（扩展参考）"],',
    '["11030", "发送图片", "出站 `image` 段（传本地绝对路径）"],')
add("ch_appendix.py",
    '["11031", "发送文件", "未实现（扩展参考）"],',
    '["11031", "发送文件", "出站 `file` 段（传本地绝对路径）"],')
add("ch_appendix.py",
    '["11067", "发送视频", "未实现（扩展参考）"],',
    '["11067", "发送视频", "出站 `video` 段（传本地绝对路径）"],')
add("ch_appendix.py",
    '["11070", "发送动图", "未实现（扩展参考）"],',
    '["11070", "发送动图", "出站 GIF `image` 段（传本地绝对路径）"],')
add("ch_appendix.py",
    '["11115", "C2cCDN 上传", "未实现（扩展参考，用于换取 file_id）"],',
    '["11115", "C2cCDN 上传", "未使用（出站媒体直接传本地文件路径）"],')
add("ch_appendix.py",
    '["11170 / 11171", "CDN 文件下载", "未实现（首版无二进制下载通道）"],',
    '["11170 / 11171", "CDN 文件下载", "未实现（入站媒体按需走 HTTP 直链下载）"],')
add("ch_appendix.py",
    "下表列出改造版实际使用的指令，以及扩展出站媒体时可能用到的指令（后者未在首版实现）：",
    "下表列出改造版实际使用的指令（含出站媒体发送）：")
add("ch_appendix.py",
    "本文以 **2026-11-12 13:21:15** 构建为基线（47,104 字节）",
    "本文以 **2026-11-12 19:34:22** 构建为基线（48,128 字节）")
add("ch_appendix.py",
    'caption="表 13-4　相关文件",\n        mono_cols=[0],\n        size=9,\n    )',
    'caption="表 13-4　相关文件",\n        mono_cols=[0],\n        size=9,\n    )\n'
    '    K.add_bullet(doc, "部署产物：`微灵-OneBotV11-1.11.0.exe`（51,489,280 字节，2026-11-12 19:35:10，'
    'SHA256 `7999CF660318622C83F2F784BE76449965C23CB709378F90A808C911EBBBF0AA`）；'
    '`OneBotShim.dll`（48,128 字节，19:34:22，SHA256 `8ADFBD0242E0CB6183E007373752C2DB5FC3F93E2073883A29BE274C6FED95C0`）。")')
add("ch_appendix.py",
    '"常用的 OneBot v11 机器人框架及其适配器；本文以其为例"],',
    '"常用的 OneBot v11 机器人框架及其适配器；本文以其为例"],\n'
    '            ["消息能力矩阵", "第 6 章给出的「接收/发送支持哪些消息段、如何降级」总表"],\n'
    '            ["`onebot_media_tmp`", "出站媒体落盘目录（位于微灵程序目录），后台清理 1 小时前的旧文件"],\n'
    '            ["AI 审批插件", "AstrBot 侧第三方插件 astrbot_plugin_ai_approval，非桥接层内置功能（见第 11 章）"],')
add("ch_appendix.py",
    '"会出现 8060 不监听、企微注入失败等与补丁无关的故障；给出症状判别、正确目录与本机实测证据"],',
    '"会出现 8060 不监听、企微注入失败等与补丁无关的故障；给出症状判别、正确目录与本机实测证据"],\n'
    '            ["v1.4", "2026-11-12", "实现基线（19:34 构建）：出站图片/视频/文件/动图发送、位置/链接/名片结构化段、'
    '`get_group_member_list` 与 `get_group_msg_history`、reply 忽略与 @全体降级"],\n'
    '            ["v1.5", "2026-11-13", "本文档：新增第 6 章「消息能力矩阵」与第 11 章「AI 审批插件」；'
    '动作 API、媒体章节与限制按 19:34 构建同步；保留 v1.3 运行环境内容"],')
add("ch_appendix.py",
    "实现更新后应同步修订第 5、6、7、10 章。",
    "实现更新后应同步修订第 5、6、7、8、12 章。")

# ---------------------------------------------------------------- docx_kit.py
add("docx_kit.py", '("文档版本", "v1.3"),', '("文档版本", "v1.5"),')


def main():
    cache = {}
    missing = []
    for f, old, new in EDITS:
        if f not in cache:
            with io.open(os.path.join(HERE, f), "r", encoding="utf-8") as fh:
                cache[f] = fh.read()
        if old not in cache[f]:
            missing.append((f, old))
    if missing:
        print("MISSING %d:" % len(missing))
        for f, old in missing:
            print("--- %s: %r" % (f, old[:100]))
        sys.exit(1)
    for f, old, new in EDITS:
        cache[f] = cache[f].replace(old, new, 1)
    for f, text in cache.items():
        with io.open(os.path.join(HERE, f), "w", encoding="utf-8", newline="\n") as fh:
            fh.write(text)
        print("patched", f)
    print("done")


if __name__ == "__main__":
    main()
