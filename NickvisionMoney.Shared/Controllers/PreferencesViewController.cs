using NickvisionMoney.Shared.Models;
using System;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a PreferencesView
/// </summary>
public class PreferencesViewController
{
    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;

    /// <summary>
    /// Creates a PreferencesViewController
    /// </summary>
    internal PreferencesViewController()
    {

    }

    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme
    {
        get => Configuration.Current.Theme;

        set => Configuration.Current.Theme = value;
    }

    /// <summary>
    /// The default color of a transaction
    /// </summary>
    public string TransactionDefaultColor
    {
        get => Configuration.Current.TransactionDefaultColor;

        set => Configuration.Current.TransactionDefaultColor = value;
    }

    /// <summary>
    /// The default color of a transfer
    /// </summary>
    public string TransferDefaultColor
    {
        get => Configuration.Current.TransferDefaultColor;

        set => Configuration.Current.TransferDefaultColor = value;
    }

    /// <summary>
    /// The default color of a group
    /// </summary>
    public string GroupDefaultColor
    {
        get => Configuration.Current.GroupDefaultColor;

        set => Configuration.Current.GroupDefaultColor = value;
    }

    /// <summary>
    /// The color of accounts with Checking type
    /// </summary>
    public string AccountCheckingColor
    {
        get => Configuration.Current.AccountCheckingColor;

        set => Configuration.Current.AccountCheckingColor = value;
    }

    /// <summary>
    /// The color of accounts with Savings type
    /// </summary>
    public string AccountSavingsColor
    {
        get => Configuration.Current.AccountSavingsColor;

        set => Configuration.Current.AccountSavingsColor = value;
    }

    /// <summary>
    /// The color of accounts with Business type
    /// </summary>
    public string AccountBusinessColor
    {
        get => Configuration.Current.AccountBusinessColor;

        set => Configuration.Current.AccountBusinessColor = value;
    }

    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits
    {
        get => Configuration.Current.UseNativeDigits;

        set => Configuration.Current.UseNativeDigits = value;
    }

    /// <summary>
    /// Decimal Separator Inserting
    /// <summary>
    public InsertSeparator InsertSeparator
    {
        get => Configuration.Current.InsertSeparator;

        set => Configuration.Current.InsertSeparator = value;
    }

    /// <summary>
    /// A folder to use to backup accounts as CSV
    /// </summary>
    public string CSVBackupFolder
    {
        get => Configuration.Current.CSVBackupFolder;

        set => Configuration.Current.CSVBackupFolder = value;
    }

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    public void SaveConfiguration() => Configuration.Current.Save();
}