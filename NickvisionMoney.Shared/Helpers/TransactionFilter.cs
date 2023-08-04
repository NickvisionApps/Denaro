using System.Collections.Generic;

namespace NickvisionMoney.Shared.Helpers;

/// <summary>
/// A helper class to manage filtering transactions
/// </summary>
public class TransactionFilter
{
    /// <summary>
    /// The list of filtered transaction ids
    /// </summary>
    public List<uint> FilteredIds { get; init; }
    
    /// <summary>
    /// Constructs a TransactionFilter
    /// </summary>
    public TransactionFilter()
    {
        FilteredIds = new List<uint>();
    }
}