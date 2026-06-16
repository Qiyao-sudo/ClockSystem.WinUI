using ClockSystem.WinUI.Models;
using ClockSystem.WinUI.Services;
using System;
using System.Text;

namespace ClockSystem.WinUI.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly ConfigService _configService;
        private readonly ClockService _clockService;
        private readonly AudioService _audioService;

        // 母钟时间与偏移
        private DateTime _masterTime;
        private bool _useSystemTime = true;
        private double _timeOffsetSeconds = 0;

        // 高精度子秒插值（毫秒），供秒针平滑走动
        private DateTime _lastBaseTime;
        private double _subSecondMs = 0;

        // 日志：环形缓冲，限制长度避免无限增长
        private const int MaxLogLines = 500;
        private readonly System.Collections.Generic.LinkedList<string> _logLines = new();
        private string _logText = "";
        private bool _isLightOn;

        public event Action<string>? LogMessageReceived;

        public DateTime MasterTime
        {
            get => _masterTime;
            private set
            {
                if (_masterTime != value)
                {
                    _masterTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeDisplay));
                }
            }
        }

        /// <summary>带子秒插值的当前时间，用于平滑秒针。</summary>
        public DateTime HighPrecisionTime => _lastBaseTime.AddMilliseconds(_subSecondMs);

        public string TimeDisplay => MasterTime.ToString("yyyy-MM-dd HH:mm:ss");

        public string LogText
        {
            get => _logText;
            private set
            {
                if (_logText != value)
                {
                    _logText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLightOn
        {
            get => _isLightOn;
            private set
            {
                if (_isLightOn != value)
                {
                    _isLightOn = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel() : this(new ConfigService()) { }

        public MainViewModel(ConfigService configService)
        {
            _configService = configService;
            _audioService = new AudioService();
            _clockService = new ClockService(_configService, _audioService);

            _clockService.LogMessage += OnClockLog;
            _clockService.LightStatusChanged += on => IsLightOn = on;

            MasterTime = DateTime.Now;
            IsLightOn = _clockService.LightOn;
            _clockService.SetTimeProvider(() => MasterTime);
            _clockService.Start();
        }

        private void OnClockLog(string message)
        {
            AppendLog(message);
            LogMessageReceived?.Invoke(message);
        }

        private void AppendLog(string message)
        {
            _logLines.AddLast(message);
            while (_logLines.Count > MaxLogLines)
            {
                _logLines.RemoveFirst();
            }

            // 仅在需要时重建文本（避免每次追加都全量拼接造成 O(n²)）
            var sb = new StringBuilder();
            foreach (var line in _logLines)
            {
                sb.Append(line).Append('\n');
            }
            LogText = sb.ToString();
        }

        /// <summary>每帧（约 16ms）由 UI DispatcherTimer 调用：推进时间与子秒插值。</summary>
        public void Tick(TimeSpan elapsedSinceLastTick)
        {
            var realNow = DateTime.Now;

            if (_useSystemTime)
            {
                MasterTime = realNow;
            }
            else
            {
                MasterTime = realNow.AddSeconds(_timeOffsetSeconds);
            }

            // 子秒插值：在两次 Tick 之间用经过时长插值，秒针更顺滑
            _subSecondMs = Math.Min(elapsedSinceLastTick.TotalMilliseconds, 1000);
            _lastBaseTime = _masterTime;

            OnPropertyChanged(nameof(HighPrecisionTime));
        }

        public void SyncSystemTime()
        {
            _useSystemTime = true;
            _timeOffsetSeconds = 0;
            MasterTime = DateTime.Now;
            AppendLog("[手动] 已同步为系统时间");
        }

        public void AdjustTime(DateTime newTime)
        {
            _useSystemTime = false;
            _timeOffsetSeconds = (newTime - DateTime.Now).TotalSeconds;
            MasterTime = newTime;
            AppendLog($"[手动] 母钟时间已调整为 {newTime:yyyy-MM-dd HH:mm:ss}");
        }

        public ConfigModel LoadConfig() => _configService.LoadConfig();

        public void SaveConfig(ConfigModel config) => _configService.SaveConfig(config);

        public void Dispose()
        {
            _clockService?.Stop();
            _audioService?.Dispose();
        }
    }
}
