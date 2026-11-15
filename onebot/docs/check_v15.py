# -*- coding: utf-8 -*-
import docx

p = r'D:\wecom-ling\微灵 OneBot v11 对接文档.docx'
d = docx.Document(p)
print('file', p)
print('paras', len(d.paragraphs), 'tables', len(d.tables))
hs = [(x.style.name, x.text.strip()) for x in d.paragraphs if x.style.name.startswith('Heading') and x.text.strip()]
print('headings', len(hs))
for style, txt in hs:
    print(style, '|', txt)
# check version record line
for para in d.paragraphs:
    t = para.text.strip()
    if t.startswith('v1.') or '版本记录' in t:
        print('VER:', t[:200])
