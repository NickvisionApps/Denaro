using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using NickvisionMoney.Shared.Models;
using SkiaSharp;
using static NickvisionMoney.Shared.Helpers.Gettext;


namespace NickvisionMoney.Shared.Helpers;

public static class AccountHelpers
{
    /// <summary>
    /// Gets the total amount for the date range
    /// </summary>
    /// <param name="transactions">The collection of transactions to consider</param>
    /// <returns>The total amount for the date range</returns>
    public static decimal GetTotal(IDictionary<uint, Transaction> transactions)
    {
        var income = GetIncome(transactions);
        var expense = GetExpense(transactions);
        return income - expense;
    }

    /// <summary>
    /// Gets the expense amount for the date range
    /// </summary>
    /// <param name="transactions">The collection of transactions to consider</param>
    /// <returns>The expense amount for the date range</returns>
    public static decimal GetExpense(IDictionary<uint, Transaction> transactions)
    {
        var expense = 0m;
        foreach (var pair in transactions)
        {
            if (pair.Value.Type == TransactionType.Expense)
            {
                expense += pair.Value.Amount;
            }
        }
        return expense;
    }

    /// <summary>
    /// Gets the income amount for the date range
    /// </summary>
    /// <param name="transactions">The collection of transactions to consider</param>
    /// <returns>The income amount for the date range</returns>
    public static decimal GetIncome(IDictionary<uint, Transaction> transactions)
    {
        var income = 0m;
        foreach (var pair in transactions)
        {
            if (pair.Value.Type == TransactionType.Income)
            {
                income += pair.Value.Amount;
            }
        }
        return income;
    }

    public static (decimal, decimal) GetGroupIncomeExpense(IDictionary<uint, Transaction> transactions, Group group)
    {
        return transactions.Values
            .Where(x => x.GroupId == group.Id || (x.GroupId == -1 && group.Id == 0))
            .Aggregate((0m, 0m), (data, transaction) => 
                transaction.Type == TransactionType.Income ? (data.Item1 + transaction.Amount, data.Item2) : (data.Item1, data.Item2 + transaction.Amount));
            
    }
    
