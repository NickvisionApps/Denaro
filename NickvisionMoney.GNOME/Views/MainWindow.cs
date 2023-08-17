using Nickvision.GirExt;
using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static NickvisionMoney.Shared.Helpers.Gettext;

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
public partial class MainWindow : Adw.ApplicationWindow
{
    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;

    [Gtk.Connect] private readonly Adw.HeaderBar _headerBar;
    [Gtk.Connect] private readonly Adw.WindowTitle _windowTitle;
    [Gtk.Connect] private readonly Gtk.MenuButton _accountMenuButton;
    [Gtk.Connect] private readonly Gtk.Popover _accountPopover;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStackAccountPopover;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _recentAccountsGroup;
    [Gtk.Connect] private readonly Gtk.ToggleButton _flapToggleButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _graphToggleButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _dashboardButton;
    [Gtk.Connect] private readonly Adw.Bin _dashboardBin;
    [Gtk.Connect] private readonly Adw.ToastOverlay _toastOverlay;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStack;
    [Gtk.Connect] private readonly Adw.ButtonContent _greeting;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _startPageRecentAccountsGroup;
    [Gtk.Connect] private readonly Gtk.Button _newAccountButton;
    [Gtk.Connect] private readonly Gtk.Button _openAccountButton;
    [Gtk.Connect] private readonly Adw.TabView _tabView;

    private readonly List<Adw.ActionRow> _listRecentAccountsRows;
    private readonly List<Adw.ActionRow> _listRecentAccountsOnStartRows;
    private readonly List<Adw.TabPage> _accountViews;
    private readonly Gtk.DropTarget _dropTarget;
    private readonly Gio.SimpleAction _actNewAccount;
    private readonly Gio.SimpleAction _actOpenAccount;
    private readonly Gio.SimpleAction _actCloseAccount;

    public bool CompactMode { get; private set; }

