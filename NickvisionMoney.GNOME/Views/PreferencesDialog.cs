using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public class PreferencesDialog : Adw.Window
{
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
        _grpUserInterface.SetTitle(_controller.Localizer["SettingsUserInterface"]);
        _grpUserInterface.SetDescription(_controller.Localizer["SettingsUserInterfaceDescription"]);
        //Theme Row
        _rowTheme = Adw.ComboRow.New();
        _rowTheme.SetTitle(_controller.Localizer["SettingsTheme"]);
        _rowTheme.SetSubtitle(_controller.Localizer["SettingThemeDescriptionGtk"]);
        _rowTheme.SetModel(Gtk.StringList.New(new string[] { _controller.Localizer["SettingsThemeLight"], _controller.Localizer["SettingsThemeDark"], _controller.Localizer["SettingsThemeSystem"] }));
        _grpUserInterface.Add(_rowTheme);
        //Transaction Color Row
        _rowTransactionColor = Adw.ActionRow.New();
        _rowTransactionColor.SetTitle(_controller.Localizer["SettingsTransactionColor"]);
        _rowTransactionColor.SetSubtitle(_controller.Localizer["SettingsChangeForNewTransactions"]);
        _btnTransactionColor = Gtk.ColorButton.New();
        _btnTransactionColor.SetSensitive(false);
        _btnTransactionColor.SetValign(Gtk.Align.Center);
        _rowTransactionColor.AddSuffix(_btnTransactionColor);
        _rowTransactionColor.SetActivatableWidget(_btnTransactionColor);
        _grpUserInterface.Add(_rowTransactionColor);
        //Transfer Color Row
        _rowTransferColor = Adw.ActionRow.New();
        _rowTransferColor.SetTitle(_controller.Localizer["SettingsTransferColor"]);
        _rowTransferColor.SetSubtitle(_controller.Localizer["SettingsChangeForNewTransactions"]);
        _btnTransferColor = Gtk.ColorButton.New();
        _btnTransferColor.SetSensitive(false);
        _btnTransferColor.SetValign(Gtk.Align.Center);
        _rowTransferColor.AddSuffix(_btnTransferColor);
        _rowTransferColor.SetActivatableWidget(_btnTransferColor);
        _grpUserInterface.Add(_rowTransferColor);
        _page.Add(_grpUserInterface);
        //Layout
        SetContent(_mainBox);
        OnHide += Hide;
        //Load Config
        _rowTheme.SetSelected((uint)_controller.Theme);
    }

    /// <summary>
    /// Occurs when the dialog is hidden
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private void Hide(Gtk.Widget sender, EventArgs e)
    {
        _controller.Theme = (Theme)_rowTheme.GetSelected();
        _application.StyleManager!.ColorScheme = _controller.Theme switch
        {
            Theme.System => Adw.ColorScheme.PreferLight,
            Theme.Light => Adw.ColorScheme.ForceLight,
            Theme.Dark => Adw.ColorScheme.ForceDark,
            _ => Adw.ColorScheme.PreferLight
        };
        _controller.SaveConfiguration();
        Destroy();
    }
}
