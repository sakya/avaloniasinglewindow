using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.SingleWindow.Abstracts;

namespace SingleWindow.Pages
{
    public class Page1 : BasePage
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            PageTitle = "Page 1";
        }

        private async void OnGoPage2Click(object sender, RoutedEventArgs e)
        {
            await NavigateTo(new Page2());
        }

        private async void OnGoBackClick(object sender, RoutedEventArgs e)
        {
            await NavigateBack();
        }
    }
}
