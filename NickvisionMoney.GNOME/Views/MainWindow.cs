using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
public partial class MainWindow : Adw.ApplicationWindow
{
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

    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;

    [Gtk.Connect] private readonly Adw.WindowTitle _windowTitle;
    [Gtk.Connect] private readonly Gtk.MenuButton _accountMenuButton;
    [Gtk.Connect] private readonly Gtk.Popover _accountPopover;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _recentAccountsGroup;
    [Gtk.Connect] private readonly Gtk.ToggleButton _flapToggleButton;
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

    private GAsyncReadyCallback _saveCallback { get; set; }
    private GAsyncReadyCallback _openCallback { get; set; }

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
        OnShow += OnWindowShow;
        CompactMode = false;
        if (_controller.IsDevVersion)
        {
            AddCssClass("devel");
        }
        OnCloseRequest += OnCloseRequested;
        //Register Events
        _controller.NotificationSent += NotificationSent;
        _controller.AccountLoginAsync += AccountLoginAsync;
        _controller.AccountAdded += AccountAdded;
        _controller.RecentAccountsChanged += (object? sender, EventArgs e) =>
        {
            UpdateRecentAccountsOnStart();
            UpdateRecentAccounts();
        };
        OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "default-width")
            {
                OnWidthChanged();
            }
        };
        //Header Bar
        _windowTitle.SetTitle(_controller.AppInfo.ShortName);
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
        actHelp.OnActivate += (sender, e) => Gtk.Functions.ShowUri(this, "help:denaro", 0);
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
    public MainWindow(MainWindowController controller, Adw.Application application) : this(Builder.FromFile("window.ui", controller.Localizer, (s) => s == "About" ? string.Format(controller.Localizer[s], controller.AppInfo.ShortName) : controller.Localizer[s]), controller, application)
    {
    }

    /// <summary>
    /// Starts the MainWindow
    /// </summary>
    public void Startup()
    {
        _application.AddWindow(this);
        if (_controller.RecentAccounts.Count > 0)
        {
            UpdateRecentAccountsOnStart();
            _startPageRecentAccountsGroup.SetVisible(true);
        }
        Present();
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
            toast.OnButtonClicked += (sender, e) => Gtk.Functions.ShowUri(this, "help:denaro/import-export", 0);
        }
        _toastOverlay.AddToast(toast);
    }

    /// <summary>
    /// Occurs when the window tries to close
    /// </summary>
    /// <param name="sender">Gtk.Window</param>
    /// <param name="e">EventArgs</param>
    /// <returns>True to stop close, else false</returns>
    private bool OnCloseRequested(Gtk.Window sender, EventArgs e)
    {
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
        var passwordDialog = new PasswordDialog(this, title, _controller.Localizer, tcs);
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
        var newAccountView = new AccountView(_controller.GetMostRecentAccountViewController(), this, _tabView, _flapToggleButton, UpdateSubtitle);
        _tabView.SetSelectedPage(newAccountView.Page);
        _accountViews.Add(newAccountView.Page);
        _windowTitle.SetSubtitle(_controller.NumberOfOpenAccounts == 1 ? _controller.GetMostRecentAccountViewController().AccountTitle : "");
        _accountMenuButton.SetVisible(true);
        _flapToggleButton.SetVisible(true);
        await newAccountView.StartupAsync();
    }

    /// <summary>
    /// Occurs when the window is shown
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private async void OnWindowShow(Gtk.Widget sender, EventArgs e)
    {
        GLib.Functions.TimeoutAddFull(0, 100, (data) =>
        {
            var maxBtnWidth = Math.Max(_newAccountButton.GetAllocatedWidth(), _openAccountButton.GetAllocatedWidth());
            _newAccountButton.SetSizeRequest(maxBtnWidth, -1);
            _openAccountButton.SetSizeRequest(maxBtnWidth, -1);
            return false;
        });
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
        _accountPopover.Popdown();
        var filter = Gtk.FileFilter.New();
        filter.SetName($"{_controller.Localizer["NickvisionMoneyAccount"]} (*.nmoney)");
        filter.AddPattern("*.nmoney");
        var saveFileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(saveFileDialog, _controller.Localizer["NewAccount"]);
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filter);
        gtk_file_dialog_set_filters(saveFileDialog, filters.Handle);
        _saveCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_save_finish(saveFileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
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
        gtk_file_dialog_save(saveFileDialog, Handle, IntPtr.Zero, _saveCallback, IntPtr.Zero);
    }

    /// <summary>
    /// Opens a new account
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void OnOpenAccount(Gio.SimpleAction sender, EventArgs e)
    {
        _accountPopover.Popdown();
        var filter = Gtk.FileFilter.New();
        filter.SetName($"{_controller.Localizer["NickvisionMoneyAccount"]} (*.nmoney)");
        filter.AddPattern("*.nmoney");
        filter.AddPattern("*.NMONEY");
        var openFileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(openFileDialog, _controller.Localizer["OpenAccount"]);
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filter);
        gtk_file_dialog_set_filters(openFileDialog, filters.Handle);
        _openCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_open_finish(openFileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
                await _controller.AddAccountAsync(path);
            }
        };
        gtk_file_dialog_open(openFileDialog, Handle, IntPtr.Zero, _openCallback, IntPtr.Zero);
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
        if (_controller.NumberOfOpenAccounts == 0)
        {
            _viewStack.SetVisibleChildName("pageNoAccounts");
            _accountMenuButton.SetVisible(false);
            _flapToggleButton.SetVisible(false);
            UpdateRecentAccountsOnStart();
            _startPageRecentAccountsGroup.SetVisible(true);
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
        var builder = Builder.FromFile("shortcuts_dialog.ui", _controller.Localizer);
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
        var dialog = Adw.AboutWindow.New();
        dialog.SetTransientFor(this);
        dialog.SetIconName(_controller.AppInfo.ID);
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
        _newAccountButton.RemoveCssClass("suggested-action");
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
        var row = new RecentAccountRow(recentAccount, _controller.GetColorForAccountType(recentAccount.Type), onStartScreen, _controller.Localizer);
        row.OnOpenAccount += async (sender, e) =>
        {
            _accountPopover.Popdown();
            await _controller.AddAccountAsync(row.GetSubtitle()!);
        };
        return row;
    }

    /// <summary>
    /// Occurs when the window's width is changed
    /// </summary>
    public void OnWidthChanged()
    {
        var compactModeNeeded = DefaultWidth < 450;
        if (compactModeNeeded != CompactMode)
        {
            CompactMode = !CompactMode;
            WidthChanged?.Invoke(this, new WidthChangedEventArgs(compactModeNeeded));
        }
    }
}
