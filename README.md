# Codex + DeepSeek 任务栏双环

Windows 桌面用量工具：两个透明任务栏圆环、一个统一启动入口，以及配套的 Win-CodexBar。

## 功能

| 视图 | 中心数字 | 外环 | 内环 |
| --- | --- | --- | --- |
| Codex | 5h 剩余百分比 | 5h 剩余比例 | 每周剩余比例 |
| DeepSeek | 账户余额，保留两位小数 | 余额 / 满圈基准；峰期红、谷期绿 | 余额预警颜色 |

- 两个圆环均为 60×60 像素，透明背景，位于任务栏通知区左侧。
- 点击圆心、环线或环间空隙均能打开详情；点击窗口内部可操作，点击外部收起。
- 一个“Codex 用量”桌面图标启动三个组件，已运行的组件不会重复启动。
- DeepSeek 默认 **20 元为满圈**，10 元为半圈；可右键修改。预警：不足 5 元红色，5–不足 10 元黄色，10–20 元蓝色，超过 20 元绿色。
- DeepSeek 峰谷按北京时间判断：工作日 09:00–12:00、14:00–18:00 为峰期，其余时段、周末和中国法定节假日为谷期。当前内置 **2026 年假日表**，后续年份需要更新；无法确认时提示灰色。

## 使用便携包

1. 下载本仓库 Release 的 Windows x64 ZIP，完整解压到可写目录。
2. 安装 [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) 和 [.NET 10 Desktop Runtime x64](https://dotnet.microsoft.com/download/dotnet/10.0)（若尚未安装）。
3. 双击 `Start.cmd`。在 Win-CodexBar 中完成 Codex 登录来源设置和 DeepSeek API 密钥配置。
4. 可运行 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\install.ps1` 创建统一桌面快捷方式。没有自动添加新的开机启动项。

DeepSeek 圆环复用当前 Windows 用户在 Win-CodexBar 中保存的密钥，不需要再次填写。Codex 圆环需要已安装并登录的 Codex 应用/CLI。

## 从源码构建

需要 Windows x64 自带的 .NET Framework C# 编译器（`Framework64/v4.0.30319/csc.exe`），以及上述运行时。

```powershell
.\build.ps1                 # 下载并校验固定版本上游依赖，编译三个入口
.\scripts\Test.ps1          # 本地逻辑检查，无模型调用
.\install.ps1 -Start        # 创建统一桌面图标并启动
```

依赖已就绪时可用 `build.ps1 -Offline`。制作便携包：`scripts/New-PortablePackage.ps1 -Version v0.1.0`。

## 数据来源与限制

- **Codex**：读取当前 `CODEX_HOME/sessions`（默认 `~/.codex/sessions`）中额度事件的元数据，并以 CodexMeter 的只读接口查询补充。事件通常在数秒内反映；接口约每 30 秒查询一次，失败约每 15 秒重试。不同会话与服务端存在延迟，不能保证与 Codex 每一帧完全一致。数据过期或已重置时显示 `--`，不推算虚构额度。
- **DeepSeek**：查询官方 `https://api.deepseek.com/user/balance`；约每 60 秒更新，失败每 15 秒重试。过期金额不作为当前读数展示。外环是相对用户设定基准的比例，不是官方请求次数额度。
- Windows 系统代理由统一启动器传给子程序；不会为你切换代理节点。
- Win-CodexBar 的详情由上游独立刷新，和圆环不是同一个界面数据对象。

## 目录与许可证

`DualQuotaBar/` 为 Codex 圆环与统一启动器；`DeepSeekBalance/` 为余额视图；`third_party/` 保存固定依赖清单及许可证。原版 Win-CodexBar 和 CodexMeter 以校验后的原文件随便携包提供。

本项目代码 MIT，第三方许可见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。仓库及便携包均不附带个人 API 密钥、账户配置或日志。
