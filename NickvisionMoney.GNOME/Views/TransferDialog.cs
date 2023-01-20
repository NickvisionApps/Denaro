using NickvisionMoney.Shared.Controllers;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transfer
/// </summary>
public partial class TransferDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

    private readonly TransferDialogController _controller;
    private readonly Gtk.Window _parentWindow;
    private readonly Adw.MessageDialog _dialog;
    private readonly Gtk.Box _boxMain;
    private readonly Gtk.Label _lblDestination;
    private readonly Gtk.Label _lblSelectedAccount;
    private readonly Gtk.Button _btnSelectAccount;
    private readonly Gtk.MenuButton _btnRecentAccounts;
    private readonly Gtk.Popover _popRecentAccounts;
    private readonly Adw.PreferencesGroup _grpRecentAccounts;
    private readonly Gtk.Box _boxSelectedAccount;
    private readonly Adw.Clamp _clampSelectedAccount;
    private readonly Gtk.Box _boxTransferAccount;
    private readonly Gtk.Label _lblCurrency;
    private readonly Adw.EntryRow _rowAmount;
    private readonly Adw.PreferencesGroup _grpAmount;

    /// <summary>
    /// Constructs a TransferDialog
    /// </summary>
    /// <param name="controller">TransferDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public TransferDialog(TransferDialogController controller, Gtk.Window parentWindow)
    {
        _controller = controller;
        _parentWindow = parentWindow;
        //Dialog Settings
        _dialog = Adw.MessageDialog.New(parentWindow, _controller.Localizer["Transfer"], _controller.Localizer["TransferDescription"]);
        _dialog.SetDefaultSize(420, -1);
        _dialog.SetHideOnClose(true);
        _dialog.SetModal(true);
        _dialog.AddResponse("cancel", _controller.Localizer["Cancel"]);
        _dialog.SetCloseResponse("cancel");
        _dialog.AddResponse("ok", _controller.Localizer["OK"]);
        _dialog.SetDefaultResponse("ok");
        _dialog.SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
        //Main Box
        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        //Destination Label
        _lblDestination = Gtk.Label.New(_controller.Localizer["DestinationAccount", "Field"]);
        _lblDestination.AddCssClass("title-4");
        _lblDestination.SetMarginTop(6);
        //Transfer Account Label
        _lblSelectedAccount = Gtk.Label.New(_controller.Localizer["NoAccountSelected"]);
        _lblSelectedAccount.SetValign(Gtk.Align.Center);
        _lblSelectedAccount.SetEllipsize(Pango.EllipsizeMode.Start);
        _lblSelectedAccount.SetMarginStart(20);
        //Select Account Button
        _btnSelectAccount = Gtk.Button.NewFromIconName("document-open-symbolic");
        _btnSelectAccount.AddCssClass("flat");
        _btnSelectAccount.SetValign(Gtk.Align.Center);
        _btnSelectAccount.SetTooltipText(_controller.Localizer["DestinationAccount", "Placeholder"]);
        _btnSelectAccount.OnClicked += OnSelectAccount;
        //Selected Account Box
        _boxSelectedAccount = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
        _boxSelectedAccount.SetHalign(Gtk.Align.Center);
        _boxSelectedAccount.Append(_lblSelectedAccount);
        _boxSelectedAccount.Append(_btnSelectAccount);
        //Selected Account Clamp
        _clampSelectedAccount = Adw.Clamp.New();
        _clampSelectedAccount.SetMaximumSize(280);
        _clampSelectedAccount.SetChild(_boxSelectedAccount);
        //Recent Accounts
        _grpRecentAccounts = Adw.PreferencesGroup.New();
        _grpRecentAccounts.SetTitle(_controller.Localizer["Recents", "GTK"]);
        _grpRecentAccounts.SetSizeRequest(200, 55);
        _popRecentAccounts = Gtk.Popover.New();
        _popRecentAccounts.SetChild(_grpRecentAccounts);
        _btnRecentAccounts = Gtk.MenuButton.New();
        _btnRecentAccounts.AddCssClass("flat");
        _btnRecentAccounts.SetIconName("document-open-recent-symbolic");
        _btnRecentAccounts.SetPopover(_popRecentAccounts);
        _boxSelectedAccount.Append(_btnRecentAccounts);
        //Transfer Account Box
        _boxTransferAccount = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        _boxTransferAccount.AddCssClass("card");
        _boxTransferAccount.Append(_lblDestination);
        _boxTransferAccount.Append(_clampSelectedAccount);
        _boxMain.Append(_boxTransferAccount);
        //Amount
        _lblCurrency = Gtk.Label.New($"{_controller.CultureForNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForNumberString.NumberFormat.NaNSymbol})");
        _lblCurrency.AddCssClass("dim-label");
        _rowAmount = Adw.EntryRow.New();
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        _rowAmount.SetInputPurpose(Gtk.InputPurpose.Number);
        _rowAmount.SetActivatesDefault(true);
        _rowAmount.AddSuffix(_lblCurrency);
        _rowAmount.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        _grpAmount = Adw.PreferencesGroup.New();
        _grpAmount.Add(_rowAmount);
        _boxMain.Append(_grpAmount);
        //Layout
        _dialog.SetExtraChild(_boxMain);
        //Load
        foreach (var recentAccount in _controller.RecentAccounts)
        {
            var row = Adw.ActionRow.New();
            row.SetTitle(recentAccount.Name);
            row.SetSubtitle(recentAccount.Path);
            var button = Gtk.Button.NewFromIconName("wallet2-symbolic");
            button.SetHalign(Gtk.Align.Center);
            button.SetValign(Gtk.Align.Center);
            var bgColorString = _controller.GetColorForAccountType(recentAccount.Type);
            var bgColorStrArray = new Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
            var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
            var btnCssProvider = Gtk.CssProvider.New();
            var btnCss = "#btnWallet { color: " + (luma < 0.5 ? "#fff" : "#000") + "; background-color: " + bgColorString + "; }";
            gtk_css_provider_load_from_data(btnCssProvider.Handle, btnCss, btnCss.Length);
            button.SetName("btnWallet");
            button.GetStyleContext().AddProvider(btnCssProvider, 800);
            button.OnClicked += (Gtk.Button sender, EventArgs e) =>
            {
                //TODO
            };
            row.AddPrefix(button);
            row.SetActivatableWidget(button);
            _grpRecentAccounts.Add(row);
        }
        _rowAmount.SetText(_controller.Transfer.SourceAmount.ToString("N2", _controller.CultureForNumberString));
        Validate();
    }

    public event GObject.SignalHandler<Adw.MessageDialog, Adw.MessageDialog.ResponseSignalArgs> OnResponse
    {
        add
        {
            _dialog.OnResponse += value;
        }
        remove
        {
            _dialog.OnResponse -= value;
        }
    }

    /// <summary>
    /// Shows the dialog
    /// </summary>
    public void Show() => _dialog.Show();

    /// <summary>
    /// Destroys the dialog
    /// </summary>
    public void Destroy() => _dialog.Destroy();

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateTransfer(_lblSelectedAccount.GetText(), _rowAmount.GetText());
        _lblDestination.RemoveCssClass("error");
        _lblSelectedAccount.RemoveCssClass("error");
        _btnSelectAccount.RemoveCssClass("error");
        _lblDestination.SetText(_controller.Localizer["DestinationAccount", "Field"]);
        _rowAmount.RemoveCssClass("error");
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        if (checkStatus == TransferCheckStatus.Valid)
        {
            _dialog.SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidDestPath))
            {
                _lblDestination.AddCssClass("error");
                _lblSelectedAccount.AddCssClass("error");
                _btnSelectAccount.AddCssClass("error");
                _lblDestination.SetText(_controller.Localizer["DestinationAccount", "Invalid"]);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidAmount))
            {
                _rowAmount.AddCssClass("error");
                _rowAmount.SetTitle(_controller.Localizer["Amount", "Invalid"]);
            }
            _dialog.SetResponseEnabled("ok", false);
        }
    }

    /// <summary>
    /// Occurs when Select Destination Account button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnSelectAccount(Gtk.Button sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["SelectAccount"], _parentWindow, Gtk.FileChooserAction.Open, _controller.Localizer["Open"], _controller.Localizer["Cancel"]);
        openFileDialog.SetModal(true);
        var filter = Gtk.FileFilter.New();
        filter.SetName(_controller.Localizer["AccountFileFilter", "GTK"]);
        filter.AddPattern("*.nmoney");
        openFileDialog.AddFilter(filter);
        openFileDialog.OnResponse += (sender, e) =>
        {
            if (e.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var path = openFileDialog.GetFile()!.GetPath();
                _lblSelectedAccount.SetText(path ?? "");
                Validate();
            }
        };
        openFileDialog.Show();
    }
}