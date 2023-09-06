using Nickvision.GirExt;
using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Threading.Tasks;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transfer
/// </summary>
public partial class TransferDialog : Adw.Window
{
    private readonly TransferDialogController _controller;
    private readonly Gtk.Window _parentWindow;

    [Gtk.Connect] private readonly Gtk.Button _selectAccountButton;
    [Gtk.Connect] private readonly Gtk.MenuButton _recentAccountsButton;
    [Gtk.Connect] private readonly Gtk.Popover _recentAccountsPopover;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _recentAccountsGroup;
    [Gtk.Connect] private readonly Adw.ActionRow _destinationAccountRow;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _destinationPasswordRow;
    [Gtk.Connect] private readonly Gtk.Label _currencyLabel;
    [Gtk.Connect] private readonly Adw.EntryRow _amountRow;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _conversionRateGroup;
    [Gtk.Connect] private readonly Adw.ExpanderRow _rowUseCustomRates;
    [Gtk.Connect] private readonly Adw.EntryRow _sourceCurrencyRow;
    [Gtk.Connect] private readonly Adw.EntryRow _destinationCurrencyRow;
    [Gtk.Connect] private readonly Gtk.Label _conversionResultLabel;
    [Gtk.Connect] private readonly Gtk.Button _transferButton;

    private readonly Gtk.EventControllerKey _amountKeyController;
    private readonly Gtk.EventControllerKey _sourceCurrencyKeyController;
    private readonly Gtk.EventControllerKey _destCurrencyKeyController;

    public event EventHandler? OnApply;

    private TransferDialog(Gtk.Builder builder, TransferDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _parentWindow = parent;
        //Build UI
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        //Destination Password Row
        _destinationPasswordRow.OnApply += OnApplyDestinationPassword;
        //Select Account Button
        _selectAccountButton.OnClicked += OnSelectAccount;
        //Transfer Button
        _transferButton.OnClicked += (sender, e) => OnApply?.Invoke(this, EventArgs.Empty);
        //Amount
        _currencyLabel.SetLabel($"{_controller.CultureForSourceNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForSourceNumberString.NumberFormat.NaNSymbol})");
        _amountRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        _amountKeyController = Gtk.EventControllerKey.New();
        _amountKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _amountKeyController.OnKeyPressed += OnKeyPressedSource;
        _amountRow.AddController(_amountKeyController);
        //Conversion Rate
        _conversionRateGroup.OnNotify += async (sender, e) =>
        {
            if (e.Pspec.GetName() == "visible")
            {
                await SetupConversionRateGroupAsync();
            }
        };
        _sourceCurrencyRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        _sourceCurrencyKeyController = Gtk.EventControllerKey.New();
        _sourceCurrencyKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _sourceCurrencyKeyController.OnKeyPressed += OnKeyPressedSource;
        _sourceCurrencyRow.AddController(_sourceCurrencyKeyController);
        _destinationCurrencyRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        _destCurrencyKeyController = Gtk.EventControllerKey.New();
        _destCurrencyKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _destCurrencyKeyController.OnKeyPressed += OnKeyPressedDest;
        _destinationCurrencyRow.AddController(_destCurrencyKeyController);
        //Load
        if (_controller.RecentAccounts.Count > 0)
        {
            foreach (var recentAccount in _controller.RecentAccounts)
            {
                var row = new RecentAccountRow(recentAccount, _controller.GetColorForAccountType(recentAccount.Type), false, false);
                row.Selected += (sender, e) =>
                {
                    _recentAccountsPopover.Popdown();
                    _destinationAccountRow.SetSubtitle(e.Path);
                    _destinationPasswordRow.SetVisible(false);
                    _destinationPasswordRow.SetSensitive(true);
                    _destinationPasswordRow.SetText("");
                    _amountRow.SetText("");
                    _conversionRateGroup.SetVisible(false);
                    _sourceCurrencyRow.SetText("");
                    _destinationCurrencyRow.SetText("");
                    Validate();
                };
                _recentAccountsGroup.Add(row);
            }
        }
        else
        {
            _recentAccountsButton.SetSensitive(false);
        }
        _amountRow.SetText(_controller.Transfer.SourceAmount.ToAmountString(_controller.CultureForSourceNumberString, _controller.UseNativeDigits, false));
        Validate();
    }

