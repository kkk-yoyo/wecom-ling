# -*- coding: utf-8 -*-
"""生成《微灵 OneBot v11 对接文档.docx》。

用法：
    python build_doc.py
输出：
    D:\\wecom-ling\\微灵 OneBot v11 对接文档.docx
"""
import os
import sys
import traceback

# 允许从任意工作目录导入同目录模块
HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)

import docx_kit as K
from docx import Document
from docx.shared import Pt

import ch_intro
import ch_quick
import ch_deploy
import ch_protocol
import ch_events
import ch_capabilities
import ch_actions
import ch_media
import ch_personal
import ch_compat
import ch_approval
import ch_limits
import ch_appendix

OUT_PATH = r"D:\wecom-ling\微灵 OneBot v11 对接文档.docx"

TOC = [
    (1, "", "前言"),
    (2, "", "文档目的"),
    (2, "", "读者对象"),
    (2, "", "与官方文档的关系"),
    (2, "", "阅读指引"),
    (2, "", "排版约定"),

    (1, "1", "改造概述"),
    (2, "1.1", "改造背景"),
    (2, "1.2", "改造后架构"),
    (2, "1.3", "改造前后对比"),
    (2, "1.4", "设计原则与兼容性"),

    (1, "2", "快速开始"),
    (2, "2.1", "准备工作"),
    (2, "2.2", "AstrBot 侧配置"),
    (2, "2.3", "微灵侧配置"),
    (2, "2.4", "启动与验证"),
    (2, "2.5", "首次接入常见问题"),

    (1, "3", "部署与配置"),
    (2, "3.1", "运行环境要求（重要）"),
    (2, "3.2", "部署形态"),
    (2, "3.3", "连接地址规则"),
    (2, "3.4", "Token 与鉴权"),
    (2, "3.5", "配置项说明"),
    (2, "3.6", "对接其他 OneBot v11 实现"),
    (2, "3.7", "网络与端口"),

    (1, "4", "连接与协议"),
    (2, "4.1", "连接流程"),
    (2, "4.2", "握手请求头"),
    (2, "4.3", "元事件：连接与心跳"),
    (2, "4.4", "事件帧格式"),
    (2, "4.5", "动作请求与响应"),
    (2, "4.6", "重连、心跳与链路检测"),

    (1, "5", "事件映射"),
    (2, "5.1", "通用规则"),
    (2, "5.2", "消息事件总览"),
    (2, "5.3", "消息段字段详解"),
    (2, "5.4", "撤回与好友申请"),
    (2, "5.5", "群事件与通知"),
    (2, "5.6", "登录与状态事件"),
    (2, "5.7", "被忽略的事件"),
    (2, "5.8", "ling：原始微灵事件"),

    (1, "6", "消息能力矩阵"),
    (2, "6.1", "接收方向：微灵 → OneBot"),
    (2, "6.2", "发送方向：OneBot → 微灵"),
    (2, "6.3", "媒体落盘与发送顺序"),
    (2, "6.4", "能力边界速览"),

    (1, "7", "动作接口"),
    (2, "7.1", "支持的动作总览"),
    (2, "7.2", "发送消息"),
    (2, "7.3", "账号与状态"),
    (2, "7.4", "群与好友查询"),
    (2, "7.5", "消息查询与能力"),
    (2, "7.6", "媒体与文件动作"),
    (2, "7.7", "不支持的动作与错误处理"),

    (1, "8", "媒体与文件"),
    (2, "8.1", "入站媒体"),
    (2, "8.2", "出站媒体"),
    (2, "8.3", "媒体缓存与本地媒体服务"),
    (2, "8.4", "媒体相关限制汇总"),

    (1, "9", "个人微信通道（实验性）"),
    (2, "9.1", "启用方式"),
    (2, "9.2", "事件映射"),
    (2, "9.3", "动作支持"),
    (2, "9.4", "个微通道限制"),

    (1, "10", "保留能力与兼容性"),
    (2, "10.1", "微灵原有 HTTP API 保留"),
    (2, "10.2", "HTTP 事件回调"),
    (2, "10.3", "配置与界面"),
    (2, "10.4", "通道互斥与保持在线"),
    (2, "10.5", "升级与回滚"),
    (2, "10.6", "安全注意事项"),

    (1, "11", "AI 审批插件"),
    (2, "11.1", "功能概览"),
    (2, "11.2", "工作流程"),
    (2, "11.3", "配置项"),
    (2, "11.4", "部署步骤"),
    (2, "11.5", "边界与说明"),

    (1, "12", "已知限制与排障"),
    (2, "12.1", "self_id 与连接相关限制"),
    (2, "12.2", "事件相关限制"),
    (2, "12.3", "动作与发送相关限制"),
    (2, "12.4", "媒体相关限制"),
    (2, "12.5", "常见问题速查"),
    (2, "12.6", "日志阅读指南"),

    (1, "13", "附录"),
    (2, "13.1", "微灵事件号速查"),
    (2, "13.2", "微灵指令号速查"),
    (2, "13.3", "术语表"),
    (2, "13.4", "相关文件与生成信息"),
    (2, "13.5", "版本记录"),
]


