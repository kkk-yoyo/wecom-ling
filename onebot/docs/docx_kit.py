# -*- coding: utf-8 -*-
"""docx_kit —— 面向《微灵 OneBot v11 对接文档》的 python-docx 排版工具。

统一封装：中文字体、标题层级、表格（深蓝表头/斑马纹/固定列宽/表头重复）、
代码块、提示框、项目符号、页眉页脚页码、目录条目等。
"""
import datetime

from docx import Document
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_ALIGN_VERTICAL, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

CN_FONT = "微软雅黑"
MONO_FONT = "Consolas"

ACCENT = RGBColor(0x1F, 0x4E, 0x79)        # 深蓝：章节标题
ACCENT2 = RGBColor(0x2E, 0x74, 0xB5)       # 中蓝：二级标题
GRAY = RGBColor(0x59, 0x59, 0x59)          # 辅助文字
LIGHT_GRAY = RGBColor(0x80, 0x80, 0x80)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
DARK = RGBColor(0x26, 0x26, 0x26)

CONTENT_WIDTH_CM = 16.0


# ---------------------------------------------------------------- 底层 XML 工具
def _el(tag, **attrs):
    e = OxmlElement(tag)
    for k, v in attrs.items():
        e.set(qn(k), str(v))
    return e


def _set_run_font(run, name, size, bold=None, color=None, italic=None):
    run.font.name = name
    rPr = run._element.get_or_add_rPr()
    rFonts = rPr.find(qn("w:rFonts"))
    if rFonts is None:
        rFonts = _el("w:rFonts")
        rPr.insert(0, rFonts)
    rFonts.set(qn("w:ascii"), name)
    rFonts.set(qn("w:hAnsi"), name)
    rFonts.set(qn("w:eastAsia"), name)
    rFonts.set(qn("w:cs"), name)
    run.font.size = Pt(size)
    if bold is not None:
        run.bold = bold
    if italic is not None:
        run.italic = italic
    if color is not None:
        run.font.color.rgb = color


def _shade(element_pr, fill):
    shd = _el("w:shd", **{"w:val": "clear", "w:color": "auto", "w:fill": fill})
    element_pr.append(shd)


def _para_shade(p, fill):
    pPr = p._p.get_or_add_pPr()
    _shade(pPr, fill)


def _para_border(p, sides, color, sz=4, space=4):
    pPr = p._p.get_or_add_pPr()
    pBdr = pPr.find(qn("w:pBdr"))
    if pBdr is None:
        pBdr = OxmlElement("w:pBdr")
        pPr.append(pBdr)
    for side in sides:
        pBdr.append(_el(
            "w:%s" % side,
            **{"w:val": "single", "w:sz": sz, "w:space": space, "w:color": color}))


def _cell_shade(cell, fill):
    tcPr = cell._tc.get_or_add_tcPr()
    _shade(tcPr, fill)


def _cell_margins(table, top=30, bottom=30, left=80, right=80):
    tblPr = table._tbl.tblPr
    mar = _el("w:tblCellMar")
    for side, w in (("top", top), ("left", left), ("bottom", bottom), ("right", right)):
        mar.append(_el("w:%s" % side, **{"w:w": w, "w:type": "dxa"}))
    tblPr.append(mar)


def _table_borders(table, color="BFBFBF", sz=4):
    tblPr = table._tbl.tblPr
    borders = _el("w:tblBorders")
    for side in ("top", "left", "bottom", "right", "insideH", "insideV"):
        borders.append(_el(
            "w:%s" % side,
            **{"w:val": "single", "w:sz": sz, "w:space": 0, "w:color": color}))
    tblPr.append(borders)


def _repeat_header(row):
    trPr = row._tr.get_or_add_trPr()
    trPr.append(_el("w:tblHeader", **{"w:val": "true"}))


def _cant_split(row):
    trPr = row._tr.get_or_add_trPr()
    trPr.append(_el("w:cantSplit", **{"w:val": "true"}))


