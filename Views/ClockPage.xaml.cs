using ClockSystem.WinUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace ClockSystem.WinUI.Views
{
    public sealed partial class ClockPage : Page
    {
        private MainViewModel ViewModel => App.Clock;

        private readonly DispatcherTimer _clockTimer;
        private TimeSpan _lastTickAt;

        // 表盘几何
        private double _centerX, _centerY, _size;
        private bool _dialBuilt;

        // 指针（持久化，每帧仅更新旋转角度）
        private Line? _hourHand, _minuteHand, _secondHand;
        private RotateTransform? _hourRotate, _minuteRotate, _secondRotate;

        private bool _lastNight;
        private bool _nightTracked;

        public ClockPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;

            ViewModel.LogMessageReceived += OnLogMessageReceived;

            this.Loaded += ClockPage_Loaded;
            this.Unloaded += ClockPage_Unloaded;

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _clockTimer.Tick += OnClockTick;
        }

        private void ClockPage_Loaded(object sender, RoutedEventArgs e)
        {
            ClockCanvas.SizeChanged += OnCanvasSizeChanged;
            BuildDial();
            _lastTickAt = TimeSpan.FromMilliseconds(Environment.TickCount64);
            _clockTimer.Start();
        }

        private void ClockPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _clockTimer.Stop();
            ViewModel.LogMessageReceived -= OnLogMessageReceived;
        }

        private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e) => BuildDial();

        private void OnLogMessageReceived(string message)
        {
            LogTextBlock.Text = ViewModel.LogText;
            LogScrollViewer?.ChangeView(0, LogScrollViewer.ScrollableHeight, 1);
        }

        private void OnClockTick(object? sender, object e)
        {
            var nowTicks = TimeSpan.FromMilliseconds(Environment.TickCount64);
            var elapsed = nowTicks - _lastTickAt;
            _lastTickAt = nowTicks;

            ViewModel.Tick(elapsed);
            UpdateHands();
        }

        private void BuildDial()
        {
            if (ClockCanvas == null) return;

            var width = ClockCanvas.ActualWidth;
            var height = ClockCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            ClockCanvas.Children.Clear();

            _size = Math.Min(width, height) - 40;
            _centerX = width / 2;
            _centerY = height / 2;

            var isNight = ViewModel.IsLightOn;
            var dialFill = isNight ? Microsoft.UI.Colors.Black : Microsoft.UI.Colors.White;
            var numeralColor = new SolidColorBrush(isNight ? Microsoft.UI.Colors.LimeGreen : Microsoft.UI.Colors.DarkSlateGray);
            var handColor = numeralColor;
            var secHandColor = new SolidColorBrush(isNight ? Microsoft.UI.Colors.Red : Microsoft.UI.Colors.DarkRed);

            var dial = new Ellipse
            {
                Width = _size, Height = _size,
                Fill = new SolidColorBrush(dialFill),
                Stroke = new SolidColorBrush(dialFill),
                StrokeThickness = 5
            };
            Canvas.SetLeft(dial, _centerX - _size / 2);
            Canvas.SetTop(dial, _centerY - _size / 2);
            ClockCanvas.Children.Add(dial);

            for (int i = 0; i < 12; i++)
            {
                var angle = Math.PI / 6 * i;
                var length = _size / 2;
                var tickLength = _size / 20;

                var x1 = _centerX + Math.Sin(angle) * (length - tickLength);
                var y1 = _centerY - Math.Cos(angle) * (length - tickLength);
                var x2 = _centerX + Math.Sin(angle) * length;
                var y2 = _centerY - Math.Cos(angle) * length;

                ClockCanvas.Children.Add(new Line
                {
                    X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                    Stroke = numeralColor, StrokeThickness = 3
                });

                var num = i == 0 ? 12 : i;
                var numX = _centerX + Math.Sin(angle) * (length - tickLength * 2);
                var numY = _centerY - Math.Cos(angle) * (length - tickLength * 2);

                var text = new TextBlock
                {
                    Text = num.ToString(),
                    FontSize = _size / 20,
                    Foreground = numeralColor,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Width = _size / 10,
                    Height = _size / 15
                };
                Canvas.SetLeft(text, numX - _size / 20);
                Canvas.SetTop(text, numY - _size / 30);
                ClockCanvas.Children.Add(text);
            }

            for (int i = 0; i < 60; i++)
            {
                if (i % 5 == 0) continue;
                var angle = Math.PI / 30 * i;
                var length = _size / 2;
                var tickLength = _size / 40;

                var x1 = _centerX + Math.Sin(angle) * (length - tickLength);
                var y1 = _centerY - Math.Cos(angle) * (length - tickLength);
                var x2 = _centerX + Math.Sin(angle) * length;
                var y2 = _centerY - Math.Cos(angle) * length;

                ClockCanvas.Children.Add(new Line
                {
                    X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                    Stroke = numeralColor, StrokeThickness = 1
                });
            }

            _hourHand = MakeHand(_size / 3, handColor, _size / 30, out _hourRotate);
            _minuteHand = MakeHand(_size / 2 - 10, handColor, _size / 50, out _minuteRotate);
            _secondHand = MakeHand(_size / 2 - 5, secHandColor, _size / 100, out _secondRotate);

            ClockCanvas.Children.Add(_hourHand);
            ClockCanvas.Children.Add(_minuteHand);
            ClockCanvas.Children.Add(_secondHand);

            var centerDot = new Ellipse
            {
                Width = _size / 50, Height = _size / 50,
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            Canvas.SetLeft(centerDot, _centerX - _size / 100);
            Canvas.SetTop(centerDot, _centerY - _size / 100);
            ClockCanvas.Children.Add(centerDot);

            _dialBuilt = true;
        }

        private Line MakeHand(double length, Brush stroke, double thickness, out RotateTransform rotate)
        {
            rotate = new RotateTransform { Angle = 0, CenterX = 0, CenterY = 0 };
            var line = new Line
            {
                X1 = 0, Y1 = 0, X2 = 0, Y2 = -length,
                Stroke = stroke, StrokeThickness = thickness,
                RenderTransform = rotate
            };
            Canvas.SetLeft(line, _centerX);
            Canvas.SetTop(line, _centerY);
            return line;
        }

        private void UpdateHands()
        {
            if (!_dialBuilt || _hourRotate == null) return;

            if (IsNightChanged()) BuildDial();

            var now = ViewModel.HighPrecisionTime;
            var hours = now.Hour % 12;
            var minutes = now.Minute;
            var seconds = now.Second + now.Millisecond / 1000.0;

            _hourRotate.Angle = 30.0 * (hours + minutes / 60.0 + seconds / 3600.0);
            _minuteRotate!.Angle = 6.0 * (minutes + seconds / 60.0);
            _secondRotate!.Angle = 6.0 * seconds;
        }

        private bool IsNightChanged()
        {
            var night = ViewModel.IsLightOn;
            if (!_nightTracked)
            {
                _lastNight = night;
                _nightTracked = true;
                return false;
            }
            if (night != _lastNight)
            {
                _lastNight = night;
                return true;
            }
            return false;
        }

        private void OnSyncTimeClicked(object sender, RoutedEventArgs e) => ViewModel.SyncSystemTime();

        private async void OnAdjustTimeClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.TimeAdjustDialog(ViewModel.MasterTime)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.IsValid)
            {
                ViewModel.AdjustTime(dialog.SelectedTime);
            }
        }
    }
}
