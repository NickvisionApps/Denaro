using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transaction
/// </summary>
public partial class TransactionDialog : Adw.Window
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MoneyDateTime
    {
        ulong Usec;
        nint Tz;
        int Interval;
        int Days;
        int RefCount;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextIter {
        public nint dummy1;
        public nint dummy2;
        public int dummy3;
        public int dummy4;
        public int dummy5;
        public int dummy6;
        public int dummy7;
        public int dummy8;
        public nint dummy9;
        public nint dummy10;
        public int dummy11;
        public int dummy12;
        public int dummy13;
        public nint dummy14;
    }

    private delegate void GAsyncReadyCallback(nint source, nint res, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_button_set_rgba(nint button, ref Color rgba);
    [DllImport("libadwaita-1.so.0")]
    private static extern ref Color gtk_color_dialog_button_get_rgba(nint button);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_button_set_dialog(nint button, nint dialog);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_color_dialog_new();
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_set_with_alpha(nint dialog, [MarshalAs(UnmanagedType.I1)] bool with_alpha);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_year(ref MoneyDateTime datetime);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_month(ref MoneyDateTime datetime);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_day_of_month(ref MoneyDateTime datetime);
    [DllImport("libadwaita-1.so.0")]
    private static extern ref MoneyDateTime g_date_time_new_local(int year, int month, int day, int hour, int minute, double seconds);
    [DllImport("libadwaita-1.so.0")]
    private static extern ref MoneyDateTime gtk_calendar_get_date(nint calendar);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_calendar_select_day(nint calendar, ref MoneyDateTime datetime);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_new();
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_title(nint dialog, string title);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_filters(nint dialog, nint filters);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_open(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_open_finish(nint dialog, nint result, nint error);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_text_buffer_get_bounds(nint buffer, ref TextIter startIter, ref TextIter endIter);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gtk_text_buffer_get_text(nint buffer, ref TextIter startIter, ref TextIter endIter, [MarshalAs(UnmanagedType.I1)]bool include_hidden_chars);

    private bool _constructing;
    private readonly TransactionDialogController _controller;
    private string? _receiptPath;
    private nint _colorDialog;
    private GAsyncReadyCallback _openCallback;
    private AutocompleteDialog _autocompleteDialog;

    [Gtk.Connect] private readonly Adw.ViewStack _stack;
    [Gtk.Connect] private readonly Gtk.Button _backButton;
    [Gtk.Connect] private readonly Gtk.Button _copyButton;
    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _scrolledWindow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Adw.EntryRow _amountRow;
    [Gtk.Connect] private readonly Gtk.Label _currencyLabel;
    [Gtk.Connect] private readonly Gtk.ToggleButton _incomeButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _expenseButton;
    [Gtk.Connect] private readonly Gtk.Calendar _dateCalendar;
    [Gtk.Connect] private readonly Gtk.MenuButton _dateCalendarButton;
    [Gtk.Connect] private readonly Gtk.Label _dateDashLabel;
    [Gtk.Connect] private readonly Adw.ComboRow _repeatIntervalRow;
    [Gtk.Connect] private readonly Gtk.Calendar _repeatEndDateCalendar;
    [Gtk.Connect] private readonly Gtk.MenuButton _repeatEndDateCalendarButton;
    [Gtk.Connect] private readonly Gtk.Button _repeatEndDateClearButton;
    [Gtk.Connect] private readonly Adw.ComboRow _groupRow;
    [Gtk.Connect] private readonly Gtk.DropDown _colorDropDown;
    [Gtk.Connect] private readonly Gtk.Widget _colorButton;
    [Gtk.Connect] private readonly Adw.ActionRow _extrasRow;
    [Gtk.Connect] private readonly Adw.ActionRow _receiptRow;
    [Gtk.Connect] private readonly Gtk.Button _viewReceiptButton;
    [Gtk.Connect] private readonly Adw.ButtonContent _viewReceiptButtonContent;
    [Gtk.Connect] private readonly Gtk.Button _deleteReceiptButton;
    [Gtk.Connect] private readonly Gtk.Button _uploadReceiptButton;
    [Gtk.Connect] private readonly Adw.ButtonContent _uploadReceiptButtonContent;
    [Gtk.Connect] private readonly Gtk.TextView _notesView;
    [Gtk.Connect] private readonly Gtk.Button _deleteButton;
    [Gtk.Connect] private readonly Gtk.Button _applyButton;

    private readonly Gtk.EventControllerKey _descriptionKeyController;
    private readonly Gtk.EventControllerKey _amountKeyController;
    private readonly Gtk.ShortcutController _shortcutController;

    public event EventHandler? OnApply;
    public event EventHandler? OnDelete;

    private TransactionDialog(Gtk.Builder builder, TransactionDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _constructing = true;
        _controller = controller;
        _receiptPath = null;
        _autocompleteDialog = new AutocompleteDialog(this);
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        //Build UI
        builder.Connect(this);
        OnNotify += (sender, e) =>
        {
            if(e.Pspec.GetName() == "is-active")
            {
                if(GetIsActive())
                {
                    _autocompleteDialog.Hide();
                }
            }
        };
        OnCloseRequest += (sender, e) =>
        {
            _autocompleteDialog.Close();
            return false;
        };
        _scrolledWindow.GetVadjustment().OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "page-size")
            {
                if (_scrolledWindow.GetVadjustment().GetPageSize() < _scrolledWindow.GetVadjustment().GetUpper())
                {
                    _scrolledWindow.AddCssClass("scrolled-dialog");
                }
                else
                {
                    _scrolledWindow.RemoveCssClass("scrolled-dialog");
                }
            }
        };
        var idString = _controller.Transaction.Id.ToString();
        var nativeDigits = CultureInfo.CurrentCulture.NumberFormat.NativeDigits;
        if(_controller.UseNativeDigits && "0" != nativeDigits[0])
        {
            idString = idString.Replace("0", nativeDigits[0])
                               .Replace("1", nativeDigits[1])
                               .Replace("2", nativeDigits[2])
                               .Replace("3", nativeDigits[3])
                               .Replace("4", nativeDigits[4])
                               .Replace("5", nativeDigits[5])
                               .Replace("6", nativeDigits[6])
                               .Replace("7", nativeDigits[7])
                               .Replace("8", nativeDigits[8])
                               .Replace("9", nativeDigits[9]);
        }
        _titleLabel.SetLabel($"{_("Transaction")} — {idString}");
        _deleteButton.SetVisible(_controller.IsEditing);
        _deleteButton.OnClicked += (sender, e) => OnDelete?.Invoke(this, EventArgs.Empty);
        _copyButton.SetVisible(_controller.CanCopy);
        _copyButton.OnClicked += (sender, e) =>
        {
            _controller.CopyRequested = true;
            OnApply?.Invoke(this, EventArgs.Empty);
        };
        _applyButton.SetLabel(_controller.IsEditing ? _("Apply") : _("Add"));
        _applyButton.OnClicked += (sender, e) => OnApply?.Invoke(this, EventArgs.Empty);
        _backButton.OnClicked += (sender, e) =>
        {
            _stack.SetVisibleChildName("main");
            _backButton.SetVisible(false);
            _copyButton.SetVisible(true);
            SetDefaultWidget(_applyButton);
        };
        _extrasRow.OnActivated += (sender, e) =>
        {
            _stack.SetVisibleChildName("extras");
            _backButton.SetVisible(true);
            _copyButton.SetVisible(false);
        };
        //Description
        _descriptionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    AutocompleteDescription();
                    Validate();
                }
            }
        };
        _descriptionKeyController = Gtk.EventControllerKey.New();
        _descriptionKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _descriptionKeyController.OnKeyPressed += (sender, e) => { if (e.Keyval == 59) { return true; } return false; };
        _descriptionRow.AddController(_descriptionKeyController);
        //Amount
        _amountRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _amountKeyController = Gtk.EventControllerKey.New();
        _amountKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _amountKeyController.OnKeyPressed += OnKeyPressed;
        _amountRow.AddController(_amountKeyController);
        _currencyLabel.SetLabel($"{_controller.CultureForNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForNumberString.NumberFormat.NaNSymbol})");
        //Type Box and Buttons
        _incomeButton.OnToggled += OnTypeChanged;
        _expenseButton.OnToggled += OnTypeChanged;
        _expenseButton.BindProperty("active", _incomeButton, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        //Date
        _dateCalendar.SetName("calendarTransactions");
        _dateCalendar.OnDaySelected += OnDateChanged;
        _dateCalendar.OnNextMonth += OnDateChanged;
        _dateCalendar.OnNextYear += OnDateChanged;
        _dateCalendar.OnPrevMonth += OnDateChanged;
        _dateCalendar.OnPrevYear += OnDateChanged;
        //Repeat Interval
        _repeatIntervalRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                OnRepeatIntervalChanged();
            }
        };
        //Repeat End Date
        _repeatEndDateCalendar.OnDaySelected += OnRepeatEndDateChanged;
        _repeatEndDateClearButton.OnClicked += OnRepeatEndDateClear;
        //Group
        _groupRow.SetModel(Gtk.StringList.New(_controller.GroupNames.ToArray()));
        _groupRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                if (_groupRow.GetSelected() == 0)
                {
                    _colorDropDown.SetSelected(1);
                }
                _colorDropDown.SetVisible(_groupRow.GetSelected() != 0);
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        //Color
        ((Gtk.Box)_colorButton.GetParent()).SetSpacing(4);
        _colorDropDown.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                if (!_constructing)
                {
                    _colorButton.SetVisible(_colorDropDown.GetSelected() == 1);
                    Validate();
                }
            }
        };
        _colorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_colorDialog, false);
        gtk_color_dialog_button_set_dialog(_colorButton.Handle, _colorDialog);
        _colorButton.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "rgba")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        //Receipt
        _viewReceiptButton.OnClicked += OnViewReceipt;
        _deleteReceiptButton.OnClicked += OnDeleteReceipt;
        _uploadReceiptButton.OnClicked += OnUploadReceipt;
        //Notes
        _notesView.GetBuffer().OnChanged += (sender, e) =>
        {
            if (!_constructing)
            {
                Validate();
            }
        };
        //Shortcut Controller
        _shortcutController = Gtk.ShortcutController.New();
        _shortcutController.SetScope(Gtk.ShortcutScope.Managed);
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("Escape"), Gtk.CallbackAction.New((sender, e) =>
        {
            Close();
            return true;
        })));
        AddController(_shortcutController);
        //Load Transaction
        gtk_calendar_select_day(_dateCalendar.Handle, ref g_date_time_new_local(_controller.Transaction.Date.Year, _controller.Transaction.Date.Month, _controller.Transaction.Date.Day, 0, 0, 0.0));
        OnDateChanged(_dateCalendar, EventArgs.Empty);
        _descriptionRow.SetText(_controller.Transaction.Description);
        _amountRow.SetText(_controller.Transaction.Amount.ToAmountString(_controller.CultureForNumberString, _controller.UseNativeDigits, false));
        _incomeButton.SetActive(_controller.Transaction.Type == TransactionType.Income);
        _repeatIntervalRow.SetSelected(_controller.RepeatIntervalIndex);
        _dateDashLabel.SetVisible(_controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never);
        _repeatEndDateCalendarButton.SetVisible(_controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never);
        if (_controller.Transaction.RepeatEndDate != null)
        {
            gtk_calendar_select_day(_repeatEndDateCalendar.Handle, ref g_date_time_new_local(_controller.Transaction.RepeatEndDate.Value.Year, _controller.Transaction.RepeatEndDate.Value.Month, _controller.Transaction.RepeatEndDate.Value.Day, 0, 0, 0.0));
            OnRepeatEndDateChanged(_repeatEndDateCalendar, EventArgs.Empty);
        }
        else
        {
            _repeatEndDateCalendarButton.SetLabel(_("No End Date"));
        }
        if (_controller.Transaction.GroupId == -1)
        {
            _groupRow.SetSelected(0);
        }
        else
        {
            _groupRow.SetSelected((uint)_controller.GroupNames.IndexOf(_controller.GetGroupNameFromId((uint)_controller.Transaction.GroupId)));
        }
        _colorDropDown.SetSelected((_controller.Transaction.UseGroupColor && _groupRow.GetSelected() != 0) ? 0u : 1u);
        _colorDropDown.SetVisible(_groupRow.GetSelected() != 0);
        _colorButton.SetVisible(_colorDropDown.GetSelected() == 1);
        var transactionColor = new Color();
        gdk_rgba_parse(ref transactionColor, _controller.Transaction.RGBA);
        gtk_color_dialog_button_set_rgba(_colorButton.Handle, ref transactionColor);
        _viewReceiptButton.SetSensitive(_controller.Transaction.Receipt != null);
        _deleteReceiptButton.SetSensitive(_controller.Transaction.Receipt != null);
        if (_controller.Transaction.Receipt != null)
        {
            _viewReceiptButtonContent.SetLabel(_("View"));
        }
        else
        {
            _uploadReceiptButtonContent.SetLabel(_("Upload"));
        }
        _notesView.GetBuffer().SetText(_controller.Transaction.Notes, _controller.Transaction.Notes.Length);
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Constructs a TransactionDialog
    /// </summary>
    /// <param name="controller">TransactionDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public TransactionDialog(TransactionDialogController controller, Gtk.Window parent) : this(Builder.FromFile("transaction_dialog.ui"), controller, parent)
    {
    }

    /// <summary>
    /// Callback for key-pressed signal
    /// </summary>
    /// <param name="sender">Gtk.EventControllerKey</param>
    /// <param name="e">Gtk.EventControllerKey.KeyPressedSignalArgs</param>
    private bool OnKeyPressed(Gtk.EventControllerKey sender, Gtk.EventControllerKey.KeyPressedSignalArgs e)
    {
        if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (e.Keyval == 65454 || e.Keyval == 65452 || e.Keyval == 2749 || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (e.Keyval == 44 || e.Keyval == 46)))
            {
                var row = (Adw.EntryRow)(sender.GetWidget());
                if (!row.GetText().Contains(_controller.CultureForNumberString.NumberFormat.CurrencyDecimalSeparator))
                {
                    var position = row.GetPosition();
                    row.SetText(row.GetText().Insert(position, _controller.CultureForNumberString.NumberFormat.CurrencyDecimalSeparator));
                    row.SetPosition(position + Math.Min(_controller.CultureForNumberString.NumberFormat.CurrencyDecimalSeparator.Length, 2));
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var selectedDay = gtk_calendar_get_date(_dateCalendar.Handle);
        var date = new DateOnly(g_date_time_get_year(ref selectedDay), g_date_time_get_month(ref selectedDay), g_date_time_get_day_of_month(ref selectedDay));
        var repeatEndDate = default(DateOnly?);
        if (_repeatEndDateCalendarButton.GetLabel() != _("No End Date"))
        {
            var selectedEndDay = gtk_calendar_get_date(_repeatEndDateCalendar.Handle);
            repeatEndDate = new DateOnly(g_date_time_get_year(ref selectedEndDay), g_date_time_get_month(ref selectedEndDay), g_date_time_get_day_of_month(ref selectedEndDay));
        }
        var groupObject = (Gtk.StringObject)_groupRow.GetSelectedItem()!;
        var color = gtk_color_dialog_button_get_rgba(_colorButton.Handle);
        var iterStart = new TextIter();
        var iterEnd = new TextIter();
        gtk_text_buffer_get_bounds(_notesView.GetBuffer().Handle, ref iterStart, ref iterEnd);
        var checkStatus = _controller.UpdateTransaction(date, _descriptionRow.GetText(), _incomeButton.GetActive() ? TransactionType.Income : TransactionType.Expense, (int)_repeatIntervalRow.GetSelected(), groupObject.GetString(), gdk_rgba_to_string(ref color), _colorDropDown.GetSelected() == 0, _amountRow.GetText(), _receiptPath, repeatEndDate, gtk_text_buffer_get_text(_notesView.GetBuffer().Handle, ref iterStart, ref iterEnd, false));
        _descriptionRow.RemoveCssClass("error");
        _descriptionRow.SetTitle(_("Description"));
        _amountRow.RemoveCssClass("error");
        _amountRow.SetTitle(_("Amount"));
        _repeatEndDateCalendarButton.RemoveCssClass("error");
        _repeatEndDateCalendarButton.SetTooltipText(_("Repeat End Date"));
        _receiptRow.RemoveCssClass("error");
        _receiptRow.SetTitle(_("Receipt"));
        if (checkStatus == TransactionCheckStatus.Valid)
        {
            _applyButton.SetSensitive(true);
        }
        else
        {
            if (checkStatus.HasFlag(TransactionCheckStatus.EmptyDescription))
            {
                _descriptionRow.AddCssClass("error");
                _descriptionRow.SetTitle(_("Description (Empty)"));
            }
            if (checkStatus.HasFlag(TransactionCheckStatus.InvalidAmount))
            {
                _amountRow.AddCssClass("error");
                _amountRow.SetTitle(_("Amount (Invalid)"));
            }
            if (checkStatus.HasFlag(TransactionCheckStatus.InvalidRepeatEndDate))
            {
                _repeatEndDateCalendarButton.AddCssClass("error");
                _repeatEndDateCalendarButton.SetTooltipText(_("Repeat End Date (Invalid)"));
            }
            _applyButton.SetSensitive(false);
            if (checkStatus.HasFlag(TransactionCheckStatus.CannotAccessReceipt))
            {
                _receiptRow.AddCssClass("error");
                _receiptRow.SetTitle(_("Receipt (File Inaccessible)"));
                _applyButton.SetSensitive(true);
            }
        }
    }

    /// <summary>
    /// Sets up autocomplete for description
    /// </summary>
    private void AutocompleteDescription()
    {
        if(!string.IsNullOrEmpty(_descriptionRow.GetText()))
        {
            var matchingDescriptions = _controller.GetDescriptionSuggestions(_descriptionRow.GetText());
            if(matchingDescriptions.Count != 0 && matchingDescriptions[0] != _descriptionRow.GetText())
            {
                _autocompleteDialog.UpdateSuggestions(matchingDescriptions);
                _autocompleteDialog.SuggestionClicked += (sender, e) =>
                {
                    _descriptionRow.SetText(e);
                    _descriptionRow.SetPosition(-1);
                };
                _autocompleteDialog.Present();
                return;
            }
        }
        _autocompleteDialog.Hide();
    }

    /// <summary>
    /// Occurs when either Income or Expense button is toggled
    /// </summary>
    /// <param name="sender">Gtk.ToggleButton</param>
    /// <param name="e">EventArgs</param>
    private void OnTypeChanged(Gtk.ToggleButton sender, EventArgs e)
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
    /// Occurs when the date in the calendar is changed
    /// </summary>
    /// <param name="sender">Gtk.Calendar</param>
    /// <param name="e">EventArgs</param>
    private void OnDateChanged(Gtk.Calendar sender, EventArgs e)
    {
        var selectedDay = gtk_calendar_get_date(sender.Handle);
        var date = new DateOnly(g_date_time_get_year(ref selectedDay), g_date_time_get_month(ref selectedDay), g_date_time_get_day_of_month(ref selectedDay));
        _dateCalendarButton.SetLabel(date.ToString("d", _controller.CultureForDateString));
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the repeat interval is changed
    /// </summary>
    private void OnRepeatIntervalChanged()
    {
        var isRepeatIntervalNever = ((Gtk.StringObject)_repeatIntervalRow.SelectedItem!).String == _("Never");
        _dateDashLabel.SetVisible(!isRepeatIntervalNever);
        _repeatEndDateCalendarButton.SetVisible(!isRepeatIntervalNever);
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the repeat end date in the calendar is changed
    /// </summary>
    /// <param name="sender">Gtk.Calendar</param>
    /// <param name="e">EventArgs</param>
    private void OnRepeatEndDateChanged(Gtk.Calendar sender, EventArgs e)
    {
        var selectedDay = gtk_calendar_get_date(sender.Handle);
        var date = new DateOnly(g_date_time_get_year(ref selectedDay), g_date_time_get_month(ref selectedDay), g_date_time_get_day_of_month(ref selectedDay));
        _repeatEndDateCalendarButton.SetLabel(date.ToString("d", _controller.CultureForDateString));
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the clear repeat end date in clicked
    /// </summary>
    /// <param name="sender">Gtk.Calendar</param>
    /// <param name="e">EventArgs</param>
    private void OnRepeatEndDateClear(Gtk.Button sender, EventArgs e)
    {
        _repeatEndDateCalendarButton.SetLabel(_("No End Date"));
        _repeatEndDateCalendarButton.GetPopover().Popdown();
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the view receipt button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private async void OnViewReceipt(Gtk.Button sender, EventArgs e) => await _controller.OpenReceiptImageAsync(_receiptPath);

    /// <summary>
    /// Occurs when the delete receipt button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnDeleteReceipt(Gtk.Button sender, EventArgs e)
    {
        _receiptPath = "";
        _viewReceiptButton.SetSensitive(false);
        _viewReceiptButtonContent.SetLabel("");
        _deleteReceiptButton.SetSensitive(false);
        _uploadReceiptButtonContent.SetLabel(_("Upload"));
        Validate();
    }

    /// <summary>
    /// Occurs when the upload receipt button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnUploadReceipt(Gtk.Button sender, EventArgs e)
    {
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName($"{_("All files")} (*.jpg, *.jpeg, *.png, *.pdf)");
        filterAll.AddPattern("*.jpg");
        filterAll.AddPattern("*.jpeg");
        filterAll.AddPattern("*.png");
        filterAll.AddPattern("*.pdf");
        var filterJpeg = Gtk.FileFilter.New();
        filterJpeg.SetName("JPEG (*.jpg, *.jpeg)");
        filterJpeg.AddPattern("*.jpg");
        filterJpeg.AddPattern("*.jpeg");
        var filterPng = Gtk.FileFilter.New();
        filterPng.SetName("PNG (*.png)");
        filterPng.AddPattern("*.png");
        var filterPdf = Gtk.FileFilter.New();
        filterPdf.SetName("PDF (*.pdf)");
        filterPdf.AddPattern("*.pdf");
        var openFileDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(openFileDialog, _("Receipt"));
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterAll);
        filters.Append(filterJpeg);
        filters.Append(filterPng);
        filters.Append(filterPdf);
        gtk_file_dialog_set_filters(openFileDialog, filters.Handle);
        _openCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_open_finish(openFileDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
                _receiptPath = path;
                _viewReceiptButton.SetSensitive(true);
                _deleteReceiptButton.SetSensitive(true);
                _viewReceiptButtonContent.SetLabel(_("View"));
                _uploadReceiptButtonContent.SetLabel("");
                Validate();
            }
        };
        gtk_file_dialog_open(openFileDialog, Handle, IntPtr.Zero, _openCallback, IntPtr.Zero);
    }
}