# ---------------------------------------------------------------- 文档初始化
def new_document():
    doc = Document()

    # 页面：A4，页边距
    sec = doc.sections[0]
    sec.page_width = Cm(21.0)
    sec.page_height = Cm(29.7)
    sec.top_margin = Cm(2.3)
    sec.bottom_margin = Cm(2.2)
    sec.left_margin = Cm(2.5)
    sec.right_margin = Cm(2.5)
    sec.header_distance = Cm(1.2)
    sec.footer_distance = Cm(1.1)

    styles = doc.styles

    # 正文
    normal = styles["Normal"]
    normal.font.name = CN_FONT
    normal.font.size = Pt(10.5)
    normal.font.color.rgb = DARK
    rPr = normal.element.get_or_add_rPr()
    rFonts = rPr.find(qn("w:rFonts"))
    if rFonts is None:
        rFonts = _el("w:rFonts")
        rPr.insert(0, rFonts)
    for a in ("w:ascii", "w:hAnsi", "w:eastAsia", "w:cs"):
        rFonts.set(qn(a), CN_FONT)
    pf = normal.paragraph_format
    pf.line_spacing_rule = WD_LINE_SPACING.MULTIPLE
    pf.line_spacing = 1.35
    pf.space_before = Pt(0)
    pf.space_after = Pt(4)

    specs = [
        ("Heading 1", 16, ACCENT, True, 18, 8),
        ("Heading 2", 13, ACCENT2, True, 12, 5),
        ("Heading 3", 11.5, RGBColor(0x33, 0x33, 0x33), True, 9, 4),
    ]
    for name, size, color, bold, before, after in specs:
        st = styles[name]
        st.font.name = CN_FONT
        st.font.size = Pt(size)
        st.font.bold = bold
        st.font.color.rgb = color
        rPr = st.element.get_or_add_rPr()
        rFonts = rPr.find(qn("w:rFonts"))
        if rFonts is None:
            rFonts = _el("w:rFonts")
            rPr.insert(0, rFonts)
        for a in ("w:ascii", "w:hAnsi", "w:eastAsia", "w:cs"):
            rFonts.set(qn(a), CN_FONT)
        st.paragraph_format.space_before = Pt(before)
        st.paragraph_format.space_after = Pt(after)
        st.paragraph_format.keep_with_next = True
        st.paragraph_format.line_spacing = 1.15
        st.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.LEFT

    # 表题样式
    cap = styles.add_style("TableCaption", 1)  # paragraph
    cap.base_style = styles["Normal"]
    cap.font.name = CN_FONT
    cap.font.size = Pt(9)
    cap.font.bold = True
    cap.font.color.rgb = GRAY
    cap.paragraph_format.space_before = Pt(8)
    cap.paragraph_format.space_after = Pt(3)
    cap.paragraph_format.keep_with_next = True
    cap.paragraph_format.line_spacing = 1.0

    # 代码样式
    code = styles.add_style("CodeBlock", 1)
    code.base_style = styles["Normal"]
    code.font.name = MONO_FONT
    code.font.size = Pt(9)
    code.font.color.rgb = RGBColor(0x1A, 0x1A, 0x1A)
    rPr = code.element.get_or_add_rPr()
    rFonts = rPr.find(qn("w:rFonts"))
    if rFonts is None:
        rFonts = _el("w:rFonts")
        rPr.insert(0, rFonts)
    for a in ("w:ascii", "w:hAnsi", "w:eastAsia", "w:cs"):
        rFonts.set(qn(a), MONO_FONT)
    code.paragraph_format.line_spacing = 1.05
    code.paragraph_format.space_before = Pt(0)
    code.paragraph_format.space_after = Pt(0)

    doc.core_properties.title = "微灵 OneBot v11 对接文档"
    doc.core_properties.subject = "微灵 1.11.0 改造版 · OneBot v11 反向 WebSocket 对接"
    doc.core_properties.author = "微灵对接文档组"
    doc.core_properties.comments = "基于 OneBotShim/WxSocketClient 实现行为编写"
    doc.core_properties.created = datetime.datetime(2026, 11, 12, 10, 0, 0)
    doc.core_properties.last_modified_by = "微灵对接文档组"
    return doc


