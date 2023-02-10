using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing a Transaction
/// </summary>
public sealed partial class TransactionDialog : ContentDialog
{
    private bool _constructing;
    private readonly TransactionDialogController _controller;
    private readonly Action<object> _initializeWithWindow;
    private Color _selectedColor;
    private string? _receiptPath;

    /// <summary>
    /// Constructs a TransactionDialog
    /// </summary>
    /// <param name="controller">The TransactionDialogController</param>
    /// <param name="initializeWithWindow">The Action<object> callback for InitializeWithWindow</param>
    public TransactionDialog(TransactionDialogController controller, Action<object> initializeWithWindow)
    {
        InitializeComponent();
        _constructing = true;
        _controller = controller;
        _initializeWithWindow = initializeWithWindow;
        _receiptPath = null;
        //Localize Strings
        Title = $"{_controller.Localizer["Transaction"]} - {_controller.Transaction.Id}";
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer["OK"];
        if (_controller.CanCopy)
        {
            SecondaryButtonText = _controller.Localizer["MakeCopy"];
        }
        TxtDescription.Header = _controller.Localizer["Description", "Field"];
        TxtDescription.PlaceholderText = _controller.Localizer["Description", "Placeholder"];
        TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} - {_controller.CultureForNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForNumberString.NumberFormat.NaNSymbol})";
        TxtAmount.PlaceholderText = _controller.Localizer["Amount", "Placeholder"];
        CmbType.Header = _controller.Localizer["TransactionType", "Field"];
        CmbType.Items.Add(_controller.Localizer["Income"]);
        CmbType.Items.Add(_controller.Localizer["Expense"]);
        CalendarDate.Language = _controller.CultureForDateString.Name;
        CalendarDate.Header = _controller.Localizer["Date", "Field"];
        CalendarDate.DateFormat = new DateTimeFormatter("shortdate", new List<string>() { _controller.CultureForDateString.Name }).Patterns[0];
        CalendarDate.FirstDayOfWeek = (Windows.Globalization.DayOfWeek)_controller.CultureForDateString.DateTimeFormat.FirstDayOfWeek;
        CmbGroup.Header = _controller.Localizer["Group", "Field"];
        CmbRepeatInterval.Header = _controller.Localizer["TransactionRepeatInterval", "Field"];
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Never"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Daily"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Weekly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Biweekly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Monthly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Quarterly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Yearly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Biyearly"]);
        CalendarRepeatEndDate.Language = _controller.CultureForDateString.Name;
        CalendarRepeatEndDate.Header = _controller.Localizer["TransactionRepeatEndDate", "Field"];
        CalendarRepeatEndDate.DateFormat = new DateTimeFormatter("shortdate", new List<string>() { _controller.CultureForDateString.Name }).Patterns[0];
        CalendarRepeatEndDate.FirstDayOfWeek = (Windows.Globalization.DayOfWeek)_controller.CultureForDateString.DateTimeFormat.FirstDayOfWeek;
        ToolTipService.SetToolTip(BtnRepeatEndDateClear, controller.Localizer["TransactionRepeatEndDate", "Clear"]);
        LblColor.Text = _controller.Localizer["Color", "Field"];
        LblReceipt.Text = _controller.Localizer["Receipt", "Field"];
        LblBtnReceiptView.Text = _controller.Localizer["View"];
        LblBtnReceiptDelete.Text = _controller.Localizer["Delete"];
        LblBtnReceiptUpload.Text = _controller.Localizer["Upload"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        //Load Transaction
        TxtDescription.Text = _controller.Transaction.Description;
        TxtAmount.Text = _controller.Transaction.Amount.ToString("N2", _controller.CultureForNumberString);
        CmbType.SelectedIndex = (int)_controller.Transaction.Type;
        CalendarDate.Date = new DateTimeOffset(new DateTime(_controller.Transaction.Date.Year, _controller.Transaction.Date.Month, _controller.Transaction.Date.Day));
        foreach (var pair in _controller.Groups.OrderBy(x => x.Value == _controller.Localizer["Ungrouped"] ? " " : x.Value))
        {
            CmbGroup.Items.Add(pair.Value);
        }
        if (_controller.Transaction.GroupId == -1)
        {
            CmbGroup.SelectedIndex = 0;
        }
        else
        {
            CmbGroup.SelectedItem = _controller.Groups[(uint)_controller.Transaction.GroupId];
        }
        CmbRepeatInterval.SelectedIndex = (int)_controller.RepeatIntervalIndex;
        CalendarRepeatEndDate.IsEnabled = _controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never;
        BtnRepeatEndDateClear.IsEnabled = _controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never;
        if (_controller.Transaction.RepeatEndDate != null)
        {
            CalendarRepeatEndDate.Date = new DateTimeOffset(new DateTime(_controller.Transaction.RepeatEndDate.Value.Year, _controller.Transaction.RepeatEndDate.Value.Month, _controller.Transaction.RepeatEndDate.Value.Day));
        }
        _selectedColor = (Color)ColorHelpers.FromRGBA(_controller.Transaction.RGBA)!;
        BtnReceiptView.IsEnabled = _controller.Transaction.Receipt != null;
        BtnReceiptDelete.IsEnabled = _controller.Transaction.Receipt != null;
        Validate();
        _constructing = false;
    }

    public Color SelectedColor
    {
        get => _selectedColor;

        set
        {
            _selectedColor = value;
            if (!_constructing)
            {
                Validate();
            }
        }
    }

    /// <summary>
    /// Shows the TransactionDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            _controller.Accepted = false;
            return false;
        }
        if (result == ContentDialogResult.Secondary)
        {
            _controller.CopyRequested = true;
        }
        _controller.Accepted = true;
        return true;
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateTransaction(DateOnly.FromDateTime(CalendarDate.Date!.Value.Date), TxtDescription.Text, (TransactionType)CmbType.SelectedIndex, CmbRepeatInterval.SelectedIndex, (string)CmbGroup.SelectedItem, ColorHelpers.ToRGBA(SelectedColor), TxtAmount.Text, _receiptPath, CalendarRepeatEndDate.Date == null ? null : DateOnly.FromDateTime(CalendarRepeatEndDate.Date!.Value.Date));
        TxtDescription.Header = _controller.Localizer["Description", "Field"];
        TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} -  {_controller.CultureForNumberString.NumberFormat.CurrencySymbol} {(string.IsNullOrEmpty(_controller.CultureForNumberString.NumberFormat.NaNSymbol) ? "" : $"({_controller.CultureForNumberString.NumberFormat.NaNSymbol})")}";
        CalendarRepeatEndDate.Header = _controller.Localizer["TransactionRepeatEndDate", "Field"];
        if (checkStatus == TransactionCheckStatus.Valid)
        {
            TxtErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
        else
        {
            if (checkStatus.HasFlag(TransactionCheckStatus.EmptyDescription))
            {
                TxtDescription.Header = _controller.Localizer["Description", "Empty"];
            }
            if (checkStatus.HasFlag(TransactionCheckStatus.InvalidAmount))
            {
                TxtAmount.Header = _controller.Localizer["Amount", "Invalid"];
            }
            if (checkStatus.HasFlag(TransactionCheckStatus.InvalidRepeatEndDate))
            {
                CalendarRepeatEndDate.Header = _controller.Localizer["TransactionRepeatEndDate", "Invalid"];
            }
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
    }

    /// <summary>
    /// Occurs when the description textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtDescription_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_constructing)
        {
            while(TxtDescription.Text.Contains(';'))
            {
                TxtDescription.Text = TxtDescription.Text.Remove(TxtDescription.Text.IndexOf(';'), 1);
            }
            TxtDescription.Select(TxtDescription.Text.Length, 0);
            Validate();
        }
    }

    /// <summary>
    /// Occurs when a key is pressed on the amount textbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxtAmount_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (e.Key == VirtualKey.Decimal || e.Key == VirtualKey.Separator || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (e.Key == (VirtualKey)188 || e.Key == (VirtualKey)190)))
            {
                TxtAmount.Text = TxtAmount.Text.Substring(0, TxtAmount.Text.Length - 1) + _controller.CultureForNumberString.NumberFormat.NumberDecimalSeparator;
                TxtAmount.Select(TxtAmount.Text.Length, 0);
                e.Handled = true;
            }
        }
        e.Handled = false;
    }

    /// <summary>
    /// Occurs when the amount textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the type combobox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the date is changed
    /// </summary>
    /// <param name="sender">CalendarDatePicker</param>
    /// <param name="e">CalendarDatePickerDateChangedEventArgs</param>
    private void CalendarDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the group combobox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the repeat interval combobox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbRepeatInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var isRepeatIntervalNever = (string)CmbRepeatInterval.SelectedItem == _controller.Localizer["RepeatInterval", "Never"];
        CalendarRepeatEndDate.IsEnabled = !isRepeatIntervalNever;
        BtnRepeatEndDateClear.IsEnabled = !isRepeatIntervalNever;
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the repeat end date is changed
    /// </summary>
    /// <param name="sender">CalendarDatePicker</param>
    /// <param name="e">CalendarDatePickerDateChangedEventArgs</param>
    private void CalendarRepeatEndDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the clear repeat end date button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ClearRepeatEndDate(object sender, RoutedEventArgs e) => CalendarRepeatEndDate.Date = null;

    /// <summary>
    /// Occurs when the view receipt button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ViewReceipt(object sender, RoutedEventArgs e) => await _controller.OpenReceiptImageAsync(_receiptPath);

    /// <summary>
    /// Occurs when the delete receipt button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void DeleteReceipt(object sender, RoutedEventArgs e)
    {
        _receiptPath = "";
        BtnReceiptView.IsEnabled = false;
        BtnReceiptDelete.IsEnabled = false;
        Validate();
    }

    /// <summary>
    /// Occurs when the upload receipt button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void UploadReceipt(object sender, RoutedEventArgs e)
    {
        var fileOpenPicker = new FileOpenPicker();
        _initializeWithWindow(fileOpenPicker);
        fileOpenPicker.FileTypeFilter.Add(".jpg");
        fileOpenPicker.FileTypeFilter.Add(".jpeg");
        fileOpenPicker.FileTypeFilter.Add(".png");
        fileOpenPicker.FileTypeFilter.Add(".pdf");
        fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
        {
            _receiptPath = file.Path;
            BtnReceiptView.IsEnabled = true;
            Validate();
        }
    }
}
