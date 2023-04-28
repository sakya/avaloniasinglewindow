using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.SingleWindow;
using Avalonia.SingleWindow.Abstracts;
using SingleWindow.Dialogs;

namespace SingleWindow.Pages
{
    public class WelcomePage : BasePage
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            PageTitle = "Welcome";
        }

        private async void OnPage1Click(object sender, RoutedEventArgs e)
        {
            await NavigateTo(new Page1());
        }

        private async void OnPage2Click(object sender, RoutedEventArgs e)
        {
            await NavigateTo(new Page2());
        }

        private async void OnOpenDialogClick(object sender, RoutedEventArgs e)
        {
            var dlg = new MessageDialog();
            await dlg.Show();
        }

        private void OnAnimationChanged(object sender, SelectionChangedEventArgs args)
        {
            if (sender is ComboBox cmb)
                this.MainWindow.Transition.Type = (MainWindowBase.TransitionSettings.EnterTransitions)cmb.SelectedIndex;
        }

        private void OnDurationChanged(object sender, NumericUpDownValueChangedEventArgs args) {
            this.MainWindow.Transition.Duration = TimeSpan.FromMilliseconds(args.NewValue);
        }

        private void OnEasingChanged(object sender, SelectionChangedEventArgs args)
        {
            if (sender is ComboBox cmb)
                this.MainWindow.Transition.Easing = cmb.SelectedIndex switch
                {
                    0 => new Avalonia.Animation.Easings.LinearEasing(),
                    1 => new Avalonia.Animation.Easings.CubicEaseIn(),
                    2 => new Avalonia.Animation.Easings.CubicEaseOut(),
                    3 => new Avalonia.Animation.Easings.CubicEaseInOut(),
                    _ => this.MainWindow.Transition.Easing
                };
        }
    }
}
