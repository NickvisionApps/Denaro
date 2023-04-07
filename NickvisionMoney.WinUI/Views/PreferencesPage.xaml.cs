using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// The PreferencesPage for the application
/// </summary>
public sealed partial class PreferencesPage : UserControl, INotifyPropertyChanged
{
    private readonly PreferencesViewController _controller;
    private readonly Action<object> _initializeWithWindow;
    private Color _transactionDefaultColor;
    private Color _transferDefaultColor;
    private Color _groupDefaultColor;
    private Color _accountCheckingColor;
    private Color _accountSavingsColor;
    private Color _accountBusinessColor;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Constructs a PreferencesPage
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    /// <param name="initializeWithWindow">The Action<object> callback for InitializeWithWindow</param>
    public PreferencesPage(PreferencesViewController controller, Action<object> initializeWithWindow)
    {
        InitializeComponent();
        _controller = controller;
        _initializeWithWindow = initializeWithWindow;
        //Localize Strings
        LblTitle.Text = _controller.Localizer["Settings"];
        LblAbout.Text = string.Format(_controller.Localizer["About"], _controller.AppInfo.Name);
        LblDescription.Text = $"{_controller.AppInfo.Description}\n";
        LblVersion.Text = string.Format(_controller.Localizer["Version"], _controller.AppInfo.Version);
        LblBtnChangelog.Text = _controller.Localizer["Changelog"];
        LblBtnCredits.Text = _controller.Localizer["Credits"];
        LblBtnGitHubRepo.Text = _controller.Localizer["GitHubRepo"];
        LblBtnReportABug.Text = _controller.Localizer["ReportABug"];
        LblBtnDiscussions.Text = _controller.Localizer["Discussions"];
        CardUserInterface.Header = _controller.Localizer["UserInterface"];
        CardUserInterface.Description = _controller.Localizer["UserInterfaceDescription"];
        CardTheme.Header = _controller.Localizer["Theme"];
        CardTheme.Description = _controller.Localizer["ThemeDescription", "WinUI"];
        CmbTheme.Items.Add(_controller.Localizer["ThemeLight"]);
        CmbTheme.Items.Add(_controller.Localizer["ThemeDark"]);
        CmbTheme.Items.Add(_controller.Localizer["ThemeSystem"]);
        CardColors.Header = _controller.Localizer["Colors"];
        CardColors.Description = _controller.Localizer["ColorsDescription"];
        CardTransactionDefaultColor.Header = _controller.Localizer["TransactionColor"];
        CardTransactionDefaultColor.Description = _controller.Localizer["TransactionColorDescription"];
        CardTransferDefaultColor.Header = _controller.Localizer["TransferColor"];
        CardTransferDefaultColor.Description = _controller.Localizer["TransferColorDescription"];
        CardGroupDefaultColor.Header = _controller.Localizer["GroupColor"];
        CardAccountCheckingColor.Header = _controller.Localizer["AccountCheckingColor"];
        CardAccountSavingsColor.Header = _controller.Localizer["AccountSavingsColor"];
        CardAccountBusinessColor.Header = _controller.Localizer["AccountBusinessColor"];
        CardLocale.Header = _controller.Localizer["Locale"];
        CardLocale.Description = _controller.Localizer["LocaleDescription"];
        CardInsertSeparator.Header = _controller.Localizer["InsertSeparator"];
        CardInsertSeparator.Description = _controller.Localizer["InsertSeparator", "Description"];
        CmbInsertSeparator.Items.Add(_controller.Localizer["InsertSeparatorOff"]);
        CmbInsertSeparator.Items.Add(_controller.Localizer["InsertSeparatorNumpad"]);
        CmbInsertSeparator.Items.Add(_controller.Localizer["InsertSeparatorPeriodComma"]);
        CardBackup.Header = _controller.Localizer["Backup"];
        CardBackup.Description = _controller.Localizer["BackupDescription"];
        CardBackupFolder.Header = _controller.Localizer["BackupFolder"];
        CardBackupFolder.Description = _controller.Localizer["BackupFolderDescription"];
        LblBackupFolder.Text = _controller.Localizer["NoBackupFolder"];
        ToolTipService.SetToolTip(BtnSelectBackupFolder, _controller.Localizer["SelectBackupFolder"]);
        ToolTipService.SetToolTip(BtnClearBackupFolder, _controller.Localizer["ClearBackupFolder"]);
        //Load Config
        CmbTheme.SelectedIndex = (int)_controller.Theme;
        TransactionDefaultColor = ColorHelpers.FromRGBA(_controller.TransactionDefaultColor) ?? Color.FromArgb(255, 255, 255, 255);
        TransferDefaultColor = ColorHelpers.FromRGBA(_controller.TransferDefaultColor) ?? Color.FromArgb(255, 255, 255, 255);
        GroupDefaultColor = ColorHelpers.FromRGBA(_controller.GroupDefaultColor) ?? Color.FromArgb(255, 255, 255, 255);
        AccountCheckingColor = ColorHelpers.FromRGBA(_controller.AccountCheckingColor) ?? Color.FromArgb(255, 255, 255, 255);
        AccountSavingsColor = ColorHelpers.FromRGBA(_controller.AccountSavingsColor) ?? Color.FromArgb(255, 255, 255, 255);
        AccountBusinessColor = ColorHelpers.FromRGBA(_controller.AccountBusinessColor) ?? Color.FromArgb(255, 255, 255, 255);
        CmbInsertSeparator.SelectedIndex = (int)_controller.InsertSeparator;
        LblBackupFolder.Text = !Directory.Exists(_controller.CSVBackupFolder) ? _controller.Localizer["NoBackupFolder"] : _controller.CSVBackupFolder;
        _initializeWithWindow = initializeWithWindow;
    }

