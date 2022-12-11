using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    private bool _isOpened;
    private List<string> _openAccounts;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The path of the folder opened
    /// </summary>
    public string FolderPath { get; private set; }
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
    /// Whether or not the folder is opened
    /// </summary>
    public bool IsFolderOpened => FolderPath != "No Folder Opened";

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    public event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when a folder is opened or closed
    /// </summary>
    public event EventHandler? FolderChanged;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
        _isOpened = false;
        _openAccounts = new List<string> {};
        Localizer = new Localizer();
        FolderPath = "No Folder Opened";
    }

    /// <summary>
    /// Get the string for greeting on the start screen
    /// </summary>
    public string WelcomeMessage
    {
        get
        {
            var timeNowHours = DateTime.Now.Hour;
            if(timeNowHours >= 0 && timeNowHours < 6)
            {
                return Localizer["GreetingNight"];
            }
            else if(timeNowHours >= 6 && timeNowHours < 12)
            {
                return Localizer["GreetingMorning"];
            }
            else if(timeNowHours >= 12 && timeNowHours < 18)
            {
                return Localizer["GreetingDay"];
            }
            else if(timeNowHours >= 18 && timeNowHours < 24)
            {
                return Localizer["GreetingEvening"];
            }
            else
            {
                return Localizer["Greeting"];
            }
        }
    }

    /// <summary>
    /// Runs startup functions
    /// </summary>
    public void Startup()
    {
        if(!_isOpened)
        {
            _isOpened = false;
        }
    }

    public void AddAccount(string path)
    {
        if(Path.GetExtension(path) != ".nmoney")
        {
            path += ".nmoney";
        }
        if(!_openAccounts.Contains(path))
        {
            _openAccounts.Add(path);
            Configuration.Current.AddRecentAccount(path);
            Configuration.Current.Save();
        }
    }

    public void CloseAccount(int index) => _openAccounts.RemoveAt(index);

    public int GetNumberOfOpenAccounts() => _openAccounts.Count;

    public string GetFirstOpenAccountPath() => _openAccounts[0];
}