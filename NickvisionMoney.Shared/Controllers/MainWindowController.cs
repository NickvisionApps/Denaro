using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The list of open accounts
    /// </summary>
    public List<AccountViewController> OpenAccounts { get; init; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// A PreferencesViewController
    /// </summary>
    public PreferencesViewController PreferencesViewController => new PreferencesViewController(RecentAccountsChanged, Localizer);
    /// <summary>
    /// Whether or not the version is a development version or not
    /// </summary>
    public bool IsDevVersion => AppInfo.Current.Version.IndexOf('-') != -1;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;
    /// <summary>
    /// The list of recent accounts
    /// </summary>
    public List<RecentAccount> RecentAccounts => Configuration.Current.RecentAccounts;

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    public event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when an account is added
    /// </summary>
    public event EventHandler? AccountAdded;
    /// <summary>
    /// Occurs when the recent accounts list is changed
    /// </summary>
    public event EventHandler? RecentAccountsChanged;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
        _disposed = false;
        OpenAccounts = new List<AccountViewController>();
        Localizer = new Localizer();
    }

    /// <summary>
    /// Whether or not to show a sun icon on the home page
    /// </summary>
    public bool ShowSun
    {
        get
        {
            var timeNowHours = DateTime.Now.Hour;
            return timeNowHours >= 6 && timeNowHours < 18;
        }
    }

    /// <summary>
    /// The string for greeting on the home page
    /// </summary>
    public string Greeting
    {
        get
        {
            var timeNowHours = DateTime.Now.Hour;
            if (timeNowHours >= 0 && timeNowHours < 6)
            {
                return Localizer["Greeting", "Night"];
            }
            else if (timeNowHours >= 6 && timeNowHours < 12)
            {
                return Localizer["Greeting", "Morning"];
            }
            else if (timeNowHours >= 12 && timeNowHours < 18)
            {
                return Localizer["Greeting", "Afternoon"];
            }
            else if (timeNowHours >= 18 && timeNowHours < 24)
            {
                return Localizer["Greeting", "Evening"];
            }
            else
            {
                return Localizer["Greeting", "Generic"];
            }
        }
    }

    /// <summary>
    /// Frees resources used by the Account object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the Account object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Localizer.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// Gets a color for an account type
    /// </summary>
    /// <param name="accountType">The account type</param>
    /// <returns>The rgb color for the account type</returns>
    public string GetColorForAccountType(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Checking => Configuration.Current.AccountCheckingColor,
            AccountType.Savings => Configuration.Current.AccountSavingsColor,
            AccountType.Business => Configuration.Current.AccountBusinessColor,
            _ => Configuration.Current.AccountSavingsColor
        };
    }

    /// <summary>
    /// Gets whether or not an account with the given path is opened or not
    /// </summary>
    /// <param name="path">The path of the account to check</param>
    /// <returns>True if the account is open, else false</returns>
    public bool IsAccountOpen(string path) => OpenAccounts.Any(x => x.AccountPath == path);

    /// <summary>
    /// Adds an account to the list of opened accounts
    /// </summary>
    /// <param name="path">The path of the account</param>
    /// <param name="showOpenedNotification">Whether or not to show a notification if an account is opened</param>
    /// <returns>True if account added, else false (account already added)</returns>
    public bool AddAccount(string path, bool showOpenedNotification = true)
    {
        if(Path.GetExtension(path) != ".nmoney")
        {
            path += ".nmoney";
        }
        if(!OpenAccounts.Any(x => x.AccountPath == path))
        {
            var controller = new AccountViewController(path, Localizer, NotificationSent, RecentAccountsChanged);
            controller.TransferSent += OnTransferSent;
            OpenAccounts.Add(controller);
            Configuration.Current.AddRecentAccount(new RecentAccount(path)
            {
                Name = controller.AccountTitle,
                Type = controller.AccountType
            });
            Configuration.Current.Save();
            RecentAccountsChanged?.Invoke(this, EventArgs.Empty);
            AccountAdded?.Invoke(this, EventArgs.Empty);
            return true;
        }
        else
        {
            if(showOpenedNotification)
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["AccountOpenedAlready"], NotificationSeverity.Warning));
            }
            return false;
        }
    }

    /// <summary>
    /// Closes the account with the provided index
    /// </summary>
    /// <param name="index">int</param>
    public void CloseAccount(int index) => OpenAccounts.RemoveAt(index);

    /// <summary>
    /// Occurs when a transfer is sent from an account
    /// </summary>
    /// <param name="transfer">The transfer sent</param>
    private async void OnTransferSent(object? sender, Transfer transfer)
    {
        AddAccount(transfer.DestinationAccountPath, false);
        var controller = OpenAccounts.Find(x => x.AccountPath == transfer.DestinationAccountPath)!;
        await controller.ReceiveTransferAsync(transfer);
    }
}