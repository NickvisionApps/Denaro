using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A page control for a ViewStack
/// </summary>
public sealed partial class ViewStackPage : Frame
{
    public static DependencyProperty PageNameProperty { get; } = DependencyProperty.Register("PageName", typeof(string), typeof(ViewStackPage), new PropertyMetadata(""));

    /// <summary>
    /// Constructs a ViewStackPage
    /// </summary>
    public ViewStackPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The name of this page
    /// </summary>
    public string PageName
    {
        get => (string)GetValue(PageNameProperty);

        set => SetValue(PageNameProperty, value);
    }
}