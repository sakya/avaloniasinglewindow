using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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

        private void OnAnimationChanged(object sender, SelectionChangedEventArgs args)
        {
            var cmb = sender as ComboBox;
            this.MainWindow.Transition.Type = (MainWindow.TransitionSettings.EnterTransitions)cmb.SelectedIndex;
        }

        private void OnDurationChanged(object sender, NumericUpDownValueChangedEventArgs args) {
            this.MainWindow.Transition.Duration = TimeSpan.FromMilliseconds(args.NewValue);
        }

        private void OnEasingChanged(object sender, SelectionChangedEventArgs args)
        {
            var cmb = sender as ComboBox;
            switch(cmb.SelectedIndex) {
                case 0:
                    this.MainWindow.Transition.Easing = new Avalonia.Animation.Easings.LinearEasing();
                    break;
                case 1:
                    this.MainWindow.Transition.Easing = new Avalonia.Animation.Easings.CubicEaseIn();
                    break;
                case 2:
                    this.MainWindow.Transition.Easing = new Avalonia.Animation.Easings.CubicEaseOut();
                    break;
                case 3:
                    this.MainWindow.Transition.Easing = new Avalonia.Animation.Easings.CubicEaseInOut();
                    break;                    
            }
        }        
    }
}
