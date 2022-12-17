using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.WinUI.Controls;
using System;

namespace NickvisionMoney.WinUI.Views;

public sealed partial class AccountView : UserControl
{
    private readonly AccountViewController _controller;

    public AccountView(AccountViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        LblTotalTitle.Text = $"{_controller.Localizer["Total"]}:";
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
        LblTotalAmount.Text = _controller.AccountTotal;
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
            var transactionRow = new TransactionRow(pair.Value);
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
