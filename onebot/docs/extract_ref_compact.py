# -*- coding: utf-8 -*-
"""紧凑提取参考文档正文 + 全部表格文本。"""
from docx import Document

PATH = r"D:\wecom-ling\微灵对接文档.docx"
OUT = r"D:\wecom-ling\onebot\docs\ref_compact.txt"

doc = Document(PATH)
lines = []

def para_line(p):
    text = p.text.strip()
    if not text:
        return None
    # 记录首个 run 的字体信息
    info = ""
    for r in p.runs:
        if r.text.strip():
            sz = r.font.size.pt if r.font.size else None
            info = " {font=%s sz=%s b=%s}" % (r.font.name, sz, r.font.bold)
            break
    return "[p%s]%s %s" % (p.style.name, info, text)

for p in doc.paragraphs:
    s = para_line(p)
    if s:
        lines.append(s)

for ti, table in enumerate(doc.tables):
    lines.append("\n===== TABLE %d rows=%d cols=%d =====" % (ti, len(table.rows), len(table.columns)))
    # 合并单元格导致的重复文本：按行输出单元格
    for ri, row in enumerate(table.rows):
        cells = []
        seen = set()
        for c in row.cells:
            t = c.text.replace("\n", " / ").strip()
            if id(c._tc) in seen:
                continue
            seen.add(id(c._tc))
            cells.append(t)
        lines.append("  r%d: %s" % (ri, " | ".join(cells)))

with open(OUT, "w", encoding="utf-8") as f:
    f.write("\n".join(lines))
print("lines", len(lines))
