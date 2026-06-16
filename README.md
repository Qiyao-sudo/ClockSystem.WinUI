# **WinUI子母钟管理系统** 

**本项目为**[**子母钟管理系统**](https://github.com/qiyao11/ClockSystem)**的WinUI版本，用于安装在个人电脑及远程服务器上以远程控制塔钟子母钟**

**系统包含（将实现）以下功能**

###### **母钟时间**

母钟基础时间显示（时 / 分 / 秒、日期 / 星期）✅

母钟时间调整✅

###### **远程控制**

> 远程控制已搭建通信抽象层 `IClockCommunication`（`Services/Communication/`），
> 当前提供占位实现 `StubClockCommunication`（仅记录日志，不下发真实指令）。
> 待现场硬件通信协议（串口 / TCP / Modbus 等）明确后，实现对应类即可接入。

远程调节子钟走时🟡（接口已就绪，待接入协议）

远程上传报时文件🟡（接口已就绪，待接入协议）

钟表灯光远程开关✅（已通过抽象层下发，占位实现）

###### **钟表灯光**

定时开关钟表灯光✅

###### **报时设置**

自定义报时✅

###### **部署说明**

该系统部署于塔钟远程端，适配 Windows10 / Windows11 系统（.NET 10）。

- 运行时配置保存在 `%AppData%\ClockSystem.WinUI\clock.json`，首次启动自动生成默认值。
- 报时音频默认放在程序目录的 `Resources/` 下（也可在「设置 → 文件路径」改为绝对路径）。
- 运行日志写入 `%AppData%\ClockSystem.WinUI\logs\app_YYYY_MM_DD.log`。



