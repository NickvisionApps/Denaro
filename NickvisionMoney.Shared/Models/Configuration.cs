using Nickvision.Aura;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// Decimal Separator Inserting
/// </summary>
public enum InsertSeparator
{
    Off = 0,
    NumpadOnly,
    PeriodComma
}

/// <summary>
/// A model for the configuration of the application
/// </summary>
public class Configuration : ConfigurationBase
{
    /// <summary>
    /// The preferred theme for the application
    /// </summary>
    public Theme Theme { get; set; }
    /// <summary>
    /// The first recent account
    /// </summary>
    [JsonInclude]
    public RecentAccount RecentAccount1 { get; private set; }
    /// <summary>
    /// The second recent account
    /// </summary>
    [JsonInclude]
    public RecentAccount RecentAccount2 { get; private set; }
    /// <summary>
    /// The third recent account
    /// </summary>
    [JsonInclude]
    public RecentAccount RecentAccount3 { get; private set; }
    /// <summary>
    /// The default color of a transaction
    /// </summary>
    public string TransactionDefaultColor { get; set; }
    /// <summary>
    /// The default color of a transfer
    /// </summary>
    public string TransferDefaultColor { get; set; }
    /// <summary>
    /// The default color of a group
    /// </summary>
    public string GroupDefaultColor { get; set; }
    /// <summary>
    /// The color of accounts with Checking type
    /// </summary>
    public string AccountCheckingColor { get; set; }
    /// <summary>
    /// The color of accounts with Savings type
    /// </summary>
    public string AccountSavingsColor { get; set; }
    /// <summary>
    /// The color of accounts with Business type
    /// </summary>
    public string AccountBusinessColor { get; set; }
    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits { get; set; }
    /// <summary>
    /// Decimal Separator Inserting
    /// </summary>
    public InsertSeparator InsertSeparator { get; set; }
    /// <summary>
    /// A folder to use to backup accounts as CSV
    /// </summary>
    public string CSVBackupFolder { get; set; }

    /// <summary>
    /// Constructs a Configuration
    /// </summary>
    public Configuration()
    {
        Theme = Theme.System;
        RecentAccount1 = new RecentAccount();
        RecentAccount2 = new RecentAccount();
        RecentAccount3 = new RecentAccount();
        TransactionDefaultColor = "rgb(53,132,228)";
        TransferDefaultColor = "rgb(192,97,203)";
        GroupDefaultColor = "rgb(51,209,122)";
        AccountCheckingColor = "rgb(129,61,156)";
        AccountSavingsColor = "rgb(53,132,228)";
        AccountBusinessColor = "rgb(38,162,105)";
        UseNativeDigits = true;
        InsertSeparator = InsertSeparator.NumpadOnly;
        CSVBackupFolder = "";
    }

    /// <summary>
    /// Gets the singleton object
    /// </summary>
    internal static Configuration Current => (Configuration)Aura.Active.ConfigFiles["config"];

    /// <summary>
    /// Gets the list of recent accounts available
    /// </summary>
    [JsonIgnore]
    public List<RecentAccount> RecentAccounts
    {
        get
        {
            var recents = new List<RecentAccount>();
            var update = false;
            if (File.Exists(RecentAccount1.Path))
            {
                recents.Add(RecentAccount1);
            }
            else
            {
                update = true;
            }
            if (File.Exists(RecentAccount2.Path))
            {
                recents.Add(RecentAccount2);
            }
            else
            {
                update = true;
            }
            if (File.Exists(RecentAccount3.Path))
            {
                recents.Add(RecentAccount3);
            }
            else
            {
                update = true;
            }
            if (update)
            {
                if (recents.Count == 0)
                {
                    RecentAccount1 = new RecentAccount();
                    RecentAccount2 = new RecentAccount();
                    RecentAccount3 = new RecentAccount();
                }
                else if (recents.Count == 1)
                {
                    RecentAccount1 = recents[0];
                    RecentAccount2 = new RecentAccount();
                    RecentAccount3 = new RecentAccount();
                }
                else if (recents.Count == 2)
                {
                    RecentAccount1 = recents[0];
                    RecentAccount2 = recents[1];
                    RecentAccount3 = new RecentAccount();
                }
                Aura.Active.SaveConfig("config");
            }
            return recents;
        }
    }

    /// <summary>
    /// Adds a recent account
    /// </summary>
    /// <param name="newRecentAccount">The new recent account</param>
    public void AddRecentAccount(RecentAccount newRecentAccount)
    {
        if (newRecentAccount == RecentAccount1)
        {
            RecentAccount1.Name = newRecentAccount.Name;
            RecentAccount1.Type = newRecentAccount.Type;
        }
        else if (newRecentAccount == RecentAccount2)
        {
            var temp = RecentAccount1;
            RecentAccount1 = RecentAccount2;
            RecentAccount2 = temp;
            RecentAccount1.Name = newRecentAccount.Name;
            RecentAccount1.Type = newRecentAccount.Type;
        }
        else if (newRecentAccount == RecentAccount3)
        {
            var temp1 = RecentAccount1;
            var temp2 = RecentAccount2;
            RecentAccount1 = RecentAccount3;
            RecentAccount2 = temp1;
            RecentAccount3 = temp2;
            RecentAccount1.Name = newRecentAccount.Name;
            RecentAccount1.Type = newRecentAccount.Type;
        }
        else
        {
            RecentAccount3 = RecentAccount2;
            RecentAccount2 = RecentAccount1;
            RecentAccount1 = newRecentAccount;
        }
    }

    /// <summary>
    /// Removes a recent account
    /// </summary>
    /// <param name="recentAccount">The RecentAccount to remove</param>
    public void RemoveRecentAccount(RecentAccount recentAccount)
    {
        var ra1 = RecentAccount1;
        var ra2 = RecentAccount2;
        var ra3 = RecentAccount3;
        RecentAccount1 = new RecentAccount();
        RecentAccount2 = new RecentAccount();
        RecentAccount3 = new RecentAccount();
        if(ra3.Path != recentAccount.Path)
        {
            AddRecentAccount(ra3);
        }
        if(ra2.Path != recentAccount.Path)
        {
            AddRecentAccount(ra2);
        }
        if(ra1.Path != recentAccount.Path)
        {
            AddRecentAccount(ra1);
        }
    }
}