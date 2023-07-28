using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a transaction
/// </summary>
public partial class TransactionRow : Gtk.FlowBoxChild, IModelRowControl<Transaction>
{
    private Transaction _transaction;
    private string _defaultColor;
    private CultureInfo _cultureAmount;
    private CultureInfo _cultureDate;
    private bool _isSmall;
    private Dictionary<uint, Group> _groups;
    private TransactionId _idWidget;
    private bool _useNativeDigits;

    [Gtk.Connect] private readonly Adw.ActionRow _row;
    [Gtk.Connect] private readonly Gtk.Label _amountLabel;
    [Gtk.Connect] private readonly Gtk.Button _editButton;
    [Gtk.Connect] private readonly Gtk.Box _suffixBox;

    /// <summary>
    /// The id of the Transaction
    /// </summary>
    public uint Id => _transaction.Id;

    /// <summary>
    /// Occurs when the edit button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? EditTriggered;

    /// <summary>
    /// Constructs a TransactionRow
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="transaction">The Transaction to display</param>
    /// <param name="groups">The groups in the account</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    /// <param name="useNativeDigits">Whether to use native digits</param>
    /// <param name="defaultColor">Default transaction color</param>
    private TransactionRow(Gtk.Builder builder, Transaction transaction, Dictionary<uint, Group> groups, CultureInfo cultureAmount, CultureInfo cultureDate, bool useNativeDigits, string defaultColor) : base(builder.GetPointer("_root"), false)
    {
        _transaction = transaction;
        _defaultColor = defaultColor;
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        _useNativeDigits = useNativeDigits;
        _isSmall = false;
        _groups = groups;
        //Build UI
        builder.Connect(this);
        _editButton.OnClicked += Edit;
        _idWidget = new TransactionId(transaction.Id);
        _row.AddPrefix(_idWidget);
        //Group Settings
        UpdateRow(transaction, defaultColor, cultureAmount, cultureDate);
    }

    /// <summary>
    /// Constructs a TransactionRow
    /// </summary>
    /// <param name="transaction">The Transaction to display</param>
    /// <param name="groups">The groups in the account</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    /// <param name="useNativeDigits">Whether to use native digits</param>
    /// <param name="defaultColor">Default transaction color</param>
    public TransactionRow(Transaction transaction, Dictionary<uint, Group> groups, CultureInfo cultureAmount, CultureInfo cultureDate, bool useNativeDigits, string defaultColor) : this(Builder.FromFile("transaction_row.ui"), transaction, groups, cultureAmount, cultureDate, useNativeDigits, defaultColor)
    {
    }

    /// <summary>
    /// Whether or not the row uses a small style
    /// </summary>
    public bool IsSmall
    {
        get => _isSmall;

        set
        {
            _isSmall = value;
            if (_isSmall)
            {
                _suffixBox.SetOrientation(Gtk.Orientation.Vertical);
                _suffixBox.SetMarginTop(4);
            }
            else
            {
                _suffixBox.SetOrientation(Gtk.Orientation.Horizontal);
                _suffixBox.SetMarginTop(0);
            }
            _idWidget.SetCompact(_isSmall);
        }
    }

    /// <summary>
    /// Updates the row with the new model
    /// </summary>
    /// <param name="transaction">The new Transaction model</param>
    /// <param name="defaultColor">The default color for the row</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="cultureDate">The culture to use for displaying date strings</param>
    public void UpdateRow(Transaction transaction, string defaultColor, CultureInfo cultureAmount, CultureInfo cultureDate)
    {
        _transaction = transaction;
        _defaultColor = defaultColor;
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        GLib.Functions.IdleAdd(0, () =>
        {
            //Row Settings
            _row.SetTitle(_transaction.Description);
            _row.SetSubtitle($"{_transaction.Date.ToString("d", _cultureDate)}{(_transaction.RepeatInterval != TransactionRepeatInterval.Never ? $"\n{_("Repeat Interval")}: {_(_transaction.RepeatInterval.ToString())}" : "")}");
            _idWidget.UpdateColor(_transaction.UseGroupColor ? _groups[_transaction.GroupId <= 0 ? 0u : (uint)_transaction.GroupId].RGBA : _transaction.RGBA, _defaultColor, _useNativeDigits);
            //Amount Label
            _amountLabel.SetLabel($"{(_transaction.Type == TransactionType.Income ? "+  " : "−  ")}{_transaction.Amount.ToAmountString(_cultureAmount, _useNativeDigits)}");
            _amountLabel.RemoveCssClass(_transaction.Type == TransactionType.Income ? "denaro-expense" : "denaro-income");
            _amountLabel.AddCssClass(_transaction.Type == TransactionType.Income ? "denaro-income" : "denaro-expense");
            //Buttons Box
            _editButton.SetVisible(_transaction.RepeatFrom <= 0);
            _editButton.SetSensitive(_transaction.RepeatFrom <= 0);
            return false;
        });
    }

    /// <summary>
    /// Shows the row
    /// </summary>
    public new void Show()
    {
        GLib.Functions.IdleAdd(0, () =>
        {
            SetVisible(true);
            return false;
        });
    }

    /// <summary>
    /// Hides the row
    /// </summary>
    public new void Hide()
    {
        GLib.Functions.IdleAdd(0, () =>
        {
            SetVisible(false);
            return false;
        });
    }

    /// <summary>
    /// Occurs when the edit button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Edit(Gtk.Button sender, EventArgs e) => EditTriggered?.Invoke(this, Id);
}