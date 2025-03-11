using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using System.Globalization;
using Gtk.Internal;
using Builder = NickvisionMoney.GNOME.Helpers.Builder;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The DashboardView for the application
/// </summary>
public class DashboardView : Gtk.ScrolledWindow
{
    [Gtk.Connect] private readonly Adw.ActionRow _incomeRow;
    [Gtk.Connect] private readonly Gtk.Label _incomeSuffix;
    [Gtk.Connect] private readonly Adw.ActionRow _expenseRow;
    [Gtk.Connect] private readonly Gtk.Label _expenseSuffix;
    [Gtk.Connect] private readonly Adw.ActionRow _totalRow;
    [Gtk.Connect] private readonly Gtk.Label _totalSuffix;
    [Gtk.Connect] private readonly Gtk.FlowBox _groupsFlowbox;

    public DashboardView(Gtk.Builder builder, DashboardViewController controller) : base(builder.GetObject("_root").Handle as ScrolledWindowHandle)
    {
        builder.Connect(this);
        var culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true);
        var subtitle = "";
        var suffix = "";
        foreach (var currency in controller.Income.Currencies)
        {
            subtitle += controller.Income.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            suffix += $"+ {controller.Income.Breakdowns[currency].Total.ToAmountString(culture, controller.UseNativeDigits)}\n";
        }
        _incomeRow.SetSubtitle(subtitle.Trim('\n'));
        _incomeSuffix.SetText(suffix.Trim('\n'));
        subtitle = "";
        suffix = "";
        foreach (var currency in controller.Expense.Currencies)
        {
            subtitle += controller.Expense.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            suffix += $"− {controller.Expense.Breakdowns[currency].Total.ToAmountString(culture, controller.UseNativeDigits)}\n";
        }
        _expenseRow.SetSubtitle(subtitle.Trim('\n'));
        _expenseSuffix.SetText(suffix.Trim('\n'));
        subtitle = "";
        suffix = "";
        foreach (var currency in controller.Total.Currencies)
        {
            subtitle += controller.Total.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            suffix += $"{(controller.Total.Breakdowns[currency].Total >= 0 ? "+ " : "− ")}{controller.Total.Breakdowns[currency].Total.ToAmountString(culture, controller.UseNativeDigits)}\n";
        }
        _totalRow.SetSubtitle(subtitle.Trim('\n'));
        _totalSuffix.SetText(suffix.Trim('\n'));
        foreach (var pair in controller.Groups)
        {
            var row = Adw.ActionRow.New();
            row.SetTitle(pair.Key);
            row.AddCssClass("card");
            var prefix = new TransactionId(0);
            prefix.UpdateColor(pair.Value.RGBA, "", controller.UseNativeDigits);
            prefix.SetCompact(true);
            row.AddPrefix(prefix);
            var suffixBox = Gtk.Box.New(Gtk.Orientation.Vertical, 1);
            suffixBox.SetValign(Gtk.Align.Center);
            row.AddSuffix(suffixBox);
            subtitle = "";
            foreach (var currency in pair.Value.DashboardAmount.Currencies)
            {
                subtitle += pair.Value.DashboardAmount.Breakdowns[currency].PerAccount;
                culture.NumberFormat.CurrencySymbol = currency.Symbol;
                var suffixLabel = Gtk.Label.New($"{(pair.Value.DashboardAmount.Breakdowns[currency].Total >= 0 ? "+ " : "− ")}{pair.Value.DashboardAmount.Breakdowns[currency].Total.ToAmountString(culture, controller.UseNativeDigits)}");
                suffixLabel.AddCssClass(pair.Value.DashboardAmount.Breakdowns[currency].Total >= 0 ? "denaro-income" : "denaro-expense");
                suffixLabel.SetHalign(Gtk.Align.End);
                suffixBox.Append(suffixLabel);
            }
            row.SetSubtitle(subtitle.Trim('\n'));
            _groupsFlowbox.Append(row);
        }
    }

    public DashboardView(DashboardViewController controller) : this(Builder.FromFile("dashboard_view.ui"), controller)
    {
    }
}