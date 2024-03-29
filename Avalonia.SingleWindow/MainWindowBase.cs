﻿using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.SingleWindow.Abstracts;
using Avalonia.Styling;
using System.ComponentModel;

namespace Avalonia.SingleWindow
{
    public abstract class MainWindowBase : Window
    {
        private string _windowTitle;
        private readonly List<BasePage> _pageHistory = new();
        private readonly Dictionary<string, BasePage.PageState> _pageStates = new();
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

            public TransitionSettings(EnterTransitions transition, TimeSpan duration, Animation.Easings.Easing easing = null)
            {
                Type = transition;
                Duration = duration;
                Easing = easing ?? new Animation.Easings.LinearEasing();
            }

            public EnterTransitions Type { get; set; }
            public TimeSpan Duration { get; set; }
            public Animation.Easings.Easing Easing { get; set; }
        } // TransitionSettings
        #endregion

        #region public properties
        /// <summary>
        /// The <see cref="MainWindowBase"/> instance
        /// </summary>
        public static MainWindowBase Instance { get; private set; }
        /// <summary>
        /// Defines the transition used when navigating to a new page
        /// </summary>
        public TransitionSettings Transition { get; set; }
        /// <summary>
        ///  When this key is pressed the current page navigates back (default: Escape)
        /// </summary>
        public Key BackKey { get; set; }

        /// <summary>
        /// The window title
        /// </summary>
        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value;
                Title = WindowTitle;
            }
        }

        /// <summary>
        /// The current <see cref="BasePage"/>
        /// </summary>
        public BasePage CurrentPage
        {
            get
            {
                if (Container.Children.Count > 0)
                    return Container.Children.FirstOrDefault(c => c is BasePage) as BasePage;
                return null;
            }
        }

        /// <summary>
        /// Returns if the current page can navigate back
        /// </summary>
        public bool CanNavigateBack => _pageHistory.Count > 0;
        #endregion

        protected MainWindowBase()
        {
            if (Instance != null)
                throw new Exception("Only one instance of MainWindowBase is allowed");

            Instance = this;
            Closing += OnWindowClosing;
            KeyDown += OnKeyDown;

            Transition = new TransitionSettings(TransitionSettings.EnterTransitions.SlideLeft, TimeSpan.FromMilliseconds(250));
            BackKey = Key.Escape;
        }

        #region public operations
        /// <summary>
        /// The panel where pages and dialogs are displayed
        /// </summary>
        public Panel Container { get; set; }

        /// <summary>
        /// Navigate to a new page
        /// </summary>
        /// <param name="page"></param>
        /// <returns>True on success</returns>
        public async Task<bool> NavigateTo(BasePage page)
        {
            if (_changingPage)
                return false;

            // This delay lets the window draw for the first page
            await Task.Delay(1);
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

        /// <summary>
        /// Save the current page state
        /// </summary>
        /// <param name="state">The page state</param>
        public void SavePageState(BasePage.PageState state)
        {
            _pageStates[state.PageId] = state;
        } // SavePageState

        /// <summary>
        /// Load the page state of the given <see cref="BasePage"/>
        /// </summary>
        /// <param name="page">The page</param>
        public BasePage.PageState LoadPageState<T>(BasePage page) where T : BasePage.PageState
        {
            if (_pageStates.TryGetValue(page.Id, out var res))
                return res as T;
            return null;
        } // LoadPageState
        #endregion

        /// <summary>
        /// Called when the page is changing
        /// </summary>
        protected virtual void PageChanging()
        {
        }

        /// <summary>
        /// Called when the page changed
        /// </summary>
        protected virtual void PageChanged()
        {
        }

        #region private operations
        private async Task ChangePage(BasePage exiting, BasePage entering, bool back)
        {
            PageChanging();
            _changingPage = true;
            entering.Opacity = 0;
            Container.Children.Add(entering);
            if (exiting == null) {
                entering.Opacity = 1;
                _changingPage = false;
                return;
            }

            if (Transition == null || Transition.Type == TransitionSettings.EnterTransitions.None) {
                Container.Children.Remove(exiting);
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
                    to = Bounds.Size.Width;

                    exiting.RenderTransform = new TranslateTransform();
                    entering.RenderTransform = new TranslateTransform()
                    {
                        X = to
                    };
                    break;
                case TransitionSettings.EnterTransitions.SlideUp:
                    property = TranslateTransform.YProperty;
                    from = 0.0;
                    to = Bounds.Size.Height;

                    exiting.RenderTransform = new TranslateTransform();
                    entering.RenderTransform = new TranslateTransform()
                    {
                        Y = to
                    };
                    break;
                case TransitionSettings.EnterTransitions.FadeIn:
                    property = OpacityProperty;
                    from = 1.0;
                    to = 0.0;
                    break;
            }

            // Exiting
            var exitAnim = new Animation.Animation()
            {
                Duration = Transition.Duration,
                Easing = Transition.Easing,
                FillMode = FillMode.Forward
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
            var enterAnim = new Animation.Animation()
            {
                Duration = Transition.Duration,
                Easing = Transition.Easing,
                FillMode = FillMode.Forward
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
                await exitAnim.RunAsync(exiting, CancellationToken.None);
                exiting.Opacity = 0;
                await enterAnim.RunAsync(entering, CancellationToken.None);
                entering.Opacity = 1.0;
            } else {
                var tasks = new List<Task>
                {
                    exitAnim.RunAsync(exiting, CancellationToken.None),
                    enterAnim.RunAsync(entering, CancellationToken.None)
                };
                entering.Opacity = 1.0;
                await Task.WhenAll(tasks);
            }

            entering.RenderTransform = null;
            exiting.RenderTransform = null;

            entering.IsHitTestVisible = true;
            Container.Children.Remove(exiting);

            _changingPage = false;
            PageChanged();
        } // ChangePage

        private void RemovePageState(BasePage page)
        {
            if (page != null)
                _pageStates.Remove(page.Id);
        } // RemovePageState

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

            if (CurrentPage is { NavigateBackWithKeyboard: true } && e.KeyModifiers == KeyModifiers.None && e.Key == BackKey) {
                e.Handled = true;
                await NavigateBack();
            }
        } // OnKeyDown
        #endregion
    }
}
