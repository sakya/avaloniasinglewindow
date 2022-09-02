using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SingleWindow.Abstracts;

namespace SingleWindow.Dialogs;

public partial class MessageDialog : BaseDialog
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}