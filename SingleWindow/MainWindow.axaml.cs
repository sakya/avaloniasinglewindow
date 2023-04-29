using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SingleWindow.Pages;
using System;
using Avalonia.SingleWindow;

namespace SingleWindow
{
    public class MainWindow : MainWindowBase
    {
        private Controls.TitleBar _titleBar;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                ExtendClientAreaToDecorationsHint = true;
                ExtendClientAreaTitleBarHeightHint = -1;
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
                Background = new SolidColorBrush(Colors.Transparent);
            }

            WindowTitle = "SingleWindow";

            Container = this.FindControl<Panel>("Container");
            _titleBar = this.FindControl<Controls.TitleBar>("TitleBar");
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            await NavigateTo(new WelcomePage());
        }

        protected override void PageChanged()
        {
            _titleBar.CanGoBack = CanNavigateBack;
        }
    }
}
