using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_get_rgba(nint chooser, ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_set_rgba(nint chooser, ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_new();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_title(nint dialog, string title);

    private delegate void GAsyncReadyCallback(nint source, nint res, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_select_folder(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_select_folder_finish(nint dialog, nint result, nint error);

    private readonly PreferencesViewController _controller;
    private readonly Adw.Application _application;

    [Gtk.Connect] private readonly Adw.ComboRow _themeRow;
    [Gtk.Connect] private readonly Gtk.ColorButton _transactionColor;
    [Gtk.Connect] private readonly Gtk.ColorButton _transferColor;
    [Gtk.Connect] private readonly Gtk.ColorButton _accountCheckingColor;
    [Gtk.Connect] private readonly Gtk.ColorButton _accountSavingsColor;
    [Gtk.Connect] private readonly Gtk.ColorButton _accountBusinessColor;
    [Gtk.Connect] private readonly Adw.ComboRow _insertSeparatorRow;
    [Gtk.Connect] private readonly Adw.ViewStack _backupViewStack;
    [Gtk.Connect] private readonly Gtk.Button _selectBackupFolderButton;
    [Gtk.Connect] private readonly Gtk.Button _backupFolderButton;
    [Gtk.Connect] private readonly Gtk.Label _backupFolderLabel;
    [Gtk.Connect] private readonly Gtk.Button _unsetBackupFolderButton;

    private GAsyncReadyCallback _fileDialogCallback { get; set; }

    private PreferencesDialog(Gtk.Builder builder, PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
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
        _transactionColor.OnColorSet += OnTransactionColorSet;
        _transferColor.OnColorSet += OnTransferColorSet;
        _accountCheckingColor.OnColorSet += OnAccountCheckingColorSet;
        _accountSavingsColor.OnColorSet += OnAccountSavingsColorSet;
        _accountBusinessColor.OnColorSet += OnAccountBusinessColorSet;
        _insertSeparatorRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                _controller.InsertSeparator = (InsertSeparator)_insertSeparatorRow.GetSelected();
            }
        };
        _selectBackupFolderButton.OnClicked += SelectBackupFolder;
        _backupFolderButton.OnClicked += SelectBackupFolder;
        _unsetBackupFolderButton.OnClicked += UnsetBackupFolder;
        //Layout
        OnHide += Hide;
        //Load Config
        _themeRow.SetSelected((uint)_controller.Theme);
        var transactionColor = new Color();
        gdk_rgba_parse(ref transactionColor, _controller.TransactionDefaultColor);
        gtk_color_chooser_set_rgba(_transactionColor.Handle, ref transactionColor);
        var transferColor = new Color();
        gdk_rgba_parse(ref transferColor, _controller.TransferDefaultColor);
        gtk_color_chooser_set_rgba(_transferColor.Handle, ref transferColor);
        var accountCheckingColor = new Color();
        gdk_rgba_parse(ref accountCheckingColor, _controller.AccountCheckingColor);
        gtk_color_chooser_set_rgba(_accountCheckingColor.Handle, ref accountCheckingColor);
        var accountSavingsColor = new Color();
        gdk_rgba_parse(ref accountSavingsColor, _controller.AccountSavingsColor);
        gtk_color_chooser_set_rgba(_accountSavingsColor.Handle, ref accountSavingsColor);
        var accountBusinessColor = new Color();
        gdk_rgba_parse(ref accountBusinessColor, _controller.AccountBusinessColor);
        gtk_color_chooser_set_rgba(_accountBusinessColor.Handle, ref accountBusinessColor);
        _insertSeparatorRow.SetSelected((uint)_controller.InsertSeparator);
        if (!string.IsNullOrEmpty(_controller.CSVBackupFolder))
        {
            _backupViewStack.SetVisibleChildName("folder-selected");
            _backupFolderLabel.SetText(_controller.CSVBackupFolder);
        }
    }

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    /// <param name="application">Adw.Application</param>
    /// <param name="parent">Gtk.Window</param>
    public PreferencesDialog(PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : this(Builder.FromFile("preferences_dialog.ui", controller.Localizer), controller, application, parent)
    {
    }

    /// <summary>
    /// Occurs when the dialog is hidden
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private void Hide(Gtk.Widget sender, EventArgs e)
    {
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
    /// Occurs when the transaction color is set
    /// </summary>
    private void OnTransactionColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_transactionColor.Handle, ref color);
        _controller.TransactionDefaultColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when the transfer color is set
    /// </summary>
    private void OnTransferColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_transferColor.Handle, ref color);
        _controller.TransferDefaultColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when the checking account color is set
    /// </summary>
    private void OnAccountCheckingColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_accountCheckingColor.Handle, ref color);
        _controller.AccountCheckingColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when the savings account color is set
    /// </summary>
    private void OnAccountSavingsColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_accountSavingsColor.Handle, ref color);
        _controller.AccountSavingsColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when the business account color is set
    /// </summary>
    private void OnAccountBusinessColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_accountBusinessColor.Handle, ref color);
        _controller.AccountBusinessColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when a button to select backup folder is clicked
    /// </summary>
    private void SelectBackupFolder(Gtk.Button sender, EventArgs e)
    {
        var fileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(fileDialog, _controller.Localizer["SelectBackupFolder"]);
        _fileDialogCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_select_folder_finish(fileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
                if (path.StartsWith("/run/user"))
                {
                    AddToast(Adw.Toast.New(_controller.Localizer["FolderFlatpakUnavailable", "GTK"]));
                }
                else
                {
                    _controller.CSVBackupFolder = path;
                    _backupViewStack.SetVisibleChildName("folder-selected");
                    _backupFolderLabel.SetText(path);
                }
            }
        };
        gtk_file_dialog_select_folder(fileDialog, Handle, IntPtr.Zero, _fileDialogCallback, IntPtr.Zero);
    }

    /// <summary>
    /// Occurs when a button to disable CSV backup is clicked
    /// </summary>
    private void UnsetBackupFolder(Gtk.Button sender, EventArgs e)
    {
        _controller.CSVBackupFolder = "";
        _backupViewStack.SetVisibleChildName("no-folder");
    }
}
