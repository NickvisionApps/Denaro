using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Globalization;
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
            LblIncomeBreakdown.Text += _controller.Income.Breakdowns[currency].PerAccount.Replace("\n", "\n\n");
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            LblIncomeTotal.Text += $"{(_controller.Income.Breakdowns[currency].Total >= 0 ? "+ " : "- ")}{_controller.Income.Breakdowns[currency].Total.ToAmountString(culture)}\n\n";
        }
        foreach (var currency in _controller.Expense.Currencies)
        {
            LblExpenseBreakdown.Text += _controller.Expense.Breakdowns[currency].PerAccount.Replace("\n", "\n\n");
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            LblExpenseTotal.Text += $"{(_controller.Expense.Breakdowns[currency].Total >= 0 ? "+ " : "- ")}{_controller.Expense.Breakdowns[currency].Total.ToAmountString(culture)}\n\n";
        }
        foreach (var currency in _controller.Total.Currencies)
        {
            LblTotalBreakdown.Text += _controller.Total.Breakdowns[currency].PerAccount.Replace("\n", "\n\n");
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            LblTotalTotal.Text += $"{(_controller.Total.Breakdowns[currency].Total >= 0 ? "+ " : "- ")}{_controller.Total.Breakdowns[currency].Total.ToAmountString(culture)}\n\n";
        }
        BorderIncome.Background = new SolidColorBrush(Color.FromArgb(255, 38, 162, 105));
        BorderExpense.Background = new SolidColorBrush(Color.FromArgb(255, 192, 28, 40));
        BorderTotal.Background = new SolidColorBrush(Color.FromArgb(255, 53, 132, 228));
        foreach (var pair in _controller.Groups)
        {
            //DockPanel
            var dockPanel = new DockPanel()
            {
                MinHeight = 100,
                LastChildFill = true
            };
            var lblGroupTotal = new TextBlock()
            {
                Style = (Style)Application.Current.Resources["NavigationViewItemHeaderTextStyle"],
            };
            DockPanel.SetDock(lblGroupTotal, Dock.Right);
            var groupSeparator = new AppBarSeparator()
            {
                Margin = new Thickness(6, 0, 6, 0)
            };
            DockPanel.SetDock(groupSeparator, Dock.Right);
            var lblGroupBreakdown = new TextBlock();
            DockPanel.SetDock(lblGroupBreakdown, Dock.Right);
            foreach (var currency in pair.Value.DashboardAmount.Currencies)
            {
                lblGroupBreakdown.Text += pair.Value.DashboardAmount.Breakdowns[currency].PerAccount.Replace("\n", "\n\n");
                culture.NumberFormat.CurrencySymbol = currency.Symbol;
                lblGroupTotal.Text += $"{(pair.Value.DashboardAmount.Breakdowns[currency].Total >= 0 ? "+ " : "- ")}{pair.Value.DashboardAmount.Breakdowns[currency].Total.ToAmountString(culture)}\n\n";
            }
            dockPanel.Children.Add(lblGroupTotal);
            dockPanel.Children.Add(groupSeparator);
            dockPanel.Children.Add(lblGroupBreakdown);
            //StackPanel
            var stackPanel = new StackPanel()
            {
                Margin = new Thickness(10, 10, 10, 10),
                Orientation = Orientation.Vertical,
                Spacing = 6
            };
            stackPanel.Children.Add(new TextBlock()
            {
                Text = pair.Key,
                Style = (Style)Application.Current.Resources["NavigationViewItemHeaderTextStyle"]
            });
            stackPanel.Children.Add(new MenuFlyoutSeparator());
            stackPanel.Children.Add(dockPanel);
            stackPanel.Children.Add(new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 20,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(ColorHelpers.FromRGBA(pair.Value.RGBA)!.Value)
            });
            //Border
            var border = new Border()
            {
                MinWidth = 300,
                MinHeight = 140,
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                CornerRadius = new CornerRadius(6)
            };
            border.Child = stackPanel;
            ListGroups.Items.Add(border);
        }
    }
}
