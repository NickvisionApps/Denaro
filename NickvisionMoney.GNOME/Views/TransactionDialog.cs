using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Adw.Internal;
using static Nickvision.Aura.Localization.Gettext;
using DateTime = GLib.DateTime;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transaction
/// </summary>
public partial class TransactionDialog : Adw.Window
{
    private bool _constructing;
    private readonly TransactionDialogController _controller;
    private Image? _receipt;
    private Gtk.ColorDialog _colorDialog;
    private AutocompleteBox<Transaction> _autocompleteBox;
    private bool _canHideAutobox;
    private Dictionary<string, bool> _tags;

    [Gtk.Connect] private readonly Adw.ViewStack _stack;
    [Gtk.Connect] private readonly Gtk.Button _backButton;
    [Gtk.Connect] private readonly Gtk.Button _copyButton;
    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _scrolledWindow;
    [Gtk.Connect] private readonly Gtk.Overlay _overlay;
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
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _colorButton;
    [Gtk.Connect] private readonly Gtk.MenuButton _tagsButton;
    [Gtk.Connect] private readonly Gtk.Entry _addTagEntry;
    [Gtk.Connect] private readonly Gtk.Button _addTagButton;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _tagsScrolledWindow;
    [Gtk.Connect] private readonly Gtk.FlowBox _tagsFlowBox;
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

    /// <summary>
    /// Occurs when the apply button is clicked
    /// </summary>
    public event EventHandler<EventArgs>? OnApply;
    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    public event EventHandler<EventArgs>? OnDelete;