    /// <summary>
    /// Occurs when the window's width is changed
    /// </summary>
    public event EventHandler<WidthChangedEventArgs>? WidthChanged;

    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        _listRecentAccountsRows = new List<Adw.ActionRow>();
        _listRecentAccountsOnStartRows = new List<Adw.ActionRow>();
        _accountViews = new List<Adw.TabPage>();
        //Build UI
        builder.Connect(this);
        SetTitle(_controller.AppInfo.ShortName);
        SetIconName(_controller.AppInfo.ID);
        CompactMode = false;
        if (_controller.AppInfo.IsDevVersion)
        {
            AddCssClass("devel");
        }
        OnCloseRequest += OnCloseRequested;
        //Register Events
        _controller.NotificationSent += NotificationSent;
        _controller.AccountLoginAsync += AccountLoginAsync;
        _controller.AccountAdded += AccountAdded;
        _controller.RecentAccountsChanged += (sender, e) =>
        {
            GLib.Functions.IdleAdd(0, () =>
            {
                UpdateRecentAccountsOnStart();
                UpdateRecentAccounts();
                return false;
            });
        };
        OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "default-width" || e.Pspec.GetName() == "maximized")
            {
                OnWidthChanged();
            }
        };
        _dashboardButton.OnToggled += OnToggleDashboard;
        //Header Bar
        _windowTitle.SetTitle(_controller.AppInfo.ShortName);
        _graphToggleButton.SetActive(_controller.ShowGraphs);
        //Greeting
        _greeting.SetIconName(_controller.ShowSun ? "sun-outline-symbolic" : "moon-outline-symbolic");
        _greeting.SetLabel(_controller.Greeting);
        var image = (Gtk.Image)_greeting.GetFirstChild();
        image.SetPixelSize(48);
        image.SetMarginEnd(6);
        var label = (Gtk.Label)_greeting.GetLastChild();
        label.AddCssClass("greeting-title");
        _greeting.SetHalign(Gtk.Align.Center);
        _greeting.SetMarginTop(24);
        _greeting.SetMarginBottom(14);
        //Page Tabs
        _tabView.OnClosePage += OnCloseAccountPage;
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
        //New Window Action
        var actNewWindow = Gio.SimpleAction.New("newWindow", null);
        actNewWindow.OnActivate += (sender, e) => Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule!.FileName) { UseShellExecute = true });
        AddAction(actNewWindow);
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
        //Quit Action
        var actQuit = Gio.SimpleAction.New("quit", null);
        actQuit.OnActivate += (sender, e) => _application.Quit();
        AddAction(actQuit);
        application.SetAccelsForAction("win.quit", new string[] { "<Ctrl>q" });
        //Help Action
        var actHelp = Gio.SimpleAction.New("help", null);
        actHelp.OnActivate += (sender, e) => Gtk.Functions.ShowUri(this, Help.GetHelpURL("index"), 0);
        AddAction(actHelp);
        application.SetAccelsForAction("win.help", new string[] { "F1" });
        //About Action
        var actAbout = Gio.SimpleAction.New("about", null);
        actAbout.OnActivate += About;
        AddAction(actAbout);
        //Drop Target
        _dropTarget = Gtk.DropTarget.New(Gio.FileHelper.GetGType(), Gdk.DragAction.Copy);
        _dropTarget.OnDrop += OnDrop;
        AddController(_dropTarget);
    }

    /// <summary>
    /// Constructs a MainWindow
    /// </summary>
    /// <param name="controller">The MainWindowController</param>
    /// <param name="application">The Adw.Application</param>
    public MainWindow(MainWindowController controller, Adw.Application application) : this(Builder.FromFile("window.ui"), controller, application)
    {
    }

    /// <summary>
    /// Starts the MainWindow
    /// </summary>
    public async Task StartupAsync()
    {
        _application.AddWindow(this);
        UpdateRecentAccountsOnStart();
        Present();
        await _controller.StartupAsync();
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
            toast.SetButtonLabel(_("Help"));
            toast.OnButtonClicked += (s, ex) =>  Gtk.Functions.ShowUri(this, Help.GetHelpURL("import-export"), 0);
        }
        else if (e.Action == "open-export")
        {
            var file = Gio.FileHelper.NewForPath(e.ActionParam);
            var fileLauncher = Gtk.FileLauncher.New(file);
            toast.SetButtonLabel(_("Open"));
            toast.OnButtonClicked += async (s, ex) =>
            {
                try
                {
                    await fileLauncher.LaunchAsync(this);
                }
                catch { }
            };
        }
        _toastOverlay.AddToast(toast);
    }

    /// <summary>
    /// Sends a shell notification
    /// </summary>
    /// <param name="e">ShellNotificationSentEventArgs</param>
    private void SendShellNotification(ShellNotificationSentEventArgs e)
    {
        var notification = Gio.Notification.New(e.Title);
        notification.SetBody(e.Message);
        notification.SetPriority(e.Severity switch
        {
            NotificationSeverity.Success => Gio.NotificationPriority.High,
            NotificationSeverity.Warning => Gio.NotificationPriority.Urgent,
            NotificationSeverity.Error => Gio.NotificationPriority.Urgent,
            _ => Gio.NotificationPriority.Normal
        });
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            notification.SetIcon(Gio.ThemedIcon.New($"{_controller.AppInfo.ID}-symbolic"));
        }
        else
        {
            var fileIcon = Gio.FileIcon.New(Gio.FileHelper.NewForPath($"{Environment.GetEnvironmentVariable("SNAP")}/usr/share/icons/hicolor/symbolic/apps/{_controller.AppInfo.ID}-symbolic.svg"));
            notification.SetIcon(fileIcon);
        }
        _application.SendNotification(_controller.AppInfo.ID, notification);
    }

    /// <summary>
    /// Occurs when the window tries to close
    /// </summary>
    /// <param name="sender">Gtk.Window</param>
    /// <param name="e">EventArgs</param>
    /// <returns>True to stop close, else false</returns>
    private bool OnCloseRequested(Gtk.Window sender, EventArgs e)
    {
        _controller.ShowGraphs = _graphToggleButton.GetActive();
        _controller.Dispose();
        return false;
    }

    /// <summary>
    /// Updates the window's subtitle
    /// </summary>
    /// <param name="s">The new subtitle</param>
    private void UpdateSubtitle(string s) => _windowTitle.SetSubtitle(_controller.NumberOfOpenAccounts == 1 ? s : "");

    /// <summary>
    /// Occurs when an account needs a login
    /// </summary>
    /// <param name="title">The title of the account</param>
    public async Task<string?> AccountLoginAsync(string title)
    {
        var tcs = new TaskCompletionSource<string?>();
        var passwordDialog = new PasswordDialog(this, title, tcs);
        passwordDialog.SetIconName(_controller.AppInfo.ID);
        passwordDialog.Present();
        return await tcs.Task;
    }

    /// <summary>
    /// Occurs when an account is created or opened
    /// </summary>
    private async void AccountAdded(object? sender, EventArgs e)
    {
        _viewStack.SetVisibleChildName("pageTabs");
        _headerBar.RemoveCssClass("flat");
        var newAccountView = new AccountView(_controller.GetMostRecentAccountViewController(), this, _tabView, _flapToggleButton, _graphToggleButton, UpdateSubtitle);
        _tabView.SetSelectedPage(newAccountView.Page);
        _accountViews.Add(newAccountView.Page);
        _windowTitle.SetSubtitle(_controller.NumberOfOpenAccounts == 1 ? _controller.GetMostRecentAccountViewController().AccountTitle : "");
        _accountMenuButton.SetVisible(true);
        _flapToggleButton.SetVisible(true);
        _graphToggleButton.SetVisible(true);
        _dashboardButton.SetVisible(_controller.NumberOfOpenAccounts > 1);
        await newAccountView.StartupAsync();
    }

    /// <summary>
    /// Creates a new account
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void OnNewAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _accountPopover.Popdown();
        var newAccountController = _controller.CreateNewAccountDialogController();
        var newAccountDialog = new NewAccountDialog(newAccountController, this);
        newAccountDialog.OnApply += async (sender, e) =>
        {
            newAccountDialog.SetVisible(false);
            await _controller.NewAccountAsync(newAccountController);
            newAccountDialog.Close();
        };
        newAccountDialog.Present();
    }

    /// <summary>
    /// Opens a new account
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void OnOpenAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _accountPopover.Popdown();
        var openFileDialog = Gtk.FileDialog.New();
        openFileDialog.SetTitle(_("Open Account"));
        var filter = Gtk.FileFilter.New();
        filter.SetName($"{_("Nickvision Denaro Account")} (*.nmoney)");
        filter.AddPattern("*.nmoney");
        filter.AddPattern("*.NMONEY");
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filter);
        openFileDialog.SetFilters(filters);
        try
        {
            var file = await openFileDialog.OpenAsync(this);
            await _controller.AddAccountAsync(file.GetPath());
        }
        catch { }
    }

    /// <summary>
    /// Closes an opened account
    /// </summary>
    private void OnCloseAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _accountPopover.Popdown();
        if (_controller.NumberOfOpenAccounts == 0)
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
        _windowTitle.SetSubtitle(_controller.NumberOfOpenAccounts == 1 ? _controller.GetMostRecentAccountViewController().AccountTitle : "");
        _dashboardButton.SetVisible(_controller.NumberOfOpenAccounts > 1);
        if (_controller.NumberOfOpenAccounts == 0)
        {
            _viewStack.SetVisibleChildName("pageNoAccounts");
            _headerBar.AddCssClass("flat");
            _accountMenuButton.SetVisible(false);
            _flapToggleButton.SetVisible(false);
            _graphToggleButton.SetVisible(false);
        }
        _tabView.ClosePageFinish(args.Page, true);
        return true;
    }

    /// <summary>
    /// Occurs when dashboard should be opened or closed
    /// </summary>
    /// <param name="sender">Gtk.ToggleButton</param>
    /// <param name="e">EventArgs</param>
    private void OnToggleDashboard(Gtk.ToggleButton sender, EventArgs e)
    {
        if (sender.GetActive())
        {
            _dashboardBin.SetChild(new DashboardView(_controller.CreateDashboardViewController()));
            _viewStack.SetVisibleChildName("dashboard");
            _actCloseAccount.SetEnabled(false);
            _accountMenuButton.SetVisible(false);
            _flapToggleButton.SetVisible(false);
            _graphToggleButton.SetVisible(false);
        }
        else
        {
            _viewStack.SetVisibleChildName("pageTabs");
            _actCloseAccount.SetEnabled(true);
            _accountMenuButton.SetVisible(true);
            _flapToggleButton.SetVisible(true);
            _graphToggleButton.SetVisible(true);
        }
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Preferences(Gio.SimpleAction sender, EventArgs e)
    {
        var preferencesDialog = new PreferencesDialog(_controller.CreatePreferencesViewController(), _application, this);
        preferencesDialog.SetIconName(_controller.AppInfo.ID);
        preferencesDialog.Present();
    }

    /// <summary>
    /// Occurs when the keyboard shortcuts action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void KeyboardShortcuts(Gio.SimpleAction sender, EventArgs e)
    {
        var builder = Builder.FromFile("shortcuts_dialog.ui");
        var shortcutsWindow = (Gtk.ShortcutsWindow)builder.GetObject("_root");
        shortcutsWindow.SetTransientFor(this);
        shortcutsWindow.SetIconName(_controller.AppInfo.ID);
        shortcutsWindow.Present();
    }

    /// <summary>
    /// Occurs when the about action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void About(Gio.SimpleAction sender, EventArgs e)
    {
        var debugInfo = new StringBuilder();
        debugInfo.AppendLine(_controller.AppInfo.ID);
        debugInfo.AppendLine(_controller.AppInfo.Version);
        debugInfo.AppendLine($"GTK {Gtk.Functions.GetMajorVersion()}.{Gtk.Functions.GetMinorVersion()}.{Gtk.Functions.GetMicroVersion()}");
        debugInfo.AppendLine($"libadwaita {Adw.Functions.GetMajorVersion()}.{Adw.Functions.GetMinorVersion()}.{Adw.Functions.GetMicroVersion()}");
        if (File.Exists("/.flatpak-info"))
        {
            debugInfo.AppendLine("Flatpak");
        }
        else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            debugInfo.AppendLine("Snap");
        }
        debugInfo.AppendLine(CultureInfo.CurrentCulture.ToString());
        var localeProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "locale",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        try
        {
            localeProcess.Start();
            var localeString = localeProcess.StandardOutput.ReadToEnd().Trim();
            localeProcess.WaitForExit();
            debugInfo.AppendLine(localeString);
        }
        catch
        {
            debugInfo.AppendLine("Unknown locale");
        }
        var dialog = Adw.AboutWindow.New();
        dialog.SetTransientFor(this);
        dialog.SetIconName(_controller.AppInfo.ID);
        dialog.SetApplicationName(_controller.AppInfo.ShortName);
        dialog.SetApplicationIcon(_controller.AppInfo.ID + (_controller.AppInfo.IsDevVersion ? "-devel" : ""));
        dialog.SetVersion(_controller.AppInfo.Version);
        dialog.SetDebugInfo(debugInfo.ToString());
        dialog.SetComments(_controller.AppInfo.Description);
        dialog.SetDeveloperName("Nickvision");
        dialog.SetLicenseType(Gtk.License.MitX11);
        dialog.SetCopyright("© Nickvision 2021-2023");
        dialog.SetWebsite("https://nickvision.org/");
        dialog.SetIssueUrl(_controller.AppInfo.IssueTracker.ToString());
        dialog.SetSupportUrl(_controller.AppInfo.SupportUrl.ToString());
        dialog.AddLink(_("GitHub Repo"), _controller.AppInfo.SourceRepo.ToString());
        foreach (var pair in _controller.AppInfo.ExtraLinks)
        {
            dialog.AddLink(pair.Key, pair.Value.ToString());
        }
        dialog.SetDevelopers(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Developers));
        dialog.SetDesigners(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Designers));
        dialog.SetArtists(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Artists));
        dialog.SetTranslatorCredits(_controller.AppInfo.TranslatorCredits);
        dialog.SetReleaseNotes(_controller.AppInfo.HTMLChangelog);
        dialog.Present();
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gtk.DropTarget</param>
    /// <param name="e">Gtk.DropTarget.DropSignalArgs</param>
    private bool OnDrop(Gtk.DropTarget sender, Gtk.DropTarget.DropSignalArgs e)
    {
        var obj = e.Value.GetObject();
        if (obj is Gio.FileHelper file)
        {
            var path = file.GetPath() ?? "";
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
        _viewStackAccountPopover.SetVisibleChildName(_controller.RecentAccounts.Count > 0 ? "recents" : "no-recents");
        foreach (var row in _listRecentAccountsRows)
        {
            _recentAccountsGroup.Remove(row);
        }
        _listRecentAccountsRows.Clear();
        foreach (var recentAccount in _controller.RecentAccounts)
        {
            var row = CreateRecentAccountRow(recentAccount, false);
            _recentAccountsGroup.Add(row);
            _listRecentAccountsRows.Add(row);
        }
    }

    /// <summary>
    /// Updates the list of recent accounts on start screen
    /// </summary>
    private void UpdateRecentAccountsOnStart()
    {
        if(_controller.RecentAccounts.Count > 0)
        {
            _newAccountButton.RemoveCssClass("suggested-action");
        }
        else
        {
            _newAccountButton.AddCssClass("suggested-action");
        }
        _startPageRecentAccountsGroup.SetVisible(_controller.RecentAccounts.Count > 0);
        foreach (var row in _listRecentAccountsOnStartRows)
        {
            _startPageRecentAccountsGroup.Remove(row);
        }
        _listRecentAccountsOnStartRows.Clear();
        foreach (var recentAccount in _controller.RecentAccounts)
        {
            var row = CreateRecentAccountRow(recentAccount, true);
            _startPageRecentAccountsGroup.Add(row);
            _listRecentAccountsOnStartRows.Add(row);
        }
    }

    /// <summary>
    /// Creates a row for recent accounts lists
    /// </summary>
    /// <param name="recentAccount">Account to create the row for</param>
    /// <param name="onStartScreen">Whether the row will appear on start screen or in popover</param>
    private Adw.ActionRow CreateRecentAccountRow(RecentAccount recentAccount, bool onStartScreen)
    {
        var row = new RecentAccountRow(recentAccount, _controller.GetColorForAccountType(recentAccount.Type), onStartScreen, true);
        row.Selected += async (sender, e) =>
        {
            _accountPopover.Popdown();
            await _controller.AddAccountAsync(e.Path);
        };
        row.RemoveRequested += (sender, e) =>
        {
            _accountPopover.Popdown();
            _controller.RemoveRecentAccount(e);
        };
        return row;
    }

    /// <summary>
    /// Occurs when the window's width is changed
    /// </summary>
    public void OnWidthChanged()
    {
        var compactModeNeeded = DefaultWidth < 450 && !IsMaximized();
        if (compactModeNeeded != CompactMode)
        {
            CompactMode = !CompactMode;
            WidthChanged?.Invoke(this, new WidthChangedEventArgs(compactModeNeeded));
        }
    }
}
