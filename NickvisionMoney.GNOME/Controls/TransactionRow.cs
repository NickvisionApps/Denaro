using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a transaction
/// </summary>
public partial class TransactionRow : Adw.PreferencesGroup, IModelRowControl<Transaction>
{
    private CultureInfo _cultureAmount;
    private CultureInfo _cultureDate;
    private Localizer _localizer;
    private bool _isSmall;
    private TransactionId _idWidget;

    [Gtk.Connect] private readonly Adw.ActionRow _row;
    [Gtk.Connect] private readonly Gtk.Label _amountLabel;
    [Gtk.Connect] private readonly Gtk.Button _editButton;
    [Gtk.Connect] private readonly Gtk.Button _deleteButton;
    [Gtk.Connect] private readonly Gtk.Box _suffixBox;

    /// <summary>
    /// The id of the Transaction
    /// </summary>
    public uint Id { get; private set; }

    public Gtk.FlowBoxChild? Container { get; set; }

    /// <summary>
    /// Occurs when the edit button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    private TransactionRow(Gtk.Builder builder, Transaction transaction, CultureInfo cultureAmount, CultureInfo cultureDate, Localizer localizer) : base(builder.GetPointer("_root"), false)
    {
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        _localizer = localizer;
        _isSmall = false;
        //Build UI
        builder.Connect(this);
        _editButton.OnClicked += Edit;
        _deleteButton.OnClicked += Delete;
        _idWidget = new TransactionId(transaction.Id, localizer);
        _row.AddPrefix(_idWidget);
        //Group Settings
        UpdateRow(transaction, cultureAmount, cultureDate);
    }

    /// <summary>
    /// Constructs a TransactionRow
    /// </summary>
    /// <param name="transaction">The Transaction to display</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    /// <param name="localizer">The Localizer for the app</param>
    public TransactionRow(Transaction transaction, CultureInfo cultureAmount, CultureInfo cultureDate, Localizer localizer) : this(Builder.FromFile("transaction_row.ui", localizer), transaction, cultureAmount, cultureDate, localizer)
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
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="cultureDate">The culture to use for displaying date strings</param>
    public void UpdateRow(Transaction transaction, CultureInfo cultureAmount, CultureInfo cultureDate)
    {
        Id = transaction.Id;
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        //Row Settings
        _row.SetTitle(transaction.Description);
        _row.SetSubtitle($"{transaction.Date.ToString("d", _cultureDate)}{(transaction.RepeatInterval != TransactionRepeatInterval.Never ? $"\n{_localizer["TransactionRepeatInterval", "Field"]}: {_localizer["RepeatInterval", transaction.RepeatInterval.ToString()]}" : "")}");
        _idWidget.UpdateColor(transaction.RGBA);
        //Amount Label
        _amountLabel.SetLabel($"{(transaction.Type == TransactionType.Income ? "+  " : "-  ")}{transaction.Amount.ToAmountString(_cultureAmount)}");
        _amountLabel.RemoveCssClass(transaction.Type == TransactionType.Income ? "denaro-expense" : "denaro-income");
        _amountLabel.AddCssClass(transaction.Type == TransactionType.Income ? "denaro-income" : "denaro-expense");
        //Buttons Box
        _editButton.SetVisible(transaction.RepeatFrom <= 0);
        _editButton.SetSensitive(transaction.RepeatFrom <= 0);
        _deleteButton.SetVisible(transaction.RepeatFrom <= 0);
        _deleteButton.SetSensitive(transaction.RepeatFrom <= 0);
    }

    public void Show() => Container!.SetVisible(true);

    public void Hide() => Container!.SetVisible(false);

    /// <summary>
    /// Occurs when the edit button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Edit(Gtk.Button sender, EventArgs e) => EditTriggered?.Invoke(this, Id);

    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Delete(Gtk.Button sender, EventArgs e) => DeleteTriggered?.Invoke(this, Id);
}