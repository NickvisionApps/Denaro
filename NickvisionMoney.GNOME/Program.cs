using NickvisionMoney.GNOME.Views;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

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
    public static int Main(string[] args) => new Program(args).Run(args);

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program(string[] args)
    {
        if (CultureInfo.CurrentCulture.Equals(CultureInfo.InvariantCulture))
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US"); // Fix #465
        }
        else if (CultureInfo.CurrentCulture.ToString() == "ar-RG")
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG"); // Fix #211
        }
        _application = Adw.Application.New("org.nickvision.money", Gio.ApplicationFlags.NonUnique);
        _mainWindow = null;
        _mainWindowController = new MainWindowController(args);
        _mainWindowController.AppInfo.Changelog =
            @"* Fixed an issue where the generated ids of new transactions were incorrect.
              * Fixed an issue that causes sort to behave inconsistently.
              * Fixed calendar not showing marks for transactions after pressing the ""Today"" button.
              * Updated and added translations (Thanks to everyone on Weblate)!";
        _application.OnActivate += OnActivate;
        if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.money.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.money.gresource"));
        }
        else
        {
            var prefixes = new List<string> {
               Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
               Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
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
    /// <param name="args">string[]</param>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run(string[] args)
    {
        try
        {
            var argv = new string[args.Length + 1];
            argv[0] = "org.nickvision.money";
            args.CopyTo(argv, 1);
            return _application.RunWithSynchronizationContext(argv);
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
}
