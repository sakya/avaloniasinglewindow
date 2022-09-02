using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SingleWindow.Abstracts;

namespace SingleWindow.Pages
{
    public class Page2 : BasePage
    {
        public Page2()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            PageTitle = "Page 2";
            NavigateBackWithKeyboard = true;
        }

        private async void OnGoBackClick(object sender, RoutedEventArgs e)
        {
            await NavigateBack();
        }
    }
}
