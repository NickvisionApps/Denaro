using NickvisionMoney.GNOME.Views;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME;

/// <summary>
/// The Program
/// </summary>
public partial class Program
{
    private readonly Adw.Application _application;
    private MainWindow? _mainWindow;
    private MainWindowController _mainWindowController;

    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">string[]</param>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public static int Main(string[] args) => new Program().Run(args);

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program()
    {
        if (CultureInfo.CurrentCulture.Equals(CultureInfo.InvariantCulture))
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US"); // Fix #465
        }
        else if (CultureInfo.CurrentCulture.ToString() == "ar-RG")
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG"); // Fix #211
        }
        _application = Adw.Application.New("org.nickvision.money", Gio.ApplicationFlags.HandlesOpen | Gio.ApplicationFlags.NonUnique);
        _mainWindow = null;
        _mainWindowController = new MainWindowController();
        _mainWindowController.AppInfo.ID = "org.nickvision.money";
        _mainWindowController.AppInfo.Name = "Nickvision Denaro";
        _mainWindowController.AppInfo.ShortName = _("Denaro");
        _mainWindowController.AppInfo.Description = $"{_("Manage your personal finances")}.";
        _mainWindowController.AppInfo.Version = "2023.8.0-next";
        _mainWindowController.AppInfo.Changelog = "<ul><li>Fixed an issue where the help button in the import toast was not working</li><li>Fixed an issue where Denaro would crash if an account had incorrect formatted metadata</li><li>Updated and added translations (Thanks to everyone on Weblate)!</li></ul>";
        _mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/NickvisionApps/Denaro");
        _mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/NickvisionApps/Denaro/issues/new");
        _mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/NickvisionApps/Denaro/discussions");
        _application.OnActivate += OnActivate;
        _application.OnOpen += OnOpen;
        if (File.Exists(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.money.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.money.gresource"));
        }
        else
        {
            var prefixes = new List<string> {
               Directory.GetParent(Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
               Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
               "/usr"
            };
            foreach (var prefix in prefixes)
            {
                if (File.Exists(prefix + "/share/org.nickvision.money/org.nickvision.money.gresource"))
                {
                    Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(prefix + "/share/org.nickvision.money/org.nickvision.money.gresource")));
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run(string[] args)
    {
        try
        {
            return _application.RunWithSynchronizationContext();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine($"\n\n{ex.StackTrace}");
            return -1;
        }
    }

    /// <summary>
    /// Occurs when the application is activated
    /// </summary>
    /// <param name="sender">Gio.Application</param>
    /// <param name="e">EventArgs</param>
    private async void OnActivate(Gio.Application sender, EventArgs e)
    {
        //Set Adw Theme
        _application.StyleManager!.ColorScheme = _mainWindowController.Theme switch
        {
            Theme.System => Adw.ColorScheme.PreferLight,
            Theme.Light => Adw.ColorScheme.ForceLight,
            Theme.Dark => Adw.ColorScheme.ForceDark,
            _ => Adw.ColorScheme.PreferLight
        };
        //Main Window
        _mainWindow = new MainWindow(_mainWindowController, _application);
        await _mainWindow.StartupAsync();
    }

    /// <summary>
    /// Occurs when an nmoney file is double clicked to open the app
    /// </summary>
    /// <param name="sender">Gio.Application</param>
    /// <param name="e">Gio.Application.OpenSignalArgs</param>
    private void OnOpen(Gio.Application sender, Gio.Application.OpenSignalArgs e)
    {
        if (e.NFiles > 0)
        {
            _mainWindowController.FileToLaunch = e.Files[0].GetPath();
            OnActivate(_application, EventArgs.Empty);
        }
    }
}
