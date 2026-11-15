p=r'D:\wecom-ling\onebot\analysis\WeComBridgeRuntime.cs'
raw=open(p,'rb').read()
txt=raw.decode('utf-16') if raw[:2]==b'\xff\xfe' else raw.decode('utf-8',errors='replace')
lines=txt.splitlines()
for i in range(1560,1780):
    print(i+1, lines[i])
