using ClockSystem.WinUI.Models;
using ClockSystem.WinUI.Services.Communication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClockSystem.WinUI.Services
{
    public class ClockService
    {
        private readonly ConfigService _configService;
        private readonly AudioService _audioService;
        private readonly IClockCommunication _communication;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Func<DateTime> _timeProvider;
        private bool _lightOn = false;

        // 报时去重：记录上一次已触发的整点 / 半点，避免同一秒内重复触发
        private int _lastHourAlarm = -1;
        private int _lastHalfAlarm = -1;
        // 报时串行化，避免整点/半点叠加播放相互打断
        private readonly SemaphoreSlim _alarmLock = new SemaphoreSlim(1, 1);

        public bool LightOn => _lightOn;

        public event Action<string>? LogMessage;

        /// <summary>灯光状态变化（开/关）。UI 与通信层均可订阅。</summary>
        public event Action<bool>? LightStatusChanged;

        public ClockService(ConfigService configService, AudioService audioService, IClockCommunication? communication = null)
        {
            _configService = configService;
            _audioService = audioService;
            _communication = communication ?? new StubClockCommunication();
            _timeProvider = () => DateTime.Now;
        }

        public void SetTimeProvider(Func<DateTime> provider)
        {
            _timeProvider = provider;
        }

        public void Start()
        {
            InitializeLightStatus();
            Task.Run(() => RunClock(_cts.Token));
        }

        public void Stop()
        {
            try { _cts.Cancel(); }
            catch (Exception ex) { AppLog.Warn("停止时钟失败: " + ex.Message); }
        }

        private void InitializeLightStatus()
        {
            try
            {
                var now = _timeProvider();
                var config = _configService.LoadConfig();
                _lightOn = ComputeLightOn(now, config);
            }
            catch (Exception ex)
            {
                AppLog.Error("初始化灯光状态失败", ex);
                _lightOn = false;
            }
        }

        private void RunClock(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var now = _timeProvider();
                    var config = _configService.LoadConfig(); // 已缓存，开销极小

                    CheckLightStatus(now, config);
                    CheckHourAlarm(now, config);
                    CheckHalfAlarm(now, config);

                    token.WaitHandle.WaitOne(1000);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AppLog.Error("时钟主循环异常", ex);
                    try { token.WaitHandle.WaitOne(1000); } catch { }
                }
            }
        }

        private void CheckHourAlarm(DateTime now, ConfigModel config)
        {
            // 在每小时的 59:30 起提前触发，给报时序列留出播放时间，使其在整点附近敲响
            if (now.Minute == 59 && now.Second >= 30)
            {
                var nextHour = (now.Hour + 1) % 24;

                if (_lastHourAlarm == nextHour) return;

                if (config.IsHourEnabled(nextHour))
                {
                    _lastHourAlarm = nextHour;
                    // Fire-and-forget，但经 _alarmLock 串行化
                    _ = RunAlarmAsync(() => ExecuteHourAlarmAsync(nextHour, config, now));
                }
            }

            // 离开整点窗口后重置，使下一小时可再次触发
            if (now.Minute == 0 && now.Second >= 5)
            {
                _lastHourAlarm = -1;
            }
        }

        private async Task ExecuteHourAlarmAsync(int hour, ConfigModel config, DateTime now)
        {
            Log("整点报时 - " + hour + "点", now);

            try
            {
                if (!string.IsNullOrEmpty(config.Path.MusicPath))
                {
                    await _audioService.PlayAudioAsync(config.Path.MusicPath, _cts.Token);
                }

                if (!string.IsNullOrEmpty(config.Path.BellPath))
                {
                    var bellCount = ClockLogic.BellCount(hour);
                    for (int i = 0; i < bellCount && !_cts.IsCancellationRequested; i++)
                    {
                        await _audioService.PlayAudioAsync(config.Path.BellPath, _cts.Token);
                        if (i < bellCount - 1)
                        {
                            await Task.Delay(500, _cts.Token);
                        }
                    }
                }

                var hourPath = config.GetHourPath(hour);
                if (!string.IsNullOrEmpty(hourPath))
                {
                    await _audioService.PlayAudioAsync(hourPath, _cts.Token);
                }

                Log("整点报时完成", now);
            }
            catch (OperationCanceledException)
            {
                Log("整点报时已取消", now);
            }
            catch (Exception ex)
            {
                AppLog.Error("整点报时执行失败", ex);
                Log("整点报时执行失败", now);
            }
        }

        private void CheckHalfAlarm(DateTime now, ConfigModel config)
        {
            // 半点窗口：XX:29:30 触发，报的是【当前小时】的半点
            if (now.Minute == 29 && now.Second >= 30)
            {
                var currentHour = now.Hour; // 修复：原代码误用 (now.Hour + 1) % 24，导致 14:30 报成 15 点半
                if (_lastHalfAlarm == currentHour) return;

                if (config.IsHalfEnabled(currentHour))
                {
                    _lastHalfAlarm = currentHour;
                    _ = RunAlarmAsync(() => ExecuteHalfAlarmAsync(currentHour, config, now));
                }
            }

            if (now.Minute == 30 && now.Second >= 5)
            {
                _lastHalfAlarm = -1;
            }
        }

        private async Task ExecuteHalfAlarmAsync(int hour, ConfigModel config, DateTime now)
        {
            Log("半点报时 - " + hour + "点半", now);

            try
            {
                if (!string.IsNullOrEmpty(config.Path.MusicPath))
                {
                    await _audioService.PlayAudioAsync(config.Path.MusicPath, _cts.Token);
                }

                if (!string.IsNullOrEmpty(config.Path.BellPath))
                {
                    await _audioService.PlayAudioAsync(config.Path.BellPath, _cts.Token);
                }

                var halfPath = config.GetHalfPath(hour);
                if (!string.IsNullOrEmpty(halfPath))
                {
                    await _audioService.PlayAudioAsync(halfPath, _cts.Token);
                }

                Log("半点报时完成", now);
            }
            catch (OperationCanceledException)
            {
                Log("半点报时已取消", now);
            }
            catch (Exception ex)
            {
                AppLog.Error("半点报时执行失败", ex);
                Log("半点报时执行失败", now);
            }
        }

        private async Task RunAlarmAsync(Func<Task> action)
        {
            await _alarmLock.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                _alarmLock.Release();
            }
        }

        private void CheckLightStatus(DateTime now, ConfigModel config)
        {
            bool newLightOn = ComputeLightOn(now, config);

            if (newLightOn != _lightOn)
            {
                _lightOn = newLightOn;
                Log("钟表灯光 " + (_lightOn ? "开启" : "关闭"), now);
                LightStatusChanged?.Invoke(_lightOn);

                // 下发到硬件（占位实现仅记录日志）
                _ = SafeSetLightAsync(_lightOn);
            }
        }

        private async Task SafeSetLightAsync(bool on)
        {
            try
            {
                await _communication.SetLightAsync(on).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppLog.Error("下发灯光指令失败", ex);
            }
        }

        private bool ComputeLightOn(DateTime now, ConfigModel config)
        {
            var onTime = ParseTime(config.Light.LightOnTime, TimeSpan.FromHours(18));
            var offTime = ParseTime(config.Light.LightOffTime, TimeSpan.FromHours(6));
            var current = new TimeSpan(now.Hour, now.Minute, now.Second);
            return ClockLogic.IsLightOn(current, onTime, offTime);
        }

        private static TimeSpan ParseTime(string text, TimeSpan fallback)
        {
            return TimeSpan.TryParse(text, out var ts) ? ts : fallback;
        }

        private void Log(string message, DateTime? time = null)
        {
            var logTime = time ?? _timeProvider();
            var logMessage = "[" + logTime.ToString("HH:mm:ss") + "] " + message;
            LogMessage?.Invoke(logMessage);
        }
    }
}
