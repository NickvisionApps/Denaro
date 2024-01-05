using Nickvision.Aura;
using Nickvision.Aura.Events;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController : IDisposable
{
    private bool _disposed;
    private string? _fileToLaunch;
    private List<AccountViewController> _openAccounts;

    /// <summary>
    /// A function for getting a password for an account
    /// </summary>
    public Func<string, Task<string?>>? AccountLoginAsync { get; set; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;
    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits => Configuration.Current.UseNativeDigits;
    /// <summary>
    /// The list of recent accounts
    /// </summary>
    public List<RecentAccount> RecentAccounts => Configuration.Current.RecentAccounts;
    /// <summary>
    /// The number of open accounts
    /// </summary>
    public int NumberOfOpenAccounts => _openAccounts.Count;

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    public event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when an account is added
    /// </summary>
    public event EventHandler<EventArgs>? AccountAdded;
    /// <summary>
    /// Occurs when the recent accounts list is changed
    /// </summary>
    public event EventHandler<EventArgs>? RecentAccountsChanged;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    public MainWindowController(string[] args)
    {
        _disposed = false;
        if (args.Length > 0)
        {
            _fileToLaunch = (Path.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() == ".nmoney") ? args[0] : null;
        }
        _openAccounts = new List<AccountViewController>();
        Aura.Init("org.nickvision.money", "Nickvision Denaro");
        AppInfo.EnglishShortName = "Denaro";
        if (Directory.Exists($"{UserDirectories.Config}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Name}"))
        {
            // Move config files from older versions and delete old directory
            try
            {
                foreach (var file in Directory.GetFiles($"{UserDirectories.Config}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Name}"))
                {
                    File.Move(file, $"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
                }
            }
            catch (IOException) { }
            Directory.Delete($"{UserDirectories.Config}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Name}", true);
        }
        AppInfo.Version = "2024.1.0";
        AppInfo.ShortName = _("Denaro");
        AppInfo.Description = _("Manage your personal finances");
        AppInfo.SourceRepo = new Uri("https://github.com/NickvisionApps/Denaro");
        AppInfo.IssueTracker = new Uri("https://github.com/NickvisionApps/Denaro/issues/new");
        AppInfo.SupportUrl = new Uri("https://github.com/NickvisionApps/Denaro/discussions");
        AppInfo.ExtraLinks[_("Matrix Chat")] = new Uri("https://matrix.to/#/#nickvision:matrix.org");
        AppInfo.Developers[_("Nicholas Logozzo")] = new Uri("https://github.com/nlogozzo");
        AppInfo.Developers[_("Contributors on GitHub ❤️")] = new Uri("https://github.com/NickvisionApps/Denaro/graphs/contributors");
        AppInfo.Designers[_("Nicholas Logozzo")] = new Uri("https://github.com/nlogozzo");
        AppInfo.Designers[_("Fyodor Sobolev")] = new Uri("https://github.com/fsobolev");
        AppInfo.Designers[_("DaPigGuy")] = new Uri("https://github.com/DaPigGuy");
        AppInfo.Artists[_("David Lapshin")] = new Uri("https://github.com/daudix-UFO");
        AppInfo.Artists[_("Tobias Bernard")] = new Uri("https://github.com/bertob");
        AppInfo.TranslatorCredits = _("translator-credits");
    }

    /// <summary>
    /// Finalizes the MainWindowController
    /// </summary>
    ~MainWindowController() => Dispose(false);

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
            return DateTime.Now.Hour switch
            {
                >= 0 and < 6 => _p("Night", "Good Morning!"),
                < 12 => _p("Morning", "Good Morning!"),
                < 18 => _("Good Afternoon!"),
                < 24 => _("Good Evening!"),
                _ => _("Good Day!")
            };
        }
    }

    /// <summary>
    /// Whether or not to show graphs
    /// </summary>
    public bool ShowGraphs
    {
        get => Configuration.Current.ShowGraphs;

        set
        {
            Configuration.Current.ShowGraphs = value;
            Configuration.Current.Save();
        }
    }

    /// <summary>
    /// Frees resources used by the MainWindowController object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the MainWindowController object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            foreach (var controller in _openAccounts)
            {
                controller.Dispose();
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController();

    /// <summary>
    /// Creates a new NewAccountDialogController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public NewAccountDialogController CreateNewAccountDialogController() => new NewAccountDialogController(_openAccounts.Select(x => x.AccountPath));

    /// <summary>
    /// Creates a new DashboardViewController
    /// </summary>
    /// <returns>The DashboardViewController</returns>
    public DashboardViewController CreateDashboardViewController() => new DashboardViewController(_openAccounts, Configuration.Current.GroupDefaultColor);

    /// <summary>
    /// Gets an AccountViewController for the most recent account
    /// </summary>
    /// <returns>The AccountViewController</returns>
    public AccountViewController GetMostRecentAccountViewController() => _openAccounts[_openAccounts.Count - 1];

    /// <summary>
    /// Starts the application
    /// </summary>
    public async Task StartupAsync()
    {
        if (_fileToLaunch != null)
        {
            await AddAccountAsync(_fileToLaunch);
            _fileToLaunch = null;
        }
    }


    /// <summary>
    /// Creates an AccountViewController for the specified path
    /// </summary>
    /// <param name="path">The path of the open account</param>
    /// <returns>The AccountViewController or null if the account path is not open</returns>
    public AccountViewController? CreateAccountViewController(string path)
    {
        try
        {
            return _openAccounts[_openAccounts.FindIndex(x => x.AccountPath == path)];
        }
        catch
        {
            return null;
        }
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
    public bool IsAccountOpen(string path) => _openAccounts.Any(x => x.AccountPath == path);

    /// <summary>
    /// Creates a new account and adds it to the list of opened accounts
    /// </summary>
    /// <param name="controller">NewAccountDialogController</param>
    /// <returns>True if new account created and opened, else false</returns>
    public async Task<bool> NewAccountAsync(NewAccountDialogController controller)
    {
        if (IsAccountOpen(controller.Path))
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to overwrite an opened account."), NotificationSeverity.Error));
            return false;
        }
        if (File.Exists(controller.Path))
        {
            if (controller.OverwriteExisting)
            {
                File.Delete(controller.Path);
            }
            else
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to overwrite an existing account."), NotificationSeverity.Error));
                return false;
            }
        }
        var accountViewController = new AccountViewController(controller.Path, NotificationSent, RecentAccountsChanged);
        accountViewController.TransferSent += OnTransferSent;
        try
        {
            accountViewController.Login(null);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to open the account. Please ensure that the app has permissions to access the file and try again."), NotificationSeverity.Error));
            return false;
        }
        _openAccounts.Add(accountViewController);
        AccountAdded?.Invoke(this, EventArgs.Empty);
        await Task.Delay(100);
        accountViewController.UpdateMetadata(controller.Metadata);
        if (!string.IsNullOrEmpty(controller.Password))
        {
            accountViewController.SetPassword(controller.Password, false);
        }
        if (File.Exists(controller.ImportFile))
        {
            await accountViewController.ImportFromFileAsync(controller.ImportFile);
        }
        return true;
    }

    /// <summary>
    /// Adds an account to the list of opened accounts
    /// </summary>
    /// <param name="path">The path of the account</param>
    /// <param name="showOpenedNotification">Whether or not to show a notification if an account is opened</param>
    /// <param name="password">A password for an account (if available)</param>
    /// <returns>True if account added, else false (account already added)</returns>
    public async Task<bool> AddAccountAsync(string path, bool showOpenedNotification = true, string? password = null)
    {
        if (Path.GetExtension(path).ToLower() != ".nmoney")
        {
            path += ".nmoney";
        }
        if (!_openAccounts.Any(x => x.AccountPath == path))
        {
            var controller = new AccountViewController(path, NotificationSent, RecentAccountsChanged);
            controller.TransferSent += OnTransferSent;
            try
            {
                if (controller.AccountNeedsPassword && string.IsNullOrEmpty(password))
                {
                    password = await AccountLoginAsync!(controller.AccountPath);
                }
                if (!controller.Login(password))
                {
                    controller.Dispose();
                    if (password != null)
                    {
                        NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to login to account. Provided password is invalid."), NotificationSeverity.Error));
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to open the account. Please ensure that the app has permissions to access the file and try again."), NotificationSeverity.Error));
                return false;
            }
            _openAccounts.Add(controller);
            AccountAdded?.Invoke(this, EventArgs.Empty);
            return true;
        }
        else
        {
            if (showOpenedNotification)
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("This account is already opened."), NotificationSeverity.Warning));
            }
            return false;
        }
    }

    /// <summary>
    /// Closes the account with the provided index
    /// </summary>
    /// <param name="index">int</param>
    public void CloseAccount(int index)
    {
        _openAccounts[index].Dispose();
        _openAccounts.RemoveAt(index);
    }

    /// <summary>
    /// Removes a recent account from the list
    /// </summary>
    /// <param name="recentAccount">The RecentAccount to remove</param>
    public void RemoveRecentAccount(RecentAccount recentAccount)
    {
        Configuration.Current.RemoveRecentAccount(recentAccount);
        Configuration.Current.Save();
        RecentAccountsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when a transfer is sent from an account
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="transfer">The transfer sent</param>
    private async void OnTransferSent(object? sender, Transfer transfer)
    {
        await AddAccountAsync(transfer.DestinationAccountPath, false, transfer.DestinationAccountPassword);
        var controller = _openAccounts.Find(x => x.AccountPath == transfer.DestinationAccountPath)!;
        while (!controller.IsOpened)
        {
            await Task.Delay(200);
        }
        await controller.ReceiveTransferAsync(transfer);
    }
}
