using System.ComponentModel;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Avalonia.SingleWindow.Abstracts;

public abstract class BaseDialog : UserControl, IDisposable
{
    public static BaseDialog CurrentDialog = null;
    private bool _closed;
    private object _result;

    protected BaseDialog()
    {
        Animated = true;
        CloseOnBackdropClick = true;
        KeyDown += OnKeyDown;

        VerticalAlignment = VerticalAlignment.Stretch;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        Background = new SolidColorBrush(Colors.Transparent);
    }

    public bool Animated { get; set; }
    public bool CloseOnBackdropClick { get; set; }

    protected MainWindowBase MainWindow
    {
        get
        {
            if (Application.Current != null) {
                return (Application.Current.ApplicationLifetime as
                        Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)
                    ?.MainWindow as MainWindowBase;
            }

            return null;
        }
    }

    public virtual void Dispose()
    {
    }

    public virtual void OnKeyDown(object sender, KeyEventArgs e)
    {
    }

    public virtual Task Show(Window owner)
    {
        return Show<object>(owner);
    }

    protected virtual void Opened()
    {
    }

    public virtual async Task<T> Show<T>(Window owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        var container = MainWindow.Container;
        if (container == null)
            throw new Exception("Container not found");

        CurrentDialog = this;
        _closed = false;
        owner.Closing += OwnerOnClosing;

        // Create the backdrop
        var border = new Border()
        {
            Background = new SolidColorBrush(new Color(255, 0, 0, 0)),
            Opacity = 0.8
        };
        border.Tapped += (s, a) =>
        {
            if (CloseOnBackdropClick)
                Close();
        };
        container.Children.Add(border);
        var bAnim = AnimateBackdrop(border, 0, 0.8);

        // Animate entrance
        container.Children.Add(this);
        if (Animated)
            await Animate(container.Bounds.Height, 0.0);
        await bAnim;
        Focus();
        Opened();

        while (!_closed)
            await Task.Delay(100);

        // Animate exit
        bAnim = AnimateBackdrop(border, 0.8, 0);
        if (Animated)
            await Animate(0.0, container.Bounds.Height);
        await bAnim;

        container.Children.Remove(this);
        container.Children.Remove(border);
        owner.Closing -= OwnerOnClosing;
        CurrentDialog = null;

        return (T)_result;
    }

    public virtual void Close()
    {
        Close(null);
    }

    public virtual void Close(object result)
    {
        _result = result;
        _closed = true;
    }

    #region private operations
    private void OwnerOnClosing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Close(null);
    }

    private async Task Animate(double from, double to)
    {
        IsHitTestVisible = false;

        RenderTransform = new TranslateTransform() {
            Y = from
        };

        var animation = new Animation.Animation()
        {
            Duration = TimeSpan.FromMilliseconds(250),
            Easing = new LinearEasing()
        };

        var kf = new KeyFrame()
        {
            Cue = new Cue(0.0)
        };
        kf.Setters.Add(new Setter()
        {
            Property = TranslateTransform.YProperty,
            Value = from
        });
        animation.Children.Add(kf);

        kf = new KeyFrame()
        {
            Cue = new Cue(1.0)
        };
        kf.Setters.Add(new Setter()
        {
            Property = TranslateTransform.YProperty,
            Value = to
        });
        animation.Children.Add(kf);

        await animation.RunAsync(this, null);
        RenderTransform = new TranslateTransform() {
            Y = to
        };
        IsHitTestVisible = true;
    }

    private async Task AnimateBackdrop(Border backdrop, double from, double to)
    {
        backdrop.Opacity = from;
        var animation = new Animation.Animation()
        {
            Duration = TimeSpan.FromMilliseconds(250),
            Easing = new LinearEasing()
        };

        var kf = new KeyFrame()
        {
            Cue = new Cue(0.0)
        };
        kf.Setters.Add(new Setter()
        {
            Property = OpacityProperty,
            Value = from
        });
        animation.Children.Add(kf);

        kf = new KeyFrame()
        {
            Cue = new Cue(1.0)
        };
        kf.Setters.Add(new Setter()
        {
            Property = OpacityProperty,
            Value = to
        });
        animation.Children.Add(kf);

        await animation.RunAsync(backdrop, null);
        backdrop.Opacity = to;
    }
    #endregion
}