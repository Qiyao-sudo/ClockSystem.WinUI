using ClockSystem.WinUI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace ClockSystem.WinUI.Views
{
    public sealed partial class SettingsPage : Page
    {
        public ConfigModel Config { get; private set; }

        public SettingsPage()
        {
            this.InitializeComponent();
            Config = App.Config.LoadConfig();
            DataContext = this;

            PopulatePathItems();
            PopulateSwitchItems();
            SyncThemeRadio();
        }

        protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // 离开设置页时自动保存，避免用户忘记点保存
            Save();
            base.OnNavigatedFrom(e);
        }

        private void PopulatePathItems()
        {
            HourPathsItemsControl.Items.Clear();
            foreach (var item in Config.Path.HourPaths)
            {
                HourPathsItemsControl.Items.Add(MakePathRow(item.Key, "点", item.Value, v => Config.Path.SetHourPath(item.Key, v)));
            }

            HalfPathsItemsControl.Items.Clear();
            foreach (var item in Config.Path.HalfPaths)
            {
                HalfPathsItemsControl.Items.Add(MakePathRow(item.Key, "点半", item.Value, v => Config.Path.SetHalfPath(item.Key, v)));
            }
        }

        private static Panel MakePathRow(int key, string suffix, string value, System.Action<string> onChanged)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2), Spacing = 5 };
            panel.Children.Add(new TextBlock { Text = key.ToString(), Width = 40, VerticalAlignment = VerticalAlignment.Center });
            panel.Children.Add(new TextBlock { Text = suffix + ":", VerticalAlignment = VerticalAlignment.Center });
            var textBox = new TextBox { Text = value, Width = 200 };
            textBox.TextChanged += (s, _) => onChanged(((TextBox)s).Text);
            panel.Children.Add(textBox);
            return panel;
        }

        private void PopulateSwitchItems()
        {
            HourSwitchesItemsControl.Items.Clear();
            foreach (var item in Config.Switch.HourSwitches)
            {
                HourSwitchesItemsControl.Items.Add(MakeSwitchRow(item.Key, "点", item.Value, v => Config.Switch.SetHour(item.Key, v)));
            }

            HalfSwitchesItemsControl.Items.Clear();
            foreach (var item in Config.Switch.HalfSwitches)
            {
                HalfSwitchesItemsControl.Items.Add(MakeSwitchRow(item.Key, "点半", item.Value, v => Config.Switch.SetHalf(item.Key, v)));
            }
        }

        private static CheckBox MakeSwitchRow(int key, string suffix, bool isChecked, System.Action<bool> onChanged)
        {
            var checkBox = new CheckBox { Content = $"{key}{suffix}", IsChecked = isChecked, Margin = new Thickness(5) };
            checkBox.Checked += (s, e) => onChanged(true);
            checkBox.Unchecked += (s, e) => onChanged(false);
            return checkBox;
        }

        private async void BrowseMusicButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickAudioFileAsync();
            if (file != null) Config.Path.MusicPath = file.Path;
        }

        private async void BrowseBellButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickAudioFileAsync();
            if (file != null) Config.Path.BellPath = file.Path;
        }

        private static async Task<Windows.Storage.StorageFile?> PickAudioFileAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".ogg");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow!);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            return await picker.PickSingleFileAsync();
        }

        private void SelectAllHourButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HourSwitches.Count; i++)
                Config.Switch.SetHour(Config.Switch.HourSwitches[i].Key, true);
            PopulateSwitchItems();
        }

        private void DeselectAllHourButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HourSwitches.Count; i++)
                Config.Switch.SetHour(Config.Switch.HourSwitches[i].Key, false);
            PopulateSwitchItems();
        }

        private void SelectAllHalfButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HalfSwitches.Count; i++)
                Config.Switch.SetHalf(Config.Switch.HalfSwitches[i].Key, true);
            PopulateSwitchItems();
        }

        private void DeselectAllHalfButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Config.Switch.HalfSwitches.Count; i++)
                Config.Switch.SetHalf(Config.Switch.HalfSwitches[i].Key, false);
            PopulateSwitchItems();
        }

        // ---------- 主题 ----------

        private void SyncThemeRadio()
        {
            var t = App.CurrentTheme;
            ThemeSystemRadio.IsChecked = t == ElementTheme.Default;
            ThemeLightRadio.IsChecked = t == ElementTheme.Light;
            ThemeDarkRadio.IsChecked = t == ElementTheme.Dark;
        }

        private void OnThemeRadio(object sender, RoutedEventArgs e)
        {
            if (sender == ThemeSystemRadio) App.ApplyTheme(ElementTheme.Default);
            else if (sender == ThemeLightRadio) App.ApplyTheme(ElementTheme.Light);
            else if (sender == ThemeDarkRadio) App.ApplyTheme(ElementTheme.Dark);
        }

        // ---------- 保存 ----------

        private void OnSaveClicked(object sender, RoutedEventArgs e) => Save();

        private void Save()
        {
            // 写回当前主题，确保与界面一致
            Config.Appearance.Theme = App.ThemeToName(App.CurrentTheme);
            App.Config.SaveConfig(Config);
        }
    }
}
