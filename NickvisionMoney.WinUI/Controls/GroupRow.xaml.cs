using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;
using Windows.UI;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A row to display a Group model
/// </summary>
public sealed partial class GroupRow : UserControl, IGroupRowControl
{
    private CultureInfo _culture;

    /// <summary>
    /// The Id of the Group the row represents
    /// </summary>
    public uint Id { get; private set; }

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    public event EventHandler<(uint Id, bool Filter)>? FilterChanged;
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
    /// <param name="culture">The CultureInfo to use for the amount string</param>
    /// <param name="localizer">The Localizer for the app</param>
    /// <param name="filterActive">Whether or not the filter on the row should be active</param>
    public GroupRow(Group group, CultureInfo culture, Localizer localizer, bool filterActive)
    {
        InitializeComponent();
        _culture = culture;
        //Localize Strings
        MenuEdit.Text = localizer["Edit", "GroupRow"];
        MenuDelete.Text = localizer["Delete", "GroupRow"];
        ToolTipService.SetToolTip(BtnEdit, localizer["Edit", "GroupRow"]);
        ToolTipService.SetToolTip(BtnDelete, localizer["Delete", "GroupRow"]);
        UpdateRow(group, filterActive);
    }

    /// <summary>
    /// Shows the row
    /// </summary>
    public void Show() => Visibility = Visibility.Visible;

    /// <summary>
    /// Hides the row
    /// </summary>
    public void Hide() => Visibility = Visibility.Collapsed;

    /// <summary>
    /// Updates the row with the new model
    /// </summary>
    /// <param name="group">The new Group model</param>
    /// <param name="filterActive">Whether or not the filter checkbox is active</param>
    public void UpdateRow(Group group, bool filterActive)
    {
        Id = group.Id;
        if(group.Id == 0)
        {
            MenuEdit.IsEnabled = false;
            BtnEdit.Visibility = Visibility.Collapsed;
            MenuDelete.IsEnabled = false;
            BtnDelete.Visibility = Visibility.Collapsed;
        }
        ChkFilter.IsChecked = filterActive;
        LblName.Text = group.Name;
        LblDescription.Visibility = string.IsNullOrEmpty(group.Description) ? Visibility.Collapsed : Visibility.Visible;
        LblDescription.Text = group.Description;
        LblAmount.Text = group.Balance.ToString("C", _culture);
        LblAmount.Foreground = group.Balance >= 0 ? new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 38, 162, 105) : Color.FromArgb(255, 143, 240, 164)) : new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 192, 28, 40) : Color.FromArgb(255, 255, 123, 99));
    }

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ChkFilterChanged(object sender, RoutedEventArgs e) => FilterChanged?.Invoke(this, (Id, ChkFilter.IsChecked ?? false));

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    public void Edit(object sender, RoutedEventArgs e)
    {
        if(Id != 0)
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
        if(Id != 0)
        {
            DeleteTriggered?.Invoke(this, Id);
        }
    }
}
