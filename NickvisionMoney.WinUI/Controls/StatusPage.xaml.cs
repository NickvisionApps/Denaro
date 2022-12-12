using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A control for displaying a status
/// </summary>
public sealed partial class StatusPage : UserControl
{
    public static DependencyProperty GlyphProperty { get; } = DependencyProperty.Register("Glyph", typeof(string), typeof(StatusPage), new PropertyMetadata(""));
    public static DependencyProperty TitleProperty { get; } = DependencyProperty.Register("Title", typeof(string), typeof(StatusPage), new PropertyMetadata(""));
    public static DependencyProperty DescriptionProperty { get; } = DependencyProperty.Register("Description", typeof(string), typeof(StatusPage), new PropertyMetadata(""));

    /// <summary>
    /// Constructs a StatusPage
    /// </summary>
    public StatusPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The glyph code for the FontIcon
    /// </summary>
    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);

        set => SetValue(GlyphProperty, value);
    }

    /// <summary>
    /// The title of the status
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);

        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The message of the status
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);

        set => SetValue(DescriptionProperty, value);
    }
}