    /// <summary>
    /// Generates a graph based on the type
    /// </summary>
    /// <param name="type">GraphType</param>
    /// <param name="darkMode">Whether or not to draw the graph in dark mode</param>
    /// <param name="filteredIds">A list of filtered ids</param>
    /// <param name="width">The width of the graph</param>
    /// <param name="height">The height of the graph</param>
    /// <param name="showLegend">Whether or not to show the legend</param>
    /// <returns>The byte[] of the graph</returns>
    public static byte[] GenerateGraph(GraphType type, bool darkMode, IDictionary<uint, Transaction> transactions, IDictionary<uint, Group> groups, int width = -1, int height = -1, bool showLegend = true)
    {
        InMemorySkiaSharpChart? chart = null;
        if (type == GraphType.IncomeExpensePie)
        {
            var income = GetIncome(transactions);
            var expense = GetExpense(transactions);
            chart = new SKPieChart()
            {
                Background = SKColor.Empty,
                Series = new ISeries[]
                {
                    new PieSeries<decimal> { Name = _("Income"), Values = new decimal[] { income }, Fill = new SolidColorPaint(SKColors.Green) },
                    new PieSeries<decimal> { Name = _("Expense"), Values = new decimal[] { expense }, Fill = new SolidColorPaint(SKColors.Red) }
                },
                LegendPosition = showLegend ? LegendPosition.Top : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        else if (type == GraphType.IncomeExpensePerGroup)
        {
            var data = new Dictionary<string, decimal[]>();
            foreach (var group in groups.Values)
            {
                var incomeExpense = GetGroupIncomeExpense(transactions, group);
                if(incomeExpense == (0m, 0m))
                    continue;
                data.Add(group.Name, new[] { incomeExpense.Item1, incomeExpense.Item2 });
            }
            chart = new SKCartesianChart()
            {
                Background = SKColor.Empty,
                Series = new ISeries[]
                {
                    new ColumnSeries<decimal>() { Name = _("Income"), Values = data.OrderBy(x => x.Key == _("Ungrouped") ? " " : x.Key).Select(x => x.Value[0]).ToArray(), Fill = new SolidColorPaint(SKColors.Green) },
                    new ColumnSeries<decimal>() { Name = _("Expense"), Values = data.OrderBy(x => x.Key == _("Ungrouped") ? " " : x.Key).Select(x => x.Value[1]).ToArray(), Fill = new SolidColorPaint(SKColors.Red) },
                },
                XAxes = new Axis[]
                {
                    new Axis() { Labels = data.Keys.OrderBy(x => x == _("Ungrouped") ? " " : x).ToArray(), LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black) }
                },
                YAxes = new Axis[]
                {
                    new Axis() { LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black) }
                },
                LegendPosition = showLegend ? LegendPosition.Top : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        else if (type == GraphType.IncomeExpenseOverTime)
        {
            //Graph
            var data = new Dictionary<DateOnly, decimal[]>();
            foreach (var transaction in transactions.Values)
            {
                if (!data.ContainsKey(transaction.Date))
                {
                    data.Add(transaction.Date, new decimal[2] { 0m, 0m });
                }
                if (transaction.Type == TransactionType.Income)
                {
                    data[transaction.Date][0] += transaction.Amount;
                }
                else
                {
                    data[transaction.Date][1] += transaction.Amount;
                }
            }
            chart = new SKCartesianChart()
            {
                Background = SKColor.Empty,
                Series = new ISeries[]
                {
                    new LineSeries<decimal>() { Name = _("Income"), Values = data.OrderBy(x => x.Key).Select(x => x.Value[0]).ToArray(), GeometryFill = new SolidColorPaint(SKColors.Green), GeometryStroke = new SolidColorPaint(SKColors.Green), Fill = null, Stroke = new SolidColorPaint(SKColors.Green) },
                    new LineSeries<decimal>() { Name = _("Expense"), Values = data.OrderBy(x => x.Key).Select(x => x.Value[1]).ToArray(), GeometryFill = new SolidColorPaint(SKColors.Red), GeometryStroke = new SolidColorPaint(SKColors.Red), Fill = null, Stroke = new SolidColorPaint(SKColors.Red) }
                },
                XAxes = new Axis[]
                {
                    new Axis() { Labels = data.Keys.Order().Select(x => x.ToString("d", CultureHelpers.DateCulture)).ToArray(), LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black), LabelsRotation = 50 }
                },
                YAxes = new Axis[]
                {
                    new Axis() { LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black) }
                },
                LegendPosition = showLegend ? LegendPosition.Top : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        else if (type == GraphType.IncomeByGroup || type == GraphType.ExpenseByGroup)
        {
            var data = new Dictionary<uint, decimal>();
            foreach (var transaction in transactions.Values)
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                if (type == GraphType.IncomeByGroup && transaction.Type == TransactionType.Income)
                {
                    if (!data.ContainsKey(groupId))
                    {
                        data.Add(groupId, 0m);
                    }
                    data[groupId] += transaction.Amount;
                }
                if (type == GraphType.ExpenseByGroup && transaction.Type == TransactionType.Expense)
                {
                    if (!data.ContainsKey(groupId))
                    {
                        data.Add(groupId, 0m);
                    }
                    data[groupId] += transaction.Amount;
                }
            }
            var series = new List<ISeries>(data.Count);
            foreach (var pair in data.OrderBy(x => groups[x.Key].Name == _("Ungrouped") ? " " : groups[x.Key].Name))
            {
                var hex = "#FF"; //255
                var rgba = string.IsNullOrEmpty(groups[pair.Key].RGBA) ? Configuration.Current.GroupDefaultColor : groups[pair.Key].RGBA;
                if (rgba.StartsWith("#"))
                {
                    rgba = rgba.Remove(0, 1);
                    if (rgba.Length == 8)
                    {
                        rgba = rgba.Remove(rgba.Length - 2);
                    }
                    hex += rgba;
                }
                else
                {
                    rgba = rgba.Remove(0, rgba.StartsWith("rgb(") ? 4 : 5);
                    rgba = rgba.Remove(rgba.Length - 1);
                    var fields = rgba.Split(',');
                    hex += byte.Parse(fields[0]).ToString("X2");
                    hex += byte.Parse(fields[1]).ToString("X2");
                    hex += byte.Parse(fields[2]).ToString("X2");
                }
                series.Add(new PieSeries<decimal>()
                {
                    Name = groups[pair.Key].Name,
                    Values = new decimal[] { pair.Value },
                    Fill = new SolidColorPaint(SKColor.Parse(hex))
                });
            }
            chart = new SKPieChart()
            {
                Title = new LabelVisual()
                {
                    Text = type == GraphType.IncomeByGroup ? _("Income") : _("Expense"),
                    Paint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
                    Padding = new LiveChartsCore.Drawing.Padding(15),
                    TextSize = 16
                },
                Background = SKColor.Empty,
                Series = series,
                LegendPosition = showLegend ? LegendPosition.Bottom : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        if (chart != null)
        {
            if (width > 0)
            {
                chart.Width = width;
            }
            if (height > 0)
            {
                chart.Height = height;
            }
            return chart.GetImage().Encode().ToArray();
        }
        return Array.Empty<byte>();
    }
}