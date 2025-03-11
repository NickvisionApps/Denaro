using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.IO;
using Adw.Internal;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    private readonly PreferencesViewController _controller;
    private readonly Adw.Application _application;
    private readonly Gtk.ColorDialog _transactionColorDialog;
    private readonly Gtk.ColorDialog _transferColorDialog;
    private readonly Gtk.ColorDialog _groupColorDialog;
    private readonly Gtk.ColorDialog _accountCheckingColorDialog;
    private readonly Gtk.ColorDialog _accountSavingsColorDialog;
    private readonly Gtk.ColorDialog _accountBusinessColorDialog;

    [Gtk.Connect] private readonly Adw.ComboRow _themeRow;
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _transactionColorButton;
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _transferColorButton;
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _groupColorButton;
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _accountCheckingColorButton;
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _accountSavingsColorButton;
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _accountBusinessColorButton;
    [Gtk.Connect] private readonly Adw.SwitchRow _nativeDigitsRow;
    [Gtk.Connect] private readonly Adw.ComboRow _insertSeparatorRow;
    [Gtk.Connect] private readonly Adw.EntryRow _csvBackupRow;
    [Gtk.Connect] private readonly Gtk.Button _selectBackupFolderButton;
    [Gtk.Connect] private readonly Gtk.Button _unsetBackupFolderButton;

    private PreferencesDialog(Gtk.Builder builder, PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : base(builder.GetObject("_root").Handle as PreferencesWindowHandle)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        SetTransientFor(parent);
        //Build UI
        builder.Connect(this);
        _themeRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                OnThemeChanged();
            }
        };
        _transactionColorDialog = Gtk.ColorDialog.New();
        _transactionColorDialog.SetWithAlpha(false);
        _transactionColorButton.SetDialog(_transactionColorDialog);
        _transferColorDialog = Gtk.ColorDialog.New();
        _transferColorDialog.SetWithAlpha(false);
        _transferColorButton.SetDialog(_transferColorDialog);
        _groupColorDialog = Gtk.ColorDialog.New();
        _groupColorDialog.SetWithAlpha(false);
        _groupColorButton.SetDialog(_groupColorDialog);
        _accountCheckingColorDialog = Gtk.ColorDialog.New();
        _accountCheckingColorDialog.SetWithAlpha(false);
        _accountCheckingColorButton.SetDialog(_accountCheckingColorDialog);
        _accountSavingsColorDialog = Gtk.ColorDialog.New();
        _accountSavingsColorDialog.SetWithAlpha(false);
        _accountSavingsColorButton.SetDialog(_accountSavingsColorDialog);
        _accountBusinessColorDialog = Gtk.ColorDialog.New();
        _accountBusinessColorDialog.SetWithAlpha(false);
        _accountBusinessColorButton.SetDialog(_accountBusinessColorDialog);
        _selectBackupFolderButton.OnClicked += SelectBackupFolder;
        _unsetBackupFolderButton.OnClicked += UnsetBackupFolder;
        //Layout
        OnHide += Hide;
        //Load Config
        _themeRow.SetSelected((uint)_controller.Theme);
        GdkHelpers.RGBA.Parse(out var transactionColor, _controller.TransactionDefaultColor);
        _transactionColorButton.SetExtRgba(transactionColor!.Value);
        GdkHelpers.RGBA.Parse(out var transferColor, _controller.TransferDefaultColor);
        _transferColorButton.SetExtRgba(transferColor!.Value);
        GdkHelpers.RGBA.Parse(out var groupColor, _controller.GroupDefaultColor);
        _groupColorButton.SetExtRgba(groupColor!.Value);
        GdkHelpers.RGBA.Parse(out var accountCheckingColor, _controller.AccountCheckingColor);
        _accountCheckingColorButton.SetExtRgba(accountCheckingColor!.Value);
        GdkHelpers.RGBA.Parse(out var accountSavingsColor, _controller.AccountSavingsColor);
        _accountSavingsColorButton.SetExtRgba(accountSavingsColor!.Value);
        GdkHelpers.RGBA.Parse(out var accountBusinessColor, _controller.AccountBusinessColor);
        _accountBusinessColorButton.SetExtRgba(accountBusinessColor!.Value);
        _nativeDigitsRow.SetActive(_controller.UseNativeDigits);
        _insertSeparatorRow.SetSelected((uint)_controller.InsertSeparator);
        if (File.Exists(_controller.CSVBackupFolder))
        {
            _csvBackupRow.SetText(_controller.CSVBackupFolder);
        }
    }

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    /// <param name="application">Adw.Application</param>
    /// <param name="parent">Gtk.Window</param>
    public PreferencesDialog(PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : this(Builder.FromFile("preferences_dialog.ui"), controller, application, parent)
    {
    }

    /// <summary>
    /// Occurs when the dialog is hidden
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private void Hide(Gtk.Widget sender, EventArgs e)
    {
        var color = _transactionColorButton.GetRgba();
        _controller.TransactionDefaultColor = color.ToString();
        color = _transferColorButton.GetRgba();
        _controller.TransferDefaultColor = color.ToString();
        color = _groupColorButton.GetRgba();
        _controller.GroupDefaultColor = color.ToString();
        color = _accountCheckingColorButton.GetRgba();
        _controller.AccountCheckingColor = color.ToString();
        color = _accountSavingsColorButton.GetRgba();
        _controller.AccountSavingsColor = color.ToString();
        color = _accountBusinessColorButton.GetRgba();
        _controller.AccountBusinessColor = color.ToString();
        _controller.UseNativeDigits = _nativeDigitsRow.GetActive();
        _controller.InsertSeparator = (InsertSeparator)_insertSeparatorRow.GetSelected();
        _controller.SaveConfiguration();
        Destroy();
    }

    /// <summary>
    /// Occurs when the theme selection is changed
    /// </summary>
    private void OnThemeChanged()
    {
        _controller.Theme = (Theme)_themeRow.GetSelected();
        _application.StyleManager!.ColorScheme = _controller.Theme switch
        {
            Theme.System => Adw.ColorScheme.PreferLight,
            Theme.Light => Adw.ColorScheme.ForceLight,
            Theme.Dark => Adw.ColorScheme.ForceDark,
            _ => Adw.ColorScheme.PreferLight
        };
    }

    /// <summary>
    /// Occurs when a button to select backup folder is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private async void SelectBackupFolder(Gtk.Button sender, EventArgs e)
    {
        var fileDialog = Gtk.FileDialog.New();
        fileDialog.SetTitle(_("Select Backup Folder"));
        try
        {
            var folder = await fileDialog.SelectFolderAsync(this);
            var path = folder!.GetPath();
            if (path.StartsWith("/run/user"))
            {
                AddToast(Adw.Toast.New(_("Can't access the selected folder, check Flatpak permissions.")));
            }
            else
            {
                _controller.CSVBackupFolder = path;
                _csvBackupRow.SetText(path);
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
        }
    }

    /// <summary>
    /// Occurs when a button to disable CSV backup is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void UnsetBackupFolder(Gtk.Button sender, EventArgs e)
    {
        _controller.CSVBackupFolder = "";
        _csvBackupRow.SetText("");
    }
}
