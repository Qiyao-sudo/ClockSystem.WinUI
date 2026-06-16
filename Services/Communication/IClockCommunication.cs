using System.Threading;
using System.Threading.Tasks;

namespace ClockSystem.WinUI.Services.Communication
{
    /// <summary>
    /// 与塔钟子母钟硬件的通信抽象。
    /// 真实协议（串口 / TCP / Modbus 等）取决于现场硬件，请在实现类中补全字节帧。
    /// </summary>
    public interface IClockCommunication
    {
        bool IsConnected { get; }

        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task DisconnectAsync(CancellationToken cancellationToken = default);

        /// <summary>下发母钟时间，使子钟与母钟走时同步。</summary>
        Task SyncTimeAsync(int year, int month, int day, int hour, int minute, int second, CancellationToken cancellationToken = default);

        /// <summary>远程调节子钟走时快慢（单位由硬件定义）。</summary>
        Task AdjustSlaveTimingAsync(int adjustment, CancellationToken cancellationToken = default);

        /// <summary>远程上传报时音频文件。</summary>
        Task UploadChimeFileAsync(string localPath, string remoteName, CancellationToken cancellationToken = default);

        /// <summary>开关钟表灯光。</summary>
        Task SetLightAsync(bool on, CancellationToken cancellationToken = default);
    }
}
