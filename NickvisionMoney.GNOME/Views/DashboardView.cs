using NickvisionMoney.GNOME.Controls;
using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using System;
using System.Globalization;

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

    public DashboardView(Gtk.Builder builder, DashboardViewController controller) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        var culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true);
        var subtitle = "";
        var suffix = "";
        foreach (var currency in controller.Income.Currencies)
        {
            subtitle += controller.Income.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            suffix += $"+ {controller.Income.Breakdowns[currency].Total.ToAmountString(culture)}\n";
        }
        _incomeRow.SetSubtitle(subtitle.Trim('\n'));
        _incomeSuffix.SetText(suffix.Trim('\n'));
        subtitle = "";
        suffix = "";
        foreach (var currency in controller.Expense.Currencies)
        {
            subtitle += controller.Expense.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            suffix += $"- {controller.Expense.Breakdowns[currency].Total.ToAmountString(culture)}\n";
        }
        _expenseRow.SetSubtitle(subtitle.Trim('\n'));
        _expenseSuffix.SetText(suffix.Trim('\n'));
        subtitle = "";
        suffix = "";
        foreach (var currency in controller.Total.Currencies)
        {
            subtitle += controller.Total.Breakdowns[currency].PerAccount;
            culture.NumberFormat.CurrencySymbol = currency.Symbol;
            suffix += $"{(controller.Total.Breakdowns[currency].Total >= 0 ? "+ " : "- ")}{controller.Total.Breakdowns[currency].Total.ToAmountString(culture)}\n";
        }
        _totalRow.SetSubtitle(subtitle.Trim('\n'));
        _totalSuffix.SetText(suffix.Trim('\n'));
        foreach (var pair in controller.Groups)
        {
            var row = Adw.ActionRow.New();
            row.SetTitle(pair.Key);
            row.AddCssClass("card");
            var prefix = new TransactionId(0, controller.Localizer);
            prefix.UpdateColor(pair.Value.RGBA, "");
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
                var suffixLabel = Gtk.Label.New($"{(pair.Value.DashboardAmount.Breakdowns[currency].Total >= 0 ? "+ " : "- ")}{pair.Value.DashboardAmount.Breakdowns[currency].Total.ToAmountString(culture)}");
                suffixLabel.AddCssClass(pair.Value.DashboardAmount.Breakdowns[currency].Total >= 0 ? "denaro-income" : "denaro-expense");
                suffixLabel.SetHalign(Gtk.Align.End);
                suffixBox.Append(suffixLabel);
            }
            row.SetSubtitle(subtitle.Trim('\n'));
            _groupsFlowbox.Append(row);
        }
    }

    public DashboardView(DashboardViewController controller) : this(Builder.FromFile("dashboard_view.ui", controller.Localizer), controller)
    {
    }
}