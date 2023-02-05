using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Controls;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// The AccountView for the application
/// </summary>
public sealed partial class AccountView : UserControl, INotifyPropertyChanged
{
    private readonly AccountViewController _controller;
    private readonly Action<string, string> _updateNavViewItemTitle;
    private readonly Action<object> _initializeWithWindow;
    private bool _isOpened;
    private double _transactionRowWidth;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Constructs an AccountView
    /// </summary>
    /// <param name="controller">The AccountViewController</param>
    /// <param name="updateNavViewItemTitle">The Action<string, string> callback for updating a nav view item title</param>
    /// <param name="initializeWithWindow">The Action<object> callback for InitializeWithWindow</param>
    public AccountView(AccountViewController controller, Action<string, string> updateNavViewItemTitle, Action<object> initializeWithWindow)
    {
        InitializeComponent();
        _controller = controller;
        _updateNavViewItemTitle = updateNavViewItemTitle;
        _initializeWithWindow = initializeWithWindow;
        _isOpened = false;
        _transactionRowWidth = -1;
        //Localize Strings
        LblBtnNew.Text = _controller.Localizer["New"];
        MenuNewTransaction.Text = _controller.Localizer["Transaction"];
        MenuContextNewTransaction.Text = _controller.Localizer["NewTransaction"];
        MenuNewGroup.Text = _controller.Localizer["Group"];
        MenuContextNewGroup.Text = _controller.Localizer["NewGroup"];
        MenuTransferMoney.Text = _controller.Localizer["Transfer"];
        MenuContextTransferMoney.Text = _controller.Localizer["TransferMoney"];
        ToolTipService.SetToolTip(BtnActions, _controller.Localizer["AccountActions"]);
        MenuImportFromFile.Text = _controller.Localizer["ImportFromFile"];
        MenuExportToFile.Text = _controller.Localizer["ExportToFile"];
        ToolTipService.SetToolTip(BtnResetOverviewFilters, _controller.Localizer["ResetFilters", "Overview"]);
        ToolTipService.SetToolTip(BtnResetGroupsFilters, _controller.Localizer["ResetFilters", "Groups"]);
        ToolTipService.SetToolTip(BtnResetDatesFilters, _controller.Localizer["ResetFilters", "Dates"]);
        MenuAccountSettings.Text = _controller.Localizer["AccountSettings"];
        TxtSearchDescription.PlaceholderText = _controller.Localizer["SearchDescription", "Placeholder"];
        LblOverview.Text = _controller.Localizer["Overview", "Today"];
        LblTotalTitle.Text = $"{_controller.Localizer["Total"]}:";
        LblIncomeTitle.Text = $"{_controller.Localizer["Income"]}:";
        LblExpenseTitle.Text = $"{_controller.Localizer["Expense"]}:";
        LblGroups.Text = _controller.Localizer["Groups"];
        LblCalendar.Text = _controller.Localizer["Calendar"];
        ExpDateRange.Header = _controller.Localizer["SelectRange"];
        DateRangeStart.Header = _controller.Localizer["Start", "DateRange"];
        DateRangeEnd.Header = _controller.Localizer["End", "DateRange"];
        LblTransactions.Text = _controller.Localizer["Transactions"];
        CmbSortTransactionsBy.Items.Add(_controller.Localizer["SortBy", "Id"]);
        CmbSortTransactionsBy.Items.Add(_controller.Localizer["SortBy", "Date"]);
        CmbSortTransactionsBy.Items.Add(_controller.Localizer["SortBy", "Amount"]);
        ToolTipService.SetToolTip(BtnSortTopBottom, _controller.Localizer["SortFirstLast"]);
        ToolTipService.SetToolTip(BtnSortBottomTop, _controller.Localizer["SortLastFirst"]);
        //Register Events
        _controller.AccountTransactionsChanged += AccountTransactionsChanged;
        _controller.UICreateGroupRow = CreateGroupRow;
        _controller.UIDeleteGroupRow = DeleteGroupRow;
        _controller.UICreateTransactionRow = CreateTransactionRow;
        _controller.UIMoveTransactionRow = MoveTransactionRow;
        _controller.UIDeleteTransactionRow = DeleteTransactionRow;
    }

