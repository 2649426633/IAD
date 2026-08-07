# IAD - 通用工业瑕疵质检系统

本仓库采用 **C# WinForms (.NET Framework 4.7.2)**，按工业软件页面职责重新组织。

## 项目结构

- `Shell/MainForm`：全屏主壳、左侧导航、页面切换、底部运行状态。
- `Pages/DashboardPage`：工作台。
- `Pages/ProductDefinitionPage`：产品定义、定位模板、标定。
- `Pages/DatasetAnnotationPage`：数据集标注。
- `Pages/TemplateRecognitionPage`：少样本瑕疵模板识别与辅助扩标。
- `Pages/TrainingModelsPage`：训练任务、验证验收、模型库。
- `Pages/RulesRecipePage`：质量规则、检测区域、Inspection Recipe。
- `Pages/OnlineInspectionPage`：在线/离线图片检测工作台。
- `Pages/TraceabilityPage`：检测结果追溯与审计。
- `Pages/SystemSettingsPage`：运行时、存储、部署、备份、适配器配置。
- `UI/UiTheme`、`UI/UiFactory`：统一灰/黑/白视觉规范与通用控件工厂。

## 窗口策略

程序启动后强制最大化、无可调整边框，不提供最小化与普通窗口模式，仅保留关闭入口。

## 后续集成边界

当前版本完成 UI 骨架与静态演示数据，后续可按架构接入 HALCON、SQLite、ONNX Runtime、Python Trainer Worker、相机/PLC/MES 适配器。
