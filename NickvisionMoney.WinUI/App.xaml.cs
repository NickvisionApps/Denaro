using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Views;
using System;

namespace NickvisionMoney.WinUI;

/// <summary>
/// The App
/// </summary>
public partial class App : Application
{
    public static Window? MainWindow { get; private set; } = null;
    private readonly MainWindowController _mainWindowController;

    /// <summary>
    /// Constructs an App
    /// </summary>
    public App()
    {
        InitializeComponent();
        _mainWindowController = new MainWindowController();
        //AppInfo
        _mainWindowController.AppInfo.ID = "org.nickvision.money";
        _mainWindowController.AppInfo.Name = "Nickvision Denaro";
        _mainWindowController.AppInfo.ShortName = "Denaro";
        _mainWindowController.AppInfo.Description = $"{_mainWindowController.Localizer["Description"]}.";
        _mainWindowController.AppInfo.Version = "2023.4.0-beta1";
        _mainWindowController.AppInfo.Changelog = "- Added a new Dashboard page to view quick information about all accounts\n- Added the ability to add a color to groups\n- Added the ability to customize the decimal and group separators used per account\n- Added the ability to password protect a PDF file\n- Added a preference option to automatically backup account files to a specific folder\n- Redesigned the AccountView for a more fluent design\n- Fixed an issue where OFX files with security could not be imported\n- Fixed an issue where QIF files could not be imported on non-English systems\n- Updated and added translations (Thanks to everyone on Weblate)!";
        _mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/nlogozzo/NickvisionMoney");
        _mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/nlogozzo/NickvisionMoney/issues/new");
        _mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/nlogozzo/NickvisionMoney/discussions");
        //Theme
        if (_mainWindowController.Theme == Theme.Light)
        {
            RequestedTheme = ApplicationTheme.Light;
        }
        else if (_mainWindowController.Theme == Theme.Dark)
        {
            RequestedTheme = ApplicationTheme.Dark;
        }
    }

    /// <summary>
    /// Occurs when the app is launched
    /// </summary>
    /// <param name="args">LaunchActivatedEventArgs</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        if (activatedArgs.Kind == ExtendedActivationKind.File)
        {
            var fileArgs = (Windows.ApplicationModel.Activation.IFileActivatedEventArgs)activatedArgs.Data;
            _mainWindowController.FileToLaunch = fileArgs.Files[0].Path;
        }
        MainWindow = new MainWindow(_mainWindowController);
        MainWindow.Activate();
    }
}