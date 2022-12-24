using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The AccountView for the application
/// </summary>
public partial class AccountView
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MoneyDateTime
    {
        UInt64 Usec;
        nint Tz;
        int Interval;
        Int32 Days;
        int RefCount;
    };

    private delegate void SignalCallback(nint gObject, nint gParamSpec, nint data);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)] SignalCallback c_handler, nint data, nint destroy_data, int connect_flags);

    [DllImport("adwaita-1")]
    private static extern ref MoneyDateTime gtk_calendar_get_date(nint calendar);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_calendar_select_day(nint calendar, ref MoneyDateTime datetime);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_year(ref MoneyDateTime datetime);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_month(ref MoneyDateTime datetime);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_day_of_month(ref MoneyDateTime datetime);

    [DllImport("adwaita-1")]
    private static extern ref MoneyDateTime g_date_time_add_years(ref MoneyDateTime datetime, int years);

    [DllImport("adwaita-1")]
    private static extern ref MoneyDateTime g_date_time_new_now_local();

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial uint g_list_model_get_n_items(nint list);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool may_block);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    private readonly AccountViewController _controller;
    private bool _isFirstTimeLoading;
    private bool _isAccountLoading;
    private List<GroupRow> _groupRows;
    private List<TransactionRow> _transactionRows;
    private readonly MainWindow _parentWindow;
    private readonly Adw.Flap _flap;
    private readonly Gtk.ScrolledWindow _scrollPane;
    private readonly Gtk.Box _paneBox;
    private readonly Gtk.Label _lblTotal;
    private readonly Adw.ActionRow _rowTotal;
    private readonly Gtk.Label _lblIncome;
    private readonly Gtk.CheckButton _chkIncome;
    private readonly Adw.ActionRow _rowIncome;
    private readonly Gtk.Label _lblExpense;
    private readonly Gtk.CheckButton _chkExpense;
    private readonly Adw.ActionRow _rowExpense;
    private readonly Gtk.Box _boxButtonsOverview;
    private readonly Gtk.MenuButton _btnMenuAccountActions;
    private readonly Adw.ButtonContent _btnMenuAccountActionsContent;
    private readonly Gtk.Button _btnResetOverviewFilter;
    private readonly Adw.PreferencesGroup _grpOverview;
    private readonly Gtk.Box _boxButtonsGroups;
    private readonly Gtk.Button _btnNewGroup;
    private readonly Adw.ButtonContent _btnNewGroupContent;
    private readonly Gtk.Button _btnResetGroupsFilter;
    private readonly Adw.PreferencesGroup _grpGroups;
    private readonly Gtk.Calendar _calendar;
    private readonly Gtk.Button _btnResetCalendarFilter;
    private readonly Gtk.DropDown _ddStartYear;
    private readonly Gtk.DropDown _ddStartMonth;
    private readonly Gtk.DropDown _ddStartDay;
    private readonly Gtk.DropDown _ddEndYear;
    private readonly Gtk.DropDown _ddEndMonth;
    private readonly Gtk.DropDown _ddEndDay;
    private readonly Gtk.Box _boxStartRange;
    private readonly Gtk.Box _boxEndRange;
    private readonly Adw.ActionRow _rowStartRange;
    private readonly Adw.ActionRow _rowEndRange;
    private readonly Adw.PreferencesGroup _grpRange;
    private readonly Adw.ExpanderRow _expRange;
    private readonly Adw.PreferencesGroup _grpCalendar;
    private readonly Gtk.Button _btnNewTransaction;
    private readonly Adw.ButtonContent _btnNewTransactionContent;
    private readonly Gtk.ToggleButton _btnSortFirstToLast;
    private readonly Gtk.ToggleButton _btnSortLastToFirst;
    private readonly Gtk.Box _boxSort;
    private readonly Adw.PreferencesGroup _grpTransactions;
    private readonly Gtk.FlowBox _flowBox;
    private readonly Gtk.ScrolledWindow _scrollTransactions;
    private readonly Adw.StatusPage _statusPageNoTransactions;
    private readonly Adw.Bin _binSpinner;
    private readonly Gtk.Spinner _spinner;
    private readonly Gtk.Box _boxMain;
    private readonly Gtk.Overlay _overlayMain;

    /// <summary>
    /// The Page widget
    /// </summary>
    public Adw.TabPage Page { get; init; }

    /// <summary>
    /// Constructs an AccountView
    /// </summary>
    /// <param name="controller">AccountViewController</param>
    /// <param name="parentWindow">MainWindow</param>
    /// <param name="parentTabView">Adw.TabView</param>
    /// <param name="btnFlapToggle">Gtk.ToggleButton</param>
    public AccountView(AccountViewController controller, MainWindow parentWindow, Adw.TabView parentTabView, Gtk.ToggleButton btnFlapToggle)
    {
        _parentWindow = parentWindow;
        _parentWindow.WidthChanged += OnWindowWidthChanged;
        _controller = controller;
        _isFirstTimeLoading = true;
        _isAccountLoading = false;
        _groupRows = new List<GroupRow> {};
        _transactionRows = new List<TransactionRow> {};
        //Register Controller Events
        _controller.AccountInfoChanged += OnAccountInfoChanged;
        //Flap
        _flap = Adw.Flap.New();
        btnFlapToggle.BindProperty("active", _flap, "reveal-flap", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate));
        //Left Pane
        _scrollPane = Gtk.ScrolledWindow.New();
        _scrollPane.AddCssClass("background");
        _scrollPane.SetSizeRequest(360, -1);
        _flap.SetFlap(_scrollPane);
        //Pane Box
        _paneBox = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        _paneBox.SetHexpand(false);
        _paneBox.SetVexpand(true);
        _paneBox.SetMarginTop(10);
        _paneBox.SetMarginStart(10);
        _paneBox.SetMarginEnd(10);
        _paneBox.SetMarginBottom(10);
        _scrollPane.SetChild(_paneBox);
        //Account Total
        _lblTotal = Gtk.Label.New("");
        _lblTotal.SetValign(Gtk.Align.Center);
        _lblTotal.AddCssClass("accent");
        _lblTotal.AddCssClass("money-total");
        _rowTotal = Adw.ActionRow.New();
        _rowTotal.SetTitle(_controller.Localizer["Total"]);
        _rowTotal.AddSuffix(_lblTotal);
        //Account Income
        _lblIncome = Gtk.Label.New("");
        _lblIncome.SetValign(Gtk.Align.Center);
        _lblIncome.AddCssClass("success");
        _lblTotal.AddCssClass("money-income");
        _chkIncome = Gtk.CheckButton.New();
        _chkIncome.SetActive(true);
        _chkIncome.AddCssClass("selection-mode");
        _chkIncome.OnToggled += (Gtk.CheckButton sender, EventArgs e) => _controller.UpdateFilterValue(-3, _chkIncome.GetActive());
        _rowIncome = Adw.ActionRow.New();
        _rowIncome.SetTitle(_controller.Localizer["Income"]);
        _rowIncome.AddPrefix(_chkIncome);
        _rowIncome.AddSuffix(_lblIncome);
        //Account Expense
        _lblExpense = Gtk.Label.New("");
        _lblExpense.SetValign(Gtk.Align.Center);
        _lblExpense.AddCssClass("error");
        _lblExpense.AddCssClass("money-expense");
        _chkExpense = Gtk.CheckButton.New();
        _chkExpense.SetActive(true);
        _chkExpense.AddCssClass("selection-mode");
        _chkExpense.OnToggled += (Gtk.CheckButton sender, EventArgs e) => _controller.UpdateFilterValue(-2, _chkExpense.GetActive());
        _rowExpense = Adw.ActionRow.New();
        _rowExpense.SetTitle(_controller.Localizer["Expense"]);
        _rowExpense.AddPrefix(_chkExpense);
        _rowExpense.AddSuffix(_lblExpense);
        //Overview Buttons Box
        _boxButtonsOverview = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        //Button Menu Account Actions
        _btnMenuAccountActions = Gtk.MenuButton.New();
        _btnMenuAccountActions.AddCssClass("flat");
        _btnMenuAccountActionsContent = Adw.ButtonContent.New();
        _btnMenuAccountActionsContent.SetIconName("document-properties-symbolic");
        _btnMenuAccountActionsContent.SetLabel(_controller.Localizer["AccountActions", "GTK"]);
        _btnMenuAccountActions.SetChild(_btnMenuAccountActionsContent);
        var menuActionsCsv = Gio.Menu.New();
        menuActionsCsv.Append(_controller.Localizer["ExportToFile"], "account.exportToFile");
        menuActionsCsv.Append(_controller.Localizer["ImportFromFile"], "account.importFromFile");
        var menuActions = Gio.Menu.New();
        menuActions.Append(_controller.Localizer["TransferMoney"], "account.transferMoney");
        menuActions.AppendSection(null, menuActionsCsv);
        _btnMenuAccountActions.SetMenuModel(menuActions);
        _boxButtonsOverview.Append(_btnMenuAccountActions);
        //Button Reset Overview Filter
        _btnResetOverviewFilter = Gtk.Button.NewFromIconName("larger-brush-symbolic");
        _btnResetOverviewFilter.AddCssClass("flat");
        _btnResetOverviewFilter.SetTooltipText(_controller.Localizer["ResetFilters", "Overview"]);
        _btnResetOverviewFilter.OnClicked += OnResetOverviewFilter;
        _boxButtonsOverview.Append(_btnResetOverviewFilter);
        //Overview Group
        _grpOverview = Adw.PreferencesGroup.New();
        _grpOverview.SetTitle(_controller.Localizer["Overview"]);
        _grpOverview.Add(_rowTotal);
        _grpOverview.Add(_rowIncome);
        _grpOverview.Add(_rowExpense);
        _grpOverview.SetHeaderSuffix(_boxButtonsOverview);
        _paneBox.Append(_grpOverview);
        //Group Buttons Box
        _boxButtonsGroups = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        //Button New Group
        _btnNewGroup = Gtk.Button.New();
        _btnNewGroup.AddCssClass("flat");
        _btnNewGroupContent = Adw.ButtonContent.New();
        _btnNewGroupContent.SetIconName("list-add-symbolic");
        _btnNewGroupContent.SetLabel(_controller.Localizer["NewGroup", "Short"]);
        _btnNewGroup.SetChild(_btnNewGroupContent);
        _btnNewGroup.SetTooltipText(_controller.Localizer["NewGroup", "Tooltip"]);
        _btnNewGroup.SetDetailedActionName("account.newGroup");
        _boxButtonsGroups.Append(_btnNewGroup);
        //Button Reset Groups Filter
        _btnResetGroupsFilter = Gtk.Button.NewFromIconName("larger-brush-symbolic");
        _btnResetGroupsFilter.AddCssClass("flat");
        _btnResetGroupsFilter.SetTooltipText(_controller.Localizer["ResetFilters", "Groups"]);
        _btnResetGroupsFilter.OnClicked += (Gtk.Button sender, EventArgs e) => _controller.ResetGroupsFilter();
        _boxButtonsGroups.Append(_btnResetGroupsFilter);
        //Groups Group
        _grpGroups = Adw.PreferencesGroup.New();
        _grpGroups.SetTitle(_controller.Localizer["Groups"]);
        _grpGroups.SetHeaderSuffix(_boxButtonsGroups);
        _paneBox.Append(_grpGroups);
        //Calendar Widget
        _calendar = Gtk.Calendar.New();
        _calendar.SetName("calendarAccount");
        _calendar.AddCssClass("card");
        _calendar.OnPrevMonth += OnCalendarMonthYearChanged;
        _calendar.OnPrevYear += OnCalendarMonthYearChanged;
        _calendar.OnNextMonth += OnCalendarMonthYearChanged;
        _calendar.OnNextYear += OnCalendarMonthYearChanged;
        _calendar.OnDaySelected += OnCalendarSelectedDateChanged;
        //Button Reset Calendar Filter
        _btnResetCalendarFilter = Gtk.Button.NewFromIconName("larger-brush-symbolic");
        _btnResetCalendarFilter.AddCssClass("flat");
        _btnResetCalendarFilter.SetTooltipText(_controller.Localizer["ResetFilters", "Dates"]);
        _btnResetCalendarFilter.OnClicked += OnResetCalendarFilter;
        //Start Range DropDowns
        _ddStartYear = Gtk.DropDown.NewFromStrings(new string[1] { "" });
        _ddStartYear.SetValign(Gtk.Align.Center);
        _ddStartYear.SetShowArrow(false);
        g_signal_connect_data(_ddStartYear.Handle, "notify::selected", (nint sender, nint gParamSpec, nint data) => OnDateRangeStartYearChanged(), IntPtr.Zero, IntPtr.Zero, 0);
        _ddStartMonth = Gtk.DropDown.NewFromStrings(new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
        _ddStartMonth.SetValign(Gtk.Align.Center);
        _ddStartMonth.SetShowArrow(false);
        g_signal_connect_data(_ddStartMonth.Handle, "notify::selected", (nint sender, nint gParamSpec, nint data) => OnDateRangeStartMonthChanged(), IntPtr.Zero, IntPtr.Zero, 0);
        _ddStartDay = Gtk.DropDown.NewFromStrings(Enumerable.Range(1, 31).Select(x => x.ToString()).ToArray());
        _ddStartDay.SetValign(Gtk.Align.Center);
        _ddStartDay.SetShowArrow(false);
        g_signal_connect_data(_ddStartDay.Handle, "notify::selected", (nint sender, nint gParamSpec, nint data) => OnDateRangeStartDayChanged(), IntPtr.Zero, IntPtr.Zero, 0);
        //End Range DropDowns
        _ddEndYear = Gtk.DropDown.NewFromStrings(new string[1] { "" });
        _ddEndYear.SetValign(Gtk.Align.Center);
        _ddEndYear.SetShowArrow(false);
        g_signal_connect_data(_ddEndYear.Handle, "notify::selected", (nint sender, nint gParamSpec, nint data) => OnDateRangeEndYearChanged(), IntPtr.Zero, IntPtr.Zero, 0);
        _ddEndMonth = Gtk.DropDown.NewFromStrings(new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
        _ddEndMonth.SetValign(Gtk.Align.Center);
        _ddEndMonth.SetShowArrow(false);
        g_signal_connect_data(_ddEndMonth.Handle, "notify::selected", (nint sender, nint gParamSpec, nint data) => OnDateRangeEndMonthChanged(), IntPtr.Zero, IntPtr.Zero, 0);
        _ddEndDay = Gtk.DropDown.NewFromStrings(Enumerable.Range(1, 31).Select(x => x.ToString()).ToArray());
        _ddEndDay.SetValign(Gtk.Align.Center);
        _ddEndDay.SetShowArrow(false);
        g_signal_connect_data(_ddEndDay.Handle, "notify::selected", (nint sender, nint gParamSpec, nint data) => OnDateRangeEndDayChanged(), IntPtr.Zero, IntPtr.Zero, 0);
        //Start Range Boxes
        _boxStartRange = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _boxStartRange.Append(_ddStartYear);
        _boxStartRange.Append(_ddStartMonth);
        _boxStartRange.Append(_ddStartDay);
        //End Range Boxes
        _boxEndRange = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _boxEndRange.Append(_ddEndYear);
        _boxEndRange.Append(_ddEndMonth);
        _boxEndRange.Append(_ddEndDay);
        //Start Range Row
        _rowStartRange = Adw.ActionRow.New();
        _rowStartRange.SetTitle(controller.Localizer["Start", "DateRange"]);
        _rowStartRange.AddSuffix(_boxStartRange);
        //End Range Row
        _rowEndRange = Adw.ActionRow.New();
        _rowEndRange.SetTitle(_controller.Localizer["End", "DateRange"]);
        _rowEndRange.AddSuffix(_boxEndRange);
        //Select Range Group
        _grpRange = Adw.PreferencesGroup.New();
        //Expander Row Select Range
        _expRange = Adw.ExpanderRow.New();
        _expRange.SetTitle(_controller.Localizer["SelectRange"]);
        _expRange.SetEnableExpansion(false);
        _expRange.SetShowEnableSwitch(true);
        _expRange.AddRow(_rowStartRange);
        _expRange.AddRow(_rowEndRange);
        g_signal_connect_data(_expRange.Handle, "notify::enable-expansion", (nint sender, nint gParamSpec, nint data) => OnDateRangeToggled(), IntPtr.Zero, IntPtr.Zero, 0);
        _grpRange.Add(_expRange);
        //Calendar Group
        _grpCalendar = Adw.PreferencesGroup.New();
        _grpCalendar.SetTitle(_controller.Localizer["Calendar"]);
        _grpCalendar.SetHeaderSuffix(_btnResetCalendarFilter);
        _grpCalendar.Add(_calendar);
        _paneBox.Append(_grpCalendar);
        _paneBox.Append(_grpRange);
        //Separator
        _flap.SetSeparator(Gtk.Separator.New(Gtk.Orientation.Vertical));
        //Button New Transaction
        _btnNewTransaction = Gtk.Button.New();
        _btnNewTransaction.AddCssClass("pill");
        _btnNewTransaction.AddCssClass("suggested-action");
        _btnNewTransactionContent = Adw.ButtonContent.New();
        _btnNewTransactionContent.SetIconName("list-add-symbolic");
        _btnNewTransactionContent.SetLabel(_controller.Localizer["NewTransaction", "Short"]);
        _btnNewTransaction.SetTooltipText(_controller.Localizer["NewTransaction", "Tooltip"]);
        _btnNewTransaction.SetChild(_btnNewTransactionContent);
        _btnNewTransaction.SetHalign(Gtk.Align.Center);
        _btnNewTransaction.SetValign(Gtk.Align.End);
        _btnNewTransaction.SetMarginBottom(10);
        _btnNewTransaction.SetDetailedActionName("account.newTransaction");
        //Sort Box And buttons
        _btnSortFirstToLast = Gtk.ToggleButton.New();
        _btnSortFirstToLast.SetIconName("view-sort-descending-symbolic");
        _btnSortFirstToLast.SetTooltipText(_controller.Localizer["SortFirstLast"]);
        _btnSortFirstToLast.OnToggled += (Gtk.ToggleButton sender, EventArgs e) => _controller.SortFirstToLast = _btnSortFirstToLast.GetActive();
        _btnSortLastToFirst = Gtk.ToggleButton.New();
        _btnSortLastToFirst.SetIconName("view-sort-ascending-symbolic");
        _btnSortLastToFirst.SetTooltipText(_controller.Localizer["SortLastFirst"]);
        _btnSortFirstToLast.BindProperty("active", _btnSortLastToFirst, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        _boxSort = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        _boxSort.AddCssClass("linked");
        _boxSort.SetValign(Gtk.Align.Center);
        _boxSort.Append(_btnSortFirstToLast);
        _boxSort.Append(_btnSortLastToFirst);
        //Transaction Group
        _grpTransactions = Adw.PreferencesGroup.New();
        _grpTransactions.SetTitle(_controller.Localizer["Transactions"]);
        _grpTransactions.SetHeaderSuffix(_boxSort);
        //Transactions Flow Box
        _flowBox = Gtk.FlowBox.New();
        _flowBox.SetHomogeneous(true);
        _flowBox.SetColumnSpacing(10);
        _flowBox.SetRowSpacing(10);
        _flowBox.SetMarginBottom(60);
        _flowBox.SetHalign(Gtk.Align.Fill);
        _flowBox.SetValign(Gtk.Align.Start);
        _flowBox.SetSelectionMode(Gtk.SelectionMode.None);
        //Transactions Scrolled Window
        _scrollTransactions = Gtk.ScrolledWindow.New();
        _scrollTransactions.SetSizeRequest(300, 360);
        _scrollTransactions.SetMinContentHeight(360);
        _scrollTransactions.SetVexpand(true);
        _scrollTransactions.SetChild(_flowBox);
        //Page No Transactions
        _statusPageNoTransactions = Adw.StatusPage.New();
        _statusPageNoTransactions.SetIconName("money-none-symbolic");
        _statusPageNoTransactions.SetVexpand(true);
        _statusPageNoTransactions.SetSizeRequest(300, 360);
        _statusPageNoTransactions.SetMarginBottom(60);
        //Loading Spinner
        _binSpinner = Adw.Bin.New();
        _binSpinner.SetHexpand(true);
        _binSpinner.SetVexpand(true);
        _spinner = Gtk.Spinner.New();
        _spinner.SetSizeRequest(42, 42);
        _spinner.SetHalign(Gtk.Align.Center);
        _spinner.SetValign(Gtk.Align.Center);
        _binSpinner.SetChild(_spinner);
        //Main Box
        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        _boxMain.SetHexpand(true);
        _boxMain.SetVexpand(true);
        _boxMain.SetMarginTop(10);
        _boxMain.SetMarginStart(10);
        _boxMain.SetMarginEnd(10);
        _boxMain.Append(_grpTransactions);
        _boxMain.Append(_scrollTransactions);
        _boxMain.Append(_statusPageNoTransactions);
        _boxMain.Append(_binSpinner);
        //Main Overlay
        _overlayMain = Gtk.Overlay.New();
        _overlayMain.SetVexpand(true);
        _overlayMain.SetChild(_boxMain);
        _overlayMain.AddOverlay(_btnNewTransaction);
        _flap.SetContent(_overlayMain);
        //Tab Page
        Page = parentTabView.Append(_flap);
        Page.SetTitle(_controller.AccountTitle);
        //Action Map
        var actionMap = Gio.SimpleActionGroup.New();
        _flap.InsertActionGroup("account", actionMap);
        //New Group Action
        var actNewGroup = Gio.SimpleAction.New("newGroup", null);
        actNewGroup.OnActivate += NewGroup;
        actionMap.AddAction(actNewGroup);
        //Load
        OnAccountInfoChanged(null, EventArgs.Empty);
    }

    private async void OnAccountInfoChanged(object? sender, EventArgs e)
    {
        if(_isFirstTimeLoading)
        {
            await _controller.RunRepeatTransactionsAsync();
            _isFirstTimeLoading = false;
        }
        if(!_isAccountLoading)
        {
            _isAccountLoading = true;
            _scrollTransactions.SetVisible(false);
            _statusPageNoTransactions.SetVisible(false);
            _binSpinner.SetVisible(true);
            _spinner.Start();
            _paneBox.SetSensitive(false);
            _boxSort.SetSensitive(false);
            //Overview
            _lblTotal.SetLabel(_controller.AccountTotalString);
            _lblIncome.SetLabel(_controller.AccountIncomeString);
            _lblExpense.SetLabel(_controller.AccountExpenseString);
            //Groups
            foreach (var groupRow in _groupRows)
            {
                _grpGroups.Remove(groupRow);
            }
            _groupRows.Clear();
            //Ungrouped Row
            var ungroupedRow = new GroupRow(_controller.UngroupedGroup, _controller.Localizer, _controller.IsFilterActive(-1));
            ungroupedRow.FilterChanged += UpdateGroupFilter;
            _grpGroups.Add(ungroupedRow);
            _groupRows.Add(ungroupedRow);
            var groups = new List<Group>();
            //Normal groups
            foreach (var pair in _controller.Groups)
            {
                groups.Add(pair.Value);
            }
            groups.Sort();
            foreach (var group in groups)
            {
                var row = new GroupRow(group, _controller.Localizer, _controller.IsFilterActive((int)group.Id));
                row.FilterChanged += UpdateGroupFilter;
                row.EditTriggered += EditGroup;
                row.DeleteTriggered += DeleteGroup;
                _grpGroups.Add(row);
                _groupRows.Add(row);
            }
            //Transactions
            foreach (var transactionRow in _transactionRows)
            {
                _flowBox.Remove(transactionRow);
            }
            _transactionRows.Clear();
            _btnSortFirstToLast.SetActive(_controller.SortFirstToLast);
            if (_controller.Transactions.Count > 0)
            {
                var filteredTransactions = _controller.FilteredTransactions;
                OnCalendarMonthYearChanged(null, EventArgs.Empty);
                if (filteredTransactions.Count > 0)
                {
                    _statusPageNoTransactions.SetVisible(false);
                    _scrollTransactions.SetVisible(true);
                    foreach (var transaction in filteredTransactions)
                    {
                        var row = new TransactionRow(transaction, _controller.Localizer);
                        row.DeleteTriggered += DeleteTransaction;
                        if (_controller.SortFirstToLast)
                        {
                            _flowBox.Append(row);
                        }
                        else
                        {
                            _flowBox.Prepend(row);
                        }
                        _transactionRows.Add(row);
                    }
                }
                else
                {
                    _statusPageNoTransactions.SetVisible(true);
                    _scrollTransactions.SetVisible(false);
                    _statusPageNoTransactions.SetTitle(_controller.Localizer["NoTransactionsTitle", "Filter"]);
                    _statusPageNoTransactions.SetDescription(_controller.Localizer["NoTransactionsDescription", "Filter"]);
                }
            }
            else
            {
                _calendar.ClearMarks();
                _statusPageNoTransactions.SetVisible(true);
                _scrollTransactions.SetVisible(false);
                _statusPageNoTransactions.SetTitle(_controller.Localizer["NoTransactionsTitle"]);
                _statusPageNoTransactions.SetDescription(_controller.Localizer["NoTransactionsDescription"]);
            }
            _spinner.Stop();
            _paneBox.SetSensitive(true);
            _boxSort.SetSensitive(true);
            _binSpinner.SetVisible(false);
            g_main_context_iteration(g_main_context_default(), true);
            _isAccountLoading = false;
        }
    }

    private void OnResetOverviewFilter(Gtk.Button sender, EventArgs e)
    {
        _chkIncome.SetActive(true);
        _chkExpense.SetActive(true);
    }

    /// <summary>
    /// Occurs when the group filter is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">The id of the group who's filter changed and whether to filter or not</param>
    private void UpdateGroupFilter(object? sender, (int Id, bool Filter) e) => _controller?.UpdateFilterValue(e.Id, e.Filter);

    private async void NewGroup(Gio.SimpleAction sender, EventArgs e)
    {
        var groupController = _controller.CreateGroupDialogController();
        var groupDialog = new GroupDialog(groupController, _parentWindow, _controller.Localizer);
        if(groupDialog.Run())
        {
            await _controller.AddGroupAsync(groupController.Group);
        }
    }

    private async void EditGroup(object? sender, uint id)
    {
        var groupController = _controller.CreateGroupDialogController(id);
        var groupDialog = new GroupDialog(groupController, _parentWindow, _controller.Localizer);
        if(groupDialog.Run())
        {
            await _controller.UpdateGroupAsync(groupController.Group);
        }
    }

    private async void DeleteGroup(object? sender, uint id)
    {
        var dialog = new MessageDialog(_parentWindow, _controller.Localizer["DeleteGroup"], _controller.Localizer["DeleteGroupDescription"], _controller.Localizer["No"], _controller.Localizer["Yes"]);
        if(dialog.Run() == MessageDialogResponse.Destructive)
        {
            await _controller.DeleteGroupAsync(id);
        }
    }

    private void OnCalendarMonthYearChanged(Gtk.Calendar? sender, EventArgs e)
    {
        _calendar.ClearMarks();
        var selectedDay = gtk_calendar_get_date(_calendar.Handle);
        foreach(var pair in _controller.Transactions)
        {
            if(pair.Value.Date.Month == g_date_time_get_month(ref selectedDay) && pair.Value.Date.Year == g_date_time_get_year(ref selectedDay))
            {
                _calendar.MarkDay((uint)pair.Value.Date.Day);
            }
        }
        gtk_calendar_select_day(_calendar.Handle, ref g_date_time_add_years(ref selectedDay, -1)); // workaround bug to show marks
        gtk_calendar_select_day(_calendar.Handle, ref g_date_time_add_years(ref selectedDay, 0));
    }

    private void OnCalendarSelectedDateChanged(Gtk.Calendar sender, EventArgs e)
    {
        if(!_isAccountLoading)
        {
            var selectedDay = gtk_calendar_get_date(_calendar.Handle);
            _controller.SetSingleDateFilter(new DateOnly(g_date_time_get_year(ref selectedDay), g_date_time_get_month(ref selectedDay), g_date_time_get_day_of_month(ref selectedDay)));
        }
    }

    private void OnResetCalendarFilter(Gtk.Button sender, EventArgs e)
    {
        gtk_calendar_select_day(_calendar.Handle, ref g_date_time_new_now_local());
        _expRange.SetEnableExpansion(false);
    }

    private void OnDateRangeToggled()
    {
        if(_expRange.GetEnableExpansion())
        {
            //Years For Date Filter
            var previousStartYear = _ddStartYear.GetSelected();
            var previousEndYear = _ddEndYear.GetSelected();
            var yearsForRangeFilter = _controller.YearsForRangeFilter.ToArray();
            _ddStartYear.SetModel(Gtk.StringList.New(yearsForRangeFilter));
            _ddEndYear.SetModel(Gtk.StringList.New(yearsForRangeFilter));
            _ddStartYear.SetSelected(previousStartYear > yearsForRangeFilter.Length - 1 ? 0 : previousStartYear);
            _ddEndYear.SetSelected(previousEndYear > yearsForRangeFilter.Length - 1 ? 0 : previousEndYear);
            //Set Date
            _controller.FilterStartDate = new DateOnly(int.Parse(yearsForRangeFilter[_ddStartYear.GetSelected()]), (int)_ddStartMonth.GetSelected() + 1, (int)_ddStartDay.GetSelected() + 1);
            _controller.FilterEndDate = new DateOnly(int.Parse(yearsForRangeFilter[_ddEndYear.GetSelected()]), (int)_ddEndMonth.GetSelected() + 1, (int)_ddEndDay.GetSelected() + 1);
        }
        else
        {
            _controller.SetSingleDateFilter(DateOnly.FromDateTime(DateTime.Now));
        }
    }

    private void OnDateRangeStartYearChanged() => _controller.FilterStartDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_ddStartYear.GetSelected()]), (int)_ddStartMonth.GetSelected() + 1, (int)_ddStartDay.GetSelected() + 1);

    private void OnDateRangeStartMonthChanged()
    {
        var year = int.Parse(_controller.YearsForRangeFilter[(int)_ddStartYear.GetSelected()]);
        var previousDay = (int)_ddStartDay.GetSelected() + 1;
        var newNumberOfDays = ((int)_ddStartMonth.GetSelected() + 1) switch
        {
            1 => 31,
            2 => (year % 400 == 0 || year % 100 != 0) && year % 4 == 0 ? 29 : 28,
            3 => 31,
            5 => 31,
            7 => 31,
            8 => 31,
            10 => 31,
            12 => 31,
            _ => 30
        };
        _ddStartDay.SetModel(Gtk.StringList.New(Enumerable.Range(1, newNumberOfDays).Select(x => x.ToString()).ToArray()));
        _ddStartDay.SetSelected(previousDay > newNumberOfDays ? 0 : (uint)previousDay - 1);
    }

    private void OnDateRangeStartDayChanged() => _controller.FilterStartDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_ddStartYear.GetSelected()]), (int)_ddStartMonth.GetSelected() + 1, (int)_ddStartDay.GetSelected() + 1);

    private void OnDateRangeEndYearChanged() => _controller.FilterEndDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_ddEndYear.GetSelected()]), (int)_ddEndMonth.GetSelected() + 1, (int)_ddEndDay.GetSelected() + 1);

    private void OnDateRangeEndMonthChanged()
    {
        var year = int.Parse(_controller.YearsForRangeFilter[(int)_ddEndYear.GetSelected()]);
        var previousDay = (int)_ddEndDay.GetSelected() + 1;
        var newNumberOfDays = ((int)_ddEndMonth.GetSelected() + 1) switch
        {
            1 => 31,
            2 => (year % 400 == 0 || year % 100 != 0) && year % 4 == 0 ? 29 : 28,
            3 => 31,
            5 => 31,
            7 => 31,
            8 => 31,
            10 => 31,
            12 => 31,
            _ => 30
        };
        _ddEndDay.SetModel(Gtk.StringList.New(Enumerable.Range(1, newNumberOfDays).Select(x => x.ToString()).ToArray()));
        _ddEndDay.SetSelected(previousDay > newNumberOfDays ? 0 : (uint)previousDay - 1);
    }

    private void OnDateRangeEndDayChanged() => _controller.FilterEndDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_ddEndYear.GetSelected()]), (int)_ddEndMonth.GetSelected() + 1, (int)_ddEndDay.GetSelected() + 1);

    private async void DeleteTransaction(object? sender, uint id)
    {
        var dialog = new MessageDialog(_parentWindow, _controller.Localizer["DeleteTransaction"], _controller.Localizer["DeleteTransactionDescription"], _controller.Localizer["No"], _controller.Localizer["Yes"]);
        if(dialog.Run() == MessageDialogResponse.Destructive)
        {
            await _controller.DeleteTransactionAsync(id);
        }
    }

    private void OnWindowWidthChanged(object? sender, WidthChangedEventArgs e)
    {
        foreach(var row in _transactionRows)
        {
            row.IsSmall = e.SmallWidth;
        }
    }
}