# ---------------------------------------------------------------- 页眉页脚
def setup_header_footer(doc, header_text):
    sec = doc.sections[0]
    sec.different_first_page_header_footer = True

    header = sec.header
    hp = header.paragraphs[0]
    hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    run = hp.add_run(header_text)
    _set_run_font(run, CN_FONT, 8.5, color=LIGHT_GRAY)
    _para_border(hp, ("bottom",), "BFBFBF", sz=4, space=2)

    footer = sec.footer
    fp = footer.paragraphs[0]
    fp.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r1 = fp.add_run("— ")
    _set_run_font(r1, CN_FONT, 8.5, color=LIGHT_GRAY)
    run = fp.add_run()
    _set_run_font(run, CN_FONT, 8.5, color=LIGHT_GRAY)
    fld1 = _el("w:fldChar", **{"w:fldCharType": "begin"})
    instr = OxmlElement("w:instrText")
    instr.set(qn("xml:space"), "preserve")
    instr.text = " PAGE "
    fld2 = _el("w:fldChar", **{"w:fldCharType": "separate"})
    t = OxmlElement("w:t")
    t.text = "1"
    fld3 = _el("w:fldChar", **{"w:fldCharType": "end"})
    for node in (fld1, instr, fld2, t, fld3):
        run._r.append(node)
    r2 = fp.add_run(" —")
    _set_run_font(r2, CN_FONT, 8.5, color=LIGHT_GRAY)


# ---------------------------------------------------------------- 富文本
def add_rich(paragraph, text, size=10.5, color=None, base_bold=False, mono_size=None):
    """解析 **加粗** 与 `等宽` 标记的简易富文本。"""
    if mono_size is None:
        mono_size = max(size - 1.0, 7.5)
    i = 0
    buf = ""
    mode = "normal"  # normal / bold / mono
    while i < len(text):
        if text.startswith("**", i):
            if buf:
                r = paragraph.add_run(buf)
                _set_run_font(r, CN_FONT, size, bold=(base_bold or mode == "bold"), color=color)
                buf = ""
            mode = "normal" if mode == "bold" else "bold"
            i += 2
            continue
        if text[i] == "`" and mode != "mono":
            if buf:
                r = paragraph.add_run(buf)
                _set_run_font(r, CN_FONT, size, bold=(base_bold or mode == "bold"), color=color)
                buf = ""
            mode = "mono"
            i += 1
            continue
        if text[i] == "`" and mode == "mono":
            if buf:
                r = paragraph.add_run(buf)
                _set_run_font(r, MONO_FONT, mono_size, bold=False, color=color)
                buf = ""
            mode = "normal"
            i += 1
            continue
        buf += text[i]
        i += 1
    if buf:
        if mode == "mono":
            r = paragraph.add_run(buf)
            _set_run_font(r, MONO_FONT, mono_size, bold=False, color=color)
        else:
            r = paragraph.add_run(buf)
            _set_run_font(r, CN_FONT, size, bold=(base_bold or mode == "bold"), color=color)


# ---------------------------------------------------------------- 标题 / 正文
def add_h1(doc, number, text, page_break=False):
    if page_break:
        doc.add_page_break()
    p = doc.add_paragraph(style="Heading 1")
    r = p.add_run("%s　%s" % (number, text) if number else text)
    _set_run_font(r, CN_FONT, 16, bold=True, color=ACCENT)
    return p


def add_h2(doc, number, text):
    p = doc.add_paragraph(style="Heading 2")
    r = p.add_run("%s　%s" % (number, text) if number else text)
    _set_run_font(r, CN_FONT, 13, bold=True, color=ACCENT2)
    return p


def add_h3(doc, number, text):
    p = doc.add_paragraph(style="Heading 3")
    r = p.add_run("%s　%s" % (number, text) if number else text)
    _set_run_font(r, CN_FONT, 11.5, bold=True, color=RGBColor(0x33, 0x33, 0x33))
    return p


