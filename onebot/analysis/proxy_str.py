import re,sys
p=r'D:\wecom-ling\dll\951a724492438030be27ba5f6461b352\msimg32.dll'
data=open(p,'rb').read()
# UTF-16LE strings
out=[]
i=0
while i<len(data)-4:
    if data[i]!=0 and data[i+1]==0 and data[i+2]!=0 and data[i+3]==0:
        j=i
        chars=[]
        while j<len(data)-1 and data[j]!=0 and data[j+1]==0:
            chars.append(chr(data[j])); j+=2
        s=''.join(chars)
        if len(s)>=4: out.append((i,s))
        i=j
    else: i+=1
keys=['config','runtime','LOCALAPPDATA','WeComHook','.json','session','handoff','%s','%lu','%d','%u','status','proxy','Attach','ready','tcp','port','error']
for off,s in out:
    low=s.lower()
    if any(k.lower() in low for k in keys):
        print(hex(off), repr(s))
