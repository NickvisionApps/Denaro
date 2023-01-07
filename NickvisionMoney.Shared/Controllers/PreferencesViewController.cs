using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a PreferencesView
/// </summary>
public class PreferencesViewController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;

    /// <summary>
    /// Constructs a PreferencesViewController
    /// </summary>
    internal PreferencesViewController(Localizer localizer)
    {
        Localizer = localizer;
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
    /// Saves the configuration to disk
    /// </summary>
    public void SaveConfiguration() => Configuration.Current.Save();
}