    /// <summary>
    /// Constructs a TransferDialog
    /// </summary>
    /// <param name="controller">TransferDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public TransferDialog(TransferDialogController controller, Gtk.Window parent) : this(Builder.FromFile("transfer_dialog.ui"), controller, parent)
    {
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateTransfer(_destinationAccountRow.GetSubtitle() ?? "", _destinationPasswordRow.GetText(), _amountRow.GetText(), _sourceCurrencyRow.GetText(), _destinationCurrencyRow.GetText());
        _destinationAccountRow.RemoveCssClass("error");
        _destinationAccountRow.SetTitle(_("Destination Account"));
        _destinationPasswordRow.RemoveCssClass("error");
        _destinationPasswordRow.SetTitle(_("Destination Account Password"));
        _amountRow.RemoveCssClass("error");
        _amountRow.SetTitle(_("Amount"));
        _amountRow.SetVisible(true);
        _sourceCurrencyRow.RemoveCssClass("error");
        _sourceCurrencyRow.SetTitle(_controller.SourceCurrencyCode);
        _destinationCurrencyRow.RemoveCssClass("error");
        _destinationCurrencyRow.SetTitle(_controller.DestinationCurrencyCode ?? "");
        if (checkStatus == TransferCheckStatus.Valid)
        {
            _conversionResultLabel.SetText(_controller.Transfer.DestinationAmount.ToAmountString(_controller.CultureForDestNumberString, _controller.UseNativeDigits));
            _transferButton.SetSensitive(true);
        }
        else
        {
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidDestPath))
            {
                _destinationAccountRow.AddCssClass("error");
                _destinationAccountRow.SetTitle(_("Destination Account (Invalid)"));
                _amountRow.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountRequiresPassword))
            {
                _destinationPasswordRow.SetVisible(true);
                _destinationPasswordRow.AddCssClass("error");
                _destinationPasswordRow.SetTitle(_("Destination Account Password (Required)"));
                _amountRow.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountPasswordInvalid))
            {
                _destinationPasswordRow.AddCssClass("error");
                _destinationPasswordRow.SetTitle(_("Destination Account Password (Invalid)"));
                _amountRow.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidAmount))
            {
                _amountRow.AddCssClass("error");
                _amountRow.SetTitle(_("Amount (Invalid)"));
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidConversionRate))
            {
                _conversionRateGroup.SetVisible(true);
                _sourceCurrencyRow.AddCssClass("error");
                _sourceCurrencyRow.SetTitle(_controller.SourceCurrencyCode);
                _destinationCurrencyRow.AddCssClass("error");
                _destinationCurrencyRow.SetTitle(_controller.DestinationCurrencyCode!);
                _conversionResultLabel.SetText(_("N/A"));
            }
            _transferButton.SetSensitive(false);
        }
        if (!checkStatus.HasFlag(TransferCheckStatus.DestAccountRequiresPassword) && !checkStatus.HasFlag(TransferCheckStatus.DestAccountPasswordInvalid))
        {
            _destinationPasswordRow.SetSensitive(false);
        }
    }

    /// <summary>
    /// Occurs when Select Destination Account button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private async void OnSelectAccount(Gtk.Button sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileDialog.New();
        openFileDialog.SetTitle(_("Select Account"));
        var filter = Gtk.FileFilter.New();
        filter.SetName($"{_("Nickvision Denaro Account")} (*.nmoney)");
        filter.AddPattern("*.nmoney");
        filter.AddPattern("*.NMONEY");
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filter);
        openFileDialog.SetFilters(filters);
        try
        {
            var file = await openFileDialog.OpenAsync(this);
            _destinationAccountRow.SetSubtitle(file!.GetPath() ?? "");
            _destinationPasswordRow.SetVisible(false);
            _destinationPasswordRow.SetSensitive(true);
            _destinationPasswordRow.SetText("");
            _amountRow.SetText("");
            _conversionRateGroup.SetVisible(false);
            _sourceCurrencyRow.SetText("");
            _destinationCurrencyRow.SetText("");
            Validate();
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the apply destination password button is clicked
    /// </summary>
    /// <param name="sender">Adw.EntryRow</param>
    /// <param name="e">EventArgs</param>
    private void OnApplyDestinationPassword(Adw.EntryRow sender, EventArgs e) => Validate();

    /// <summary>
    /// Callback for key-pressed signal for source entries
    /// </summary>
    /// <param name="sender">Gtk.EventControllerKey</param>
    /// <param name="e">Gtk.EventControllerKey.KeyPressedSignalArgs</param>
    private bool OnKeyPressedSource(Gtk.EventControllerKey sender, Gtk.EventControllerKey.KeyPressedSignalArgs e)
    {
        if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (e.Keyval == 65454 || e.Keyval == 65452 || e.Keyval == 2749 || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (e.Keyval == 44 || e.Keyval == 46)))
            {
                var row = (Adw.EntryRow)(sender.GetWidget());
                if (!row.GetText().Contains(_controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator))
                {
                    var position = row.GetPosition();
                    row.SetText(row.GetText().Insert(position, _controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator));
                    row.SetPosition(position + Math.Min(_controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator.Length, 2));
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Callback for key-pressed signal for dest entries
    /// </summary>
    /// <param name="sender">Gtk.EventControllerKey</param>
    /// <param name="e">Gtk.EventControllerKey.KeyPressedSignalArgs</param>
    private bool OnKeyPressedDest(Gtk.EventControllerKey sender, Gtk.EventControllerKey.KeyPressedSignalArgs e)
    {
        if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (e.Keyval == 65454 || e.Keyval == 65452 || e.Keyval == 2749 || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (e.Keyval == 44 || e.Keyval == 46)))
            {
                var row = (Adw.EntryRow)(sender.GetWidget());
                if (!row.GetText().Contains(_controller.CultureForDestNumberString.NumberFormat.CurrencyDecimalSeparator))
                {
                    var position = row.GetPosition();
                    row.SetText(row.GetText().Insert(position, _controller.CultureForDestNumberString.NumberFormat.CurrencyDecimalSeparator));
                    row.SetPosition(position + Math.Min(_controller.CultureForDestNumberString.NumberFormat.CurrencyDecimalSeparator.Length, 2));
                }
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Sets up the ConversionRateGroup
    /// </summary>
    private async Task SetupConversionRateGroupAsync()
    {
        if (_conversionRateGroup.Visible)
        {
            var res = await _controller.GetConversionRateOnlineAsync();
            if (string.IsNullOrEmpty(res.Source) || string.IsNullOrEmpty(res.Destination))
            {
                _sourceCurrencyRow.SetText("");
                _destinationCurrencyRow.SetText("");
                _rowUseCustomRates.SetShowEnableSwitch(false);
                _rowUseCustomRates.SetEnableExpansion(true);
            }
            else
            {
                _sourceCurrencyRow.SetText(res.Source);
                _destinationCurrencyRow.SetText(res.Destination);
                _rowUseCustomRates.SetShowEnableSwitch(true);
                _rowUseCustomRates.SetEnableExpansion(false);
            }
        }
    }
}