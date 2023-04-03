using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using System.Globalization;
using System;
using Windows.UI;
using NickvisionMoney.Shared.Helpers;

namespace NickvisionMoney.WinUI.Views;

public sealed partial class DashboardPage : UserControl
{
    private DashboardViewController _controller;

    public DashboardPage(DashboardViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        LblTitle.Text = _controller.Localizer["Dashboard"];
        LblAllAccounts.Text = _controller.Localizer["AllAccounts"];
        LblIncome.Text = _controller.Localizer["Income"];
        LblExpense.Text = _controller.Localizer["Expense"];
        LblTotal.Text = _controller.Localizer["Total"];
        LblGroups.Text = _controller.Localizer["Groups"];
        //Load
        var lcMonetary = Environment.GetEnvironmentVariable("LC_MONETARY");
        if (lcMonetary != null && lcMonetary.Contains(".UTF-8"))
        {
            lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".UTF-8"), 6);
        }
        else if (lcMonetary != null && lcMonetary.Contains(".utf8"))
        {
            lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".utf8"), 5);
        }
        if (lcMonetary != null && lcMonetary.Contains('_'))
        {
            lcMonetary = lcMonetary.Replace('_', '-');
        }
        var culture = new CultureInfo(!string.IsNullOrEmpty(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name, true);
        foreach (var currency in _controller.Income.Currencies)
        {
            LblIncomeBreakdown.Text += _controller.Income.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            LblIncomeTotal.Text += $"{_controller.Income.Breakdowns[currency].Total.ToAmountString(culture)}\n\n";
        }
        foreach (var currency in _controller.Expense.Currencies)
        {
            LblExpenseBreakdown.Text += _controller.Expense.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            LblExpenseTotal.Text += $"{_controller.Expense.Breakdowns[currency].Total.ToAmountString(culture)}\n\n";
        }
        foreach (var currency in _controller.Total.Currencies)
        {
            LblTotalBreakdown.Text += _controller.Total.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            LblTotalTotal.Text += $"{_controller.Total.Breakdowns[currency].Total.ToAmountString(culture)}\n\n";
        }
        BorderIncome.Background = new SolidColorBrush(Color.FromArgb(255, 38, 162, 105));
        BorderExpense.Background = new SolidColorBrush(Color.FromArgb(255, 192, 28, 40));
        BorderTotal.Background = new SolidColorBrush(Color.FromArgb(255, 53, 132, 228));
    }
}
