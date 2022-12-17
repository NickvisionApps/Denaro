using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using System;
using System.IO;
using System.Collections.Generic;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public class MainWindow : Adw.ApplicationWindow
{
    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    private readonly Gtk.Box _mainBox;
    private readonly Adw.HeaderBar _headerBar;
    private readonly Adw.WindowTitle _windowTitle;
    private readonly Gtk.MenuButton _btnMenuAccount;
    private readonly Gtk.Popover _popoverAccount;
    private readonly Gtk.Box _popBoxAccount;
    private readonly Gtk.Box _popBoxButtons;
    private readonly Gtk.Button _popBtnNewAccount;
    private readonly Adw.ButtonContent _popBtnNewAccountContext;
    private readonly Gtk.Button _popBtnOpenAccount;
    private readonly List<Adw.ActionRow> _listRecentAccountsRows;
    private readonly Adw.PreferencesGroup _groupRecentAccounts;
    private readonly Gtk.ToggleButton _btnFlapToggle;
    private readonly Gtk.MenuButton _btnMenuHelp;
    private readonly Adw.ToastOverlay _toastOverlay;
    private readonly Adw.ViewStack _viewStack;
    private readonly Adw.StatusPage _pageStatusNoAccounts;
    private readonly Gtk.Box _boxStatusPage;
    private readonly Gtk.Label _lblRecentAccounts;
    private readonly List<Adw.ActionRow> _listRecentAccountsOnStartRows;
    private readonly Adw.PreferencesGroup _groupRecentAccountsOnStart;
    private readonly Gtk.Box _boxStatusButtons;
    private readonly Gtk.Button _btnNewAccount;
    private readonly Gtk.Button _btnOpenAccount;
    private readonly Gtk.Label _lblDrag;
    private readonly Gtk.Box _pageTabs;
    private readonly Adw.TabView _tabView;
    private readonly Adw.TabBar _tabBar;
    private readonly List<Adw.TabPage> _accountViews;
    private readonly Gtk.DropTarget _dropTarget;
    private readonly Gio.SimpleAction _actNewAccount;
    private readonly Gio.SimpleAction _actOpenAccount;
    private readonly Gio.SimpleAction _actCloseAccount;

    /// <summary>
    /// Constructs a MainWindow
    /// </summary>
    /// <param name="controller">The MainWindowController</param>
    /// <param name="application">The Adw.Application</param>
    public MainWindow(MainWindowController controller, Adw.Application application)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        SetDefaultSize(900, 700);
        if(_controller.IsDevVersion)
        {
            AddCssClass("devel");
        }
        //Main Box
        _mainBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        //Header Bar
        _headerBar = Adw.HeaderBar.New();
        _windowTitle = Adw.WindowTitle.New(_controller.AppInfo.ShortName, "");
        _headerBar.SetTitleWidget(_windowTitle);
        _mainBox.Append(_headerBar);
        //Account Popover
        _popoverAccount = Gtk.Popover.New();
        //Account Popover Buttons Box
        _popBoxButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        _popBoxButtons.AddCssClass("linked");
        _popBoxButtons.SetHalign(Gtk.Align.Center);
        _popBoxButtons.SetValign(Gtk.Align.Center);
        //Account Popover New Account Button
        _popBtnNewAccount = Gtk.Button.New();
        _popBtnNewAccount.AddCssClass("suggested-action");
        _popBtnNewAccountContext = Adw.ButtonContent.New();
        _popBtnNewAccountContext.SetLabel(_controller.Localizer["NewAccountPopover"]);
        _popBtnNewAccountContext.SetIconName("document-new-symbolic");
        _popBtnNewAccount.SetChild(_popBtnNewAccountContext);
        _popBtnNewAccount.SetTooltipText(_controller.Localizer["NewAccountTooltip"]);
        _popBtnNewAccount.SetDetailedActionName("win.newAccount");
        _popBoxButtons.Append(_popBtnNewAccount);
        //Account Popover Open Account Button
        _popBtnOpenAccount = Gtk.Button.New();
        _popBtnOpenAccount.SetIconName("document-open-symbolic");
        _popBtnOpenAccount.SetTooltipText(_controller.Localizer["OpenAccountTooltip"]);
        _popBtnOpenAccount.SetDetailedActionName("win.openAccount");
        _popBoxButtons.Append(_popBtnOpenAccount);
        //List Recent Accounts
        _groupRecentAccounts = Adw.PreferencesGroup.New();
        _groupRecentAccounts.SetTitle(_controller.Localizer["RecentsPopover"]);
        _groupRecentAccounts.SetHeaderSuffix(_popBoxButtons);
        _groupRecentAccounts.SetSizeRequest(200, 55);
        //Account Popover Box
        _popBoxAccount = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        _popBoxAccount.SetMarginStart(5);
        _popBoxAccount.SetMarginEnd(5);
        _popBoxAccount.SetMarginTop(5);
        _popBoxAccount.SetMarginBottom(5);
        _popBoxAccount.Append(_groupRecentAccounts);
        _popoverAccount.SetChild(_popBoxAccount);
        //Menu Account Button
        _btnMenuAccount = Gtk.MenuButton.New();
        _btnMenuAccount.SetVisible(false);
        _btnMenuAccount.SetIconName("bank-symbolic");
        _btnMenuAccount.SetPopover(_popoverAccount);
        _btnMenuAccount.SetTooltipText(_controller.Localizer["ButtonMenuAccountTooltip"]);
        _headerBar.PackStart(_btnMenuAccount);
        //Flap Toggle Button
        _btnFlapToggle = Gtk.ToggleButton.New();
        _btnFlapToggle.SetVisible(false);
        _btnFlapToggle.SetActive(true);
        _btnFlapToggle.SetIconName("sidebar-show-symbolic");
        _btnFlapToggle.SetTooltipText(_controller.Localizer["ToggleSidebarTooltip"]);
        _headerBar.PackStart(_btnFlapToggle);
        //Menu Help Button
        _btnMenuHelp = Gtk.MenuButton.New();
        var menuHelp = Gio.Menu.New();
        menuHelp.Append(_controller.Localizer["Preferences"], "win.preferences");
        menuHelp.Append(_controller.Localizer["KeyboardShortcuts"], "win.keyboardShortcuts");
        menuHelp.Append(string.Format(_controller.Localizer["About"], _controller.AppInfo.ShortName), "win.about");
        _btnMenuHelp.SetDirection(Gtk.ArrowType.None);
        _btnMenuHelp.SetMenuModel(menuHelp);
        _btnMenuHelp.SetTooltipText(_controller.Localizer["MainMenu"]);
        _headerBar.PackEnd(_btnMenuHelp);
        //Toast Overlay
        _toastOverlay = Adw.ToastOverlay.New();
        _toastOverlay.SetHexpand(true);
        _toastOverlay.SetVexpand(true);
        _mainBox.Append(_toastOverlay);
        //Status Buttons
        _boxStatusButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 12);
        _boxStatusButtons.SetHexpand(true);
        _boxStatusButtons.SetHalign(Gtk.Align.Center);
        //List Recent Accounts On The Start Screen
        _groupRecentAccountsOnStart = Adw.PreferencesGroup.New();
        _groupRecentAccountsOnStart.SetTitle(_controller.Localizer["RecentAccounts"]);
        _groupRecentAccountsOnStart.SetSizeRequest(200, 55);
        _groupRecentAccountsOnStart.SetMarginTop(24);
        _groupRecentAccountsOnStart.SetMarginBottom(24);
        _groupRecentAccountsOnStart.SetVisible(false);
        //New Account Button
        _btnNewAccount = Gtk.Button.NewWithLabel(_controller.Localizer["NewAccount"]);
        _btnNewAccount.SetHalign(Gtk.Align.Center);
        _btnNewAccount.SetSizeRequest(200, 50);
        _btnNewAccount.AddCssClass("pill");
        _btnNewAccount.AddCssClass("suggested-action");
        _btnNewAccount.SetDetailedActionName("win.newAccount");
        _boxStatusButtons.Append(_btnNewAccount);
        //Open Account Button
        _btnOpenAccount = Gtk.Button.NewWithLabel(_controller.Localizer["OpenAccount"]);
        _btnOpenAccount.SetHalign(Gtk.Align.Center);
        _btnOpenAccount.SetSizeRequest(200, 50);
        _btnOpenAccount.AddCssClass("pill");
        _btnOpenAccount.SetDetailedActionName("win.openAccount");
        _boxStatusButtons.Append(_btnOpenAccount);
        //Drag Label
        _lblDrag = Gtk.Label.New(_controller.Localizer["DragLabel"]);
        _lblDrag.AddCssClass("dim-label");
        _lblDrag.SetWrap(true);
        _lblDrag.SetJustify(Gtk.Justification.Center);
        //Status Page Box
        _boxStatusPage = Gtk.Box.New(Gtk.Orientation.Vertical, 12);
        _boxStatusPage.SetHexpand(false);
        _boxStatusPage.SetHalign(Gtk.Align.Center);
        _boxStatusPage.Append(_groupRecentAccountsOnStart);
        _boxStatusPage.Append(_boxStatusButtons);
        _boxStatusPage.Append(_lblDrag);
        //Page No Accounts
        _pageStatusNoAccounts = Adw.StatusPage.New();
        _pageStatusNoAccounts.SetIconName(_controller.ShowSun ? "sun-alt-symbolic" : "moon-symbolic");
        _pageStatusNoAccounts.SetTitle(_controller.Greeting);
        _pageStatusNoAccounts.SetDescription(_controller.Localizer["StartPageDescription"]);
        _pageStatusNoAccounts.SetChild(_boxStatusPage);
        //Page Tabs
        _pageTabs = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        _tabView = Adw.TabView.New();
        _tabView.OnClosePage += OnCloseAccountPage;
        _tabBar = Adw.TabBar.New();
        _tabBar.SetView(_tabView);
        _pageTabs.Append(_tabBar);
        _pageTabs.Append(_tabView);
        //View Stack
        _viewStack = Adw.ViewStack.New();
        _viewStack.AddNamed(_pageStatusNoAccounts, "pageNoAccounts");
        _viewStack.AddNamed(_pageTabs, "pageTabs");
        _toastOverlay.SetChild(_viewStack);
        //Layout
        SetContent(_mainBox);
        //Register Events
        _controller.NotificationSent += NotificationSent;
        _controller.AccountAdded += AccountAdded;
        //New Account Action
        _actNewAccount = Gio.SimpleAction.New("newAccount", null);
        _actNewAccount.OnActivate += OnNewAccount;
        AddAction(_actNewAccount);
        application.SetAccelsForAction("win.newAccount", new string[] { "<Ctrl>N" });
        //Open Account Action
        _actOpenAccount = Gio.SimpleAction.New("openAccount", null);
        _actOpenAccount.OnActivate += OnOpenAccount;
        AddAction(_actOpenAccount);
        application.SetAccelsForAction("win.openAccount", new string[] { "<Ctrl>O" });
        //Close Account Action
        _actCloseAccount = Gio.SimpleAction.New("closeAccount", null);
        _actCloseAccount.OnActivate += OnCloseAccount;
        AddAction(_actCloseAccount);
        application.SetAccelsForAction("win.closeAccount", new string[] { "<Ctrl>W" });
        _actCloseAccount.SetEnabled(false);
        //Preferences Action
        var actPreferences = Gio.SimpleAction.New("preferences", null);
        actPreferences.OnActivate += Preferences;
        AddAction(actPreferences);
        application.SetAccelsForAction("win.preferences", new string[] { "<Ctrl>comma" });
        //Keyboard Shortcuts Action
        var actKeyboardShortcuts = Gio.SimpleAction.New("keyboardShortcuts", null);
        actKeyboardShortcuts.OnActivate += KeyboardShortcuts;
        AddAction(actKeyboardShortcuts);
        application.SetAccelsForAction("win.keyboardShortcuts", new string[] { "<Ctrl>question" });
        //About Action
        var actAbout = Gio.SimpleAction.New("about", null);
        actAbout.OnActivate += About;
        AddAction(actAbout);
        application.SetAccelsForAction("win.about", new string[] { "F1" });
        //Drop Target
        // _dropTarget = Gtk.DropTarget.New(GLib.Type.File, Gdk.DragAction.Copy);
        // _dropTarget.OnCurrentDrop += OnDrop;
        // AddController(_dropTarget);
        //Initialize additional variables
        _listRecentAccountsRows = new List<Adw.ActionRow> {};
        _listRecentAccountsOnStartRows = new List<Adw.ActionRow> {};
        _accountViews = new List<Adw.TabPage> {};
    }

    /// <summary>
    /// Starts the MainWindow
    /// </summary>
    public void Start()
    {
        Show();
        if(_controller.RecentAccounts.Count > 0)
        {
            UpdateRecentAccountsOnStart();
            _pageStatusNoAccounts.SetDescription("");
            _groupRecentAccountsOnStart.SetVisible(true);
        }
    }

    /// <summary>
    /// Occurs when a notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotificationSentEventArgs</param>
    private void NotificationSent(object? sender, NotificationSentEventArgs e) => _toastOverlay.AddToast(Adw.Toast.New(e.Message));

    /// <summary>
    /// Occurs when an account is created or opened
    /// </summary>
    private void AccountAdded(object? sender, EventArgs e)
    {
        _actCloseAccount.SetEnabled(true);
        _viewStack.SetVisibleChildName("pageTabs");
        var newAccountView = new AccountView(this, _tabView, _btnFlapToggle, _controller.CreateAccountController(_controller.OpenAccounts.Count - 1));
        _tabView.SetSelectedPage(newAccountView.GetPage());
        _accountViews.Add(newAccountView.GetPage());
        _windowTitle.SetSubtitle(_controller.OpenAccounts.Count == 1 ? _controller.OpenAccounts[0] : null);
        UpdateRecentAccounts();
        _btnMenuAccount.SetVisible(true);
        _btnFlapToggle.SetVisible(true);
    }

    /// <summary>
    /// Creates a new account
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void OnNewAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _popoverAccount.Popdown();
        var saveFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["NewAccount"], this, Gtk.FileChooserAction.Save, _controller.Localizer["OK"], _controller.Localizer["Cancel"]);
        saveFileDialog.SetModal(true);
        var filter = Gtk.FileFilter.New();
        filter.SetName(_controller.Localizer["NMoneyFilter"]);
        filter.AddPattern("*.nmoney");
        saveFileDialog.AddFilter(filter);
        //saveFileDialog.OnResponse +=
        saveFileDialog.Show();
    }

    /// <summary>
    /// Opens a new account
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void OnOpenAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _popoverAccount.Popdown();
        var openFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["OpenAccount"], this, Gtk.FileChooserAction.Open, _controller.Localizer["OK"], _controller.Localizer["Cancel"]);
        openFileDialog.SetModal(true);
        var filter = Gtk.FileFilter.New();
        filter.SetName(_controller.Localizer["NMoneyFilter"]);
        filter.AddPattern("*.nmoney");
        openFileDialog.AddFilter(filter);
        //openFileDialog.OnResponse +=
        openFileDialog.Show();
    }

    /// <summary>
    /// Closes an opened account
    /// </summary>
    private void OnCloseAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _popoverAccount.Popdown();
        _tabView.ClosePage(_tabView.GetSelectedPage());
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Preferences(Gio.SimpleAction sender, EventArgs e)
    {
        var preferencesDialog = new PreferencesDialog(_controller.PreferencesViewController, _application, this);
        preferencesDialog.Show();
    }

    /// <summary>
    /// Occurs when the keyboard shortcuts action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void KeyboardShortcuts(Gio.SimpleAction sender, EventArgs e)
    {
        var shortcutsDialog = new ShortcutsDialog(_controller.Localizer, _controller.AppInfo.ShortName, this);
        shortcutsDialog.Show();
    }

    /// <summary>
    /// Occurs when the about action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void About(Gio.SimpleAction sender, EventArgs e)
    {
        var aboutWindow = Gtk.AboutDialog.New();
        aboutWindow.SetTransientFor(this);
        aboutWindow.SetModal(true);
        aboutWindow.SetProgramName(_controller.AppInfo.ShortName);
        aboutWindow.SetLogoIconName(_controller.AppInfo.ID);
        aboutWindow.SetVersion(_controller.AppInfo.Version);
        aboutWindow.SetComments(_controller.AppInfo.Description);
        aboutWindow.SetLicenseType(Gtk.License.Gpl30);
        aboutWindow.SetCopyright("© Nickvision 2021-2022");
        aboutWindow.SetWebsite(_controller.AppInfo.GitHubRepo.ToString());
        aboutWindow.SetAuthors(_controller.Localizer["Developers"].Split(Environment.NewLine));
        aboutWindow.SetArtists(new string[2] { "Nicholas Logozzo https://github.com/nlogozzo", "Fyodor Sobolev https://github.com/fsobolev" });
        aboutWindow.SetTranslatorCredits(string.IsNullOrEmpty(_controller.Localizer["TranslatorCredits"]) ? null : _controller.Localizer["TranslatorCredits"]);
        aboutWindow.Show();
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="dropValue">GObject.Value</param>
    /// <param name="e">EventArgs</param>
    private bool OnDrop(GObject.Value dropValue, EventArgs e)
    {
        var file = (Gio.File)dropValue.GetObject();
        var path = file.GetPath();
        if(Path.GetExtension(path) == ".nmoney")
        {
            _controller.AddAccount(path);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Occurs when an account page is closing
    /// </summary>
    /// <param name="page">Adw.TabPage</param>
    private void OnCloseAccountPage(Adw.TabView view, Adw.TabView.ClosePageSignalArgs args)
    {
        var indexPage = _tabView.GetPagePosition(args.Page);
        _controller.CloseAccount(indexPage);
        _accountViews.RemoveAt(indexPage);
        _windowTitle.SetSubtitle(_controller.OpenAccounts.Count == 1 ? _controller.OpenAccounts[0] : null);
        if(_controller.OpenAccounts.Count == 0)
        {
            _actCloseAccount.SetEnabled(false);
            _viewStack.SetVisibleChildName("pageNoAccounts");
            _btnMenuAccount.SetVisible(false);
            _btnFlapToggle.SetVisible(false);
            UpdateRecentAccountsOnStart();
            _groupRecentAccountsOnStart.SetVisible(true);
        }
        //return true; // FS: I didn't find how to use GDK_EVENT_STOP, but it equals to true anyway
    }

    /// <summary>
    /// Updates the list of recent accounts
    /// </summary>
    private void UpdateRecentAccounts()
    {
        foreach(var row in _listRecentAccountsRows)
        {
            _groupRecentAccounts.Remove(row);
        }
        _listRecentAccountsRows.Clear();
        foreach(var accountPath in _controller.RecentAccounts)
        {
            var row = CreateRecentAccountRow(accountPath);
            _groupRecentAccounts.Add(row);
            _listRecentAccountsRows.Add(row);
        }
    }

    /// <summary>
    /// Updates the list of recent accounts on start screen
    /// </summary>
    private void UpdateRecentAccountsOnStart()
    {
        foreach(var row in _listRecentAccountsOnStartRows)
        {
            _groupRecentAccountsOnStart.Remove(row);
        }
        _listRecentAccountsOnStartRows.Clear();
        foreach(var accountPath in _controller.RecentAccounts)
        {
            var row = CreateRecentAccountRow(accountPath);
            _groupRecentAccountsOnStart.Add(row);
            _listRecentAccountsOnStartRows.Add(row);
        }
    }

    /// <summary>
    /// Creates a row for recent accounts lists
    /// </summary>
    /// <param name="accountPath">string</param>
    private Adw.ActionRow CreateRecentAccountRow(string accountPath)
    {
        var row = Adw.ActionRow.New();
        row.SetTitle(Path.GetFileName(accountPath));
        row.SetSubtitle(accountPath);
        var button = Gtk.Button.NewFromIconName("wallet2-symbolic");
        button.SetHalign(Gtk.Align.Center);
        button.SetValign(Gtk.Align.Center);
        button.AddCssClass("wallet-button");
        button.OnClicked += delegate(Gtk.Button sender, EventArgs e) { OnOpenRecentAccount(sender, row.GetSubtitle(), e); };
        row.AddPrefix(button);
        row.SetActivatableWidget(button);
        return row;
    }

    private void OnOpenRecentAccount(Gtk.Widget sender, string path, EventArgs e)
    {
        _popoverAccount.Popdown();
        _controller.AddAccount(path);
    }
}
