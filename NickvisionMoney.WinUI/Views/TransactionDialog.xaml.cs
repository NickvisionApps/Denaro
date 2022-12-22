using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Threading.Tasks;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing a Transaction
/// </summary>
public sealed partial class TransactionDialog : ContentDialog
{
    private readonly TransactionDialogController _controller;

    /// <summary>
    /// Constructs a TransactionDialog
    /// </summary>
    /// <param name="controller">The TransactionDialogController</param>
    public TransactionDialog(TransactionDialogController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        Title = $"{_controller.Localizer["Transaction"]} - {_controller.Transaction.Id}";
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer["OK"];
        TxtDescription.Header = _controller.Localizer["Description", "Field"];
        TxtDescription.PlaceholderText = _controller.Localizer["Description", "Placeholder"];
        TxtAmount.Header = _controller.Localizer["Amount", "Field"];
        TxtAmount.PlaceholderText = _controller.Localizer["Amount", "Placeholder"];
        CmbType.Header = _controller.Localizer["TransactionType", "Field"];
        CmbType.Items.Add(_controller.Localizer["Income"]);
        CmbType.Items.Add(_controller.Localizer["Expense"]);
        CalendarDate.Header = _controller.Localizer["Date", "Field"];
        CmbRepeatInterval.Header = _controller.Localizer["TransactionRepeatInterval", "Field"];
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Never"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Daily"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Weekly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Monthly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Quarterly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Yearly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Biyearly"]);
        CmbGroup.Header = _controller.Localizer["Group", "Field"];
        CmbGroup.Items.Add(_controller.Localizer["Ungrouped"]);
        LblColor.Text = _controller.Localizer["Color", "Field"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        //Load Transaction
        TxtDescription.Text = _controller.Transaction.Description;
        TxtAmount.Text = _controller.Transaction.Amount.ToString("C");
        CmbType.SelectedIndex = (int)_controller.Transaction.Type;
        CalendarDate.Date = new DateTimeOffset(new DateTime(_controller.Transaction.Date.Year, _controller.Transaction.Date.Month, _controller.Transaction.Date.Day));
        CmbRepeatInterval.SelectedIndex = (int)_controller.Transaction.RepeatInterval;
        foreach (var pair in _controller.Groups)
        {
            CmbGroup.Items.Add(pair.Value);
        }
        if (_controller.Transaction.GroupId == -1)
        {
            CmbGroup.SelectedIndex = 0;
        }
        else
        {
            CmbGroup.SelectedItem = _controller.Groups[(uint)_controller.Transaction.GroupId];
        }
        BtnColor.SelectedColor = (Windows.UI.Color)(ColorHelpers.FromRGBA(_controller.Transaction.RGBA) ?? ColorHelpers.FromRGBA(_controller.TransactionDefaultColor)!);
    }

    /// <summary>
    /// Shows the TransactionDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            _controller.Accepted = false;
            return false;
        }
        else if (result == ContentDialogResult.Primary)
        {

        }
        return false;
    }
}
