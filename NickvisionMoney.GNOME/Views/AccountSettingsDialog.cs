using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog to configure account
/// </summary>
public partial class AccountSettingsDialog : Adw.MessageDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

    private bool _constructing;
    private readonly AccountSettingsDialogController _controller;

    [Gtk.Connect] private readonly Adw.EntryRow _nameRow;
    [Gtk.Connect] private readonly Adw.ComboRow _accountTypeRow;
    [Gtk.Connect] private readonly Gtk.ToggleButton _incomeButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _expenseButton;
    [Gtk.Connect] private readonly Gtk.Label _reportedCurrencyLabel;
    [Gtk.Connect] private readonly Adw.ExpanderRow _customCurrencyRow;
    [Gtk.Connect] private readonly Gtk.Entry _customSymbolText;
    [Gtk.Connect] private readonly Adw.ActionRow _customSymbolRow;
    [Gtk.Connect] private readonly Gtk.Entry _customCodeText;
    [Gtk.Connect] private readonly Adw.ActionRow _customCodeRow;
    [Gtk.Connect] private readonly Gtk.DropDown _customDecimalSeparatorDropDown;
    [Gtk.Connect] private readonly Gtk.Entry _customDecimalSeparatorText;
    [Gtk.Connect] private readonly Adw.ActionRow _customDecimalSeparatorRow;
    [Gtk.Connect] private readonly Gtk.DropDown _customGroupSeparatorDropDown;
    [Gtk.Connect] private readonly Gtk.Entry _customGroupSeparatorText;
    [Gtk.Connect] private readonly Adw.ActionRow _customGroupSeparatorRow;
    [Gtk.Connect] private readonly Gtk.DropDown _customDecimalDigitsDropDown;
    [Gtk.Connect] private readonly Adw.ActionRow _customDecimalDigitsRow;
    [Gtk.Connect] private readonly Adw.ExpanderRow _passwordRow;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _newPasswordRow;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _newPasswordConfirmRow;
    [Gtk.Connect] private readonly Gtk.Button _removePasswordButton;

    private AccountSettingsDialog(Gtk.Builder builder, AccountSettingsDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _constructing = true;
        _controller = controller;
        //Dialog Settings
        SetTransientFor(parent);
        if (!_controller.NeedsSetup)
        {
            AddResponse("cancel", _controller.Localizer["Cancel"]);
            SetCloseResponse("cancel");
        }
        AddResponse("ok", _controller.Localizer["Apply"]);
        SetDefaultResponse("ok");
        SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
        //Build UI
        builder.Connect(this);
        //Account Name
        _nameRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        //Account Type
        _accountTypeRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                Validate();
            }
        };
        //Default Transaction Type
        _incomeButton.OnToggled += OnTransactionTypeChanged;
        _expenseButton.OnToggled += OnTransactionTypeChanged;
        _expenseButton.BindProperty("active", _incomeButton, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        //Reported Currency
        _reportedCurrencyLabel.SetLabel($"{_controller.Localizer["ReportedCurrency"]}\n<b>{_controller.ReportedCurrencyString}</b>");
        //Custom Currency
        _customCurrencyRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "enable-expansion")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _customSymbolText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _customCodeText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _customDecimalSeparatorDropDown.SetModel(Gtk.StringList.New(new string [3] { ".", ",", _controller.Localizer["CustomCurrencyDecimalSeparator", "Other"] }));
        _customDecimalSeparatorDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                if (!_constructing)
                {
                    _customDecimalSeparatorText.Visible = _customDecimalSeparatorDropDown.GetSelected() == 2;
                    Validate();
                }
            }
        };
        _customDecimalSeparatorText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _customGroupSeparatorDropDown.SetModel(Gtk.StringList.New(new string [5] { ".", ",", "'", _controller.Localizer["CustomCurrencyGroupSeparator", "None"], _controller.Localizer["CustomCurrencyGroupSeparator", "Other"] }));
        _customGroupSeparatorDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                if (!_constructing)
                {
                    _customGroupSeparatorText.Visible = _customGroupSeparatorDropDown.GetSelected() == 4;
                    Validate();
                }
            }
        };
        _customGroupSeparatorText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _customDecimalDigitsDropDown.SetModel(Gtk.StringList.New(new string [6] { "2", "3", "4", "5", "6", _controller.Localizer["CustomCurrencyDecimalDigits", "Unlimited"] }));
        _customDecimalDigitsDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        //Password Row
        _passwordRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "enable-expansion")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _newPasswordRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _newPasswordConfirmRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _removePasswordButton.OnClicked += OnRemovePassword;
        //Load
        _nameRow.SetText(_controller.Metadata.Name);
        _accountTypeRow.SetSelected((uint)_controller.Metadata.AccountType);
        _incomeButton.SetActive(_controller.Metadata.DefaultTransactionType == TransactionType.Income);
        _customCurrencyRow.SetEnableExpansion(_controller.Metadata.UseCustomCurrency);
        _customSymbolText.SetText(_controller.Metadata.CustomCurrencySymbol ?? "");
        _customCodeText.SetText(_controller.Metadata.CustomCurrencyCode ?? "");
        _customDecimalSeparatorDropDown.SetSelected(_controller.Metadata.CustomCurrencyDecimalSeparator switch
        {
            null => 0,
            "." => 0,
            "," => 1,
            _ => 2
        });
        if (_customDecimalSeparatorDropDown.GetSelected() == 2)
        {
            _customDecimalSeparatorText.SetVisible(true);
            _customDecimalSeparatorText.SetText(_controller.Metadata.CustomCurrencyDecimalSeparator);
        }
        _customGroupSeparatorDropDown.SetSelected(_controller.Metadata.CustomCurrencyGroupSeparator switch
        {
            null => 1,
            "." => 0,
            "," => 1,
            "'" => 2,
            "" => 3,
            _ => 4
        });
        if (_customGroupSeparatorDropDown.GetSelected() == 4)
        {
            _customGroupSeparatorText.SetVisible(true);
            _customGroupSeparatorText.SetText(_controller.Metadata.CustomCurrencyGroupSeparator);
        }
        _customDecimalDigitsDropDown.SetSelected(_controller.Metadata.CustomCurrencyDecimalDigits switch
        {
            null => 0,
            99 => 5,
            _ => (uint)_controller.Metadata.CustomCurrencyDecimalDigits - 2
        });
        Validate();
        _constructing = false;
    }


    /// <summary>
    /// Constructs an AccountSettingsDialog
    /// </summary>
    /// <param name="controller">AccountSettingsDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public AccountSettingsDialog(AccountSettingsDialogController controller, Gtk.Window parent) : this(Builder.FromFile("account_settings_dialog.ui", controller.Localizer), controller, parent)
    {
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var transactionType = _incomeButton.GetActive() ? TransactionType.Income : TransactionType.Expense;
        var newPassword = "";
        var newPasswordConfirm = "";
        if (_passwordRow.GetEnableExpansion())
        {
            newPassword = _newPasswordRow.GetText();
            newPasswordConfirm = _newPasswordConfirmRow.GetText();
        }
        var customDecimalSeparator = _customDecimalSeparatorDropDown.GetSelected() switch
        {
            0 => ".",
            1 => ",",
            2 => _customDecimalSeparatorText.GetText()
        };
        var customGroupSeparator = _customGroupSeparatorDropDown.GetSelected() switch
        {
            0 => ".",
            1 => ",",
            2 => "'",
            3 => "",
            4 => _customGroupSeparatorText.GetText()
        };
        var customDecimalDigits = _customDecimalDigitsDropDown.GetSelected() == 5 ? 99 : _customDecimalDigitsDropDown.GetSelected() + 2;
        var checkStatus = _controller.UpdateMetadata(_nameRow.GetText(), (AccountType)_accountTypeRow.GetSelected(), _customCurrencyRow.GetEnableExpansion(), _customSymbolText.GetText(), _customCodeText.GetText(), customDecimalSeparator, customGroupSeparator, customDecimalDigits, transactionType, newPassword, newPasswordConfirm);
        _nameRow.RemoveCssClass("error");
        _nameRow.SetTitle(_controller.Localizer["Name", "Field"]);
        _customSymbolRow.RemoveCssClass("error");
        _customSymbolRow.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Field"]);
        _customCodeRow.RemoveCssClass("error");
        _customCodeRow.SetTitle(_controller.Localizer["CustomCurrencyCode", "Field"]);
        _customDecimalSeparatorRow.RemoveCssClass("error");
        _customDecimalSeparatorRow.SetTitle(_controller.Localizer["CustomCurrencyDecimalSeparator", "Field"]);
        _customGroupSeparatorRow.RemoveCssClass("error");
        if (checkStatus == AccountMetadataCheckStatus.Valid)
        {
            SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyName))
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_controller.Localizer["Name", "Empty"]);
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyCurrencySymbol))
            {
                _customSymbolRow.AddCssClass("error");
                _customSymbolRow.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Empty"]);
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyCurrencyCode))
            {
                _customCodeRow.AddCssClass("error");
                _customCodeRow.SetTitle(_controller.Localizer["CustomCurrencyCode", "Empty"]);
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyDecimalSeparator))
            {
                _customDecimalSeparatorRow.AddCssClass("error");
                _customDecimalSeparatorRow.SetTitle(_controller.Localizer["CustomCurrencyDecimalSeparator", "Empty"]);
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.SameSeparators))
            {
                _customDecimalSeparatorRow.AddCssClass("error");
                _customGroupSeparatorRow.AddCssClass("error");
            }
            SetResponseEnabled("ok", false);
        }
    }

    /// <summary>
    /// Occurs when either Income or Expense button is toggled
    /// </summary>
    /// <param name="sender">Gtk.ToggleButton</param>
    /// <param name="e">EventArgs</param>
    private void OnTransactionTypeChanged(Gtk.ToggleButton sender, EventArgs e)
    {
        if (_incomeButton.GetActive())
        {
            _incomeButton.AddCssClass("denaro-income");
            _expenseButton.RemoveCssClass("denaro-expense");
        }
        else
        {
            _incomeButton.RemoveCssClass("denaro-income");
            _expenseButton.AddCssClass("denaro-expense");
        }
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the remove password button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnRemovePassword(Gtk.Button sender, EventArgs e)
    {
        _controller.SetRemovePassword();
        _passwordRow.SetEnableExpansion(false);
        _passwordRow.SetSensitive(false);
        _passwordRow.SetTitle(_controller.Localizer["PasswordRemoveRequest.GTK"]);
        _passwordRow.SetSubtitle("");
    }
}
