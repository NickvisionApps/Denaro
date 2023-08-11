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
    public static int Main(string[] args) => new Program(args).Run();

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
            @"* Added graph visuals to the Account view as well as in exported PDFs
              * Added tags that can be added to transactions for better management and filtering
              * Added the option to select the entire current month as the date range filter
              * Improved the transaction description suggestion algorithm with fuzzy search
              * Fixed an issue where the help button in the import toast was not working
              * Fixed an issue where Denaro would crash if an account had incorrect formatted metadata
              * Fixed an issue where docs were not available when running Denaro via snap
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
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run()
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
}
