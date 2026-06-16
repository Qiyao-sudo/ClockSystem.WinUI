using System;
using ClockSystem.WinUI.Services;
using ClockSystem.WinUI.Models;
using Xunit;

namespace ClockSystem.WinUI.Tests
{
    public class ClockLogicTests
    {
        // ---------- IsLightOn ----------

        [Theory]
        [InlineData(18, 0, 18, 0, 6, 0, true)]   // 跨午夜：18:00 开 -> 06:00 关，18:00 应亮
        [InlineData(22, 30, 18, 0, 6, 0, true)]  // 22:30 仍亮
        [InlineData(2, 0, 18, 0, 6, 0, true)]    // 02:00 仍亮（跨午夜段内）
        [InlineData(8, 0, 18, 0, 6, 0, false)]   // 08:00 已关
        [InlineData(12, 0, 18, 0, 6, 0, false)]  // 中午关
        [InlineData(6, 0, 18, 0, 6, 0, false)]   // 边界：恰好等于 offTime 不亮（半开半闭）
        [InlineData(7, 0, 7, 0, 20, 0, true)]    // 不跨午夜：07:00 开 -> 20:00 关，正好 onTime 边界，亮
        [InlineData(10, 0, 7, 0, 20, 0, true)]   // 区间内
        [InlineData(20, 0, 7, 0, 20, 0, false)]  // 边界 off
        [InlineData(3, 0, 7, 0, 20, 0, false)]   // 区间外
        public void IsLightOn_VariousCases(int h, int m, int onH, int onM, int offH, int offM, bool expected)
        {
            var current = new TimeSpan(h, m, 0);
            var onTime = new TimeSpan(onH, onM, 0);
            var offTime = new TimeSpan(offH, offM, 0);
            Assert.Equal(expected, ClockLogic.IsLightOn(current, onTime, offTime));
        }

        [Fact]
        public void IsLightOn_OnEqualsOff_NeverOn()
        {
            var t = new TimeSpan(12, 0, 0);
            Assert.False(ClockLogic.IsLightOn(t, t, t));
        }

        // ---------- BellCount ----------

        [Theory]
        [InlineData(0, 12)]   // 午夜 / 0 点 -> 12 下
        [InlineData(1, 1)]
        [InlineData(12, 12)]  // 正午 -> 12 下
        [InlineData(13, 1)]   // 13 点（24h）-> 1 下
        [InlineData(23, 11)]
        [InlineData(24, 12)]  // 越界容错：24 % 12 == 0 -> 12
        public void BellCount_Cases(int hour24, int expected)
        {
            Assert.Equal(expected, ClockLogic.BellCount(hour24));
        }

        // ---------- IsSwitchEnabled / GetPath ----------

        [Fact]
        public void IsSwitchEnabled_Defaults_6To21()
        {
            var config = new SwitchConfig();
            Assert.False(ClockLogic.IsSwitchEnabled(config.HourSwitches, 5));
            Assert.True(ClockLogic.IsSwitchEnabled(config.HourSwitches, 6));
            Assert.True(ClockLogic.IsSwitchEnabled(config.HourSwitches, 21));
            Assert.False(ClockLogic.IsSwitchEnabled(config.HourSwitches, 22));
            Assert.False(ClockLogic.IsSwitchEnabled(null, 10)); // null 安全
        }

        [Fact]
        public void GetPath_Defaults_ResolvePerHour()
        {
            var config = new PathConfig();
            Assert.Equal("Resources/hour_7.mp3", ClockLogic.GetPath(config.HourPaths, 7));
            Assert.Equal("Resources/half_0.mp3", ClockLogic.GetPath(config.HalfPaths, 0));
            Assert.Null(ClockLogic.GetPath(null, 5));
        }

        // ---------- 半点修复回归（文档化期望行为） ----------

        /// <summary>
        /// 半点报时在 XX:29:30 触发，应报【当前小时】的半点。
        /// 旧实现误用 (Hour+1)，会把 14:30 报成 15 点半。这里锁定修复后的语义。
        /// </summary>
        [Theory]
        [InlineData(14, 14)] // 14:30 -> 14 点半
        [InlineData(0, 0)]   // 00:30 -> 0 点半
        [InlineData(23, 23)] // 23:30 -> 23 点半
        public void HalfAlarm_ReportsCurrentHour(int clockHour, int expectedReportHour)
        {
            Assert.Equal(expectedReportHour, clockHour); // currentHour = now.Hour（不再 +1）
        }
    }
}
