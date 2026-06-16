using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClockSystem.WinUI.Services.Communication
{
    /// <summary>
    /// 占位实现：所有指令仅记录日志、不真正下发。
    /// 待硬件通信协议明确后，用 Serial / Tcp 等实现替换（建议通过依赖注入注入）。
    /// </summary>
    public sealed class StubClockCommunication : IClockCommunication
    {
        public bool IsConnected { get; private set; }

        public Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = true;
            AppLog.Info("[Comm/Stub] 已连接（占位）");
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = false;
            AppLog.Info("[Comm/Stub] 已断开（占位）");
            return Task.CompletedTask;
        }

        public Task SyncTimeAsync(int year, int month, int day, int hour, int minute, int second, CancellationToken cancellationToken = default)
        {
            AppLog.Info($"[Comm/Stub] 下发时间同步: {year:0000}-{month:00}-{day:00} {hour:00}:{minute:00}:{second:00}（占位，未真实发送）");
            return Task.CompletedTask;
        }

        public Task AdjustSlaveTimingAsync(int adjustment, CancellationToken cancellationToken = default)
        {
            AppLog.Info($"[Comm/Stub] 调节子钟走时: {adjustment}（占位，未真实发送）");
            return Task.CompletedTask;
        }

        public Task UploadChimeFileAsync(string localPath, string remoteName, CancellationToken cancellationToken = default)
        {
            AppLog.Info($"[Comm/Stub] 上传报时文件 {localPath} -> {remoteName}（占位，未真实发送）");
            return Task.CompletedTask;
        }

        public Task SetLightAsync(bool on, CancellationToken cancellationToken = default)
        {
            AppLog.Info($"[Comm/Stub] 设置灯光: {(on ? "开" : "关")}（占位，未真实发送）");
            return Task.CompletedTask;
        }
    }
}
