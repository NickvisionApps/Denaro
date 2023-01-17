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

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_get_rgba(nint chooser, ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
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
    private readonly Adw.ActionRow _rowAccountCheckingColor;
    private readonly Gtk.ColorButton _btnAccountCheckingColor;
    private readonly Adw.ActionRow _rowAccountSavingsColor;
    private readonly Gtk.ColorButton _btnAccountSavingsColor;
    private readonly Adw.ActionRow _rowAccountBusinessColor;
    private readonly Gtk.ColorButton _btnAccountBusinessColor;

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
        _page.Add(_grpUserInterface);
        //Theme Row
        _rowTheme = Adw.ComboRow.New();
        _rowTheme.SetTitle(_controller.Localizer["Theme"]);
        _rowTheme.SetModel(Gtk.StringList.New(new string[] { _controller.Localizer["ThemeLight"], _controller.Localizer["ThemeDark"], _controller.Localizer["ThemeSystem"] }));
        _rowTheme.OnNotify += (sender, e) =>
        {
            if(e.Pspec.GetName() == "selected-item")
            {
                OnThemeChanged();
            }
        };
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
        //Account Checking Color Row
        _rowAccountCheckingColor = Adw.ActionRow.New();
        _rowAccountCheckingColor.SetTitle(_controller.Localizer["AccountCheckingColor"]);
        _btnAccountCheckingColor = Gtk.ColorButton.New();
        _btnAccountCheckingColor.SetValign(Gtk.Align.Center);
        _btnAccountCheckingColor.OnColorSet += OnAccountCheckingColorSet;
        _rowAccountCheckingColor.AddSuffix(_btnAccountCheckingColor);
        _rowAccountCheckingColor.SetActivatableWidget(_btnAccountCheckingColor);
        _grpUserInterface.Add(_rowAccountCheckingColor);
        //Account Savings Color Row
        _rowAccountSavingsColor = Adw.ActionRow.New();
        _rowAccountSavingsColor.SetTitle(_controller.Localizer["AccountSavingsColor"]);
        _btnAccountSavingsColor = Gtk.ColorButton.New();
        _btnAccountSavingsColor.SetValign(Gtk.Align.Center);
        _btnAccountSavingsColor.OnColorSet += OnAccountSavingsColorSet;
        _rowAccountSavingsColor.AddSuffix(_btnAccountSavingsColor);
        _rowAccountSavingsColor.SetActivatableWidget(_btnAccountSavingsColor);
        _grpUserInterface.Add(_rowAccountSavingsColor);
        //Account Business Color Row
        _rowAccountBusinessColor = Adw.ActionRow.New();
        _rowAccountBusinessColor.SetTitle(_controller.Localizer["AccountBusinessColor"]);
        _btnAccountBusinessColor = Gtk.ColorButton.New();
        _btnAccountBusinessColor.SetValign(Gtk.Align.Center);
        _btnAccountBusinessColor.OnColorSet += OnAccountBusinessColorSet;
        _rowAccountBusinessColor.AddSuffix(_btnAccountBusinessColor);
        _rowAccountBusinessColor.SetActivatableWidget(_btnAccountBusinessColor);
        _grpUserInterface.Add(_rowAccountBusinessColor);
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
        var accountCheckingColor = new Color();
        gdk_rgba_parse(ref accountCheckingColor, _controller.AccountCheckingColor);
        gtk_color_chooser_set_rgba(_btnAccountCheckingColor.Handle, ref accountCheckingColor);
        var accountSavingsColor = new Color();
        gdk_rgba_parse(ref accountSavingsColor, _controller.AccountSavingsColor);
        gtk_color_chooser_set_rgba(_btnAccountSavingsColor.Handle, ref accountSavingsColor);
        var accountBusinessColor = new Color();
        gdk_rgba_parse(ref accountBusinessColor, _controller.AccountBusinessColor);
        gtk_color_chooser_set_rgba(_btnAccountBusinessColor.Handle, ref accountBusinessColor);
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

    /// <summary>
    /// Occurs when the checking account color is set
    /// </summary>
    private void OnAccountCheckingColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_btnAccountCheckingColor.Handle, ref color);
        _controller.AccountCheckingColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when the savings account color is set
    /// </summary>
    private void OnAccountSavingsColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_btnAccountSavingsColor.Handle, ref color);
        _controller.AccountSavingsColor = gdk_rgba_to_string(ref color);
    }

    /// <summary>
    /// Occurs when the business account color is set
    /// </summary>
    private void OnAccountBusinessColorSet(Gtk.ColorButton sender, EventArgs e)
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_btnAccountBusinessColor.Handle, ref color);
        _controller.AccountBusinessColor = gdk_rgba_to_string(ref color);
    }
}
