using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace ClockSystem.WinUI.Dialogs
{
    public sealed partial class TimeAdjustDialog : ContentDialog
    {
        public DateTime SelectedTime { get; private set; }

        /// <summary>用户点击“应用”时构造出的时间是否合法。</summary>
        public bool IsValid { get; private set; }

        private DateTime _originalTime;

        public TimeAdjustDialog(DateTime currentTime)
        {
            this.InitializeComponent();
            _originalTime = currentTime;

            YearBox.Value = currentTime.Year;
            MonthBox.Value = currentTime.Month;
            DayBox.Value = currentTime.Day;
            HourBox.Value = currentTime.Hour;
            MinuteBox.Value = currentTime.Minute;
            SecondBox.Value = currentTime.Second;

            YearBox.ValueChanged += (s, e) => UpdatePreview();
            MonthBox.ValueChanged += (s, e) => UpdatePreview();
            DayBox.ValueChanged += (s, e) => UpdatePreview();
            HourBox.ValueChanged += (s, e) => UpdatePreview();
            MinuteBox.ValueChanged += (s, e) => UpdatePreview();
            SecondBox.ValueChanged += (s, e) => UpdatePreview();

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (TryBuild(out var preview, out var error))
            {
                PreviewTextBlock.Text = preview.ToString("yyyy-MM-dd HH:mm:ss");
                PreviewTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LimeGreen);
                ErrorTextBlock.Text = "";
                ErrorBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                PreviewTextBlock.Text = "无效日期时间";
                PreviewTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.OrangeRed);
                ErrorTextBlock.Text = error;
                ErrorBorder.Visibility = Visibility.Visible;
            }
        }

        private bool TryBuild(out DateTime result, out string error)
        {
            error = "";
            try
            {
                result = new DateTime(
                    (int)YearBox.Value,
                    (int)MonthBox.Value,
                    (int)DayBox.Value,
                    (int)HourBox.Value,
                    (int)MinuteBox.Value,
                    (int)SecondBox.Value
                );
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                result = default;
                error = "数值超出有效范围，请检查各字段。";
                return false;
            }
            catch (Exception ex)
            {
                result = default;
                error = "时间设置错误: " + ex.Message;
                return false;
            }
        }

        private void OnApplyClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // 取消关闭并保留对话框，让用户在内联错误提示中修正，避免嵌套 ContentDialog
            if (!TryBuild(out var time, out var error))
            {
                args.Cancel = true;
                ErrorTextBlock.Text = error;
                ErrorBorder.Visibility = Visibility.Visible;
                IsValid = false;
                return;
            }

            SelectedTime = time;
            IsValid = true;
        }

        private void SetCurrentTimeButton_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            YearBox.Value = now.Year;
            MonthBox.Value = now.Month;
            DayBox.Value = now.Day;
            HourBox.Value = now.Hour;
            MinuteBox.Value = now.Minute;
            SecondBox.Value = now.Second;
            UpdatePreview();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            YearBox.Value = _originalTime.Year;
            MonthBox.Value = _originalTime.Month;
            DayBox.Value = _originalTime.Day;
            HourBox.Value = _originalTime.Hour;
            MinuteBox.Value = _originalTime.Minute;
            SecondBox.Value = _originalTime.Second;
            UpdatePreview();
        }
    }
}
