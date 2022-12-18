using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.WinUI.Controls;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// The AccountView for the application
/// </summary>
public sealed partial class AccountView : UserControl
{
    private readonly AccountViewController _controller;
    private readonly Action<object> _initializeWithWindow;

    /// <summary>
    /// Constructs an AccountView
    /// </summary>
    /// <param name="controller">The AccountViewController</param>
    /// <param name="initializeWithWindow">The Action<object> callback for InitializeWithWindow</param>
    public AccountView(AccountViewController controller, Action<object> initializeWithWindow)
    {
        InitializeComponent();
        _controller = controller;
        _initializeWithWindow = initializeWithWindow;
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
        LblOverview.Text = _controller.Localizer["Overview"];
        LblIncomeTitle.Text = $"{_controller.Localizer["Income"]}:";
        LblExpenseTitle.Text = $"{_controller.Localizer["Expense"]}:";
        LblGroups.Text = _controller.Localizer["Groups"];
        LblTransactions.Text = _controller.Localizer["Transactions"];
        //Register Events
        _controller.AccountInfoChanged += AccountInfoChanged;
        //Load Account
        AccountInfoChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the account information is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void AccountInfoChanged(object? sender, EventArgs e)
    {
        //Overview
        LblTitle.Text = _controller.AccountTitle;
        LblTotalAmount.Text = _controller.AccountTotalString;
        ChkFilterIncome.IsChecked = true;
        LblIncomeAmount.Text = _controller.AccountIncomeString;
        LblIncomeAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 38, 162, 105) : Color.FromArgb(255, 143, 240, 164));
        ChkFilterExpense.IsChecked = true;
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

    /// <summary>
    /// Occurs when the new transaction button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void NewTransaction(object? sender, RoutedEventArgs e)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "New transaction not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the edit transaction action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the transaction to be edited</param>
    private async void EditTransaction(object? sender, uint transactionId)
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

    /// <summary>
    /// Occurs when the delete transaction action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the transaction to be deleted</param>
    private async void DeleteTransaction(object? sender, uint transactionId)
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

    /// <summary>
    /// Occurs when the new group button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void NewGroup(object? sender, RoutedEventArgs e)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "New group not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the edit group action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the group to be edited</param>
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

    /// <summary>
    /// Occurs when the delete group action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the group to be deleted</param>
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

    /// <summary>
    /// Occurs when the group filter is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">The id of the group who's filter changed and whether to filter or not</param>
    private async void UpdateGroupFilter(object? sender, (int Id, bool Filter) e)
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

    /// <summary>
    /// Occurs when the transfer money button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void TransferMoney(object? sender, RoutedEventArgs e)
    {
        var contentDialog = new ContentDialog()
        {
            Title = "TODO",
            Content = "Transfer money not implemented yet.",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        await contentDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the import from file button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ImportFromFile(object? sender, RoutedEventArgs e)
    {
        var fileOpenPicker = new FileOpenPicker();
        _initializeWithWindow(fileOpenPicker);
        fileOpenPicker.FileTypeFilter.Add(".csv");
        fileOpenPicker.FileTypeFilter.Add(".ofx");
        fileOpenPicker.FileTypeFilter.Add(".qif");
        fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
        {
            _controller.ImportFromFile(file.Path);
        }
    }

    /// <summary>
    /// Occurs when the export to file button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ExportToFile(object? sender, RoutedEventArgs e)
    {
        var fileSavePicker = new FileSavePicker();
        _initializeWithWindow(fileSavePicker);
        fileSavePicker.FileTypeChoices.Add("CSV", new List<string>() { ".csv" });
        fileSavePicker.SuggestedFileName = _controller.AccountTitle;
        fileSavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileSavePicker.PickSaveFileAsync();
        if (file != null)
        {
            _controller.ExportToFile(file.Path);
        }
    }

    /// <summary>
    /// Occurs when the income filter checkbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ChkFilterIncome_Changed(object sender, RoutedEventArgs e) => UpdateGroupFilter(this, (-3, ChkFilterIncome.IsChecked ?? false));

    /// <summary>
    /// Occurs when the expense filter checkbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ChkFilterExpense_Changed(object sender, RoutedEventArgs e) => UpdateGroupFilter(this, (-2, ChkFilterExpense.IsChecked ?? false));

    /// <summary>
    /// Occurs when the ListGroups' selection is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void ListGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(ListGroups.SelectedIndex != -1)
        {
            EditGroup(null, ((GroupRow)ListGroups.SelectedItem).Id);
            ListGroups.SelectedIndex = -1;
        }
    }

    /// <summary>
    /// Occurs when the ListTransactions' selection is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void ListTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListTransactions.SelectedIndex != -1)
        {
            EditTransaction(null, ((TransactionRow)ListTransactions.SelectedItem).Id);
            ListTransactions.SelectedIndex = -1;
        }
    }
}
