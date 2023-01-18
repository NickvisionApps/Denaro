using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private string _searchDescription;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// Whether or not filtered transactions are available
    /// </summary>
    public bool HasFilteredTransactions { get; private set; }
    /// <summary>
    /// The list of UI transaction row objects
    /// </summary>
    public Dictionary<uint, IModelRowControl<Transaction>> TransactionRows { get; init; }
    /// <summary>
    /// The list of UI group row objects
    /// </summary>
    public Dictionary<uint, IGroupRowControl> GroupRows { get; init; }
    /// <summary>
    /// The UI function for creating a group row
    /// </summary>
    public Func<Group, int?, IGroupRowControl>? UICreateGroupRow { get; set; }
    /// <summary>
    /// The UI function for deleting a group row
    /// </summary>
    public Action<IGroupRowControl>? UIDeleteGroupRow { get; set; }
    /// <summary>
    /// The UI function for creating a transaction row
    /// </summary>
    public Func<Transaction, int?, IModelRowControl<Transaction>>? UICreateTransactionRow { get; set; }
    /// <summary>
    /// The UI function for moving a transaction row
    /// </summary>
    public Action<IModelRowControl<Transaction>, int>? UIMoveTransactionRow { get; set; }
    /// <summary>
    /// The UI function for deleting a transaction rowe
    /// </summary>
    public Action<IModelRowControl<Transaction>>? UIDeleteTransactionRow { get; set; }

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
    public string AccountTodayTotalString => $"{(_account.TodayTotal >= 0 ? "+ " : "- ")}{Math.Abs(_account.TodayTotal).ToString("C", CultureForNumberString)}";
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
    /// The count of transactions in the account
    /// </summary>
    public int TransactionsCount => _account.Transactions.Count;
    /// <summary>
    /// The count of groups in the account
    /// </summary>
    public int GroupsCount => _account.Groups.Count;

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    private event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when the recent accounts list is changed
    /// </summary>
    private event EventHandler? RecentAccountsChanged;
    /// <summary>
    /// Occurs when the transactions of an account are changed
    /// </summary>
    public event EventHandler? AccountTransactionsChanged;
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
        TransactionRows = new Dictionary<uint, IModelRowControl<Transaction>>();
        GroupRows = new Dictionary<uint, IGroupRowControl>();
        _filters = new Dictionary<int, bool>();
        Localizer = localizer;
        HasFilteredTransactions = false;
        NotificationSent = notificationSent;
        RecentAccountsChanged = recentAccountsChanged;
        //Setup Filters
        _filters.Add(-3, true); //Income 
        _filters.Add(-2, true); //Expense
        _filters.Add(-1, true); //No Group
        foreach (var pair in _account.Groups)
        {
            _filters.Add((int)pair.Value.Id, true);
        }
        _filterStartDate = DateOnly.FromDateTime(DateTime.Today);
        _filterEndDate = DateOnly.FromDateTime(DateTime.Today);
        _searchDescription = "";
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
            else
            {
                culture.NumberFormat.NaNSymbol = RegionInfo.CurrentRegion.ISOCurrencySymbol;
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
            }
        }
    }

    /// <summary>
    /// The way in which to sort transactions
    /// </summary>
    public SortBy SortTransactionsBy
    {
        get => _account.Metadata.SortTransactionsBy;

        set
        {
            if (_account.Metadata.SortTransactionsBy != value)
            {
                _account.Metadata.SortTransactionsBy = value;
                _account.UpdateMetadata(_account.Metadata);
                SortUIUpdate();
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
                SortUIUpdate();
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
    /// The search description text
    /// </summary>
    public string SearchDescription
    {
        get => _searchDescription;

        set
        {
            _searchDescription = value;
            FilterUIUpdate();
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
            FilterUIUpdate();
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
            FilterUIUpdate();
        }
    }

    /// <summary>
    /// Sends a notification
    /// </summary>
    /// <param name="message">The message of the notification</param>
    /// <param name="severity">The NotificationSeverity of the notification</param>
    public void SendNotification(string message, NotificationSeverity severity) => NotificationSent?.Invoke(this, new NotificationSentEventArgs(message, severity));

    /// <summary>
    /// Initializes the AccountView
    /// </summary>
    public async Task StartupAsync()
    {
        await _account.SyncRepeatTransactionsAsync();
        _searchDescription = "";
        //Groups
        GroupRows.Clear();
        foreach (var pair in _account.Groups.OrderBy(x => x.Value.Name == Localizer["Ungrouped"] ? " " : x.Value.Name))
        {
            GroupRows.Add(pair.Value.Id, UICreateGroupRow!(pair.Value, null));
        }
        //Transactions
        TransactionRows.Clear();
        var transactions = _account.Transactions.Values.ToList();
        transactions.Sort((a, b) =>
        {
            int compareTo = 0;
            if (SortTransactionsBy == SortBy.Id)
            {
                compareTo = a.CompareTo(b);
            }
            else if (SortTransactionsBy == SortBy.Date)
            {
                compareTo = a.Date.CompareTo(b.Date);
            }
            else if (SortTransactionsBy == SortBy.Amount)
            {
                var aAmount = a.Amount * (a.Type == TransactionType.Income ? 1m : -1m);
                var bAmount = b.Amount * (b.Type == TransactionType.Income ? 1m : -1m);
                compareTo = aAmount.CompareTo(bAmount);
            }
            if (!SortFirstToLast)
            {
                compareTo *= -1;
            }
            return compareTo;
        });
        foreach (var transaction in transactions)
        {
            TransactionRows.Add(transaction.Id, UICreateTransactionRow!(transaction, null));
        }
        HasFilteredTransactions = transactions.Count > 0;
        AccountTransactionsChanged?.Invoke(this, EventArgs.Empty);
    }

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
        return new TransactionDialogController(_account.NextAvailableTransactionId, groups, _account.Metadata.DefaultTransactionType, TransactionDefaultColor, CultureForNumberString, Localizer);
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
        return new GroupDialogController(_account.NextAvailableGroupId, existingNames, Localizer);
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
    public TransferDialogController CreateTransferDialogController() => new TransferDialogController(new Transfer(AccountPath), _account.TodayTotal, CultureForNumberString, Localizer);

    /// <summary>
    /// Updates the metadata of the account
    /// </summary>
    /// <param name="metadata">The new metadata</param>
    /// <returns>True if successful, else false</returns>
    public void UpdateMetadata(AccountMetadata metadata)
    {
        var oldSymbol = _account.Metadata.CustomCurrencySymbol;
        _account.UpdateMetadata(metadata);
        Configuration.Current.AddRecentAccount(new RecentAccount(AccountPath)
        {
            Name = AccountTitle,
            Type = AccountType
        });
        Configuration.Current.Save();
        RecentAccountsChanged?.Invoke(this, EventArgs.Empty);
        if(oldSymbol != metadata.CustomCurrencySymbol)
        {
            foreach(var row in GroupRows)
            {
                row.Value.UpdateRow(_account.Groups[row.Key], CultureForNumberString, _filters[(int)row.Key]);
            }
            foreach(var row in TransactionRows)
            {
                row.Value.UpdateRow(_account.Transactions[row.Key], CultureForNumberString);
            }
            AccountTransactionsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Adds a transaction to the account
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    public async Task AddTransactionAsync(Transaction transaction)
    {
        var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
        await _account.AddTransactionAsync(transaction);
        var transactions = _account.Transactions.Keys.ToList();
        transactions.Sort((a, b) =>
        {
            var compareTo = SortTransactionsBy == SortBy.Date ? _account.Transactions[a].Date.CompareTo(_account.Transactions[b].Date) : a.CompareTo(b);
            if (!SortFirstToLast)
            {
                compareTo *= -1;
            }
            return compareTo;
        });
        for(var i = 0; i < transactions.Count; i++)
        {
            if (transactions[i] == transaction.Id)
            {
                TransactionRows.Add(transaction.Id, UICreateTransactionRow!(transaction, i));
            }
            if (_account.Transactions[transactions[i]].RepeatFrom == transaction.Id)
            {
                TransactionRows.Add(_account.Transactions[transactions[i]].Id, UICreateTransactionRow!(_account.Transactions[transactions[i]], i));
            }
        }
        GroupRows[groupId].UpdateRow(_account.Groups[groupId], CultureForNumberString, _filters[(int)groupId]);
        FilterUIUpdate();
    }

    /// <summary>
    /// Updates a transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        var originalGroupId = _account.Transactions[transaction.Id].GroupId == -1 ? 0u : (uint)_account.Transactions[transaction.Id].GroupId;
        var newGroupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
        await _account.UpdateTransactionAsync(transaction);
        TransactionRows[transaction.Id].UpdateRow(transaction, CultureForNumberString);
        if(transaction.RepeatInterval != TransactionRepeatInterval.Never)
        {
            foreach (var pair in _account.Transactions)
            {
                if (pair.Value.RepeatFrom == transaction.Id)
                {
                    TransactionRows.Add(pair.Key, UICreateTransactionRow!(pair.Value, null));
                }
            }
        }
        GroupRows[originalGroupId].UpdateRow(_account.Groups[originalGroupId], CultureForNumberString, _filters[(int)originalGroupId]);
        GroupRows[newGroupId].UpdateRow(_account.Groups[newGroupId], CultureForNumberString, _filters[(int)newGroupId]);
        FilterUIUpdate();
    }

    /// <summary>
    /// Updates a source transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    /// <param name="updateGenerated">Whether or not to update generated transactions associated with the source</param>
    public async Task UpdateSourceTransactionAsync(Transaction transaction, bool updateGenerated)
    {
        var originalGroupId = _account.Transactions[transaction.Id].GroupId == -1 ? 0u : (uint)_account.Transactions[transaction.Id].GroupId;
        var newGroupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
        await _account.UpdateSourceTransactionAsync(transaction, updateGenerated);
        TransactionRows[transaction.Id].UpdateRow(transaction, CultureForNumberString);
        foreach(var pair in _account.Transactions)
        {
            if(updateGenerated && pair.Value.RepeatFrom == transaction.Id)
            {
                if(TransactionRows.ContainsKey(pair.Key))
                {
                    TransactionRows[pair.Value.Id].UpdateRow(pair.Value, CultureForNumberString);
                }
                else
                {
                    TransactionRows.Add(pair.Key, UICreateTransactionRow!(pair.Value, null));
                }
            }
            else if(!updateGenerated)
            {
                if(pair.Value.RepeatFrom == -1)
                {
                    TransactionRows[pair.Value.Id].UpdateRow(pair.Value, CultureForNumberString);
                }
                else if(pair.Value.RepeatFrom == transaction.Id)
                {
                    TransactionRows.Add(pair.Key, UICreateTransactionRow!(pair.Value, null));
                }
            }
            if(!_account.Transactions.ContainsKey(pair.Key))
            {
                UIDeleteTransactionRow!(TransactionRows[pair.Key]);
                TransactionRows.Remove(pair.Key);
            }
        }
        GroupRows[originalGroupId].UpdateRow(_account.Groups[originalGroupId], CultureForNumberString, _filters[(int)originalGroupId]);
        GroupRows[newGroupId].UpdateRow(_account.Groups[newGroupId], CultureForNumberString, _filters[(int)newGroupId]);
        AccountTransactionsChanged?.Invoke(this, EventArgs.Empty);
        FilterUIUpdate();
    }

    /// <summary>
    /// Removes a transaction from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    public async Task DeleteTransactionAsync(uint id)
    {
        var groupId = _account.Transactions[id].GroupId == -1 ? 0u : (uint)_account.Transactions[id].GroupId;
        await _account.DeleteTransactionAsync(id);
        UIDeleteTransactionRow!(TransactionRows[id]);
        TransactionRows.Remove(id);
        GroupRows[groupId].UpdateRow(_account.Groups[groupId], CultureForNumberString, _filters[(int)groupId]);
        AccountTransactionsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes a source transaction from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    /// <param name="deleteGenerated">Whether or not to delete generated transactions associated with the source</param>
    public async Task DeleteSourceTransactionAsync(uint id, bool deleteGenerated)
    {
        var groupId = _account.Transactions[id].GroupId == -1 ? 0u : (uint)_account.Transactions[id].GroupId;
        UIDeleteTransactionRow!(TransactionRows[id]);
        TransactionRows.Remove(id);
        if (deleteGenerated)
        {
            foreach(var pair in _account.Transactions) 
            { 
                if(pair.Value.RepeatFrom == id)
                {
                    UIDeleteTransactionRow!(TransactionRows[pair.Value.Id]);
                    TransactionRows.Remove(pair.Value.Id);
                }
            }
        }
        await _account.DeleteSourceTransactionAsync(id, deleteGenerated);
        if(!deleteGenerated)
        {
            foreach (var pair in _account.Transactions)
            {
                if(pair.Value.RepeatFrom == -1)
                {
                    TransactionRows[pair.Value.Id].UpdateRow(pair.Value, CultureForNumberString);
                }
            }
        }
        GroupRows[groupId].UpdateRow(_account.Groups[groupId], CultureForNumberString, _filters[(int)groupId]);
        AccountTransactionsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes generated repeat transactions from the account
    /// </summary>
    /// <param name="id">The id of the source transaction</param>
    public async Task DeleteGeneratedTransactionsAsync(uint id)
    {
        var groupId = _account.Transactions[id].GroupId == -1 ? 0u : (uint)_account.Transactions[id].GroupId;
        foreach (var pair in _account.Transactions)
        {
            if (pair.Value.RepeatFrom == id)
            {
                UIDeleteTransactionRow!(TransactionRows[pair.Value.Id]);
                TransactionRows.Remove(pair.Value.Id);
            }
        }
        await _account.DeleteGeneratedTransactionsAsync(id);
        GroupRows[groupId].UpdateRow(_account.Groups[groupId], CultureForNumberString, _filters[(int)groupId]);
        AccountTransactionsChanged?.Invoke(this, EventArgs.Empty);
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
            return _account.Transactions[id].RepeatFrom == 0;
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
        var groups = _account.Groups.Values.OrderBy(x => x.Name == Localizer["Ungrouped"] ? " " : x.Name).ToList();
        _filters.Add((int)group.Id, true);
        GroupRows.Add(group.Id, UICreateGroupRow!(group, groups.IndexOf(group)));
    }

    /// <summary>
    /// Updates a group in the account
    /// </summary>
    /// <param name="group">The group to update</param>
    public async Task UpdateGroupAsync(Group group)
    {
        await _account.UpdateGroupAsync(group);
        GroupRows[group.Id].UpdateRow(group, CultureForNumberString, _filters[(int)group.Id]);
    }

    /// <summary>
    /// Removes a group from the account
    /// </summary>
    /// <param name="id">The id of the group to delete</param>
    public async Task DeleteGroupAsync(uint id)
    {
        await _account.DeleteGroupAsync(id);
        _filters.Remove((int)id);
        UIDeleteGroupRow!(GroupRows[id]);
        GroupRows.Remove(id);
    }

    /// <summary>
    /// Sends a transfer to another account
    /// </summary>
    /// <param name="transfer">The transfer to send</param>
    public async Task SendTransferAsync(Transfer transfer)
    {
        transfer.SourceAccountName = AccountTitle;
        var newTransaction = await _account.SendTransferAsync(transfer, string.Format(Localizer["Transfer", "To"], AccountMetadata.LoadFromAccountFile(transfer.DestinationAccountPath)!.Name));
        TransactionRows.Add(newTransaction.Id, UICreateTransactionRow!(newTransaction, null));
        FilterUIUpdate();
        TransferSent?.Invoke(this, transfer);
    }

    /// <summary>
    /// Receives a transfer from another account 
    /// </summary>
    /// <param name="transfer">The transfer to receive</param>
    public async Task ReceiveTransferAsync(Transfer transfer)
    {
        var newTransaction = await _account.ReceiveTransferAsync(transfer, string.Format(Localizer["Transfer", "From"], transfer.SourceAccountName));
        TransactionRows.Add(newTransaction.Id, UICreateTransactionRow!(newTransaction, null));
        FilterUIUpdate();
    }

    /// <summary>
    /// Imports transaction from a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    public async Task ImportFromFileAsync(string path)
    {
        var importedIds = await _account.ImportFromFileAsync(path);
        if(importedIds.Count >= 0)
        {
            foreach(var pair in _account.Groups)
            {
                if(!_filters.ContainsKey((int)pair.Value.Id))
                {
                    _filters.Add((int)pair.Value.Id, true);
                    GroupRows.Add(pair.Value.Id, UICreateGroupRow!(pair.Value, null));
                }
            }
            foreach(var id in importedIds)
            {
                var groupId = _account.Transactions[id].GroupId == -1 ? 0u : (uint)_account.Transactions[id].GroupId;
                TransactionRows.Add(id, UICreateTransactionRow!(_account.Transactions[id], null));
                GroupRows[groupId].UpdateRow(_account.Groups[groupId], CultureForNumberString, _filters[(int)groupId]);
            }    
            FilterUIUpdate();
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(importedIds.Count == 1 ? string.Format(Localizer["Imported"], importedIds.Count) : string.Format(Localizer["Imported", true], importedIds.Count), NotificationSeverity.Success));
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
        FilterUIUpdate();
    }

    /// <summary>
    /// Sets the start and end date of the filter to the same date
    /// </summary>
    /// <param name="date">The date to set</param>
    public void SetSingleDateFilter(DateOnly date)
    {
        _filterStartDate = date;
        _filterEndDate = date;
        FilterUIUpdate();
    }

    /// <summary>
    /// Resets groups filter
    /// </summary>
    public void ResetGroupsFilter()
    {
        _filters[-1] = true; //Ungrouped
        GroupRows[0].FilterChecked = true;
        foreach(var pair in _account.Groups)
        {
            _filters[(int)pair.Key] = true;
            GroupRows[pair.Key].FilterChecked = true;
        }
        FilterUIUpdate();
    }

    /// <summary>
    /// Updates the UI when filters are changed
    /// </summary>
    private void FilterUIUpdate()
    {
        var filteredTransactions = new List<uint>();
        foreach (var pair in _account.Transactions)
        {
            if(!string.IsNullOrEmpty(SearchDescription))
            {
                if(!pair.Value.Description.ToLower().Contains(SearchDescription.ToLower()))
                {
                    continue;
                }
            }
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
            filteredTransactions.Add(pair.Value.Id);
        }
        HasFilteredTransactions = filteredTransactions.Count > 0;
        if(HasFilteredTransactions)
        {
            //Update UI
            foreach (var pair in TransactionRows)
            {
                if (filteredTransactions.Contains(pair.Value.Id))
                {
                    pair.Value.Show();
                }
                else
                {
                    pair.Value.Hide();
                }
            }
        }
        AccountTransactionsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates the UI when sorting is changed
    /// </summary>
    private void SortUIUpdate()
    {
        var transactions = _account.Transactions.Keys.ToList();
        transactions.Sort((a, b) =>
        {
            int compareTo = 0;
            if (SortTransactionsBy == SortBy.Id)
            {
                compareTo = a.CompareTo(b);
            }
            else if (SortTransactionsBy == SortBy.Date)
            {
                compareTo = _account.Transactions[a].Date.CompareTo(_account.Transactions[b].Date);
            }
            else if (SortTransactionsBy == SortBy.Amount)
            {
                var aAmount = _account.Transactions[a].Amount * (_account.Transactions[a].Type == TransactionType.Income ? 1m : -1m);
                var bAmount = _account.Transactions[b].Amount * (_account.Transactions[b].Type == TransactionType.Income ? 1m : -1m);
                compareTo = aAmount.CompareTo(bAmount);
            }
            if (!SortFirstToLast)
            {
                compareTo *= -1;
            }
            return compareTo;
        });
        for (var i = 0; i < transactions.Count; i++)
        {
            UIMoveTransactionRow!(TransactionRows[transactions[i]], i);
        }
    }
}
