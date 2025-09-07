using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System;
using Avalonia.Reactive;

namespace SingleWindow.Controls
{
    public partial class TitleBar : UserControl
    {
        bool _canGoBack;

        public TitleBar()
        {
            InitializeComponent();

            CanMinimize = true;
            CanMaximize = true;
        }

        public bool CanGoBack {
            get => _canGoBack;
            set
            {
                _canGoBack = value;
                var btn = this.FindControl<Button>("BackBtn");
                btn.IsVisible = _canGoBack;

                var icon = this.FindControl<Image>("Icon");
                icon.IsVisible = !_canGoBack;
            }
        }

        public bool CanMinimize { get; set; }
        public bool CanMaximize { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.IsVisible = Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var pw = (Window)this.VisualRoot;
            if (pw != null) {
                SetTitle(pw.Title);
                var title = pw.GetObservable(Window.TitleProperty);
                title.Subscribe(new AnonymousObserver<string>(value =>
                {
                    SetTitle(value);
                }));

                var wState = pw.GetObservable(Window.WindowStateProperty);
                wState.Subscribe(new AnonymousObserver<WindowState>(s =>
                {
                    var btn = this.FindControl<Button>("MaximizeBtn");
                    if (s == WindowState.Maximized) {
                        pw.Padding = new Thickness(5);
                        btn.Content = new Projektanker.Icons.Avalonia.Icon() { Value = "fas fa-window-restore" };
                    } else {
                        pw.Padding = new Thickness(0);
                        btn.Content = new Projektanker.Icons.Avalonia.Icon() { Value = "fas fa-window-maximize" };
                    }
                }));
            }

            var btn = this.FindControl<Button>("MinimizeBtn");
            btn.Click += (e, a) =>
            {
                ((Window)this.VisualRoot).WindowState = WindowState.Minimized;
            };
            btn.IsVisible = CanMinimize;

            btn = this.FindControl<Button>("MaximizeBtn");
            btn.Click += (s, a) =>
            {
                var vr = (Window)this.VisualRoot;
                if (vr.WindowState == WindowState.Maximized)
                    vr.WindowState = WindowState.Normal;
                else
                    vr.WindowState = WindowState.Maximized;
            };
            btn.IsVisible = CanMinimize;

            btn = this.FindControl<Button>("CloseBtn");
            btn.Click += (s, a) =>
            {
                ((Window)this.VisualRoot).Close();
            };

            btn = this.FindControl<Button>("BackBtn");
            btn.Click += async (s, a) =>
            {
                await (pw as MainWindow).NavigateBack();
            };
        }

        private void SetTitle(string title)
        {
            var txt = this.FindControl<TextBlock>("Title");
            txt.Text = title;
        }
    }
}