def add_para(doc, text, indent=True, align="justify", size=10.5, space_after=4, color=None):
    p = doc.add_paragraph()
    pf = p.paragraph_format
    if indent:
        pf.first_line_indent = Cm(0.74)
    pf.space_after = Pt(space_after)
    pf.alignment = (WD_ALIGN_PARAGRAPH.JUSTIFY if align == "justify"
                    else WD_ALIGN_PARAGRAPH.LEFT if align == "left"
                    else WD_ALIGN_PARAGRAPH.CENTER)
    add_rich(p, text, size=size, color=color)
    return p


def add_note_para(doc, text, size=9.5):
    p = doc.add_paragraph()
    p.paragraph_format.left_indent = Cm(0.4)
    p.paragraph_format.space_after = Pt(3)
    add_rich(p, text, size=size, color=GRAY)
    return p


def add_bullet(doc, text, level=0, size=10.5):
    p = doc.add_paragraph()
    pf = p.paragraph_format
    pf.left_indent = Cm(0.65 + 0.55 * level)
    pf.first_line_indent = Cm(-0.35)
    pf.space_after = Pt(2)
    pf.line_spacing = 1.3
    mark = "•" if level == 0 else "–"
    r = p.add_run(mark + "  ")
    _set_run_font(r, CN_FONT, size, color=ACCENT if level == 0 else GRAY)
    add_rich(p, text, size=size)
    return p


def add_step(doc, number, text, size=10.5):
    p = doc.add_paragraph()
    pf = p.paragraph_format
    pf.left_indent = Cm(0.9)
    pf.first_line_indent = Cm(-0.9)
    pf.space_after = Pt(3)
    pf.line_spacing = 1.3
    r = p.add_run("步骤 %s　" % number)
    _set_run_font(r, CN_FONT, size, bold=True, color=ACCENT)
    add_rich(p, text, size=size)
    return p


# ---------------------------------------------------------------- 代码块 / 提示框
def add_code(doc, lines, caption=None):
    if caption:
        p = doc.add_paragraph(style="TableCaption")
        add_rich(p, caption, size=9, color=GRAY)
    p = doc.add_paragraph(style="CodeBlock")
    pf = p.paragraph_format
    pf.left_indent = Cm(0.2)
    pf.right_indent = Cm(0.2)
    pf.space_before = Pt(3)
    pf.space_after = Pt(7)
    pf.keep_together = True
    _para_shade(p, "F5F6F8")
    _para_border(p, ("box",), "D9D9D9", sz=4, space=4)
    if isinstance(lines, str):
        lines = lines.split("\n")
    for idx, line in enumerate(lines):
        if idx > 0:
            p.add_run().add_break()
        if line is None:
            continue
        r = p.add_run(line.rstrip("\n"))
        _set_run_font(r, MONO_FONT, 9, color=RGBColor(0x1A, 0x1A, 0x1A))
    return p


CALLOUT_STYLE = {
    "info": ("EAF1FA", "2E74B5", "提示"),
    "warn": ("FFF6E5", "ED7D31", "注意"),
    "limit": ("FDECEA", "C00000", "限制"),
    "example": ("F2F7EE", "548235", "示例"),
}


def add_callout(doc, kind, text, title=None):
    fill, border, default_title = CALLOUT_STYLE[kind]
    p = doc.add_paragraph()
    pf = p.paragraph_format
    pf.left_indent = Cm(0.2)
    pf.right_indent = Cm(0.2)
    pf.space_before = Pt(5)
    pf.space_after = Pt(7)
    pf.line_spacing = 1.3
    _para_shade(p, fill)
    _para_border(p, ("left",), border, sz=18, space=6)
    label = title if title else default_title
    r = p.add_run("%s　" % label)
    _set_run_font(r, CN_FONT, 10, bold=True, color=RGBColor.from_string(border))
    add_rich(p, text, size=9.5, color=DARK)
    return p


