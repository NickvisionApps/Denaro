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
    private static partial void gtk_color_dialog_button_set_rgba(nint button, ref Color rgba);

    [DllImport("libadwaita-1.so.0")]
    static extern ref Color gtk_color_dialog_button_get_rgba(nint button);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_button_set_dialog(nint button, nint dialog);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_color_dialog_new();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_set_with_alpha(nint dialog, [MarshalAs(UnmanagedType.I1)] bool with_alpha);

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
    private readonly nint _transactionColorDialog;
    private readonly nint _transferColorDialog;
    private readonly nint _groupColorDialog;
    private readonly nint _accountCheckingColorDialog;
    private readonly nint _accountSavingsColorDialog;
    private readonly nint _accountBusinessColorDialog;

    [Gtk.Connect] private readonly Adw.ComboRow _themeRow;
    [Gtk.Connect] private readonly Gtk.Widget _transactionColorButton;
    [Gtk.Connect] private readonly Gtk.Widget _transferColorButton;
    [Gtk.Connect] private readonly Gtk.Widget _groupColorButton;
    [Gtk.Connect] private readonly Gtk.Widget _accountCheckingColorButton;
    [Gtk.Connect] private readonly Gtk.Widget _accountSavingsColorButton;
    [Gtk.Connect] private readonly Gtk.Widget _accountBusinessColorButton;
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
        _transactionColorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_transactionColorDialog, false);
        gtk_color_dialog_button_set_dialog(_transactionColorButton.Handle, _transactionColorDialog);
        _transferColorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_transferColorDialog, false);
        gtk_color_dialog_button_set_dialog(_transferColorButton.Handle, _transferColorDialog);
        _groupColorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_groupColorDialog, false);
        gtk_color_dialog_button_set_dialog(_groupColorButton.Handle, _groupColorDialog);
        _accountCheckingColorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_accountCheckingColorDialog, false);
        gtk_color_dialog_button_set_dialog(_accountCheckingColorButton.Handle, _accountCheckingColorDialog);
        _accountSavingsColorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_accountSavingsColorDialog, false);
        gtk_color_dialog_button_set_dialog(_accountSavingsColorButton.Handle, _accountSavingsColorDialog);
        _accountBusinessColorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_accountBusinessColorDialog, false);
        gtk_color_dialog_button_set_dialog(_accountBusinessColorButton.Handle, _accountBusinessColorDialog);
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
        gtk_color_dialog_button_set_rgba(_transactionColorButton.Handle, ref transactionColor);
        var transferColor = new Color();
        gdk_rgba_parse(ref transferColor, _controller.TransferDefaultColor);
        gtk_color_dialog_button_set_rgba(_transferColorButton.Handle, ref transferColor);
        var groupColor = new Color();
        gdk_rgba_parse(ref groupColor, _controller.GroupDefaultColor);
        gtk_color_dialog_button_set_rgba(_groupColorButton.Handle, ref groupColor);
        var accountCheckingColor = new Color();
        gdk_rgba_parse(ref accountCheckingColor, _controller.AccountCheckingColor);
        gtk_color_dialog_button_set_rgba(_accountCheckingColorButton.Handle, ref accountCheckingColor);
        var accountSavingsColor = new Color();
        gdk_rgba_parse(ref accountSavingsColor, _controller.AccountSavingsColor);
        gtk_color_dialog_button_set_rgba(_accountSavingsColorButton.Handle, ref accountSavingsColor);
        var accountBusinessColor = new Color();
        gdk_rgba_parse(ref accountBusinessColor, _controller.AccountBusinessColor);
        gtk_color_dialog_button_set_rgba(_accountBusinessColorButton.Handle, ref accountBusinessColor);
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
        var color = gtk_color_dialog_button_get_rgba(_transactionColorButton.Handle);
        _controller.TransactionDefaultColor = gdk_rgba_to_string(ref color);
        color = gtk_color_dialog_button_get_rgba(_transferColorButton.Handle);
        _controller.TransferDefaultColor = gdk_rgba_to_string(ref color);
        color = gtk_color_dialog_button_get_rgba(_groupColorButton.Handle);
        _controller.GroupDefaultColor = gdk_rgba_to_string(ref color);
        color = gtk_color_dialog_button_get_rgba(_accountCheckingColorButton.Handle);
        _controller.AccountCheckingColor = gdk_rgba_to_string(ref color);
        color = gtk_color_dialog_button_get_rgba(_accountSavingsColorButton.Handle);
        _controller.AccountSavingsColor = gdk_rgba_to_string(ref color);
        color = gtk_color_dialog_button_get_rgba(_accountBusinessColorButton.Handle);
        _controller.AccountBusinessColor = gdk_rgba_to_string(ref color);
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
