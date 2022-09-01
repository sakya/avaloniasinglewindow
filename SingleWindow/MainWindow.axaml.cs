using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using SingleWindow.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SingleWindow
{
    public class MainWindow : Window
    {
        private Grid _container;
        private Controls.TitleBar _titleBar;
        readonly List<BasePage> _pageHistory = new();
        readonly Dictionary<string, BasePage.PageState> _pageStates = new();
        private bool _changingPage;

        #region classes
        public class TransitionSettings
        {
            public enum EnterTransitions
            {
                None,
                SlideLeft,
                SlideUp,
                FadeIn
            }

            public TransitionSettings(EnterTransitions transition, TimeSpan duration, Avalonia.Animation.Easings.Easing easing = null) {
                Type = transition;
                Duration = duration;
                Easing = easing ?? new Avalonia.Animation.Easings.LinearEasing();
            }

            public EnterTransitions Type { get; set; }
            public TimeSpan Duration { get; set; }
            public Avalonia.Animation.Easings.Easing Easing { get; set; }
        } // TransitionSettings
        #endregion

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
                this.ExtendClientAreaToDecorationsHint = true;
                this.ExtendClientAreaTitleBarHeightHint = -1;
                this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                this.TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
                this.Background = new SolidColorBrush(Colors.Transparent);
            }

            WindowTitle = "SingleWindow";
            Transition = new TransitionSettings(TransitionSettings.EnterTransitions.SlideLeft, TimeSpan.FromMilliseconds(250));
            BackKey = Key.Escape;

            _container = this.FindControl<Grid>("Container");
            _titleBar = this.FindControl<Controls.TitleBar>("TitleBar");

            Closing += OnWindowClosing;
            KeyDown += OnKeyDown;
        }

        #region public properties
        public TransitionSettings Transition { get; set; }
        public Key BackKey { get; set; }

        public string WindowTitle { get; set; }

        public BasePage CurrentPage
        {
            get
            {
                if (_container.Children.Count > 0)
                    return _container.Children[0] as BasePage;
                return null;
            }
        }

        public bool CanNavigateBack
        {
            get {
                return _pageHistory.Count > 0;
            }
        }
        #endregion

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            await NavigateTo(new WelcomePage());
        }

        #region public operations
        /// <summary>
        /// Navigate to a new page
        /// </summary>
        /// <param name="page"></param>
        /// <returns>True on success</returns>
        public async Task<bool> NavigateTo(BasePage page)
        {
            if (_changingPage)
                return false;

            var exitingPage = CurrentPage;
            if (exitingPage != null) {
                if (!await exitingPage.OnNavigatingFrom(BasePage.NavigationDirection.Forward))
                    return false;
                _pageHistory.Add(exitingPage);
            }

            await ChangePage(exitingPage, page, false);
            page.OnNavigatedTo(BasePage.NavigationDirection.Forward);

            return true;
        } // NavigateTo

        /// <summary>
        /// Navigate to the previous page
        /// </summary>
        /// <returns>True on success</returns>
        public async Task<bool> NavigateBack()
        {
            if (_changingPage)
                return false;

            if (_pageHistory.Count > 0) {
                var exitingPage = CurrentPage;
                if (exitingPage != null && await exitingPage.OnNavigatingFrom(BasePage.NavigationDirection.Backward) == false)
                    return false;

                var enteringPage = _pageHistory.Last();
                _pageHistory.Remove(enteringPage);

                await ChangePage(exitingPage, enteringPage, true);
                exitingPage?.Dispose();
                RemovePageState(exitingPage);

                enteringPage.OnNavigatedTo(BasePage.NavigationDirection.Backward);
                return true;
            }
            return false;
        } // NavigateBack

        public void SavePageState(BasePage.PageState state) {
            _pageStates[state.PageId] = state;
        } // SavePageState

        public BasePage.PageState LoadPageState<T>(BasePage page) where T : BasePage.PageState
        {
            BasePage.PageState res;
            if (_pageStates.TryGetValue(page.Id, out res))
                return res as T;
            return null;
        } // LoadPageState

        public void RemovePageState(BasePage page) {
            if (page != null)
                _pageStates.Remove(page.Id);
        } // RemovePageState
        #endregion

        private async Task ChangePage(BasePage exiting, BasePage entering, bool back)
        {
            _changingPage = true;
            entering.Opacity = 0;
            _container.Children.Add(entering);
            if (exiting == null) {
                entering.Opacity = 1;
                _changingPage = false;
                return;
            }

            if (Transition == null || Transition.Type == TransitionSettings.EnterTransitions.None) {
                _container.Children.Remove(exiting);
                entering.Opacity = 1;
                _changingPage = false;
                return;
            }

            exiting.IsHitTestVisible = false;
            entering.IsHitTestVisible = false;

            AvaloniaProperty property = null;
            double from = 0.0;
            double to = 0.0;
            switch (Transition.Type) {
                case TransitionSettings.EnterTransitions.SlideLeft:
                    property = TranslateTransform.XProperty;
                    from = 0.0;
                    to = this.Bounds.Size.Width;

                    exiting.RenderTransform = new TranslateTransform();
                    entering.RenderTransform = new TranslateTransform() {
                        X = to
                    };
                    break;
                case TransitionSettings.EnterTransitions.SlideUp:
                    property = TranslateTransform.YProperty;
                    from = 0.0;
                    to = this.Bounds.Size.Height;

                    exiting.RenderTransform = new TranslateTransform();
                    entering.RenderTransform = new TranslateTransform() {
                        Y = to
                    };
                    break;
                case TransitionSettings.EnterTransitions.FadeIn:
                    property = UserControl.OpacityProperty;
                    from = 1.0;
                    to = 0.0;
                    break;
            }

            // Exiting
            Animation exitAnim = new Animation()
            {
                Duration = Transition.Duration,
                Easing = Transition.Easing
            };

            var kf = new KeyFrame()
            {
                Cue = new Cue(0.0)
            };
            kf.Setters.Add(new Setter()
            {
                Property = property,
                Value = from
            });
            exitAnim.Children.Add(kf);

            kf = new KeyFrame()
            {
                Cue = new Cue(1.0)
            };
            kf.Setters.Add(new Setter()
            {
                Property = property,
                Value = back ? to : -to
            });
            exitAnim.Children.Add(kf);

            // Entering
            Animation enterAnim = new Animation()
            {
                Duration = Transition.Duration,
                Easing = Transition.Easing
            };

            kf = new KeyFrame()
            {
                Cue = new Cue(0.0)
            };
            kf.Setters.Add(new Setter()
            {
                Property = property,
                Value = back ? -to : to
            });
            enterAnim.Children.Add(kf);

            kf = new KeyFrame()
            {
                Cue = new Cue(1.0)
            };
            kf.Setters.Add(new Setter()
            {
                Property = property,
                Value = from
            });
            enterAnim.Children.Add(kf);

            if (Transition.Type == TransitionSettings.EnterTransitions.FadeIn) {
                await exitAnim.RunAsync(exiting, null);
                exiting.Opacity = 0;
                await enterAnim.RunAsync(entering, null);
                entering.Opacity = 1.0;
            } else {
                List<Task> tasks = new List<Task>();
                tasks.Add(exitAnim.RunAsync(exiting, null));
                tasks.Add(enterAnim.RunAsync(entering, null));
                entering.Opacity = 1;
                await Task.WhenAll(tasks);
            }

            entering.RenderTransform = null;
            exiting.RenderTransform = null;

            entering.IsHitTestVisible = true;
            _container.Children.Remove(exiting);

            _titleBar.CanGoBack = CanNavigateBack;
            _changingPage = false;
        } // ChangePage

        private async void OnWindowClosing(object sender, CancelEventArgs args)
        {
            if (_changingPage) {
                args.Cancel = true;
                return;
            }

            if (CurrentPage?.NavigateBackOnWindowClose == true && CanNavigateBack) {
                args.Cancel = true;
                await NavigateBack();
            }
        } // OnWindowClosing

        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_changingPage)
                return;

            if (CurrentPage != null && CurrentPage.NavigateBackWithKeyboard && e.KeyModifiers == KeyModifiers.None && e.Key == BackKey) {
                e.Handled = true;
                await NavigateBack();
            }
        } // OnKeyDown
    }
}
