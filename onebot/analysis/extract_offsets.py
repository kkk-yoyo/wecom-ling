import re, io, json
p = r"D:\wecom-ling\onebot\analysis\MainWindow.cs"
s = io.open(p, encoding="utf-16").read()
pat = re.compile(r"(?:kYPMBqDNYaqYda5Nkam|tE1kD1vbB)\s*\(([^()]*)\)")
def eval_int(expr):
    e = expr.strip()
    try:
        v = eval(e, {"__builtins__": {}}, {})
    except Exception:
        return None
    v &= 0xFFFFFFFF
    return v - 0x100000000 if v >= 0x80000000 else v
res = {}
for m in pat.finditer(s):
    expr = m.group(1).strip()
    if "^" not in expr and "<<" not in expr:
        continue
    v = eval_int(expr)
    if v is None:
        continue
    line = s.count("\n", 0, m.start()) + 1
    res.setdefault(v, []).append(line)
offs = sorted(res)
print("unique offsets:", len(offs))
print("sample:", offs[:10])
json.dump({"offsets": offs, "lines": {str(k): v[:5] for k, v in res.items()}}, open(r"D:\wecom-ling\onebot\analysis\offsets.json", "w"), ensure_ascii=False)
