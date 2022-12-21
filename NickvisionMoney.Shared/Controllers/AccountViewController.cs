using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A control for an AccountView
/// </summary>
public class AccountViewController
{
    private readonly Account _account;
    private readonly Dictionary<int, bool> _filters;
    private DateOnly _filterStartDate;
    private DateOnly _filterEndDate;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }

    /// <summary>
    /// The default color to use for a transaction
    /// </summary>
    public string TransactionDefaultColor => Configuration.Current.TransactionDefaultColor;
    /// <summary>
    /// The title (filename without extension) of the account
    /// </summary>
    public string AccountTitle => Path.GetFileNameWithoutExtension(_account.Path);
    /// <summary>
    /// The total amount of the account
    /// </summary>
    public decimal AccountTotal => _account.Total;
    /// <summary>
    /// The total amount of the account as a string
    /// </summary>
    public string AccountTotalString => _account.Total.ToString("C");
    /// <summary>
    /// The income amount of the account
    /// </summary>
    public decimal AccountIncome => _account.Income;
    /// <summary>
    /// The income amount of the account as a string
    /// </summary>
    public string AccountIncomeString => _account.Income.ToString("C");
    /// <summary>
    /// The expense amount of the account
    /// </summary>
    public decimal AccountExpense => _account.Expense;
    /// <summary>
    /// The expense amount of the account as a string
    /// </summary>
    public string AccountExpenseString => _account.Expense.ToString("C");
    /// <summary>
    /// The groups of the account
    /// </summary>
    public Dictionary<uint, Group> Groups => _account.Groups;
    /// <summary>
    /// The transactions of the account
    /// </summary>
    public Dictionary<uint, Transaction> Transactions => _account.Transactions;

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    private event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when the information of an account is changed
    /// </summary>
    public event EventHandler? AccountInfoChanged;

    /// <summary>
    /// Creates an AccountViewController
    /// </summary>
    /// <param name="path">The path of the account</param>
    /// <param name="localizer">The Localizer of the app</param>
    /// <param name="notificationSent">The notification sent event</param>
    public AccountViewController(string path, Localizer localizer, EventHandler<NotificationSentEventArgs>? notificationSent)
    {
        _account = new Account(path);
        _filters = new Dictionary<int, bool>();
        Localizer = localizer;
        NotificationSent = notificationSent;
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

    /// <summary>
    /// Finalizes an AccountViewController
    /// </summary>
    ~AccountViewController() => _account.Dispose();

    /// <summary>
    /// Whether or not to sort transactions from first to last
    /// </summary>
    public bool SortFirstToLast
    {
        get => Configuration.Current.SortFirstToLast;

        set
        {
            if(Configuration.Current.SortFirstToLast != value)
            {
                Configuration.Current.SortFirstToLast = value;
                Configuration.Current.Save();
                AccountInfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public List<DateOnly> DatesInAccount
    {
        get
        {
            var datesInAccount = new List<DateOnly>();
            foreach (var pair in _account.Transactions)
            {
                if (!datesInAccount.Contains(pair.Value.Date))
                {
                    datesInAccount.Add(pair.Value.Date);
                }
            }
            return datesInAccount;
        }
    }

    public Group UngroupedGroup
    {
        get
        {
            var total = 0m;
            foreach (var pair in Transactions)
            {
                if (pair.Value.GroupId == -1)
                {
                    total += pair.Value.Type == TransactionType.Income ? pair.Value.Amount : (pair.Value.Amount * -1);
                }
            }
            return new Group(0)
            {
                Name = Localizer["Ungrouped"],
                Description = Localizer["UngroupedDescription"],
                Balance = total
            };
        }
    }

    /// <summary>
    /// The list of filtered transactions
    /// </summary>
    public List<Transaction> FilteredTransactions
    {
        get
        {
            var filteredTransactions = new List<Transaction>();
            foreach (var pair in _account.Transactions)
            {
                if (pair.Value.Type == TransactionType.Income && !_filters[-3])
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
                if (_filterStartDate != DateOnly.FromDateTime(DateTime.Today) && _filterEndDate != DateOnly.FromDateTime(DateTime.Today))
                {
                    if (pair.Value.Date < _filterStartDate || pair.Value.Date > _filterEndDate)
                    {
                        continue;
                    }
                }
                filteredTransactions.Add(pair.Value);
            }
            return filteredTransactions;
        }
    }

    /// <summary>
    /// The start date of the filter
    /// </summary>
    public DateOnly FilterStartDate
    {
        get => _filterStartDate;

        set
        {
            _filterStartDate = value;
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// The end date of the filter
    /// </summary>
    public DateOnly FilterEndDate
    {
        get => _filterEndDate;

        set
        {
            _filterEndDate = value;
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Checks if repeat transactions are needed and creates them if so
    /// </summary>
    public async Task RunRepeatTransactionsAsync()
    {
        if(await _account.RunRepeatTransactionsAsync())
        {
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Imports transaction from a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    public async Task ImportFromFileAsync(string path)
    {
        var imported = await _account.ImportFromFileAsync(path);
        if(imported >= 0)
        {
            foreach(var pair in _account.Groups)
            {
                if(!_filters.ContainsKey((int)pair.Value.Id))
                {
                    _filters.Add((int)pair.Value.Id, true);
                }
            }
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(imported == 1 ? string.Format(Localizer["Imported"], imported) : string.Format(Localizer["Imported", true], imported), NotificationSeverity.Success));
        }
        else
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["UnableToImport"], NotificationSeverity.Error));
        }
    }

    /// <summary>
    /// Exports the account to a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    public void ExportToFile(string path)
    {
        if(Path.GetExtension(path) != ".csv")
        {
            path += ".csv";
        }
        if(_account.ExportToCSV(path))
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["Exported"], NotificationSeverity.Success));
        }
        else
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["UnableToExport"], NotificationSeverity.Error));
        }
    }

    /// <summary>
    /// Gets whether or not a filter is active
    /// </summary>
    /// <param name="key">The id of the filter</param>
    /// <returns>True if active, else false</returns>
    public bool IsFilterActive(int key) => _filters[key];

    /// <summary>
    /// Updates whether or not a filter is active
    /// </summary>
    /// <param name="key">The id of the filter</param>
    /// <param name="value">The value of the filter</param>
    public void UpdateFilterValue(int key, bool value)
    {
        _filters[key] = value;
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the start and end date of the filter to the same date
    /// </summary>
    /// <param name="date">The date to set</param>
    public void SetSingleDateFilter(DateOnly date)
    {
        _filterStartDate = date;
        _filterEndDate = date;
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resets groups filter
    /// </summary>
    public void ResetGroupsFilter()
    {
        foreach(var pair in _account.Groups)
        {
            _filters[(int)pair.Key] = true;
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
