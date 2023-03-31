using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The AccountView for the application
/// </summary>
public partial class AccountView : Adw.Bin
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MoneyDateTime
    {
        ulong Usec;
        nint Tz;
        int Interval;
        int Days;
        int RefCount;
    };

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    [DllImport("libadwaita-1.so.0")]
    private static extern ref MoneyDateTime gtk_calendar_get_date(nint calendar);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_calendar_select_day(nint calendar, ref MoneyDateTime datetime);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_year(ref MoneyDateTime datetime);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_month(ref MoneyDateTime datetime);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_day_of_month(ref MoneyDateTime datetime);

    [DllImport("libadwaita-1.so.0")]
    private static extern ref MoneyDateTime g_date_time_add_years(ref MoneyDateTime datetime, int years);

    [DllImport("libadwaita-1.so.0")]
    private static extern ref MoneyDateTime g_date_time_new_now_local();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_new();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_title(nint dialog, string title);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_filters(nint dialog, nint filters);

    private delegate void GAsyncReadyCallback(nint source, nint res, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_open(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_open_finish(nint dialog, nint result, nint error);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_save(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_save_finish(nint dialog, nint result, nint error);

    private delegate bool GSourceFunc(nint data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_invoke(nint context, GSourceFunc function, nint data);

    private GAsyncReadyCallback _saveCallback { get; set; }
    private GAsyncReadyCallback _openCallback { get; set; }

    private readonly AccountViewController _controller;
    private bool _isAccountLoading;
    private readonly MainWindow _parentWindow;

    [Gtk.Connect] private readonly Adw.Flap _flap;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _paneScroll;
    [Gtk.Connect] private readonly Gtk.SearchEntry _searchDescriptionEntry;
    [Gtk.Connect] private readonly Gtk.Label _totalLabel;
    [Gtk.Connect] private readonly Gtk.Label _incomeLabel;
    [Gtk.Connect] private readonly Gtk.CheckButton _incomeCheck;
    [Gtk.Connect] private readonly Gtk.Label _expenseLabel;
    [Gtk.Connect] private readonly Gtk.CheckButton _expenseCheck;
    [Gtk.Connect] private readonly Gtk.Button _resetOverviewFilterButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _toggleGroupsButton;
    [Gtk.Connect] private readonly Adw.ButtonContent _toggleGroupsButtonContent;
    [Gtk.Connect] private readonly Gtk.Button _resetGroupsFilterButton;
    [Gtk.Connect] private readonly Gtk.ListBox _groupsList;
    [Gtk.Connect] private readonly Gtk.Calendar _calendar;
    [Gtk.Connect] private readonly Gtk.Button _resetCalendarFilterButton;
    [Gtk.Connect] private readonly Gtk.DropDown _startYearDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _startMonthDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _startDayDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _endYearDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _endMonthDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _endDayDropDown;
    [Gtk.Connect] private readonly Adw.ExpanderRow _rangeExpander;
    [Gtk.Connect] private readonly Gtk.DropDown _sortTransactionByDropDown;
    [Gtk.Connect] private readonly Gtk.ToggleButton _sortFirstToLastButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _sortLastToFirstButton;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _transactionsGroup;
    [Gtk.Connect] private readonly Gtk.Box _transactionsHeaderBox;
    [Gtk.Connect] private readonly Gtk.FlowBox _flowBox;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _transactionsScroll;
    [Gtk.Connect] private readonly Adw.StatusPage _noTransactionsStatusPage;
    [Gtk.Connect] private readonly Adw.Bin _spinnerBin;
    [Gtk.Connect] private readonly Gtk.Spinner _spinner;
    [Gtk.Connect] private readonly Gtk.Overlay _mainOverlay;

    private readonly Gtk.Adjustment _transactionsScrollAdjustment;
    private readonly Gtk.ShortcutController _shortcutController;
    private readonly Action<string> _updateSubtitle;
    private GSourceFunc[] _rowCallbacks;

    /// <summary>
    /// The Page widget
    /// </summary>
    public Adw.TabPage Page { get; init; }

    public AccountView(Gtk.Builder builder, AccountViewController controller, MainWindow parentWindow, Adw.TabView parentTabView, Gtk.ToggleButton btnFlapToggle, Action<string> updateSubtitle) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _parentWindow = parentWindow;
        _parentWindow.WidthChanged += OnWindowWidthChanged;
        _isAccountLoading = false;
        _updateSubtitle = updateSubtitle;
        _rowCallbacks = new GSourceFunc[5];
        //Register Controller Events
        _controller.AccountTransactionsChanged += OnAccountTransactionsChanged;
        _controller.UICreateGroupRow = CreateGroupRow;
        _controller.UIDeleteGroupRow = DeleteGroupRow;
        _controller.UICreateTransactionRow = CreateTransactionRow;
        _controller.UIMoveTransactionRow = MoveTransactionRow;
        _controller.UIDeleteTransactionRow = DeleteTransactionRow;
        //Build UI
        builder.Connect(this);
        btnFlapToggle.BindProperty("active", _flap, "reveal-flap", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate));
        //Search Description Text
        _searchDescriptionEntry.OnSearchChanged += (sender, e) => _controller.SearchDescription = _searchDescriptionEntry.GetText();
        //Account Income
        _incomeCheck.OnToggled += (Gtk.CheckButton sender, EventArgs e) => _controller.UpdateFilterValue(-3, _incomeCheck.GetActive());
        //Account Expense
        _expenseCheck.OnToggled += (Gtk.CheckButton sender, EventArgs e) => _controller.UpdateFilterValue(-2, _expenseCheck.GetActive());
        //Button Reset Overview Filter
        _resetOverviewFilterButton.OnClicked += OnResetOverviewFilter;
        //Button Toggle Groups
        _toggleGroupsButton.OnToggled += OnToggleGroups;
        //Button Reset Groups Filter
        _resetGroupsFilterButton.OnClicked += (Gtk.Button sender, EventArgs e) => _controller.ResetGroupsFilter();
        //Calendar Widget
        _calendar.OnPrevMonth += OnCalendarMonthYearChanged;
        _calendar.OnPrevYear += OnCalendarMonthYearChanged;
        _calendar.OnNextMonth += OnCalendarMonthYearChanged;
        _calendar.OnNextYear += OnCalendarMonthYearChanged;
        _calendar.OnDaySelected += OnCalendarSelectedDateChanged;
        //Button Reset Calendar Filter
        _resetCalendarFilterButton.OnClicked += OnResetCalendarFilter;
        //Start Range DropDowns
        _startYearDropDown.SetModel(Gtk.StringList.New(new string[1] { "" }));
        _startYearDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                OnDateRangeStartYearChanged();
            }
        };
        var dtFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;
        _startMonthDropDown.SetModel(Gtk.StringList.New(Enumerable.Range(1, 12).Select(x => dtFormatInfo.GetMonthName(x)).ToArray()));
        _startMonthDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                OnDateRangeStartMonthChanged();
            }
        };
        _startDayDropDown.SetModel(Gtk.StringList.New(Enumerable.Range(1, 31).Select(x => x.ToString()).ToArray()));
        _startDayDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                OnDateRangeStartDayChanged();
            }
        };
        //End Range DropDowns
        _endYearDropDown.SetModel(Gtk.StringList.New(new string[1] { "" }));
        _endYearDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                OnDateRangeEndYearChanged();
            }
        };
        _endMonthDropDown.SetModel(Gtk.StringList.New(Enumerable.Range(1, 12).Select(x => dtFormatInfo.GetMonthName(x)).ToArray()));
        _endMonthDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                OnDateRangeEndMonthChanged();
            }
        };
        _endDayDropDown.SetModel(Gtk.StringList.New(Enumerable.Range(1, 31).Select(x => x.ToString()).ToArray()));
        _endDayDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                OnDateRangeEndDayChanged();
            }
        };
        //Expander Row Select Range
        _rangeExpander.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "enable-expansion")
            {
                OnDateRangeToggled();
            }
        };
        //Sort Box And Buttons
        _sortTransactionByDropDown.SetModel(Gtk.StringList.New(new string[3] { _controller.Localizer["SortBy", "Id"], _controller.Localizer["SortBy", "Date"], _controller.Localizer["SortBy", "Amount"] }));
        _sortTransactionByDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                _controller.SortTransactionsBy = (SortBy)_sortTransactionByDropDown.GetSelected();
            }
        };
        _sortFirstToLastButton.OnToggled += (Gtk.ToggleButton sender, EventArgs e) => _controller.SortFirstToLast = _sortFirstToLastButton.GetActive();
        //Transactions Scrolled Window
        _transactionsScrollAdjustment = _transactionsScroll.GetVadjustment();
        _transactionsScrollAdjustment.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "value")
            {
                if (_transactionsScrollAdjustment.GetValue() == 0.0)
                {
                    _transactionsHeaderBox.RemoveCssClass("transactions-header");
                }
                else
                {
                    _transactionsHeaderBox.AddCssClass("transactions-header");
                }
            }
        };
        //Tab Page
        Page = parentTabView.Append(this);
        Page.SetTitle(_controller.AccountTitle);
        //Action Map
        var actionMap = Gio.SimpleActionGroup.New();
        _flap.InsertActionGroup("account", actionMap);
        //New Transaction Action
        var actNewTransaction = Gio.SimpleAction.New("newTransaction", null);
        actNewTransaction.OnActivate += NewTransaction;
        actionMap.AddAction(actNewTransaction);
        //New Group Action
        var actNewGroup = Gio.SimpleAction.New("newGroup", null);
        actNewGroup.OnActivate += NewGroup;
        actionMap.AddAction(actNewGroup);
        //Transfer Action
        var actTransfer = Gio.SimpleAction.New("transferMoney", null);
        actTransfer.OnActivate += TransferMoney;
        actionMap.AddAction(actTransfer);
        //Export To CSV Action
        var actExportCSV = Gio.SimpleAction.New("exportToCSV", null);
        actExportCSV.OnActivate += ExportToCSV;
        actionMap.AddAction(actExportCSV);
        //Export To PDF Action
        var actExportPDF = Gio.SimpleAction.New("exportToPDF", null);
        actExportPDF.OnActivate += ExportToPDF;
        actionMap.AddAction(actExportPDF);
        //Import Action
        var actImport = Gio.SimpleAction.New("importFromFile", null);
        actImport.OnActivate += ImportFromFile;
        actionMap.AddAction(actImport);
        //Account Settings Action
        var actAccountSettings = Gio.SimpleAction.New("accountSettings", null);
        actAccountSettings.OnActivate += AccountSettings;
        actionMap.AddAction(actAccountSettings);
        //Toggle Sidebar Action
        var actToggleSidebar = Gio.SimpleAction.New("toggleSidebar", null);
        actToggleSidebar.OnActivate += (sender, e) => _flap.SetRevealFlap(!_flap.GetRevealFlap());
        actionMap.AddAction(actToggleSidebar);
        //Shortcut Controller
        _shortcutController = Gtk.ShortcutController.New();
        _shortcutController.SetScope(Gtk.ShortcutScope.Managed);
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("<Ctrl>T"), Gtk.NamedAction.New("account.transferMoney")));
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("<Ctrl>E"), Gtk.NamedAction.New("account.exportToFile")));
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("<Ctrl>I"), Gtk.NamedAction.New("account.importFromFile")));
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("<Ctrl>G"), Gtk.NamedAction.New("account.newGroup")));
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("<Ctrl><Shift>N"), Gtk.NamedAction.New("account.newTransaction")));
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("F9"), Gtk.NamedAction.New("account.toggleSidebar")));
        _flap.AddController(_shortcutController);
    }

    /// <summary>
    /// Constructs an AccountView
    /// </summary>
    /// <param name="controller">AccountViewController</param>
    /// <param name="parentWindow">MainWindow</param>
    /// <param name="parentTabView">Adw.TabView</param>
    /// <param name="btnFlapToggle">Gtk.ToggleButton</param>
    /// <param name="updateSubtitle">A Action<string> callback to update the MainWindow's subtitle</param>
    public AccountView(AccountViewController controller, MainWindow parentWindow, Adw.TabView parentTabView, Gtk.ToggleButton btnFlapToggle, Action<string> updateSubtitle) : this(Builder.FromFile("account_view.ui", controller.Localizer), controller, parentWindow, parentTabView, btnFlapToggle, updateSubtitle)
    {
    }

    /// <summary>
    /// Creates a group row and adds it to the view
    /// </summary>
    /// <param name="group">The Group model</param>
    /// <param name="index">The optional index to insert</param>
    /// <returns>The IGroupRowControl</returns>
    private IGroupRowControl CreateGroupRow(Group group, int? index)
    {
        var row = new GroupRow(group, _controller.CultureForNumberString, _controller.Localizer, _controller.IsFilterActive(group.Id == 0 ? -1 : (int)group.Id));
        row.EditTriggered += EditGroup;
        row.DeleteTriggered += DeleteGroup;
        row.FilterChanged += UpdateGroupFilter;
        if (index != null)
        {
            _rowCallbacks[0] = (x) =>
            {
                try
                {
                    _groupsList.Insert(row, index.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                return false;
            };
        }
        else
        {
            _rowCallbacks[0] = (x) =>
            {
                try
                {
                    _groupsList.Append(row);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                return false;
            };
        }
        g_main_context_invoke(IntPtr.Zero, _rowCallbacks[0], IntPtr.Zero);
        return row;
    }

    /// <summary>
    /// Removes a group row from the view
    /// </summary>
    /// <param name="row">The IGroupRowControl</param>
    private void DeleteGroupRow(IGroupRowControl row)
    {
        _rowCallbacks[1] = (x) =>
        {
            try
            {
                _groupsList.Remove((GroupRow)row);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return false;
        };
        g_main_context_invoke(IntPtr.Zero, _rowCallbacks[1], IntPtr.Zero);
    }

    /// <summary>
    /// Creates a transaction row and adds it to the view
    /// </summary>
    /// <param name="transaction">The Transaction model</param>
    /// <param name="index">The optional index to insert</param>
    /// <returns>The IModelRowControl<Transaction></returns>
    private IModelRowControl<Transaction> CreateTransactionRow(Transaction transaction, int? index)
    {
        var row = new TransactionRow(transaction, _controller.CultureForNumberString, _controller.CultureForDateString, _controller.Localizer);
        row.EditTriggered += EditTransaction;
        row.DeleteTriggered += DeleteTransaction;
        if (index != null)
        {
            _rowCallbacks[2] = (x) =>
            {
                try
                {
                    row.IsSmall = _parentWindow.DefaultWidth < 450;
                    _flowBox.Insert(row, index.Value);
                    g_main_context_iteration(g_main_context_default(), false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                return false;
            };

        }
        else
        {
            _rowCallbacks[2] = (x) =>
            {
                try
                {
                    row.IsSmall = _parentWindow.DefaultWidth < 450;
                    _flowBox.Append(row);
                    g_main_context_iteration(g_main_context_default(), false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                return false;
            };
        }
        g_main_context_invoke(IntPtr.Zero, _rowCallbacks[2], IntPtr.Zero);
        return row;
    }

    /// <summary>
    /// Moves a row in the list
    /// </summary>
    /// <param name="row">The row to move</param>
    /// <param name="index">The new position</param>
    private void MoveTransactionRow(IModelRowControl<Transaction> row, int index)
    {
        _rowCallbacks[3] = (x) =>
        {
            try
            {
                var oldVisisbility = _flowBox.GetChildAtIndex(index)!.GetChild()!.IsVisible();
                _flowBox.Remove((TransactionRow)row);
                _flowBox.Insert((TransactionRow)row, index);
                g_main_context_iteration(g_main_context_default(), false);
                if (oldVisisbility)
                {
                    row.Show();
                }
                else
                {
                    row.Hide();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return false;
        };
        g_main_context_invoke(IntPtr.Zero, _rowCallbacks[3], IntPtr.Zero);
    }

    /// <summary>
    /// Removes a transaction row from the view
    /// </summary>
    /// <param name="row">The IModelRowControl<Transaction></param>
    private void DeleteTransactionRow(IModelRowControl<Transaction> row)
    {
        _rowCallbacks[4] = (x) =>
        {
            try
            {
                _flowBox.Remove((TransactionRow)row);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return false;
        };
        g_main_context_invoke(IntPtr.Zero, _rowCallbacks[4], IntPtr.Zero);
    }

    /// <summary>
    /// Starts the account view
    /// </summary>
    public async Task StartupAsync()
    {
        //Start Spinner
        _noTransactionsStatusPage.SetVisible(false);
        _transactionsScroll.SetVisible(true);
        _mainOverlay.SetOpacity(0.0);
        _spinnerBin.SetVisible(true);
        _spinner.Start();
        _paneScroll.SetSensitive(false);
        //Work
        await _controller.StartupAsync();
        if (_controller.AccountNeedsSetup)
        {
            AccountSettings(Gio.SimpleAction.New("ignore", null), EventArgs.Empty);
        }
        //Setup Other UI Elements
        _sortTransactionByDropDown.SetSelected((uint)_controller.SortTransactionsBy);
        if (_controller.SortFirstToLast)
        {
            _sortFirstToLastButton.SetActive(true);
        }
        else
        {
            _sortLastToFirstButton.SetActive(true);
        }
        OnToggleGroups(null, EventArgs.Empty);
        OnWindowWidthChanged(null, new WidthChangedEventArgs(_parentWindow.CompactMode));
        //Stop Spinner
        _spinner.Stop();
        _spinnerBin.SetVisible(false);
        _mainOverlay.SetOpacity(1.0);
        _paneScroll.SetSensitive(true);
    }

    /// <summary>
    /// Occurs when the account's transactions are changed 
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void OnAccountTransactionsChanged(object? sender, EventArgs e)
    {
        if (!_isAccountLoading)
        {
            _isAccountLoading = true;
            //Overview
            Page.SetTitle(_controller.AccountTitle);
            _updateSubtitle(_controller.AccountTitle);
            _totalLabel.SetLabel(_controller.AccountTodayTotalString);
            _incomeLabel.SetLabel(_controller.AccountTodayIncomeString);
            _expenseLabel.SetLabel(_controller.AccountTodayExpenseString);
            //Transactions
            if (_controller.TransactionsCount > 0)
            {
                OnCalendarMonthYearChanged(null, EventArgs.Empty);
                _transactionsGroup.SetTitle(string.Format(_controller.FilteredTransactionsCount != 1 ? _controller.Localizer["TransactionNumber", "Plural"] : _controller.Localizer["TransactionNumber"], _controller.FilteredTransactionsCount));
                if (_controller.FilteredTransactionsCount > 0)
                {
                    _noTransactionsStatusPage.SetVisible(false);
                    _transactionsScroll.SetVisible(true);
                }
                else
                {
                    _noTransactionsStatusPage.SetVisible(true);
                    _transactionsScroll.SetVisible(false);
                    _noTransactionsStatusPage.SetTitle(_controller.Localizer["NoTransactionsTitle", "Filter"]);
                    _noTransactionsStatusPage.SetDescription(_controller.Localizer["NoTransactionsDescription", "Filter"]);
                }
                _rangeExpander.SetSensitive(true);
            }
            else
            {
                _calendar.ClearMarks();
                _noTransactionsStatusPage.SetVisible(true);
                _transactionsScroll.SetVisible(false);
                _noTransactionsStatusPage.SetTitle(_controller.Localizer["NoTransactionsTitle"]);
                _noTransactionsStatusPage.SetDescription(_controller.Localizer["NoTransactionsDescription"]);
                _rangeExpander.SetSensitive(false);
            }
            _isAccountLoading = false;
        }
    }

    /// <summary>
    /// Occurs when the transfer money item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void TransferMoney(Gio.SimpleAction sender, EventArgs e)
    {
        if (_controller.AccountTodayTotal > 0)
        {
            var transferController = _controller.CreateTransferDialogController();
            var transferDialog = new TransferDialog(transferController, _parentWindow);
            transferDialog.Present();
            transferDialog.OnApply += async (sender, e) =>
            {
                transferDialog.SetVisible(false);
                await _controller.SendTransferAsync(transferController.Transfer);
                transferDialog.Close();
            };
        }
        else
        {
            _controller.SendNotification(_controller.Localizer["NoMoneyToTransfer"], NotificationSeverity.Error);
        }
    }

    /// <summary>
    /// Occurs when the import from file item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void ImportFromFile(Gio.SimpleAction sender, EventArgs e)
    {
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName($"{_controller.Localizer["AllFiles"]} (*.csv, *.ofx, *.qif)");
        filterAll.AddPattern("*.csv");
        filterAll.AddPattern("*.CSV");
        filterAll.AddPattern("*.ofx");
        filterAll.AddPattern("*.OFX");
        filterAll.AddPattern("*.qif");
        filterAll.AddPattern("*.QIF");
        var filterCsv = Gtk.FileFilter.New();
        filterCsv.SetName("CSV (*.csv)");
        filterCsv.AddPattern("*.csv");
        filterCsv.AddPattern("*.CSV");
        var filterOfx = Gtk.FileFilter.New();
        filterOfx.SetName("Open Financial Exchange (*.ofx)");
        filterOfx.AddPattern("*.ofx");
        filterOfx.AddPattern("*.OFX");
        var filterQif = Gtk.FileFilter.New();
        filterQif.SetName("Quicken Format (*.qif)");
        filterQif.AddPattern("*.qif");
        filterQif.AddPattern("*.QIF");
        var openFileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(openFileDialog, _controller.Localizer["OpenAccount"]);
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterAll);
        filters.Append(filterCsv);
        filters.Append(filterOfx);
        filters.Append(filterQif);
        gtk_file_dialog_set_filters(openFileDialog, filters.Handle);
        _openCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_open_finish(openFileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
                //Start Spinner
                _noTransactionsStatusPage.SetVisible(false);
                _transactionsScroll.SetVisible(true);
                _mainOverlay.SetOpacity(0.0);
                _spinnerBin.SetVisible(true);
                _spinner.Start();
                _paneScroll.SetSensitive(false);
                //Work
                await Task.Run(async () =>
                {
                    try
                    {
                        await _controller.ImportFromFileAsync(path ?? "");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                });
                //Stop Spinner
                _spinner.Stop();
                _spinnerBin.SetVisible(false);
                _mainOverlay.SetOpacity(1.0);
                _paneScroll.SetSensitive(true);
            }
        };
        gtk_file_dialog_open(openFileDialog, _parentWindow.Handle, IntPtr.Zero, _openCallback, IntPtr.Zero);
    }

    /// <summary>
    /// Occurs when the export to csv item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void ExportToCSV(Gio.SimpleAction sender, EventArgs e)
    {
        var filterCsv = Gtk.FileFilter.New();
        filterCsv.SetName("CSV (*.csv)");
        filterCsv.AddPattern("*.csv");
        var saveFileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(saveFileDialog, _controller.Localizer["ExportToFile"]);
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterCsv);
        gtk_file_dialog_set_filters(saveFileDialog, filters.Handle);
        _saveCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_save_finish(saveFileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
                if (Path.GetExtension(path).ToLower() != ".csv")
                {
                    path += ".csv";
                }
                _controller.ExportToCSV(path ?? "");
            }
        };
        gtk_file_dialog_save(saveFileDialog, _parentWindow.Handle, IntPtr.Zero, _saveCallback, IntPtr.Zero);
    }

    /// <summary>
    /// Occurs when the export to pdf item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void ExportToPDF(Gio.SimpleAction sender, EventArgs e)
    {
        var filterPdf = Gtk.FileFilter.New();
        filterPdf.SetName("PDF (*.pdf)");
        filterPdf.AddPattern("*.pdf");
        var saveFileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(saveFileDialog, _controller.Localizer["ExportToFile"]);
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterPdf);
        gtk_file_dialog_set_filters(saveFileDialog, filters.Handle);
        _saveCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_save_finish(saveFileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
                if (Path.GetExtension(path).ToLower() != ".pdf")
                {
                    path += ".pdf";
                }
                string? password = null;
                var dialog = new MessageDialog(_parentWindow, _controller.Localizer["AddPasswordToPDF"], _controller.Localizer["AddPasswordToPDF", "Description"], _controller.Localizer["No"], null, _controller.Localizer["Yes"]);
                dialog.Present();
                dialog.OnResponse += async (sender, e) =>
                {
                    if (dialog.Response == MessageDialogResponse.Suggested)
                    {
                        var tcs = new TaskCompletionSource<string?>();
                        var newPasswordDialog = new NewPasswordDialog(_parentWindow, _controller.Localizer["PDFPassword"], _controller.Localizer, tcs);
                        newPasswordDialog.Present();
                        var password = await tcs.Task;
                        _controller.ExportToPDF(path ?? "", password);
                    }
                    else
                    {
                        _controller.ExportToPDF(path ?? "", null);
                    }
                    dialog.Destroy();
                };
            }
        };
        gtk_file_dialog_save(saveFileDialog, _parentWindow.Handle, IntPtr.Zero, _saveCallback, IntPtr.Zero);
    }

    /// <summary>
    /// Occurs when the account settings item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void AccountSettings(Gio.SimpleAction sender, EventArgs e)
    {
        var accountSettingsController = _controller.CreateAccountSettingsDialogController();
        var accountSettingsDialog = new AccountSettingsDialog(accountSettingsController, _parentWindow);
        accountSettingsDialog.Present();
        accountSettingsDialog.OnApply += (sender, e) =>
        {
            accountSettingsDialog.SetVisible(false);
            _controller.UpdateMetadata(accountSettingsController.Metadata);
            if (accountSettingsController.NewPassword != null)
            {
                _controller.SetPassword(accountSettingsController.NewPassword);
            }
            accountSettingsDialog.Close();
        };
    }

    /// <summary>
    /// Occurs when the new transaction item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void NewTransaction(Gio.SimpleAction sender, EventArgs e)
    {
        var transactionController = _controller.CreateTransactionDialogController();
        var transactionDialog = new TransactionDialog(transactionController, _parentWindow);
        transactionDialog.Present();
        transactionDialog.OnApply += async (sender, e) =>
        {
            transactionDialog.SetVisible(false);
            //Start Spinner
            _noTransactionsStatusPage.SetVisible(false);
            _transactionsScroll.SetVisible(true);
            _mainOverlay.SetOpacity(0.0);
            _spinnerBin.SetVisible(true);
            _spinner.Start();
            _paneScroll.SetSensitive(false);
            //Work
            await Task.Run(async () =>
            {
                try
                {
                    await _controller.AddTransactionAsync(transactionController.Transaction);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            });
            //Stop Spinner
            _spinner.Stop();
            _spinnerBin.SetVisible(false);
            _mainOverlay.SetOpacity(1.0);
            _paneScroll.SetSensitive(true);
            transactionController.Dispose();
            transactionDialog.Close();
        };
    }

    /// <summary>
    /// Occurs when creation of transaction copy was requested
    /// </summary>
    /// <param name="source">Source transaction for copy</param>
    private void CopyTransaction(Transaction source)
    {
        var transactionController = _controller.CreateTransactionDialogController(source);
        var transactionDialog = new TransactionDialog(transactionController, _parentWindow);
        transactionDialog.Present();
        transactionDialog.OnApply += async (sender, e) =>
        {
            transactionDialog.SetVisible(false);
            //Start Spinner
            _noTransactionsStatusPage.SetVisible(false);
            _transactionsScroll.SetVisible(true);
            _mainOverlay.SetOpacity(0.0);
            _spinnerBin.SetVisible(true);
            _spinner.Start();
            _paneScroll.SetSensitive(false);
            //Work
            await Task.Run(async () =>
            {
                try
                {
                    await _controller.AddTransactionAsync(transactionController.Transaction);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            });
            //Stop Spinner
            _spinner.Stop();
            _spinnerBin.SetVisible(false);
            _mainOverlay.SetOpacity(1.0);
            _paneScroll.SetSensitive(true);
            transactionController.Dispose();
            transactionDialog.Close();
        };
    }

    /// <summary>
    /// Occurs when the edit transaction item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void EditTransaction(object? sender, uint id)
    {
        var transactionController = _controller.CreateTransactionDialogController(id);
        var transactionDialog = new TransactionDialog(transactionController, _parentWindow);
        transactionDialog.Present();
        transactionDialog.OnApply += async (sender, e) =>
        {
            transactionDialog.SetVisible(false);
            if (transactionController.CopyRequested)
            {
                CopyTransaction(transactionController.Transaction);
                return;
            }
            if (_controller.GetIsSourceRepeatTransaction(id) && transactionController.OriginalRepeatInterval != TransactionRepeatInterval.Never)
            {
                if (transactionController.OriginalRepeatInterval != transactionController.Transaction.RepeatInterval)
                {
                    var dialog = new MessageDialog(_parentWindow, _controller.Localizer["RepeatIntervalChanged"], _controller.Localizer["RepeatIntervalChangedDescription"], _controller.Localizer["Cancel"], _controller.Localizer["DisassociateExisting"], _controller.Localizer["DeleteExisting"]);
                    dialog.UnsetDestructiveApperance();
                    dialog.UnsetSuggestedApperance();
                    dialog.Present();
                    dialog.OnResponse += async (sender, e) =>
                    {
                        if (dialog.Response == MessageDialogResponse.Suggested)
                        {
                            //Start Spinner
                            _noTransactionsStatusPage.SetVisible(false);
                            _transactionsScroll.SetVisible(true);
                            _mainOverlay.SetOpacity(0.0);
                            _spinnerBin.SetVisible(true);
                            _spinner.Start();
                            _paneScroll.SetSensitive(false);
                            //Work
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await _controller.DeleteGeneratedTransactionsAsync(id);
                                    await _controller.UpdateTransactionAsync(transactionController.Transaction);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine(ex.StackTrace);
                                }
                            });
                            //Stop Spinner
                            _spinner.Stop();
                            _spinnerBin.SetVisible(false);
                            _mainOverlay.SetOpacity(1.0);
                            _paneScroll.SetSensitive(true);
                        }
                        else if (dialog.Response == MessageDialogResponse.Destructive)
                        {
                            //Start Spinner
                            _noTransactionsStatusPage.SetVisible(false);
                            _transactionsScroll.SetVisible(true);
                            _mainOverlay.SetOpacity(0.0);
                            _spinnerBin.SetVisible(true);
                            _spinner.Start();
                            _paneScroll.SetSensitive(false);
                            //Work
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await _controller.UpdateSourceTransactionAsync(transactionController.Transaction, false);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine(ex.StackTrace);
                                }
                            });
                            //Stop Spinner
                            _spinner.Stop();
                            _spinnerBin.SetVisible(false);
                            _mainOverlay.SetOpacity(1.0);
                            _paneScroll.SetSensitive(true);
                        }
                        dialog.Destroy();
                    };
                }
                else
                {
                    var dialog = new MessageDialog(_parentWindow, _controller.Localizer["EditTransaction", "SourceRepeat"], _controller.Localizer["EditTransactionDescription", "SourceRepeat"], _controller.Localizer["Cancel"], _controller.Localizer["EditOnlySourceTransaction"], _controller.Localizer["EditSourceGeneratedTransaction"]);
                    dialog.UnsetDestructiveApperance();
                    dialog.UnsetSuggestedApperance();
                    dialog.Present();
                    dialog.OnResponse += async (sender, e) =>
                    {
                        if (dialog.Response != MessageDialogResponse.Cancel)
                        {
                            //Start Spinner
                            _noTransactionsStatusPage.SetVisible(false);
                            _transactionsScroll.SetVisible(true);
                            _mainOverlay.SetOpacity(0.0);
                            _spinnerBin.SetVisible(true);
                            _spinner.Start();
                            _paneScroll.SetSensitive(false);
                            //Work
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await _controller.UpdateSourceTransactionAsync(transactionController.Transaction, dialog.Response == MessageDialogResponse.Suggested);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine(ex.StackTrace);
                                }
                            });
                            //Stop Spinner
                            _spinner.Stop();
                            _spinnerBin.SetVisible(false);
                            _mainOverlay.SetOpacity(1.0);
                            _paneScroll.SetSensitive(true);
                        }
                        dialog.Destroy();
                    };
                }
            }
            else
            {
                //Start Spinner
                _noTransactionsStatusPage.SetVisible(false);
                _transactionsScroll.SetVisible(true);
                _mainOverlay.SetOpacity(0.0);
                _spinnerBin.SetVisible(true);
                _spinner.Start();
                _paneScroll.SetSensitive(false);
                //Work
                await Task.Run(async () =>
                {
                    try
                    {
                        await _controller.UpdateTransactionAsync(transactionController.Transaction);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                });
                //Stop Spinner
                _spinner.Stop();
                _spinnerBin.SetVisible(false);
                _mainOverlay.SetOpacity(1.0);
                _paneScroll.SetSensitive(true);
            }
            transactionController.Dispose();
            transactionDialog.Close();
        };
    }

    /// <summary>
    /// Occurs when the delete transaction item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void DeleteTransaction(object? sender, uint id)
    {
        if (_controller.GetIsSourceRepeatTransaction(id))
        {
            var dialog = new MessageDialog(_parentWindow, _controller.Localizer["DeleteTransaction", "SourceRepeat"], _controller.Localizer["DeleteTransactionDescription", "SourceRepeat"], _controller.Localizer["Cancel"], _controller.Localizer["DeleteOnlySourceTransaction"], _controller.Localizer["DeleteSourceGeneratedTransaction"]);
            dialog.UnsetDestructiveApperance();
            dialog.UnsetSuggestedApperance();
            dialog.Present();
            dialog.OnResponse += async (sender, e) =>
            {
                if (dialog.Response != MessageDialogResponse.Cancel)
                {
                    //Start Spinner
                    _noTransactionsStatusPage.SetVisible(false);
                    _transactionsScroll.SetVisible(true);
                    _mainOverlay.SetOpacity(0.0);
                    _spinnerBin.SetVisible(true);
                    _spinner.Start();
                    _paneScroll.SetSensitive(false);
                    //Work
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await _controller.DeleteSourceTransactionAsync(id, dialog.Response == MessageDialogResponse.Suggested);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    });
                    //Stop Spinner
                    _spinner.Stop();
                    _spinnerBin.SetVisible(false);
                    _mainOverlay.SetOpacity(1.0);
                    _paneScroll.SetSensitive(true);
                }
                dialog.Destroy();
            };
        }
        else
        {
            var dialog = new MessageDialog(_parentWindow, _controller.Localizer["DeleteTransaction"], _controller.Localizer["DeleteTransactionDescription"], _controller.Localizer["No"], _controller.Localizer["Yes"]);
            dialog.Present();
            dialog.OnResponse += async (sender, e) =>
            {
                if (dialog.Response == MessageDialogResponse.Destructive)
                {
                    await _controller.DeleteTransactionAsync(id);
                }
                dialog.Destroy();
            };
        }
    }

    /// <summary>
    /// Occurs when the new group item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void NewGroup(Gio.SimpleAction sender, EventArgs e)
    {
        var groupController = _controller.CreateGroupDialogController();
        var groupDialog = new GroupDialog(groupController, _parentWindow);
        groupDialog.Present();
        groupDialog.OnApply += async (sender, e) =>
        {
            groupDialog.SetVisible(false);
            //Start Spinner
            _noTransactionsStatusPage.SetVisible(false);
            _transactionsScroll.SetVisible(true);
            _mainOverlay.SetOpacity(0.0);
            _spinnerBin.SetVisible(true);
            _spinner.Start();
            _paneScroll.SetSensitive(false);
            //Work
            await Task.Run(async () =>
            {
                try
                {
                    await _controller.AddGroupAsync(groupController.Group);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            });
            //Stop Spinner
            _spinner.Stop();
            _spinnerBin.SetVisible(false);
            _mainOverlay.SetOpacity(1.0);
            _paneScroll.SetSensitive(true);
            groupDialog.Close();
        };
    }

    /// <summary>
    /// Occurs when the edit group item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void EditGroup(object? sender, uint id)
    {
        var groupController = _controller.CreateGroupDialogController(id);
        var groupDialog = new GroupDialog(groupController, _parentWindow);
        groupDialog.Present();
        groupDialog.OnApply += async (sender, e) =>
        {
            groupDialog.SetVisible(false);
            //Start Spinner
            _noTransactionsStatusPage.SetVisible(false);
            _transactionsScroll.SetVisible(true);
            _mainOverlay.SetOpacity(0.0);
            _spinnerBin.SetVisible(true);
            _spinner.Start();
            _paneScroll.SetSensitive(false);
            //Work
            await Task.Run(async () =>
            {
                try
                {
                    await _controller.UpdateGroupAsync(groupController.Group);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            });
            //Stop Spinner
            _spinner.Stop();
            _spinnerBin.SetVisible(false);
            _mainOverlay.SetOpacity(1.0);
            _paneScroll.SetSensitive(true);
            groupDialog.Close();
        };
    }

    /// <summary>
    /// Occurs when the delete group item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void DeleteGroup(object? sender, uint id)
    {
        var dialog = new MessageDialog(_parentWindow, _controller.Localizer["DeleteGroup"], _controller.Localizer["DeleteGroupDescription"], _controller.Localizer["No"], _controller.Localizer["Yes"]);
        dialog.Present();
        dialog.OnResponse += async (sender, e) =>
        {
            if (dialog.Response == MessageDialogResponse.Destructive)
            {
                await _controller.DeleteGroupAsync(id);
            }
            dialog.Destroy();
        };
    }

    /// <summary>
    /// Occurs when the reset overview filter button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnResetOverviewFilter(Gtk.Button sender, EventArgs e)
    {
        _incomeCheck.SetActive(true);
        _expenseCheck.SetActive(true);
    }

    /// <summary>
    /// Occurs when the group filter is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">The id of the group who's filter changed and whether to filter or not</param>
    private void UpdateGroupFilter(object? sender, (uint Id, bool Filter) e) => _controller?.UpdateFilterValue(e.Id == 0 ? -1 : (int)e.Id, e.Filter);

    /// <summary>
    /// Occurs when the user presses the button to show/hide groups
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void OnToggleGroups(object? sender, EventArgs e)
    {
        if (_toggleGroupsButton.GetActive())
        {
            _toggleGroupsButtonContent.SetIconName("view-reveal-symbolic");
            _toggleGroupsButtonContent.SetLabel(_controller.Localizer["Show"]);
        }
        else
        {
            _toggleGroupsButtonContent.SetIconName("view-conceal-symbolic");
            _toggleGroupsButtonContent.SetLabel(_controller.Localizer["Hide"]);
        }
        _groupsList.SetVisible(!_toggleGroupsButton.GetActive());
        _controller.ShowGroupsList = !_toggleGroupsButton.GetActive();
    }

    /// <summary>
    /// Occurs when the calendar's month/year is changed
    /// </summary>
    /// <param name="sender">Gtk.Calendar?</param>
    /// <param name="e">EventArgs</param>
    private void OnCalendarMonthYearChanged(Gtk.Calendar? sender, EventArgs e)
    {
        _calendar.ClearMarks();
        var selectedDay = gtk_calendar_get_date(_calendar.Handle);
        foreach (var date in _controller.DatesInAccount)
        {
            if (date.Month == g_date_time_get_month(ref selectedDay) && date.Year == g_date_time_get_year(ref selectedDay))
            {
                _calendar.MarkDay((uint)date.Day);
            }
        }
        gtk_calendar_select_day(_calendar.Handle, ref g_date_time_add_years(ref selectedDay, -1)); // workaround bug to show marks
        gtk_calendar_select_day(_calendar.Handle, ref g_date_time_add_years(ref selectedDay, 0));
    }

    /// <summary>
    /// Occurs when the calendar's date selection is changed
    /// </summary>
    /// <param name="sender">Gtk.Calendar</param>
    /// <param name="e">EventArgs</param>
    private void OnCalendarSelectedDateChanged(Gtk.Calendar sender, EventArgs e)
    {
        if (!_isAccountLoading)
        {
            var selectedDay = gtk_calendar_get_date(_calendar.Handle);
            _controller.SetSingleDateFilter(new DateOnly(g_date_time_get_year(ref selectedDay), g_date_time_get_month(ref selectedDay), g_date_time_get_day_of_month(ref selectedDay)));
        }
    }

    /// <summary>
    /// Occurs when the reset calendar filter button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnResetCalendarFilter(Gtk.Button sender, EventArgs e)
    {
        gtk_calendar_select_day(_calendar.Handle, ref g_date_time_new_now_local());
        _rangeExpander.SetEnableExpansion(false);
    }

    /// <summary>
    /// Occurs when the select date range is toggled
    /// </summary>
    private void OnDateRangeToggled()
    {
        if (_rangeExpander.GetEnableExpansion())
        {
            //Years For Date Filter
            var previousStartYear = _startYearDropDown.GetSelected();
            var previousEndYear = _endYearDropDown.GetSelected();
            var yearsForRangeFilter = _controller.YearsForRangeFilter.ToArray();
            _startYearDropDown.SetModel(Gtk.StringList.New(yearsForRangeFilter));
            _endYearDropDown.SetModel(Gtk.StringList.New(yearsForRangeFilter));
            _startYearDropDown.SetSelected(previousStartYear > yearsForRangeFilter.Length - 1 ? 0 : previousStartYear);
            _endYearDropDown.SetSelected(previousEndYear > yearsForRangeFilter.Length - 1 ? 0 : previousEndYear);
            //Set Date
            _controller.FilterStartDate = new DateOnly(int.Parse(yearsForRangeFilter[_startYearDropDown.GetSelected()]), (int)_startMonthDropDown.GetSelected() + 1, (int)_startDayDropDown.GetSelected() + 1);
            _controller.FilterEndDate = new DateOnly(int.Parse(yearsForRangeFilter[_endYearDropDown.GetSelected()]), (int)_endMonthDropDown.GetSelected() + 1, (int)_endDayDropDown.GetSelected() + 1);
        }
        else
        {
            _controller.SetSingleDateFilter(DateOnly.FromDateTime(DateTime.Now));
        }
    }

    /// <summary>
    /// Occurs when the date range's start year is changed
    /// </summary>
    private void OnDateRangeStartYearChanged() => _controller.FilterStartDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_startYearDropDown.GetSelected()]), (int)_startMonthDropDown.GetSelected() + 1, (int)_startDayDropDown.GetSelected() + 1);

    /// <summary>
    /// Occurs when the date range's start month is changed
    /// </summary>
    private void OnDateRangeStartMonthChanged()
    {
        var year = int.Parse(_controller.YearsForRangeFilter[(int)_startYearDropDown.GetSelected()]);
        var previousDay = (int)_startDayDropDown.GetSelected() + 1;
        var newNumberOfDays = ((int)_startMonthDropDown.GetSelected() + 1) switch
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
        _startDayDropDown.SetModel(Gtk.StringList.New(Enumerable.Range(1, newNumberOfDays).Select(x => x.ToString()).ToArray()));
        _startDayDropDown.SetSelected(previousDay > newNumberOfDays ? 0 : (uint)previousDay - 1);
    }

    /// <summary>
    /// Occurs when the date range's start day is changed
    /// </summary>
    private void OnDateRangeStartDayChanged() => _controller.FilterStartDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_startYearDropDown.GetSelected()]), (int)_startMonthDropDown.GetSelected() + 1, (int)_startDayDropDown.GetSelected() + 1);

    /// <summary>
    /// Occurs when the date range's end year is changed
    /// </summary>
    private void OnDateRangeEndYearChanged() => _controller.FilterEndDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_endYearDropDown.GetSelected()]), (int)_endMonthDropDown.GetSelected() + 1, (int)_endDayDropDown.GetSelected() + 1);

    /// <summary>
    /// Occurs when the date range's end month is changed
    /// </summary>
    private void OnDateRangeEndMonthChanged()
    {
        var year = int.Parse(_controller.YearsForRangeFilter[(int)_endYearDropDown.GetSelected()]);
        var previousDay = (int)_endDayDropDown.GetSelected() + 1;
        var newNumberOfDays = ((int)_endMonthDropDown.GetSelected() + 1) switch
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
        _endDayDropDown.SetModel(Gtk.StringList.New(Enumerable.Range(1, newNumberOfDays).Select(x => x.ToString()).ToArray()));
        _endDayDropDown.SetSelected(previousDay > newNumberOfDays ? 0 : (uint)previousDay - 1);
    }

    /// <summary>
    /// Occurs when the date range's end day is changed
    /// </summary>
    private void OnDateRangeEndDayChanged() => _controller.FilterEndDate = new DateOnly(int.Parse(_controller.YearsForRangeFilter[(int)_endYearDropDown.GetSelected()]), (int)_endMonthDropDown.GetSelected() + 1, (int)_endDayDropDown.GetSelected() + 1);

    /// <summary>
    /// Occurs when the window's width is changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnWindowWidthChanged(object? sender, WidthChangedEventArgs e)
    {
        foreach (TransactionRow row in _controller.TransactionRows.Values)
        {
            row.IsSmall = e.SmallWidth;
        }
        if (e.SmallWidth)
        {
            _transactionsGroup.SetTitle("");
        }
        else
        {
            _transactionsGroup.SetTitle(string.Format(_controller.FilteredTransactionsCount != 1 ? _controller.Localizer["TransactionNumber", "Plural"] : _controller.Localizer["TransactionNumber"], _controller.FilteredTransactionsCount));
        }
    }
}