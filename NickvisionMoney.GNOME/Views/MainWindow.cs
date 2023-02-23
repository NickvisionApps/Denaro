using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// EventArgs for WidthChanged Event
/// </summary>
public class WidthChangedEventArgs : EventArgs
{
    public bool SmallWidth { get; init; }

    public WidthChangedEventArgs(bool smallWidth) => SmallWidth = smallWidth;
}

/// <summary>
/// The MainWindow for the application
/// </summary>
public partial class MainWindow
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

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
    private readonly Gtk.MenuButton _btnMainMenu;
    private readonly Adw.ToastOverlay _toastOverlay;
    private readonly Adw.ViewStack _viewStack;
    private readonly Gtk.ScrolledWindow _scrollStartPage;
    private readonly Adw.Clamp _clampStartPage;
    private readonly Gtk.Box _boxStartPage;
    private readonly Adw.ButtonContent _greeting;
    private readonly List<Adw.ActionRow> _listRecentAccountsOnStartRows;
    private readonly Adw.PreferencesGroup _grpRecentAccountsOnStart;
    private readonly Gtk.FlowBox _flowBoxStatusButtons;
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

    public Adw.ApplicationWindow Handle { get; init; }
    public bool CompactMode { get; private set; }

    /// <summary>
    /// Occurs when the window's width is changed
    /// </summary>
    public event EventHandler<WidthChangedEventArgs>? WidthChanged;

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
        _listRecentAccountsRows = new List<Adw.ActionRow>();
        _listRecentAccountsOnStartRows = new List<Adw.ActionRow>();
        _accountViews = new List<Adw.TabPage>();
        Handle = Adw.ApplicationWindow.New(_application);
        Handle.SetDefaultSize(900, 720);
        Handle.SetSizeRequest(360, -1);
        Handle.SetTitle(_controller.AppInfo.ShortName);
        Handle.OnShow += OnShow;
        CompactMode = false;
        if (_controller.IsDevVersion)
        {
            Handle.AddCssClass("devel");
        }
        //Register Events
        _controller.NotificationSent += NotificationSent;
        _controller.AccountLoginAsync += AccountLoginAsync;
        _controller.AccountAdded += AccountAdded;
        _controller.RecentAccountsChanged += (object? sender, EventArgs e) =>
        {
            UpdateRecentAccountsOnStart();
            UpdateRecentAccounts();
        };
        Handle.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "default-width")
            {
                OnWidthChanged();
            }
        };
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
        _popBtnNewAccountContext.SetLabel(_controller.Localizer["New"]);
        _popBtnNewAccountContext.SetIconName("document-new-symbolic");
        _popBtnNewAccount.SetChild(_popBtnNewAccountContext);
        _popBtnNewAccount.SetTooltipText(_controller.Localizer["NewAccount", "Tooltip"]);
        _popBtnNewAccount.SetDetailedActionName("win.newAccount");
        _popBoxButtons.Append(_popBtnNewAccount);
        //Account Popover Open Account Button
        _popBtnOpenAccount = Gtk.Button.New();
        _popBtnOpenAccount.SetIconName("document-open-symbolic");
        _popBtnOpenAccount.SetTooltipText(_controller.Localizer["OpenAccount", "Tooltip"]);
        _popBtnOpenAccount.SetDetailedActionName("win.openAccount");
        _popBoxButtons.Append(_popBtnOpenAccount);
        //List Recent Accounts
        _groupRecentAccounts = Adw.PreferencesGroup.New();
        _groupRecentAccounts.SetTitle(_controller.Localizer["Recents", "GTK"]);
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
        _btnMenuAccount.SetTooltipText(_controller.Localizer["AccountMenu", "GTK"]);
        _headerBar.PackStart(_btnMenuAccount);
        //Flap Toggle Button
        _btnFlapToggle = Gtk.ToggleButton.New();
        _btnFlapToggle.SetVisible(false);
        _btnFlapToggle.SetActive(true);
        _btnFlapToggle.SetIconName("sidebar-show-symbolic");
        _btnFlapToggle.SetTooltipText(_controller.Localizer["ToggleSidebar", "GTK"]);
        _headerBar.PackStart(_btnFlapToggle);
        //Main Menu Button
        _btnMainMenu = Gtk.MenuButton.New();
        var mainMenu = Gio.Menu.New();
        mainMenu.Append(_controller.Localizer["NewWindow.GTK"], "win.newWindow");
        var appMenuSection = Gio.Menu.New();
        appMenuSection.Append(_controller.Localizer["Preferences"], "win.preferences");
        appMenuSection.Append(_controller.Localizer["KeyboardShortcuts"], "win.keyboardShortcuts");
        appMenuSection.Append(_controller.Localizer["Help"], "win.help");
        appMenuSection.Append(string.Format(_controller.Localizer["About"], _controller.AppInfo.ShortName), "win.about");
        mainMenu.AppendSection(null, appMenuSection);
        _btnMainMenu.SetDirection(Gtk.ArrowType.None);
        _btnMainMenu.SetMenuModel(mainMenu);
        _btnMainMenu.SetTooltipText(_controller.Localizer["MainMenu", "GTK"]);
        _btnMainMenu.SetPrimary(true);
        _headerBar.PackEnd(_btnMainMenu);
        //Toast Overlay
        _toastOverlay = Adw.ToastOverlay.New();
        _toastOverlay.SetHexpand(true);
        _toastOverlay.SetVexpand(true);
        _mainBox.Append(_toastOverlay);
        //Greeting
        _greeting = Adw.ButtonContent.New();
        _greeting.SetIconName(_controller.ShowSun ? "sun-outline-symbolic" : "moon-outline-symbolic");
        _greeting.SetLabel(_controller.Greeting);
        _greeting.AddCssClass("title-2");
        var image = (Gtk.Image)_greeting.GetFirstChild();
        image.SetIconSize(Gtk.IconSize.Large);
        _greeting.SetHalign(Gtk.Align.Center);
        _greeting.SetMarginBottom(32);
        //Drag Label
        _lblDrag = Gtk.Label.New(_controller.Localizer["NoAccountDescription"]);
        _lblDrag.AddCssClass("dim-label");
        _lblDrag.SetWrap(true);
        _lblDrag.SetJustify(Gtk.Justification.Center);
        _lblDrag.SetVisible(_controller.RecentAccounts.Count == 0);
        //Recent Accounts On Start Page
        _grpRecentAccountsOnStart = Adw.PreferencesGroup.New();
        _grpRecentAccountsOnStart.SetTitle(_controller.Localizer["RecentAccounts"]);
        _grpRecentAccountsOnStart.SetSizeRequest(200, 55);
        _grpRecentAccountsOnStart.SetMarginTop(24);
        _grpRecentAccountsOnStart.SetMarginBottom(24);
        _grpRecentAccountsOnStart.SetVisible(false);
        //Status Buttons
        _flowBoxStatusButtons = Gtk.FlowBox.New();
        _flowBoxStatusButtons.SetColumnSpacing(4);
        _flowBoxStatusButtons.SetRowSpacing(4);
        _flowBoxStatusButtons.SetMaxChildrenPerLine(2);
        _flowBoxStatusButtons.SetHomogeneous(true);
        _flowBoxStatusButtons.SetHexpand(true);
        _flowBoxStatusButtons.SetHalign(Gtk.Align.Fill);
        _flowBoxStatusButtons.SetSelectionMode(Gtk.SelectionMode.None);
        //New Account Button
        var btnNewAccountContainer = Gtk.FlowBoxChild.New();
        btnNewAccountContainer.SetFocusable(false);
        _btnNewAccount = Gtk.Button.NewWithLabel(_controller.Localizer["NewAccount"]);
        _btnNewAccount.SetHalign(Gtk.Align.Center);
        _btnNewAccount.AddCssClass("pill");
        _btnNewAccount.AddCssClass("suggested-action");
        _btnNewAccount.SetDetailedActionName("win.newAccount");
        btnNewAccountContainer.SetChild(_btnNewAccount);
        _flowBoxStatusButtons.Append(btnNewAccountContainer);
        //Open Account Button
        var btnOpenAccountContainer = Gtk.FlowBoxChild.New();
        btnOpenAccountContainer.SetFocusable(false);
        _btnOpenAccount = Gtk.Button.NewWithLabel(_controller.Localizer["OpenAccount"]);
        _btnOpenAccount.SetHalign(Gtk.Align.Center);
        _btnOpenAccount.AddCssClass("pill");
        _btnOpenAccount.SetDetailedActionName("win.openAccount");
        btnOpenAccountContainer.SetChild(_btnOpenAccount);
        _flowBoxStatusButtons.Append(btnOpenAccountContainer);
        //Start Page
        _scrollStartPage = Gtk.ScrolledWindow.New();
        _clampStartPage = Adw.Clamp.New();
        _clampStartPage.SetMaximumSize(450);
        _clampStartPage.SetValign(Gtk.Align.Center);
        _clampStartPage.SetMarginStart(12);
        _clampStartPage.SetMarginEnd(12);
        _clampStartPage.SetMarginTop(12);
        _clampStartPage.SetMarginBottom(12);
        _scrollStartPage.SetChild(_clampStartPage);
        _boxStartPage = Gtk.Box.New(Gtk.Orientation.Vertical, 12);
        _boxStartPage.SetHexpand(true);
        _boxStartPage.SetHalign(Gtk.Align.Fill);
        _clampStartPage.SetChild(_boxStartPage);
        _boxStartPage.Append(_greeting);
        _boxStartPage.Append(_lblDrag);
        _boxStartPage.Append(_grpRecentAccountsOnStart);
        _boxStartPage.Append(_flowBoxStatusButtons);
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
        _viewStack.AddNamed(_scrollStartPage, "pageNoAccounts");
        _viewStack.AddNamed(_pageTabs, "pageTabs");
        _toastOverlay.SetChild(_viewStack);
        //Layout
        Handle.SetContent(_mainBox);
        //New Account Action
        _actNewAccount = Gio.SimpleAction.New("newAccount", null);
        _actNewAccount.OnActivate += OnNewAccount;
        Handle.AddAction(_actNewAccount);
        application.SetAccelsForAction("win.newAccount", new string[] { "<Ctrl>N" });
        //Open Account Action
        _actOpenAccount = Gio.SimpleAction.New("openAccount", null);
        _actOpenAccount.OnActivate += OnOpenAccount;
        Handle.AddAction(_actOpenAccount);
        application.SetAccelsForAction("win.openAccount", new string[] { "<Ctrl>O" });
        //Close Account Action
        _actCloseAccount = Gio.SimpleAction.New("closeAccount", null);
        _actCloseAccount.OnActivate += OnCloseAccount;
        Handle.AddAction(_actCloseAccount);
        application.SetAccelsForAction("win.closeAccount", new string[] { "<Ctrl>W" });
        //New Window Action
        var actNewWindow = Gio.SimpleAction.New("newWindow", null);
        actNewWindow.OnActivate += (sender, e) => Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule!.FileName) { UseShellExecute = true });
        Handle.AddAction(actNewWindow);
        //Preferences Action
        var actPreferences = Gio.SimpleAction.New("preferences", null);
        actPreferences.OnActivate += Preferences;
        Handle.AddAction(actPreferences);
        application.SetAccelsForAction("win.preferences", new string[] { "<Ctrl>comma" });
        //Keyboard Shortcuts Action
        var actKeyboardShortcuts = Gio.SimpleAction.New("keyboardShortcuts", null);
        actKeyboardShortcuts.OnActivate += KeyboardShortcuts;
        Handle.AddAction(actKeyboardShortcuts);
        application.SetAccelsForAction("win.keyboardShortcuts", new string[] { "<Ctrl>question" });
        //Quit Action
        var actQuit = Gio.SimpleAction.New("quit", null);
        actQuit.OnActivate += (sender, e) => _application.Quit();
        Handle.AddAction(actQuit);
        application.SetAccelsForAction("win.quit", new string[] { "<Ctrl>q" });
        //Help Action
        var actHelp = Gio.SimpleAction.New("help", null);
        actHelp.OnActivate += (sender, e) => Gtk.Functions.ShowUri(Handle, "help:denaro", 0);
        Handle.AddAction(actHelp);
        application.SetAccelsForAction("win.help", new string[] { "F1" });
        //About Action
        var actAbout = Gio.SimpleAction.New("about", null);
        actAbout.OnActivate += About;
        Handle.AddAction(actAbout);
        //Drop Target
        _dropTarget = Gtk.DropTarget.New(Gio.FileHelper.GetGType(), Gdk.DragAction.Copy);
        _dropTarget.OnDrop += OnDrop;
        Handle.AddController(_dropTarget);
    }

    /// <summary>
    /// Starts the MainWindow
    /// </summary>
    public void Startup()
    {
        _application.AddWindow(Handle);
        if (_controller.RecentAccounts.Count > 0)
        {
            UpdateRecentAccountsOnStart();
            _grpRecentAccountsOnStart.SetVisible(true);
        }
        Handle.Show();
    }

    /// <summary>
    /// Occurs when a notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotificationSentEventArgs</param>
    private void NotificationSent(object? sender, NotificationSentEventArgs e)
    {
        var toast = Adw.Toast.New(e.Message);
        if (e.Action == "help-import")
        {
            toast.SetButtonLabel(_controller.Localizer["Help"]);
            toast.OnButtonClicked += (sender, e) => Gtk.Functions.ShowUri(Handle, "help:denaro/import-export", 0);
        }
        _toastOverlay.AddToast(toast);
    }

    /// <summary>
    /// Updates the window's subtitle
    /// </summary>
    /// <param name="s">The new subtitle</param>
    private void UpdateSubtitle(string s) => _windowTitle.SetSubtitle(_controller.OpenAccounts.Count == 1 ? s : "");

    /// <summary>
    /// Occurs when an account needs a login
    /// </summary>
    /// <param name="title">The title of the account</param>
    public async Task<string?> AccountLoginAsync(string title)
    {
        var passwordDialog = new PasswordDialog(Handle, title, _controller.Localizer);
        return await passwordDialog.RunAsync();
    }

    /// <summary>
    /// Occurs when an account is created or opened
    /// </summary>
    private async void AccountAdded(object? sender, EventArgs e)
    {
        _viewStack.SetVisibleChildName("pageTabs");
        var newAccountView = new AccountView(_controller.OpenAccounts[_controller.OpenAccounts.Count - 1], this, _tabView, _btnFlapToggle, UpdateSubtitle);
        _tabView.SetSelectedPage(newAccountView.Page);
        _accountViews.Add(newAccountView.Page);
        _windowTitle.SetSubtitle(_controller.OpenAccounts.Count == 1 ? _controller.OpenAccounts[0].AccountTitle : "");
        _btnMenuAccount.SetVisible(true);
        _btnFlapToggle.SetVisible(true);
        await newAccountView.StartupAsync();
    }

    /// <summary>
    /// Occurs when the window is shown
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private async void OnShow(Gtk.Widget sender, EventArgs e)
    {
        if (_controller.FileToLaunch != null)
        {
            GLib.Functions.TimeoutAddFull(0, 250, (data) =>
            {
                _controller.AddAccountAsync(_controller.FileToLaunch);
                _controller.FileToLaunch = null;
                return false;
            });
        }
    }

    /// <summary>
    /// Creates a new account
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void OnNewAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _popoverAccount.Popdown();
        var saveFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["NewAccount"], Handle, Gtk.FileChooserAction.Save, _controller.Localizer["Save"], _controller.Localizer["Cancel"]);
        saveFileDialog.SetModal(true);
        var filter = Gtk.FileFilter.New();
        filter.SetName($"{_controller.Localizer["NickvisionMoneyAccount"]} (*.nmoney)");
        filter.AddPattern("*.nmoney");
        saveFileDialog.AddFilter(filter);
        saveFileDialog.OnResponse += async (sender, e) =>
        {
            if (e.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var path = saveFileDialog.GetFile()!.GetPath() ?? "";
                if (_controller.IsAccountOpen(path))
                {
                    _toastOverlay.AddToast(Adw.Toast.New(_controller.Localizer["UnableToOverride"]));
                }
                else
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    await _controller.AddAccountAsync(path);
                }
            }
        };
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
        var openFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["OpenAccount"], Handle, Gtk.FileChooserAction.Open, _controller.Localizer["Open"], _controller.Localizer["Cancel"]);
        openFileDialog.SetModal(true);
        var filter = Gtk.FileFilter.New();
        filter.SetName($"{_controller.Localizer["NickvisionMoneyAccount"]} (*.nmoney)");
        filter.AddPattern("*.nmoney");
        filter.AddPattern("*.NMONEY");
        openFileDialog.AddFilter(filter);
        openFileDialog.OnResponse += async (sender, e) =>
        {
            if (e.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var path = openFileDialog.GetFile()!.GetPath() ?? "";
                await _controller.AddAccountAsync(path);
            }
        };
        openFileDialog.Show();
    }

    /// <summary>
    /// Closes an opened account
    /// </summary>
    private void OnCloseAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _popoverAccount.Popdown();
        if (_controller.OpenAccounts.Count == 0)
        {
            _application.Quit();
            return;
        }
        _tabView.ClosePage(_tabView.GetSelectedPage()!);
    }

    /// <summary>
    /// Occurs when an account page is closing
    /// </summary>
    /// <param name="view">Adw.TabView</param>
    /// <param name="args">Adw.TabView.ClosePageSignalArgs</param>
    private bool OnCloseAccountPage(Adw.TabView view, Adw.TabView.ClosePageSignalArgs args)
    {
        var indexPage = _tabView.GetPagePosition(args.Page);
        _controller.CloseAccount(indexPage);
        _accountViews.RemoveAt(indexPage);
        _windowTitle.SetSubtitle(_controller.OpenAccounts.Count == 1 ? _controller.OpenAccounts[0].AccountTitle : "");
        if (_controller.OpenAccounts.Count == 0)
        {
            _viewStack.SetVisibleChildName("pageNoAccounts");
            _btnMenuAccount.SetVisible(false);
            _btnFlapToggle.SetVisible(false);
            UpdateRecentAccountsOnStart();
            _grpRecentAccountsOnStart.SetVisible(true);
        }
        _tabView.ClosePageFinish(args.Page, true);
        return true;
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Preferences(Gio.SimpleAction sender, EventArgs e)
    {
        var preferencesDialog = new PreferencesDialog(_controller.PreferencesViewController, _application, Handle);
        preferencesDialog.Show();
    }

    /// <summary>
    /// Occurs when the keyboard shortcuts action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void KeyboardShortcuts(Gio.SimpleAction sender, EventArgs e)
    {
        var shortcutsDialog = new ShortcutsDialog(_controller.Localizer, Handle);
        shortcutsDialog.Show();
    }

    /// <summary>
    /// Occurs when the about action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void About(Gio.SimpleAction sender, EventArgs e)
    {
        var dialog = Adw.AboutWindow.New();
        dialog.SetTransientFor(Handle);
        dialog.SetApplicationName(_controller.AppInfo.ShortName);
        dialog.SetApplicationIcon(_controller.AppInfo.ID + (_controller.AppInfo.GetIsDevelVersion() ? "-devel" : ""));
        dialog.SetVersion(_controller.AppInfo.Version);
        dialog.SetComments(_controller.AppInfo.Description);
        dialog.SetDeveloperName("Nickvision");
        dialog.SetLicenseType(Gtk.License.MitX11);
        dialog.SetCopyright("© Nickvision 2021-2023");
        dialog.SetWebsite(_controller.AppInfo.GitHubRepo.ToString());
        dialog.SetIssueUrl(_controller.AppInfo.IssueTracker.ToString());
        dialog.SetSupportUrl(_controller.AppInfo.SupportUrl.ToString());
        dialog.AddLink(_controller.Localizer["MatrixChat"], "https://matrix.to/#/#nickvision:matrix.org");
        dialog.SetDevelopers(_controller.Localizer["Developers", "Credits"].Split(Environment.NewLine));
        dialog.SetDesigners(_controller.Localizer["Designers", "Credits"].Split(Environment.NewLine));
        dialog.SetArtists(_controller.Localizer["Artists", "Credits"].Split(Environment.NewLine));
        dialog.SetTranslatorCredits((string.IsNullOrEmpty(_controller.Localizer["Translators", "Credits"]) ? "" : _controller.Localizer["Translators", "Credits"]));
        dialog.SetReleaseNotes(_controller.AppInfo.Changelog);
        dialog.Show();
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gtk.DropTarget</param>
    /// <param name="e">Gtk.DropTarget.DropSignalArgs</param>
    private bool OnDrop(Gtk.DropTarget sender, Gtk.DropTarget.DropSignalArgs e)
    {
        var obj = e.Value.GetObject();
        if (obj != null)
        {
            var path = g_file_get_path(obj.Handle);
            if (File.Exists(path))
            {
                _controller.AddAccountAsync(path).Wait();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Updates the list of recent accounts
    /// </summary>
    private void UpdateRecentAccounts()
    {
        foreach (var row in _listRecentAccountsRows)
        {
            _groupRecentAccounts.Remove(row);
        }
        _listRecentAccountsRows.Clear();
        foreach (var recentAccount in _controller.RecentAccounts)
        {
            var row = CreateRecentAccountRow(recentAccount, false);
            _groupRecentAccounts.Add(row);
            _listRecentAccountsRows.Add(row);
        }
    }

    /// <summary>
    /// Updates the list of recent accounts on start screen
    /// </summary>
    private void UpdateRecentAccountsOnStart()
    {
        _btnNewAccount.RemoveCssClass("suggested-action");
        foreach (var row in _listRecentAccountsOnStartRows)
        {
            _grpRecentAccountsOnStart.Remove(row);
        }
        _listRecentAccountsOnStartRows.Clear();
        foreach (var recentAccount in _controller.RecentAccounts)
        {
            var row = CreateRecentAccountRow(recentAccount, true);
            _grpRecentAccountsOnStart.Add(row);
            _listRecentAccountsOnStartRows.Add(row);
        }
    }

    /// <summary>
    /// Creates a row for recent accounts lists
    /// </summary>
    /// <param name="accountPath">string</param>
    private Adw.ActionRow CreateRecentAccountRow(RecentAccount recentAccount, bool onStartScreen)
    {
        var row = Adw.ActionRow.New();
        row.SetTitle(recentAccount.Name);
        row.SetSubtitle(recentAccount.Path);
        var button = Gtk.Button.NewFromIconName("wallet2-symbolic");
        button.SetHalign(Gtk.Align.Center);
        button.SetValign(Gtk.Align.Center);
        button.SetFocusable(false);
        if (onStartScreen)
        {
            button.AddCssClass("wallet-button");
            var strType = _controller.Localizer["AccountType", recentAccount.Type.ToString()];
            var btnType = Gtk.Button.NewWithLabel(strType);
            btnType.SetValign(Gtk.Align.Center);
            btnType.SetSizeRequest(100, -1);
            btnType.SetFocusable(false);
            btnType.SetCanTarget(false);
            var bgColorString = _controller.GetColorForAccountType(recentAccount.Type);
            var bgColorStrArray = new Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
            var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
            var btnCssProvider = Gtk.CssProvider.New();
            var btnCss = "#btnType { color: " + (luma < 0.5 ? "#fff" : "#000") + "; background-color: " + bgColorString + "; }";
            gtk_css_provider_load_from_data(btnCssProvider.Handle, btnCss, btnCss.Length);
            btnType.SetName("btnType");
            btnType.GetStyleContext().AddProvider(btnCssProvider, 800);
            btnType.AddCssClass("account-tag");
            row.AddSuffix(btnType);
        }
        else
        {
            var bgColorString = _controller.GetColorForAccountType(recentAccount.Type);
            var bgColorStrArray = new Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
            var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
            var btnCssProvider = Gtk.CssProvider.New();
            var btnCss = "#btnWallet { color: " + (luma < 0.5 ? "#fff" : "#000") + "; background-color: " + bgColorString + "; }";
            gtk_css_provider_load_from_data(btnCssProvider.Handle, btnCss, btnCss.Length);
            button.SetName("btnWallet");
            button.GetStyleContext().AddProvider(btnCssProvider, 800);
        }
        button.OnClicked += async (Gtk.Button sender, EventArgs e) =>
        {
            _popoverAccount.Popdown();
            await _controller.AddAccountAsync(row.GetSubtitle()!);
        };
        row.AddPrefix(button);
        row.SetActivatableWidget(button);
        return row;
    }

    /// <summary>
    /// Occurs when the window's width is changed
    /// </summary>
    public void OnWidthChanged()
    {
        var compactModeNeeded = Handle.DefaultWidth < 450;
        if (compactModeNeeded != CompactMode)
        {
            CompactMode = !CompactMode;
            WidthChanged?.Invoke(this, new WidthChangedEventArgs(compactModeNeeded));
        }
    }
}
