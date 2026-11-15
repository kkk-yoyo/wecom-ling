# -*- coding: utf-8 -*-
import struct

BLOB = r"D:\wecom-ling\onebot\analysis\res\Vddcg0PBc9s1Y40Ht5.F0ANIOEy9fAwW7eIKk.bin"
OUT = r"D:\wecom-ling\onebot\analysis\decrypted_keys.txt"

blob = open(BLOB, "rb").read()


def dec(off):
    n = struct.unpack_from("<i", blob, off)[0]
    raw = blob[off + 4: off + 4 + n]
    return raw.decode("utf-16-le")


def both(x, y):
    return (x & 0xFFFFFFFF) ^ (y & 0xFFFFFFFF)


items = [
    ("GetWxWsUrl", both(-1947700697, -1947675535)),
    ("GetWxWsToken", both(0xAC5AB62, 0xAC50D0A)),
    ("AutoConnect.key", both(-569117117, -569159619)),
    ("AutoConnect.default", both(-183227658, -183202730)),
    ("AutoConnect.true", both(-409114785, -409091599)),
    ("GetClientName", both(-1796327704, -1796325638)),
    ("hint.2066", 2066),
    ("chWeCom", both(-608990028, -608986682)),
    ("pathWeCom", both(0x1A739410, 0x1A738190)),
    ("chPers", both(0x234FD180, 0x234FC410)),
    ("pathPers", both(-30119401, -30113869)),
]

lines = []
for name, off in items:
    try:
        value = dec(off)
    except Exception as exc:  # noqa: BLE001
        value = "<error %r>" % (exc,)
    lines.append("%s off=%d => %r" % (name, off, value))

open(OUT, "w", encoding="utf-8").write("\n".join(lines))
print("done ->", OUT)
