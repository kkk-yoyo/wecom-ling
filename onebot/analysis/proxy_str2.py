import re
p=r'D:\wecom-ling\dll\951a724492438030be27ba5f6461b352\msimg32.dll'
data=open(p,'rb').read()
out=[]
i=0
while i<len(data)-4:
    if data[i]!=0 and data[i+1]==0:
        j=i; chars=[]
        while j<len(data)-1 and data[j]!=0 and data[j+1]==0:
            chars.append(chr(data[j])); j+=2
        s=''.join(chars)
        if len(s)>=3: out.append((i,s))
        i=j
    else: i+=1
for off,s in out:
    if 0xFF000 <= off <= 0x112000:
        print(hex(off), repr(s))
