# -*- coding: utf-8 -*-
"""交付前自检：表格列宽、题注顺序、标题层级、正文导出、超长单元格。

用法: python inspect_doc.py
"""
import re
import sys
from docx import Document
from docx.shared import Cm
from docx.oxml.ns import qn

PATH = r"D:\wecom-ling\微灵 OneBot v11 对接文档.docx"
OUT_TEXT = r"D:\wecom-ling\onebot\docs\doc_text.txt"

doc = Document(PATH)
print("paragraphs:", len(doc.paragraphs), "tables:", len(doc.tables))

# ---- 表格列宽 ----
print("\n== table width check ==")
bad = []
for i, t in enumerate(doc.tables, 1):
    widths = []
    tblGrid = t._tbl.tblGrid
    if tblGrid is not None:
        for gc in tblGrid.findall(qn("w:gridCol")):
            w = gc.get(qn("w:w"))
            if w:
                widths.append(int(w) / 567.0)  # twips -> cm
    total = sum(widths)
    flag = " <== OVER" if total > 16.05 else ""
    if flag:
        bad.append(i)
    if i <= 3 or flag:
        print("  table %-2d cols=%-2d widths=%s total=%.2fcm%s"
              % (i, len(t.columns), [round(x, 2) for x in widths], total, flag))
print("tables over 16cm:", bad if bad else "none")

# ---- 题注顺序 ----
print("\n== captions ==")
cap_re = re.compile(r"^(表|图|代码)\s*(\d+)-(\d+)")
caps = []
for p in doc.paragraphs:
    txt = p.text.strip()
    if cap_re.match(txt):
        caps.append(txt)
for c in caps:
    print("  ", c)

# ---- 标题 ----
print("\n== headings ==")
for p in doc.paragraphs:
    if p.style.name in ("Heading 1", "Heading 2", "Heading 3"):
        print("  %-9s %s" % (p.style.name[-1], p.text.strip()))

# ---- 正文导出 ----
lines = []
for p in doc.paragraphs:
    lines.append(p.text)
for i, t in enumerate(doc.tables, 1):
    lines.append("\n[TABLE %d]" % i)
    for row in t.rows:
        lines.append(" | ".join(c.text.replace("\n", " ") for c in row.cells))
with open(OUT_TEXT, "w", encoding="utf-8") as f:
    f.write("\n".join(lines))
print("\ntext dumped:", OUT_TEXT, len("\n".join(lines)), "chars")

# ---- 超长单元格 ----
print("\n== long cells (>120 chars) ==")
for i, t in enumerate(doc.tables, 1):
    for r, row in enumerate(t.rows):
        for c, cell in enumerate(row.cells):
            if len(cell.text) > 120:
                print("  table %d row %d col %d len=%d" % (i, r, c, len(cell.text)))
print("done")