    /// <summary>
    /// The width for transaction rows
    /// </summary>
    public double TransactionRowWidth
    {
        get => _transactionRowWidth;

        set
        {
            _transactionRowWidth = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Creates a group row and adds it to the view
    /// </summary>
    /// <param name="group">The Group model</param>
    /// <param name="index">The optional index to insert</param>
    /// <returns>The IGroupRowControl</returns>
    private IGroupRowControl CreateGroupRow(Group group, int? index)
    {
        var row = new GroupRow(group, _controller.CultureForNumberString, _controller.CultureForDateString, _controller.Localizer, _controller.IsFilterActive(group.Id == 0 ? -1 : (int)group.Id));
        row.EditTriggered += EditGroup;
        row.DeleteTriggered += DeleteGroup;
        row.FilterChanged += UpdateGroupFilter;
        if (index != null)
        {
            ListGroups.Items.Insert(index.Value, row);
        }
        else
        {
            ListGroups.Items.Add(row);
        }
        return row;
    }

    /// <summary>
    /// Removes a group row from the view
    /// </summary>
    /// <param name="row">The IGroupRowControl</param>
    private void DeleteGroupRow(IGroupRowControl row) => ListGroups.Items.Remove(row);

    /// <summary>
    /// Creates a transaction row and adds it to the view
    /// </summary>
    /// <param name="transaction">The Transaction model</param>
    /// <param name="index">The optional index to insert</param>
    /// <returns>The IModelRowControl<Transaction></returns>
    private IModelRowControl<Transaction> CreateTransactionRow(Transaction transaction, int? index)
    {
        ViewStackTransactions.ChangePage("Transactions");
        var row = new TransactionRow(transaction, _controller.CultureForNumberString, _controller.CultureForDateString, ColorHelpers.FromRGBA(_controller.TransactionDefaultColor) ?? Color.FromArgb(255, 0, 0, 0), _controller.Localizer);
        row.EditTriggered += EditTransaction;
        row.DeleteTriggered += DeleteTransaction;
        if (index != null)
        {
            ListTransactions.Items.Insert(index.Value, row);
            ListTransactions.UpdateLayout();
            row.Container = (GridViewItem)ListTransactions.ContainerFromIndex(index.Value);
            row.UpdateLayout();
        }
        else
        {
            ListTransactions.Items.Add(row);
            ListTransactions.UpdateLayout();
            row.Container = (GridViewItem)ListTransactions.ContainerFromIndex(ListTransactions.Items.Count - 1);
            row.UpdateLayout();
        }
        if (row.ActualWidth > TransactionRowWidth)
        {
            TransactionRowWidth = row.ActualWidth;
        }
        row.SetBinding(WidthProperty, new Binding()
        {
            Source = this,
            Path = new PropertyPath("TransactionRowWidth"),
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        return row;
    }

    /// <summary>
    /// Moves a row in the list
    /// </summary>
    /// <param name="row">The row to move</param>
    /// <param name="index">The new position</param>
    private void MoveTransactionRow(IModelRowControl<Transaction> row, int index)
    {
        if (ListTransactions.Items[index] != row)
        {
            var oldVisibility = ((GridViewItem)ListTransactions.ContainerFromItem(row)).Visibility;
            ListTransactions.Items.Remove(row);
            ListTransactions.Items.Insert(index, row);
            ListTransactions.UpdateLayout();
            ((TransactionRow)row).Container = (GridViewItem)ListTransactions.ContainerFromIndex(index);
            if (oldVisibility == Visibility.Visible)
            {
                row.Show();
            }
            else
            {
                row.Hide();
            }
        }
    }

    /// <summary>
    /// Removes a transaction row from the view
    /// </summary>
    /// <param name="row">The IModelRowControl<Transaction></param>
    private void DeleteTransactionRow(IModelRowControl<Transaction> row) => ListTransactions.Items.Remove(row);

    /// <summary>
    /// Occurs when the page is loaded
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_isOpened)
        {
            //Start Loading
            LoadingCtrl.IsLoading = true;
            //Work
            await Task.Delay(50);
            await _controller.StartupAsync();
            if (_controller.AccountNeedsSetup)
            {
                AccountSettings(null, new RoutedEventArgs());
            }
            //Setup Transactions
            ListTransactions.UpdateLayout();
            for (var i = 0; i < ListTransactions.Items.Count; i++)
            {
                ((TransactionRow)ListTransactions.Items[i]).Container = (GridViewItem)ListTransactions.ContainerFromIndex(i);
            }
            ScrollSidebar.Height = ActualHeight - 60;
            GridSidebar.Margin = new Thickness(0, 0, ScrollSidebar.Height < GridSidebar.ActualHeight ? 14 : 0, 0);
            //Setup Other UI Elements
            DateRangeStart.Date = DateTimeOffset.Now;
            DateRangeEnd.Date = DateTimeOffset.Now;
            if (_controller.ShowGroupsList)
            {
                SectionGroups.Visibility = Visibility.Visible;
                DockPanel.SetDock(SectionCalendar, Dock.Bottom);
                MenuShowHideGrous.Text = _controller.Localizer["HideGroups"];
                MenuShowHideGrous.Icon = new FontIcon() { FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], Glyph = "\uED1A" };
            }
            else
            {
                SectionGroups.Visibility = Visibility.Collapsed;
                DockPanel.SetDock(SectionCalendar, Dock.Top);
                MenuShowHideGrous.Text = _controller.Localizer["ShowGroups"];
                MenuShowHideGrous.Icon = new FontIcon() { FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], Glyph = "\uE7B3" };
            }
            CmbSortTransactionsBy.SelectedIndex = (int)_controller.SortTransactionsBy;
            if (_controller.SortFirstToLast)
            {
                BtnSortTopBottom.IsChecked = true;
            }
            else
            {
                BtnSortBottomTop.IsChecked = true;
            }
            //Done Loading
            LoadingCtrl.IsLoading = false;
            _isOpened = true;
        }
    }

