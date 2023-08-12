using System.Collections.Generic;
using System.Linq;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of the result from importing a file
/// </summary>
public class ImportResult
{
    private static readonly ImportResult _empty;
    
    /// <summary>
    /// An empty ImportResult
    /// </summary>
    public static ImportResult Empty => _empty;
    
    /// <summary>
    /// The list of newly added transaction ids
    /// </summary>
    public List<uint> NewTransactionIds { get; init; }
    /// <summary>
    /// The list of newly added group ids
    /// </summary>
    public List<uint> NewGroupIds { get; init; }
    /// <summary>
    /// The list of newly added tags
    /// </summary>
    public List<string> NewTags { get; init; }
    /// <summary>
    /// Whether or not the ImportResult is empty
    /// </summary>
    public bool IsEmpty => NewTransactionIds.Count == 0 && NewGroupIds.Count == 0 && NewTags.Count == 0;
    
    /// <summary>
    /// Static constructor for ImportResult
    /// </summary>
    static ImportResult()
    {
        _empty = new ImportResult();
    }
        
    /// <summary>
    /// Creates an ImportResult
    /// </summary>
    internal ImportResult()
    {
        NewTransactionIds = new List<uint>();
        NewGroupIds = new List<uint>();
        NewTags = new List<string>();
    }

    /// <summary>
    /// Adds a list of tags to the new tags
    /// </summary>
    /// <param name="tags">IEnumerable</param>
    /// <remarks>Will only add non-existing tags. Existing tags will just be skipped to avoid duplicates</remarks>
    public void AddTags(IEnumerable<string> tags) => NewTags.AddRange(tags.Where(t => !NewTags.Contains(t)));
}