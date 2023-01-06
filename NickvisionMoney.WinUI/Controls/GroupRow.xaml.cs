using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using Windows.UI;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A row to display a Group model
/// </summary>
public sealed partial class GroupRow : UserControl
{
    private readonly Group _group;

    /// <summary>
    /// The Id of the Group the row represents
    /// </summary>
    public uint Id => _group.Id;

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    public event EventHandler<(int Id, bool Filter)>? FilterChanged;
    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    /// <summary>
    /// Constructs a GroupRow
    /// </summary>
    /// <param name="group">The Group model to represent</param>
    /// <param name="localizer">The Localizer for the app</param>
    /// <param name="filterActive">Whether or not the filter on the row should be active</param>
    public GroupRow(Group group, Localizer localizer, bool filterActive)
    {
        InitializeComponent();
        _group = group;
        //Localize Strings
        MenuEdit.Text = localizer["Edit", "GroupRow"];
        MenuDelete.Text = localizer["Delete", "GroupRow"];
        if (_group.Id == 0)
        {
            MenuEdit.IsEnabled = false;
            BtnEdit.Visibility = Visibility.Collapsed;
            MenuDelete.IsEnabled = false;
            BtnDelete.Visibility = Visibility.Collapsed;
        }
        else
        {
            ToolTipService.SetToolTip(BtnEdit, localizer["Edit", "GroupRow"]);
            ToolTipService.SetToolTip(BtnDelete, localizer["Delete", "GroupRow"]);
        }
        //Load Group
        ChkFilter.IsChecked = filterActive;
        LblName.Text = _group.Name;
        LblDescription.Visibility = string.IsNullOrEmpty(_group.Description) ? Visibility.Collapsed : Visibility.Visible;
        LblDescription.Text = _group.Description;
        LblAmount.Text = _group.Balance.ToString("C");
        LblAmount.Foreground = _group.Balance >= 0 ? new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 38, 162, 105) : Color.FromArgb(255, 143, 240, 164)) : new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 192, 28, 40) : Color.FromArgb(255, 255, 123, 99));
    }

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ChkFilterChanged(object sender, RoutedEventArgs e) => FilterChanged?.Invoke(this, ((int)Id, ChkFilter.IsChecked ?? false));

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Edit(object sender, RoutedEventArgs e)
    {
        if(_group.Id != 0)
        {
            EditTriggered?.Invoke(this, Id);
        }
    }

    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Delete(object sender, RoutedEventArgs e)
    {
        if(_group.Id != 0)
        {
            DeleteTriggered?.Invoke(this, Id);
        }
    }
}
