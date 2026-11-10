# dist — 发布物说明

本目录**不收录**原始 `微灵.exe`（商业闭源软件，版权归原作者；本仓库只发布我们自己的
反编译源码、文档、桥接层与原生 DLL 的引用信息）。运行时请从原作者提供的原始安装包获取，
并自行核对下方 SHA256。

## 文件清单与哈希

| 文件 | 大小 | SHA256 |
| --- | --- | --- |
| `微灵.exe` | 15,920,128 B | `92566528faae1e9d8920ddbde42dec907de20b51309bb4bc8d66bb99d225d1ea` |
| `Loader.dll` | 5,771,776 B | `ce4557bf449fd53078f2eaa9becbf43b6f2bf40f16199e1a4bd6088ab233a65a` |
| `Helper_5.0.0.6008.dll` | 10,254,848 B | `038e16a76f6f29e4bb41a811585f027d5506aa1a50e3cd67381a38dcfdb58438` |
| `API.docx`（原始接口文档） | 52,233 B | `47b60b553741d6aaf33eeb5ad138ddd9b5bcff970de788091a6d2c237a13eb08` |

原始安装包（用户提供，未随仓库分发）内文件结构：

```
微灵.exe
Loader.dll
Helper_5.0.0.6008.dll
API.docx
config.ini
说明文档.txt
logs/            （原作者机器上的历史日志）
```

## 本地如何就位

把 `Loader.dll`、`Helper_5.0.0.6008.dll` 与 `微灵.exe` 放在**同一目录**（微灵要求 DLL 与
exe 同目录，x86、管理员权限运行），`config.ini` 参考 `examples/config.ini`。

原生 DLL 的偏移表、调用约定见 `docs/NATIVE_DLLS.md`；仓库内 `native/bin/` 已包含两个 DLL
副本（仅供研究与复现，版权归原作者）。