# ---------------------------------------------------------------- 表格
def _set_table_width(table, total_cm):
    tblPr = table._tbl.tblPr
    tblW = tblPr.find(qn("w:tblW"))
    if tblW is None:
        tblW = _el("w:tblW")
        tblPr.append(tblW)
    tblW.set(qn("w:w"), str(int(total_cm * 567)))
    tblW.set(qn("w:type"), "dxa")
    layout = _el("w:tblLayout", **{"w:type": "fixed"})
    tblPr.append(layout)


def add_table(doc, headers, rows, widths, caption=None, size=9, header_fill="1F4E79",
              zebra="F2F6FB", first_col_center=False, center_cols=None,
              mono_cols=None, header_size=9):
    """创建专业表格：深蓝表头、斑马纹、固定列宽、表头跨页重复、行不跨页。"""
    if caption:
        p = doc.add_paragraph(style="TableCaption")
        add_rich(p, caption, size=9, color=GRAY)
    center_cols = set(center_cols or [])
    mono_cols = set(mono_cols or [])
    if first_col_center:
        center_cols.add(0)

    table = doc.add_table(rows=1 + len(rows), cols=len(headers))
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    _set_table_width(table, sum(widths))
    _table_borders(table)
    _cell_margins(table)

    # 列宽
    grid = table._tbl.tblGrid
    grid_cols = grid.findall(qn("w:gridCol"))
    for i, w in enumerate(widths):
        if i < len(grid_cols):
            grid_cols[i].set(qn("w:w"), str(int(w * 567)))
        table.columns[i].width = Cm(w)

    def fill_row(row, values, header=False, row_idx=0):
        for ci, val in enumerate(values):
            cell = row.cells[ci]
            cell.width = Cm(widths[ci])
            cell.vertical_alignment = WD_ALIGN_VERTICAL.CENTER
            if header:
                _cell_shade(cell, header_fill)
            elif zebra and row_idx % 2 == 1:
                _cell_shade(cell, zebra)
            p = cell.paragraphs[0]
            p.paragraph_format.space_before = Pt(1)
            p.paragraph_format.space_after = Pt(1)
            p.paragraph_format.line_spacing = 1.15
            if ci in center_cols and not header:
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            if header:
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            text = "" if val is None else str(val)
            parts = text.split("\n")
            for pi, part in enumerate(parts):
                if pi > 0:
                    p = cell.add_paragraph()
                    p.paragraph_format.space_before = Pt(0)
                    p.paragraph_format.space_after = Pt(0)
                    p.paragraph_format.line_spacing = 1.15
                    if ci in center_cols and not header:
                        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
                if header:
                    r = p.add_run(part)
                    _set_run_font(r, CN_FONT, header_size, bold=True, color=WHITE)
                elif ci in mono_cols:
                    add_rich(p, part, size=size - 0.5, mono_size=size - 0.5)
                else:
                    add_rich(p, part, size=size)

    # 表头
    hdr_row = table.rows[0]
    _cant_split(hdr_row)
    _repeat_header(hdr_row)
    fill_row(hdr_row, headers, header=True)

    for ri, values in enumerate(rows):
        row = table.rows[ri + 1]
        _cant_split(row)
        fill_row(row, values, header=False, row_idx=ri)

    # 表后留白
    tail = doc.add_paragraph()
    tail.paragraph_format.space_before = Pt(0)
    tail.paragraph_format.space_after = Pt(0)
    tail.paragraph_format.line_spacing = 0.6
    return table


# ---------------------------------------------------------------- 封面 / 目录
def add_spacer(doc, count=1, size=10.5):
    for _ in range(count):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.line_spacing = 1.0
        r = p.add_run("")
        _set_run_font(r, CN_FONT, size)


