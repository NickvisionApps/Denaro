﻿using NickvisionMoney.GNOME.Views;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME;

/// <summary>
/// The Program
/// </summary>
public partial class Program
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nuint gtk_file_chooser_cell_get_type();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_resource_load(string path);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_resources_register(nint file);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    private delegate void OpenCallback(nint application, nint[] files, int n_files, nint hint, nint data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string signal, OpenCallback callback, nint data, nint destroy_data, int flags);

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
        gtk_file_chooser_cell_get_type();
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
        _mainWindowController.AppInfo.Version = "2023.6.0-beta1";
        _mainWindowController.AppInfo.Changelog = "<ul><li>Added a new account setup dialog to make it easier to configure new accounts</li><li>Added the ability to remove recent accounts from the list</li><li>Moved deleting groups and transactions from their rows to their dialogs</li><li>Changed the default sorting order of new accounts to last to first by date</li><li>Fixed an issue importing CSV files</li><li>Improved UI/UX</li><li>Updated and added translations (Thanks to everyone on Weblate)!</li></ul>";
        _mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/NickvisionApps/Denaro");
        _mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/NickvisionApps/Denaro/issues/new");
        _mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/NickvisionApps/Denaro/discussions");
        _application.OnActivate += OnActivate;
        g_signal_connect_data(_application.Handle, "open", OnOpen, IntPtr.Zero, IntPtr.Zero, 0);
        if (File.Exists(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.money.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            g_resources_register(g_resource_load(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.money.gresource"));
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
                    g_resources_register(g_resource_load(Path.GetFullPath(prefix + "/share/org.nickvision.money/org.nickvision.money.gresource")));
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
            var argv = new string[args.Length + 1];
            argv[0] = "NickvisionMoney.GNOME";
            args.CopyTo(argv, 1);
            return _application.Run(args.Length + 1, argv);
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
    private void OnActivate(Gio.Application sender, EventArgs e)
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
        _mainWindow.Startup();
    }

    /// <summary>
    /// Occurs when an nmoney file is double clicked to open the app
    /// </summary>
    /// <param name="sender">Gio.Application</param>
    /// <param name="e">Gio.Application.OpenSignalArgs</param>
    private void OnOpen(nint application, nint[] files, int n_files, nint hint, nint data)
    {
        if (n_files > 0)
        {
            _mainWindowController.FileToLaunch = g_file_get_path(files[0]);
            OnActivate(_application, EventArgs.Empty);
        }
    }
}
