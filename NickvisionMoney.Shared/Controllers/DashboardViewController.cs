using NickvisionMoney.Shared.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// An amount in the dashboard
/// </summary>
public class DashboardAmount
{
    /// <summary>
    /// The list of currencies in the amount
    /// </summary>
    public List<(string Code, string Symbol)> Currencies { get; init; }
    /// <summary>
    /// The breakdown dictionary 
    /// </summary>
    public Dictionary<(string Code, string Symbol), (decimal Total, string PerAccount)> Breakdowns { get; init; }

    /// <summary>
    /// Constructs a DashboardAmount
    /// </summary>
    public DashboardAmount()
    {
        Currencies = new List<(string Code, string Symbol)>();
        Breakdowns = new Dictionary<(string Code, string Symbol), (decimal Total, string PerAccount)>();
    }
}

/// <summary>
/// A controller for the a DashboardView
/// </summary>
public class DashboardViewController
{
    private List<AccountViewController> _openAccounts;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The DashboardAmount object for incomes
    /// </summary>
    public DashboardAmount Income { get; init; }
    /// <summary>
    /// The DashboardAmount object for expenses
    /// </summary>
    public DashboardAmount Expense { get; init; }
    /// <summary>
    /// The DashboardAmount object for totals
    /// </summary>
    public DashboardAmount Total { get; init; }
    /// <summary>
    /// The list of DashboardAmounts for groups
    /// </summary>
    public Dictionary<string, (DashboardAmount DashboardAmount, string RGBA)> Groups { get; init; }

    /// <summary>
    /// Constructs a DashboardViewController
    /// </summary>
    /// <param name="localizer">The Localizer of the app</param>
    /// <param name="defaultColor">A default group color</param>
    internal DashboardViewController(List<AccountViewController> openAccounts, Localizer localizer, string defaultColor)
    {
        _openAccounts = openAccounts;
        Localizer = localizer;
        Income = new DashboardAmount();
        Expense = new DashboardAmount();
        Total = new DashboardAmount();
        Groups = new Dictionary<string, (DashboardAmount DashboardAmount, string RGBA)>();
        foreach (var controller in _openAccounts)
        {
            (string Code, string Symbol) currency = (controller.CultureForNumberString.NumberFormat.NaNSymbol, controller.CultureForNumberString.NumberFormat.CurrencySymbol);
            if (controller.AccountTodayIncome > 0)
            {
                if (!Income.Currencies.Contains(currency))
                {
                    Income.Currencies.Add(currency);
                    Income.Breakdowns[currency] = (0, "");
                }
                Income.Breakdowns[currency] = (Income.Breakdowns[currency].Total + controller.AccountTodayIncome, Income.Breakdowns[currency].PerAccount + $"{string.Format(Localizer["AmountFromAccount"], controller.AccountTodayIncomeString, controller.AccountTitle)}\n");
            }
            if (controller.AccountTodayExpense > 0)
            {
                if (!Expense.Currencies.Contains(currency))
                {
                    Expense.Currencies.Add(currency);
                    Expense.Breakdowns[currency] = (0, "");
                }
                Expense.Breakdowns[currency] = (Expense.Breakdowns[currency].Total + controller.AccountTodayExpense, Expense.Breakdowns[currency].PerAccount + $"{string.Format(Localizer["AmountFromAccount"], controller.AccountTodayExpenseString, controller.AccountTitle)}\n");
            }
            if (controller.AccountTodayTotal != 0)
            {
                if (!Total.Currencies.Contains(currency))
                {
                    Total.Currencies.Add(currency);
                    Total.Breakdowns[currency] = (0, "");
                }
                Total.Breakdowns[currency] = (Total.Breakdowns[currency].Total + controller.AccountTodayTotal, Total.Breakdowns[currency].PerAccount + $"{string.Format(Localizer["AmountFromAccount"], controller.AccountTodayTotalString, controller.AccountTitle)}\n");
            }
            foreach (var group in controller.Groups.Values)
            {
                if (group.Balance != 0)
                {
                    var name = group.Name.ToLower();
                    var nameBuilder = new StringBuilder(name);
                    nameBuilder[0] = char.ToUpper(name[0], CultureInfo.CurrentCulture);
                    name = nameBuilder.ToString();
                    if (!Groups.ContainsKey(name))
                    {
                        Groups[name] = (new DashboardAmount(), string.IsNullOrEmpty(group.RGBA) ? defaultColor : group.RGBA);
                    }
                    if (!Groups[name].DashboardAmount.Currencies.Contains(currency))
                    {
                        Groups[name].DashboardAmount.Currencies.Add(currency);
                        Groups[name].DashboardAmount.Breakdowns[currency] = (0, "");
                    }
                    Groups[name].DashboardAmount.Breakdowns[currency] = (Groups[name].DashboardAmount.Breakdowns[currency].Total + group.Balance, Groups[name].DashboardAmount.Breakdowns[currency].PerAccount + $"{string.Format(Localizer["AmountFromAccount"], $"{(group.Balance >= 0 ? "+ " : "- ")}{group.Balance.ToAmountString(controller.CultureForNumberString)}", controller.AccountTitle)}\n");
                }
            }
        }
    }
}