def add_cover(doc):
    add_spacer(doc, 4)
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("W E C O M   L I N G   ·   O N E B O T   V 1 1")
    _set_run_font(r, CN_FONT, 9, bold=True, color=LIGHT_GRAY)
    p.paragraph_format.space_after = Pt(26)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("微灵 OneBot v11")
    _set_run_font(r, CN_FONT, 34, bold=True, color=ACCENT)
    p.paragraph_format.space_after = Pt(2)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("对接文档")
    _set_run_font(r, CN_FONT, 34, bold=True, color=ACCENT)
    p.paragraph_format.space_after = Pt(18)

    # 装饰线
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    _para_border(p, ("bottom",), "2E74B5", sz=12, space=1)
    p.paragraph_format.left_indent = Cm(4.5)
    p.paragraph_format.right_indent = Cm(4.5)
    p.paragraph_format.space_after = Pt(16)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("微灵 1.11.0 改造版　·　标准 OneBot v11 反向 WebSocket 协议")
    _set_run_font(r, CN_FONT, 12.5, color=GRAY)
    p.paragraph_format.space_after = Pt(4)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("从微灵自研 WS 协议迁移到 OneBot v11 的完整对接指南")
    _set_run_font(r, CN_FONT, 10.5, color=LIGHT_GRAY)
    p.paragraph_format.space_after = Pt(48)

    add_spacer(doc, 2)

    # 封面信息表
    info = [
        ("文档名称", "微灵 OneBot v11 对接文档"),
        ("适用软件", "微灵（WeComSever）1.11.0 改造版"),
        ("通信协议", "OneBot v11 · 反向 WebSocket（Reverse WS）"),
        ("文档版本", "v1.6"),
        ("生成日期", "2026-11-13"),
        ("适用读者", "机器人开发者、运维部署人员、二次开发对接方"),
    ]
    table = doc.add_table(rows=len(info), cols=2)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    _set_table_width(table, 12.6)
    _table_borders(table, color="D9D9D9")
    _cell_margins(table, top=70, bottom=70, left=140, right=140)
    grid = table._tbl.tblGrid
    for i, w in enumerate((3.4, 9.2)):
        grid.findall(qn("w:gridCol"))[i].set(qn("w:w"), str(int(w * 567)))
    for ri, (k, v) in enumerate(info):
        row = table.rows[ri]
        _cant_split(row)
        c0, c1 = row.cells
        c0.width = Cm(3.4)
        c1.width = Cm(9.2)
        _cell_shade(c0, "EAF1FA")
        p0 = c0.paragraphs[0]
        r = p0.add_run(k)
        _set_run_font(r, CN_FONT, 9.5, bold=True, color=ACCENT)
        p1 = c1.paragraphs[0]
        add_rich(p1, v, size=9.5, color=DARK)
    doc.add_page_break()


def add_toc(doc, entries):
    """entries: [(level, number, title)]，手工目录（无自动域）。"""
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    r = p.add_run("目录")
    _set_run_font(r, CN_FONT, 20, bold=True, color=ACCENT)
    p.paragraph_format.space_after = Pt(2)
    p.paragraph_format.space_before = Pt(6)

    p = doc.add_paragraph()
    r = p.add_run("CONTENTS")
    _set_run_font(r, CN_FONT, 8.5, bold=True, color=LIGHT_GRAY)
    p.paragraph_format.space_after = Pt(10)
    _para_border(p, ("bottom",), "BFBFBF", sz=4, space=4)

    for level, number, title in entries:
        p = doc.add_paragraph()
        pf = p.paragraph_format
        pf.space_before = Pt(0)
        pf.space_after = Pt(2)
        pf.line_spacing = 1.25
        if level == 1:
            pf.space_before = Pt(7)
            pf.left_indent = Cm(0)
            if number:
                r = p.add_run("%s" % number)
                _set_run_font(r, CN_FONT, 11, bold=True, color=ACCENT)
                r = p.add_run("　%s" % title)
            else:
                r = p.add_run("%s" % title)
            _set_run_font(r, CN_FONT, 11, bold=True, color=DARK)
        else:
            pf.left_indent = Cm(0.9)
            if number:
                r = p.add_run("%s" % number)
                _set_run_font(r, MONO_FONT, 9.5, color=GRAY)
                r = p.add_run("　%s" % title)
            else:
                r = p.add_run("%s" % title)
            _set_run_font(r, CN_FONT, 9.5, color=GRAY)
    doc.add_page_break()


def add_caption_table_note(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(0)
    p.paragraph_format.space_after = Pt(8)
    add_rich(p, text, size=8.5, color=LIGHT_GRAY)
