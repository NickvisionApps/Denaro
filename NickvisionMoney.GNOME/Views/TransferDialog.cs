using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;
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

    private new delegate bool PressedSignal(nint gObject, uint keyval, uint keycode, nint state, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)] PressedSignal c_handler, nint data, nint destroy_data, int connect_flags);

    private readonly TransferDialogController _controller;
    private readonly Gtk.Window _parentWindow;
    private readonly Adw.MessageDialog _dialog;
    private readonly Adw.PreferencesGroup _grpMain;
    private readonly Gtk.Box _boxButtonsAccount;
    private readonly Gtk.Button _btnSelectAccount;
    private readonly Gtk.MenuButton _btnRecentAccounts;
    private readonly Gtk.Popover _popRecentAccounts;
    private readonly Adw.PreferencesGroup _grpRecentAccounts;
    private readonly Adw.ActionRow _rowDestinationAccount;
    private readonly Adw.PasswordEntryRow _rowDestinationPassword;
    private readonly Gtk.Label _lblCurrency;
    private readonly Adw.EntryRow _rowAmount;
    private readonly Adw.PreferencesGroup _grpConversionRate;
    private readonly Adw.EntryRow _rowSourceCurrency;
    private readonly Adw.EntryRow _rowDestCurrency;
    private readonly Adw.ActionRow _rowConversionResult;
    private readonly Gtk.Label _lblConversionResult;
    private readonly Gtk.EventControllerKey _amountKeyController;

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
        //Main Preferences Group
        _grpMain = Adw.PreferencesGroup.New();
        _dialog.SetExtraChild(_grpMain);
        //Destination Account Row
        _rowDestinationAccount = Adw.ActionRow.New();
        _rowDestinationAccount.SetTitle(_controller.Localizer["DestinationAccount", "Field"]);
        _rowDestinationAccount.SetSubtitle(_controller.Localizer["NoAccountSelected"]);
        _grpMain.Add(_rowDestinationAccount);
        //Destination Password Row
        _rowDestinationPassword = Adw.PasswordEntryRow.New();
        _rowDestinationPassword.SetTitle(_controller.Localizer["DestinationPassword", "Field"]);
        _rowDestinationPassword.SetShowApplyButton(true);
        _rowDestinationPassword.SetVisible(false);
        _rowDestinationPassword.OnApply += OnApplyDestinationPassword;
        _grpMain.Add(_rowDestinationPassword);
        //Select Account Button
        _btnSelectAccount = Gtk.Button.NewFromIconName("document-open-symbolic");
        _btnSelectAccount.SetTooltipText(_controller.Localizer["DestinationAccount", "Placeholder"]);
        _btnSelectAccount.OnClicked += OnSelectAccount;
        //Recent Accounts
        _grpRecentAccounts = Adw.PreferencesGroup.New();
        _grpRecentAccounts.SetTitle(_controller.Localizer["Recents", "GTK"]);
        _grpRecentAccounts.SetSizeRequest(200, 55);
        _popRecentAccounts = Gtk.Popover.New();
        _popRecentAccounts.SetChild(_grpRecentAccounts);
        _btnRecentAccounts = Gtk.MenuButton.New();
        _btnRecentAccounts.SetIconName("document-open-recent-symbolic");
        _btnRecentAccounts.SetTooltipText(_controller.Localizer["RecentAccounts"]);
        _btnRecentAccounts.SetPopover(_popRecentAccounts);
        //Buttons Account Box
        _boxButtonsAccount = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        _boxButtonsAccount.SetValign(Gtk.Align.Center);
        _boxButtonsAccount.AddCssClass("linked");
        _boxButtonsAccount.Append(_btnSelectAccount);
        _boxButtonsAccount.Append(_btnRecentAccounts);
        _rowDestinationAccount.AddSuffix(_boxButtonsAccount);
        _rowDestinationAccount.SetActivatableWidget(_btnSelectAccount);
        //Amount
        _lblCurrency = Gtk.Label.New($"{_controller.CultureForSourceNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForSourceNumberString.NumberFormat.NaNSymbol})");
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
        _amountKeyController = Gtk.EventControllerKey.New();
        _amountKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _rowAmount.AddController(_amountKeyController);
        g_signal_connect_data(_amountKeyController.Handle, "key-pressed", OnKeyPressed, IntPtr.Zero, IntPtr.Zero, 0);
        _grpMain.Add(_rowAmount);
        //Conversion Rate
        _grpConversionRate = Adw.PreferencesGroup.New();
        _grpConversionRate.SetMarginTop(6);
        _grpConversionRate.SetTitle(_controller.Localizer["ConversionNeeded"]);
        _grpConversionRate.SetVisible(false);
        _rowSourceCurrency = Adw.EntryRow.New();
        _rowSourceCurrency.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        _rowDestCurrency = Adw.EntryRow.New();
        _rowDestCurrency.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        _rowConversionResult = Adw.ActionRow.New();
        _lblConversionResult = Gtk.Label.New(null);
        _rowConversionResult.SetTitle(_controller.Localizer["Result"]);
        _rowConversionResult.AddSuffix(_lblConversionResult);
        _grpConversionRate.Add(_rowSourceCurrency);
        _grpConversionRate.Add(_rowDestCurrency);
        _grpConversionRate.Add(_rowConversionResult);
        _grpMain.Add(_grpConversionRate);
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
                _popRecentAccounts.Popdown();
                _rowDestinationAccount.SetSubtitle(row.GetSubtitle() ?? "");
                _rowDestinationPassword.SetVisible(false);
                _rowDestinationPassword.SetText("");
                _rowAmount.SetText("");
                _grpConversionRate.SetVisible(false);
                _rowSourceCurrency.SetText("");
                _rowDestCurrency.SetText("");
                Validate();
            };
            row.AddPrefix(button);
            row.SetActivatableWidget(button);
            _grpRecentAccounts.Add(row);
        }
        _rowAmount.SetText(_controller.Transfer.SourceAmount.ToString("N2", _controller.CultureForSourceNumberString));
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
        var checkStatus = _controller.UpdateTransfer(_rowDestinationAccount.GetSubtitle() ?? "", _rowDestinationPassword.GetText(), _rowAmount.GetText(), _rowSourceCurrency.GetText(), _rowDestCurrency.GetText());
        _rowDestinationAccount.RemoveCssClass("error");
        _rowDestinationAccount.SetTitle(_controller.Localizer["DestinationAccount", "Field"]);
        _rowDestinationPassword.RemoveCssClass("error");
        _rowDestinationPassword.SetTitle(_controller.Localizer["DestinationPassword", "Field"]);
        _rowAmount.RemoveCssClass("error");
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        _rowAmount.SetVisible(true);
        _rowSourceCurrency.RemoveCssClass("error");
        _rowSourceCurrency.SetTitle(_controller.SourceCurrencyCode);
        _rowDestCurrency.RemoveCssClass("error");
        _rowDestCurrency.SetTitle(_controller.DestinationCurrencyCode ?? "");
        if (checkStatus == TransferCheckStatus.Valid)
        {
            _lblConversionResult.SetText(_controller.Transfer.DestinationAmount.ToString("C", _controller.CultureForDestNumberString));
            _dialog.SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidDestPath))
            {
                _rowDestinationAccount.AddCssClass("error");
                _rowDestinationAccount.SetTitle(_controller.Localizer["DestinationAccount", "Invalid"]);
                _rowAmount.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountRequiresPassword))
            {
                _rowDestinationPassword.SetVisible(true);
                _rowDestinationPassword.AddCssClass("error");
                _rowDestinationPassword.SetTitle(_controller.Localizer["DestinationPassword", "Required"]);
                _rowAmount.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountPasswordInvalid))
            {
                _rowDestinationPassword.AddCssClass("error");
                _rowDestinationPassword.SetTitle(_controller.Localizer["DestinationPassword", "Invalid"]);
                _rowAmount.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidAmount))
            {
                _rowAmount.AddCssClass("error");
                _rowAmount.SetTitle(_controller.Localizer["Amount", "Invalid"]);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidConversionRate))
            {
                _grpConversionRate.SetVisible(true);
                _rowSourceCurrency.AddCssClass("error");
                _rowSourceCurrency.SetTitle(_controller.SourceCurrencyCode);
                _rowDestCurrency.AddCssClass("error");
                _rowDestCurrency.SetTitle(_controller.DestinationCurrencyCode!);
                _lblConversionResult.SetText(_controller.Localizer["NotAvailable"]);
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
                var path = openFileDialog.GetFile()!.GetPath() ?? "";
                _rowDestinationAccount.SetSubtitle(path);
                _rowDestinationPassword.SetVisible(false);
                _rowDestinationPassword.SetText("");
                _rowAmount.SetText("");
                _grpConversionRate.SetVisible(false);
                _rowSourceCurrency.SetText("");
                _rowDestCurrency.SetText("");
                Validate();
            }
        };
        openFileDialog.Show();
    }

    /// <summary>
    /// Occurs when the apply destination password button is clicked
    /// </summary>
    /// <param name="sender">Adw.EntryRow</param>
    /// <param name="e">EventArgs</param>
    private void OnApplyDestinationPassword(Adw.EntryRow sender, EventArgs e) => Validate();

    /// <summary>
    /// Callback for key-pressed signal
    /// </summary>
    private bool OnKeyPressed(nint sender, uint keyval, uint keycode, nint state, nint data)
    {
         if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (keyval == 65454 || keyval == 65452 || keyval == 2749 || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (keyval == 44 || keyval == 46)))
            {
                _rowAmount.SetText(_rowAmount.GetText() + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                _rowAmount.SetPosition(_rowAmount.GetText().Length);
                return true;
            }
        }
        return false;
    }
}