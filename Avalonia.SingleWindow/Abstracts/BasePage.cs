using Avalonia.Controls;

namespace Avalonia.SingleWindow.Abstracts
{
    public class BasePage : UserControl, IDisposable
    {
        public enum NavigationDirection
        {
            Forward,
            Backward
        }

        public abstract class PageState
        {
            protected PageState(BasePage page) {
                PageId = page.Id;
            }

            public string PageId {get; private set; }
        } // PageState

        protected BasePage()
        {
            Id = Guid.NewGuid().ToString("N");
            Margin = new Thickness(15);

            NavigateBackWithKeyboard = true;
            NavigateBackOnWindowClose = true;
        }

        protected MainWindowBase MainWindow => MainWindowBase.Instance;

        /// <summary>
        /// The unique Id of the page
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Defines if the page should navigate back when <see cref="MainWindowBase.BackKey"/> is pressed (default: true)
        /// </summary>
        public bool NavigateBackWithKeyboard { get; set; }
        /// <summary>
        /// Defines if the page should navigate back when the close window button is clicked (default: true)
        /// </summary>
        public bool NavigateBackOnWindowClose { get; set;}
        /// <summary>
        /// The page title. Page title is postponed to the <see cref="MainWindowBase.WindowTitle"/>
        /// </summary>
        public string PageTitle { get; set; }

        public bool CanNavigateBack => MainWindow.CanNavigateBack;

        /// <summary>
        /// Called before navigating to a new page
        /// </summary>
        /// <param name="direction">The navigation direction</param>
        /// <returns>True to allow the navigation, false to deny it</returns>
        public virtual Task<bool> OnNavigatingFrom(NavigationDirection direction)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Called after the navigation to this page
        /// </summary>
        /// <param name="direction"></param>
        public virtual void OnNavigatedTo(NavigationDirection direction)
        {
            if (!string.IsNullOrEmpty(PageTitle)) {
                MainWindow.Title = !string.IsNullOrEmpty(MainWindow.WindowTitle) ? $"{MainWindow.WindowTitle} - {PageTitle}" : PageTitle;
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

        public void SaveState(PageState state) {
            MainWindow.SavePageState(state);
        } // SaveState

        public PageState LoadState<T>() where T: PageState
        {
            return MainWindow.LoadPageState<T>(this);
        } // LoadState
    }
}
