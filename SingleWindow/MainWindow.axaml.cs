using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
        Grid m_Container = null;
        Controls.TitleBar m_TitleBar = null;
        List<BasePage> m_PageHistory = new List<BasePage>();
        Dictionary<string, BasePage.PageState> m_PageStates = new Dictionary<string, BasePage.PageState>();
        bool m_ChangingPage = false;

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
                var size = new Size(this.Width, this.Height);
                var sizeToContent = this.SizeToContent;

                this.ExtendClientAreaToDecorationsHint = true;
                this.ExtendClientAreaTitleBarHeightHint = -1;
                this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                this.TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
                this.Background = new SolidColorBrush(Colors.Transparent);

                this.Width = size.Width;
                this.Height = size.Height;
                this.SizeToContent = sizeToContent;
            }

            WindowTitle = "SingleWindow";
            Transition = new TransitionSettings(TransitionSettings.EnterTransitions.SlideLeft, TimeSpan.FromMilliseconds(250));
            BackKey = Key.Escape;

            m_Container = this.FindControl<Grid>("m_Container");
            m_TitleBar = this.FindControl<Controls.TitleBar>("m_TitleBar");

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
                if (m_Container.Children.Count > 0)
                    return m_Container.Children[0] as BasePage;
                return null;
            }
        }

        public bool CanNavigateBack
        {
            get {
                return m_PageHistory.Count > 0;
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
            if (m_ChangingPage)
                return false;

            var exitingPage = CurrentPage;
            if (exitingPage != null) {
                if (!exitingPage.OnNavigatingFrom(BasePage.NavigationDirection.Forward))
                    return false;
                m_PageHistory.Add(exitingPage);
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
            if (m_ChangingPage)
                return false;

            if (m_PageHistory.Count > 0) {
                var exitingPage = CurrentPage;
                if (exitingPage?.OnNavigatingFrom(BasePage.NavigationDirection.Backward) == false)
                    return false;

                var enteringPage = m_PageHistory.Last();
                m_PageHistory.Remove(enteringPage);
                
                await ChangePage(exitingPage, enteringPage, true);
                exitingPage?.Dispose();
                RemovePageState(exitingPage);

                enteringPage.OnNavigatedTo(BasePage.NavigationDirection.Backward);                
                return true;
            }
            return false;
        } // NavigateBack

        public void SavePageState(BasePage.PageState state) {
            m_PageStates[state.PageId] = state;
        } // SavePageState

        public BasePage.PageState LoadPageState<T>(BasePage page) where T : BasePage.PageState
        {
            BasePage.PageState res = null;
            if (m_PageStates.TryGetValue(page.Id, out res))
                return res as T;
            return null;
        } // LoadPageState

        public void RemovePageState(BasePage page) {
            if (page != null)
                m_PageStates.Remove(page.Id);
        } // RemovePageState
        #endregion

        private async Task<bool> ChangePage(BasePage exiting, BasePage entering, bool back)
        {
            m_ChangingPage = true;
            entering.Opacity = 0;
            m_Container.Children.Add(entering);
            if (exiting == null) {
                entering.Opacity = 1;
                m_ChangingPage = false;
                return true;
            }

            if (Transition == null || Transition.Type == TransitionSettings.EnterTransitions.None) {
                m_Container.Children.Remove(exiting);
                entering.Opacity = 1;
                m_ChangingPage = false;
                return true;
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
                await exitAnim.RunAsync(exiting);
                exiting.Opacity = 0;
                await enterAnim.RunAsync(entering);
                entering.Opacity = 1.0;
            } else {
                List<Task> tasks = new List<Task>();
                tasks.Add(exitAnim.RunAsync(exiting));
                tasks.Add(enterAnim.RunAsync(entering));
                entering.Opacity = 1;
                await Task.WhenAll(tasks);
            }

            entering.RenderTransform = null;
            exiting.RenderTransform = null;

            entering.IsHitTestVisible = true;
            m_Container.Children.Remove(exiting);

            m_TitleBar.CanGoBack = CanNavigateBack;
            m_ChangingPage = false;
            return true;
        } // ChangePage

        private async void OnWindowClosing(object sender, CancelEventArgs args)
        {
            if (m_ChangingPage) {
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

            if (m_ChangingPage)
                return;

            if (CurrentPage != null && CurrentPage.NavigateBackWithKeyboard && e.KeyModifiers == KeyModifiers.None && e.Key == BackKey) {
                e.Handled = true;
                await NavigateBack();
            }
        } // OnKeyDown
    }
}
