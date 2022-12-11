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
        Localizer = new Localizer();
        FolderPath = "No Folder Opened";
    }

    /// <summary>
    /// Runs startup functions
    /// </summary>
    public void Startup()
    {
        if (!_isOpened)
        {
            _isOpened = false;
        }
    }

    /// <summary>
    /// Opens a folder
    /// </summary>
    /// <param name="folderPath">The path of the folder to open</param>
    /// <returns>True if folder opened, else false</returns>
    public bool OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            FolderPath = folderPath;
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(string.Format(Localizer["FolderOpened"], FolderPath), NotificationSeverity.Success));
            FolderChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Closes the folder
    /// </summary>
    public void CloseFolder()
    {
        FolderPath = "No Folder Opened";
        NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["FolderClosed"], NotificationSeverity.Warning));
        FolderChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<List<string>?> GetFilesInFolderAsync()
    {
        if (IsFolderOpened)
        {
            var files = new List<string>();
            await Task.Run(() =>
            {
                foreach (var path in Directory.EnumerateFiles(FolderPath, "*", SearchOption.AllDirectories))
                {
                    files.Add(Path.GetFileName(path));
                }
            });
            return files;
        }
        return null;
    }

    /// <summary>
    /// Get the string for greeting on the start screen
    /// </summary>
    public string WelcomeMessage
    {
    	get
    	{
	        int timeNowHours = DateTime.Now.Hour;
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
}