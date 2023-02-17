using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transaction
/// </summary>
public partial class TransactionDialog
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

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_year(ref MoneyDateTime datetime);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_month(ref MoneyDateTime datetime);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_day_of_month(ref MoneyDateTime datetime);

    [DllImport("libadwaita-1.so.0")]
    private static extern ref MoneyDateTime g_date_time_new_local(int year, int month, int day, int hour, int minute, double seconds);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [DllImport("libadwaita-1.so.0")]
    private static extern ref MoneyDateTime gtk_calendar_get_date(nint calendar);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_calendar_select_day(nint calendar, ref MoneyDateTime datetime);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_get_rgba(nint chooser, ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_set_rgba(nint chooser, ref Color rgba);

    private bool _constructing;
    private readonly TransactionDialogController _controller;
    private string? _receiptPath;
    private readonly Adw.MessageDialog _dialog;
    private readonly Gtk.Window _parentWindow;
    private readonly Gtk.Box _boxMain;
    private readonly Adw.PreferencesGroup _grpMain;
    private readonly Adw.EntryRow _rowDescription;
    private readonly Adw.EntryRow _rowAmount;
    private readonly Gtk.Label _lblCurrency;
    private readonly Adw.ActionRow _rowType;
    private readonly Gtk.Box _boxType;
    private readonly Gtk.ToggleButton _btnIncome;
    private readonly Gtk.ToggleButton _btnExpense;
    private readonly Adw.PreferencesGroup _grpDateRepeat;
    private readonly Adw.ActionRow _rowDate;
    private readonly Gtk.Popover _popoverDate;
    private readonly Gtk.Calendar _calendarDate;
    private readonly Gtk.MenuButton _btnDate;
    private readonly Adw.ComboRow _rowRepeatInterval;
    private readonly Adw.ActionRow _rowRepeatEndDate;
    private readonly Gtk.Popover _popeverRepeatEndDate;
    private readonly Gtk.Calendar _calendarRepeatEndDate;
    private readonly Gtk.MenuButton _btnRepeatEndDate;
    private readonly Gtk.Button _btnRepeatEndDateClear;
    private readonly Adw.PreferencesGroup _grpGroupColor;
    private readonly Adw.ComboRow _rowGroup;
    private readonly Adw.ActionRow _rowColor;
    private readonly Gtk.ColorButton _btnColor;
    private readonly Adw.PreferencesGroup _grpReceipt;
    private readonly Adw.ActionRow _rowReceipt;
    private readonly Gtk.Box _boxReceiptButtons;
    private readonly Gtk.Button _btnReceiptView;
    private readonly Adw.ButtonContent _btnReceiptViewContent;
    private readonly Gtk.Button _btnReceiptDelete;
    private readonly Gtk.Button _btnReceiptUpload;
    private readonly Adw.ButtonContent _btnReceiptUploadContent;
    private readonly Gtk.EventControllerKey _descriptionKeyController;
    private readonly Gtk.EventControllerKey _amountKeyController;

    /// <summary>
    /// Constructs a TransactionDialog
    /// </summary>
    /// <param name="controller">TransactionDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public TransactionDialog(TransactionDialogController controller, Gtk.Window parentWindow)
    {
        _constructing = true;
        _controller = controller;
        _receiptPath = null;
        _parentWindow = parentWindow;
        //Dialog Settings
        _dialog = Adw.MessageDialog.New(_parentWindow, $"{_controller.Localizer["Transaction"]} - {_controller.Transaction.Id}", "");
        _dialog.SetDefaultSize(420, -1);
        _dialog.SetHideOnClose(true);
        _dialog.SetModal(true);
        _dialog.AddResponse("cancel", _controller.Localizer["Cancel"]);
        _dialog.SetCloseResponse("cancel");
        if (_controller.CanCopy)
        {
            _dialog.AddResponse("copy", _controller.Localizer["MakeCopy"]);
        }
        _dialog.AddResponse("ok", _controller.Localizer["OK"]);
        _dialog.SetDefaultResponse("ok");
        _dialog.SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) =>
        {
            _controller.Accepted = e.Response != "cancel";
            _controller.CopyRequested = e.Response == "copy";
        };
        //Main Box
        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        //Main Preferences Group
        _grpMain = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpMain);
        //Description
        _rowDescription = Adw.EntryRow.New();
        _rowDescription.SetTitle(_controller.Localizer["Description", "Field"]);
        _rowDescription.SetInputHints(Gtk.InputHints.Spellcheck);
        _rowDescription.SetActivatesDefault(true);
        _rowDescription.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _descriptionKeyController = Gtk.EventControllerKey.New();
        _descriptionKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _descriptionKeyController.OnKeyPressed += (sender, e) => { if (e.Keyval == 59) { return true; } return false; };
        _rowDescription.AddController(_descriptionKeyController);
        _grpMain.Add(_rowDescription);
        //Amount
        _rowAmount = Adw.EntryRow.New();
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        _rowAmount.SetInputPurpose(Gtk.InputPurpose.Number);
        _rowAmount.SetActivatesDefault(true);
        _rowAmount.OnNotify += (sender, e) =>
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
        _rowAmount.AddController(_amountKeyController);
        _lblCurrency = Gtk.Label.New($"{_controller.CultureForNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForNumberString.NumberFormat.NaNSymbol})");
        _lblCurrency.AddCssClass("dim-label");
        _rowAmount.AddSuffix(_lblCurrency);
        _grpMain.Add(_rowAmount);
        //Type Box and Buttons
        _btnIncome = Gtk.ToggleButton.NewWithLabel(_controller.Localizer["Income"]);
        _btnIncome.OnToggled += OnTypeChanged;
        _btnExpense = Gtk.ToggleButton.NewWithLabel(_controller.Localizer["Expense"]);
        _btnExpense.OnToggled += OnTypeChanged;
        _btnIncome.BindProperty("active", _btnExpense, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        _boxType = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        _boxType.SetValign(Gtk.Align.Center);
        _boxType.AddCssClass("linked");
        _boxType.Append(_btnIncome);
        _boxType.Append(_btnExpense);
        //Type
        _rowType = Adw.ActionRow.New();
        _rowType.SetTitle(_controller.Localizer["TransactionType", "Field"]);
        _rowType.AddSuffix(_boxType);
        _grpMain.Add(_rowType);
        //Date and Repeat Interval Preferences Group
        _grpDateRepeat = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpDateRepeat);
        //Date
        _calendarDate = Gtk.Calendar.New();
        _calendarDate.SetName("calendarTransactions");
        _calendarDate.OnDaySelected += OnDateChanged;
        _calendarDate.OnNextMonth += OnDateChanged;
        _calendarDate.OnNextYear += OnDateChanged;
        _calendarDate.OnPrevMonth += OnDateChanged;
        _calendarDate.OnPrevYear += OnDateChanged;
        _popoverDate = Gtk.Popover.New();
        _popoverDate.SetChild(_calendarDate);
        _btnDate = Gtk.MenuButton.New();
        _btnDate.AddCssClass("flat");
        _btnDate.SetValign(Gtk.Align.Center);
        _btnDate.SetPopover(_popoverDate);
        _rowDate = Adw.ActionRow.New();
        _rowDate.SetTitle(_controller.Localizer["Date", "Field"]);
        _rowDate.AddSuffix(_btnDate);
        _rowDate.SetActivatableWidget(_btnDate);
        _grpDateRepeat.Add(_rowDate);
        //Repeat Interval
        _rowRepeatInterval = Adw.ComboRow.New();
        _rowRepeatInterval.SetTitle(_controller.Localizer["TransactionRepeatInterval", "Field"]);
        _rowRepeatInterval.SetModel(Gtk.StringList.New(new string[8] { _controller.Localizer["RepeatInterval", "Never"], _controller.Localizer["RepeatInterval", "Daily"], _controller.Localizer["RepeatInterval", "Weekly"], _controller.Localizer["RepeatInterval", "Biweekly"], _controller.Localizer["RepeatInterval", "Monthly"], _controller.Localizer["RepeatInterval", "Quarterly"], _controller.Localizer["RepeatInterval", "Yearly"], _controller.Localizer["RepeatInterval", "Biyearly"] }));
        _rowRepeatInterval.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                OnRepeatIntervalChanged();
            }
        };
        _grpDateRepeat.Add(_rowRepeatInterval);
        //Repeat End Date
        _calendarRepeatEndDate = Gtk.Calendar.New();
        _calendarRepeatEndDate.SetName("calendarTransactions");
        _calendarRepeatEndDate.OnDaySelected += OnRepeatEndDateChanged;
        _popeverRepeatEndDate = Gtk.Popover.New();
        _popeverRepeatEndDate.SetChild(_calendarRepeatEndDate);
        _btnRepeatEndDate = Gtk.MenuButton.New();
        _btnRepeatEndDate.AddCssClass("flat");
        _btnRepeatEndDate.SetValign(Gtk.Align.Center);
        _btnRepeatEndDate.SetPopover(_popeverRepeatEndDate);
        _btnRepeatEndDateClear = Gtk.Button.New();
        _btnRepeatEndDateClear.AddCssClass("flat");
        _btnRepeatEndDateClear.SetValign(Gtk.Align.Center);
        _btnRepeatEndDateClear.SetIconName("window-close-symbolic");
        _btnRepeatEndDateClear.SetTooltipText(_controller.Localizer["TransactionRepeatEndDate", "Clear"]);
        _btnRepeatEndDateClear.OnClicked += OnRepeatEndDateClear;
        _rowRepeatEndDate = Adw.ActionRow.New();
        _rowRepeatEndDate.SetTitle(_controller.Localizer["TransactionRepeatEndDate", "Field"]);
        _rowRepeatEndDate.AddSuffix(_btnRepeatEndDate);
        _rowRepeatEndDate.AddSuffix(_btnRepeatEndDateClear);
        _rowRepeatEndDate.SetActivatableWidget(_btnRepeatEndDate);
        _grpDateRepeat.Add(_rowRepeatEndDate);
        //Group and Color Preferences Group
        _grpGroupColor = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpGroupColor);
        //Group
        _rowGroup = Adw.ComboRow.New();
        _rowGroup.SetTitle(_controller.Localizer["Group", "Field"]);
        var groups = new List<string>();
        foreach (var pair in _controller.Groups.OrderBy(x => x.Value == _controller.Localizer["Ungrouped"] ? " " : x.Value))
        {
            groups.Add(pair.Value);
        }
        _rowGroup.SetModel(Gtk.StringList.New(groups.ToArray()));
        _rowGroup.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _grpGroupColor.Add(_rowGroup);
        //Color
        _btnColor = Gtk.ColorButton.New();
        _btnColor.SetValign(Gtk.Align.Center);
        _btnColor.OnColorSet += (sender, e) => Validate();
        _rowColor = Adw.ActionRow.New();
        _rowColor.SetTitle(_controller.Localizer["Color", "Field"]);
        _rowColor.AddSuffix(_btnColor);
        _rowColor.SetActivatableWidget(_btnColor);
        _grpGroupColor.Add(_rowColor);
        //Group Receipt
        _grpReceipt = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpReceipt);
        //Receipt
        _btnReceiptView = Gtk.Button.New();
        _btnReceiptView.SetValign(Gtk.Align.Center);
        _btnReceiptView.AddCssClass("flat");
        _btnReceiptView.SetTooltipText(_controller.Localizer["View"]);
        _btnReceiptViewContent = Adw.ButtonContent.New();
        _btnReceiptViewContent.SetIconName("image-x-generic-symbolic");
        _btnReceiptView.SetChild(_btnReceiptViewContent);
        _btnReceiptView.OnClicked += OnViewReceipt;
        _btnReceiptDelete = Gtk.Button.New();
        _btnReceiptDelete.SetValign(Gtk.Align.Center);
        _btnReceiptDelete.AddCssClass("flat");
        _btnReceiptDelete.SetIconName("user-trash-symbolic");
        _btnReceiptDelete.SetTooltipText(_controller.Localizer["Delete"]);
        _btnReceiptDelete.OnClicked += OnDeleteReceipt;
        _btnReceiptUpload = Gtk.Button.New();
        _btnReceiptUpload.SetValign(Gtk.Align.Center);
        _btnReceiptUpload.AddCssClass("flat");
        _btnReceiptUpload.SetTooltipText(_controller.Localizer["Upload"]);
        _btnReceiptUploadContent = Adw.ButtonContent.New();
        _btnReceiptUploadContent.SetIconName("document-send-symbolic");
        _btnReceiptUpload.SetChild(_btnReceiptUploadContent);
        _btnReceiptUpload.OnClicked += OnUploadReceipt;
        _boxReceiptButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _boxReceiptButtons.Append(_btnReceiptView);
        _boxReceiptButtons.Append(_btnReceiptDelete);
        _boxReceiptButtons.Append(_btnReceiptUpload);
        _rowReceipt = Adw.ActionRow.New();
        _rowReceipt.SetTitle(_controller.Localizer["Receipt", "Field"]);
        _rowReceipt.AddSuffix(_boxReceiptButtons);
        _grpReceipt.Add(_rowReceipt);
        //Layout
        _dialog.SetExtraChild(_boxMain);
        //Load Transaction
        gtk_calendar_select_day(_calendarDate.Handle, ref g_date_time_new_local(_controller.Transaction.Date.Year, _controller.Transaction.Date.Month, _controller.Transaction.Date.Day, 0, 0, 0.0));
        OnDateChanged(_calendarDate, EventArgs.Empty);
        _rowDescription.SetText(_controller.Transaction.Description);
        _rowAmount.SetText(_controller.Transaction.Amount.ToString("N2", _controller.CultureForNumberString));
        _btnIncome.SetActive(_controller.Transaction.Type == TransactionType.Income);
        _rowRepeatInterval.SetSelected(_controller.RepeatIntervalIndex);
        _rowRepeatEndDate.SetSensitive(_controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never);
        if (_controller.Transaction.RepeatEndDate != null)
        {
            gtk_calendar_select_day(_calendarRepeatEndDate.Handle, ref g_date_time_new_local(_controller.Transaction.RepeatEndDate.Value.Year, _controller.Transaction.RepeatEndDate.Value.Month, _controller.Transaction.RepeatEndDate.Value.Day, 0, 0, 0.0));
            OnRepeatEndDateChanged(_calendarRepeatEndDate, EventArgs.Empty);
        }
        else
        {
            _btnRepeatEndDate.SetLabel(_controller.Localizer["NoEndDate"]);
        }
        if (_controller.Transaction.GroupId == -1)
        {
            _rowGroup.SetSelected(0);
        }
        else
        {
            _rowGroup.SetSelected((uint)groups.IndexOf(_controller.Groups[(uint)_controller.Transaction.GroupId]));
        }
        var transactionColor = new Color();
        gdk_rgba_parse(ref transactionColor, _controller.Transaction.RGBA);
        gtk_color_chooser_set_rgba(_btnColor.Handle, ref transactionColor);
        _btnReceiptView.SetSensitive(_controller.Transaction.Receipt != null);
        _btnReceiptDelete.SetSensitive(_controller.Transaction.Receipt != null);
        if (_controller.Transaction.Receipt != null)
        {
            _btnReceiptViewContent.SetLabel(_controller.Localizer["View"]);
        }
        else
        {
            _btnReceiptUploadContent.SetLabel(_controller.Localizer["Upload"]);
        }
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
                row.SetText(row.GetText() + _controller.CultureForNumberString.NumberFormat.NumberDecimalSeparator);
                row.SetPosition(row.GetText().Length);
                return true;
            }
        }
        return false;
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
        var selectedDay = gtk_calendar_get_date(_calendarDate.Handle);
        var date = new DateOnly(g_date_time_get_year(ref selectedDay), g_date_time_get_month(ref selectedDay), g_date_time_get_day_of_month(ref selectedDay));
        var repeatEndDate = default(DateOnly?);
        if (_btnRepeatEndDate.GetLabel() != _controller.Localizer["NoEndDate"])
        {
            var selectedEndDay = gtk_calendar_get_date(_calendarRepeatEndDate.Handle);
            repeatEndDate = new DateOnly(g_date_time_get_year(ref selectedEndDay), g_date_time_get_month(ref selectedEndDay), g_date_time_get_day_of_month(ref selectedEndDay));
        }
        var groupObject = (Gtk.StringObject)_rowGroup.GetSelectedItem()!;
        var color = new Color();
        gtk_color_chooser_get_rgba(_btnColor.Handle, ref color);
        var checkStatus = _controller.UpdateTransaction(date, _rowDescription.GetText(), _btnIncome.GetActive() ? TransactionType.Income : TransactionType.Expense, (int)_rowRepeatInterval.GetSelected(), groupObject.GetString(), gdk_rgba_to_string(ref color), _rowAmount.GetText(), _receiptPath, repeatEndDate);
        _rowDescription.RemoveCssClass("error");
        _rowDescription.SetTitle(_controller.Localizer["Description", "Field"]);
        _rowAmount.RemoveCssClass("error");
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        _rowRepeatEndDate.RemoveCssClass("error");
        _rowRepeatEndDate.SetTitle(_controller.Localizer["TransactionRepeatEndDate", "Field"]);
        if (checkStatus == TransactionCheckStatus.Valid)
        {
            _dialog.SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus.HasFlag(TransactionCheckStatus.EmptyDescription))
            {
                _rowDescription.AddCssClass("error");
                _rowDescription.SetTitle(_controller.Localizer["Description", "Empty"]);
            }
            if (checkStatus.HasFlag(TransactionCheckStatus.InvalidAmount))
            {
                _rowAmount.AddCssClass("error");
                _rowAmount.SetTitle(_controller.Localizer["Amount", "Invalid"]);
            }
            if (checkStatus.HasFlag(TransactionCheckStatus.InvalidRepeatEndDate))
            {
                _rowRepeatEndDate.AddCssClass("error");
                _rowRepeatEndDate.SetTitle(_controller.Localizer["TransactionRepeatEndDate", "Invalid"]);
            }
            _dialog.SetResponseEnabled("ok", false);
        }
    }

    /// <summary>
    /// Occurs when either Income or Expense button is toggled
    /// </summary>
    /// <param name="sender">Gtk.ToggleButton</param>
    /// <param name="e">EventArgs</param>
    private void OnTypeChanged(Gtk.ToggleButton sender, EventArgs e)
    {
        if (_btnIncome.GetActive())
        {
            _btnIncome.AddCssClass("denaro-income");
            _btnExpense.RemoveCssClass("denaro-expense");
        }
        else
        {
            _btnIncome.RemoveCssClass("denaro-income");
            _btnExpense.AddCssClass("denaro-expense");
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
        _btnDate.SetLabel(date.ToString("d", _controller.CultureForDateString));
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
        var isRepeatIntervalNever = ((Gtk.StringObject)_rowRepeatInterval.SelectedItem!).String == _controller.Localizer["RepeatInterval", "Never"];
        _rowRepeatEndDate.SetSensitive(!isRepeatIntervalNever);
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
        _btnRepeatEndDate.SetLabel(date.ToString("d", _controller.CultureForDateString));
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
        _btnRepeatEndDate.SetLabel(_controller.Localizer["NoEndDate"]);
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
        _btnReceiptView.SetSensitive(false);
        _btnReceiptViewContent.SetLabel("");
        _btnReceiptDelete.SetSensitive(false);
        _btnReceiptUploadContent.SetLabel(_controller.Localizer["Upload"]);
        Validate();
    }

    /// <summary>
    /// Occurs when the upload receipt button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnUploadReceipt(Gtk.Button sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["Receipt", "Field"], _parentWindow, Gtk.FileChooserAction.Open, _controller.Localizer["Open"], _controller.Localizer["Cancel"]);
        openFileDialog.SetModal(true);
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName($"{_controller.Localizer["AllFiles"]} (*.jpg, *.jpeg, *.png, *.pdf)");
        filterAll.AddPattern("*.jpg");
        filterAll.AddPattern("*.jpeg");
        filterAll.AddPattern("*.png");
        filterAll.AddPattern("*.pdf");
        openFileDialog.AddFilter(filterAll);
        var filterJpeg = Gtk.FileFilter.New();
        filterJpeg.SetName("JPEG (*.jpg, *.jpeg)");
        filterJpeg.AddPattern("*.jpg");
        filterJpeg.AddPattern("*.jpeg");
        openFileDialog.AddFilter(filterJpeg);
        var filterPng = Gtk.FileFilter.New();
        filterPng.SetName("PNG (*.png)");
        filterPng.AddPattern("*.png");
        openFileDialog.AddFilter(filterPng);
        var filterPdf = Gtk.FileFilter.New();
        filterPdf.SetName("PDF (*.pdf)");
        filterPdf.AddPattern("*.pdf");
        openFileDialog.AddFilter(filterPdf);
        openFileDialog.OnResponse += (sender, e) =>
        {
            if (e.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var path = openFileDialog.GetFile()!.GetPath();
                _receiptPath = path;
                _btnReceiptView.SetSensitive(true);
                _btnReceiptDelete.SetSensitive(true);
                _btnReceiptViewContent.SetLabel(_controller.Localizer["View"]);
                _btnReceiptUploadContent.SetLabel("");
                Validate();
            }
        };
        openFileDialog.Show();
    }
}