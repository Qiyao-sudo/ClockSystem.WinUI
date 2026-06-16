using System.Collections.ObjectModel;

namespace ClockSystem.WinUI.Models
{
    public class ConfigModel
    {
        public PathConfig Path { get; set; } = new PathConfig();
        public SwitchConfig Switch { get; set; } = new SwitchConfig();
        public LightConfig Light { get; set; } = new LightConfig();
        public AppearanceConfig Appearance { get; set; } = new AppearanceConfig();

        public bool IsHourEnabled(int hour) => Switch.IsHourEnabled(hour);
        public bool IsHalfEnabled(int hour) => Switch.IsHalfEnabled(hour);
        public string GetHourPath(int hour) => Path.GetHourPath(hour);
        public string GetHalfPath(int hour) => Path.GetHalfPath(hour);
    }

    public class PathConfig
    {
        public string MusicPath { get; set; } = "Resources/music.mp3";
        public string BellPath { get; set; } = "Resources/bell.mp3";
        public ObservableCollection<KeyValuePair<int, string>> HourPaths { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();
        public ObservableCollection<KeyValuePair<int, string>> HalfPaths { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();

        public PathConfig()
        {
            for (int i = 0; i < 24; i++)
            {
                HourPaths.Add(new KeyValuePair<int, string>(i, $"Resources/hour_{i}.mp3"));
                HalfPaths.Add(new KeyValuePair<int, string>(i, $"Resources/half_{i}.mp3"));
            }
        }

        public string? GetHourPath(int hour) => Lookup(HourPaths, hour);
        public string? GetHalfPath(int hour) => Lookup(HalfPaths, hour);
        public void SetHourPath(int hour, string value) => Set(HourPaths, hour, value);
        public void SetHalfPath(int hour, string value) => Set(HalfPaths, hour, value);

        private static string? Lookup(System.Collections.Generic.IList<KeyValuePair<int, string>> list, int key)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key == key) return list[i].Value;
            }
            return null;
        }

        private static void Set(System.Collections.Generic.IList<KeyValuePair<int, string>> list, int key, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key == key)
                {
                    list[i] = new KeyValuePair<int, string>(key, value);
                    return;
                }
            }
        }
    }

    public class SwitchConfig
    {
        public ObservableCollection<KeyValuePair<int, bool>> HourSwitches { get; set; } = new ObservableCollection<KeyValuePair<int, bool>>();
        public ObservableCollection<KeyValuePair<int, bool>> HalfSwitches { get; set; } = new ObservableCollection<KeyValuePair<int, bool>>();

        public SwitchConfig()
        {
            for (int i = 0; i < 24; i++)
            {
                HourSwitches.Add(new KeyValuePair<int, bool>(i, i >= 6 && i <= 21));
                HalfSwitches.Add(new KeyValuePair<int, bool>(i, i >= 6 && i <= 21));
            }
        }

        public bool IsHourEnabled(int hour) => Lookup(HourSwitches, hour);
        public bool IsHalfEnabled(int hour) => Lookup(HalfSwitches, hour);
        public void SetHour(int hour, bool value) => Set(HourSwitches, hour, value);
        public void SetHalf(int hour, bool value) => Set(HalfSwitches, hour, value);

        private static bool Lookup(System.Collections.Generic.IList<KeyValuePair<int, bool>> list, int key)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key == key) return list[i].Value;
            }
            return false;
        }

        private static void Set(System.Collections.Generic.IList<KeyValuePair<int, bool>> list, int key, bool value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key == key)
                {
                    list[i] = new KeyValuePair<int, bool>(key, value);
                    return;
                }
            }
        }
    }

    public class LightConfig
    {
        public string LightOnTime { get; set; } = "18:00";
        public string LightOffTime { get; set; } = "06:00";
    }

    public class AppearanceConfig
    {
        /// <summary>主题：System / Light / Dark</summary>
        public string Theme { get; set; } = "System";
    }

    public class KeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
