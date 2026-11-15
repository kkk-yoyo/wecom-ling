import re
p=r'D:\wecom-ling\onebot\analysis\WeComBridgeRuntime.cs'
raw=open(p,'rb').read()
txt=raw.decode('utf-16') if raw[:2]==b'\xff\xfe' else raw.decode('utf-8',errors='replace')
lines=txt.splitlines()
names=['EnsureReady','StartListener','WaitStartedPid','WaitNewWxWorkPid','cOLLLAcidlrWei9XWsK','GetBridgeDirectory','TryResolveLaunchExe','MatchesExe','CompleteAttach']
for i,l in enumerate(lines):
    for n in names:
        if n in l and ('private' in l or 'public' in l or 'internal' in l or 'static' in l) and '(' in l and ';' not in l:
            print(i+1, l.strip()[:200])