    /// <summary>
    /// The default color to use for a transaction
    /// </summary>
    public Color TransactionDefaultColor
    {
        get => _transactionDefaultColor;

        set
        {
            _transactionDefaultColor = value;
            _controller.TransactionDefaultColor = _transactionDefaultColor.ToRGBA();
            _controller.SaveConfiguration();
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The default color to use for a transfer
    /// </summary>
    public Color TransferDefaultColor
    {
        get => _transferDefaultColor;

        set
        {
            _transferDefaultColor = value;
            _controller.TransferDefaultColor = _transferDefaultColor.ToRGBA();
            _controller.SaveConfiguration();
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The default color to use for a group
    /// </summary>
    public Color GroupDefaultColor
    {
        get => _groupDefaultColor;

        set
        {
            _groupDefaultColor = value;
            _controller.GroupDefaultColor = _groupDefaultColor.ToRGBA();
            _controller.SaveConfiguration();
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The color of accounts with Checking type
    /// </summary>
    public Color AccountCheckingColor
    {
        get => _accountCheckingColor;

        set
        {
            _accountCheckingColor = value;
            _controller.AccountCheckingColor = _accountCheckingColor.ToRGBA();
            _controller.SaveConfiguration();
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The color of accounts with Savings type
    /// </summary>
    public Color AccountSavingsColor
    {
        get => _accountSavingsColor;

        set
        {
            _accountSavingsColor = value;
            _controller.AccountSavingsColor = _accountSavingsColor.ToRGBA();
            _controller.SaveConfiguration();
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The color of accounts with Business type
    /// </summary>
    public Color AccountBusinessColor
    {
        get => _accountBusinessColor;

        set
        {
            _accountBusinessColor = value;
            _controller.AccountBusinessColor = _accountBusinessColor.ToRGBA();
            _controller.SaveConfiguration();
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Removes URLs from a credits string
    /// </summary>
    /// <param name="s">The credits string</param>
    /// <returns>The new credits string with URLs removed</returns>
    private string RemoveUrlsFromCredits(string s)
    {
        var credits = s.Split('\n');
        var result = "";
        for (int i = 0; i < credits.Length; i++)
        {
            if (credits[i].IndexOf("https://") != -1)
            {
                result += credits[i].Remove(credits[i].IndexOf("https://"));
            }
            else if (credits[i].IndexOf("http://") != -1)
            {
                result += credits[i].Remove(credits[i].IndexOf("http://"));
            }
            if (i != credits.Length - 1)
            {
                result += "\n";
            }
        }
        return result;
    }

    /// <summary>
    /// Occurs when the changelog button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Changelog(object sender, RoutedEventArgs e)
    {
        var changelogDialog = new ContentDialog()
        {
            Title = _controller.Localizer["ChangelogTitle", "WinUI"],
            Content = _controller.AppInfo.Changelog,
            CloseButtonText = _controller.Localizer["OK"],
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot,
            RequestedTheme = RequestedTheme
        };
        await changelogDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the credits button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Credits(object sender, RoutedEventArgs e)
    {
        var creditsDialog = new ContentDialog()
        {
            Title = _controller.Localizer["Credits"],
            Content = string.Format(_controller.Localizer["CreditsDialogDescription", "WinUI"], RemoveUrlsFromCredits(_controller.Localizer["Developers", "Credits"]), RemoveUrlsFromCredits(_controller.Localizer["Designers", "Credits"]), RemoveUrlsFromCredits(_controller.Localizer["Artists", "Credits"]), RemoveUrlsFromCredits(_controller.Localizer["Translators", "Credits"])),
            CloseButtonText = _controller.Localizer["OK"],
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot,
            RequestedTheme = RequestedTheme
        };
        await creditsDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the github repo button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void GitHubRepo(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(_controller.AppInfo.GitHubRepo);

    /// <summary>
    /// Occurs when the report a bug button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ReportABug(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(_controller.AppInfo.IssueTracker);

    /// <summary>
    /// Occurs when the discussions button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Discussions(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(_controller.AppInfo.SupportUrl);

    /// <summary>
    /// Occurs when the CmbTheme selection is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private async void CmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_controller.Theme != (Theme)CmbTheme.SelectedIndex)
        {
            _controller.Theme = (Theme)CmbTheme.SelectedIndex;
            _controller.SaveConfiguration();
            var restartDialog = new ContentDialog()
            {
                Title = _controller.Localizer["RestartThemeTitle", "WinUI"],
                Content = string.Format(_controller.Localizer["RestartThemeDescription", "WinUI"], _controller.AppInfo.ShortName),
                PrimaryButtonText = _controller.Localizer["Yes"],
                CloseButtonText = _controller.Localizer["No"],
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = Content.XamlRoot,
                RequestedTheme = RequestedTheme
            };
            var result = await restartDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                AppInstance.Restart("Apply new theme");
            }
        }
    }

    /// <summary>
    /// Occurs when the CmbInserSeparator selection is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbInsertSeparator_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _controller.InsertSeparator = (InsertSeparator)CmbInsertSeparator.SelectedIndex;
        _controller.SaveConfiguration();
    }

    /// <summary>
    /// Occurs when the select backup folder button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void SelectBackupFolder(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FolderPicker();
        _initializeWithWindow(folderPicker);
        folderPicker.FileTypeFilter.Add("*");
        folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            LblBackupFolder.Text = folder.Path;
            _controller.CSVBackupFolder = folder.Path;
            _controller.SaveConfiguration();
        }
    }

    /// <summary>
    /// Occurs when the clear backup folder button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ClearBackupFolder(object sender, RoutedEventArgs e)
    {
        LblBackupFolder.Text = _controller.Localizer["NoBackupFolder"];
        _controller.CSVBackupFolder = "";
        _controller.SaveConfiguration();
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
