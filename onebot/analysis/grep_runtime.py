import io,re,sys
p=r'D:\wecom-ling\onebot\analysis\WeComBridgeRuntime.cs'
raw=open(p,'rb').read()
txt=raw.decode('utf-16') if raw[:2]==b'\xff\xfe' else raw.decode('utf-8',errors='replace')
lines=txt.splitlines()
print('lines',len(lines))
pats=['OpenWeCom','msimg32','Copy','WXWork','LaunchAndInject','wecom_inject','config.json','status.json','session.json','DLL already','AppDomain']
for i,l in enumerate(lines):
    if any(pt in l for pt in pats):
        print(i+1, l.strip()[:220])
