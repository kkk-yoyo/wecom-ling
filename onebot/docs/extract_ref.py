# -*- coding: utf-8 -*-
"""提取参考文档《微灵对接文档.docx》的结构与样式，供新文档排版参考。"""
import sys
from docx import Document
from docx.shared import RGBColor
from docx.oxml.ns import qn

PATH = r"D:\wecom-ling\微灵对接文档.docx"
OUT = r"D:\wecom-ling\onebot\docs\ref_outline.txt"

doc = Document(PATH)

lines = []
lines.append("=== SECTIONS ===")
for sec in doc.sections:
    lines.append(
        "page %s x %s; margins L%s R%s T%s B%s" % (
            sec.page_width, sec.page_height, sec.left_margin, sec.right_margin,
            sec.top_margin, sec.bottom_margin))

lines.append("\n=== STYLES USED ===")
used = {}
for p in doc.paragraphs:
    used[p.style.name] = used.get(p.style.name, 0) + 1
for k, v in sorted(used.items(), key=lambda x: -x[1]):
    lines.append("%s : %d" % (k, v))

lines.append("\n=== BODY (paragraph runs detail) ===")
for i, p in enumerate(doc.paragraphs):
    text = p.text.strip()
    if not text:
        continue
    runs = []
    for r in p.runs:
        f = r.font
        ea = None
        rPr = r._element.rPr
        if rPr is not None and rPr.rFonts is not None:
            ea = rPr.rFonts.get(qn('w:eastAsia'))
        runs.append({
            "t": r.text[:30],
            "font": f.name,
            "ea": ea,
            "sz": f.size.pt if f.size else None,
            "b": f.bold,
            "color": str(f.color.rgb) if f.color and f.color.type is not None else None,
        })
    lines.append("[%03d][%s] %s" % (i, p.style.name, text))
    lines.append("      runs=%s" % (runs[:6],))

lines.append("\n=== TABLES (%d) ===" % len(doc.tables))
for ti, table in enumerate(doc.tables):
    lines.append("--- table %d: rows=%d cols=%d style=%s" % (
        ti, len(table.rows), len(table.columns), table.style.name if table.style else None))
    for ri, row in enumerate(table.rows[:8]):
        cells = [c.text.replace("\n", " / ")[:80] for c in row.cells]
        lines.append("  r%d: %s" % (ri, " | ".join(cells)))

with open(OUT, "w", encoding="utf-8") as f:
    f.write("\n".join(lines))

# 统计内联图片
import zipfile
with zipfile.ZipFile(PATH) as z:
    media = [n for n in z.namelist() if n.startswith("word/media/")]
print("saved", OUT)
print("paragraphs", len(doc.paragraphs), "tables", len(doc.tables), "media", len(media))
print("headings:")
for p in doc.paragraphs:
    if p.style.name.startswith("Heading") or p.style.name in ("Title",):
        print(" ", p.style.name, "|", p.text[:80])
