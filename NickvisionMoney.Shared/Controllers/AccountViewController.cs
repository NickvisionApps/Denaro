using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NickvisionMoney.Shared.Controllers;

public class AccountViewController
{
    private readonly Account _account;

    public Localizer Localizer { get; init; }

    public string DefaultTransactionColor => Configuration.Current.TransactionDefaultColor;
    public string AccountTitle => Path.GetFileNameWithoutExtension(_account.Path);
    public string AccountTotal => _account.Total.ToString("C");
    public string AccountIncome => _account.Income.ToString("C");
    public string AccountExpense => _account.Expense.ToString("C");
    public Dictionary<uint, Group> Groups => _account.Groups;
    public Dictionary<uint, Transaction> Transactions => _account.Transactions;

    public event EventHandler? AccountInfoChanged;

    public AccountViewController(Localizer localizer, string path)
    {
        _account = new Account(path);
        Localizer = localizer;
    }
}
