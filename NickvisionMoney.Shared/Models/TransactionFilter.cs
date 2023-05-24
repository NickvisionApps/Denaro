using System;
using System.Collections.Generic;
using System.Linq;

namespace NickvisionMoney.Shared.Models;

enum Filters {
    Income = -3,
    Expense = -2,
    NoGroup = -1,
}

class TransactionFilter
{
    public DateOnly FilterStartDate { get; private set; }
    public DateOnly FilterEndDate { get; private set; }
    private readonly Dictionary<int, bool> _filters;
    public string SearchDescription { get; set; } = "";

    internal TransactionFilter()
    {
        FilterStartDate = DateOnly.FromDateTime(DateTime.Today);
        FilterEndDate = DateOnly.FromDateTime(DateTime.Today);
        _filters = new Dictionary<int, bool>();
        //Setup Filters
        _filters.Add((int)Filters.Income, true);
        _filters.Add((int)Filters.Expense, true);
        _filters.Add((int)Filters.NoGroup, true);
    }

    internal List<Transaction> Filter(List<Transaction> transactions)
    {
        List<Transaction> filteredTransactions = new List<Transaction>();
        foreach (var transaction in transactions)
        {
            if (!string.IsNullOrEmpty(SearchDescription))
            {
                if (!transaction.Description.ToLower().Contains(SearchDescription.ToLower()))
                {
                    continue;
                }
            }
            if (transaction.Type == TransactionType.Income && !_filters[-3])
            {
                continue;
            }
            if (transaction.Type == TransactionType.Expense && !_filters[-2])
            {
                continue;
            }
            if (!_filters[transaction.GroupId])
            {
                continue;
            }
            if (FilterStartDate != DateOnly.FromDateTime(DateTime.Today) || FilterEndDate != DateOnly.FromDateTime(DateTime.Today))
            {
                if (transaction.Date < FilterStartDate || transaction.Date > FilterEndDate)
                {
                    continue;
                }
            }
            filteredTransactions.Add(transaction);
        }
        return filteredTransactions;
    }

    internal void SetStartDate(DateOnly value)
    {
        FilterStartDate = value;
    }

    internal void SetEndDate(DateOnly value)
    {
        FilterEndDate = value;
    }

    internal bool IsFilterActive(int key)
    {
        return _filters[key];
    }

    internal void RemoveFilter(int key)
    {
        UpdateFilter(key, false);
    }

    internal void AddFilter(int key)
    {
        UpdateFilter(key, true);
    }

    internal void UpdateFilter(int key, bool value)
    {
        _filters[key] = value;
    }
}
