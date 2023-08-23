using Nickvision.Aura.Keyring;
using Nickvision.GirExt;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.IO;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for creating a new account
/// </summary>
public partial class NewAccountDialog : Adw.Window
{
    private readonly NewAccountDialogController _controller;
    private uint _currentPageNumber;

    [Gtk.Connect] private readonly Gtk.Button _backButton;
    [Gtk.Connect] private readonly Adw.Carousel _carousel;
    [Gtk.Connect] private readonly Gtk.Button _startButton;
    [Gtk.Connect] private readonly Adw.EntryRow _accountNameRow;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _accountPasswordRow;
    [Gtk.Connect] private readonly Adw.ActionRow _accountPasswordStrengthRow;
    [Gtk.Connect] private readonly Gtk.LevelBar _accountPasswordStrengthBar;
    [Gtk.Connect] private readonly Adw.EntryRow _folderRow;
    [Gtk.Connect] private readonly Gtk.Button _selectFolderButton;
    [Gtk.Connect] private readonly Adw.ActionRow _overwriteRow;
    [Gtk.Connect] private readonly Gtk.Switch _overwriteSwitch;
    [Gtk.Connect] private readonly Gtk.Button _nextButton1;
    [Gtk.Connect] private readonly Adw.ComboRow _accountTypeRow;
    [Gtk.Connect] private readonly Gtk.ToggleButton _incomeButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _expenseButton;
    [Gtk.Connect] private readonly Adw.ComboRow _transactionRemindersRow;
    [Gtk.Connect] private readonly Gtk.Button _nextButton2;
    [Gtk.Connect] private readonly Gtk.Label _reportedCurrencyLabel;
    [Gtk.Connect] private readonly Adw.ExpanderRow _rowCustomCurrency;
    [Gtk.Connect] private readonly Adw.EntryRow _customSymbolRow;
    [Gtk.Connect] private readonly Adw.EntryRow _customCodeRow;
    [Gtk.Connect] private readonly Gtk.Entry _customDecimalSeparatorText;
    [Gtk.Connect] private readonly Adw.ComboRow _customDecimalSeparatorRow;
    [Gtk.Connect] private readonly Gtk.Entry _customGroupSeparatorText;
    [Gtk.Connect] private readonly Adw.ComboRow _customGroupSeparatorRow;
    [Gtk.Connect] private readonly Adw.ComboRow _customDecimalDigitsRow;
    [Gtk.Connect] private readonly Gtk.Button _nextButton3;
    [Gtk.Connect] private readonly Adw.EntryRow _importRow;
    [Gtk.Connect] private readonly Gtk.Button _selectImportFileButton;
    [Gtk.Connect] private readonly Gtk.Button _clearImportFileButton;
    [Gtk.Connect] private readonly Gtk.Button _createButton;
    
    public event EventHandler? OnApply;
    
