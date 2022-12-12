using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A control for showing one page at a time
/// </summary>
public sealed partial class ViewStack : Frame
{
    public static DependencyProperty PagesProperty { get; } = DependencyProperty.Register("Pages", typeof(ObservableCollection<ViewStackPage>), typeof(ViewStack), new PropertyMetadata(new ObservableCollection<ViewStackPage>()));

    /// <summary>
    /// The pages of the ViewStack
    /// </summary>
    public ObservableCollection<ViewStackPage> Pages => (ObservableCollection<ViewStackPage>)GetValue(PagesProperty);

    /// <summary>
    /// Constructs a ViewStack
    /// </summary>
    public ViewStack()
    {
        InitializeComponent();
        Pages.CollectionChanged += CollectionChanged;
    }

    /// <summary>
    /// Changes the page of the ViewStack
    /// </summary>
    /// <param name="pageName">The name of the page to change to</param>
    /// <returns>True if successful, else false</returns>
    public bool ChangePage(string pageName)
    {
        foreach (var page in Pages)
        {
            if (page.PageName == pageName)
            {
                Content = page;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Occurs when the pages collection is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotifyCollectionChangedEventArgs</param>
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Pages.Count == 0)
        {
            Content = null;
        }
        else
        {
            Content = Pages[Pages.Count - 1];
        }
    }
}