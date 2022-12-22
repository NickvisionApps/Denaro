using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.Window
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SignalCallback(nint gObject, nint gParamSpec, nint data);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)]SignalCallback c_handler, nint data, nint destroy_data, int connect_flags);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_get_rgba(nint chooser, ref Color rgba);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_set_rgba(nint chooser, ref Color rgba);

    private readonly PreferencesViewController _controller;
    private readonly Adw.Application _application;
    private readonly Gtk.Box _mainBox;
    private readonly Adw.HeaderBar _headerBar;
    private readonly Adw.PreferencesPage _page;
    private readonly Adw.PreferencesGroup _grpUserInterface;
    private readonly Adw.ComboRow _rowTheme;
    private readonly Adw.ActionRow _rowTransactionColor;
    private readonly Gtk.ColorButton _btnTransactionColor;
    private readonly Adw.ActionRow _rowTransferColor;
    private readonly Gtk.ColorButton _btnTransferColor;

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    /// <param name="application">Adw.Application</param>
    /// <param name="parent">Gtk.Window</param>
    public PreferencesDialog(PreferencesViewController controller, Adw.Application application, Gtk.Window parent)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        New();
        SetTransientFor(parent);
        SetDefaultSize(600, 400);
        SetModal(true);
        SetDestroyWithParent(false);
        SetHideOnClose(true);
        //Main Box
        _mainBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        //Header Bar
        _headerBar = Adw.HeaderBar.New();
        _headerBar.SetTitleWidget(Adw.WindowTitle.New(_controller.Localizer["Preferences"], ""));
        _mainBox.Append(_headerBar);
        //Preferences Page
        _page = Adw.PreferencesPage.New();
        _mainBox.Append(_page);
        //User Interface Group
        _grpUserInterface = Adw.PreferencesGroup.New();
        _grpUserInterface.SetTitle(_controller.Localizer["UserInterface"]);
        _grpUserInterface.SetDescription(_controller.Localizer["UserInterfaceDescription"]);
        //Theme Row
        _rowTheme = Adw.ComboRow.New();
        _rowTheme.SetTitle(_controller.Localizer["Theme"]);
        _rowTheme.SetModel(Gtk.StringList.New(new string[] { _controller.Localizer["ThemeLight"], _controller.Localizer["ThemeDark"], _controller.Localizer["ThemeSystem"] }));
        g_signal_connect_data(_rowTheme.Handle, "notify::selected-item", (nint sender, nint gParamSpec, nint data) => OnThemeChanged(), IntPtr.Zero, IntPtr.Zero, 0);
        _grpUserInterface.Add(_rowTheme);
        //Transaction Color Row
        _rowTransactionColor = Adw.ActionRow.New();
        _rowTransactionColor.SetTitle(_controller.Localizer["TransactionColor"]);
        _rowTransactionColor.SetSubtitle(_controller.Localizer["TransactionColorDescription"]);
        _btnTransactionColor = Gtk.ColorButton.New();
        _btnTransactionColor.SetValign(Gtk.Align.Center);
        _btnTransactionColor.OnColorSet += OnTransactionColorSet;
        _rowTransactionColor.AddSuffix(_btnTransactionColor);
        _rowTransactionColor.SetActivatableWidget(_btnTransactionColor);
        _grpUserInterface.Add(_rowTransactionColor);
        //Transfer Color Row
        _rowTransferColor = Adw.ActionRow.New();
        _rowTransferColor.SetTitle(_controller.Localizer["TransferColor"]);
        _rowTransferColor.SetSubtitle(_controller.Localizer["TransferColorDescription"]);
        _btnTransferColor = Gtk.ColorButton.New();
        _btnTransferColor.SetValign(Gtk.Align.Center);
        _btnTransferColor.OnColorSet += OnTransferColorSet;
        _rowTransferColor.AddSuffix(_btnTransferColor);
        _rowTransferColor.SetActivatableWidget(_btnTransferColor);
        _grpUserInterface.Add(_rowTransferColor);
        _page.Add(_grpUserInterface);
        //Layout
        SetContent(_mainBox);
        OnHide += Hide;
        //Load Config
        _rowTheme.SetSelected((uint)_controller.Theme);
        var transactionColor = new Color();
        gdk_rgba_parse(ref transactionColor, _controller.TransactionDefaultColor);
        gtk_color_chooser_set_rgba(_btnTransactionColor.Handle, ref transactionColor);
        var transferColor = new Color();
        gdk_rgba_parse(ref transferColor, _controller.TransferDefaultColor);
        gtk_color_chooser_set_rgba(_btnTransferColor.Handle, ref transferColor);
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
        _controller.Theme = (Theme)_rowTheme.GetSelected();
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
        gtk_color_chooser_get_rgba(_btnTransactionColor.Handle, ref color);
        _controller.TransactionDefaultColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when the transfer color is set
    /// </summary>
    private void OnTransferColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_btnTransferColor.Handle, ref color);
        _controller.TransferDefaultColor = gdk_rgba_to_string(ref color);
    }
}
