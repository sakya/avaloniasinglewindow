using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleWindow.Pages
{
    public class BasePage : UserControl, IDisposable
    {
        public enum NavigationDirection
        {
            Forward,
            Backward
        }

        public BasePage()
        {
            Margin = new Thickness(15);

            NavigateBackWithKeyboard = true;
            NavigateBackOnWindowClose = true;
        }

        protected MainWindow MainWindow
        {
            get { return (Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime).MainWindow as MainWindow; }
        }

        public bool NavigateBackWithKeyboard { get; set; }
        public bool NavigateBackOnWindowClose { get; set;}
        public string PageTitle { get; set; }

        public bool CanNavigateBack
        {
            get { return MainWindow.CanNavigateBack; }
        }

        public virtual bool OnNavigatingFrom(NavigationDirection direction)
        {
            return true;
        }

        public virtual void OnNavigatedTo(NavigationDirection direction)
        {
            if (!string.IsNullOrEmpty(PageTitle)) {
                if (!string.IsNullOrEmpty(MainWindow.WindowTitle))
                    MainWindow.Title = $"{MainWindow.WindowTitle} - {PageTitle}";
                else
                    MainWindow.Title = PageTitle;
            } else {
                MainWindow.Title = MainWindow.WindowTitle;
            }
        }

        public virtual void Dispose()
        {

        }

        public async Task<bool> NavigateTo(BasePage page)
        {
            return await MainWindow.NavigateTo(page);
        }

        public async Task<bool> NavigateBack()
        {
            return await MainWindow.NavigateBack();
        }
    }
}
