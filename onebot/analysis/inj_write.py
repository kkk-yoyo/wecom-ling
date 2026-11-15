p=r'D:\wecom-ling\dll\951a724492438030be27ba5f6461b352\wecom_inject.exe'
data=open(p,'rb').read()
i=data.find(b'write failed')
while i!=-1:
    print('offset',hex(i), 'ctx', data[max(0,i-40):i+60])
    i=data.find(b'write failed', i+1)
print('--- raw 0x19a2a0-0x19a2f0 ---')
print(data[0x19a2a0:0x19a2f4])
