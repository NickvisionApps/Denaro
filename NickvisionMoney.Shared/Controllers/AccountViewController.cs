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
    private DateOnly _filterStartDate;
    private DateOnly _filterEndDate;

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
        _filterStartDate = DateOnly.FromDateTime(DateTime.Today);
        _filterEndDate = DateOnly.FromDateTime(DateTime.Today);
    }

    public List<Transaction> FilteredTransactions
    {
        get
        {
            var filteredTransactions = new List<Transaction>();
            foreach(var pair in _account.Transactions)
            {
                if(pair.Value.Type == TransactionType.Income && !_filters[-3])
                {
                    continue;
                }
                if (pair.Value.Type == TransactionType.Expense && !_filters[-2])
                {
                    continue;
                }
                if (!_filters[pair.Value.GroupId])
                {
                    continue;
                }
                if(_filterStartDate != DateOnly.FromDateTime(DateTime.Today) && _filterEndDate != DateOnly.FromDateTime(DateTime.Today))
                {
                    if(pair.Value.Date < _filterStartDate || pair.Value.Date > _filterEndDate)
                    {
                        continue;
                    }
                }
                filteredTransactions.Add(pair.Value);
            }
            return filteredTransactions;
        }
    }

    public bool SortFirstToLast
    {
        get => Configuration.Current.SortFirstToLast;

        set
        {
            Configuration.Current.SortFirstToLast = value;
            Configuration.Current.Save();
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public DateOnly FilterStartDate
    {
        get => _filterStartDate;

        set
        {
            _filterStartDate = value;
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public DateOnly FilterEndDate
    {
        get => _filterEndDate;

        set
        {
            _filterEndDate = value;
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
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

    public bool IsFilterActive(int key) => _filters[key];

    public void UpdateFilterValue(int key, bool value)
    {
        _filters[key] = value;
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetDateFilters()
    {
        _filterStartDate = DateOnly.FromDateTime(DateTime.Today);
        _filterEndDate = DateOnly.FromDateTime(DateTime.Today);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }
}
