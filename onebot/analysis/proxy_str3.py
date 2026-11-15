import re
p=r'D:\wecom-ling\dll\951a724492438030be27ba5f6461b352\msimg32.dll'
data=open(p,'rb').read()
strs=re.findall(rb'[\x20-\x7e]{4,}', data)
keys=['config','session','proxy','entry','dll','tcp','port','token','pid','creation','identity','json','load_only','observation','heartbeat','format','version','client','path','sha','status','hook','runtime','localappdata']
seen=set()
for b in strs:
    s=b.decode('latin1')
    low=s.lower()
    if any(k in low for k in keys) and not s.startswith('api-') and len(s)<160:
        if s in seen: continue
        seen.add(s)
        off=data.find(b)
        print(hex(off), repr(s))
