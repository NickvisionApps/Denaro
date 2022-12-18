using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The list of open accounts
    /// </summary>
    public List<string> OpenAccounts { get; init; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// A PreferencesViewController
    /// </summary>
    public PreferencesViewController PreferencesViewController => new PreferencesViewController(Localizer);
    /// <summary>
    /// Whether or not the version is a development version or not
    /// </summary>
    public bool IsDevVersion => AppInfo.Current.Version.IndexOf('-') != -1;
    /// <summary>
    /// The prefered theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;
    /// <summary>
    /// The list of recent accounts
    /// </summary>
    public List<string> RecentAccounts => Configuration.Current.RecentAccounts;

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
        OpenAccounts = new List<string>();
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
    /// Creates a new AccountViewController
    /// </summary>
    /// <param name="index">The index of the open account to base the controller on</param>
    /// <returns>The AccountViewController for the open account</returns>
    public AccountViewController CreateAccountController(int index) => new AccountViewController(OpenAccounts[index], Localizer, NotificationSent);

    /// <summary>
    /// Adds an account to the list of opened accounts
    /// </summary>
    /// <param name="path">string</param>
    public void AddAccount(string path)
    {
        if(Path.GetExtension(path) != ".nmoney")
        {
            path += ".nmoney";
        }
        if(!OpenAccounts.Contains(path))
        {
            OpenAccounts.Add(path);
            Configuration.Current.AddRecentAccount(path);
            Configuration.Current.Save();
            AccountAdded?.Invoke(this, EventArgs.Empty);
            RecentAccountsChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["AccountOpenedAlready"], NotificationSeverity.Warning));
        }
    }

    /// <summary>
    /// Closes the account with the provided index
    /// </summary>
    /// <param name="index">int</param>
    public void CloseAccount(int index) => OpenAccounts.RemoveAt(index);
}