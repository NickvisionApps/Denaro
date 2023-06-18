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
    private static partial void gtk_file_dialog_set_initial_folder(nint dialog, nint folder);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_select_folder(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_select_folder_finish(nint dialog, nint result, nint error);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_get_user_special_dir(int dir);
    
    private readonly NewAccountDialogController _controller;
    private uint _currentPageNumber;
    private GAsyncReadyCallback? _saveCallback;
    
    [Gtk.Connect] private readonly Gtk.Button _backButton;
    [Gtk.Connect] private readonly Adw.Carousel _carousel;
    [Gtk.Connect] private readonly Gtk.Button _startButton;
    [Gtk.Connect] private readonly Adw.EntryRow _accountNameRow;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _accountPasswordRow;
    [Gtk.Connect] private readonly Adw.EntryRow _folderRow;
    [Gtk.Connect] private readonly Gtk.Button _selectFolderButton;
    [Gtk.Connect] private readonly Adw.ActionRow _overwriteRow;
    [Gtk.Connect] private readonly Gtk.Switch _overwriteSwitch;
    [Gtk.Connect] private readonly Gtk.Button _saveButton;
    [Gtk.Connect] private readonly Adw.ComboRow _accountTypeRow;
    [Gtk.Connect] private readonly Gtk.ToggleButton _incomeButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _expenseButton;
    [Gtk.Connect] private readonly Gtk.Button _nextButton;
    [Gtk.Connect] private readonly Gtk.Label _reportedCurrencyLabel;
    [Gtk.Connect] private readonly Adw.ExpanderRow _rowCustomCurrency;
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
                _saveButton.SetSensitive(!string.IsNullOrEmpty(_accountNameRow.GetText()));
            }
        };
        _selectFolderButton.OnClicked += SelectFolder;
        _overwriteSwitch.OnNotify += (sender, e) =>
        {
            if(e.Pspec.GetName() == "active")
            {
                _controller.OverwriteExisting = _overwriteSwitch.GetActive();
            }
        };
        _saveButton.OnClicked += GoForward;
        _incomeButton.OnToggled += OnTransactionTypeChanged;
        _expenseButton.OnToggled += OnTransactionTypeChanged;
        _expenseButton.BindProperty("active", _incomeButton, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        _nextButton.OnClicked += GoForward;
        _rowCustomCurrency.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "expanded")
            {
                ValidateCurrency();
            }
        };
        _customSymbolText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                ValidateCurrency();
            }
        };
        _customCodeText.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                ValidateCurrency();
            }
        };
        _customDecimalSeparatorDropDown.SetModel(Gtk.StringList.New(new string[3] { ".", ",", _p("DecimalSeparator", "Other") }));
        _customDecimalSeparatorDropDown.SetSelected(0);
        _customDecimalSeparatorDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                if (_customDecimalSeparatorDropDown.GetSelected() == 2)
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
        _customGroupSeparatorDropDown.SetModel(Gtk.StringList.New(new string[5] { ".", ",", "'", _p("GroupSeparator", "None"), _p("GroupSeparator", "Other") }));
        _customGroupSeparatorDropDown.SetSelected(1);
        _customGroupSeparatorDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                if (_customGroupSeparatorDropDown.GetSelected() == 4)
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
        _customDecimalDigitsDropDown.SetModel(Gtk.StringList.New(new string[6] { "2", "3", "4", "5", "6", _("Unlimited") }));
        _customDecimalDigitsDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                ValidateCurrency();
            }
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
            }
        };
        gtk_file_dialog_select_folder(folderDialog, Handle, IntPtr.Zero, _saveCallback, IntPtr.Zero);
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
        var checkStatus = _controller.UpdateCurrency(_rowCustomCurrency.GetExpanded(), _customSymbolText.GetText(), _customCodeText.GetText(), customDecimalSeparator, customGroupSeparator, customDecimalDigits);
        _customSymbolRow.RemoveCssClass("error");
        _customSymbolRow.SetTitle(_("Currency Symbol"));
        _customCodeRow.RemoveCssClass("error");
        _customCodeRow.SetTitle(_("Currency Code"));
        _customDecimalSeparatorRow.RemoveCssClass("error");
        _customDecimalSeparatorRow.SetTitle(_("Decimal Separator"));
        _customGroupSeparatorRow.RemoveCssClass("error");
        _customGroupSeparatorRow.SetTitle(_("Group Separator"));
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
    /// Applies the dialog
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void Apply(object? sender, EventArgs e)
    {
        _controller.Password = _accountPasswordRow.GetText();
        _controller.Metadata.Name = _accountNameRow.GetText();
        _controller.Metadata.AccountType = (AccountType)_accountTypeRow.GetSelected();
        _controller.Metadata.DefaultTransactionType = _incomeButton.GetActive() ? TransactionType.Income : TransactionType.Expense;
        OnApply?.Invoke(this, EventArgs.Empty);
    }
}
