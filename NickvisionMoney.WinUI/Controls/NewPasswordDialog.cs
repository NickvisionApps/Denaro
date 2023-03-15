using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Helpers;
using System;
using System.Threading.Tasks;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A dialog for creating a password
/// </summary>
public sealed partial class NewPasswordDialog : ContentDialog
{
    /// <summary>
    /// Constructs a NewPasswordDialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="localizer">The localizer for the app</param>
    public NewPasswordDialog(string title, Localizer localizer)
    {
        InitializeComponent();
        //Localize Strings
        CloseButtonText = localizer["Cancel"];
        PrimaryButtonText = localizer["Add"];
        Title = title;
        TxtPasswordNew.Header = localizer["NewPassword", "Field"];
        TxtPasswordNew.PlaceholderText = localizer["Password", "Placeholder"];
        TxtPasswordConfirm.Header = localizer["ConfirmPassword", "Field"];
        TxtPasswordConfirm.PlaceholderText = localizer["Password", "Placeholder"];
        LblPasswordWarning.Text = localizer["ManagePassword", "Warning"];
        LblPasswordErrors.Text = localizer["NonMatchingPasswords", "WinUI"];
        IsPrimaryButtonEnabled = false;
    }

    /// <summary>
    /// Shows the GroupDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<string?> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return TxtPasswordNew.Password;
        }
        return null;
    }

    /// <summary>
    /// Validates the user input
    /// </summary>
    private void Validate()
    {
        if(TxtPasswordNew.Password != TxtPasswordConfirm.Password)
        {
            LblPasswordErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
        else if(string.IsNullOrEmpty(TxtPasswordNew.Password))
        {
            LblPasswordErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = false;
        }
        else
        {
            LblPasswordErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
    }

    /// <summary>
    /// Occurs when the new password textbox is changed
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">RoutedEventArgs</param>
    private void TxtPasswordNew_PasswordChanged(object sender, RoutedEventArgs e) => Validate();

    /// <summary>
    /// Occurs when the confirm password textbox is changed
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">RoutedEventArgs</param>
    private void TxtPasswordConfirm_PasswordChanged(object sender, RoutedEventArgs e) => Validate();
}