    /// <summary>
    /// Occurs when the page size is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ScrollSidebar.Height = e.NewSize.Height < 60 ? 60 : e.NewSize.Height - 60;
        GridSidebar.Margin = new Thickness(0, 0, ScrollSidebar.Height < GridSidebar.ActualHeight ? 14 : 0, 0);
    }

    /// <summary>
    /// Occurs when the NavView's pane is opened
    /// </summary>
    /// <param name="sender">NavigationView</param>
    /// <param name="args">object</param>
    private void NavView_PaneOpened(NavigationView sender, object args) => ScrollSidebar.Margin = new Thickness(6, 0, 0, 0);

    /// <summary>
    /// Occurs when the NavView's pane is closed
    /// </summary>
    /// <param name="sender">NavigationView</param>
    /// <param name="args">object</param>
    private void NavView_PaneClosed(NavigationView sender, object args) => ScrollSidebar.Margin = new Thickness(60, 0, 0, 0);

    /// <summary>
    /// Occurs when the account's transactions are changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void AccountTransactionsChanged(object? sender, EventArgs e)
    {
        //Overview
        _updateNavViewItemTitle(_controller.AccountPath, _controller.AccountTitle);
        LblTotalAmount.Text = _controller.AccountTodayTotalString;
        LblTotalAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 28, 113, 216) : Color.FromArgb(255, 120, 174, 237));
        LblIncomeAmount.Text = _controller.AccountTodayIncomeString;
        LblIncomeAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 38, 162, 105) : Color.FromArgb(255, 143, 240, 164));
        LblExpenseAmount.Text = _controller.AccountTodayExpenseString;
        LblExpenseAmount.Foreground = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 192, 28, 40) : Color.FromArgb(255, 255, 123, 99));
        ///Transactions
        if (_controller.TransactionsCount > 0)
        {
            //Highlight Days
            var datesInAccount = _controller.DatesInAccount;
            var displayedDays = Calendar.FindDescendants().Where(x => x is CalendarViewDayItem);
            foreach (CalendarViewDayItem displayedDay in displayedDays)
            {
                if (datesInAccount.Contains(DateOnly.FromDateTime(displayedDay.Date.Date)) && displayedDay.Date.Date != DateTime.Today)
                {
                    displayedDay.Background = new SolidColorBrush(ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 143, 240, 164) : Color.FromArgb(255, 38, 162, 105));
                }
                else
                {
                    displayedDay.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                }
            }
            //Transaction Page
            if (_controller.HasFilteredTransactions)
            {
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
        //No Transactions
        else
        {
            ViewStackTransactions.ChangePage("NoTransactions");
            StatusPageNoTransactions.Glyph = "\xE152";
            StatusPageNoTransactions.Title = _controller.Localizer["NoTransactionsTitle"];
            StatusPageNoTransactions.Description = _controller.Localizer["NoTransactionsDescription"];
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
        if (await transactionDialog.ShowAsync())
        {
            //Start Loading
            LoadingCtrl.IsLoading = true;
            //Work
            await Task.Delay(50);
            await _controller.AddTransactionAsync(transactionController.Transaction);
            //Done Loading
            LoadingCtrl.IsLoading = false;
        }
    }

    /// <summary>
    /// Occurs when creation of transaction copy was requested
    /// </summary>
    /// <param name="source">Source transaction for copy</param>
    private async Task CopyTransactionAsync(Transaction source)
    {
        using var transactionController = _controller.CreateTransactionDialogController(source);
        var transactionDialog = new TransactionDialog(transactionController, _initializeWithWindow)
        {
            XamlRoot = Content.XamlRoot
        };
        if (await transactionDialog.ShowAsync())
        {
            //Start Loading
            LoadingCtrl.IsLoading = true;
            //Work
            await Task.Delay(50);
            await _controller.AddTransactionAsync(transactionController.Transaction);
            //Done Loading
            LoadingCtrl.IsLoading = false;
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
            if (transactionController.CopyRequested)
            {
                await CopyTransactionAsync(transactionController.Transaction);
                return;
            }
            if (_controller.GetIsSourceRepeatTransaction(transactionId) && transactionController.OriginalRepeatInterval != TransactionRepeatInterval.Never)
            {
                if (transactionController.OriginalRepeatInterval != transactionController.Transaction.RepeatInterval)
                {
                    var editDialog = new ContentDialog()
                    {
                        Title = _controller.Localizer["RepeatIntervalChanged"],
                        Content = _controller.Localizer["RepeatIntervalChangedDescription"],
                        CloseButtonText = _controller.Localizer["Cancel"],
                        PrimaryButtonText = _controller.Localizer["DeleteExisting"],
                        SecondaryButtonText = _controller.Localizer["DisassociateExisting"],
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = Content.XamlRoot
                    };
                    var result = await editDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        //Start Loading
                        LoadingCtrl.IsLoading = true;
                        //Work
                        await Task.Delay(50);
                        await _controller.DeleteGeneratedTransactionsAsync(transactionId);
                        await _controller.UpdateTransactionAsync(transactionController.Transaction);
                        //Done Loading
                        LoadingCtrl.IsLoading = false;
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        //Start Loading
                        LoadingCtrl.IsLoading = true;
                        //Work
                        await Task.Delay(50);
                        await _controller.UpdateSourceTransactionAsync(transactionController.Transaction, false);
                        //Done Loading
                        LoadingCtrl.IsLoading = false;
                    }
                }
                else
                {
                    var editDialog = new ContentDialog()
                    {
                        Title = _controller.Localizer["EditTransaction", "SourceRepeat"],
                        Content = _controller.Localizer["EditTransactionDescription", "SourceRepeat"],
                        CloseButtonText = _controller.Localizer["Cancel"],
                        PrimaryButtonText = _controller.Localizer["EditSourceGeneratedTransaction"],
                        SecondaryButtonText = _controller.Localizer["EditOnlySourceTransaction"],
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = Content.XamlRoot
                    };
                    var result = await editDialog.ShowAsync();
                    if (result != ContentDialogResult.None)
                    {
                        //Start Loading
                        LoadingCtrl.IsLoading = true;
                        //Work
                        await Task.Delay(50);
                        await _controller.UpdateSourceTransactionAsync(transactionController.Transaction, result == ContentDialogResult.Primary);
                        //Done Loading
                        LoadingCtrl.IsLoading = false;
                    }
                }
            }
            else
            {
                //Start Loading
                LoadingCtrl.IsLoading = true;
                //Work
                await Task.Delay(50);
                await _controller.UpdateTransactionAsync(transactionController.Transaction);
                //Done Loading
                LoadingCtrl.IsLoading = false;
            }
        }
    }

    /// <summary>
    /// Occurs when the delete transaction action is triggered
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="groupId">The id of the transaction to be deleted</param>
    private async void DeleteTransaction(object? sender, uint transactionId)
    {
        if (_controller.GetIsSourceRepeatTransaction(transactionId))
        {
            var deleteDialog = new ContentDialog()
            {
                Title = _controller.Localizer["DeleteTransaction", "SourceRepeat"],
                Content = _controller.Localizer["DeleteTransactionDescription", "SourceRepeat"],
                CloseButtonText = _controller.Localizer["Cancel"],
                PrimaryButtonText = _controller.Localizer["DeleteSourceGeneratedTransaction"],
                SecondaryButtonText = _controller.Localizer["DeleteOnlySourceTransaction"],
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = Content.XamlRoot
            };
            var result = await deleteDialog.ShowAsync();
            if (result != ContentDialogResult.None)
            {
                //Start Loading
                LoadingCtrl.IsLoading = true;
                //Work
                await Task.Delay(50);
                await _controller.DeleteSourceTransactionAsync(transactionId, result == ContentDialogResult.Primary);
                //Done Loading
                LoadingCtrl.IsLoading = false;
            }
        }
        else
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
        if (await groupDialog.ShowAsync())
        {
            //Start Loading
            LoadingCtrl.IsLoading = true;
            //Work
            await Task.Delay(50);
            await _controller.AddGroupAsync(groupController.Group);
            //Done Loading
            LoadingCtrl.IsLoading = false;
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
        if (await groupDialog.ShowAsync())
        {
            //Start Loading
            LoadingCtrl.IsLoading = true;
            //Work
            await Task.Delay(50);
            await _controller.UpdateGroupAsync(groupController.Group);
            //Done Loading
            LoadingCtrl.IsLoading = false;
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
        if (await deleteDialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await _controller.DeleteGroupAsync(groupId);
        }
    }

    /// <summary>
    /// Occurs when the group filter is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">The id of the group who's filter changed and whether to filter or not</param>
    private void UpdateGroupFilter(object? sender, (uint Id, bool Filter) e) => _controller?.UpdateFilterValue(e.Id == 0 ? -1 : (int)e.Id, e.Filter);

    /// <summary>
    /// Occurs when the transfer money button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void TransferMoney(object? sender, RoutedEventArgs e)
    {
        if (_controller.AccountTodayTotal > 0)
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
            //Start Loading
            LoadingCtrl.IsLoading = true;
            //Work
            await Task.Delay(50);
            await _controller.ImportFromFileAsync(file.Path);
            //Done Loading
            LoadingCtrl.IsLoading = false;
        }
    }

    /// <summary>
    /// Occurs when the export to csv menu item is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ExportToCSV(object? sender, RoutedEventArgs e)
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
    /// Occurs when the export to pdf menu item is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ExportToPDF(object? sender, RoutedEventArgs e)
    {
        var fileSavePicker = new FileSavePicker();
        _initializeWithWindow(fileSavePicker);
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
        if (SectionGroups.Visibility == Visibility.Visible)
        {
            SectionGroups.Visibility = Visibility.Collapsed;
            RowSidebarGroups.Height = new GridLength(0, GridUnitType.Pixel);
            MenuShowHideGrous.Text = _controller.Localizer["ShowGroups"];
            MenuShowHideGrous.Icon = new FontIcon() { FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], Glyph = "\uE7B3" };
        }
        else
        {
            SectionGroups.Visibility = Visibility.Visible;
            RowSidebarGroups.Height = new GridLength(1, GridUnitType.Star);
            MenuShowHideGrous.Text = _controller.Localizer["HideGroups"];
            MenuShowHideGrous.Icon = new FontIcon() { FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], Glyph = "\uED1A" };
        }
        _controller.ShowGroupsList = SectionGroups.Visibility == Visibility.Visible;
    }

    /// <summary>
    /// Occurs when the reset overview filters menu item is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ResetOverviewFilters(object? sender, RoutedEventArgs e)
    {
        if (!(ChkFilterIncome.IsChecked ?? false))
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
    /// Occurs when the account settings button is clicked
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void AccountSettings(object? sender, RoutedEventArgs e)
    {
        var accountSettingsController = _controller.CreateAccountSettingsDialogController();
        var accountSettingsDialog = new AccountSettingsDialog(accountSettingsController)
        {
            XamlRoot = Content.XamlRoot
        };
        if (await accountSettingsDialog.ShowAsync())
        {
            _controller.UpdateMetadata(accountSettingsController.Metadata);
            if (accountSettingsController.NewPassword != null)
            {
                //Start Loading
                LoadingCtrl.IsLoading = true;
                //Work
                await Task.Delay(50);
                await Task.Run(async () => await App.MainWindow!.DispatcherQueue.EnqueueAsync(() => _controller.SetPassword(accountSettingsController.NewPassword)));
                //Done Loading
                LoadingCtrl.IsLoading = false;
            }
        }
    }

    /// <summary>
    /// Occurs when the search description textbox is changed
    /// </summary>
    /// <param name="sender">AutoSuggestBox</param>
    /// <param name="args">AutoSuggestBoxTextChangedEventArgs</param>
    private void TxtSearchDescription_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => _controller.SearchDescription = TxtSearchDescription.Text;

    /// <summary>
    /// Occurs when the income filter checkbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ChkFilterIncome_Changed(object sender, RoutedEventArgs e) => _controller?.UpdateFilterValue(-3, ChkFilterIncome.IsChecked ?? false);

    /// <summary>
    /// Occurs when the expense filter checkbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ChkFilterExpense_Changed(object sender, RoutedEventArgs e) => _controller?.UpdateFilterValue(-2, ChkFilterExpense.IsChecked ?? false);

    /// <summary>
    /// Occurs when the calendar's selected date is changed
    /// </summary>
    /// <param name="sender">CalendarView</param>
    /// <param name="e">CalendarViewSelectedDatesChangedEventArgs</param>
    private void Calendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
    {
        if (Calendar.SelectedDates.Count == 1)
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
        if (ListGroups.SelectedIndex != -1)
        {
            var groupRow = (GroupRow)ListGroups.SelectedItem;
            groupRow.Edit(this, new RoutedEventArgs());
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
            var transactionRow = (TransactionRow)ListTransactions.SelectedItem;
            transactionRow.Edit(this, new RoutedEventArgs());
            ListTransactions.SelectedIndex = -1;
        }
    }

    /// <summary>
    /// Occurs when the sort transactions by combobox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbSortTransactionsBy_SelectionChanged(object sender, SelectionChangedEventArgs e) => _controller.SortTransactionsBy = (SortBy)CmbSortTransactionsBy.SelectedIndex;

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

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
