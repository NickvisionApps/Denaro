using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for creating a new account
/// </summary>
public partial class NewAccountDialog : Adw.Window
{
    private delegate void GAsyncReadyCallback(nint source, nint res, nint user_data);
    
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_new();
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_title(nint dialog, string title);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_filters(nint dialog, nint filters);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_initial_folder(nint dialog, nint folder);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_open(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_open_finish(nint dialog, nint result, nint error);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_select_folder(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_select_folder_finish(nint dialog, nint result, nint error);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_get_user_special_dir(int dir);
    
    private readonly NewAccountDialogController _controller;
    private uint _currentPageNumber;
    private GAsyncReadyCallback? _saveCallback;
    private GAsyncReadyCallback? _openCallback;
    
    [Gtk.Connect] private readonly Gtk.Button _backButton;
    [Gtk.Connect] private readonly Adw.Carousel _carousel;
    [Gtk.Connect] private readonly Gtk.Button _startButton;
    [Gtk.Connect] private readonly Adw.EntryRow _accountNameRow;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _accountPasswordRow;
    [Gtk.Connect] private readonly Adw.EntryRow _folderRow;
    [Gtk.Connect] private readonly Gtk.Button _selectFolderButton;
    [Gtk.Connect] private readonly Adw.ActionRow _overwriteRow;
    [Gtk.Connect] private readonly Gtk.Switch _overwriteSwitch;
    [Gtk.Connect] private readonly Gtk.Button _nextButton1;
    [Gtk.Connect] private readonly Adw.ComboRow _accountTypeRow;
    [Gtk.Connect] private readonly Gtk.ToggleButton _incomeButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _expenseButton;
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
        _customDecimalSeparatorRow.SetModel(Gtk.StringList.New(new string[3] { ". ", ", ", _p("DecimalSeparator", "Other") }));
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
        _customGroupSeparatorRow.SetModel(Gtk.StringList.New(new string[5] { ". ", ", ", "' ", _p("GroupSeparator", "None"), _p("GroupSeparator", "Other") }));
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
        _controller.Folder = g_get_user_special_dir(1); // XDG_DOCUMENTS_DIR
        if(!Directory.Exists(_controller.Folder))
        {
            _controller.Folder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{Path.DirectorySeparatorChar}Doccuments";
        }
        if(!Directory.Exists(_controller.Folder))
        {
            _controller.Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        _folderRow.SetText(Path.GetFileName(_controller.Folder));
        _accountTypeRow.SetSelected(0);
        _incomeButton.SetActive(true);
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
    private void SelectFolder(object? sender, EventArgs e)
    {
        var folderDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(folderDialog, _("Select Folder"));
        if (Directory.Exists(_controller.Folder) && _controller.Folder != "/")
        {
            var folder = Gio.FileHelper.NewForPath(_controller.Folder);
            gtk_file_dialog_set_initial_folder(folderDialog, folder.Handle);
        }
        _saveCallback = (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_select_folder_finish(folderDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                _controller.Folder = g_file_get_path(fileHandle);
                _folderRow.SetText(Path.GetFileName(_controller.Folder));
                ValidateName();
            }
        };
        gtk_file_dialog_select_folder(folderDialog, Handle, IntPtr.Zero, _saveCallback, IntPtr.Zero);
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
    private void SelectImportFile(object? sender, EventArgs e)
    {
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
        var openFileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(openFileDialog, _("Import from Account"));
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterAll);
        filters.Append(filterCsv);
        filters.Append(filterOfx);
        filters.Append(filterQif);
        gtk_file_dialog_set_filters(openFileDialog, filters.Handle);
        _openCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_open_finish(openFileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                _controller.ImportFile = g_file_get_path(fileHandle);
                _importRow.SetText(_controller.ImportFile);
            }
        };
        gtk_file_dialog_open(openFileDialog, Handle, IntPtr.Zero, _openCallback, IntPtr.Zero);
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
        OnApply?.Invoke(this, EventArgs.Empty);
    }
}
