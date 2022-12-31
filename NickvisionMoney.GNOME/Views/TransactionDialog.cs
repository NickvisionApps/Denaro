using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transaction
/// </summary>
public partial class TransactionDialog
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MoneyDateTime
    {
        UInt64 Usec;
        nint Tz;
        int Interval;
        Int32 Days;
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

    private delegate void SignalCallback(nint gObject, string response, nint data);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_add_response(nint dialog, string id, string label);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint adw_message_dialog_new(nint parent, string heading, string body);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_close_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_default_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_extra_child(nint dialog, nint child);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_response_appearance(nint dialog, string response, int appearance);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_year(ref MoneyDateTime datetime);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_month(ref MoneyDateTime datetime);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_day_of_month(ref MoneyDateTime datetime);

    [DllImport("adwaita-1")]
    private static extern ref MoneyDateTime g_date_time_new_local(int year, int month, int day, int hour, int minute, double seconds);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool may_block);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)] SignalCallback c_handler, nint data, nint destroy_data, int connect_flags);

    [DllImport("adwaita-1")]
    private static extern ref MoneyDateTime gtk_calendar_get_date(nint calendar);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_calendar_select_day(nint calendar, ref MoneyDateTime datetime);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_get_rgba(nint chooser, ref Color rgba);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_set_rgba(nint chooser, ref Color rgba);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gtk_widget_is_visible(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_show(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_destroy(nint window);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_default_size(nint window, int x, int y);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_hide_on_close(nint window, [MarshalAs(UnmanagedType.I1)] bool setting);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_modal(nint window, [MarshalAs(UnmanagedType.I1)] bool modal);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_chooser_get_file(nint chooser);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    private readonly TransactionDialogController _controller;
    private string? _receiptPath;
    private readonly nint _dialog;
    private readonly Gtk.Window _parentWindow;
    private readonly Gtk.Box _boxMain;
    private readonly Adw.PreferencesGroup _grpMain;
    private readonly Adw.ActionRow _rowDescription;
    private readonly Gtk.Entry _txtDescription;
    private readonly Adw.ActionRow _rowAmount;
    private readonly Gtk.Box _boxAmount;
    private readonly Gtk.Label _lblCurrency;
    private readonly Gtk.Entry _txtAmount;
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
    private readonly Adw.PreferencesGroup _grpGroupColor;
    private readonly Adw.ComboRow _rowGroup;
    private readonly Adw.ActionRow _rowColor;
    private readonly Gtk.ColorButton _btnColor;
    private readonly Adw.PreferencesGroup _grpReceipt;
    private readonly Adw.ActionRow _rowReceipt;
    private readonly Gtk.Box _btnReceiptButtons;
    private readonly Gtk.Button _btnReceiptView;
    private readonly Gtk.Button _btnReceiptDelete;
    private readonly Gtk.Button _btnReceiptUpload;

    /// <summary>
    /// Constructs a TransactionDialog
    /// </summary>
    /// <param name="controller">TransactionDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public TransactionDialog(TransactionDialogController controller, Gtk.Window parentWindow)
    {
        _controller = controller;
        _receiptPath = null;
        _parentWindow = parentWindow;
        //Dialog Settings
        _dialog = adw_message_dialog_new(_parentWindow.Handle, $"{_controller.Localizer["Transaction"]} - {_controller.Transaction.Id}", "");
        gtk_window_set_default_size(_dialog, 360, -1);
        gtk_window_set_hide_on_close(_dialog, true);
        adw_message_dialog_add_response(_dialog, "cancel", _controller.Localizer["Cancel"]);
        adw_message_dialog_set_close_response(_dialog, "cancel");
        adw_message_dialog_add_response(_dialog, "ok", _controller.Localizer["OK"]);
        adw_message_dialog_set_default_response(_dialog, "ok");
        adw_message_dialog_set_response_appearance(_dialog, "ok", 1); // ADW_RESPONSE_SUGGESTED
        g_signal_connect_data(_dialog, "response", (nint sender, string response, nint data) => _controller.Accepted = response == "ok", IntPtr.Zero, IntPtr.Zero, 0);
        //Main Box
        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        //Main Preferences Group
        _grpMain = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpMain);
        //Description
        _rowDescription = Adw.ActionRow.New();
        _rowDescription.SetTitle(_controller.Localizer["Description", "Field"]);
        _txtDescription = Gtk.Entry.New();
        _txtDescription.SetValign(Gtk.Align.Center);
        _txtDescription.SetPlaceholderText(_controller.Localizer["Description", "Placeholder"]);
        _txtDescription.SetActivatesDefault(true);
        _rowDescription.AddSuffix(_txtDescription);
        _grpMain.Add(_rowDescription);
        //Amount
        _rowAmount = Adw.ActionRow.New();
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        _boxAmount = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
        _boxAmount.SetValign(Gtk.Align.Center);
        _lblCurrency = Gtk.Label.New(NumberFormatInfo.CurrentInfo.CurrencySymbol);
        _boxAmount.Append(_lblCurrency);
        _txtAmount = Gtk.Entry.New();
        _txtAmount.SetPlaceholderText(_controller.Localizer["Amount", "Placeholder"]);
        _txtAmount.SetActivatesDefault(true);
        _txtAmount.SetInputPurpose(Gtk.InputPurpose.Number);
        _boxAmount.Append(_txtAmount);
        _rowAmount.AddSuffix(_boxAmount);
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
        _rowRepeatInterval.SetModel(Gtk.StringList.New(new string[7] { _controller.Localizer["RepeatInterval", "Never"], _controller.Localizer["RepeatInterval", "Daily"], _controller.Localizer["RepeatInterval", "Weekly"], _controller.Localizer["RepeatInterval", "Monthly"], _controller.Localizer["RepeatInterval", "Quarterly"], _controller.Localizer["RepeatInterval", "Yearly"], _controller.Localizer["RepeatInterval", "Biyearly"] }));
        _grpDateRepeat.Add(_rowRepeatInterval);
        //Group and Color Preferences Group
        _grpGroupColor = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpGroupColor);
        //Group
        _rowGroup = Adw.ComboRow.New();
        _rowGroup.SetTitle(_controller.Localizer["Group", "Field"]);
        var groups = new List<string> { _controller.Localizer["Ungrouped"] };
        foreach(var group in _controller.Groups)
        {
            groups.Add(group.Value);
        }
        _rowGroup.SetModel(Gtk.StringList.New(groups.ToArray()));
        _grpGroupColor.Add(_rowGroup);
        //Color
        _btnColor = Gtk.ColorButton.New();
        _btnColor.SetValign(Gtk.Align.Center);
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
        _btnReceiptView.SetIconName("image-x-generic-symbolic");
        _btnReceiptView.SetTooltipText(_controller.Localizer["View"]);
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
        _btnReceiptUpload.SetIconName("document-send-symbolic");
        _btnReceiptUpload.SetTooltipText(_controller.Localizer["Upload"]);
        _btnReceiptUpload.OnClicked += OnUploadReceipt;
        _btnReceiptButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _btnReceiptButtons.Append(_btnReceiptView);
        _btnReceiptButtons.Append(_btnReceiptDelete);
        _btnReceiptButtons.Append(_btnReceiptUpload);
        _rowReceipt = Adw.ActionRow.New();
        _rowReceipt.SetTitle(_controller.Localizer["Receipt", "Field"]);
        _rowReceipt.AddSuffix(_btnReceiptButtons);
        _grpReceipt.Add(_rowReceipt);
        //Layout
        adw_message_dialog_set_extra_child(_dialog, _boxMain.Handle);
        //Load Transaction
        gtk_calendar_select_day(_calendarDate.Handle, ref g_date_time_new_local(_controller.Transaction.Date.Year, _controller.Transaction.Date.Month, _controller.Transaction.Date.Day, 0, 0, 0.0));
        OnDateChanged(_calendarDate, EventArgs.Empty);
        _txtDescription.SetText(_controller.Transaction.Description);
        _txtAmount.SetText(_controller.Transaction.Amount.ToString("N2"));
        if (_controller.Transaction.Type == TransactionType.Income)
        {
            _btnIncome.SetActive(true);
        }
        else
        {
            _btnExpense.SetActive(true);
        }
        _rowRepeatInterval.SetSelected((uint)_controller.Transaction.RepeatInterval);
        if(_controller.Transaction.GroupId == -1)
        {
            _rowGroup.SetSelected(0);
        }
        else
        {
            _rowGroup.SetSelected((uint)_controller.Transaction.GroupId);
        }
        var transactionColor = new Color();
        if(!gdk_rgba_parse(ref transactionColor, _controller.Transaction.RGBA))
        {
            gdk_rgba_parse(ref transactionColor, _controller.TransactionDefaultColor);
        }
        gtk_color_chooser_set_rgba(_btnColor.Handle, ref transactionColor);
        _btnReceiptView.SetSensitive(_controller.Transaction.Receipt != null);
        _btnReceiptDelete.SetSensitive(_controller.Transaction.Receipt != null);
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public bool Run()
    {
        gtk_widget_show(_dialog);
        gtk_window_set_modal(_dialog, true);
        while(gtk_widget_is_visible(_dialog))
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        if(_controller.Accepted)
        {
            gtk_window_set_modal(_dialog, false);
            var selectedDay = gtk_calendar_get_date(_calendarDate.Handle);
            var date = new DateOnly(g_date_time_get_year(ref selectedDay), g_date_time_get_month(ref selectedDay), g_date_time_get_day_of_month(ref selectedDay));
            var groupObject = (Gtk.StringObject)_rowGroup.GetSelectedItem();
            var color = new Color();
            gtk_color_chooser_get_rgba(_btnColor.Handle, ref color);
            var status = _controller.UpdateTransaction(date, _txtDescription.GetText(), _btnIncome.GetActive() ? TransactionType.Income : TransactionType.Expense, (TransactionRepeatInterval)_rowRepeatInterval.GetSelected(), groupObject.GetString(), gdk_rgba_to_string(ref color), _txtAmount.GetText(), _receiptPath);
            if(status != TransactionCheckStatus.Valid)
            {
                //Reset UI
                _rowDescription.RemoveCssClass("error");
                _rowDescription.SetTitle(_controller.Localizer["Description", "Field"]);
                _rowAmount.RemoveCssClass("error");
                _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
                //Mark Error
                if(status == TransactionCheckStatus.EmptyDescription)
                {
                    _rowDescription.AddCssClass("error");
                    _rowDescription.SetTitle(_controller.Localizer["Description", "Empty"]);
                }
                else if(status == TransactionCheckStatus.InvalidAmount)
                {
                    _rowAmount.AddCssClass("error");
                    _rowAmount.SetTitle(_controller.Localizer["Amount", "Invalid"]);
                }
                return Run();
            }
        }
        gtk_window_destroy(_dialog);
        return _controller.Accepted;
    }

    /// <summary>
    /// Occurs when either Income or Expense button is toggled
    /// </summary>
    /// <param name="sender">Gtk.ToggleButton</param>
    /// <param name="e">EventArgs</param>
    private void OnTypeChanged(Gtk.ToggleButton sender, EventArgs e)
    {
        if(_btnIncome.GetActive())
        {
            _btnIncome.AddCssClass("success");
            _btnIncome.AddCssClass("money-income");
            _btnExpense.RemoveCssClass("error");
            _btnExpense.RemoveCssClass("money-expense");
        }
        else
        {

            _btnIncome.RemoveCssClass("success");
            _btnIncome.RemoveCssClass("money-income");
            _btnExpense.AddCssClass("error");
            _btnExpense.AddCssClass("money-expense");
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
        _btnDate.SetLabel(date.ToString("d"));
    }

    private void OnViewReceipt(Gtk.Button sender, EventArgs e) => _controller.OpenReceiptImage(_receiptPath);

    private void OnDeleteReceipt(Gtk.Button sender, EventArgs e)
    {
        _receiptPath = "";
        _btnReceiptView.SetSensitive(false);
        _btnReceiptDelete.SetSensitive(false);
    }

    private void OnUploadReceipt(Gtk.Button sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["Receipt", "Field"], _parentWindow, Gtk.FileChooserAction.Open, _controller.Localizer["Open"], _controller.Localizer["Cancel"]);
        openFileDialog.SetModal(true);
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName($"{_controller.Localizer["AllFiles"]} (*.jpg, *.jpeg, *.pdf)");
        filterAll.AddPattern("*.jpg");
        filterAll.AddPattern("*.jpeg");
        filterAll.AddPattern("*.pdf");
        openFileDialog.AddFilter(filterAll);
        var filterJpg = Gtk.FileFilter.New();
        filterJpg.SetName("JPG (*.jpg)");
        filterJpg.AddPattern("*.jpg");
        openFileDialog.AddFilter(filterJpg);
        var filterJpeg = Gtk.FileFilter.New();
        filterJpeg.SetName("JPEG (*.jpeg)");
        filterJpeg.AddPattern("*.jpeg");
        openFileDialog.AddFilter(filterJpeg);
        var filterPdf = Gtk.FileFilter.New();
        filterPdf.SetName("PDF (*.pdf)");
        filterPdf.AddPattern("*.pdf");
        openFileDialog.AddFilter(filterPdf);
        openFileDialog.OnResponse += async (sender, e) =>
        {
            if (e.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var path = g_file_get_path(gtk_file_chooser_get_file(openFileDialog.Handle));
                _receiptPath = path;
                _btnReceiptView.SetSensitive(true);
            }
        };
        openFileDialog.Show();
    }
}