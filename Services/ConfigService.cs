using ClockSystem.WinUI.Models;
using System;
using System.IO;
using System.Text.Json;

namespace ClockSystem.WinUI.Services
{
    public class ConfigService
    {
        /// <summary>运行时配置存放目录：%AppData%\ClockSystem.WinUI</summary>
        public static string AppDataDir { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClockSystem.WinUI");

        private readonly string _configPath = Path.Combine(AppDataDir, "clock.json");

        private readonly object _gate = new object();
        private ConfigModel? _cache;
        private bool _loaded;

        /// <summary>配置被保存（或重新加载）后触发，监听方应刷新自己的缓存。</summary>
        public event Action? ConfigChanged;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// 读取配置（带缓存，线程安全）。文件不存在或解析失败时回退到默认值。
        /// </summary>
        public ConfigModel LoadConfig()
        {
            lock (_gate)
            {
                if (_loaded)
                {
                    return _cache;
                }

                _cache = ReadFromDisk();
                _loaded = true;
                return _cache;
            }
        }

        /// <summary>保存配置并使缓存失效 / 重新加载，随后触发 ConfigChanged。</summary>
        public void SaveConfig(ConfigModel config)
        {
            if (config == null)
            {
                AppLog.Warn("SaveConfig 收到 null，已忽略");
                return;
            }

            lock (_gate)
            {
                _cache = config;
                _loaded = true;
                try
                {
                    Directory.CreateDirectory(AppDataDir);
                    var json = JsonSerializer.Serialize(config, _jsonOpts);
                    File.WriteAllText(_configPath, json);
                }
                catch (Exception ex)
                {
                    AppLog.Error("保存配置失败", ex);
                }
            }

            ConfigChanged?.Invoke();
        }

        private ConfigModel ReadFromDisk()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    var defaults = CreateDefaultConfig();
                    return defaults;
                }

                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<ConfigModel>(json) ?? CreateDefaultConfig();
            }
            catch (Exception ex)
            {
                AppLog.Error("读取配置失败，使用默认值", ex);
                return CreateDefaultConfig();
            }
        }

        private ConfigModel CreateDefaultConfig()
        {
            var config = new ConfigModel();
            try
            {
                Directory.CreateDirectory(AppDataDir);
                var json = JsonSerializer.Serialize(config, _jsonOpts);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                AppLog.Error("写入默认配置失败", ex);
            }
            return config;
        }
    }
}
