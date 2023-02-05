using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Helpers;
using System.Threading.Tasks;
using System;

namespace NickvisionMoney.WinUI.Controls;

/// <summary>
/// A dialog for receiving a password
/// </summary>
public sealed partial class PasswordDialog : ContentDialog
{
    /// <summary>
    /// Constructs a PasswordDialog
    /// </summary>
    /// <param name="title">The title of the account</param>
    /// <param name="localizer">The localizer for the app</param>
    public PasswordDialog(string title, Localizer localizer)
    {
        InitializeComponent();
        //Localize Strings
        CloseButtonText = localizer["Cancel"];
        PrimaryButtonText = localizer["OK"];
        Status.Title = localizer["EnterPassword"];
        Status.Description = title;
        TxtPassword.Header = localizer["Password", "Field"];
        TxtPassword.PlaceholderText = localizer["Password", "Placeholder"];
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
            return TxtPassword.Password;
        }
        return null;
    }
}
