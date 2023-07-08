using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.SingleWindow.Abstracts;

namespace SingleWindow.Dialogs;

public partial class MessageDialog : BaseDialog
{
    public MessageDialog()
    {
        InitializeComponent();

        VerticalAlignment = VerticalAlignment.Bottom;
        KeyDown += OnKeyDown;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.Enter) {
            e.Handled = true;
            Close();
        }
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}