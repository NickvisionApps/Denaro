using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog to configure account
/// </summary>
public partial class AccountSettingsDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    private bool _constructing;
    private readonly AccountSettingsDialogController _controller;
    private readonly Adw.MessageDialog _dialog;
    private readonly Gtk.Box _boxMain;
    private readonly Gtk.Button _btnAvatar;
    private readonly Gtk.CssProvider _btnAvatarCssProvider;
    private readonly Adw.PreferencesGroup _grpAccount;
    private readonly Adw.EntryRow _rowName;
    private readonly Adw.ComboRow _rowAccountType;
    private readonly Gtk.ToggleButton _btnIncome;
    private readonly Gtk.ToggleButton _btnExpense;
    private readonly Gtk.Box _boxTypeButtons;
    private readonly Adw.ActionRow _rowTransactionType;
    private readonly Gtk.Label _lblReportedCurrency;
    private readonly Adw.PreferencesGroup _grpCurrency;
    private readonly Adw.ExpanderRow _rowCustomCurrency;
    private readonly Gtk.Entry _txtCustomSymbol;
    private readonly Adw.ActionRow _rowCustomSymbol;
    private readonly Gtk.Entry _txtCustomCode;
    private readonly Adw.ActionRow _rowCustomCode;

    /// <summary>
    /// Constructs an AccountSettingsDialog
    /// </summary>
    /// <param name="controller">AccountSettingsDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public AccountSettingsDialog(AccountSettingsDialogController controller, Gtk.Window parentWindow)
    {
        _constructing = true;
        _controller = controller;
        //Dialog Settings
        _dialog = Adw.MessageDialog.New(parentWindow, _controller.Localizer["AccountSettings"], "");
        _dialog.SetDefaultSize(450, -1);
        _dialog.SetHideOnClose(true);
        _dialog.SetModal(true);
        if(!_controller.IsFirstTimeSetup)
        {
            _dialog.AddResponse("cancel", _controller.Localizer["Cancel"]);
            _dialog.SetCloseResponse("cancel");
        }
        _dialog.AddResponse("ok", _controller.Localizer["OK"]);
        _dialog.SetDefaultResponse("ok");
        _dialog.SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
        //Main Box
        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 16);
        //Avatar
        _btnAvatar = Gtk.Button.New();
        _btnAvatar.AddCssClass("circular");
        _btnAvatar.AddCssClass("title-1");
        _btnAvatar.SetName("btnAvatar");
        _btnAvatar.SetHalign(Gtk.Align.Center);
        _btnAvatar.SetSizeRequest(96, 96);
        _btnAvatarCssProvider = Gtk.CssProvider.New();
        _btnAvatar.GetStyleContext().AddProvider(_btnAvatarCssProvider, 800);
        _boxMain.Append(_btnAvatar);
        //Preferences Group
        _grpAccount = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpAccount);
        //Account Name
        _rowName = Adw.EntryRow.New();
        _rowName.SetShowApplyButton(true);
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        _rowName.OnApply += OnApplyName;
        _grpAccount.Add(_rowName);
        //Account Type
        _rowAccountType = Adw.ComboRow.New();
        _rowAccountType.SetModel(Gtk.StringList.New(new string[3] {_controller.Localizer["AccountType", "Checking"], _controller.Localizer["AccountType", "Savings"], _controller.Localizer["AccountType", "Business"]}));
        _rowAccountType.OnNotify += (sender, e) =>
        {
            if(e.Pspec.GetName() == "selected-item")
            {
                OnAccountTypeChanged();
            }
        };
        _rowAccountType.SetTitle(_controller.Localizer["AccountType", "Field"]);
        _rowAccountType.SetSubtitle(_controller.Localizer["AccountType", "Description"]);
        _rowAccountType.SetSubtitleLines(4);
        _grpAccount.Add(_rowAccountType);
        //Default Transaction Type
        _btnIncome = Gtk.ToggleButton.NewWithLabel(_controller.Localizer["Income"]);
        _btnIncome.OnToggled += OnTransactionTypeChanged;
        _btnExpense = Gtk.ToggleButton.NewWithLabel(_controller.Localizer["Expense"]);
        _btnExpense.OnToggled += OnTransactionTypeChanged;
        _btnExpense.BindProperty("active", _btnIncome, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        _boxTypeButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        _boxTypeButtons.SetValign(Gtk.Align.Center);
        _boxTypeButtons.AddCssClass("linked");
        _boxTypeButtons.Append(_btnIncome);
        _boxTypeButtons.Append(_btnExpense);
        _rowTransactionType = Adw.ActionRow.New();
        _rowTransactionType.SetTitle(_controller.Localizer["DefaultTransactionType", "Field"]);
        _rowTransactionType.AddSuffix(_boxTypeButtons);
        _grpAccount.Add(_rowTransactionType);
        //Reported Currency
        _lblReportedCurrency = Gtk.Label.New($"{_controller.Localizer["ReportedCurrency"]}\n<b>{NumberFormatInfo.CurrentInfo.CurrencySymbol} ({RegionInfo.CurrentRegion.ISOCurrencySymbol})</b>");
        _lblReportedCurrency.SetUseMarkup(true);
        _lblReportedCurrency.SetJustify(Gtk.Justification.Center);
        _boxMain.Append(_lblReportedCurrency);
        //Custom Currency
        _grpCurrency = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpCurrency);
        _rowCustomCurrency = Adw.ExpanderRow.New();
        _rowCustomCurrency.SetTitle(_controller.Localizer["UseCustomCurrency", "Field"]);
        _rowCustomCurrency.SetShowEnableSwitch(true);
        _rowCustomCurrency.SetEnableExpansion(false);
        _rowCustomCurrency.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "enable-expansion")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _grpCurrency.Add(_rowCustomCurrency);
        _txtCustomSymbol = Gtk.Entry.New();
        _txtCustomSymbol.SetValign(Gtk.Align.Center);
        _txtCustomSymbol.SetMaxLength(2);
        _txtCustomSymbol.SetPlaceholderText(_controller.Localizer["CustomCurrencySymbol", "Placeholder"]);
        _txtCustomSymbol.SetActivatesDefault(true);
        _txtCustomSymbol.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _rowCustomSymbol = Adw.ActionRow.New();
        _rowCustomSymbol.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Field"]);
        _rowCustomSymbol.AddSuffix(_txtCustomSymbol);
        _rowCustomCurrency.AddRow(_rowCustomSymbol);
        _txtCustomCode = Gtk.Entry.New();
        _txtCustomCode.SetValign(Gtk.Align.Center);
        _txtCustomCode.SetMaxLength(3);
        _txtCustomCode.SetPlaceholderText(_controller.Localizer["CustomCurrencyCode", "Placeholder"]);
        _txtCustomCode.SetActivatesDefault(true);
        _txtCustomCode.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _rowCustomCode = Adw.ActionRow.New();
        _rowCustomCode.SetTitle(_controller.Localizer["CustomCurrencyCode", "Field"]);
        _rowCustomCode.AddSuffix(_txtCustomCode);
        _rowCustomCurrency.AddRow(_rowCustomCode);
        //Layout
        _dialog.SetExtraChild(_boxMain);
        //Load
        _rowName.SetText(_controller.Metadata.Name);
        OnApplyName(_rowName, EventArgs.Empty);
        _rowAccountType.SetSelected((uint)_controller.Metadata.AccountType);
        OnAccountTypeChanged();
        _btnIncome.SetActive(_controller.Metadata.DefaultTransactionType == TransactionType.Income);
        _rowCustomCurrency.SetEnableExpansion(_controller.Metadata.UseCustomCurrency);
        _txtCustomSymbol.SetText(_controller.Metadata.CustomCurrencySymbol ?? "");
        _txtCustomCode.SetText(_controller.Metadata.CustomCurrencyCode ?? "");
        Validate();
        _constructing = false;
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
        var transactionType = _btnIncome.GetActive() ? TransactionType.Income : TransactionType.Expense;
        var checkStatus = _controller.UpdateMetadata(_rowName.GetText(), (AccountType)_rowAccountType.GetSelected(), _rowCustomCurrency.GetEnableExpansion(), _txtCustomSymbol.GetText(), _txtCustomCode.GetText(), transactionType);
        _rowName.RemoveCssClass("error");
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        _rowCustomSymbol.RemoveCssClass("error");
        _rowCustomSymbol.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Field"]);
        if(checkStatus == AccountMetadataCheckStatus.Valid)
        {
            _dialog.SetResponseEnabled("ok", true);
        }
        else
        {
            if(checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyName))
            {
                _rowName.AddCssClass("error");
                _rowName.SetTitle(_controller.Localizer["Name", "Empty"]);
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyCurrencySymbol))
            {
                _rowCustomSymbol.AddCssClass("error");
                _rowCustomSymbol.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Empty"]);
            }
            _dialog.SetResponseEnabled("ok", false);
        }
    }

    /// <summary>
    /// Occurs when a new name is applied to Adw.EntryRow
    /// </summary>
    /// <param name="sender">Adw.EntryRow</param>
    /// <param name="e">EventArgs</param>
    private void OnApplyName(Adw.EntryRow sender, EventArgs e)
    {
        if(!_constructing)
        {
            Validate();
        }
        if(_rowName.GetText().Length == 0)
        {
            _btnAvatar.SetLabel(_controller.Localizer["NotAvailable"]);
        }
        else
        {
            var split = _rowName.GetText().Split(' ');
            if(split.Length == 1)
            {
                _btnAvatar.SetLabel(split[0].Substring(0, split[0].Length > 1 ? 2 : 1));
            }
            else
            {
                if (string.IsNullOrEmpty(split[0]) && string.IsNullOrEmpty(split[1]))
                {
                    _btnAvatar.SetLabel(_controller.Localizer["NotAvailable"]);
                }
                else if (string.IsNullOrEmpty(split[0]))
                {
                    _btnAvatar.SetLabel(split[1].Substring(0, split[1].Length > 1 ? 2 : 1));
                }
                else if (string.IsNullOrEmpty(split[1]))
                {
                    _btnAvatar.SetLabel(split[0].Substring(0, split[0].Length > 1 ? 2 : 1));
                }
                else
                {
                    var emojiPattern = @"\p{Cs}.";
                    if (Regex.Match(split[0], emojiPattern).Success && Regex.Match(split[1], emojiPattern).Success)
                    {
                        _btnAvatar.SetLabel($"{split[0][0]}{split[0][1]}{split[1][0]}{split[1][1]}");
                    }
                    else if (Regex.Match(split[0], emojiPattern).Success)
                    {
                        _btnAvatar.SetLabel($"{split[0][0]}{split[0][1]}{split[1][0]}");
                    }
                    else if (Regex.Match(split[1], emojiPattern).Success)
                    {
                        _btnAvatar.SetLabel($"{split[0][0]}{split[1][0]}{split[1][1]}");
                    }
                    else
                    {
                        _btnAvatar.SetLabel($"{split[0][0]}{split[1][0]}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Occurs when the account type selection in changed
    /// </summary>
    private void OnAccountTypeChanged()
    {
        if (!_constructing)
        {
            Validate();
        }
        var bgColorString = _controller.GetColorForAccountType((AccountType)_rowAccountType.GetSelected());
        var bgColorStrArray = new Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
        var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
        _btnAvatar.GetStyleContext().RemoveProvider(_btnAvatarCssProvider);
        var btnCss = "#btnAvatar { color: " + (luma < 0.5 ? "#fff" : "#000") + "; background-color: " + bgColorString + "; }";
        gtk_css_provider_load_from_data(_btnAvatarCssProvider.Handle, btnCss, btnCss.Length);
        _btnAvatar.GetStyleContext().AddProvider(_btnAvatarCssProvider, 800);
    }

    /// <summary>
    /// Occurs when either Income or Expense button is toggled
    /// </summary>
    /// <param name="sender">Gtk.ToggleButton</param>
    /// <param name="e">EventArgs</param>
    private void OnTransactionTypeChanged(Gtk.ToggleButton sender, EventArgs e)
    {
        if (!_constructing)
        {
            Validate();
        }
        if (_btnIncome.GetActive())
        {
            _btnIncome.AddCssClass("success");
            _btnIncome.AddCssClass("denaro-income");
            _btnExpense.RemoveCssClass("error");
            _btnExpense.RemoveCssClass("denaro-expense");
        }
        else
        {

            _btnIncome.RemoveCssClass("success");
            _btnIncome.RemoveCssClass("denaro-income");
            _btnExpense.AddCssClass("error");
            _btnExpense.AddCssClass("denaro-expense");
        }
    }
}
