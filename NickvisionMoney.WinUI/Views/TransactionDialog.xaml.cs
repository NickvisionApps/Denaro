using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Linq;
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
        var groups = _controller.Groups.Values.ToList();
        groups.Sort();
        foreach (var group in groups)
        {
            CmbGroup.Items.Add(group);
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
            var checkStatus = _controller.UpdateTransaction(DateOnly.FromDateTime(CalendarDate.Date!.Value.Date), TxtDescription.Text, (TransactionType)CmbType.SelectedIndex, (TransactionRepeatInterval)CmbRepeatInterval.SelectedIndex, (string)CmbGroup.SelectedItem, ColorHelpers.ToRGBA(BtnColor.SelectedColor), TxtAmount.Text);
            if(checkStatus != TransactionCheckStatus.Valid)
            {
                TxtDescription.Header = _controller.Localizer["Description", "Field"];
                TxtAmount.Header = _controller.Localizer["Amount", "Field"];
                if (checkStatus == TransactionCheckStatus.EmptyDescription)
                {
                    TxtDescription.Header = _controller.Localizer["Description", "Empty"];
                }
                else if(checkStatus == TransactionCheckStatus.InvalidAmount)
                {
                    TxtAmount.Header = _controller.Localizer["Amount", "Invalid"];
                }
                TxtErrors.Visibility = Visibility.Visible;
                return await ShowAsync();
            }
            else
            {
                _controller.Accepted = true;
                return true;
            }
        }
        return false;
    }
}
