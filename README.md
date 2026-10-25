# Codex + DeepSeek 任务栏双环

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform: Windows x64](https://img.shields.io/badge/Platform-Windows%20x64-0078D6.svg)](https://github.com/gwanting/codex-deepseek-quota-rings/releases)

> **圆环设计与制作：Wanting。** 作者：[Wanting / @gwanting](https://github.com/gwanting)

Windows 桌面用量工具：两个透明任务栏圆环、一个统一启动入口，以及配套的 Win-CodexBar。

当前发布版本：**[v0.3.3](https://github.com/gwanting/codex-deepseek-quota-rings/releases/tag/v0.3.3)**。从 [Releases](https://github.com/gwanting/codex-deepseek-quota-rings/releases) 下载便携包。

## 功能

| 视图 | 中心数字 | 外环 | 内环 |
| --- | --- | --- | --- |
| Codex | 5h 剩余百分比 | 5h 剩余比例 | 每周剩余比例 |
| DeepSeek | ≥1 元显示整数，不足 1 元显示两位小数 | 余额 / 满圈基准；颜色表示余额预警 | 距下次峰谷切换的剩余时间比例；峰期红、谷期绿 |

- 两个圆环均为 60×60 像素，透明背景，位于任务栏通知区左侧。
- 点击圆心、环线或环间空隙均能打开详情；再次点击圆环收起，点击窗口内部可操作，点击外部收起。
- 一个“Codex 用量”桌面图标启动三个组件，已运行的组件不会重复启动。
- 右键任一圆环，选择“关于”可查看作者及版权声明；两个圆环和统一启动器的文件属性也包含作者、产品名和版权信息。
- DeepSeek 详情面板支持磨砂背景、深色／浅色切换和 0%–100% 连续透明度调节；100% 时关闭背景材质，仅保留内容。按住滑块或轨道可连续拖动，松开自动保存设置。
- DeepSeek 右键菜单跟随相同背景设置，点击圆环或外部区域可关闭；左键详情保留从圆环展开及收起的缩放动画。
- DeepSeek 外环默认 **20 元为满圈**，10 元为半圈；可右键修改。外环颜色表示余额预警：不足 5 元红色，5–不足 10 元黄色，10–20 元蓝色，超过 20 元绿色。
- DeepSeek 中心数字与 Codex 使用相同字体、字号和居中位置；内外环的大小、粗细一致。整数余额直接舍去小数，详情面板仍显示两位小数。
- 内环颜色为峰期红色、谷期绿色；在峰谷切换后恢复满圈，按整个连续时段的剩余比例逐秒缩短；周末、节假日不会在午夜重置。悬停显示具体倒计时；无法确认切换时间时不显示进度。
- 余额详情面板提供“去充值”入口，用默认浏览器打开 DeepSeek 官方充值页。
- 面板显示**今日消费、今日 Token**：通过官方平台网页登录态，按北京时间汇总该账户所有 API Key、模型的 API 调用；Token 包含缓存命中输入、缓存未命中输入和输出，消费包含官方返回的模型及功能费用。
- Token 根据数量显示为“亿、千万、百万”，保留两位小数；不足百万时显示完整数字。消费保留两位小数。
- DeepSeek 峰谷按北京时间判断：工作日 09:00–12:00、14:00–18:00 为峰期，其余时段、周末和中国法定节假日为谷期。当前内置 **2026 年假日表**，后续年份需要更新；无法确认时提示灰色。

## 使用便携包

1. 下载本仓库 Release 的 Windows x64 ZIP，完整解压到可写目录。
2. 安装 [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) 和 [.NET 10 Desktop Runtime x64](https://dotnet.microsoft.com/download/dotnet/10.0)（若尚未安装）。
3. 双击 `Start.cmd`。在 Win-CodexBar 中完成 Codex 登录来源设置和 DeepSeek API 密钥配置。
4. 在余额面板点击“登录用量”，在独立官方窗口登录与 API 密钥相同的开放平台账户。成功后自动读取统计；约每分钟刷新。其他账户、免费聊天产品不计入此 API 用量。
5. 可运行 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\install.ps1` 创建统一桌面快捷方式。没有自动添加新的开机启动项。

DeepSeek 圆环复用当前 Windows 用户在 Win-CodexBar 中保存的密钥，不需要再次填写。Codex 圆环需要已安装并登录的 Codex 应用/CLI。

## 从源码构建

需要 Windows x64 自带的 .NET Framework C# 编译器（`Framework64/v4.0.30319/csc.exe`），以及上述运行时。构建脚本下载并校验固定版本的微软 WebView2 SDK；运行 JavaScript 请求检查还需要 Node.js。

```powershell
.\build.ps1                 # 下载并校验固定版本上游依赖，编译三个入口
.\scripts\Test.ps1          # 本地逻辑检查，无模型调用
.\scripts\Test-UI.ps1       # 可选：需要交互式桌面，会显示测试窗口并移动鼠标
.\install.ps1 -Start        # 创建统一桌面图标并启动
```

依赖已就绪时可用 `build.ps1 -Offline`。制作便携包：`scripts/New-PortablePackage.ps1 -Version v0.3.3`。

## 数据来源与限制

- **Codex**：读取当前 `CODEX_HOME/sessions`（默认 `~/.codex/sessions`）中额度事件的元数据，并以 CodexMeter 的只读接口查询补充。事件通常在数秒内反映；接口约每 30 秒查询一次，失败约每 15 秒重试。不同会话与服务端存在延迟，不能保证与 Codex 每一帧完全一致。数据过期或已重置时显示 `--`，不推算虚构额度。
- **DeepSeek**：查询官方 `https://api.deepseek.com/user/balance`；约每 60 秒更新，失败每 15 秒重试。过期金额不作为当前读数展示。外环是相对用户设定基准的比例，不是官方请求次数额度。
- **今日账户用量**：独立 WebView2 窗口在官方平台页面内只读查询 `usage/by_api_key/amount`、`cost`，显式采用北京时间边界。网页内部接口可能变化，官方统计也可能有延迟。未登录、登录失效、跨日或缓存超过 5 分钟时显示未知，不能视为零。详细说明见 [PLATFORM_USAGE.md](DeepSeekBalance/PLATFORM_USAGE.md)。
- Windows 系统代理由统一启动器传给子程序；不会为你切换代理节点。
- Win-CodexBar 的详情由上游独立刷新，和圆环不是同一个界面数据对象。

## 目录与许可证

`DualQuotaBar/` 为 Codex 圆环与统一启动器；`DeepSeekBalance/` 为余额视图；`third_party/` 保存固定依赖清单及许可证。原版 Win-CodexBar 和 CodexMeter 以校验后的原文件随便携包提供。

本项目代码 MIT，第三方许可见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。仓库及便携包均不附带个人 API 密钥、账户配置或日志。

官方网页登录态保存在本机 `%LOCALAPPDATA%\DeepSeekQuotaUsage\WebProfile`，汇总缓存位于同目录 `today.json`；这些内容不随仓库或便携包上传，也不读取其他浏览器资料。查询过程不发起模型调用。

外观设置保存在本机 `%LOCALAPPDATA%\DeepSeekQuotaUsage\appearance.json`，不随发布包分发。更新记录见 [CHANGELOG.md](CHANGELOG.md)。

## 问题反馈

使用问题和功能建议请提交至 [Issues](https://github.com/gwanting/codex-deepseek-quota-rings/issues)，附上 Windows 版本、程序版本、复现步骤及脱敏截图。请勿上传 API 密钥、登录令牌、验证码、浏览器用户资料目录或未经脱敏的账户文件。

## 版权与许可

圆环设计与制作：**Wanting**。

Copyright © 2026 Wanting ([gwanting](https://github.com/gwanting))。

许可证：[MIT License](LICENSE)。第三方组件许可见 [第三方声明](THIRD_PARTY_NOTICES.md)。

## 免责声明

本软件按“现状”提供，不附带 MIT 许可证所述的明示或暗示担保；责任限制详见 [LICENSE](LICENSE)。额度、余额和消费统计可能因网络、缓存、官方统计延迟或接口变化而与官方页面存在差异，请以相应服务提供方的账户记录为准。

本项目为独立第三方工具，与 OpenAI、DeepSeek、Microsoft 及相关上游项目不存在官方隶属或背书关系。文中产品名称及商标属于各自权利人。
