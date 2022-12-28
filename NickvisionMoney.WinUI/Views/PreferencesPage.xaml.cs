using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// The PreferencesPage for the application
/// </summary>
public sealed partial class PreferencesPage : UserControl, INotifyPropertyChanged
{
    private readonly PreferencesViewController _controller;
    private Color _transactionDefaultColor;
    private Color _transferDefaultColor;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Constructs a PreferencesPage
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    public PreferencesPage(PreferencesViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        LblTitle.Text = _controller.Localizer["Settings"];
        LblAbout.Text = string.Format(_controller.Localizer["About"], _controller.AppInfo.Name);
        LblDescription.Text = $"{_controller.AppInfo.Description}\n";
        LblVersion.Text = string.Format(_controller.Localizer["Version"], _controller.AppInfo.Version);
        LblBtnChangelog.Text = _controller.Localizer["Changelog"];
        LblBtnCredits.Text = _controller.Localizer["Credits"];
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
        //Load Config
        CmbTheme.SelectedIndex = (int)_controller.Theme;
        TransactionDefaultColor = ColorHelpers.FromRGBA(_controller.TransactionDefaultColor) ?? Color.FromArgb(255, 255, 255, 255);
        TransferDefaultColor = ColorHelpers.FromRGBA(_controller.TransferDefaultColor) ?? Color.FromArgb(255, 255, 255, 255);
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
            XamlRoot = Content.XamlRoot
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
            XamlRoot = Content.XamlRoot
        };
        await creditsDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the CmbTheme selection is changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
                XamlRoot = Content.XamlRoot
            };
            var result = await restartDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                AppInstance.Restart("Apply new theme");
            }
        }
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
