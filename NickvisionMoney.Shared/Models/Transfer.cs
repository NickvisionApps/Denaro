namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of a transfer
/// </summary>
public class Transfer
{
    /// <summary>
    /// The path to the source account
    /// </summary>
    public string SourceAccountPath { get; init; }
    /// <summary>
    /// The path to the destination account
    /// </summary>
    public string DestinationAccountPath { get; set; }
    /// <summary>
    /// The amount of the transfer
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// The name of the source account
    /// </summary>
    public string SourceAccountName { get; set; }

    /// <summary>
    /// Constructs a Tansfer
    /// </summary>
    /// <param name="sourceAccountPath">The path to the source account</param>
    public Transfer(string sourceAccountPath)
    {
        SourceAccountPath = sourceAccountPath;
        DestinationAccountPath = "";
        Amount = 0m;
    }
}
