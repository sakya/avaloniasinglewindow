using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SingleWindow.Pages;
using System;
using Avalonia.SingleWindow;

namespace SingleWindow
{
    public partial class MainWindow : MainWindowBase
    {
        private Controls.TitleBar _titleBar;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                ExtendClientAreaToDecorationsHint = true;
                ExtendClientAreaTitleBarHeightHint = -1;
                WindowDecorations = WindowDecorations.BorderOnly;
                Background = new SolidColorBrush(Colors.Transparent);
            }

            WindowTitle = "SingleWindow";

            Container = this.FindControl<Grid>("ContainerGrid");
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
