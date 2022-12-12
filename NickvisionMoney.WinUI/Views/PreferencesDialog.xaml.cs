using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public sealed partial class PreferencesDialog : ContentDialog
{
    private readonly PreferencesViewController _controller;

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    public PreferencesDialog(PreferencesViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        Title = _controller.Localizer["Settings"];
        CardTheme.Header = _controller.Localizer["SettingsTheme"];
        CardTheme.Description = _controller.Localizer["SettingsThemeDescription"];
        CmbTheme.Items.Add(_controller.Localizer["SettingsThemeLight"]);
        CmbTheme.Items.Add(_controller.Localizer["SettingsThemeDark"]);
        CmbTheme.Items.Add(_controller.Localizer["SettingsThemeSystem"]);
    }

    /// <summary>
    /// Occurs when the dialog is opened
    /// </summary>
    /// <param name="sender">ContentDialog</param>
    /// <param name="args">ContentDialogOpenedEventArgs</param>
    private void Dialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        CmbTheme.SelectedIndex = (int)_controller.Theme;
    }

    /// <summary>
    /// Occurs when the dialog is closed
    /// </summary>
    /// <param name="sender">ContentDialog</param>
    /// <param name="args">ContentDialogOpenedEventArgs</param>
    private void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _controller.Theme = (Theme)CmbTheme.SelectedIndex;
        _controller.SaveConfiguration();
    }
}