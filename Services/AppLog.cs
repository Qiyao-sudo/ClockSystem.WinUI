using System;
using System.IO;

namespace ClockSystem.WinUI.Services
{
    /// <summary>
    /// 轻量日志，替代散落各处的空 catch。写入 %AppData%\ClockSystem.WinUI\logs。
    /// </summary>
    public static class AppLog
    {
        private static readonly string _logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) ?? string.Empty,
            "ClockSystem.WinUI", "logs");

        private static readonly object _gate = new object();

        public static void Info(string message) => Write("INFO", message, null);

        public static void Warn(string message) => Write("WARN", message, null);

        public static void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

        private static void Write(string level, string message, Exception? ex)
        {
            try
            {
                Directory.CreateDirectory(_logDir);
                var path = Path.Combine(_logDir, $"app_{DateTime.Now:yyyy_MM_dd}.log");
                var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}";
                if (ex != null) line += Environment.NewLine + ex;
                lock (_gate)
                {
                    File.AppendAllText(path, line + Environment.NewLine);
                }
            }
            catch
            {
                // 日志本身失败不可影响主流程
            }
        }
    }
}
