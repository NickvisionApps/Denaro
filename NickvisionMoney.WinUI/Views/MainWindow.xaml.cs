using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.WinUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Vanara.PInvoke;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT;
using WinRT.Interop;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly MainWindowController _controller;
    private readonly IntPtr _hwnd;
    private readonly AppWindow _appWindow;
    private bool _isActived;
    private readonly SystemBackdropConfiguration _backdropConfiguration;
    private readonly MicaController? _micaController;

    /// <summary>
    /// Constructs a MainWindow
    /// </summary>
    /// <param name="controller">The MainWindowController</param>
    public MainWindow(MainWindowController controller)
    {
        InitializeComponent();
        //Initialize Vars
        _controller = controller;
        _hwnd = WindowNative.GetWindowHandle(this);
        _appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd));
        _isActived = true;
        //Register Events
        _appWindow.Closing += Window_Closing;
        _controller.NotificationSent += NotificationSent;
        _controller.AccountAdded += AccountAdded;
        _controller.RecentAccountsChanged += RecentAccountsChanged;
        //Set TitleBar
        TitleBarTitle.Text = _controller.AppInfo.ShortName;
        _appWindow.Title = TitleBarTitle.Text;
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBarLeftPaddingColumn.Width = new GridLength(_appWindow.TitleBar.LeftInset);
            TitleBarRightPaddingColumn.Width = new GridLength(_appWindow.TitleBar.RightInset);
            _appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        else
        {
            TitleBar.Visibility = Visibility.Collapsed;
            NavView.Margin = new Thickness(0, 0, 0, 0);
        }
        //Setup Backdrop
        WindowsSystemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();
        _backdropConfiguration = new SystemBackdropConfiguration()
        {
            IsInputActive = true,
            Theme = ((FrameworkElement)Content).ActualTheme switch
            {
                ElementTheme.Default => SystemBackdropTheme.Default,
                ElementTheme.Light => SystemBackdropTheme.Light,
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                _ => SystemBackdropTheme.Default
            }
        };
        if (MicaController.IsSupported())
        {
            _micaController = new MicaController();
            _micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
            _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);
        }
        //Window Sizing
        _appWindow.Resize(new SizeInt32(900, 700));
        User32.ShowWindow(_hwnd, ShowWindowCommand.SW_SHOWMAXIMIZED);
        //Localize Strings
        NavViewItemHome.Content = _controller.Localizer["Home"];
        NavViewItemAccount.Content = _controller.Localizer["Account"];
        NavViewItemSettings.Content = _controller.Localizer["Settings"];
        StatusPageHome.Glyph = _controller.ShowSun ? "\xE706" : "\xF1DB";
        StatusPageHome.Title = _controller.Greeting;
        StatusPageHome.Description = _controller.Localizer["NoAccountDescription"];
        ToolTipService.SetToolTip(BtnHomeNewAccount, _controller.Localizer["NewAccount", "Tooltip"]);
        LblBtnHomeNewAccount.Text = _controller.Localizer["New"];
        ToolTipService.SetToolTip(BtnHomeOpenAccount, _controller.Localizer["OpenAccount", "Tooltip"]);
        LblBtnHomeOpenAccount.Text = _controller.Localizer["Open"];
        LblRecentAccounts.Text = _controller.Localizer["RecentAccounts"];
        LblNoRecentAccounts.Text = _controller.Localizer["NoRecentAccounts"];
        StatusPageAccount.Title = _controller.Localizer["NoAccountOpened"];
        StatusPageAccount.Description = _controller.Localizer["NoAccountDescription"];
        //Page
        NavViewItemHome.IsSelected = true;
        RecentAccountsChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Calls InitializeWithWindow.Initialize on the target object with the MainWindow's hwnd
    /// </summary>
    /// <param name="target">The target object to initialize</param>
    public void InitializeWithWindow(object target) => WinRT.Interop.InitializeWithWindow.Initialize(target, _hwnd);

    /// <summary>
    /// Occurs when the window is activated
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">WindowActivatedEventArgs</param>
    private void Window_Activated(object sender, WindowActivatedEventArgs e)
    {
        _isActived = e.WindowActivationState != WindowActivationState.Deactivated;
        //Update TitleBar
        TitleBarTitle.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        _appWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"]).Color;
        //Update Backdrop
        _backdropConfiguration.IsInputActive = _isActived;
    }

    /// <summary>
    /// Occurs when the window is closing
    /// </summary>
    /// <param name="sender">AppWindow</param>
    /// <param name="e">AppWindowClosingEventArgs</param>
    private void Window_Closing(AppWindow sender, AppWindowClosingEventArgs e) => _micaController?.Dispose();

    /// <summary>
    /// Occurs when the window's theme is changed
    /// </summary>
    /// <param name="sender">FrameworkElement</param>
    /// <param name="e">object</param>
    private void Window_ActualThemeChanged(FrameworkElement sender, object e)
    {
        //Update TitleBar
        TitleBarTitle.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        _appWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"]).Color;
        //Update Backdrop
        _backdropConfiguration.Theme = sender.ActualTheme switch
        {
            ElementTheme.Default => SystemBackdropTheme.Default,
            ElementTheme.Light => SystemBackdropTheme.Light,
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            _ => SystemBackdropTheme.Default
        };
    }

    /// <summary>
    /// Occurs when something is dragged over the window
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">DragEventArgs</param>
    private void Window_DragOver(object sender, DragEventArgs e) => e.AcceptedOperation = DataPackageOperation.Link;

    /// <summary>
    /// Occurs when something is dropped on the window
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">DragEventArgs</param>
    private async void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    if (item is StorageFile file)
                    {
                        if(Path.GetExtension(file.Path) == ".nmoney")
                        {
                            _controller.AddAccount(file.Path);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Occurs when the NavigationView's item selection is changed
    /// </summary>
    /// <param name="sender">NavigationView</param>
    /// <param name="e">NavigationViewSelectionChangedEventArgs</param>
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
    {
        var pageName = (string)((NavigationViewItem)e.SelectedItem).Tag;
        if (pageName == "OpenAccount")
        {
            PageOpenAccount.Content = new AccountView(_controller.OpenAccounts[_controller.OpenAccounts.FindIndex(x => Path.GetFileNameWithoutExtension(x.AccountPath) == (string)((NavigationViewItem)e.SelectedItem).Content)], InitializeWithWindow);
        }
        else if (pageName == "Settings")
        {
            PageSettings.Content = new PreferencesPage(_controller.PreferencesViewController);
        }
        ViewStack.ChangePage(pageName);
    }

    /// <summary>
    /// Occurs when a notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotificationSentEventArgs</param>
    private void NotificationSent(object? sender, NotificationSentEventArgs e)
    {
        InfoBar.Message = e.Message;
        InfoBar.Severity = e.Severity switch
        {
            NotificationSeverity.Informational => InfoBarSeverity.Informational,
            NotificationSeverity.Success => InfoBarSeverity.Success,
            NotificationSeverity.Warning => InfoBarSeverity.Warning,
            NotificationSeverity.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };
        InfoBar.IsOpen = true;
    }

    /// <summary>
    /// Occurs when an account is created/opened in the app
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void AccountAdded(object? sender, EventArgs e)
    {
        StatusPageAccount.Title = _controller.Localizer["SelectAccount"];
        StatusPageAccount.Description = _controller.Localizer["SelectAccountDescription"];
        var newNavItem = new NavigationViewItem()
        {
            Tag = "OpenAccount",
            Content = Path.GetFileNameWithoutExtension(_controller.OpenAccounts[_controller.OpenAccounts.Count - 1].AccountPath)
        };
        var items = new List<NavigationViewItem>((List<NavigationViewItem>?)NavViewItemAccount.MenuItemsSource ?? new List<NavigationViewItem>()) { newNavItem };
        NavViewItemAccount.MenuItemsSource = items;
        NavViewItemAccount.IsExpanded = true;
        NavView.SelectedItem = newNavItem;
    }

    /// <summary>
    /// Occurs when the list of recent accounts is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void RecentAccountsChanged(object? sender, EventArgs e)
    {
        ListRecentAccounts.Items.Clear();
        foreach(var recentAccount in _controller.RecentAccounts)
        {
            ListRecentAccounts.Items.Add(new ActionRow(Path.GetFileName(recentAccount), Path.GetDirectoryName(recentAccount)));
        }
        ViewStackRecents.ChangePage(_controller.RecentAccounts.Count > 0 ? "Recents" : "NoRecents");
    }

    /// <summary>
    /// Occurs when the new account button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void NewAccount(object sender, RoutedEventArgs e)
    {
        var fileSavePicker = new FileSavePicker();
        InitializeWithWindow(fileSavePicker);
        fileSavePicker.FileTypeChoices.Add(_controller.Localizer["NickvisionMoneyAccount"], new List<string>() { ".nmoney" });
        fileSavePicker.SuggestedFileName = _controller.Localizer["NewAccount"];
        fileSavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileSavePicker.PickSaveFileAsync();
        if(file != null)
        {
            if(_controller.IsAccountOpen(file.Path))
            {
                NotificationSent(null, new NotificationSentEventArgs(_controller.Localizer["UnableToOverride"], NotificationSeverity.Error));
            }
            else
            {
                if(File.Exists(file.Path))
                {
                    File.Delete(file.Path);
                }
                _controller.AddAccount(file.Path);
            }
        }
    }

    /// <summary>
    /// Occurs when the open account button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void OpenAccount(object sender, RoutedEventArgs e)
    {
        var fileOpenPicker = new FileOpenPicker();
        InitializeWithWindow(fileOpenPicker);
        fileOpenPicker.FileTypeFilter.Add(".nmoney");
        fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
        {
            _controller.AddAccount(file.Path);
        }
    }

    /// <summary>
    /// Occurs when an account is selected from the list of recent accounts
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void ListRecentAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(ListRecentAccounts.SelectedIndex != -1)
        {
            _controller.AddAccount(_controller.RecentAccounts[ListRecentAccounts.SelectedIndex]);
            ListRecentAccounts.SelectedIndex = -1;
        }
    }
}
