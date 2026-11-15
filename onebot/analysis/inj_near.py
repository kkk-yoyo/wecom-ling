import re
p=r'D:\wecom-ling\dll\951a724492438030be27ba5f6461b352\wecom_inject.exe'
data=open(p,'rb').read()
strs=re.findall(rb'[\x20-\x7e]{3,}', data)
for b in strs:
    off=data.find(b)
    if 1678000 <= off <= 1682000:
        print(hex(off), b.decode('latin1'))
