using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for an AccountView
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
    /// The path of the account
    /// </summary>
    public string AccountPath => _account.Path;
    /// <summary>
    /// Whether or not an account needs to be setup for the first time
    /// </summary>
    public bool AccountNeedsFirstTimeSetup => _account.NeedsFirstTimeSetup;
    /// <summary>
    /// The title (filename without extension) of the account
    /// </summary>
    public string AccountTitle => _account.Metadata.Name;
    /// <summary>
    /// The type of the account
    /// </summary>
    public AccountType AccountType => _account.Metadata.AccountType;
    /// <summary>
    /// The total amount of the account for today
    /// </summary>
    public decimal AccountTodayTotal => _account.TodayTotal;
    /// <summary>
    /// The total amount of the account for today as a string
    /// </summary>
    public string AccountTodayTotalString => _account.TodayTotal.ToString("C", CultureForNumberString);
    /// <summary>
    /// The income amount of the account for today
    /// </summary>
    public decimal AccountTodayIncome => _account.TodayIncome;
    /// <summary>
    /// The income amount of the account for today as a string
    /// </summary>
    public string AccountTodayIncomeString => _account.TodayIncome.ToString("C", CultureForNumberString);
    /// <summary>
    /// The expense amount of the account for today
    /// </summary>
    public decimal AccountTodayExpense => _account.TodayExpense;
    /// <summary>
    /// The expense amount of the account for today as a string
    /// </summary>
    public string AccountTodayExpenseString => _account.TodayExpense.ToString("C", CultureForNumberString);
    /// <summary>
    /// The groups of the account for today
    /// </summary>
    public Dictionary<uint, Group> Groups => _account.Groups;
    /// <summary>
    /// The transactions of the account for today
    /// </summary>
    public Dictionary<uint, Transaction> Transactions => _account.Transactions;

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    private event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when the recent accounts list is changed
    /// </summary>
    private event EventHandler? RecentAccountsChanged;
    /// <summary>
    /// Occurs when the information of an account is changed
    /// </summary>
    public event EventHandler? AccountInfoChanged;
    /// <summary>
    /// Occurs when a transfer is sent from this account
    /// </summary>
    public event EventHandler<Transfer>? TransferSent;

    /// <summary>
    /// Creates an AccountViewController
    /// </summary>
    /// <param name="path">The path of the account</param>
    /// <param name="localizer">The Localizer of the app</param>
    /// <param name="notificationSent">The notification sent event</param>
    /// <param name="recentAccountsChanged">The recent accounts changed event</param>
    internal AccountViewController(string path, Localizer localizer, EventHandler<NotificationSentEventArgs>? notificationSent, EventHandler? recentAccountsChanged)
    {
        _account = new Account(path);
        _filters = new Dictionary<int, bool>();
        Localizer = localizer;
        NotificationSent = notificationSent;
        RecentAccountsChanged = recentAccountsChanged;
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
    /// The CultureInfo to use when displaying a number string
    /// </summary>
    public CultureInfo CultureForNumberString
    {
        get
        {
            var culture = new CultureInfo(CultureInfo.CurrentCulture.Name);
            if (_account.Metadata.UseCustomCurrency)
            {
                culture.NumberFormat.CurrencySymbol = _account.Metadata.CustomCurrencySymbol ?? NumberFormatInfo.CurrentInfo.CurrencySymbol;
                culture.NumberFormat.NaNSymbol = _account.Metadata.CustomCurrencyCode ?? "";
            }
            return culture;
        }
    }

    /// <summary>
    /// Whether or not to show the groups section on the account view
    /// </summary>
    public bool ShowGroupsList
    {
        get => _account.Metadata.ShowGroupsList;

        set
        {
            if (_account.Metadata.ShowGroupsList != value)
            {
                _account.Metadata.ShowGroupsList = value;
                _account.UpdateMetadata(_account.Metadata);
                AccountInfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Whether or not to sort transactions from first to last
    /// </summary>
    public bool SortFirstToLast
    {
        get => _account.Metadata.SortFirstToLast;

        set
        {
            if(_account.Metadata.SortFirstToLast != value)
            {
                _account.Metadata.SortFirstToLast = value;
                _account.UpdateMetadata(_account.Metadata);
                AccountInfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// The list of dates in the account
    /// </summary>
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

    /// <summary>
    /// An "Ungrouped" group
    /// </summary>
    public Group UngroupedGroup
    {
        get
        {
            var total = 0m;
            foreach (var pair in Transactions)
            {
                if (pair.Value.GroupId == -1 && pair.Value.Date <= DateOnly.FromDateTime(DateTime.Now))
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
            filteredTransactions.Sort();
            return filteredTransactions;
        }
    }

    /// <summary>
    /// The list of years for the date range filter
    /// </summary>
    public List<string> YearsForRangeFilter
    {
        get
        {
            var years = new List<string>();
            if (_account.Transactions.Count > 0)
            {
                years.Add(DateTime.Now.Year.ToString());
            }
            foreach (var pair in _account.Transactions)
            {
                var year = pair.Value.Date.Year.ToString();
                if (!years.Contains(year))
                {
                    years.Add(year);
                }
            }
            years.Sort();
            return years;
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
    /// Sends a notification
    /// </summary>
    /// <param name="message">The message of the notification</param>
    /// <param name="severity">The NotificationSeverity of the notification</param>
    public void SendNotification(string message, NotificationSeverity severity) => NotificationSent?.Invoke(this, new NotificationSentEventArgs(message, severity));

    /// <summary>
    /// Creates a new AccountSettingsDialogController
    /// </summary>
    /// <returns>The new AccountSettingsDialogController</returns>
    public AccountSettingsDialogController CreateAccountSettingsDialogController() => new AccountSettingsDialogController(_account.Metadata, _account.NeedsFirstTimeSetup, Localizer);

    /// <summary>
    /// Creates a new TransactionDialogController
    /// </summary>
    /// <returns>The new TransactionDialogController</returns>
    public TransactionDialogController CreateTransactionDialogController()
    {
        var groups = new Dictionary<uint, string>();
        foreach(var pair in _account.Groups)
        {
            groups.Add(pair.Key, pair.Value.Name);
        }
        return new TransactionDialogController(new Transaction(_account.NextAvailableTransactionId), groups, _account.Metadata.DefaultTransactionType, TransactionDefaultColor, CultureForNumberString, Localizer);
    }

    /// <summary>
    /// Creates a new TransactionDialogController
    /// </summary>
    /// <param name="id">The id of the existing transaction</param>
    /// <returns>The TransactionDialogController for the existing transaction</returns>
    public TransactionDialogController CreateTransactionDialogController(uint id)
    {
        var groups = new Dictionary<uint, string>();
        foreach (var pair in _account.Groups)
        {
            groups.Add(pair.Key, pair.Value.Name);
        }
        return new TransactionDialogController(_account.Transactions[id], groups, _account.Metadata.DefaultTransactionType, TransactionDefaultColor, CultureForNumberString, Localizer);
    }

    /// <summary>
    /// Creates a new GroupDialogController
    /// </summary>
    /// <returns>The new GroupDialogController</returns>
    public GroupDialogController CreateGroupDialogController()
    {
        var existingNames = new List<string>();
        foreach(var pair in _account.Groups)
        {
            existingNames.Add(pair.Value.Name);
        }
        return new GroupDialogController(new Group(_account.NextAvailableGroupId), existingNames, Localizer);
    }

    /// <summary>
    /// Creates a new GroupDialogController
    /// </summary>
    /// <param name="id">The id of the existing group</param>
    /// <returns>The GroupDialogController for the existing group</returns>
    public GroupDialogController CreateGroupDialogController(uint id)
    {
        var existingNames = new List<string>();
        foreach (var pair in _account.Groups)
        {
            existingNames.Add(pair.Value.Name);
        }
        return new GroupDialogController(_account.Groups[id], existingNames, Localizer);
    }

    /// <summary>
    /// Creates a new TransferDialogController
    /// </summary>
    /// <returns>The new TransferDialogController</returns>
    public TransferDialogController CreateTransferDialogController() => new TransferDialogController(new Transfer(AccountPath), CultureForNumberString, Localizer);

    /// <summary>
    /// Updates the metadata of the account
    /// </summary>
    /// <param name="metadata">The new metadata</param>
    /// <returns>True if successful, else false</returns>
    public void UpdateMetadata(AccountMetadata metadata)
    {
        _account.UpdateMetadata(metadata);
        Configuration.Current.AddRecentAccount(new RecentAccount(AccountPath)
        {
            Name = AccountTitle,
            Type = AccountType
        });
        Configuration.Current.Save();
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        RecentAccountsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Checks if repeat transactions are needed and creates them if so
    /// </summary>
    /// <returns>True if AccountInfoChanged was triggered, else false</returns>
    public async Task<bool> SyncRepeatTransactionsAsync()
    {
        if(await _account.SyncRepeatTransactionsAsync())
        {
            AccountInfoChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a transaction to the account
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    public async Task AddTransactionAsync(Transaction transaction)
    {
        await _account.AddTransactionAsync(transaction);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates a transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        await _account.UpdateTransactionAsync(transaction);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates a source transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    /// <param name="updateGenerated">Whether or not to update generated transactions associated with the source</param>
    public async Task UpdateSourceTransactionAsync(Transaction transaction, bool updateGenerated)
    {
        await _account.UpdateSourceTransactionAsync(transaction, updateGenerated);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes a transaction from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    public async Task DeleteTransactionAsync(uint id)
    {
        await _account.DeleteTransactionAsync(id);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes a source transaction from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    /// <param name="deleteGenerated">Whether or not to delete generated transactions associated with the source</param>
    public async Task DeleteSourceTransactionAsync(uint id, bool deleteGenerated)
    {
        await _account.DeleteSourceTransactionAsync(id, deleteGenerated);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes generated repeat transactions from the account
    /// </summary>
    /// <param name="id">The id of the source transaction</param>
    public async Task DeleteGeneratedTransactionsAsync(uint id)
    {
        await _account.DeleteGeneratedTransactionsAsync(id);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets whether or not the transaction with the provided id is a source repeat transaction
    /// </summary>
    /// <param name="id">The id of the transaction</param>
    /// <returns>True if transaction is a source repeat transaction, else false</returns>
    public bool GetIsSourceRepeatTransaction(uint id)
    {
        try
        {
            return Transactions[id].RepeatFrom == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Adds a group to the account
    /// </summary>
    /// <param name="group">The group to add</param>
    public async Task AddGroupAsync(Group group)
    {
        await _account.AddGroupAsync(group);
        _filters.Add((int)group.Id, true);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates a group in the account
    /// </summary>
    /// <param name="group">The group to update</param>
    public async Task UpdateGroupAsync(Group group)
    {
        await _account.UpdateGroupAsync(group);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes a group from the account
    /// </summary>
    /// <param name="id">The id of the group to delete</param>
    public async Task DeleteGroupAsync(uint id)
    {
        await _account.DeleteGroupAsync(id);
        _filters.Remove((int)id);
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sends a transfer to another account
    /// </summary>
    /// <param name="transfer">The transfer to send</param>
    public async Task SendTransferAsync(Transfer transfer)
    {
        await _account.SendTransferAsync(transfer, string.Format(Localizer["Transfer", "To"], Path.GetFileNameWithoutExtension(transfer.DestinationAccountPath)));
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
        TransferSent?.Invoke(this, transfer);
    }

    /// <summary>
    /// Receives a transfer from another account 
    /// </summary>
    /// <param name="transfer">The transfer to receive</param>
    public async Task ReceiveTransferAsync(Transfer transfer)
    {
        await _account.ReceiveTransferAsync(transfer, string.Format(Localizer["Transfer", "From"], Path.GetFileNameWithoutExtension(transfer.SourceAccountPath)));
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
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
        if(_account.ExportToFile(path))
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
        _filters[-1] = true; //Ungrouped
        foreach(var pair in _account.Groups)
        {
            _filters[(int)pair.Key] = true;
        }
        AccountInfoChanged?.Invoke(this, EventArgs.Empty);
    }
}
