using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System;

namespace SingleWindow.Controls
{
    public class TitleBar : UserControl
    {
        bool m_CanGoBack = false;

        public TitleBar()
        {
            InitializeComponent();

            CanMinimize = true;
            CanMaximize = true;
        }

        public bool CanGoBack { 
            get { return m_CanGoBack; }
            set
            {
                m_CanGoBack = value;
                var btn = this.FindControl<Button>("m_BackBtn");
                btn.IsVisible = m_CanGoBack;

                var icon = this.FindControl<Image>("m_Icon");
                icon.IsVisible = !m_CanGoBack;
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
                title.Subscribe(value =>
                {
                    SetTitle(value);
                });

                var wState = pw.GetObservable(Window.WindowStateProperty);
                wState.Subscribe(s =>
                {
                    var btn = this.FindControl<Button>("m_MaximizeBtn");
                    if (s == WindowState.Maximized) {
                        pw.Padding = new Thickness(5);
                        btn.Content = new Projektanker.Icons.Avalonia.Icon() { Value = "fas fa-window-restore" };
                    } else {
                        pw.Padding = new Thickness(0);
                        btn.Content = new Projektanker.Icons.Avalonia.Icon() { Value = "fas fa-window-maximize" };
                    }
                });
            }

            var btn = this.FindControl<Button>("m_MinimizeBtn");
            btn.Click += (e, a) =>
            {
                ((Window)this.VisualRoot).WindowState = WindowState.Minimized;
            };
            btn.IsVisible = CanMinimize;

            btn = this.FindControl<Button>("m_MaximizeBtn");
            btn.Click += (e, a) =>
            {
                var pw = (Window)this.VisualRoot;
                if (pw.WindowState == WindowState.Maximized)
                    pw.WindowState = WindowState.Normal;
                else
                    pw.WindowState = WindowState.Maximized;
            };
            btn.IsVisible = CanMinimize;

            btn = this.FindControl<Button>("m_CloseBtn");
            btn.Click += (e, a) =>
            {
                ((Window)this.VisualRoot).Close();
            };

            btn = this.FindControl<Button>("m_BackBtn");
            btn.Click += async (e, a) =>
            {
                await (pw as MainWindow).NavigateBack();
            };
        }

        private void SetTitle(string title)
        {
            var txt = this.FindControl<TextBlock>("m_Title");
            txt.Text = title;
        }
    }
}
