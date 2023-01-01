using NickvisionMoney.GNOME.Views;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME;

/// <summary>
/// The Program
/// </summary>
public partial class Program
{
    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_resource_load(string path);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_resources_register(nint file);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    private readonly Adw.Application _application;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">string[]</param>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public static int Main(string[] args) => new Program().Run();

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program()
    {
        Adw.Module.Initialize();
        _application = Adw.Application.New("org.nickvision.money", Gio.ApplicationFlags.HandlesOpen);
        _mainWindow = null;
        _application.OnActivate += OnActivate;
        _application.OnOpen += OnOpen;
        var prefixes = new List<string> {
            Directory.GetParent(Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
            Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
            "/usr"
        };
        foreach(var prefix in prefixes)
        {
            if(File.Exists(prefix + "/share/org.nickvision.money/org.nickvision.money.gresource"))
            {
                g_resources_register(g_resource_load(Path.GetFullPath(prefix + "/share/org.nickvision.money/org.nickvision.money.gresource")));
                break;
            }
        }
    }

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run() => _application.Run();

    /// <summary>
    /// Occurs when the application is activated
    /// </summary>
    /// <param name="sedner">Gio.Application</param>
    /// <param name="e">EventArgs</param>
    private void OnActivate(Gio.Application sedner, EventArgs e)
    {
        //Controller Setup
        var mainWindowController = new MainWindowController();
        mainWindowController.AppInfo.ID = "org.nickvision.money";
        mainWindowController.AppInfo.Name = "Nickvision Money";
        mainWindowController.AppInfo.ShortName = "Money";
        mainWindowController.AppInfo.Description = mainWindowController.Localizer["Description"];
        mainWindowController.AppInfo.Version = "2023.1.0-beta2";
        mainWindowController.AppInfo.Changelog = "<ul><li>Money has been completely rewritten in C#. Money should now be a lot more stable and responsive. With the C# rewrite, there is now a new version of Money available on Windows!</li><li>Added an \"Ungrouped\" row to the groups section to allow filtering transactions that don't belong to a group</li><li>Added the ability to attach a jpg/pdf of a receipt to a transaction</li><li>Reworked the repeat transaction algorithm and added support for a biweekly interval</li><li>Added the ability to hide the groups section</li><li>Made a group's description an optional field</li></ul>";
        mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/nlogozzo/NickvisionMoney");
        mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/nlogozzo/NickvisionMoney/issues/new");
        mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/nlogozzo/NickvisionMoney/discussions");
        //Set Adw Theme
        _application.StyleManager!.ColorScheme = mainWindowController.Theme switch
        {
            Theme.System => Adw.ColorScheme.PreferLight,
            Theme.Light => Adw.ColorScheme.ForceLight,
            Theme.Dark => Adw.ColorScheme.ForceDark,
            _ => Adw.ColorScheme.PreferLight
        };
        //Main Window
        _mainWindow = new MainWindow(mainWindowController, _application);
        _application.AddWindow(_mainWindow);
        _mainWindow.Start();
    }

    /// <summary>
    /// Occurs when an nmoney file is double clicked to open the app
    /// </summary>
    /// <param name="sender">Gio.Application</param>
    /// <param name="e">Gio.Application.OpenSignalArgs</param>
    private void OnOpen(Gio.Application sender, Gio.Application.OpenSignalArgs e)
    {
        if(e.NFiles > 0)
        {
            var pathOfFirstFile = g_file_get_path(e.Files[0].Handle);
            OnActivate(sender, EventArgs.Empty);
            _mainWindow!.OpenAccount(pathOfFirstFile);
        }
    }
}
