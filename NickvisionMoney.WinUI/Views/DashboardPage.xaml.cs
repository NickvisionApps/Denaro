using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using Windows.UI;

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
        //Load
        foreach (var breakdown in _controller.Income.Breakdowns.Values)
        {
            LblIncomeBreakdown.Text += breakdown;
        }
        foreach (var breakdown in _controller.Expense.Breakdowns.Values)
        {
            LblExpenseBreakdown.Text += breakdown;
        }
        foreach (var breakdown in _controller.Total.Breakdowns.Values)
        {
            LblTotalBreakdown.Text += breakdown;
        }
        BorderIncome.Background = new SolidColorBrush(Color.FromArgb(255, 38, 162, 105));
        BorderExpense.Background = new SolidColorBrush(Color.FromArgb(255, 192, 28, 40));
        BorderTotal.Background = new SolidColorBrush(Color.FromArgb(255, 53, 132, 228));
    }
}
