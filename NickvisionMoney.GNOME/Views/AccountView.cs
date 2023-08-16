using Nickvision.GirExt;
using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static NickvisionMoney.Shared.Helpers.Gettext;

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
    }

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

    private readonly AccountViewController _controller;
    private bool _isAccountLoading;
    private readonly MainWindow _parentWindow;
    private readonly Gtk.Adjustment _transactionsScrollAdjustment;
    private readonly Gtk.ShortcutController _shortcutController;
    private readonly Action<string> _updateSubtitle;
    private Dictionary<uint, GroupRow> _groupRows;
    private Dictionary<string, TagButton> _tagButtons;
    private Dictionary<uint, TransactionRow> _transactionRows;
    private uint _currentGraphPage;

    [Gtk.Connect] private readonly Adw.Flap _flap;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _paneScroll;
    [Gtk.Connect] private readonly Gtk.SearchEntry _searchDescriptionEntry;
    [Gtk.Connect] private readonly Gtk.Label _totalLabel;
    [Gtk.Connect] private readonly Gtk.Label _incomeLabel;
    [Gtk.Connect] private readonly Gtk.CheckButton _incomeCheck;
    [Gtk.Connect] private readonly Gtk.Label _expenseLabel;
    [Gtk.Connect] private readonly Gtk.CheckButton _expenseCheck;
    [Gtk.Connect] private readonly Gtk.Button _remindersButton;
    [Gtk.Connect] private readonly Gtk.Button _resetOverviewFilterButton;
    [Gtk.Connect] private readonly Gtk.Button _toggleGroupsButton;
    [Gtk.Connect] private readonly Gtk.Button _resetGroupsFilterButton;
    [Gtk.Connect] private readonly Gtk.Button _unselectAllGroupsFilterButton;
    [Gtk.Connect] private readonly Gtk.ListBox _groupsList;
    [Gtk.Connect] private readonly Gtk.Button _toggleTagsButton;
    [Gtk.Connect] private readonly Gtk.Button _resetTagsFilterButton;
    [Gtk.Connect] private readonly Gtk.Button _unselectAllTagsFilterButton;
    [Gtk.Connect] private readonly Adw.Bin _tagsBin;
    [Gtk.Connect] private readonly Gtk.FlowBox _tagsFlowBox;
    [Gtk.Connect] private readonly Gtk.Calendar _calendar;
    [Gtk.Connect] private readonly Gtk.Button _selectMonthButton;
    [Gtk.Connect] private readonly Gtk.Button _resetCalendarFilterButton;
    [Gtk.Connect] private readonly Gtk.DropDown _startYearDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _startMonthDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _startDayDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _endYearDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _endMonthDropDown;
    [Gtk.Connect] private readonly Gtk.DropDown _endDayDropDown;
    [Gtk.Connect] private readonly Adw.ExpanderRow _rangeExpander;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStack;
    [Gtk.Connect] private readonly Adw.StatusPage _noTransactionsStatusPage;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _visualizeGroup;
    [Gtk.Connect] private readonly Gtk.Button _graphBackButton;
    [Gtk.Connect] private readonly Gtk.Button _graphNextButton;
    [Gtk.Connect] private readonly Adw.Carousel _carousel;
    [Gtk.Connect] private readonly Gtk.DrawingArea _incomeExpenseOverTimeImage;
    [Gtk.Connect] private readonly Gtk.DrawingArea _incomeExpensePieImage;
    [Gtk.Connect] private readonly Gtk.DrawingArea _incomeExpensePerGroupImage;
    [Gtk.Connect] private readonly Gtk.DrawingArea _incomeExpensePerGroupPieImage;
    [Gtk.Connect] private readonly Gtk.Separator _visualizeSeparator;
    [Gtk.Connect] private readonly Gtk.DropDown _sortTransactionByDropDown;
    [Gtk.Connect] private readonly Gtk.ToggleButton _sortFirstToLastButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _sortLastToFirstButton;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _transactionsGroup;
    [Gtk.Connect] private readonly Gtk.Box _transactionsHeaderBox;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _transactionsScroll;
    [Gtk.Connect] private readonly Gtk.FlowBox _transactionsFlowBox;
    
    /// <summary>
    /// The Page widget
    /// </summary>
    public Adw.TabPage Page { get; init; }

    public AccountView(Gtk.Builder builder, AccountViewController controller, MainWindow parentWindow, Adw.TabView parentTabView, Gtk.ToggleButton btnFlapToggle, Gtk.ToggleButton btnGraphToggle, Action<string> updateSubtitle) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _parentWindow = parentWindow;
        _parentWindow.WidthChanged += OnWindowWidthChanged;
        _isAccountLoading = false;
        _updateSubtitle = updateSubtitle;
        _groupRows = new Dictionary<uint, GroupRow>();
        _tagButtons = new Dictionary<string, TagButton>();
        _transactionRows = new Dictionary<uint, TransactionRow>();
        _currentGraphPage = 0;
        //Register Controller Events
        _controller.AccountInformationChanged += (sender, e) => GLib.Functions.IdleAdd(0, AccountInformationChanged);
        _controller.GroupCreated += (sender, e) => GLib.Functions.IdleAdd(0, () => CreateGroupRow(e));
        _controller.GroupDeleted += (sender, e) => GLib.Functions.IdleAdd(0, () => DeleteGroupRow(e));
        _controller.GroupUpdated += (sender, e) => GLib.Functions.IdleAdd(0, () => UpdateGroupRow(e));
        _controller.TagCreated += (sender, e) => GLib.Functions.IdleAdd(0, () => CreateTagButton(e));
        _controller.TagUpdated += (sender, e) => GLib.Functions.IdleAdd(0, () => UpdateTagButton(e));
        _controller.TransactionCreated += (sender, e) => GLib.Functions.IdleAdd(0, () => CreateTransactionRow(e));
        _controller.TransactionMoved += (sender, e) => GLib.Functions.IdleAdd(0, () => MoveTransactionRow(e));
        _controller.TransactionDeleted += (sender, e) => GLib.Functions.IdleAdd(0, () => DeleteTransactionRow(e));
        _controller.TransactionUpdated += (sender, e) => GLib.Functions.IdleAdd(0, () => UpdateTransactionRow(e));
        Adw.StyleManager.GetDefault().OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "dark")
            {
                _incomeExpenseOverTimeImage.QueueDraw();
                _incomeExpensePieImage.QueueDraw();
                _incomeExpensePerGroupImage.QueueDraw();
                _incomeExpensePerGroupPieImage.QueueDraw();
            }
        };
        //Build UI
        builder.Connect(this);
        btnFlapToggle.BindProperty("active", _flap, "reveal-flap", GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate);
        btnGraphToggle.BindProperty("active", _visualizeGroup, "visible", GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate);
        btnGraphToggle.BindProperty("active", _visualizeSeparator, "visible", GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate);
        //Search Description Text
        _searchDescriptionEntry.OnSearchChanged += (sender, e) => _controller.SearchDescription = _searchDescriptionEntry.GetText();
        //Account Income
        _incomeCheck.OnToggled += (sender, e) => _controller.UpdateGroupFilterValue(-3, _incomeCheck.GetActive());
        //Account Expense
        _expenseCheck.OnToggled += (sender, e) => _controller.UpdateGroupFilterValue(-2, _expenseCheck.GetActive());
        //Reminders
        _remindersButton.OnClicked += OnTransactionReminders;
        //Button Reset Overview Filter
        _resetOverviewFilterButton.OnClicked += OnResetOverviewFilter;
        //Button Toggle Groups
        _toggleGroupsButton.OnClicked += (sender, e) =>
        {
            _controller.ShowGroupsList = !_controller.ShowGroupsList;
            OnToggleGroups();
        };
        //Button Reset Groups Filter
        _resetGroupsFilterButton.OnClicked += (Gtk.Button sender, EventArgs e) => _controller.ResetGroupFilters();
        //Button Reset Groups Filter
        _unselectAllGroupsFilterButton.OnClicked += (Gtk.Button sender, EventArgs e) => _controller.UnselectAllGroupFilters();
        //Tags FlowBox
        _tagsFlowBox.SetSortFunc((box1, box2) =>
        {
            var tag1 = (TagButton)box1.GetChild();
            var tag2 = (TagButton)box2.GetChild();
            if (tag1.Tag == _("Untagged"))
            {
                return -1;
            }
            return string.Compare(tag1.Tag, tag2.Tag);
        });
        //Button Toggle Tags
        _toggleTagsButton.OnClicked += (sender, e) =>
        {
            _controller.ShowTagsList = !_controller.ShowTagsList;
            OnToggleTags();
        };
        //Button Reset Tags Filter
        _resetTagsFilterButton.OnClicked += (Gtk.Button sender, EventArgs e) => _controller.ResetTagFilters();
        //Button Reset Tags Filter
        _unselectAllTagsFilterButton.OnClicked += (Gtk.Button sender, EventArgs e) => _controller.UnselectAllTagFilters();
        //Calendar Widget
        _calendar.OnPrevMonth += OnCalendarMonthYearChanged;
        _calendar.OnPrevYear += OnCalendarMonthYearChanged;
        _calendar.OnNextMonth += OnCalendarMonthYearChanged;
        _calendar.OnNextYear += OnCalendarMonthYearChanged;
        _calendar.OnDaySelected += OnCalendarSelectedDateChanged;
        //Button select current month as filter
        _selectMonthButton.OnClicked += OnSelectCurrentMonth;
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
        _sortTransactionByDropDown.SetModel(Gtk.StringList.New(new string[3] { _("Sort By Id"), _("Sort By Date"), _("Sort By Amount") }));
        _sortTransactionByDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                _controller.SortTransactionsBy = (SortBy)_sortTransactionByDropDown.GetSelected();
            }
        };
        _sortFirstToLastButton.OnToggled += (Gtk.ToggleButton sender, EventArgs e) => _controller.SortFirstToLast = _sortFirstToLastButton.GetActive();
        //Graphs images
        _incomeExpenseOverTimeImage.SetDrawFunc((area, ctx, width, height) =>
        {
            if (_currentGraphPage == 0)
            {
                DrawGraph(ctx, GraphType.IncomeExpenseOverTime, width, height);
            }
        });
        _incomeExpensePieImage.SetDrawFunc((area, ctx, width, height) =>
        {
            if (_currentGraphPage == 1)
            {
                DrawGraph(ctx, GraphType.IncomeExpensePie, width, height);
            }
        });
        _incomeExpensePerGroupImage.SetDrawFunc((area, ctx, width, height) =>
        {
            if (_currentGraphPage == 2)
            {
                DrawGraph(ctx, GraphType.IncomeExpensePerGroup, width, height);
            }
        });
        _incomeExpensePerGroupPieImage.SetDrawFunc((area, ctx, width, height) =>
        {
            if (_currentGraphPage == 3)
            {
                DrawDoubleGraphs(ctx, GraphType.IncomeByGroup, GraphType.ExpenseByGroup, width, height);
            }
        });
        //Graph Carousel Buttons
        _graphBackButton.OnClicked += (sender, e) =>
        {
            if (_currentGraphPage > 0)
            {
                _currentGraphPage--;
                _carousel.ScrollTo(_carousel.GetNthPage(_currentGraphPage), true);
            }
        };
        _graphNextButton.OnClicked += (sender, e) =>
        {
            if (_currentGraphPage < _carousel.GetNPages() - 1)
            {
                _currentGraphPage++;
                _carousel.ScrollTo(_carousel.GetNthPage(_currentGraphPage), true);
            }
        };
        _carousel.OnPageChanged += (sender, e) =>
        {
            _currentGraphPage = e.Index;
            _incomeExpenseOverTimeImage.QueueDraw();
            _incomeExpensePieImage.QueueDraw();
            _incomeExpensePerGroupImage.QueueDraw();
            _incomeExpensePerGroupPieImage.QueueDraw();
        };
        //Transactions Scrolled Window
        _transactionsScrollAdjustment = _transactionsScroll.GetVadjustment();
        _transactionsScrollAdjustment.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "value")
            {
                if (_transactionsScrollAdjustment.GetValue() == 0.0)
                {
                    _transactionsScroll.RemoveCssClass("transactions-scroll");
                }
                else
                {
                    _transactionsScroll.AddCssClass("transactions-scroll");
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
        //Export To CSV All Action
        var actExportCSVAll = Gio.SimpleAction.New("exportToCSVAll", null);
        actExportCSVAll.OnActivate += async (sender, e) => await ExportToCSVAsync(ExportMode.All);
        actionMap.AddAction(actExportCSVAll);
        //Export To CSV Current Action
        var actExportCSVCurrent = Gio.SimpleAction.New("exportToCSVCurrent", null);
        actExportCSVCurrent.OnActivate += async (sender, e) => await ExportToCSVAsync(ExportMode.CurrentView);
        actionMap.AddAction(actExportCSVCurrent);
        //Export To PDF All Action
        var actExportPDFAll = Gio.SimpleAction.New("exportToPDFAll", null);
        actExportPDFAll.OnActivate += async (sender, e) => await ExportToPDFAsync(ExportMode.All);
        actionMap.AddAction(actExportPDFAll);
        //Export To PDF Current Action
        var actExportPDFCurrent = Gio.SimpleAction.New("exportToPDFCurrent", null);
        actExportPDFCurrent.OnActivate += async (sender, e) => await ExportToPDFAsync(ExportMode.CurrentView);
        actionMap.AddAction(actExportPDFCurrent);
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
    /// <param name="btnGraphToggle">Gtk.ToggleButton</param>
    /// <param name="updateSubtitle">An Action callback to update the MainWindow's subtitle</param>
    public AccountView(AccountViewController controller, MainWindow parentWindow, Adw.TabView parentTabView, Gtk.ToggleButton btnFlapToggle, Gtk.ToggleButton btnGraphToggle, Action<string> updateSubtitle) : this(Builder.FromFile("account_view.ui"), controller, parentWindow, parentTabView, btnFlapToggle, btnGraphToggle, updateSubtitle)
    {
    }

    /// <summary>
    /// Starts the account view
    /// </summary>
    public async Task StartupAsync()
    {
        _paneScroll.SetSensitive(false);
        _viewStack.SetVisibleChildName("spinner");
        await Task.Run(async () => await _controller.StartupAsync());
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
        OnToggleGroups();
        OnToggleTags();
        OnWindowWidthChanged(null, new WidthChangedEventArgs(_parentWindow.CompactMode));
        if(_controller.TransactionReminders.Count > 0)
        {
            var remindersDialog = new RemindersDialog(_parentWindow, _controller.AppInfo.ID, _("Upcoming transactions"), _controller.TransactionReminders);
            remindersDialog.Present();
        }
    }

    /// <summary>
    /// Occurs when the account's information is changed
    /// </summary>
    private bool AccountInformationChanged()
    {
        if (!_isAccountLoading)
        {
            _isAccountLoading = true;
            //Overview
            Page.SetTitle(_controller.AccountTitle);
            _updateSubtitle(_controller.AccountTitle);
            _totalLabel.SetLabel(_controller.AccountFilteredTotalString);
            _incomeLabel.SetLabel(_controller.AccountFilteredIncomeString);
            _expenseLabel.SetLabel(_controller.AccountFilteredExpenseString);
            //Transactions
            _paneScroll.SetSensitive(true);
            if (_controller.Transactions.Count > 0)
            {
                OnCalendarMonthYearChanged(null, EventArgs.Empty);
                _transactionsGroup.SetTitle(_n("{0} transaction", "{0} transactions", _controller.FilteredTransactionsCount, _controller.FilteredTransactionsCount));
                if (_controller.FilteredTransactionsCount > 0)
                {
                    _viewStack.SetVisibleChildName("transactions");
                    _incomeExpenseOverTimeImage.QueueDraw();
                    _incomeExpensePieImage.QueueDraw();
                    _incomeExpensePerGroupImage.QueueDraw();
                    _incomeExpensePerGroupPieImage.QueueDraw();
                    _remindersButton.SetIconName(_controller.TransactionReminders.Count > 0 ? "bell-symbolic" : "bell-outline-symbolic");
                }
                else
                {
                    _viewStack.SetVisibleChildName("no-transactions");
                    _noTransactionsStatusPage.SetTitle(_("No Transactions Found"));
                    _noTransactionsStatusPage.SetDescription(_("No transactions match the specified filters."));
                }
                _rangeExpander.SetSensitive(true);
            }
            else
            {
                _calendar.ClearMarks();
                _viewStack.SetVisibleChildName("no-transactions");
                _noTransactionsStatusPage.SetTitle(_("No Transactions"));
                _noTransactionsStatusPage.SetDescription(_("Add a new transaction or import transactions from a file."));
                _rangeExpander.SetSensitive(false);
            }
            _isAccountLoading = false;
        }
        return false;
    }

    /// <summary>
    /// Drawing function for graph images
    /// </summary>
    /// <param name="ctx">Cairo.Context</param>
    /// <param name="type">GraphType</param>
    /// <param name="width">The width of the graph</param>
    /// <param name="height">The height of the graph</param>
    private void DrawGraph(Cairo.Context ctx, GraphType type, int width, int height)
    {
        var graph = _controller.GenerateGraph(type, Adw.StyleManager.GetDefault().GetDark(), width, height);
        using var loader = GdkPixbuf.PixbufLoader.New();
        loader.Write(graph);
        loader.Close();
        var pixbuf = loader.GetPixbuf()!;
        Gdk.Functions.CairoSetSourcePixbuf(ctx, pixbuf, 0, 0);
        ctx.Paint();
    }

    /// <summary>
    /// Drawing function for double graph images
    /// </summary>
    /// <param name="ctx">Cairo.Context</param>
    /// <param name="type1">First GraphType</param>
    /// <param name="type2">Second GraphType</param>
    /// <param name="width">The width of the graph</param>
    /// <param name="height">The height of the graph</param>
    private void DrawDoubleGraphs(Cairo.Context ctx, GraphType type1, GraphType type2, int width, int height)
    {
        var graph1 = _controller.GenerateGraph(type1, Adw.StyleManager.GetDefault().GetDark(), width / 2, height);
        using var loader1 = GdkPixbuf.PixbufLoader.New();
        loader1.Write(graph1);
        loader1.Close();
        var pixbuf1 = loader1.GetPixbuf()!;
        Gdk.Functions.CairoSetSourcePixbuf(ctx, pixbuf1, 0, 0);
        ctx.Paint();
        var graph2 = _controller.GenerateGraph(type2, Adw.StyleManager.GetDefault().GetDark(), width / 2, height);
        using var loader2 = GdkPixbuf.PixbufLoader.New();
        loader2.Write(graph2);
        loader2.Close();
        var pixbuf2 = loader2.GetPixbuf()!;
        Gdk.Functions.CairoSetSourcePixbuf(ctx, pixbuf2, width / 2, 0);
        ctx.Paint();
    }
    
    /// <summary>
    /// Creates a group row and adds it to the view
    /// </summary>
    /// <param name="e">ModelEventArgs</param>
    private bool CreateGroupRow(ModelEventArgs<Group> e)
    {
        if (!_groupRows.ContainsKey(e.Model.Id))
        {
            var row = new GroupRow(e.Model, _controller.CultureForNumberString, _controller.UseNativeDigits, e.Active, _controller.GroupDefaultColor);
            row.EditTriggered += EditGroup;
            row.FilterChanged += UpdateGroupFilter;
            if (e.Position != null)
            {
                _groupsList.Insert(row, e.Position.Value);
            }
            else
            {
                _groupsList.Append(row);
            }
            _groupRows.Add(e.Model.Id, row);
        }
        return false;
    }

    /// <summary>
    /// Removes a group row from the view
    /// </summary>
    /// <param name="id">The id of the group</param>
    private bool DeleteGroupRow(uint id)
    {
        if (_groupRows.ContainsKey(id))
        {
            _groupsList.Remove(_groupRows[id]);
            _groupRows.Remove(id);
        }
        return false;
    }
    
    /// <summary>
    /// Updates a group row
    /// </summary>
    /// <param name="e">ModelEventArgs</param>
    private bool UpdateGroupRow(ModelEventArgs<Group> e)
    {
        if (_groupRows.ContainsKey(e.Model.Id))
        {
            _groupRows[e.Model.Id].UpdateRow(e.Model, _controller.GroupDefaultColor, _controller.CultureForNumberString, e.Active);
        }
        else
        {
            CreateGroupRow(e);
        }
        return false;
    }

    /// <summary>
    /// Creates a tag and adds it to the view
    /// </summary>
    /// <param name="e">ModelEventArgs</param>
    private bool CreateTagButton(ModelEventArgs<string> e)
    {
        if (!_tagButtons.ContainsKey(e.Model))
        {
            var tagButton = new TagButton(e.Model);
            tagButton.SetActive(true);
            tagButton.FilterChanged += UpdateTagFilter;
            _tagsFlowBox.Append(tagButton);
            _tagsFlowBox.InvalidateSort();
            _tagButtons.Add(e.Model, tagButton);
        }
        return false;
    }

    /// <summary>
    /// Updates a tag button
    /// </summary>
    /// <param name="e">ModelEventArgs</param>
    private bool UpdateTagButton(ModelEventArgs<string> e)
    {
        if (_tagButtons.ContainsKey(e.Model))
        {
            _tagButtons[e.Model].SetActive(e.Active);
        }
        return false;
    }

    /// <summary>
    /// Creates a transaction row and adds it to the view
    /// </summary>
    /// <param name="e">ModelEventArgs</param>
    private bool CreateTransactionRow(ModelEventArgs<Transaction> e)
    {
        if (!_transactionRows.ContainsKey(e.Model.Id))
        {
            var row = new TransactionRow(e.Model, _controller.Groups, _controller.CultureForNumberString, _controller.UseNativeDigits, _controller.TransactionDefaultColor);
            row.EditTriggered += EditTransaction;
            row.IsSmall = _parentWindow.DefaultWidth < 450;
            row.SetVisible(e.Active);
            if (e.Position != null)
            {
                _transactionsFlowBox.Insert(row, e.Position.Value);
            }
            else
            {

                _transactionsFlowBox.Append(row);
            }
            _transactionRows.Add(e.Model.Id, row);
        }
        return false;
    }

    /// <summary>
    /// Moves a transaction row in the list
    /// </summary>
    /// <param name="e">ModelEventArgs</param>
    private bool MoveTransactionRow(ModelEventArgs<Transaction> e)
    {
        if (_transactionRows.ContainsKey(e.Model.Id))
        {
            _transactionsFlowBox.Remove(_transactionRows[e.Model.Id]);
            _transactionsFlowBox.Insert(_transactionRows[e.Model.Id], e.Position ?? -1);
        }
        return false;
    }

    /// <summary>
    /// Removes a transaction row from the view
    /// </summary>
    /// <param name="id">uint</param>
    private bool DeleteTransactionRow(uint id)
    {
        if (_transactionRows.ContainsKey(id))
        {
            _transactionsFlowBox.Remove(_transactionRows[id]);
            _transactionRows.Remove(id);
        }
        return false;
    }
    
    /// <summary>
    /// Updates a transaction row
    /// </summary>
    /// <param name="e">ModelEventArgs</param>
    private bool UpdateTransactionRow(ModelEventArgs<Transaction> e)
    {
        if (_transactionRows.ContainsKey(e.Model.Id))
        {
            _transactionRows[e.Model.Id].UpdateRow(e.Model, _controller.TransactionDefaultColor, _controller.CultureForNumberString);
            _transactionRows[e.Model.Id].SetVisible(e.Active);
        }
        else
        {
            CreateTransactionRow(e);
        }
        return false;
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
            transferDialog.OnApply += async (s, ex) =>
            {
                await _controller.SendTransferAsync(transferController.Transfer);
                transferDialog.Destroy();
            };
        }
        else
        {
            _controller.SendNotification(_("This account has no money available to transfer."), NotificationSeverity.Error);
        }
    }

    /// <summary>
    /// Occurs when the import from file item is activated
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void ImportFromFile(Gio.SimpleAction sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileDialog.New();
        openFileDialog.SetTitle(_("Import from File"));
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName($"{_("All files")} (*.csv, *.ofx, *.qif)");
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
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterAll);
        filters.Append(filterCsv);
        filters.Append(filterOfx);
        filters.Append(filterQif);
        openFileDialog.SetFilters(filters);
        try
        {
            var file = await openFileDialog.OpenAsync(_parentWindow);
            var oldPage = _viewStack.GetVisibleChildName();
            _paneScroll.SetSensitive(false);
            _viewStack.SetVisibleChildName("spinner");
            await Task.Run(async () => await _controller.ImportFromFileAsync(file!.GetPath() ?? ""));
            _viewStack.SetVisibleChildName(_viewStack.GetVisibleChildName() == "spinner" ? oldPage : _viewStack.GetVisibleChildName());
            _paneScroll.SetSensitive(true);
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the export to csv item is activated
    /// </summary>
    /// <param name="exportMode">The information to export</param>
    private async Task ExportToCSVAsync(ExportMode exportMode)
    {
        var saveFileDialog = Gtk.FileDialog.New();
        saveFileDialog.SetTitle(_("Export to File"));
        var filterCsv = Gtk.FileFilter.New();
        filterCsv.SetName("CSV (*.csv)");
        filterCsv.AddPattern("*.csv");
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterCsv);
        saveFileDialog.SetFilters(filters);
        try
        {
            var file = await saveFileDialog.SaveAsync(_parentWindow);
            var path = file!.GetPath();
            if (Path.GetExtension(path).ToLower() != ".csv")
            {
                path += ".csv";
            }
            _controller.ExportToCSV(path ?? "", exportMode);
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the export to pdf item is activated
    /// </summary>
    /// <param name="exportMode">The information to export</param>
    private async Task ExportToPDFAsync(ExportMode exportMode)
    {
        var saveFileDialog = Gtk.FileDialog.New();
        saveFileDialog.SetTitle(_("Export to File"));
        var filterPdf = Gtk.FileFilter.New();
        filterPdf.SetName("PDF (*.pdf)");
        filterPdf.AddPattern("*.pdf");
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterPdf);
        saveFileDialog.SetFilters(filters);
        try
        {
            var file = await saveFileDialog.SaveAsync(_parentWindow);
            var path = file!.GetPath();
            if (Path.GetExtension(path).ToLower() != ".pdf")
            {
                path += ".pdf";
            }
            var dialog = Adw.MessageDialog.New(_parentWindow, _("Add Password To PDF?"), _("Would you like to password-protect the PDF file?\n\nIf the password is lost, the PDF will be inaccessible."));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("no", _("No"));
            dialog.SetDefaultResponse("no");
            dialog.SetCloseResponse("no");
            dialog.AddResponse("yes", _("Yes"));
            dialog.SetResponseAppearance("yes", Adw.ResponseAppearance.Suggested);
            dialog.OnResponse += async (sender, e) =>
            {
                if (e.Response == "yes")
                {
                    var tcs = new TaskCompletionSource<string?>();
                    var newPasswordDialog = new NewPasswordDialog(_parentWindow, _("PDF Password"), tcs);
                    newPasswordDialog.Present();
                    var password = await tcs.Task;
                    _controller.ExportToPDF(path ?? "", exportMode, password);
                }
                else
                {
                    _controller.ExportToPDF(path ?? "", exportMode, null);
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        catch { }
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
        accountSettingsDialog.OnApply += (s, ex) =>
        {
            _controller.UpdateMetadata(accountSettingsController.Metadata);
            if (accountSettingsController.NewPassword != null)
            {
                _controller.SetPassword(accountSettingsController.NewPassword);
            }
            accountSettingsDialog.Destroy();
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
        transactionDialog.OnApply += async (s, ex) =>
        {
            _paneScroll.SetSensitive(false);
            _viewStack.SetVisibleChildName("spinner");
            await Task.Run(async () => await _controller.AddTransactionAsync(transactionController.Transaction));
            transactionDialog.Destroy();
        };
        transactionDialog.OnDestroy += (s, ex) => transactionController.Dispose();
    }

    /// <summary>
    /// Occurs when creation of transaction copy was requested
    /// </summary>
    /// <param name="source">Source transaction for copy</param>
    private void CopyTransaction(Transaction source)
    {
        var transactionController = _controller.CreateTransactionDialogController(source);
        var transactionDialog = new TransactionDialog(transactionController, _parentWindow);
        transactionDialog.OnApply += async (sender, e) =>
        {
            _paneScroll.SetSensitive(false);
            _viewStack.SetVisibleChildName("spinner");
            await Task.Run(async () => await _controller.AddTransactionAsync(transactionController.Transaction));
            transactionDialog.Destroy();
        };
        transactionDialog.OnDestroy += (sender, e) => transactionController.Dispose();
        transactionDialog.Present();
    }

    /// <summary>
    /// Occurs when the edit transaction item is activated
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="id">uint</param>
    private void EditTransaction(object? sender, uint id)
    {
        var transactionController = _controller.CreateTransactionDialogController(id);
        var transactionDialog = new TransactionDialog(transactionController, _parentWindow);
        transactionDialog.OnApply += async (s, ex) =>
        {
            if (transactionController.CopyRequested)
            {
                CopyTransaction(transactionController.Transaction);
                return;
            }
            if (_controller.GetIsSourceRepeatTransaction(id) && transactionController.OriginalRepeatInterval != TransactionRepeatInterval.Never)
            {
                if (transactionController.OriginalRepeatInterval != transactionController.Transaction.RepeatInterval)
                {
                    var dialog = Adw.MessageDialog.New(_parentWindow, _("Repeat Interval Changed"), _("The repeat interval was changed.\nWhat would you like to do with existing generated transactions?\n\nNew repeat transactions will be generated based off the new interval."));
                    dialog.SetIconName(_controller.AppInfo.ID);
                    dialog.AddResponse("cancel", _("Cancel"));
                    dialog.SetDefaultResponse("cancel");
                    dialog.SetCloseResponse("cancel");
                    dialog.AddResponse("disassociate", _("Disassociate Existing"));
                    dialog.AddResponse("delete", _("Delete Existing"));
                    dialog.OnResponse += async (ss, exx) =>
                    {
                        if (exx.Response == "delete")
                        {
                            _paneScroll.SetSensitive(false);
                            _viewStack.SetVisibleChildName("spinner");
                            await Task.Run(async () =>
                            {
                                await _controller.DeleteGeneratedTransactionsAsync(id);
                                await _controller.UpdateTransactionAsync(transactionController.Transaction);
                            });
                        }
                        else if (exx.Response == "disassociate")
                        {
                            _paneScroll.SetSensitive(false);
                            _viewStack.SetVisibleChildName("spinner");
                            await Task.Run(async () => await _controller.UpdateSourceTransactionAsync(transactionController.Transaction, false));
                        }
                        dialog.Destroy();
                        transactionDialog.Destroy();
                    };
                    dialog.Present();
                }
                else
                {
                    var dialog = Adw.MessageDialog.New(_parentWindow, _("Update Transaction"), _("This transaction is a source repeat transaction.\nWhat would you like to do with the repeat transactions?\n\nUpdating only the source transaction will disassociate\ngenerated transactions from the source."));
                    dialog.SetIconName(_controller.AppInfo.ID);
                    dialog.AddResponse("cancel", _("Cancel"));
                    dialog.SetDefaultResponse("cancel");
                    dialog.SetCloseResponse("cancel");
                    dialog.AddResponse("source", _("Update Only Source"));
                    dialog.AddResponse("gen", _("Update Source and Generated"));
                    dialog.OnResponse += async (ss, exx) =>
                    {
                        if (exx.Response != "cancel")
                        {
                            _paneScroll.SetSensitive(false);
                            _viewStack.SetVisibleChildName("spinner");
                            await Task.Run(async () => await _controller.UpdateSourceTransactionAsync(transactionController.Transaction, exx.Response == "gen"));
                        }
                        dialog.Destroy();
                        transactionDialog.Destroy();
                    };
                    dialog.Present();
                }
            }
            else
            {
                _paneScroll.SetSensitive(false);
                _viewStack.SetVisibleChildName("spinner");
                await Task.Run(async () => await _controller.UpdateTransactionAsync(transactionController.Transaction));
                transactionDialog.Destroy();
            }
        };
        transactionDialog.OnDelete += (s, ex) =>
        {
            if (_controller.GetIsSourceRepeatTransaction(id))
            {
                var dialog = Adw.MessageDialog.New(_parentWindow, _("Delete Transaction"), _("This transaction is a source repeat transaction.\nWhat would you like to do with the repeat transactions?\n\nDeleting only the source transaction will allow individual\ngenerated transactions to be modifiable."));
                dialog.SetIconName(_controller.AppInfo.ID);
                dialog.AddResponse("cancel", _("Cancel"));
                dialog.SetDefaultResponse("cancel");
                dialog.SetCloseResponse("cancel");
                dialog.AddResponse("source", _("Delete Only Source"));
                dialog.AddResponse("gen", _("Delete Source and Generated"));
                dialog.OnResponse += async (ss, exx) =>
                {
                    if (exx.Response != "cancel")
                    {
                        _paneScroll.SetSensitive(false);
                        _viewStack.SetVisibleChildName("spinner");
                        await Task.Run(async () => await _controller.DeleteSourceTransactionAsync(id, exx.Response == "gen"));
                        transactionDialog.Destroy();
                    }
                    else
                    {
                        transactionDialog.Present();
                    }
                    dialog.Destroy();
                };
                dialog.Present();
            }
            else
            {
                var dialog = Adw.MessageDialog.New(_parentWindow, _("Delete Transaction"), _("Are you sure you want to delete this transaction?\nThis action is irreversible."));
                dialog.SetIconName(_controller.AppInfo.ID);
                dialog.AddResponse("no", _("No"));
                dialog.SetDefaultResponse("no");
                dialog.SetCloseResponse("no");
                dialog.AddResponse("yes", _("Yes"));
                dialog.SetResponseAppearance("yes", Adw.ResponseAppearance.Suggested);
                dialog.OnResponse += async (ss, exx) =>
                {
                    if (exx.Response == "yes")
                    {
                        await _controller.DeleteTransactionAsync(id);
                        transactionDialog.Destroy();
                    }
                    else
                    {
                        transactionDialog.Present();
                    }
                    dialog.Destroy();
                };
                dialog.Present();
            }
        };
        transactionDialog.OnDestroy += (s, ex) => transactionController.Dispose();
        transactionDialog.Present();
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
        groupDialog.OnApply += async (s, ex) =>
        {
            var oldPage = _viewStack.GetVisibleChildName();
            _paneScroll.SetSensitive(false);
            _viewStack.SetVisibleChildName("spinner");
            await Task.Run(async () => await _controller.AddGroupAsync(groupController.Group));
            _viewStack.SetVisibleChildName(oldPage);
            _paneScroll.SetSensitive(true);
            groupDialog.Destroy();
        };
        groupDialog.Present();
    }

    /// <summary>
    /// Occurs when the edit group item is activated
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="id">uint</param>
    private void EditGroup(object? sender, uint id)
    {
        var groupController = _controller.CreateGroupDialogController(id);
        var groupDialog = new GroupDialog(groupController, _parentWindow);
        groupDialog.OnApply += async (s, ex) =>
        {
            var oldPage = _viewStack.GetVisibleChildName();
            _paneScroll.SetSensitive(false);
            _viewStack.SetVisibleChildName("spinner");
            await Task.Run(async () => await _controller.UpdateGroupAsync(groupController.Group, groupController.HasColorChanged));
            _viewStack.SetVisibleChildName(oldPage);
            _paneScroll.SetSensitive(true);
            groupDialog.Destroy();
        };
        groupDialog.OnDelete += (s, ex) =>
        {
            var dialog = Adw.MessageDialog.New(_parentWindow, _("Delete Group"), _("Are you sure you want to delete this group?\nThis action is irreversible."));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("no", _("No"));
            dialog.SetDefaultResponse("no");
            dialog.SetCloseResponse("no");
            dialog.AddResponse("yes", _("Yes"));
            dialog.SetResponseAppearance("yes", Adw.ResponseAppearance.Destructive);
            dialog.OnResponse += async (ss, exx) =>
            {
                if (exx.Response == "yes")
                {
                    await _controller.DeleteGroupAsync(id);
                    groupDialog.Destroy();
                }
                else
                {
                    groupDialog.Present();
                }
                dialog.Destroy();
            };
            dialog.Present();
        };
        groupDialog.Present();
    }

    /// <summary>
    /// Occurs when the transaction reminders button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnTransactionReminders(Gtk.Button sender, EventArgs e)
    {
        var remindersDialog = new RemindersDialog(_parentWindow, _controller.AppInfo.ID, _("Upcoming transactions"), _controller.TransactionReminders);
        remindersDialog.Present();
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
    private void UpdateGroupFilter(object? sender, (uint Id, bool Filter) e) => _controller.UpdateGroupFilterValue((int)e.Id, e.Filter);

    /// <summary>
    /// Occurs when the tagfilter is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">The tag and whether to filter or not</param>
    private void UpdateTagFilter(object? sender, (string Tag, bool Filter) e) => _controller.UpdateTagFilter(e.Tag, e.Filter);

    /// <summary>
    /// Occurs when the user presses the button to show/hide groups
    /// </summary>
    private void OnToggleGroups()
    {
        _toggleGroupsButton.SetIconName(!_controller.ShowGroupsList ? "view-reveal-symbolic" : "view-conceal-symbolic");
        _groupsList.SetVisible(_controller.ShowGroupsList);
    }
    
    /// <summary>
    /// Occurs when the user presses the button to show/hide tags
    /// </summary>
    private void OnToggleTags()
    {
        _toggleTagsButton.SetIconName(!_controller.ShowTagsList ? "view-reveal-symbolic" : "view-conceal-symbolic");
        _tagsBin.SetVisible(_controller.ShowTagsList);
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
    /// Occurs when the select current month button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnSelectCurrentMonth(Gtk.Button sender, EventArgs e)
    {
        _rangeExpander.SetEnableExpansion(true);
        var selectedDay = gtk_calendar_get_date(_calendar.Handle);
        var selectedMonth = (uint)(g_date_time_get_month(ref selectedDay) - 1);
        var selectedYear = g_date_time_get_year(ref selectedDay);
        var selectedYearIndex = (uint)_controller.YearsForRangeFilter.IndexOf(selectedYear.ToString());
        _startYearDropDown.SetSelected(selectedYearIndex);
        _endYearDropDown.SetSelected(selectedYearIndex);
        _startMonthDropDown.SetSelected(selectedMonth);
        _endMonthDropDown.SetSelected(selectedMonth);
        _startDayDropDown.SetSelected(0);
        _endDayDropDown.SetSelected(_endDayDropDown.Model.GetNItems() - 1);
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
    /// <param name="sender">object?</param>
    /// <param name="e">WidthChangedEventArgs</param>
    private void OnWindowWidthChanged(object? sender, WidthChangedEventArgs e)
    {
        foreach (var pair in _transactionRows)
        {
            pair.Value.IsSmall = e.SmallWidth;
        }
        if (e.SmallWidth)
        {
            _transactionsGroup.SetTitle("");
        }
        else
        {
            _transactionsGroup.SetTitle(_n("{0} transaction", "{0} transactions", _controller.FilteredTransactionsCount, _controller.FilteredTransactionsCount));
        }
    }
}
