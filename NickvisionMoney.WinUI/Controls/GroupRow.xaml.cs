using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Models;
using System;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A row to display a Group model
/// </summary>
public sealed partial class GroupRow : UserControl
{
    private readonly Group _group;

    public uint Id => _group.Id;

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    public event EventHandler<uint>? FilterChanged;
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
    public GroupRow(Group group)
    {
        InitializeComponent();
        _group = group;
        //Load Group
        ChkFilter.IsChecked = true;
        LblName.Text = _group.Name;
        LblDescription.Text = _group.Description;
        LblAmount.Text = _group.Balance.ToString("C");
    }

    /// <summary>
    /// Whether or not the filter checkbox is active
    /// </summary>
    public bool FilterActive
    {
        get => ChkFilter.IsChecked ?? false;

        set => ChkFilter.IsChecked = value;
    }

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ChkFilterChanged(object sender, RoutedEventArgs e) => FilterChanged?.Invoke(this, Id);

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Edit(object sender, RoutedEventArgs e) => EditTriggered?.Invoke(this, Id);

    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Delete(object sender, RoutedEventArgs e) => DeleteTriggered?.Invoke(this, Id);
}
