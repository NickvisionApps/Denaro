using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// Decimal Separator Inserting
/// <summary>
public enum InsertSeparator
{
    Off = 0,
    NumpadOnly,
    PeriodComma
}

/// <summary>
/// A model for the configuration of the application
/// </summary>
public class Configuration
{
    public static readonly string ConfigDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Current.Name}";
    private static readonly string ConfigPath = $"{ConfigDir}{Path.DirectorySeparatorChar}config.json";
    private static Configuration? _instance;

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
    /// Decimal Separator Inserting
    /// <summary>
    public InsertSeparator InsertSeparator { get; set; }
    /// <summary>
    /// A folder to use to backup accounts as CSV
    /// </summary>
    public string CSVBackupFolder { get; set; }

    /// <summary>
    /// Occurs when the configuration is saved to disk
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Constructs a Configuration
    /// </summary>
    public Configuration()
    {
        if (!Directory.Exists(ConfigDir))
        {
            Directory.CreateDirectory(ConfigDir);
        }
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
        InsertSeparator = InsertSeparator.NumpadOnly;
        CSVBackupFolder = "";
    }

    /// <summary>
    /// Gets the singleton object
    /// </summary>
    internal static Configuration Current
    {
        get
        {
            if (_instance == null)
            {
                try
                {
                    _instance = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(ConfigPath)) ?? new Configuration();
                }
                catch
                {
                    _instance = new Configuration();
                }
            }
            return _instance;
        }
    }

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
                Save();
            }
            return recents;
        }
    }

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    public void Save()
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this));
        Saved?.Invoke(this, EventArgs.Empty);
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
            return;
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
}