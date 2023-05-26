using System;
using System.Collections.Generic;
using System.Linq;

namespace NickvisionMoney.Shared.Models;

enum Filters
{
    Income = -3,
    Expense = -2,
    NoGroup = -1,
}

public class TransactionFilter
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

    /// <summary>
    /// Returns the list of transaction that match the filter
    /// </summary>
    internal List<Transaction> Filter(List<Transaction> transactions)
    {
        List<Transaction> filteredTransactions = new List<Transaction>();
        foreach (var transaction in transactions)
        {
            if (Filter(transaction))
            {
                filteredTransactions.Add(transaction);
            }
        }
        return filteredTransactions;
    }

    /// <summary>
    /// Returns if a transaction matches the filter
    /// </summary>
    internal bool Filter(Transaction transaction)
    {
        if (!string.IsNullOrEmpty(SearchDescription))
        {
            if (!transaction.Description.ToLower().Contains(SearchDescription.ToLower()))
            {
                return false;
            }
        }
        if (transaction.Type == TransactionType.Income && !_filters[(int)Filters.Income])
        {
            return false;
        }
        if (transaction.Type == TransactionType.Expense && !_filters[(int)Filters.Expense])
        {
            return false;
        }
        if (_filters.ContainsKey(transaction.GroupId) && !_filters[transaction.GroupId])
        {
            return false;
        }
        if (FilterStartDate != DateOnly.FromDateTime(DateTime.Today) || FilterEndDate != DateOnly.FromDateTime(DateTime.Today))
        {
            if (transaction.Date < FilterStartDate || transaction.Date > FilterEndDate)
            {
                return false;
            }
        }
        return true;
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
