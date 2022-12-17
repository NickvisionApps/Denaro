using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Models;
using System;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A row to display a Transaction model
/// </summary>
public sealed partial class TransactionRow : UserControl
{
    private readonly Transaction _transaction;

    /// <summary>
    /// The Id of the Transaction the row represents
    /// </summary>
    public uint Id => _transaction.Id;

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    public TransactionRow(Transaction transaction)
    {
        InitializeComponent();
        _transaction = transaction;
        //Load Transaction
        LblName.Text = _transaction.Description;
        LblDate.Text = _transaction.Date.ToString("d");
        LblAmount.Text = $"{(_transaction.Type == TransactionType.Income ? "+" : "-")}  {_transaction.Amount.ToString("C")}";
    }

    /// <summary>
    /// Occurs when the edit button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Edit(object sender, RoutedEventArgs e) => EditTriggered?.Invoke(this, Id);

    /// <summary>
    /// Occurs when the delete button on the row is clicked 
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Delete(object sender, RoutedEventArgs e) => DeleteTriggered?.Invoke(this, Id);
}
