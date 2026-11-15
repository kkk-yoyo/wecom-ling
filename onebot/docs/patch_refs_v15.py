# -*- coding: utf-8 -*-
"""v1.5：批量更新既有 ch_*.py 的章节编号与交叉引用。

映射：原 6→7（动作）、7→8（媒体）、8→9（个微）、9→10（兼容）、10→12（限制）、11→13（附录）；
新增第 6 章（消息能力矩阵）与第 11 章（AI 审批插件）。
本脚本只处理 ch_intro / ch_quick / ch_deploy / ch_protocol / ch_events /
ch_personal / ch_compat / ch_limits / ch_appendix；ch_actions.py 与 ch_media.py 重写。
"""
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))

FILES = [
    "ch_intro.py", "ch_quick.py", "ch_deploy.py", "ch_protocol.py",
    "ch_events.py", "ch_personal.py", "ch_compat.py", "ch_limits.py",
    "ch_appendix.py",
]

# 引用替换按章号降序，避免链式误替换
PAIRS = [
    ("第 11 章", "第 13 章"),
    ("第 10 章", "第 12 章"),
    ("第 9 章", "第 10 章"),
    ("第 8 章", "第 9 章"),
    ("第 7 章", "第 8 章"),
    ("第 6 章", "第 7 章"),
    ("第 4～6 章", "第 4～7 章"),
    ("第 5～6 章", "第 5～7 章"),
]
TABLE_PAIRS = [
    ("表 11-", "表 13-"),
    ("表 10-", "表 12-"),
    ("表 9-", "表 10-"),
    ("表 8-", "表 9-"),
    ("表 7-", "表 8-"),
    ("表 6-", "表 7-"),
]
CODE_PAIRS = [
    ("代码 11-", "代码 13-"),
    ("代码 10-", "代码 12-"),
    ("代码 9-", "代码 10-"),
    ("代码 8-", "代码 9-"),
    ("代码 7-", "代码 8-"),
    ("代码 6-", "代码 7-"),
]

SEC_RE = re.compile(r"(?<![\d.])(\d+)\.(\d+)(?= 节)")
SEC_MAP = {6: 7, 7: 8, 8: 9, 9: 10, 10: 12, 11: 13}


def map_sec(m):
    ch = int(m.group(1))
    return "%d.%s" % (SEC_MAP.get(ch, ch), m.group(2))


# 各文件自身的标题/表号重编号
OWN = {
    "ch_personal.py": [("K.add_h1(doc, \"8\", \"个人微信通道（实验性）\"", "K.add_h1(doc, \"9\", \"个人微信通道（实验性）\"")] +
                       [("K.add_h2(doc, \"8.%d\"" % i, "K.add_h2(doc, \"9.%d\"" % i) for i in range(1, 5)],
    "ch_compat.py": [("K.add_h1(doc, \"9\", \"保留能力与兼容性\"", "K.add_h1(doc, \"10\", \"保留能力与兼容性\"")] +
                    [("K.add_h2(doc, \"9.%d\"" % i, "K.add_h2(doc, \"10.%d\"" % i) for i in range(1, 7)],
    "ch_limits.py": [("K.add_h1(doc, \"10\", \"已知限制与排障\"", "K.add_h1(doc, \"12\", \"已知限制与排障\"")] +
                    [("K.add_h2(doc, \"10.%d\"" % i, "K.add_h2(doc, \"12.%d\"" % i) for i in range(1, 7)],
    "ch_appendix.py": [("K.add_h1(doc, \"11\", \"附录\"", "K.add_h1(doc, \"13\", \"附录\"")] +
                      [("K.add_h2(doc, \"11.%d\"" % i, "K.add_h2(doc, \"13.%d\"" % i) for i in range(1, 6)],
}


def main():
    for name in FILES:
        path = os.path.join(HERE, name)
        with io.open(path, "r", encoding="utf-8") as f:
            text = f.read()
        orig = text
        for old, new in PAIRS:
            text = text.replace(old, new)
        for old, new in TABLE_PAIRS:
            text = text.replace(old, new)
        for old, new in CODE_PAIRS:
            text = text.replace(old, new)
        text = SEC_RE.sub(map_sec, text)
        for old, new in OWN.get(name, []):
            if old not in text:
                print("WARN %s: not found: %s" % (name, old))
            text = text.replace(old, new)
        if text == orig:
            print("skip", name)
            continue
        with io.open(path, "w", encoding="utf-8", newline="\n") as f:
            f.write(text)
        print("patched", name)
    print("done")


if __name__ == "__main__":
    main()