def build():
    doc = K.new_document()
    K.add_cover(doc)
    K.add_toc(doc, TOC)

    modules = [
        ch_intro, ch_quick, ch_deploy, ch_protocol, ch_events, ch_capabilities,
        ch_actions, ch_media, ch_personal, ch_compat, ch_approval, ch_limits,
        ch_appendix,
    ]
    for mod in modules:
        sys.stdout.write("building %s ...\n" % mod.__name__)
        mod.build(doc)

    K.setup_header_footer(doc, "微灵 OneBot v11 对接文档")

    os.makedirs(os.path.dirname(OUT_PATH), exist_ok=True)
    doc.save(OUT_PATH)
    size = os.path.getsize(OUT_PATH)
    sys.stdout.write("saved: %s (%d bytes)\n" % (OUT_PATH, size))
    return OUT_PATH


def validate(path):
    doc = Document(path)
    paras = doc.paragraphs
    tables = doc.tables
    nonempty = [p for p in paras if p.text.strip()]
    h1 = [p.text for p in paras if p.style is not None and p.style.name == "Heading 1"]
    h2 = [p.text for p in paras if p.style is not None and p.style.name == "Heading 2"]
    h3 = [p.text for p in paras if p.style is not None and p.style.name == "Heading 3"]
    text_all = "\n".join(p.text for p in paras)
    for t in tables:
        for row in t.rows:
            for cell in row.cells:
                text_all += "\n" + cell.text
    bad = text_all.count("\ufffd")

    print("=" * 66)
    print("VALIDATION")
    print("  file        :", path)
    print("  size        : %d bytes" % os.path.getsize(path))
    print("  paragraphs  : %d (non-empty %d)" % (len(paras), len(nonempty)))
    print("  tables      : %d" % len(tables))
    print("  headings H1 : %d" % len(h1))
    print("  headings H2 : %d" % len(h2))
    print("  headings H3 : %d" % len(h3))
    print("  U+FFFD      : %d" % bad)
    print("  table rows  :", [len(t.rows) for t in tables])
    print("  H1 list     :")
    for t in h1:
        print("     -", t)
    print("=" * 66)

    assert bad == 0, "发现乱码字符 U+FFFD"
    assert len(h1) == 14, "H1 数量应为 14（前言 + 13 章），实际 %d" % len(h1)
    assert len(h2) == 76, "H2 数量应为 76，实际 %d" % len(h2)
    assert len(tables) >= 40, "表格数量异常：%d" % len(tables)
    # 抽查关键内容
    for kw in ["OneBot v11", "ws://127.0.0.1:6200/ws", "lifecycle", "send_group_msg",
               "X-Self-ID", "ling", "retcode", "onebot_v11.log",
               "消息能力矩阵", "onebot_media_tmp", "astrbot_plugin_ai_approval",
               "get_group_member_list"]:
        assert kw in text_all, "缺少关键内容：%s" % kw
    print("OK: 文档校验通过")
    return {
        "path": path,
        "size": os.path.getsize(path),
        "paragraphs": len(paras),
        "tables": len(tables),
        "h1": len(h1),
        "h2": len(h2),
        "h3": len(h3),
    }


if __name__ == "__main__":
    try:
        p = build()
        validate(p)
    except Exception:
        traceback.print_exc()
        sys.exit(1)
