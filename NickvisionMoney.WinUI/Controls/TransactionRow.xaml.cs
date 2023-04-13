using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A row to display a Transaction model
/// </summary>
public sealed partial class TransactionRow : UserControl, INotifyPropertyChanged, IModelRowControl<Transaction>
{
    private CultureInfo _cultureAmount;
    private CultureInfo _cultureDate;
    private Localizer _localizer;
    private int _repeatFrom;
    private Dictionary<uint, Group> _groups;

    /// <summary>
    /// The Id of the Transaction the row represents
    /// </summary>
    public uint Id { get; private set; }
    /// <summary>
    /// The GridViewItem container of this row
    /// </summary>
    public GridViewItem? Container { private get; set; }

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Constructs a TransactionRow
    /// </summary>
    /// <param name="transaction">The Transaction model to represent</param>
    /// <param name="groups">The groups in the account</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    /// <param name="defaultColor">The default color for the row</param>
    /// <param name="localizer">The Localizer for the app</param>
    public TransactionRow(Transaction transaction, Dictionary<uint, Group> groups, CultureInfo cultureAmount, CultureInfo cultureDate, string defaultColor, Localizer localizer)
    {
        InitializeComponent();
        DataContext = this;
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        _localizer = localizer;
        _repeatFrom = 0;
        _groups = groups;
        //Localize Strings
        MenuEdit.Text = _localizer["Edit", "TransactionRow"];
        MenuDelete.Text = _localizer["Delete", "TransactionRow"];
        ToolTipService.SetToolTip(BtnEdit, _localizer["Edit", "TransactionRow"]);
        ToolTipService.SetToolTip(BtnDelete, _localizer["Delete", "TransactionRow"]);
        UpdateRow(transaction, defaultColor, cultureAmount, cultureDate);
    }


    public Color BtnIdBackground
    {
        get
        {
            if(BtnId.Background is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return Colors.White;
        }
    }

    public Color BtnIdForeground
    {
        get
        {
            if (BtnId.Foreground is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return Colors.White;
        }
    }


    /// <summary>
    /// Shows the row
    /// </summary>
    public void Show() => Container!.Visibility = Visibility.Visible;

    /// <summary>
    /// Hides the row
    /// </summary>
    public void Hide() => Container!.Visibility = Visibility.Collapsed;

    /// <summary>
    /// Updates the row with the new model
    /// </summary>
    /// <param name="transaction">The new Transaction model</param>
    /// <param name="defaultColor">The default color for the row</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="cultureDate">The culture to use for displaying date strings</param>
    public void UpdateRow(Transaction transaction, string defaultColor, CultureInfo cultureAmount, CultureInfo cultureDate)
    {
        Id = transaction.Id;
        _repeatFrom = transaction.RepeatFrom;
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        MenuEdit.IsEnabled = _repeatFrom <= 0;
        BtnEdit.Visibility = _repeatFrom <= 0 ? Visibility.Visible : Visibility.Collapsed;
        MenuDelete.IsEnabled = _repeatFrom <= 0;
        BtnDelete.Visibility = _repeatFrom <= 0 ? Visibility.Visible : Visibility.Collapsed;
        var bgColorString = transaction.RGBA;
        var bgColorStrArray = new System.Text.RegularExpressions.Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
        var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
        BtnId.Content = transaction.Id;
        BtnId.Background = new SolidColorBrush(ColorHelpers.FromRGBA(transaction.UseGroupColor ? _groups[transaction.GroupId <= 0 ? 0u : (uint)transaction.GroupId].RGBA : transaction.RGBA) ?? ColorHelpers.FromRGBA(defaultColor)!.Value);
        BtnId.Foreground = new SolidColorBrush(luma < 0.5 ? Colors.White : Colors.Black);
        NotifyPropertyChanged("BtnIdBackground");
        NotifyPropertyChanged("BtnIdForeground");
        LblName.Text = transaction.Description;
        LblDescription.Text = transaction.Date.ToString("d", _cultureDate);
        if (transaction.RepeatInterval != TransactionRepeatInterval.Never)
        {
            LblDescription.Text += $"\n{_localizer["TransactionRepeatInterval", "Field"]}: {_localizer["RepeatInterval", transaction.RepeatInterval.ToString()]}";
        }
        LblAmount.Text = $"{(transaction.Type == TransactionType.Income ? "+" : "-")}  {transaction.Amount.ToAmountString(_cultureAmount)}";
    }

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    public void Edit(object sender, RoutedEventArgs e)
    {
        if (_repeatFrom <= 0)
        {
            EditTriggered?.Invoke(this, Id);
        }
    }

    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Delete(object sender, RoutedEventArgs e)
    {
        if (_repeatFrom <= 0)
        {
            DeleteTriggered?.Invoke(this, Id);
        }
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
