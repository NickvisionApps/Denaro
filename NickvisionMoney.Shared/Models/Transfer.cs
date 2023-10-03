using System.IO;

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
    /// The name of the source account
    /// </summary>
    public string SourceAccountName { get; set; }
    /// <summary>
    /// The amount to transfer from the source account
    /// </summary>
    public decimal SourceAmount { get; set; }
    /// <summary>
    /// The path to the destination account
    /// </summary>
    public string DestinationAccountPath { get; set; }
    /// <summary>
    /// The name of the destination account
    /// </summary>
    public string DestinationAccountName { get; set; }
    /// <summary>
    /// The password for the destination account (if needed)
    /// </summary>
    public string? DestinationAccountPassword { get; set; }
    /// <summary>
    /// The rate of converting the source amount to the destination amount
    /// </summary>
    public decimal ConversionRate { get; set; }

    /// <summary>
    /// The amount to transfer from the destination account
    /// </summary>
    public decimal DestinationAmount => SourceAmount / ConversionRate;

    /// <summary>
    /// Constructs a Transfer
    /// </summary>
    /// <param name="sourceAccountPath">The path to the source account</param>
    /// <param name="sourceAccountName">The name of the source account</param>
    public Transfer(string sourceAccountPath, string? sourceAccountName = null)
    {
        SourceAccountPath = sourceAccountPath;
        SourceAccountName = sourceAccountName ?? Path.GetFileNameWithoutExtension(sourceAccountPath);
        SourceAmount = 0m;
        DestinationAccountPath = "";
        DestinationAccountName = "";
        DestinationAccountPassword = null;
        ConversionRate = 1.0m;
    }
}
