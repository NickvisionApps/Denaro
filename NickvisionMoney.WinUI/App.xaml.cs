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
        _mainWindowController.AppInfo.ShortName = _mainWindowController.Localizer["ShortName"];
        _mainWindowController.AppInfo.Description = $"{_mainWindowController.Localizer["Description"]}.";
        _mainWindowController.AppInfo.Version = "2023.5.0-next";
        _mainWindowController.AppInfo.Changelog = "- Fixed an issue where Denaro would crash on systems with unconfigured locales\n- Fixed an issue where an account would not load when opening one using hex colors\n- Updated and added translations (Thanks to everyone on Weblate)!";
        _mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/NickvisionApps/Denaro");
        _mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/NickvisionApps/Denaro/issues/new");
        _mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/NickvisionApps/Denaro/discussions");
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