using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Globalization;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A row to display a Group model
/// </summary>
public sealed partial class GroupRow : UserControl, IGroupRowControl
{
    private CultureInfo _cultureAmount;
    private bool _useNativeDigits;

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
    /// Constructs a group row 
    /// </summary>
    /// <param name="group">The Group to display</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="useNativeDigits">Whether to use native digits</param>
    /// <param name="localizer">The Localizer for the app</param>
    /// <param name="filterActive">Whether or not the filter checkbutton should be active</param>
    /// <param name="defaultColor">The default color for the row</param>
    public GroupRow(Group group, CultureInfo cultureAmount, bool useNativeDigits, Localizer localizer, bool filterActive, string defaultColor)
    {
        InitializeComponent();
        _cultureAmount = cultureAmount;
        _useNativeDigits = useNativeDigits;
        //Localize Strings
        MenuEdit.Text = localizer["Edit", "GroupRow"];
        MenuDelete.Text = localizer["Delete", "GroupRow"];
        ToolTipService.SetToolTip(BtnEdit, localizer["Edit", "GroupRow"]);
        ToolTipService.SetToolTip(BtnDelete, localizer["Delete", "GroupRow"]);
        UpdateRow(group, defaultColor, cultureAmount, filterActive);
    }

    /// <summary>
    /// Whether or not the filter checkbox is checked
    /// </summary>
    public bool FilterChecked
    {
        get => ChkFilter.IsChecked ?? false;

        set => ChkFilter.IsChecked = value;
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
    /// <param name="defaultColor">The default color for the row</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="filterActive">Whether or not the filter checkbox is active</param>
    public void UpdateRow(Group group, string defaultColor, CultureInfo cultureAmount, bool filterActive)
    {
        Id = group.Id;
        _cultureAmount = cultureAmount;
        if (group.Id == 0)
        {
            MenuEdit.IsEnabled = false;
            BtnEdit.Visibility = Visibility.Collapsed;
            MenuDelete.IsEnabled = false;
            BtnDelete.Visibility = Visibility.Collapsed;
        }
        BtnId.Background = new SolidColorBrush(ColorHelpers.FromRGBA(group.RGBA) ?? ColorHelpers.FromRGBA(defaultColor)!.Value);
        ChkFilter.IsChecked = filterActive;
        LblName.Text = group.Name;
        LblDescription.Visibility = string.IsNullOrEmpty(group.Description) ? Visibility.Collapsed : Visibility.Visible;
        LblDescription.Text = group.Description;
        LblAmount.Text = $"{(group.Balance >= 0 ? "+  " : "-  ")}{group.Balance.ToAmountString(_cultureAmount, _useNativeDigits)}";
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
        if (Id != 0)
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
        if (Id != 0)
        {
            DeleteTriggered?.Invoke(this, Id);
        }
    }
}
