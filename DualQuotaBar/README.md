# Codex 双环

外圈为 5h 剩余额度，内圈为每周剩余额度；中心显示 5h 剩余百分比数字。透明 60×60 像素常驻任务栏，点击查看 Win-CodexBar 详情，再次点击圆环或点击外部收起。右键“关于”可查看 Wanting 署名及版权信息。

构建和依赖准备统一在仓库根目录执行 `build.ps1`。数据源、更新周期、代理及准确性限制见根目录 README。`InteractionChecks.ps1` 为交互检查，会短暂移动鼠标并打开/关闭详情，结束后恢复鼠标位置，不属于默认逻辑检查。

启动圆环后，可在仓库根目录运行 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\DualQuotaBar\VisibilityChecks.ps1` 检查 Codex 圆环的遮挡及隐藏恢复；该检查会短暂显示测试覆盖窗口，不属于默认逻辑检查。
