import ctypes, ctypes.wintypes as wt
k=ctypes.WinDLL('kernel32', use_last_error=True)
PROCESS_ALL=0x1F0FFF
pid=10848
h=k.OpenProcess(PROCESS_ALL, False, pid)
print('OpenProcess handle=', h, 'err=', ctypes.get_last_error())
if h:
    addr=k.VirtualAllocEx(h, None, 0x1000, 0x3000, 0x40)
    print('VirtualAllocEx addr=', addr, 'err=', ctypes.get_last_error())
    buf=(ctypes.c_char*4)(b'ABCD')
    written=ctypes.c_size_t(0)
    ok=k.WriteProcessMemory(h, addr, buf, 4, ctypes.byref(written))
    print('WriteProcessMemory ok=', ok, 'written=', written.value, 'err=', ctypes.get_last_error())
    k.CloseHandle(h)
