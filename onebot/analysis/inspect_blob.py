# -*- coding: utf-8 -*-
import binascii

path = r"D:\wecom-ling\onebot\analysis\res\Vddcg0PBc9s1Y40Ht5.F0ANIOEy9fAwW7eIKk.bin"
blob = open(path, "rb").read()
print("size", len(blob))
print("head hex", binascii.hexlify(blob[:64]))
for probe in [b"WxWsUrl", b"WxWsToken", b"ClientName", b"AutoConnect", b"config.ini", b"WeComUseWebSocket"]:
    print(probe, "ascii@", blob.find(probe), "utf16@", blob.find(probe.decode().encode("utf-16-le")))
# count plausible UTF-16LE CJK/ASCII runs
import struct
runs = 0
i = 0
samples = []
while i + 1 < len(blob):
    j = i
    ch = []
    while j + 1 < len(blob):
        c = struct.unpack_from("<H", blob, j)[0]
        if 0x20 <= c <= 0x7E or 0x4E00 <= c <= 0x9FFF or c in (0x0A, 0x0D):
            j += 2
        else:
            break
    if j > i and len(blob) > 0 and j - i >= 8:
        runs += 1
        if len(samples) < 8:
            samples.append(blob[i:j].decode("utf-16-le", "replace"))
    i = j + 2 if j > i else i + 2
print("utf16 runs >=4 chars:", runs)
print("samples:", samples)

# dump first 40 dwords as offsets/lengths
print("first 40 dwords:")
for k in range(0, 160, 4):
    v = struct.unpack_from("<i", blob, k)[0]
    print(k, v)
