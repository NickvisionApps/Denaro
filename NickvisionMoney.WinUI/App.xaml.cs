using Microsoft.UI.Xaml;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Views;
using System;

namespace NickvisionMoney.WinUI;

public partial class App : Application
{
    private Window? _mainWindow;
    private MainWindowController _mainWindowController;

    public App()
    {
        InitializeComponent();
        _mainWindowController = new MainWindowController();
        //AppInfo
        _mainWindowController.AppInfo.ID = "org.nickvision.money";
        _mainWindowController.AppInfo.Name = "Nickvision Money";
        _mainWindowController.AppInfo.ShortName = "Money";
        _mainWindowController.AppInfo.Description = _mainWindowController.Localizer["Description"];
        _mainWindowController.AppInfo.Version = "2023.1.0-beta1";
        _mainWindowController.AppInfo.Changelog = "- Money has been completely rewritten in C#. Money should now be a lot more stable and responsive. With the C# rewrite, there is now a new version of Money available on Windows!";
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

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow(_mainWindowController);
        _mainWindow.Activate();
    }
}