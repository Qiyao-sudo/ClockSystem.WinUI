using System;

namespace ClockSystem.WinUI.Services
{
    /// <summary>
    /// 纯逻辑、无 WinUI 依赖的时钟判定算法，便于单元测试。
    /// </summary>
    public static class ClockLogic
    {
        /// <summary>
        /// 根据当前时间与灯光开/关时间判断灯光是否应开启（支持跨午夜时段，如 18:00 开 -> 06:00 关）。
        /// </summary>
        public static bool IsLightOn(TimeSpan current, TimeSpan onTime, TimeSpan offTime)
        {
            if (onTime == offTime)
            {
                return false;
            }

            if (onTime < offTime)
            {
                return current >= onTime && current < offTime;
            }

            // 跨午夜：onTime > offTime，例如 18:00 -> 06:00
            return current >= onTime || current < offTime;
        }

        /// <summary>
        /// 整点报时时敲钟的次数（12 小时制：13 点 -> 1 下，0 点 -> 12 下）。
        /// </summary>
        public static int BellCount(int hour24)
        {
            var bell = hour24 % 12;
            return bell == 0 ? 12 : bell;
        }

        /// <summary>
        /// 在给定集合中查找指定 Key 的开关状态，未找到返回 false。
        /// </summary>
        public static bool IsSwitchEnabled(System.Collections.Generic.IReadOnlyList<Models.KeyValuePair<int, bool>> switches, int key)
        {
            if (switches == null) return false;
            for (int i = 0; i < switches.Count; i++)
            {
                if (switches[i].Key == key)
                {
                    return switches[i].Value;
                }
            }
            return false;
        }

        /// <summary>
        /// 在给定集合中查找指定 Key 的路径，未找到返回 null。
        /// </summary>
        public static string? GetPath(System.Collections.Generic.IReadOnlyList<Models.KeyValuePair<int, string>> paths, int key)
        {
            if (paths == null) return null;
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i].Key == key)
                {
                    return paths[i].Value;
                }
            }
            return null;
        }
    }
}
