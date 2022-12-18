using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NickvisionMoney.Shared.Controllers;

public class AccountViewController
{
    private readonly Account _account;
    private readonly Dictionary<int, bool> _filters;

    public Localizer Localizer { get; init; }

    public string DefaultTransactionColor => Configuration.Current.TransactionDefaultColor;
    public string AccountTitle => Path.GetFileNameWithoutExtension(_account.Path);
    public decimal AmountTotal => _account.Total;
    public string AccountTotalString => _account.Total.ToString("C");
    public decimal AmountIncome => _account.Income;
    public string AccountIncomeString => _account.Income.ToString("C");
    public decimal AmountExpense => _account.Expense;
    public string AccountExpenseString => _account.Expense.ToString("C");
    public Dictionary<uint, Group> Groups => _account.Groups;
    public Dictionary<uint, Transaction> Transactions => _account.Transactions;

    private event EventHandler<NotificationSentEventArgs>? _notificationSent;
    public event EventHandler? AccountInfoChanged;

    public AccountViewController(string path, Localizer localizer, EventHandler<NotificationSentEventArgs>? notificationSent)
    {
        _account = new Account(path);
        _filters = new Dictionary<int, bool>();
        Localizer = localizer;
        _notificationSent = notificationSent;
        //Setup Filters
        _filters.Add(-3, true); //Income 
        _filters.Add(-2, true); //Expense
        _filters.Add(-1, true); //No Group
        foreach(var pair in _account.Groups)
        {
            _filters.Add((int)pair.Value.Id, true);
        }
    }

    public void ImportFromFile(string path)
    {
        var imported = _account.ImportFromFile(path);
        if(imported > 0)
        {
            foreach(var pair in _account.Groups)
            {
                if(!_filters.ContainsKey((int)pair.Value.Id))
                {
                    _filters.Add((int)pair.Value.Id, true);
                }
            }
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
            _notificationSent?.Invoke(this, new NotificationSentEventArgs(imported == 1 ? string.Format(Localizer["Imported"], imported) : string.Format(Localizer["Imported", true], imported), NotificationSeverity.Success));
        }
        else
        {
            _notificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["UnableToImport"], NotificationSeverity.Error));
        }
    }

    public void ExportToFile(string path)
    {
        if(Path.GetExtension(path) != ".csv")
        {
            path += ".csv";
        }
        if(_account.ExportToCSV(path))
        {
            _notificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["Exported"], NotificationSeverity.Success));
        }
        else
        {
            _notificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["UnableToExport"], NotificationSeverity.Error));
        }
    }
}
