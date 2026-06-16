using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace ClockSystem.WinUI.Services
{
    /// <summary>
    /// 音频播放：基于 MediaPlayer.MediaEnded 异步等待播放结束，
    /// 不再阻塞线程池线程（替代原先的 .AsTask().Result + Task.Delay().Wait()）。
    /// </summary>
    public sealed class AudioService : IDisposable
    {
        private readonly MediaPlayer _player = new MediaPlayer();
        private TaskCompletionSource<bool>? _endedTcs;
        private readonly object _gate = new object();

        public AudioService()
        {
            _player.MediaEnded += OnMediaEnded;
            _player.MediaFailed += OnMediaFailed;
        }

        /// <summary>播放指定音频直到结束；文件不存在或失败时安全返回。</summary>
        public async Task PlayAudioAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return;
            }

            TaskCompletionSource<bool> tcs;
            lock (_gate)
            {
                _endedTcs?.TrySetResult(false); // 取消上一个未完成的等待
                tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _endedTcs = tcs;
            }

            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath).AsTask().ConfigureAwait(false);
                var source = MediaSource.CreateFromStorageFile(file);
                _player.Source = source;
                _player.Play();

                // 等待播放结束、取消，或加载失败
                using (cancellationToken.Register(() => tcs.TrySetResult(false)))
                {
                    await tcs.Task.ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                AppLog.Error($"播放音频失败: {filePath}", ex);
                tcs.TrySetResult(false);
            }
        }

        /// <summary>立即停止当前播放。</summary>
        public void Stop()
        {
            lock (_gate)
            {
                _endedTcs?.TrySetResult(false);
            }
            try { _player.Pause(); }
            catch (Exception ex) { AppLog.Warn("停止音频失败: " + ex.Message); }
        }

        private void OnMediaEnded(MediaPlayer sender, object args)
        {
            lock (_gate)
            {
                _endedTcs?.TrySetResult(true);
            }
        }

        private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            AppLog.Warn($"音频播放失败: {args?.Error} {args?.ErrorMessage}");
            lock (_gate)
            {
                _endedTcs?.TrySetResult(false);
            }
        }

        public void Dispose()
        {
            _player.MediaEnded -= OnMediaEnded;
            _player.MediaFailed -= OnMediaFailed;
            Stop();
            _player.Dispose();
        }
    }
}
