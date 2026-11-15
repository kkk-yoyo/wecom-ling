p=r'D:\wecom-ling\onebot\analysis\WeComBridgeRuntime.cs'
raw=open(p,'rb').read()
txt=raw.decode('utf-16') if raw[:2]==b'\xff\xfe' else raw.decode('utf-8',errors='replace')
lines=txt.splitlines()
def dump(a,b):
    for i in range(a-1,b):
        print(i+1, lines[i])
print('===== EnsureReady 1110-1210 =====')
dump(1110,1210)
print('===== WaitStartedPid 2913-3040 =====')
dump(2913,3040)
