# Third-party components

This repository's original ring interfaces and launcher are licensed under MIT (see LICENSE).

| Component | Pinned version | License | Distribution |
| --- | --- | --- | --- |
| [Win-CodexBar](https://github.com/nesszer/Win-CodexBar) | v0.70.0 | MIT | Unmodified portable executable in the portable release; downloaded and verified when building source |
| [CodexMeter](https://github.com/Bangerz/CodexMeter) | v0.1.0 | CC0-1.0 | Unmodified executable used only for read-only quota checks |
| [Microsoft WebView2 SDK](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.4258.31) | 1.0.4258.31 | Microsoft license; full terms included | Core/WinForms assemblies and x64 loader for the official DeepSeek login window |

Full upstream license texts are included in `third_party/Win-CodexBar.LICENSE` and `third_party/CodexMeter.LICENSE`. Public release URLs and pinned executable SHA-256 values are in `third_party/dependencies.json`.

WebView2 license and notices are in `third_party/WebView2.LICENSE` and `third_party/WebView2.NOTICE`, and are also distributed with the helper. The system WebView2 Runtime is installed separately. The manifest records the pinned SDK archive hash.

Win-CodexBar is an upstream application dependency; this repository does not claim authorship of its source or executable. Model/account names belong to their respective owners. API keys, login data, user settings, account logs, and personal quota snapshots are not included.
