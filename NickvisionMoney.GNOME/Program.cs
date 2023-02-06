using NickvisionMoney.GNOME.Views;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME;

/// <summary>
/// The Program
/// </summary>
public partial class Program
{
    private delegate void OpenSignal(nint application, nint files, int nfiles, string hint, nint data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)] OpenSignal c_handler, nint data, nint destroy_data, int connect_flags);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_resource_load(string path);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_resources_register(nint file);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    private readonly Adw.Application _application;
    private MainWindow? _mainWindow;
    private MainWindowController _mainWindowController;
    private readonly OpenSignal _openSignal;

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
        if (CultureInfo.CurrentCulture.ToString() == "ar-RG")
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG"); // Fix #211
        }
        _application = Adw.Application.New("org.nickvision.money", Gio.ApplicationFlags.HandlesOpen | Gio.ApplicationFlags.NonUnique);
        _mainWindow = null;
        _mainWindowController = new MainWindowController();
        _mainWindowController.AppInfo.ID = "org.nickvision.money";
        _mainWindowController.AppInfo.Name = "Nickvision Denaro";
        _mainWindowController.AppInfo.ShortName = "Denaro";
        _mainWindowController.AppInfo.Description = $"{_mainWindowController.Localizer["Description"]}.";
        _mainWindowController.AppInfo.Version = "2023.2.0-beta2";
        _mainWindowController.AppInfo.Changelog = "<ul><li>Added the ability to add a password to an account (This will encrypt the nmoney file)</li><li>Added the ability to transfer money between accounts with different currencies by providing a conversion rate in TransferDialog</li><li>Added the ability to configure how Denaro uses locale separators in amount fields</li><li>Added the ability to copy individual transactions</li><li>Added the ability to sort transactions by amount</li><li>LC_MONETARY and LC_TIME will now be respected</li><li>Recent accounts are now available to select from the TransferDialog</li><li>Added a \"New Window\" action to the main menu</li></ul>";
        _mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/nlogozzo/NickvisionMoney");
        _mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/nlogozzo/NickvisionMoney/issues/new");
        _mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/nlogozzo/NickvisionMoney/discussions");
        _application.OnActivate += OnActivate;
        _openSignal = async (nint application, nint files, int nfiles, string hint, nint data) => await OnOpen(files, nfiles);
        g_signal_connect_data(_application.Handle, "open", _openSignal, IntPtr.Zero, IntPtr.Zero, 0);
        var prefixes = new List<string> {
            Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName)!.FullName,
            Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName,
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

    /// <summary>
    /// Finalizes a Program
    /// </summary>
    ~Program() => _mainWindowController.Dispose();

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run(string[] args)
    {
        var argv = new string[args.Length + 1];
        argv[0] = "NickvisionMoney.GNOME";
        args.CopyTo(argv, 1);
        return _application.Run(args.Length + 1, argv);
    }

    /// <summary>
    /// Occurs when the application is activated
    /// </summary>
    /// <param name="sedner">Gio.Application</param>
    /// <param name="e">EventArgs</param>
    private void OnActivate(Gio.Application sedner, EventArgs e)
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
        _mainWindow.Start();
    }

    /// <summary>
    /// Occurs when an nmoney file is double clicked to open the app
    /// </summary>
    /// <param name="sender">Gio.Application</param>
    /// <param name="e">Gio.Application.OpenSignalArgs</param>
    private async Task OnOpen(nint files, int nFiles)
    {
        if (nFiles > 0)
        {
            var filesArray = new IntPtr[1] { IntPtr.Zero };
            Marshal.Copy(files, filesArray, 0, 1);
            var pathOfFirstFile = g_file_get_path(filesArray[0]);
            OnActivate(_application, EventArgs.Empty);
            await _mainWindow!.OpenAccountAsync(pathOfFirstFile);
        }
    }
}
