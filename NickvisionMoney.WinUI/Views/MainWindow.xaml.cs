using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using NickvisionMoney.WinUI.Controls;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using WinRT.Interop;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly MainWindowController _controller;
    private readonly IntPtr _hwnd;
    private bool _isActived;
    private readonly Dictionary<string, AccountView> _accountViews;
    private RoutedEventHandler? _notificationButtonClickEvent;

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
        _isActived = true;
        _accountViews = new Dictionary<string, AccountView>();
        //Register Events
        AppWindow.Closing += Window_Closing;
        _controller.NotificationSent += NotificationSent;
        _controller.AccountLoginAsync += AccountLoginAsync;
        _controller.AccountAdded += AccountAdded;
        _controller.RecentAccountsChanged += RecentAccountsChanged;
        //Set TitleBar
        TitleBarTitle.Text = _controller.AppInfo.ShortName;
        AppWindow.Title = TitleBarTitle.Text;
        AppWindow.SetIcon(@"Assets\org.nickvision.money.ico");
        TitlePreview.Text = _controller.IsDevVersion ? _controller.Localizer["Preview", "WinUI"] : "";
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBarLeftPaddingColumn.Width = new GridLength(AppWindow.TitleBar.LeftInset);
            TitleBarRightPaddingColumn.Width = new GridLength(AppWindow.TitleBar.RightInset);
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        else
        {
            TitleBar.Visibility = Visibility.Collapsed;
            NavView.Margin = new Thickness(0, 0, 0, 0);
        }
        SystemBackdrop = new MicaBackdrop();
        //Window Sizing
        AppWindow.Resize(new SizeInt32(900, 700));
        User32.ShowWindow(_hwnd, ShowWindowCommand.SW_SHOWMAXIMIZED);
        //Localize Strings
        NavViewItemHome.Content = _controller.Localizer["Home"];
        NavViewItemDashboard.Content = _controller.Localizer["Dashboard"];
        NavViewItemAccounts.Content = _controller.Localizer["Accounts"];
        NavViewItemHelp.Content = _controller.Localizer["Help"];
        NavViewItemSettings.Content = _controller.Localizer["Settings"];
        StatusPageHome.Glyph = _controller.ShowSun ? "\xE706" : "\xE708";
        StatusPageHome.Title = _controller.Greeting;
        StatusPageHome.Description = _controller.Localizer["NoAccountDescription"];
        ToolTipService.SetToolTip(BtnHomeNewAccount, _controller.Localizer["NewAccount", "Tooltip"]);
        LblBtnHomeNewAccount.Text = _controller.Localizer["New"];
        ToolTipService.SetToolTip(BtnHomeOpenAccount, _controller.Localizer["OpenAccount", "Tooltip"]);
        LblBtnHomeOpenAccount.Text = _controller.Localizer["Open"];
        LblRecentAccounts.Text = _controller.Localizer["RecentAccounts"];
        LblNoRecentAccounts.Text = _controller.Localizer["NoRecentAccounts"];
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
    /// Updates a NavViewItem's title
    /// </summary>
    /// <param name="path">The path of the nav view item</param>
    /// <param name="title">The new title</param>
    private void UpdateNavViewItemTitle(string path, string title)
    {
        foreach (var obj in NavView.MenuItems)
        {
            if (obj is NavigationViewItem navViewItem)
            {
                if ((string)ToolTipService.GetToolTip(navViewItem) == path)
                {
                    navViewItem.Content = title;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Occurs when the window content is loaded
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //Launched File
        if (_controller.FileToLaunch != null)
        {
            await _controller.AddAccountAsync(_controller.FileToLaunch);
            _controller.FileToLaunch = null;
        }
    }

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
        AppWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"]).Color;
    }

    /// <summary>
    /// Occurs when the window is closing
    /// </summary>
    /// <param name="sender">AppWindow</param>
    /// <param name="e">AppWindowClosingEventArgs</param>
    private void Window_Closing(AppWindow sender, AppWindowClosingEventArgs e)
    {
        _controller.Dispose();
    }

    /// <summary>
    /// Occurs when the window's theme is changed
    /// </summary>
    /// <param name="sender">FrameworkElement</param>
    /// <param name="e">object</param>
    private void Window_ActualThemeChanged(FrameworkElement sender, object e)
    {
        //Update TitleBar
        TitleBarTitle.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        AppWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"]).Color;
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
                        if (Path.GetExtension(file.Path).ToLower() == ".nmoney")
                        {
                            await _controller.AddAccountAsync(file.Path);
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
        if (pageName == "Dashboard")
        {
            PageDashboard.Content = new DashboardPage(_controller.CreateDashboardViewController());
        }
        else if (pageName == "OpenAccount")
        {
            var path = (string)ToolTipService.GetToolTip((NavigationViewItem)e.SelectedItem);
            if (!_accountViews.ContainsKey(path))
            {
                _accountViews.Add(path, new AccountView(_controller.CreateAccountViewController(path)!, UpdateNavViewItemTitle, InitializeWithWindow));
            }
            PageOpenAccount.Content = _accountViews[path];
        }
        else if (pageName == "Settings")
        {
            PageSettings.Content = new PreferencesPage(_controller.CreatePreferencesViewController(), InitializeWithWindow);
        }
        ViewStack.ChangePage(pageName);
    }

    /// <summary>
    /// Occurs when the help button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TappedRoutedEventArgs</param>
    private async void Help(object sender, TappedRoutedEventArgs e)
    {
        var lang = "C";
        var availableTranslations = new string[2] { "es", "ru" };
        if (availableTranslations.Contains(CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
        {
            lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }
        await Launcher.LaunchUriAsync(new Uri($"https://htmlpreview.github.io/?https://raw.githubusercontent.com/NickvisionApps/Denaro/{_controller.AppInfo.Version}/NickvisionMoney.Shared/Docs/html/{lang}/index.html"));
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
        if (_notificationButtonClickEvent != null)
        {
            BtnInfoBar.Click -= _notificationButtonClickEvent;
        }
        BtnInfoBar.Visibility = !string.IsNullOrEmpty(e.Action) ? Visibility.Visible : Visibility.Collapsed;
        if (e.Action == "help-import")
        {
            BtnInfoBar.Content = _controller.Localizer["Help"];
            _notificationButtonClickEvent = async (sender, ex) =>
            {
                var lang = "C";
                var availableTranslations = new string[21] { "ar", "da", "de", "es", "et", "fi", "fr", "hi", "hr", "id", "it", "nl", "oc", "pl", "pt", "pt_BR", "ru", "sv", "tr", "ur", "zh_Hans" };
                if (availableTranslations.Contains(CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
                {
                    lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                }
                if (availableTranslations.Contains(CultureInfo.CurrentCulture.Name.Replace("-", "_")))
                {
                    lang = CultureInfo.CurrentCulture.Name.Replace("-", "_");
                }
                await Launcher.LaunchUriAsync(new Uri($"https://htmlpreview.github.io/?https://raw.githubusercontent.com/NickvisionApps/Denaro/{_controller.AppInfo.Version}/NickvisionMoney.Shared/Docs/html/{lang}/import-export.html"));
            };
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else if (e.Action == "open-export")
        {
            BtnInfoBar.Content = _controller.Localizer["Open"];
            _notificationButtonClickEvent = async (sender, ex) =>
            {
                await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(e.ActionParam));
            };
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        InfoBar.IsOpen = true;
    }

    /// <summary>
    /// Occurs when an account needs a login
    /// </summary>
    /// <param name="title">The title of the account</param>
    private async Task<string?> AccountLoginAsync(string title)
    {
        var passwordDialog = new PasswordDialog(title, _controller.Localizer)
        {
            XamlRoot = Content.XamlRoot,
            RequestedTheme = NavView.RequestedTheme
        };
        return await passwordDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when an account is created/opened in the app
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private void AccountAdded(object? sender, EventArgs e)
    {
        var newNavItem = new NavigationViewItem()
        {
            Tag = "OpenAccount",
            Content = _controller.GetMostRecentAccountViewController().AccountTitle,
            Icon = new FontIcon()
            {
                FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)Application.Current.Resources["SymbolThemeFontFamily"],
                Glyph = "\uE8C7",
            }
        };
        NavViewItemDashboard.Visibility = Visibility.Visible;
        NavViewItemAccounts.Visibility = Visibility.Visible;
        ToolTipService.SetToolTip(newNavItem, _controller.GetMostRecentAccountViewController().AccountPath);
        NavView.MenuItems.Add(newNavItem);
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
        foreach (var recentAccount in _controller.RecentAccounts)
        {
            var bgColorString = _controller.GetColorForAccountType(recentAccount.Type);
            var bgColorStrArray = new Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
            var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
            var actionRow = new ActionRow(recentAccount.Name, recentAccount.Path);
            //Icon Label
            var iconLabel = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(-10, 0, 10, 0),
                FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)Application.Current.Resources["SymbolThemeFontFamily"],
                FontSize = 16,
                Text = "\uE8C7"
            };
            DockPanel.SetDock(iconLabel, Dock.Left);
            actionRow.Children.Insert(0, iconLabel);
            //Type Box
            var typeBox = new Border()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush((Color)ColorHelpers.FromRGBA(bgColorString)!),
                CornerRadius = new CornerRadius(12)
            };
            var typeLabel = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(30, 0, 30, 0),
                Foreground = new SolidColorBrush(luma < 0.5 ? Colors.White : Colors.Black),
                Text = _controller.Localizer["AccountType", recentAccount.Type.ToString()]
            };
            typeBox.Child = typeLabel;
            DockPanel.SetDock(typeBox, Dock.Right);
            actionRow.Children.Add(typeBox);
            ListRecentAccounts.Items.Add(actionRow);
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
        if (file != null)
        {
            if (_controller.IsAccountOpen(file.Path))
            {
                NotificationSent(null, new NotificationSentEventArgs(_controller.Localizer["UnableToOverride"], NotificationSeverity.Error));
            }
            else
            {
                if (File.Exists(file.Path))
                {
                    File.Delete(file.Path);
                }
                await _controller.AddAccountAsync(file.Path);
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
            await _controller.AddAccountAsync(file.Path);
        }
    }

    /// <summary>
    /// Occurs when an account is selected from the list of recent accounts
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private async void ListRecentAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListRecentAccounts.SelectedIndex != -1)
        {
            var selectedIndex = ListRecentAccounts.SelectedIndex;
            ListRecentAccounts.SelectedIndex = -1;
            await _controller.AddAccountAsync(_controller.RecentAccounts[selectedIndex].Path);
        }
    }
}
