using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transfer
/// </summary>
public partial class TransferDialog : Adw.MessageDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_new();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_title(nint dialog, string title);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_filters(nint dialog, nint filters);

    private delegate void GAsyncReadyCallback(nint source, nint res, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_open(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_open_finish(nint dialog, nint result, nint error);

    private GAsyncReadyCallback _openCallback { get; set; }

    private readonly TransferDialogController _controller;
    private readonly Gtk.Window _parentWindow;

    [Gtk.Connect] private readonly Gtk.Button _selectAccountButton;
    [Gtk.Connect] private readonly Gtk.Popover _recentAccountsPopover;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _recentAccountsGroup;
    [Gtk.Connect] private readonly Adw.ActionRow _destinationAccountRow;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _destinationPasswordRow;
    [Gtk.Connect] private readonly Gtk.Label _currencyLabel;
    [Gtk.Connect] private readonly Adw.EntryRow _amountRow;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _conversionRateGroup;
    [Gtk.Connect] private readonly Adw.EntryRow _sourceCurrencyRow;
    [Gtk.Connect] private readonly Adw.EntryRow _destinationCurrencyRow;
    [Gtk.Connect] private readonly Gtk.Label _conversionResultLabel;

    private readonly Gtk.EventControllerKey _amountKeyController;
    private readonly Gtk.EventControllerKey _sourceCurrencyKeyController;
    private readonly Gtk.EventControllerKey _destCurrencyKeyController;

    private TransferDialog(Gtk.Builder builder, TransferDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _parentWindow = parent;
        //Build UI
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        AddResponse("cancel", _controller.Localizer["Cancel"]);
        SetCloseResponse("cancel");
        AddResponse("ok", _controller.Localizer["Transfer"]);
        SetDefaultResponse("ok");
        SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
        //Destination Password Row
        _destinationPasswordRow.OnApply += OnApplyDestinationPassword;
        //Select Account Button
        _selectAccountButton.OnClicked += OnSelectAccount;
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
                _recentAccountsPopover.Popdown();
                _destinationAccountRow.SetSubtitle(row.GetSubtitle() ?? "");
                _destinationPasswordRow.SetVisible(false);
                _destinationPasswordRow.SetSensitive(true);
                _destinationPasswordRow.SetText("");
                _amountRow.SetText("");
                _conversionRateGroup.SetVisible(false);
                _sourceCurrencyRow.SetText("");
                _destinationCurrencyRow.SetText("");
                Validate();
            };
            row.AddPrefix(button);
            row.SetActivatableWidget(button);
            _recentAccountsGroup.Add(row);
        }
        _amountRow.SetText(StringHelpers.FormatAmount(_controller.Transfer.SourceAmount, _controller.CultureForSourceNumberString, false));
        Validate();
    }

    /// <summary>
    /// Constructs a TransferDialog
    /// </summary>
    /// <param name="controller">TransferDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public TransferDialog(TransferDialogController controller, Gtk.Window parent) : this(Builder.FromFile("transfer_dialog.ui", controller.Localizer), controller, parent)
    {
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateTransfer(_destinationAccountRow.GetSubtitle() ?? "", _destinationPasswordRow.GetText(), _amountRow.GetText(), _sourceCurrencyRow.GetText(), _destinationCurrencyRow.GetText());
        _destinationAccountRow.RemoveCssClass("error");
        _destinationAccountRow.SetTitle(_controller.Localizer["DestinationAccount", "Field"]);
        _destinationPasswordRow.RemoveCssClass("error");
        _destinationPasswordRow.SetTitle(_controller.Localizer["DestinationPassword", "Field"]);
        _amountRow.RemoveCssClass("error");
        _amountRow.SetTitle(_controller.Localizer["Amount", "Field"]);
        _amountRow.SetVisible(true);
        _sourceCurrencyRow.RemoveCssClass("error");
        _sourceCurrencyRow.SetTitle(_controller.SourceCurrencyCode);
        _destinationCurrencyRow.RemoveCssClass("error");
        _destinationCurrencyRow.SetTitle(_controller.DestinationCurrencyCode ?? "");
        if (checkStatus == TransferCheckStatus.Valid)
        {
            _conversionResultLabel.SetText(StringHelpers.FormatAmount(_controller.Transfer.DestinationAmount, _controller.CultureForDestNumberString));
            SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidDestPath))
            {
                _destinationAccountRow.AddCssClass("error");
                _destinationAccountRow.SetTitle(_controller.Localizer["DestinationAccount", "Invalid"]);
                _amountRow.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountRequiresPassword))
            {
                _destinationPasswordRow.SetVisible(true);
                _destinationPasswordRow.AddCssClass("error");
                _destinationPasswordRow.SetTitle(_controller.Localizer["DestinationPassword", "Required"]);
                _amountRow.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountPasswordInvalid))
            {
                _destinationPasswordRow.AddCssClass("error");
                _destinationPasswordRow.SetTitle(_controller.Localizer["DestinationPassword", "Invalid"]);
                _amountRow.SetVisible(false);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidAmount))
            {
                _amountRow.AddCssClass("error");
                _amountRow.SetTitle(_controller.Localizer["Amount", "Invalid"]);
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidConversionRate))
            {
                _conversionRateGroup.SetVisible(true);
                _sourceCurrencyRow.AddCssClass("error");
                _sourceCurrencyRow.SetTitle(_controller.SourceCurrencyCode);
                _destinationCurrencyRow.AddCssClass("error");
                _destinationCurrencyRow.SetTitle(_controller.DestinationCurrencyCode!);
                _conversionResultLabel.SetText(_controller.Localizer["NotAvailable"]);
            }
            SetResponseEnabled("ok", false);
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
    private void OnSelectAccount(Gtk.Button sender, EventArgs e)
    {
        var filter = Gtk.FileFilter.New();
        filter.SetName(_controller.Localizer["AccountFileFilter", "GTK"]);
        filter.AddPattern("*.nmoney");
        if (Gtk.Functions.GetMinorVersion() >= 9)
        {
            var openFileDialog = gtk_file_dialog_new();
            gtk_file_dialog_set_title(openFileDialog, _controller.Localizer["SelectAccount"]);
            var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
            filters.Append(filter);
            gtk_file_dialog_set_filters(openFileDialog, filters.Handle);
            _openCallback = async (source, res, data) =>
            {
                var fileHandle = gtk_file_dialog_open_finish(openFileDialog, res, IntPtr.Zero);
                if (fileHandle != IntPtr.Zero)
                {
                    var path = g_file_get_path(fileHandle);
                    _destinationAccountRow.SetSubtitle(path);
                    _destinationPasswordRow.SetVisible(false);
                    _destinationPasswordRow.SetSensitive(true);
                    _destinationPasswordRow.SetText("");
                    _amountRow.SetText("");
                    _conversionRateGroup.SetVisible(false);
                    _sourceCurrencyRow.SetText("");
                    _destinationCurrencyRow.SetText("");
                    Validate();
                }
            };
            gtk_file_dialog_open(openFileDialog, _parentWindow.Handle, IntPtr.Zero, _openCallback, IntPtr.Zero);
        }
        else
        {
            var openFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["SelectAccount"], _parentWindow, Gtk.FileChooserAction.Open, _controller.Localizer["Open"], _controller.Localizer["Cancel"]);
            openFileDialog.SetModal(true);
            openFileDialog.AddFilter(filter);
            openFileDialog.OnResponse += (sender, e) =>
            {
                if (e.ResponseId == (int)Gtk.ResponseType.Accept)
                {
                    var path = openFileDialog.GetFile()!.GetPath() ?? "";
                    _destinationAccountRow.SetSubtitle(path);
                    _destinationPasswordRow.SetVisible(false);
                    _destinationPasswordRow.SetSensitive(true);
                    _destinationPasswordRow.SetText("");
                    _amountRow.SetText("");
                    _conversionRateGroup.SetVisible(false);
                    _sourceCurrencyRow.SetText("");
                    _destinationCurrencyRow.SetText("");
                    Validate();
                }
            };
            openFileDialog.Show();
        }
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
                row.SetText(row.GetText() + _controller.CultureForSourceNumberString.NumberFormat.NumberDecimalSeparator);
                row.SetPosition(row.GetText().Length);
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
                row.SetText(row.GetText() + _controller.CultureForDestNumberString.NumberFormat.NumberDecimalSeparator);
                row.SetPosition(row.GetText().Length);
                return true;
            }
        }
        return false;
    }
}