    private TransactionDialog(Gtk.Builder builder, TransactionDialogController controller, Gtk.Window parent) : base(builder.GetObject("_root").Handle as WindowHandle)
    {
        _constructing = true;
        _controller = controller;
        _canHideAutobox = true;
        _tags = new Dictionary<string, bool>();
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        //Build UI
        builder.Connect(this);
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
        if (_controller.UseNativeDigits && "0" != nativeDigits[0])
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
        _deleteButton.OnClicked += (sender, e) =>
        {
            Close();
            OnDelete?.Invoke(this, EventArgs.Empty);
        };
        _copyButton.SetVisible(_controller.CanCopy);
        _copyButton.OnClicked += (sender, e) =>
        {
            _controller.CopyRequested = true;
            Close();
            OnApply?.Invoke(this, EventArgs.Empty);
        };
        _applyButton.SetLabel(_controller.IsEditing ? _("Apply") : _("Add"));
        _applyButton.OnClicked += (sender, e) =>
        {
            Close();
            OnApply?.Invoke(this, EventArgs.Empty);
        };
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
        _autocompleteBox = new AutocompleteBox<Transaction>(_descriptionRow);
        _autocompleteBox.SetSizeRequest(378, -1);
        _autocompleteBox.SetMarginTop(66);
        _autocompleteBox.SuggestionAccepted += (sender, e) =>
        {
            _descriptionRow.SetText(e.Item1);
            _descriptionRow.GrabFocus();
            _descriptionRow.SetPosition(-1);
            _descriptionRow.SetActivatesDefault(true);
            if (e.Item2.GroupId != -1)
            {
                _groupRow.SetSelected((uint)_controller.GroupNames.IndexOf(_controller.GetGroupNameFromId((uint)e.Item2.GroupId)));
                _colorDropDown.SetSelected((e.Item2.UseGroupColor && _groupRow.GetSelected() != 0) ? 0u : 1u);
                _colorDropDown.SetVisible(_groupRow.GetSelected() != 0);
                _colorButton.SetVisible(_colorDropDown.GetSelected() == 1);
                var transactionColor = new Gdk.RGBA();
                transactionColor.Parse(e.Item2.RGBA);
                _colorButton.SetRgba(transactionColor);
            }
        };
        _overlay.AddOverlay(_autocompleteBox);
        _descriptionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    var matchingDescriptions = _controller.GetDescriptionSuggestions(_descriptionRow.GetText());
                    if (matchingDescriptions.Count > 0)
                    {
                        _autocompleteBox.UpdateSuggestions(matchingDescriptions);
                    }
                    _descriptionRow.SetActivatesDefault(matchingDescriptions.Count == 0);
                    _autocompleteBox.SetVisible(matchingDescriptions.Count > 0);
                    Validate();
                }
            }
        };
        _descriptionKeyController = Gtk.EventControllerKey.New();
        _descriptionKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _descriptionKeyController.OnKeyPressed += (sender, e) =>
        {
            if (e.Keyval == 59) //semicolon
            {
                return true;
            }
            return false;
        };
        _descriptionRow.AddController(_descriptionKeyController);
        OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "default-width")
            {
                _autocompleteBox.SetSizeRequest(_descriptionRow.GetAllocatedWidth() - 24, -1);
            }
        };
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
                else
                {
                    _colorDropDown.SetSelected(_controller.Transaction.RGBA == _controller.DefaultTransactionColor ? 0u : 1u);
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
        _colorDialog = Gtk.ColorDialog.New();
        _colorDialog.SetWithAlpha(false);
        _colorButton.SetDialog(_colorDialog);
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
        //Tags
        var addTagKeyController = Gtk.EventControllerKey.New();
        addTagKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        addTagKeyController.OnKeyPressed += (sender, e) =>
        {
            if (e.Keyval == 44) // Comma
            {
                return true;
            }
            return false;
        };
        _addTagEntry.AddController(addTagKeyController);
        _addTagButton.OnClicked += (sender, e) =>
        {
            var tag = _addTagEntry.GetBuffer().GetText().Trim();
            if (!string.IsNullOrWhiteSpace(tag) && !_controller.AccountTags.Contains(tag))
            {
                _controller.AccountTags.Add(tag);
                UpdateTagsList();
            }
            _addTagEntry.GetBuffer().SetText("", 0);
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
            if (_autocompleteBox.GetVisible())
            {
                _descriptionRow.SetActivatesDefault(true);
                _autocompleteBox.SetVisible(false);
            }
            else
            {
                Close();
            }
            return true;
        })));
        AddController(_shortcutController);
        //Load Transaction
        _dateCalendar.SelectDay(DateTime.NewLocal(_controller.Transaction.Date.Year, _controller.Transaction.Date.Month, _controller.Transaction.Date.Day, 0, 0, 0.0)!);
        OnDateChanged(_dateCalendar, EventArgs.Empty);
        _descriptionRow.SetText(_controller.Transaction.Description);
        _amountRow.SetText(_controller.Transaction.Amount.ToAmountString(_controller.CultureForNumberString, _controller.UseNativeDigits, false));
        _incomeButton.SetActive(_controller.Transaction.Type == TransactionType.Income);
        _repeatIntervalRow.SetSelected(_controller.RepeatIntervalIndex);
        _dateDashLabel.SetVisible(_controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never);
        _repeatEndDateCalendarButton.SetVisible(_controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never);
        if (_controller.Transaction.RepeatEndDate != null)
        {
            _repeatEndDateCalendar.SelectDay(DateTime.NewLocal(_controller.Transaction.RepeatEndDate.Value.Year, _controller.Transaction.RepeatEndDate.Value.Month, _controller.Transaction.RepeatEndDate.Value.Day, 0, 0, 0.0)!);
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
        var transactionColor = new Gdk.RGBA();
        transactionColor.Parse(_controller.Transaction.RGBA);
        _colorButton.SetRgba(transactionColor);
        UpdateTagsList();
        _receipt = _controller.Transaction.Receipt;
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
        var selectedDay = _dateCalendar.GetDate();
        var date = new DateOnly(selectedDay.GetYear(), selectedDay.GetMonth(), selectedDay.GetDayOfMonth());
        var repeatEndDate = default(DateOnly?);
        if (_repeatEndDateCalendarButton.GetLabel() != _("No End Date"))
        {
            var selectedEndDay = _repeatEndDateCalendar.GetDate();
            repeatEndDate = new DateOnly(selectedEndDay.GetYear(), selectedEndDay.GetMonth(), selectedEndDay.GetDayOfMonth());
        }
        var groupObject = (Gtk.StringObject)_groupRow.GetSelectedItem()!;
        var tags = _tags.Where(x => x.Value).Select(x => x.Key).ToList();
        var text = _notesView.GetBuffer().Text ?? "";
        var checkStatus = _controller.UpdateTransaction(date, _descriptionRow.GetText(), _incomeButton.GetActive() ? TransactionType.Income : TransactionType.Expense, (int)_repeatIntervalRow.GetSelected(), groupObject.GetString(), _colorButton.GetRgba().ToString(), _colorDropDown.GetSelected() == 0, tags, _amountRow.GetText(), _receipt, repeatEndDate, text);
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
        var selectedDay = sender.GetDate();
        var date = new DateOnly(selectedDay.GetYear(), selectedDay.GetMonth(), selectedDay.GetDayOfMonth());
        _dateCalendarButton.SetLabel(date.ToString("d", CultureHelpers.DateCulture));
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
        var selectedDay = sender.GetDate();
        var date = new DateOnly(selectedDay.GetYear(), selectedDay.GetMonth(), selectedDay.GetDayOfMonth());
        _repeatEndDateCalendarButton.SetLabel(date.ToString("d", CultureHelpers.DateCulture));
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
    /// Updates the list of tags
    /// </summary>
    private void UpdateTagsList()
    {
        foreach (var tag in _controller.AccountTags)
        {
            if (!_tags.ContainsKey(tag))
            {
                var tagButton = new TagButton(tag);
                _tagsFlowBox.Append(tagButton);
                _tags.Add(tag, false);
                if (_controller.Transaction.Tags.Contains(tag))
                {
                    tagButton.SetActive(true);
                    _tags[tag] = true;
                }
                tagButton.FilterChanged += (sender, e) =>
                {
                    _tags[tag] = e.Filter;
                    _tagsButton.SetLabel(_n("{0} tag", "{0} tags", _tags.Count(x => x.Value), _tags.Count(x => x.Value)));
                    Validate();
                };
            }
        }
        _tagsScrolledWindow.SetVisible(_tags.Count > 0);
        _tagsButton.SetLabel(_n("{0} tag", "{0} tags", _tags.Count(x => x.Value), _tags.Count(x => x.Value)));
    }

    /// <summary>
    /// Occurs when the view receipt button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private async void OnViewReceipt(Gtk.Button sender, EventArgs e) => await _controller.OpenReceiptImageAsync();

    /// <summary>
    /// Occurs when the delete receipt button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnDeleteReceipt(Gtk.Button sender, EventArgs e)
    {
        _receipt = null;
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
    private async void OnUploadReceipt(Gtk.Button sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileDialog.New();
        openFileDialog.SetTitle(_("Receipt"));
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
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterAll);
        filters.Append(filterJpeg);
        filters.Append(filterPng);
        filters.Append(filterPdf);
        openFileDialog.SetFilters(filters);
        try
        {
            var file = await openFileDialog.OpenAsync(this);
            _receipt = await _controller.GetImageFromPathAsync(file!.GetPath());
            _viewReceiptButton.SetSensitive(_receipt != null);
            _deleteReceiptButton.SetSensitive(_receipt != null);
            if (_receipt != null)
            {
                _viewReceiptButtonContent.SetLabel(_("View"));
            }
            else
            {
                _uploadReceiptButtonContent.SetLabel(_("Upload"));
            }
            Validate();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}
