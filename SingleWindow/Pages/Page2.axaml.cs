using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.SingleWindow.Abstracts;

namespace SingleWindow.Pages
{
    public partial class Page2 : BasePage
    {
        public Page2()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            PageTitle = "Page 2";
        }

        private async void OnGoBackClick(object sender, RoutedEventArgs e)
        {
            await NavigateBack();
        }
    }
}
