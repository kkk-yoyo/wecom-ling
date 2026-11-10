# NOTICE / 来源与授权说明

## 来源

- `dist/微灵.exe`：原作者发布的原始二进制（FileVersion 1.0.1.0，界面显示 v1.0.3），未作任何修改。
  - SHA256：`92566528FAAE1E9D8920DDBDE42DEC907DE20B51309BB4BC8D66BB99D225D1EA`
- `native/bin/Loader.dll`：注入器，原包自带。
  - SHA256：`CE4557BF449FD53078F2EAA9BECBF43B6F2BF40F16199E1A4BD6088AB233A65A`
- `native/bin/Helper_5.0.0.6008.dll`：hook 核心，原包自带，只适配企业微信 5.0.0.6008。
  - SHA256：`038E16A76F6F29E4BB41A811585F027D5506AA1A50E3CD67381A38DCFDB58438`
- `src/WeComSever/`：用 `ilspycmd 9.1.0.7988` 对 `dist/微灵.exe` 反编译得到（命令见 `tools/decompile.ps1`）。
- `docs/original/`：原作者随包提供的 API 文档与说明。

## 授权状态

原作者已停止维护，并将二进制与文档打包交给本项目发起者。公开仓库前建议向原作者取得书面/聊天授权，
内容可参考：

> 微灵我不做了，资料随便处理：可以逆向、修改、以 MIT 协议开源发布，源码和二进制都行。

在取得明确授权前：

- 反编译源码与原生二进制**仅供技术研究**，不视为已获再分发授权；
- 若原作者提出异议，维护者将立即移除 `src/WeComSever/`、`native/bin/`、`dist/` 中的对应文件；
- 本仓库新增内容（docs、bridge、tools）按 MIT 协议发布（见 LICENSE）。

## 免责声明

本项目与腾讯公司无任何关联。企业微信 PC hook 属于非官方客户端行为，可能触发腾讯风控
（账号被要求身份验证、限制登录乃至封号）。使用者需自行承担全部风险，不得用于违法或商业用途。
