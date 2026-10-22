# 今日账户用量

面板按北京时间（UTC+8）00:00 至次日 00:00，汇总官方已登录账户下所有 API Key、模型的 API 调用。消费直接汇总官方计费结果；Token 为缓存命中输入、缓存未命中输入与输出之和，不含请求次数字段。免费聊天产品和其他账户不在此账户 API 用量范围内。

“登录用量”打开本程序的独立 WebView2 窗口，地址为 `https://platform.deepseek.com/usage`。请登录与余额 API 密钥相同的账户。网页会话仅由本机专用浏览器配置保存，不读取其他浏览器资料。程序不接收密码或验证码；网页登录凭据只在官方页面脚本上下文用于同源请求，不传回圆环程序，也不写入统计 JSON。

登录完成后约每分钟刷新；面板实时读取结果。平台自身有统计延迟。未登录、登录失效、跨日或缓存超过 5 分钟时显示未知，不把缺失记录当作 0。

统计缓存（只含日期、币种、金额、Token 数、更新时间和状态）位于 `%LOCALAPPDATA%\DeepSeekQuotaUsage\today.json`。独立浏览器配置位于同目录 `WebProfile`。这些本机资料不要上传。

网页内部接口 `GET /api/v0/usage/by_api_key/amount` 和 `cost` 需要网页登录态，属于非公开稳定 API，未来可能变化；请求显式传 `start`、`end`、`tz=28800`，不筛选单个 API Key。只读请求不会调用模型。

构建：先运行 `build-platform.ps1` 下载并校验微软 WebView2 SDK，再运行 `build.ps1`。需要系统已安装 WebView2 Runtime。组件许可证随 `build/platform` 输出。
