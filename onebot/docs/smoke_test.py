# -*- coding: utf-8 -*-
"""docx_kit 冒烟测试：验证样式/表格/代码块/页眉页脚可用。"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import docx_kit as K

doc = K.new_document()
K.setup_header_footer(doc, "微灵 OneBot v11 对接文档")
K.add_cover(doc)
K.add_toc(doc, [(1, "1", "改造概述"), (2, "1.1", "背景")])
K.add_h1(doc, "1", "改造概述", page_break=False)
K.add_para(doc, "这是正文测试，包含 **加粗** 和 `inline code` 以及普通文字。")
K.add_callout(doc, "warn", "这是一个注意事项。")
K.add_code(doc, ['{"a": 1, "b": "中文"}', "第二行"], caption="代码 1-1 示例")
K.add_table(doc, ["列一", "列二", "列三"],
            [["11041", "文本", "hello"], ["11042", "图片", "https://example.com/x.png"]],
            widths=[3.0, 4.0, 9.0], caption="表 1-1 测试表", first_col_center=True)
K.add_bullet(doc, "项目符号测试")
K.add_step(doc, 1, "步骤测试")

out = r"D:\wecom-ling\onebot\docs\_smoke.docx"
doc.save(out)
from docx import Document
d = Document(out)
print("saved", os.path.getsize(out), "bytes; paras", len(d.paragraphs), "tables", len(d.tables))
for p in d.paragraphs[:12]:
    print(repr(p.style.name), p.text[:60])
