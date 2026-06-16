using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClockSystem.WinUI.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 默认选中主页（触发导航）
            RootNav.SelectedItem = HomeItem;
            UpdateThemeMenu();
        }

        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is not NavigationViewItem item) return;

            var tag = item.Tag as string;
            switch (tag)
            {
                case "Home":
                    ContentFrame.Navigate(typeof(ClockPage));
                    break;
                case "Settings":
                    ContentFrame.Navigate(typeof(SettingsPage));
                    break;
                case "About":
                    ContentFrame.Navigate(typeof(AboutPage));
                    break;
            }
        }

        // ---------- 主题切换 ----------

        private void OnThemeClicked(object sender, RoutedEventArgs e)
        {
            ThemeButton.Flyout.ShowAt(ThemeButton);
        }

        private void OnThemeSystem(object sender, RoutedEventArgs e) => Apply(ElementTheme.Default);
        private void OnThemeLight(object sender, RoutedEventArgs e) => Apply(ElementTheme.Light);
        private void OnThemeDark(object sender, RoutedEventArgs e) => Apply(ElementTheme.Dark);

        private void Apply(ElementTheme theme)
        {
            App.ApplyTheme(theme);
            if (App.MainWindow?.Content is FrameworkElement root)
            {
                root.RequestedTheme = theme;
            }
            UpdateThemeMenu();
        }

        /// <summary>根据当前主题刷新菜单勾选与图标。</summary>
        public void UpdateThemeMenu()
        {
            var current = App.CurrentTheme;
            ThemeSystem.IsChecked = current == ElementTheme.Default;
            ThemeLight.IsChecked = current == ElementTheme.Light;
            ThemeDark.IsChecked = current == ElementTheme.Dark;
            ThemeIcon.Glyph = current switch
            {
                ElementTheme.Light => "\xE706", // 太阳
                ElementTheme.Dark => "\xE708",  // 月亮
                _ => "\xE793"                    // 跟随
            };
            ThemeLabel.Text = current switch
            {
                ElementTheme.Light => "亮色",
                ElementTheme.Dark => "暗色",
                _ => "跟随系统"
            };
        }
    }
}
