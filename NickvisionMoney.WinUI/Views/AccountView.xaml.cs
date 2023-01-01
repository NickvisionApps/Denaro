using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.WinUI.Controls;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private bool _isAccountLoading;

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
        _isAccountLoading = false;
        //Localize Strings
        LblTotalTitle.Text = $"{_controller.Localizer["Total"]}:";
        BtnNewTransaction.Label = _controller.Localizer["NewTransaction"];
        ToolTipService.SetToolTip(BtnNewTransaction, _controller.Localizer["NewTransaction", "Tooltip"]);
        MenuNewTransaction.Text = _controller.Localizer["NewTransaction"];
        BtnNewGroup.Label = _controller.Localizer["NewGroup"];
        ToolTipService.SetToolTip(BtnNewGroup, _controller.Localizer["NewGroup", "Tooltip"]);
        MenuNewGroup.Text = _controller.Localizer["NewGroup"];
        BtnTransferMoney.Label = _controller.Localizer["TransferMoney"];
        ToolTipService.SetToolTip(BtnTransferMoney, _controller.Localizer["TransferMoney", "Tooltip"]);
        MenuTransferMoney.Text = _controller.Localizer["TransferMoney"];
        BtnImportFromFile.Label = _controller.Localizer["ImportFromFile"];
        ToolTipService.SetToolTip(BtnImportFromFile, _controller.Localizer["ImportFromFile", "Tooltip"]);
        BtnExportToFile.Label = _controller.Localizer["ExportToFile"];
        ToolTipService.SetToolTip(BtnExportToFile, _controller.Localizer["ExportToFile", "Tooltip"]);
        BtnShowHideGroups.Label = _controller.Localizer["HideGroups"];
        BtnShowHideGroups.Icon = new FontIcon() { FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], Glyph = "\uED1A" };
        BtnFilters.Label = _controller.Localizer["Filters"];
        MenuResetOverviewFilters.Text = _controller.Localizer["ResetFilters", "Overview"];
        MenuResetGroupsFilters.Text = _controller.Localizer["ResetFilters", "Groups"];
        MenuResetDatesFilters.Text = _controller.Localizer["ResetFilters", "Dates"];
        LblOverview.Text = _controller.Localizer["Overview"];
        LblIncomeTitle.Text = $"{_controller.Localizer["Income"]}:";
        LblExpenseTitle.Text = $"{_controller.Localizer["Expense"]}:";
        LblGroups.Text = _controller.Localizer["Groups"];
        LblCalendar.Text = _controller.Localizer["Calendar"];
        ExpDateRange.Header = _controller.Localizer["SelectRange"];
        DateRangeStart.Header = _controller.Localizer["Start", "DateRange"];
        DateRangeEnd.Header = _controller.Localizer["End", "DateRange"];
        LblTransactions.Text = _controller.Localizer["Transactions"];
        ToolTipService.SetToolTip(BtnSortTopBottom, _controller.Localizer["SortFirstToLast"]);
        ToolTipService.SetToolTip(BtnSortBottomTop, _controller.Localizer["SortLastToFirst"]);
        //Register Events
        _controller.AccountInfoChanged += AccountInfoChanged;
        //Load UI
        DateRangeStart.Date = DateTimeOffset.Now;
        DateRangeEnd.Date = DateTimeOffset.Now;
        if (_controller.SortFirstToLast)
        {
            BtnSortTopBottom.IsChecked = true;
        }
        else
        {
            BtnSortBottomTop.IsChecked = true;
        }
    }

    /// <summary>
    /// Occurs when the page is loaded
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        AccountInfoChanged(null, EventArgs.Empty);
        await _controller.RunRepeatTransactionsAsync();
    }

    /// <summary>
    /// Occurs when the account information is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void AccountInfoChanged(object? sender, EventArgs e)
    {
        if(!_isAccountLoading)
        {
            _isAccountLoading = true;
            //Overview
            LblTotalAmount.Text = _controller.AccountTotalString;
            LblTotalAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 28, 113, 216) : Color.FromArgb(255, 120, 174, 237));
            LblIncomeAmount.Text = _controller.AccountIncomeString;
            LblIncomeAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 38, 162, 105) : Color.FromArgb(255, 143, 240, 164));
            LblExpenseAmount.Text = _controller.AccountExpenseString;
            LblExpenseAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 192, 28, 40) : Color.FromArgb(255, 255, 123, 99));
            //Groups
            ListGroups.Items.Clear();
            //Ungrouped Row
            var ungroupedRow = new GroupRow(_controller.UngroupedGroup, _controller.Localizer, _controller.IsFilterActive(-1));
            ungroupedRow.FilterChanged += (sender, e) => _controller?.UpdateFilterValue(-1, e.Filter);
            ListGroups.Items.Add(ungroupedRow);
            //Other Group Rows
            var groups = _controller.Groups.Values.ToList();
            groups.Sort();
            foreach (var group in groups)
            {
                var groupRow = new GroupRow(group, _controller.Localizer, _controller.IsFilterActive((int)group.Id));
                groupRow.EditTriggered += EditGroup;
                groupRow.DeleteTriggered += DeleteGroup;
                groupRow.FilterChanged += UpdateGroupFilter;
                ListGroups.Items.Add(groupRow);
            }
            //Transactions
            ListTransactions.Items.Clear();
            if (_controller.Transactions.Count > 0)
            {
                //Highlight Days
                var datesInAccount = _controller.DatesInAccount;
                var displayedDays = Calendar.FindDescendants().Where(x => x is CalendarViewDayItem);
                foreach (CalendarViewDayItem displayedDay in displayedDays)
                {
                    if (datesInAccount.Contains(DateOnly.FromDateTime(displayedDay.Date.Date)))
                    {
                        displayedDay.Background = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);
                    }
                    else
                    {
                        displayedDay.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    }
                }
                var filteredTransactions = _controller.FilteredTransactions;
                if (filteredTransactions.Count > 0)
                {
                    foreach (var transaction in filteredTransactions)
                    {
                        var transactionRow = new TransactionRow(transaction, ColorHelpers.FromRGBA(_controller.TransactionDefaultColor) ?? Color.FromArgb(255, 0, 0, 0), _controller.Localizer);
                        transactionRow.EditTriggered += EditTransaction;
                        transactionRow.DeleteTriggered += DeleteTransaction;
                        if (_controller.SortFirstToLast)
                        {
                            ListTransactions.Items.Add(transactionRow);

                        }
                        else
                        {
                            ListTransactions.Items.Insert(0, transactionRow);
                        }
                    }
                    ViewStackTransactions.ChangePage("Transactions");
                }
                else
                {
                    ViewStackTransactions.ChangePage("NoTransactions");
                    StatusPageNoTransactions.Glyph = "\xE721";
                    StatusPageNoTransactions.Title = _controller.Localizer["NoTransactionsTitle", "Filter"];
                    StatusPageNoTransactions.Description = _controller.Localizer["NoTransactionsDescription", "Filter"];
                }
            }
            else
            {
                ViewStackTransactions.ChangePage("NoTransactions");
                StatusPageNoTransactions.Glyph = "\xE152";
                StatusPageNoTransactions.Title = _controller.Localizer["NoTransactionsTitle"];
                StatusPageNoTransactions.Description = _controller.Localizer["NoTransactionsDescription"];
            }
            _isAccountLoading = false;
        }
    }

    /// <summary>
    /// Occurs when the new transaction button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void NewTransaction(object? sender, RoutedEventArgs e)
    {
        using var transactionController = _controller.CreateTransactionDialogController();
        var transactionDialog = new TransactionDialog(transactionController, _initializeWithWindow)
        {
            XamlRoot = Content.XamlRoot
        };
        if(await transactionDialog.ShowAsync())
        {
            await _controller.AddTransactionAsync(transactionController.Transaction);
        }
    }

    /// <summary>
    /// Occurs when the edit transaction action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the transaction to be edited</param>
    private async void EditTransaction(object? sender, uint transactionId)
    {
        using var transactionController = _controller.CreateTransactionDialogController(transactionId);
        var transactionDialog = new TransactionDialog(transactionController, _initializeWithWindow)
        {
            XamlRoot = Content.XamlRoot
        };
        if (await transactionDialog.ShowAsync())
        {
            await _controller.UpdateTransactionAsync(transactionController.Transaction);
        }
    }

    /// <summary>
    /// Occurs when the delete transaction action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the transaction to be deleted</param>
    private async void DeleteTransaction(object? sender, uint transactionId)
    {
        var deleteDialog = new ContentDialog()
        {
            Title = _controller.Localizer["DeleteTransaction"],
            Content = _controller.Localizer["DeleteTransactionDescription"],
            CloseButtonText = _controller.Localizer["No"],
            PrimaryButtonText = _controller.Localizer["Yes"],
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        if (await deleteDialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await _controller.DeleteTransactionAsync(transactionId);
        }
    }

    /// <summary>
    /// Occurs when the new group button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void NewGroup(object? sender, RoutedEventArgs e)
    {
        var groupController = _controller.CreateGroupDialogController();
        var groupDialog = new GroupDialog(groupController)
        {
            XamlRoot = Content.XamlRoot
        };
        if(await groupDialog.ShowAsync())
        {
            await _controller.AddGroupAsync(groupController.Group);
        }
    }

    /// <summary>
    /// Occurs when the edit group action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the group to be edited</param>
    private async void EditGroup(object? sender, uint groupId)
    {
        var groupController = _controller.CreateGroupDialogController(groupId);
        var groupDialog = new GroupDialog(groupController)
        {
            XamlRoot = Content.XamlRoot
        };
        if(await groupDialog.ShowAsync())
        {
            await _controller.UpdateGroupAsync(groupController.Group);
        }
    }

    /// <summary>
    /// Occurs when the delete group action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the group to be deleted</param>
    private async void DeleteGroup(object? sender, uint groupId)
    {
        var deleteDialog = new ContentDialog()
        {
            Title = _controller.Localizer["DeleteGroup"],
            Content = _controller.Localizer["DeleteGroupDescription"],
            CloseButtonText = _controller.Localizer["No"],
            PrimaryButtonText = _controller.Localizer["Yes"],
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };
        if(await deleteDialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await _controller.DeleteGroupAsync(groupId);
        }
    }

    /// <summary>
    /// Occurs when the group filter is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">The id of the group who's filter changed and whether to filter or not</param>
    private void UpdateGroupFilter(object? sender, (int Id, bool Filter) e) => _controller?.UpdateFilterValue(e.Id, e.Filter);

    /// <summary>
    /// Occurs when the transfer money button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void TransferMoney(object? sender, RoutedEventArgs e)
    {
        if(_controller.AccountTotal > 0)
        {
            var transferController = _controller.CreateTransferDialogController();
            var transferDialog = new TransferDialog(transferController, _initializeWithWindow)
            {
                XamlRoot = Content.XamlRoot
            };
            if (await transferDialog.ShowAsync())
            {
                await _controller.SendTransferAsync(transferController.Transfer);
            }
        }
        else
        {
            _controller.SendNotification(_controller.Localizer["NoMoneyToTransfer"], NotificationSeverity.Error);
        }
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
            await _controller.ImportFromFileAsync(file.Path);
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
        fileSavePicker.FileTypeChoices.Add("PDF", new List<string>() { ".pdf" });
        fileSavePicker.SuggestedFileName = _controller.AccountTitle;
        fileSavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileSavePicker.PickSaveFileAsync();
        if (file != null)
        {
            _controller.ExportToFile(file.Path);
        }
    }

    /// <summary>
    /// Occurs when the show hide groups button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ShowHideGroups(object? sender, RoutedEventArgs e)
    {
        if(SectionGroups.Visibility == Visibility.Visible)
        {
            SectionGroups.Visibility = Visibility.Collapsed;
            DockPanel.SetDock(SectionCalendar, Dock.Top);
            BtnShowHideGroups.Label = _controller.Localizer["ShowGroups"];
            BtnShowHideGroups.Icon = new FontIcon() { FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], Glyph = "\uE7B3" };
        }
        else
        {
            SectionGroups.Visibility = Visibility.Visible;
            DockPanel.SetDock(SectionCalendar, Dock.Bottom);
            BtnShowHideGroups.Label = _controller.Localizer["HideGroups"];
            BtnShowHideGroups.Icon = new FontIcon() { FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], Glyph = "\uED1A" };
        }
    }

    /// <summary>
    /// Occurs when the reset overview filters menu item is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ResetOverviewFilters(object? sender, RoutedEventArgs e)
    {
        if(!(ChkFilterIncome.IsChecked ?? false))
        {
            ChkFilterIncome.IsChecked = true;
        }
        if (!(ChkFilterExpense.IsChecked ?? false))
        {
            ChkFilterExpense.IsChecked = true;
        }
    }

    /// <summary>
    /// Occurs when the reset group filters menu item is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ResetGroupsFilters(object? sender, RoutedEventArgs e) => _controller.ResetGroupsFilter();

    /// <summary>
    /// Occurs when the reset dates filters menu item is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ResetDatesFilters(object? sender, RoutedEventArgs e)
    {
        Calendar.SelectedDates.Clear();
        Calendar.SelectedDates.Add(DateTimeOffset.Now);
        DateRangeStart.Date = DateTimeOffset.Now;
        DateRangeEnd.Date = DateTimeOffset.Now;
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
    /// Occurs when the calendar's selected date is changed
    /// </summary>
    /// <param name="sender">CalendarView</param>
    /// <param name="e">CalendarViewSelectedDatesChangedEventArgs</param>
    private void Calendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
    {
        if(Calendar.SelectedDates.Count == 1)
        {
            _controller.SetSingleDateFilter(DateOnly.FromDateTime(Calendar.SelectedDates[0].Date));
        }
    }

    /// <summary>
    /// Occurs when the start date range's date is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">DatePickerValueChangedEventArgs</param>
    private void DateRangeStart_DateChanged(object sender, DatePickerValueChangedEventArgs e) => _controller.FilterStartDate = DateOnly.FromDateTime(DateRangeStart.Date.Date);

    /// <summary>
    /// Occurs when the end date range's date is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">DatePickerValueChangedEventArgs</param>
    private void DateRangeEnd_DateChanged(object sender, DatePickerValueChangedEventArgs e) => _controller.FilterEndDate = DateOnly.FromDateTime(DateRangeEnd.Date.Date);

    /// <summary>
    /// Occurs when the ListGroups' selection is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void ListGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(ListGroups.SelectedIndex != -1)
        {
            var groupRow = (GroupRow)ListGroups.SelectedItem;
            if(groupRow.Id != 0)
            {
                EditGroup(null, groupRow.Id);
            }
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

    /// <summary>
    /// Occurs when the sort top to bottom button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void BtnSortTopBottom_Click(object sender, RoutedEventArgs e)
    {
        BtnSortTopBottom.IsChecked = true;
        BtnSortBottomTop.IsChecked = false;
        _controller.SortFirstToLast = true;
    }

    /// <summary>
    /// Occurs when the sort bottom to top button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void BtnSortBottomTop_Click(object sender, RoutedEventArgs e)
    {
        BtnSortTopBottom.IsChecked = false;
        BtnSortBottomTop.IsChecked = true;
        _controller.SortFirstToLast = false;
    }
}
