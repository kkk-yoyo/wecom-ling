# -*- coding: utf-8 -*-
import json
import os
import time

from docx import Document

p = r"D:\wecom-ling\微灵 OneBot v11 对接文档.docx"
d = Document(p)
h1 = [x.text for x in d.paragraphs if x.style.name == "Heading 1"]
st = os.stat(p)
cover = {}
for t in d.tables[:1]:
    for row in t.rows:
        cells = [c.text.strip() for c in row.cells]
        if len(cells) >= 2:
            cover[cells[0]] = cells[1]
print(json.dumps({
    "path": p,
    "exists": os.path.exists(p),
    "size": st.st_size,
    "mtime": time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(st.st_mtime)),
    "paragraphs": len(d.paragraphs),
    "tables": len(d.tables),
    "h1": len(h1),
    "h1_list": h1,
    "cover": cover,
    "bad_chars": sum(1 for para in d.paragraphs if "\ufffd" in para.text),
}, ensure_ascii=True, indent=2))
