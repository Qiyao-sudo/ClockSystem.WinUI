using ClockSystem.WinUI.Services;
using ClockSystem.WinUI.ViewModels;
using ClockSystem.WinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace ClockSystem.WinUI
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

        /// <summary>全局配置服务单例（与窗口同生命周期）。</summary>
        public static ConfigService Config { get; } = new ConfigService();

        /// <summary>全局时钟 ViewModel 单例。</summary>
        public static MainViewModel Clock { get; private set; } = null!;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var window = new Window
            {
                Title = "子母钟管理系统"
            };
            MainWindow = window;

            // Mica 系统背景（WinAppSDK 1.3+）
            TryEnableMica(window);

            if (window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                window.Content = rootFrame;
            }

            // 创建 VM 单例并应用已保存的主题
            Clock = new MainViewModel(Config);
            ApplyTheme(LoadTheme());

            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
            window.Activate();

            ConfigureWindowSize(window, logicalWidth: 1280, logicalHeight: 860);
        }

        private static void TryEnableMica(Window window)
        {
            try
            {
                window.SystemBackdrop = new MicaBackdrop
                {
                    Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt
                };
            }
            catch
            {
                // 不支持时退化为默认背景
            }
        }

        /// <summary>把主题应用到主窗口根元素，并持久化到配置。</summary>
        public static void ApplyTheme(ElementTheme theme)
        {
            try
            {
                if (MainWindow?.Content is FrameworkElement root)
                {
                    root.RequestedTheme = theme;
                }

                var config = Config.LoadConfig();
                config.Appearance.Theme = ThemeToName(theme);
                Config.SaveConfig(config);
            }
            catch
            {
            }
        }

        public static ElementTheme LoadTheme() => NameToTheme(Config.LoadConfig().Appearance?.Theme);

        public static ElementTheme CurrentTheme
            => (MainWindow?.Content is FrameworkElement root) ? root.RequestedTheme : ElementTheme.Default;

        public static string ThemeToName(ElementTheme theme) => theme switch
        {
            ElementTheme.Light => "Light",
            ElementTheme.Dark => "Dark",
            _ => "System"
        };

        public static ElementTheme NameToTheme(string? name) => name switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };

        private static void ConfigureWindowSize(Window window, int logicalWidth, int logicalHeight)
        {
            try
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                var scale = window.Content?.XamlRoot?.RasterizationScale ?? 1.0;
                var w = (int)(logicalWidth * scale);
                var h = (int)(logicalHeight * scale);
                appWindow.Resize(new global::Windows.Graphics.SizeInt32 { Width = w, Height = h });
            }
            catch
            {
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
