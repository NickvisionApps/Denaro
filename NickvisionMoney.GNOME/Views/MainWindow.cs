using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using System;

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
    private readonly Gtk.Button _btnOpenFolder;
    private readonly Gtk.Button _btnCloseFolder;
    private readonly Gtk.MenuButton _btnMenuHelp;
    private readonly Adw.ToastOverlay _toastOverlay;
    private readonly Adw.ViewStack _viewStack;
    private readonly Adw.StatusPage _pageNoFolder;

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
        New();
        SetDefaultSize(900, 700);
        if(_controller.IsDevVersion)
        {
            AddCssClass("devel");
        }
        //Main Box
        _mainBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        //Header Bar
        _headerBar = Adw.HeaderBar.New();
        _windowTitle = Adw.WindowTitle.New(_controller.AppInfo.ShortName, _controller.FolderPath == "No Folder Opened" ? _controller.Localizer["NoFolderOpened"] : _controller.FolderPath);
        _headerBar.SetTitleWidget(_windowTitle);
        _mainBox.Append(_headerBar);
        //Open Folder Button
        _btnOpenFolder = Gtk.Button.New();
        var btnOpenFolderContent = Adw.ButtonContent.New();
        btnOpenFolderContent.SetLabel(_controller.Localizer["Open"]);
        btnOpenFolderContent.SetIconName("folder-open-symbolic");
        _btnOpenFolder.SetChild(btnOpenFolderContent);
        _btnOpenFolder.SetTooltipText(_controller.Localizer["OpenFolderTooltip"]);
        _btnOpenFolder.SetActionName("win.openFolder");
        _headerBar.PackStart(_btnOpenFolder);
        //Close Folder Button
        _btnCloseFolder = Gtk.Button.New();
        _btnCloseFolder.SetIconName("window-close-symbolic");
        _btnOpenFolder.SetTooltipText(_controller.Localizer["CloseFolderTooltip"]);
        _btnCloseFolder.SetVisible(false);
        _btnCloseFolder.SetActionName("win.closeFolder");
        _headerBar.PackStart(_btnCloseFolder);
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
        //View Stack
        _viewStack = Adw.ViewStack.New();
        _toastOverlay.SetChild(_viewStack);
        //No Folder Page
        _pageNoFolder = Adw.StatusPage.New();
        _pageNoFolder.SetIconName("folder-symbolic");
        _pageNoFolder.SetTitle(_controller.Localizer["NoFolderOpened"]);
        _pageNoFolder.SetDescription(_controller.Localizer["NoFolderDescription"]);
        _viewStack.AddNamed(_pageNoFolder, "NoFolder");
        //Folder Page
        var pageFolder = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        _viewStack.AddNamed(pageFolder, "Folder");
        //Layout
        SetContent(_mainBox);
        _viewStack.SetVisibleChildName("NoFolder");
        //Register Events
        _controller.NotificationSent += NotificationSent;
        _controller.FolderChanged += FolderChanged;
        //Open Folder Action
        var actOpenFolder = Gio.SimpleAction.New("openFolder", null);
        actOpenFolder.OnActivate += OpenFolder;
        AddAction(actOpenFolder);
        application.SetAccelsForAction("win.openFolder", new string[] { "<Ctrl>O" });
        //Close Folder Action
        var actCloseFolder = Gio.SimpleAction.New("closeFolder", null);
        actCloseFolder.OnActivate += CloseFolder;
        AddAction(actCloseFolder);
        application.SetAccelsForAction("win.closeFolder", new string[] { "<Ctrl>W" });
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
    }

    /// <summary>
    /// Occurs when a notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotificationSentEventArgs</param>
    private void NotificationSent(object? sender, NotificationSentEventArgs e) => _toastOverlay.AddToast(Adw.Toast.New(e.Message));

    /// <summary>
    /// Occurs when the folder is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void FolderChanged(object? sender, EventArgs e)
    {
        _windowTitle.SetSubtitle(_controller.FolderPath);
        _btnCloseFolder.SetVisible(true);
        _viewStack.SetVisibleChildName(_controller.IsFolderOpened ? "Folder" : "NoFolder");
    }

    /// <summary>
    /// Occurs when the open folder action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void OpenFolder(Gio.SimpleAction sender, EventArgs e)
    {
        var openFolderDialog = Gtk.FileChooserNative.New(_controller.Localizer["OpenFolder"], this, Gtk.FileChooserAction.SelectFolder, _controller.Localizer["Open"], _controller.Localizer["Cancel"]);
        openFolderDialog.SetModal(true);
        openFolderDialog.OnResponse += (sender, e) =>
        {
            if (e.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                _controller.OpenFolder(openFolderDialog.GetFile().GetPath() ?? "");
            }
        };
        openFolderDialog.Show();
    }

    /// <summary>
    /// Occurs when the close folder action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void CloseFolder(Gio.SimpleAction sender, EventArgs e) => _controller.CloseFolder();

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
        var shortcutsDialog = new ShortcutsDialog(_controller.Localizer, this);
    }

    /// <summary>
    /// Occurs when the about action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void About(Gio.SimpleAction sender, EventArgs e)
    {
        var aboutWindow = Gtk.AboutDialog.New();
        aboutWindow.SetModal(true);
        aboutWindow.SetTransientFor(this);
        aboutWindow.SetProgramName(_controller.AppInfo.ShortName);
        aboutWindow.SetLogoIconName(_controller.AppInfo.ID);
        aboutWindow.SetVersion(_controller.AppInfo.Version);
        aboutWindow.SetComments(_controller.AppInfo.Description);
        aboutWindow.SetLicenseType(Gtk.License.Gpl30);
        aboutWindow.SetCopyright("© Nickvision 2021-2022");
        aboutWindow.SetWebsite(Convert.ToString(_controller.AppInfo.GitHubRepo));
        aboutWindow.SetAuthors(_controller.Localizer["Developers"].Split(Environment.NewLine));
        aboutWindow.SetArtists(new string[2] { "Nicholas Logozzo https://github.com/nlogozzo", "Fyodor Sobolev https://github.com/fsobolev" });
        string translatorCredits = _controller.Localizer["TranslatorCredits"];
        if(translatorCredits.Length > 0)
        {
            aboutWindow.SetTranslatorCredits(translatorCredits);
        }
        aboutWindow.Present();
    }
}
