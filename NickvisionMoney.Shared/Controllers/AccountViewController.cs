using Nickvision.Aura;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for an AccountView
/// </summary>
public class AccountViewController : IDisposable
{
    private bool _disposed;
    private readonly Account _account;
    private List<uint> _filteredIds;
    private decimal _filteredIncome;
    private decimal _filteredExpense;
    private readonly Dictionary<int, bool> _groupFilters;
    private readonly Dictionary<string, bool> _tagFilters;
    private DateOnly _filterStartDate;
    private DateOnly _filterEndDate;
    private string _searchDescription;

    /// <summary>
    /// Whether or not the account has been fully opened and loaded
    /// </summary>
    public bool IsOpened { get; private set; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits => Configuration.Current.UseNativeDigits;
    /// <summary>
    /// The default color to use for a transaction
    /// </summary>
    public string TransactionDefaultColor => Configuration.Current.TransactionDefaultColor;
    /// <summary>
    /// The default color to use for a group
    /// </summary>
    public string GroupDefaultColor => Configuration.Current.GroupDefaultColor;
    /// <summary>
    /// The path of the account
    /// </summary>
    public string AccountPath => _account.Path;
    /// <summary>
    /// Whether or not the account needs a password
    /// </summary>
    public bool AccountNeedsPassword => _account.IsEncrypted;
    /// <summary>
    /// The title (filename without extension) of the account
    /// </summary>
    public string AccountTitle => _account.Metadata.Name;
    /// <summary>
    /// The type of the account
    /// </summary>
    public AccountType AccountType => _account.Metadata.AccountType;
    /// <summary>
    /// Transactions in the account
    /// </summary>
    public Dictionary<uint, Transaction> Transactions => _account.Transactions;
    /// <summary>
    /// Groups in the account
    /// </summary>
    public Dictionary<uint, Group> Groups => _account.Groups;
    /// <summary>
    /// The list of upcoming transaction reminders in the account
    /// </summary>
    public List<(string Title, string Subtitle)> TransactionReminders => _account.TransactionReminders;
    /// <summary>
    /// The CultureInfo to use when displaying a number string
    /// </summary>
    public CultureInfo CultureForNumberString => CultureHelpers.GetNumberCulture(_account.Metadata);
    /// <summary>
    /// The total amount of the account for today
    /// </summary>
    public decimal AccountTodayTotal => _account.TodayTotal;
    /// <summary>
    /// The total amount of the account for today as a string
    /// </summary>
    public string AccountTodayTotalString => $"{(_account.TodayTotal >= 0 ? "+ " : "− ")}{_account.TodayTotal.ToAmountString(CultureForNumberString, UseNativeDigits)}";
    /// <summary>
    /// The income amount of the account for today
    /// </summary>
    public decimal AccountTodayIncome => _account.TodayIncome;
    /// <summary>
    /// The income amount of the account for today as a string
    /// </summary>
    public string AccountTodayIncomeString => _account.TodayIncome.ToAmountString(CultureForNumberString, UseNativeDigits);
    /// <summary>
    /// The expense amount of the account for today
    /// </summary>
    public decimal AccountTodayExpense => _account.TodayExpense;
    /// <summary>
    /// The expense amount of the account for today as a string
    /// </summary>
    public string AccountTodayExpenseString => _account.TodayExpense.ToAmountString(CultureForNumberString, UseNativeDigits);
    /// <summary>
    /// The number of filtered transactions being shown
    /// </summary>
    public int FilteredTransactionsCount => _filteredIds.Count;
    /// <summary>
    /// The total amount of the account for today as a string
    /// </summary>
    public string AccountFilteredTotalString => $"{((_filteredIncome - _filteredExpense) >= 0 ? "+ " : "− ")}{(_filteredIncome - _filteredExpense).ToAmountString(CultureForNumberString, UseNativeDigits)}";
    /// <summary>
    /// The income amount of the account for today as a string
    /// </summary>
    public string AccountFilteredIncomeString => _filteredIncome.ToAmountString(CultureForNumberString, UseNativeDigits);
    /// <summary>
    /// The expense amount of the account for today as a string
    /// </summary>
    public string AccountFilteredExpenseString => _filteredExpense.ToAmountString(CultureForNumberString, UseNativeDigits);

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    private event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when the recent accounts list is changed
    /// </summary>
    private event EventHandler<EventArgs>? RecentAccountsChanged;

    /// <summary>
    /// Occurs when the account's information is changed
    /// </summary>
    public event EventHandler<EventArgs>? AccountInformationChanged;
    /// <summary>
    /// Occurs when a group is created
    /// </summary>
    public event EventHandler<ModelEventArgs<Group>>? GroupCreated;
    /// <summary>
    /// Occurs when a group is deleted
    /// </summary>
    public event EventHandler<uint>? GroupDeleted;
    /// <summary>
    /// Occurs when a group is updated
    /// </summary>
    public event EventHandler<ModelEventArgs<Group>>? GroupUpdated;
    /// <summary>
    /// Occurs when a tag is created
    /// </summary>
    public event EventHandler<ModelEventArgs<string>>? TagCreated;
    /// <summary>
    /// Occurs when a tag is updated
    /// </summary>
    public event EventHandler<ModelEventArgs<string>>? TagUpdated;
    /// <summary>
    /// Occurs when a transaction is created
    /// </summary>
    public event EventHandler<ModelEventArgs<Transaction>>? TransactionCreated;
    /// <summary>
    /// Occurs when a transaction's position is moved
    /// </summary>
    public event EventHandler<ModelEventArgs<Transaction>>? TransactionMoved;
    /// <summary>
    /// Occurs when a transaction is deleted
    /// </summary>
    public event EventHandler<uint>? TransactionDeleted;
    /// <summary>
    /// Occurs when a transaction is updated
    /// </summary>
    public event EventHandler<ModelEventArgs<Transaction>>? TransactionUpdated;
    /// <summary>
    /// Occurs when a transfer is sent from this account
    /// </summary>
    public event EventHandler<Transfer>? TransferSent;

    /// <summary>
    /// Creates an AccountViewController
    /// </summary>
    /// <param name="path">The path of the account</param>
    /// <param name="notificationSent">The notification sent event</param>
    /// <param name="recentAccountsChanged">The recent accounts changed event</param>
    internal AccountViewController(string path, EventHandler<NotificationSentEventArgs>? notificationSent, EventHandler<EventArgs>? recentAccountsChanged)
    {
        IsOpened = false;
        _disposed = false;
        _account = new Account(path);
        _filteredIds = new List<uint>();
        _filteredIncome = 0;
        _filteredExpense = 0;
        _groupFilters = new Dictionary<int, bool>();
        _tagFilters = new Dictionary<string, bool>();
        NotificationSent = notificationSent;
        RecentAccountsChanged = recentAccountsChanged;
        //Setup Filters
        _groupFilters.Add(-3, true); //Income 
        _groupFilters.Add(-2, true); //Expense
        _filterStartDate = DateOnly.FromDateTime(DateTime.Today);
        _filterEndDate = DateOnly.FromDateTime(DateTime.Today);
        _searchDescription = "";
    }

    /// <summary>
    /// Finalizes the AccountViewController
    /// </summary>
    ~AccountViewController() => Dispose(false);

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
    /// Whether or not to show the tags section on the account view
    /// </summary>
    public bool ShowTagsList
    {
        get => _account.Metadata.ShowTagsList;

        set
        {
            if (_account.Metadata.ShowTagsList != value)
            {
                _account.Metadata.ShowTagsList = value;
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
            if (_account.Metadata.SortFirstToLast != value)
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
    /// Frees resources used by the AccountViewController object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the AccountViewController object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _account.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// Sends a notification
    /// </summary>
    /// <param name="message">The message of the notification</param>
    /// <param name="severity">The NotificationSeverity of the notification</param>
    public void SendNotification(string message, NotificationSeverity severity) => NotificationSent?.Invoke(this, new NotificationSentEventArgs(message, severity));

    /// <summary>
    /// The sorting function for transactions
    /// </summary>
    /// <param name="a">The id of the first transaction</param>
    /// <param name="b">The id of the second transaction</param>
    /// <returns>-1 if a < b, 0 if a = b, 1 if a > b</returns>
    private int SortTransactions(uint a, uint b)
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
    }

    /// <summary>
    /// Logins into an account
    /// </summary>
    /// <param name="password">The password of the account</param>
    /// <returns>True if login successful, else false</returns>
    public bool Login(string? password) => _account.Login(password);

    /// <summary>
    /// Initializes the AccountView
    /// </summary>
    public async Task StartupAsync()
    {
        if (!IsOpened)
        {
            await _account.LoadAsync();
            _searchDescription = "";
            //Metadata
            Configuration.Current.AddRecentAccount(new RecentAccount(AccountPath)
            {
                Name = AccountTitle,
                Type = AccountType
            });
            Aura.Active.SaveConfig("config");
            RecentAccountsChanged?.Invoke(this, EventArgs.Empty);
            //Groups
            foreach (var pair in _account.Groups.OrderBy(x => x.Value.Name == _("Ungrouped") ? " " : x.Value.Name))
            {
                _groupFilters.Add((int)pair.Value.Id, true);
                GroupCreated?.Invoke(this, new ModelEventArgs<Group>(pair.Value, null, true));
            }
            //Tags
            foreach (var tag in _account.Tags)
            {
                _tagFilters.Add(tag, true);
                TagCreated?.Invoke(this, new ModelEventArgs<string>(tag, null, true));
            }
            //Transactions
            _filteredIds = _account.Transactions.Keys.ToList();
            _filteredIncome = _account.TodayIncome;
            _filteredExpense = _account.TodayExpense;
            _filteredIds.Sort(SortTransactions);
            foreach (var id in _filteredIds)
            {
                TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(_account.Transactions[id], null, true));
            }
            AccountInformationChanged?.Invoke(this, EventArgs.Empty);
            //Register Events
            Configuration.Current.Saved += ConfigurationChanged;
            IsOpened = true;
        }
    }

    /// <summary>
    /// Creates a new AccountSettingsDialogController
    /// </summary>
    /// <returns>The new AccountSettingsDialogController</returns>
    public AccountSettingsDialogController CreateAccountSettingsDialogController() => new AccountSettingsDialogController(_account.Metadata, _account.IsEncrypted);

    /// <summary>
    /// Creates a new TransactionDialogController for a new transaction
    /// </summary>
    /// <returns>The new TransactionDialogController</returns>
    public TransactionDialogController CreateTransactionDialogController() => new TransactionDialogController(_account.NextAvailableTransactionId, _account.Transactions, _account.Groups, _account.Tags, _account.Metadata.DefaultTransactionType, TransactionDefaultColor, CultureForNumberString);

    /// <summary>
    /// Creates a new TransactionDialogController for an existing transaction
    /// </summary>
    /// <param name="id">The id of the existing transaction</param>
    /// <returns>The TransactionDialogController for the existing transaction</returns>
    public TransactionDialogController CreateTransactionDialogController(uint id) => new TransactionDialogController(_account.Transactions[id], _account.Transactions, _account.Groups, _account.Tags, true, TransactionDefaultColor, CultureForNumberString);

    /// <summary>
    /// Creates a new TransactionDialogController for a copy transaction
    /// </summary>
    /// <param name="source">The transaction to copy</param>
    /// <returns>The TransactionDialogController for the copied transaction</returns>
    public TransactionDialogController CreateTransactionDialogController(Transaction source)
    {
        var toCopy = new Transaction(_account.NextAvailableTransactionId)
        {
            Date = source.Date,
            Description = $"{source.Description} {_("(Copy)")}",
            Type = source.Type,
            RepeatInterval = source.RepeatInterval,
            Amount = source.Amount,
            GroupId = source.GroupId,
            RGBA = source.RGBA,
            UseGroupColor = source.UseGroupColor,
            Receipt = source.Receipt,
            RepeatFrom = source.RepeatFrom,
            RepeatEndDate = source.RepeatEndDate
        };
        return new TransactionDialogController(toCopy, _account.Transactions, _account.Groups, _account.Tags, false, TransactionDefaultColor, CultureForNumberString);
    }

    /// <summary>
    /// Creates a new GroupDialogController
    /// </summary>
    /// <returns>The new GroupDialogController</returns>
    public GroupDialogController CreateGroupDialogController()
    {
        var existingNames = new List<string>();
        foreach (var pair in _account.Groups)
        {
            existingNames.Add(pair.Value.Name);
        }
        return new GroupDialogController(_account.NextAvailableGroupId, existingNames, GroupDefaultColor);
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
        return new GroupDialogController(_account.Groups[id], existingNames, GroupDefaultColor);
    }

    /// <summary>
    /// Creates a new TransferDialogController
    /// </summary>
    /// <returns>The new TransferDialogController</returns>
    public TransferDialogController CreateTransferDialogController() => new TransferDialogController(new Transfer(AccountPath, AccountTitle), _account.TodayTotal, Configuration.Current.RecentAccounts, CultureForNumberString);

    /// <summary>
    /// Occurs when the configuration is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void ConfigurationChanged(object? sender, EventArgs e) => GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(Groups[0], null, _groupFilters[0]));

    /// <summary>
    /// Sets the new password of the account
    /// </summary>
    /// <param name="password">The new password</param>
    /// <param name="showNotification">Whether or not to show the notification</param>
    public void SetPassword(string password, bool showNotification = true)
    {
        _account.Password = password;
        if (showNotification)
        {
            if (string.IsNullOrEmpty(password))
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("The password of the account was removed."), NotificationSeverity.Success));
            }
            else
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("The password of the account was changed."), NotificationSeverity.Success));
            }
        }
    }

    /// <summary>
    /// Updates the metadata of the account
    /// </summary>
    /// <param name="metadata">The new metadata</param>
    /// <returns>True if successful, else false</returns>
    public void UpdateMetadata(AccountMetadata metadata)
    {
        var oldSymbol = _account.Metadata.CustomCurrencySymbol;
        var oldStyle = _account.Metadata.CustomCurrencyAmountStyle;
        var oldDecimalSeparator = _account.Metadata.CustomCurrencyDecimalSeparator;
        var oldGroupSeparator = _account.Metadata.CustomCurrencyGroupSeparator;
        var oldDecimalDigits = _account.Metadata.CustomCurrencyDecimalDigits;
        _account.UpdateMetadata(metadata);
        Configuration.Current.AddRecentAccount(new RecentAccount(AccountPath)
        {
            Name = AccountTitle,
            Type = AccountType
        });
        Aura.Active.SaveConfig("config");
        RecentAccountsChanged?.Invoke(this, EventArgs.Empty);
        if (oldSymbol != metadata.CustomCurrencySymbol || oldStyle != metadata.CustomCurrencyAmountStyle || oldDecimalSeparator != metadata.CustomCurrencyDecimalSeparator || oldGroupSeparator != metadata.CustomCurrencyGroupSeparator || oldDecimalDigits != metadata.CustomCurrencyDecimalDigits)
        {
            foreach (var pair in _account.Groups)
            {
                GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(pair.Value, null, _groupFilters.ContainsKey((int)pair.Key) ? _groupFilters[(int)pair.Key] : true));
            }
            foreach (var pair in _account.Transactions)
            {
                TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, true));
            }
        }
        AccountInformationChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Adds a transaction to the account
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    public async Task AddTransactionAsync(Transaction transaction)
    {
        var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
        var res = await _account.AddTransactionAsync(transaction);
        if (res.Successful)
        {
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
            for (var i = 0; i < transactions.Count; i++)
            {
                if (transactions[i] == transaction.Id)
                {
                    TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(transaction, i, true));
                }
                if (_account.Transactions[transactions[i]].RepeatFrom == transaction.Id)
                {
                    TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(_account.Transactions[transactions[i]], i, true));
                }
            }
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[groupId], null, _groupFilters[(int)groupId]));
            foreach (var tag in res.NewTags)
            {
                _tagFilters.Add(tag, true);
                TagCreated?.Invoke(this, new ModelEventArgs<string>(tag, null, true));
            }
            FilterUIUpdate();
        }
    }

    /// <summary>
    /// Updates a transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        var originalGroupId = _account.Transactions[transaction.Id].GroupId == -1 ? 0u : (uint)_account.Transactions[transaction.Id].GroupId;
        var newGroupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
        var res = await _account.UpdateTransactionAsync(transaction);
        if (res.Successful)
        {
            TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(transaction, null, true));
            if (transaction.RepeatInterval != TransactionRepeatInterval.Never)
            {
                foreach (var pair in _account.Transactions)
                {
                    if (pair.Value.RepeatFrom == transaction.Id)
                    {
                        TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, true));
                    }
                }
            }
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[originalGroupId], null, _groupFilters[(int)originalGroupId]));
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[newGroupId], null, _groupFilters[(int)newGroupId]));
            foreach (var tag in res.NewTags)
            {
                _tagFilters.Add(tag, true);
                TagCreated?.Invoke(this, new ModelEventArgs<string>(tag, null, true));
            }
            FilterUIUpdate();
        }
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
        var res = await _account.UpdateSourceTransactionAsync(transaction, updateGenerated);
        if (res.Successful)
        {
            TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(transaction, null, true));
            foreach (var pair in _account.Transactions)
            {
                if (updateGenerated && pair.Value.RepeatFrom == transaction.Id)
                {
                    TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, true));
                }
                else if (!updateGenerated)
                {
                    if (pair.Value.RepeatFrom == -1)
                    {
                        TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, true));
                    }
                    else if (pair.Value.RepeatFrom == transaction.Id)
                    {
                        TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, true));
                    }
                }
                if (!_account.Transactions.ContainsKey(pair.Key))
                {
                    TransactionDeleted?.Invoke(this, pair.Key);
                }
            }
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[originalGroupId], null, _groupFilters[(int)originalGroupId]));
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[newGroupId], null, _groupFilters[(int)newGroupId]));
            foreach (var tag in res.NewTags)
            {
                _tagFilters.Add(tag, true);
                TagCreated?.Invoke(this, new ModelEventArgs<string>(tag, null, true));
            }
            FilterUIUpdate();
        }
    }

    /// <summary>
    /// Removes a transaction from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    public async Task DeleteTransactionAsync(uint id)
    {
        var groupId = _account.Transactions[id].GroupId == -1 ? 0u : (uint)_account.Transactions[id].GroupId;
        await _account.DeleteTransactionAsync(id);
        TransactionDeleted?.Invoke(this, id);
        GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[groupId], null, _groupFilters[(int)groupId]));
        FilterUIUpdate();
    }

    /// <summary>
    /// Removes a source transaction from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    /// <param name="deleteGenerated">Whether or not to delete generated transactions associated with the source</param>
    public async Task DeleteSourceTransactionAsync(uint id, bool deleteGenerated)
    {
        var groupId = _account.Transactions[id].GroupId == -1 ? 0u : (uint)_account.Transactions[id].GroupId;
        TransactionDeleted?.Invoke(this, id);
        if (deleteGenerated)
        {
            foreach (var pair in _account.Transactions)
            {
                if (pair.Value.RepeatFrom == id)
                {
                    TransactionDeleted?.Invoke(this, pair.Value.Id);
                }
            }
        }
        await _account.DeleteSourceTransactionAsync(id, deleteGenerated);
        if (!deleteGenerated)
        {
            foreach (var pair in _account.Transactions)
            {
                if (pair.Value.RepeatFrom == -1)
                {
                    TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, true));
                }
            }
        }
        GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[groupId], null, _groupFilters[(int)groupId]));
        FilterUIUpdate();
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
                TransactionDeleted?.Invoke(this, pair.Value.Id);
            }
        }
        await _account.DeleteGeneratedTransactionsAsync(id);
        GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[groupId], null, _groupFilters[(int)groupId]));
        FilterUIUpdate();
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
        var groups = _account.Groups.Values.OrderBy(x => x.Name == _("Ungrouped") ? " " : x.Name).ToList();
        _groupFilters.Add((int)group.Id, true);
        GroupCreated?.Invoke(this, new ModelEventArgs<Group>(group, groups.IndexOf(group), true));
    }

    /// <summary>
    /// Updates a group in the account
    /// </summary>
    /// <param name="group">The group to update</param>
    public async Task UpdateGroupAsync(Group group, bool hasColorChanged)
    {
        await _account.UpdateGroupAsync(group);
        GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(group, null, _groupFilters[(int)group.Id]));
        if (hasColorChanged)
        {
            foreach (var pair in _account.Transactions)
            {
                if (pair.Value.GroupId == group.Id)
                {
                    TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, true));
                }
            }
        }
    }

    /// <summary>
    /// Removes a group from the account
    /// </summary>
    /// <param name="id">The id of the group to delete</param>
    public async Task DeleteGroupAsync(uint id)
    {
        var result = await _account.DeleteGroupAsync(id);
        _groupFilters.Remove((int)id);
        GroupDeleted?.Invoke(this, id);
        foreach (var transaction in result.BelongingTransactions)
        {
            TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(_account.Transactions[transaction], null, true));
        }
    }

    /// <summary>
    /// Sends a transfer to another account
    /// </summary>
    /// <param name="transfer">The transfer to send</param>
    public async Task SendTransferAsync(Transfer transfer)
    {
        var newTransaction = await _account.SendTransferAsync(transfer, _("Transfer To {0}", transfer.DestinationAccountName));
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
        for (var i = 0; i < transactions.Count; i++)
        {
            if (transactions[i] == newTransaction.Id)
            {
                TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(newTransaction, i, true));
            }
        }
        FilterUIUpdate();
        TransferSent?.Invoke(this, transfer);
    }

    /// <summary>
    /// Receives a transfer from another account 
    /// </summary>
    /// <param name="transfer">The transfer to receive</param>
    public async Task ReceiveTransferAsync(Transfer transfer)
    {
        var newTransaction = await _account.ReceiveTransferAsync(transfer, _("Transfer From {0}", transfer.SourceAccountName));
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
        for (var i = 0; i < transactions.Count; i++)
        {
            if (transactions[i] == newTransaction.Id)
            {
                TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(newTransaction, i, true));
            }
        }
        FilterUIUpdate();
    }

    /// <summary>
    /// Imports transaction from a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    public async Task ImportFromFileAsync(string path)
    {
        ImportResult res;
        try
        {
            res = await _account.ImportFromFileAsync(path, TransactionDefaultColor, GroupDefaultColor);
        }
        catch
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to import information from the file. Please ensure that the app has permissions to access the file and try again."), NotificationSeverity.Error));
            return;
        }
        if (!res.IsEmpty)
        {
            if (res.NewGroupIds.Count > 0)
            {
                var groupValues = _account.Groups.OrderBy(x => x.Value.Name == _("Ungrouped") ? " " : x.Value.Name).ToDictionary(x => x.Key, x => x.Value).Values.ToList();
                foreach (var id in res.NewGroupIds)
                {
                    _groupFilters[(int)id] = true;
                    GroupCreated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[id], groupValues.IndexOf(_account.Groups[id]), true));
                }
            }
            if (res.NewTags.Count >= 0)
            {
                foreach (var tag in res.NewTags)
                {
                    _tagFilters[tag] = true;
                    TagCreated?.Invoke(this, new ModelEventArgs<string>(tag, null, true));
                }
            }
            if (res.NewTransactionIds.Count >= 0)
            {
                foreach (var id in res.NewTransactionIds)
                {
                    var groupId = _account.Transactions[id].GroupId == -1 ? 0u : (uint)_account.Transactions[id].GroupId;
                    TransactionCreated?.Invoke(this, new ModelEventArgs<Transaction>(_account.Transactions[id], null, true));
                    GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(_account.Groups[groupId], null, _groupFilters[(int)groupId]));
                }
                FilterUIUpdate();
                SortUIUpdate();
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_n("Imported {0} transaction from file.", "Imported {0} transactions from file.", res.NewTransactionIds.Count, res.NewTransactionIds.Count), NotificationSeverity.Success, res.NewTransactionIds.Count == 0 ? "help-import" : ""));
            }
            else
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to import transactions from the file."), NotificationSeverity.Error, "help-import"));
            }
        }
        else
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Nothing to import from the file."), NotificationSeverity.Error, "help-import"));
        }
    }

    /// <summary>
    /// Exports the account to a CSV file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="exportMode">The information to export</param>
    public void ExportToCSV(string path, ExportMode exportMode)
    {
        if (_account.ExportToCSV(path, exportMode, _filteredIds))
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Exported account to file successfully."), NotificationSeverity.Success, "open-export", path));
        }
        else
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to export account to file."), NotificationSeverity.Error));
        }
    }

    /// <summary>
    /// Exports the account to a PDF file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="exportMode">The information to export</param>
    /// <param name="password">The password to protect the PDF file with (null for no security)</param>
    public void ExportToPDF(string path, ExportMode exportMode, string? password)
    {
        if (_account.ExportToPDF(path, exportMode, _filteredIds, password))
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Exported account to file successfully."), NotificationSeverity.Success, "open-export", path));
        }
        else
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to export account to file."), NotificationSeverity.Error));
        }
    }

    /// <summary>
    /// Generates a graph based on the type
    /// </summary>
    /// <param name="type">GraphType</param>
    /// <param name="darkMode">Whether or not to draw the graph in dark mode</param>
    /// <param name="width">The width of the graph</param>
    /// <param name="height">The height of the graph</param>
    /// <returns>The byte[] of the graph</returns>
    public byte[] GenerateGraph(GraphType type, bool darkMode, int width, int height) => _account.GenerateGraph(type, darkMode, _filteredIds, width, height);

    /// <summary>
    /// Gets whether or not a group filter is active
    /// </summary>
    /// <param name="key">The id of the filter</param>
    /// <returns>True if active, else false</returns>
    public bool IsGroupFilterActive(int key) => _groupFilters[key];

    /// <summary>
    /// Updates whether or not a filter is active
    /// </summary>
    /// <param name="key">The id of the filter</param>
    /// <param name="value">The value of the filter</param>
    public void UpdateGroupFilterValue(int key, bool value)
    {
        _groupFilters[key] = value;
        FilterUIUpdate();
    }

    /// <summary>
    /// Resets group filters
    /// </summary>
    public void ResetGroupFilters()
    {
        foreach (var pair in _account.Groups)
        {
            _groupFilters[(int)pair.Key] = true;
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(pair.Value, null, true));
        }
        FilterUIUpdate();
    }

    /// <summary>
    /// Unselect all group filters
    /// </summary>
    public void UnselectAllGroupFilters()
    {
        foreach (var pair in _account.Groups)
        {
            _groupFilters[(int)pair.Key] = false;
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(pair.Value, null, false));
        }
        FilterUIUpdate();
    }

    /// <summary>
    /// Updates whether or not a tag filter is active
    /// </summary>
    /// <param name="tag">The tag</param>
    /// <param name="active">Whether or not the tag filter is active</param>
    public void UpdateTagFilter(string tag, bool active)
    {
        _tagFilters[tag] = active;
        FilterUIUpdate();
    }

    /// <summary>
    /// Reset tag filters, setting all tags to enabled state
    /// </summary>
    public void ResetTagFilters()
    {
        foreach (var tag in _account.Tags)
        {
            _tagFilters[tag] = true;
            TagUpdated?.Invoke(this, new ModelEventArgs<string>(tag, null, true));
        }
        FilterUIUpdate();
    }

    /// <summary>
    /// Unselect all tag filters
    /// </summary>
    public void UnselectAllTagFilters()
    {
        foreach (var tag in _account.Tags)
        {
            _tagFilters[tag] = false;
            TagUpdated?.Invoke(this, new ModelEventArgs<string>(tag, null, false));
        }
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
    /// Updates the UI when filters are changed
    /// </summary>
    private void FilterUIUpdate()
    {
        _filteredIds.Clear();
        _filteredIncome = 0;
        _filteredExpense = 0;
        var groupBalances = new Dictionary<uint, (decimal Income, decimal Expense)>();
        foreach (var pair in _account.Transactions)
        {
            if (!string.IsNullOrWhiteSpace(SearchDescription))
            {
                if (!pair.Value.Description.ToLower().Contains(SearchDescription.ToLower()))
                {
                    continue;
                }
            }
            if (pair.Value.Type == TransactionType.Income && !_groupFilters[-3])
            {
                continue;
            }
            if (pair.Value.Type == TransactionType.Expense && !_groupFilters[-2])
            {
                continue;
            }
            if (!_groupFilters[pair.Value.GroupId == -1 ? 0 : pair.Value.GroupId])
            {
                continue;
            }
            if (!_tagFilters[_("Untagged")] && pair.Value.Tags.Count == 0)
            {
                continue;
            }
            if (!pair.Value.Tags.Any(x => _tagFilters[x]) && pair.Value.Tags.Count > 0)
            {
                continue;
            }
            if (_filterStartDate != DateOnly.FromDateTime(DateTime.Today) || _filterEndDate != DateOnly.FromDateTime(DateTime.Today))
            {
                if (pair.Value.Date < _filterStartDate || pair.Value.Date > _filterEndDate)
                {
                    continue;
                }
            }
            _filteredIds.Add(pair.Value.Id);
            if (pair.Value.Type == TransactionType.Income)
            {
                _filteredIncome += pair.Value.Amount;
            }
            else
            {
                _filteredExpense += pair.Value.Amount;
            }
            var groupKey = pair.Value.GroupId == -1 ? 0u : (uint)pair.Value.GroupId;
            if (!groupBalances.ContainsKey(groupKey))
            {
                groupBalances[groupKey] = (0m, 0m);
            }
            var income = groupBalances[groupKey].Income;
            var expense = groupBalances[groupKey].Expense;
            if (pair.Value.Type == TransactionType.Income)
            {
                income += pair.Value.Amount;
            }
            else
            {
                expense += pair.Value.Amount;
            }
            groupBalances[groupKey] = (income, expense);
        }
        //Update UI
        if (_filteredIds.Count > 0)
        {
            foreach (var pair in _account.Transactions)
            {
                TransactionUpdated?.Invoke(this, new ModelEventArgs<Transaction>(pair.Value, null, _filteredIds.Contains(pair.Value.Id)));
            }
        }
        foreach (var pair in _account.Groups)
        {
            var newGroup = Groups[pair.Key].Clone(groupBalances.ContainsKey(pair.Key) ? groupBalances[pair.Key].Income : 0m, groupBalances.ContainsKey(pair.Key) ? groupBalances[pair.Key].Expense : 0m);
            GroupUpdated?.Invoke(this, new ModelEventArgs<Group>(newGroup, null, _groupFilters[(int)pair.Key]));
        }
        AccountInformationChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates the UI when sorting is changed
    /// </summary>
    private void SortUIUpdate()
    {
        var transactions = _account.Transactions.Keys.ToList();
        transactions!.Sort(SortTransactions);
        _filteredIds.Sort(SortTransactions);
        for (var i = 0; i < transactions.Count; i++)
        {
            TransactionMoved?.Invoke(this, new ModelEventArgs<Transaction>(_account.Transactions[transactions[i]], i, _filteredIds.Contains(transactions[i])));
        }
    }
}
