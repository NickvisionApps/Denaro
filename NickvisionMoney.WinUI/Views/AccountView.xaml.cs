using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.WinUI.Controls;
using NickvisionMoney.WinUI.Helpers;
using System;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

public sealed partial class AccountView : UserControl
{
    private readonly AccountViewController _controller;

    public AccountView(AccountViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        BtnNewTransaction.Label = _controller.Localizer["NewTransaction"];
        ToolTipService.SetToolTip(BtnNewTransaction, _controller.Localizer["NewTransaction", "Tooltip"]);
        BtnNewGroup.Label = _controller.Localizer["NewGroup"];
        ToolTipService.SetToolTip(BtnNewGroup, _controller.Localizer["NewGroup", "Tooltip"]);
        BtnTransferMoney.Label = _controller.Localizer["TransferMoney"];
        ToolTipService.SetToolTip(BtnTransferMoney, _controller.Localizer["TransferMoney", "Tooltip"]);
        BtnImportFromFile.Label = _controller.Localizer["ImportFromFile"];
        ToolTipService.SetToolTip(BtnImportFromFile, _controller.Localizer["ImportFromFile", "Tooltip"]);
        BtnExportToFile.Label = _controller.Localizer["ExportToFile"];
        ToolTipService.SetToolTip(BtnExportToFile, _controller.Localizer["ExportToFile", "Tooltip"]);
        LblOverview.Text = _controller.Localizer["Overview"];
        LblTotalTitle.Text = $"{_controller.Localizer["Total"]}:";
        LblIncomeTitle.Text = $"{_controller.Localizer["Income"]}:";
        LblExpenseTitle.Text = $"{_controller.Localizer["Expense"]}:";
        LblGroups.Text = _controller.Localizer["Groups"];
        LblTransactions.Text = _controller.Localizer["Transactions"];
        //Register Events
        _controller.AccountInfoChanged += AccountInfoChanged;
        //Load Account
        AccountInfoChanged(null, EventArgs.Empty);
    }

    private void AccountInfoChanged(object? sender, EventArgs e)
    {
        //Overview
        LblTitle.Text = _controller.AccountTitle;
        LblTotalAmount.Text = _controller.AccountTotalString;
        LblTotalAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 28, 113, 216) : Color.FromArgb(255, 120, 174, 237));
        LblIncomeAmount.Text = _controller.AccountIncomeString;
        LblIncomeAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 38, 162, 105) : Color.FromArgb(255, 143, 240, 164));
        LblExpenseAmount.Text = _controller.AccountExpenseString;
        LblExpenseAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 192, 28, 40) : Color.FromArgb(255, 255, 123, 99));
        //Groups
        ListGroups.Items.Clear();
        foreach (var pair in _controller.Groups)
        {
            var groupRow = new GroupRow(pair.Value);
            groupRow.EditTriggered += EditGroup;
            groupRow.DeleteTriggered += DeleteGroup;
            groupRow.FilterChanged += UpdateGroupFilter;
            ListGroups.Items.Add(groupRow);
        }
        //Transactions
        ListTransactions.Items.Clear();
        foreach(var pair in _controller.Transactions)
        {
            var transactionRow = new TransactionRow(pair.Value, ColorHelpers.FromRGBA(_controller.DefaultTransactionColor) ?? Color.FromArgb(255, 0, 0, 0));
            transactionRow.EditTriggered += EditTransaction;
            transactionRow.DeleteTriggered += DeleteTransaction;
            ListTransactions.Items.Add(transactionRow);
        }
    }

    private async void EditGroup(object? sender, uint groupId)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "Edit group not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    private async void DeleteGroup(object? sender, uint groupId)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "Delete group not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    private async void UpdateGroupFilter(object? sender, uint groupId)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "Update filter not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    private async void EditTransaction(object? sender, uint groupId)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "Edit transaction not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    private async void DeleteTransaction(object? sender, uint groupId)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "Delete transaction not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    private void ListGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(ListGroups.SelectedIndex != -1)
        {
            EditGroup(null, ((GroupRow)ListGroups.SelectedItem).Id);
            ListGroups.SelectedIndex = -1;
        }
    }

    private void ListTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListTransactions.SelectedIndex != -1)
        {
            EditTransaction(null, ((TransactionRow)ListTransactions.SelectedItem).Id);
            ListTransactions.SelectedIndex = -1;
        }
    }
}