    /// <summary>
    /// Constructs a NewAccountDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="controller">NewAccountDialogController</param>
    /// <param name="parent">Gtk.Window</param>
    private NewAccountDialog(Gtk.Builder builder, NewAccountDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _currentPageNumber = 0;
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        //Build UI
        builder.Connect(this);
        _backButton.OnClicked += GoBack;
        _startButton.OnClicked += GoForward;
        _accountNameRow.OnNotify += (sender, e) =>
        {
            if(e.Pspec.GetName() == "text")
            {
                ValidateName();
            }
        };
        _accountPasswordRow.OnNotify += (sender, e) =>
        {
            if(e.Pspec.GetName() == "text")
            {
                ShowPasswordStrength();
            }
        };
        _accountPasswordStrengthBar.SetMinValue(Convert.ToDouble((int)PasswordStrength.Blank));
        _accountPasswordStrengthBar.SetMaxValue(Convert.ToDouble((int)PasswordStrength.VeryStrong));
        _accountPasswordStrengthBar.AddOffsetValue("veryweak", 1);
        _accountPasswordStrengthBar.AddOffsetValue("weak", 2);
        _accountPasswordStrengthBar.AddOffsetValue("medium", 3);
        _accountPasswordStrengthBar.AddOffsetValue("strong", 4);
        _accountPasswordStrengthBar.AddOffsetValue("verystrong", 5);
        _selectFolderButton.OnClicked += SelectFolder;
        _overwriteSwitch.OnNotify += (sender, e) =>
        {
            if(e.Pspec.GetName() == "active")
            {
                _controller.OverwriteExisting = _overwriteSwitch.GetActive();
                ValidateName();
            }
        };
        _nextButton1.OnClicked += GoForward;
        _incomeButton.OnToggled += OnTransactionTypeChanged;
        _expenseButton.OnToggled += OnTransactionTypeChanged;
        _expenseButton.BindProperty("active", _incomeButton, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        _nextButton2.OnClicked += GoForward;
        _rowCustomCurrency.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "expanded")
            {
                ValidateCurrency();
            }
        };
        _customSymbolRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if(_customSymbolRow.GetText().Length > 3)
                {
                    _customSymbolRow.SetText(_customSymbolRow.GetText().Substring(0, 3));
                    _customSymbolRow.SetPosition(-1);
                }
                ValidateCurrency();
            }
        };
        _customCodeRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if(_customCodeRow.GetText().Length > 3)
                {
                    _customCodeRow.SetText(_customCodeRow.GetText().Substring(0, 3));
                    _customCodeRow.SetPosition(-1);
                }
                ValidateCurrency();
            }
        };
        _customDecimalSeparatorRow.SetSelected(0);
        _customDecimalSeparatorRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                if (_customDecimalSeparatorRow.GetSelected() == 2)
                {
                    _customDecimalSeparatorText.SetVisible(true);
                    _customDecimalSeparatorText.GrabFocus();
                }
                else
                {
                    _customDecimalSeparatorText.SetVisible(false);
                }
                ValidateCurrency();
            }
        };
        _customDecimalSeparatorText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                ValidateCurrency();
            }
        };
        _customGroupSeparatorRow.SetSelected(1);
        _customGroupSeparatorRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                if (_customGroupSeparatorRow.GetSelected() == 4)
                {
                    _customGroupSeparatorText.SetVisible(true);
                    _customGroupSeparatorText.GrabFocus();
                }
                else
                {
                    _customGroupSeparatorText.SetVisible(false);
                }
                ValidateCurrency();
            }
        };
        _customGroupSeparatorText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                ValidateCurrency();
            }
        };
        _customDecimalDigitsRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                ValidateCurrency();
            }
        };
        _nextButton3.OnClicked += GoForward;
        _selectImportFileButton.OnClicked += SelectImportFile;
        _clearImportFileButton.OnClicked += (sender, e) =>
        {
            _controller.ImportFile = "";
            _importRow.SetText("");
        };
        _createButton.OnClicked += Apply;
        //Load
        _controller.Folder = GLib.Functions.GetUserSpecialDir(GLib.UserDirectory.DirectoryDocuments) ?? "";
        if(!Directory.Exists(_controller.Folder))
        {
            _controller.Folder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{Path.DirectorySeparatorChar}Documents";
        }
        if(!Directory.Exists(_controller.Folder))
        {
            _controller.Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        _folderRow.SetText(Path.GetFileName(_controller.Folder));
        _accountTypeRow.SetSelected(0);
        _incomeButton.SetActive(true);
        _transactionRemindersRow.SetSelected(0);
        _reportedCurrencyLabel.SetLabel($"{_("Your system reported that your currency is")}\n<b>{_controller.ReportedCurrencyString}</b>");
    }
    
    /// <summary>
    /// Constructs a NewAccountDialog
    /// </summary>
    /// <param name="controller">NewAccountDialogController</param>
    /// <param name="parent">Gtk.Window</param>
    public NewAccountDialog(NewAccountDialogController controller, Gtk.Window parent) : this(Builder.FromFile("new_account_dialog.ui"), controller, parent)
    {
    }
    
    /// <summary>
    /// Navigates the carousel backwards
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void GoBack(object? sender, EventArgs e)
    {
        _currentPageNumber--;
        _carousel.ScrollTo(_carousel.GetNthPage(_currentPageNumber), true);
        _backButton.SetVisible(_currentPageNumber > 0);
    }
    
    /// <summary>
    /// Navigates the carousel forwards
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void GoForward(object? sender, EventArgs e)
    {
        _currentPageNumber++;
        _carousel.ScrollTo(_carousel.GetNthPage(_currentPageNumber), true);
        _backButton.SetVisible(_currentPageNumber > 0);
    }

    /// <summary>
    /// Selects a folder to save the account
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private async void SelectFolder(object? sender, EventArgs e)
    {
        var folderDialog = Gtk.FileDialog.New();
        folderDialog.SetTitle(_("Select Folder"));
        if (Directory.Exists(_controller.Folder) && _controller.Folder != "/")
        {
            var folder = Gio.FileHelper.NewForPath(_controller.Folder);
            folderDialog.SetInitialFolder(folder);
        }
        try
        {
            var file = await folderDialog.SelectFolderAsync(this);
            _controller.Folder = file.GetPath();
            _folderRow.SetText(Path.GetFileName(_controller.Folder));
            ValidateName();
        }
        catch { }
    }
    
    /// <summary>
    /// Validates the name of the account
    /// </summary>
    private void ValidateName()
    {
        _accountNameRow.RemoveCssClass("error");
        _accountNameRow.SetTitle(_("Account Name"));
        var checkStatus = _controller.UpdateName(_accountNameRow.GetText());
        if(checkStatus == NameCheckStatus.Valid)
        {
            _nextButton1.SetSensitive(!string.IsNullOrEmpty(_accountNameRow.GetText()));
        }
        else
        {
            if(checkStatus.HasFlag(NameCheckStatus.AlreadyOpen))
            {
                _accountNameRow.AddCssClass("error");
                _accountNameRow.SetTitle(_("Account Name (Opened)"));
            }
            if(checkStatus.HasFlag(NameCheckStatus.Exists))
            {
                _accountNameRow.AddCssClass("error");
                _accountNameRow.SetTitle(_("Account Name (Exists)"));
            }
            _nextButton1.SetSensitive(false);
        }
    }
    
    /// <summary>
    /// Calculates and shows the account password's strength
    /// </summary>
    private void ShowPasswordStrength()
    {
        if (!string.IsNullOrEmpty(_accountPasswordRow.GetText()))
        {
            var strength = Credential.GetPasswordStrength(_accountPasswordRow.GetText());
            _accountPasswordStrengthRow.SetVisible(true);
            _accountPasswordStrengthBar.SetValue((double)strength);
        }
        else
        {
            _accountPasswordStrengthRow.SetVisible(false);
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
    }
    
    /// <summary>
    /// Validates the custom currency of the account
    /// </summary>
    private void ValidateCurrency()
    {
        var customDecimalSeparator = _customDecimalSeparatorRow.GetSelected() switch
        {
            0 => ".",
            1 => ",",
            2 => _customDecimalSeparatorText.GetText()
        };
        var customGroupSeparator = _customGroupSeparatorRow.GetSelected() switch
        {
            0 => ".",
            1 => ",",
            2 => "'",
            3 => "",
            4 => _customGroupSeparatorText.GetText()
        };
        var customDecimalDigits = _customDecimalDigitsRow.GetSelected() == 5 ? 99 : _customDecimalDigitsRow.GetSelected() + 2;
        _customSymbolRow.RemoveCssClass("error");
        _customSymbolRow.SetTitle(_("Currency Symbol"));
        _customCodeRow.RemoveCssClass("error");
        _customCodeRow.SetTitle(_("Currency Code"));
        _customDecimalSeparatorRow.RemoveCssClass("error");
        _customDecimalSeparatorRow.SetTitle(_("Decimal Separator"));
        _customGroupSeparatorRow.RemoveCssClass("error");
        _customGroupSeparatorRow.SetTitle(_("Group Separator"));
        var checkStatus = _controller.UpdateCurrency(_rowCustomCurrency.GetExpanded(), _customSymbolRow.GetText(), _customCodeRow.GetText(), customDecimalSeparator, customGroupSeparator, customDecimalDigits);
        if (checkStatus == CurrencyCheckStatus.Valid)
        {
            _createButton.SetSensitive(true);
        }
        else
        {
            if (checkStatus.HasFlag(CurrencyCheckStatus.EmptyCurrencySymbol))
            {
                _customSymbolRow.AddCssClass("error");
                _customSymbolRow.SetTitle(_("Currency Symbol (Empty)"));
            }
            if (checkStatus.HasFlag(CurrencyCheckStatus.InvalidCurrencySymbol))
            {
                _customSymbolRow.AddCssClass("error");
                _customSymbolRow.SetTitle(_("Currency Symbol (Invalid)"));
            }
            if (checkStatus.HasFlag(CurrencyCheckStatus.EmptyCurrencyCode))
            {
                _customCodeRow.AddCssClass("error");
                _customCodeRow.SetTitle(_("Currency Code (Empty)"));
            }
            if (checkStatus.HasFlag(CurrencyCheckStatus.EmptyDecimalSeparator))
            {
                _customDecimalSeparatorRow.AddCssClass("error");
                _customDecimalSeparatorRow.SetTitle(_("Decimal Separator (Empty)"));
            }
            if (checkStatus.HasFlag(CurrencyCheckStatus.SameSeparators))
            {
                _customDecimalSeparatorRow.AddCssClass("error");
                _customDecimalSeparatorRow.SetTitle(_("Decimal Separator (Invalid)"));
                _customGroupSeparatorRow.AddCssClass("error");
                _customGroupSeparatorRow.SetTitle(_("Group Separator (Invalid)"));
            }
            if (checkStatus.HasFlag(CurrencyCheckStatus.SameSymbolAndDecimalSeparator))
            {
                _customSymbolRow.AddCssClass("error");
                _customSymbolRow.SetTitle(_("Currency Symbol (Invalid)"));
                _customDecimalSeparatorRow.AddCssClass("error");
                _customDecimalSeparatorRow.SetTitle(_("Decimal Separator (Invalid)"));
            }
            if (checkStatus.HasFlag(CurrencyCheckStatus.SameSymbolAndGroupSeparator))
            {
                _customSymbolRow.AddCssClass("error");
                _customSymbolRow.SetTitle(_("Currency Symbol (Invalid)"));
                _customGroupSeparatorRow.AddCssClass("error");
                _customGroupSeparatorRow.SetTitle(_("Group Separator (Invalid)"));
            }
            _createButton.SetSensitive(false);
        }
    }
    
    /// <summary>
    /// Selects a file to import data from
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private async void SelectImportFile(object? sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileDialog.New();
        openFileDialog.SetTitle(_("Import from Account"));
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName($"{_("All files")} (*.csv, *.ofx, *.qif)");
        filterAll.AddPattern("*.csv");
        filterAll.AddPattern("*.CSV");
        filterAll.AddPattern("*.ofx");
        filterAll.AddPattern("*.OFX");
        filterAll.AddPattern("*.qif");
        filterAll.AddPattern("*.QIF");
        var filterCsv = Gtk.FileFilter.New();
        filterCsv.SetName("CSV (*.csv)");
        filterCsv.AddPattern("*.csv");
        filterCsv.AddPattern("*.CSV");
        var filterOfx = Gtk.FileFilter.New();
        filterOfx.SetName("Open Financial Exchange (*.ofx)");
        filterOfx.AddPattern("*.ofx");
        filterOfx.AddPattern("*.OFX");
        var filterQif = Gtk.FileFilter.New();
        filterQif.SetName("Quicken Format (*.qif)");
        filterQif.AddPattern("*.qif");
        filterQif.AddPattern("*.QIF");    
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterAll);
        filters.Append(filterCsv);
        filters.Append(filterOfx);
        filters.Append(filterQif);
        openFileDialog.SetFilters(filters);
        try
        {
            var file = await openFileDialog.OpenAsync(this);
            _controller.ImportFile = file.GetPath();
            _importRow.SetText(_controller.ImportFile);
        }
        catch { }
    }

    /// <summary>
    /// Applies the dialog
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void Apply(object? sender, EventArgs e)
    {
        _controller.Password = _accountPasswordRow.GetText();
        _controller.Metadata.AccountType = (AccountType)_accountTypeRow.GetSelected();
        _controller.Metadata.DefaultTransactionType = _incomeButton.GetActive() ? TransactionType.Income : TransactionType.Expense;
        _controller.Metadata.TransactionRemindersThreshold = (RemindersThreshold)_transactionRemindersRow.GetSelected();
        OnApply?.Invoke(this, EventArgs.Empty);
    }
}
