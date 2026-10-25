# 第三方组件与版权声明 / Third-party notices

本项目原创部分 Copyright © 2026 Wanting，采用 [MIT License](LICENSE)。第三方组件不属于本项目原创部分，其版权、许可和声明继续适用；本仓库的 MIT 许可不替代这些组件的原始许可。

Win-CodexBar 随附的 MIT 许可原文署名为 **Copyright (c) 2025 Peter Steinberger**，本项目保留该原始声明。CodexMeter 采用 CC0-1.0；WebView2 保留 Microsoft Corporation 的版权及分发条款。各组件的完整文本见下方链接。构建与打包时应同时保留适用的第三方许可和声明。

本项目为独立集成工具，不代表相关服务提供方或上游项目，亦未声称获得其官方背书。

This repository's original ring interfaces and launcher are licensed under MIT (see LICENSE).

| Component | Pinned version | License | Distribution |
| --- | --- | --- | --- |
| [Win-CodexBar](https://github.com/nesszer/Win-CodexBar) | v0.70.0 | MIT | Unmodified portable executable in the portable release; downloaded and verified when building source |
| [CodexMeter](https://github.com/Bangerz/CodexMeter) | v0.1.0 | CC0-1.0 | Unmodified executable used only for read-only quota checks |
| [Microsoft WebView2 SDK](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.4258.31) | 1.0.4258.31 | Microsoft license; full terms included | Core/WinForms assemblies and x64 loader for the official DeepSeek login window |

Full upstream license texts are included in [Win-CodexBar.LICENSE](third_party/Win-CodexBar.LICENSE) and [CodexMeter.LICENSE](third_party/CodexMeter.LICENSE). Public release URLs and pinned executable SHA-256 values are in [dependencies.json](third_party/dependencies.json).

WebView2 license and notices are in [WebView2.LICENSE](third_party/WebView2.LICENSE) and [WebView2.NOTICE](third_party/WebView2.NOTICE), and are also distributed with the helper. The system WebView2 Runtime is installed separately. The manifest records the pinned SDK archive hash.

Win-CodexBar is an upstream application dependency; this repository does not claim authorship of its source or executable. Model/account names belong to their respective owners. API keys, login data, user settings, account logs, and personal quota snapshots are not included.
