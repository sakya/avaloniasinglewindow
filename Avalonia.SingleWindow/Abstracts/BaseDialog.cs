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
    public event EventHandler<CancelEventArgs> Closing;
    public static BaseDialog CurrentDialog;
    private bool _closed;
    private object _result;

    protected BaseDialog()
    {
        Focusable = true;
        Animated = true;
        CloseOnBackdropClick = true;
        KeyDown += OnKeyDown;

        VerticalAlignment = VerticalAlignment.Stretch;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        Background = new SolidColorBrush(Colors.Transparent);
    }

    /// <summary>
    /// Defines if the dialog is animated (default: true)
    /// </summary>
    public bool Animated { get; set; }
    /// <summary>
    /// Defines if the dialog is closed if the user clicks on the backdrop (default: true)
    /// </summary>
    public bool CloseOnBackdropClick { get; set; }

    protected MainWindowBase MainWindow => MainWindowBase.Instance;

    public virtual void Dispose()
    {
    }

    public virtual void OnKeyDown(object sender, KeyEventArgs e)
    {
    }

    /// <summary>
    /// Show the <see cref="BaseDialog"/>
    /// </summary>
    /// <returns></returns>
    public virtual Task Show()
    {
        return Show<object>();
    }

    /// <summary>
    /// Called when the <see cref="BaseDialog"/> is opened
    /// </summary>
    protected virtual void Opened()
    {
    }

    /// <summary>
    /// Show the <see cref="BaseDialog"/> with a return type
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual async Task<T> Show<T>()
    {
        var container = MainWindow.Container;
        if (container == null)
            throw new Exception("MainWindow.Container is null");

        CurrentDialog = this;
        _closed = false;
        MainWindowBase.Instance.Closing += OwnerOnClosing;

        // Create the backdrop
        var backdrop = new Border()
        {
            Background = new SolidColorBrush(new Color(255, 0, 0, 0)),
            Opacity = 0.8
        };
        backdrop.Tapped += (s, a) =>
        {
            if (CloseOnBackdropClick)
                Close();
        };
        container.Children.Add(backdrop);
        var bAnim = AnimateBackdrop(backdrop, 0, 0.8);

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
        bAnim = AnimateBackdrop(backdrop, 0.8, 0);
        if (Animated)
            await Animate(0.0, container.Bounds.Height);
        await bAnim;

        container.Children.Remove(this);
        container.Children.Remove(backdrop);
        MainWindowBase.Instance.Closing -= OwnerOnClosing;
        CurrentDialog = null;

        return (T)_result;
    }

    /// <summary>
    /// Close the <see cref="BaseDialog"/>
    /// </summary>
    public virtual void Close()
    {
        Close(null);
    }

    /// <summary>
    /// Close the <see cref="BaseDialog"/> returning the given value
    /// </summary>
    public virtual void Close(object result)
    {
        if (Closing != null) {
            var args = new CancelEventArgs();
            Closing.Invoke(this, args);
            if (args.Cancel)
                return;
        }

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
            Easing = new LinearEasing(),
            FillMode = FillMode.Forward
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

        await animation.RunAsync(this, CancellationToken.None);
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
            Easing = new LinearEasing(),
            FillMode = FillMode.Forward
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

        await animation.RunAsync(backdrop, CancellationToken.None);
        backdrop.Opacity = to;
    }
    #endregion
}