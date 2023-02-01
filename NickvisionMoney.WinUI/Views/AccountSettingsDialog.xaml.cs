using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing account metadata
/// </summary>
public sealed partial class AccountSettingsDialog : ContentDialog
{
    private bool _constructing;
    private readonly AccountSettingsDialogController _controller;

    /// <summary>
    /// Constructs an AccountSettingsDialog
    /// </summary>
    /// <param name="controller">The AccountSettingsDialogController</param>
    public AccountSettingsDialog(AccountSettingsDialogController controller)
    {
        InitializeComponent();
        _constructing = true;
        _controller = controller;
        //Localize Strings
        Title = _controller.Localizer["AccountSettings"];
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer["OK"];
        TxtName.Header = _controller.Localizer["Name", "Field"];
        TxtName.PlaceholderText = _controller.Localizer["Name", "Placeholder"];
        CmbAccountType.Header = _controller.Localizer["AccountType", "Field"];
        CmbAccountType.Items.Add(_controller.Localizer["AccountType", "Checking"]);
        CmbAccountType.Items.Add(_controller.Localizer["AccountType", "Savings"]);
        CmbAccountType.Items.Add(_controller.Localizer["AccountType", "Business"]);
        CmbDefaultTransactionType.Header = _controller.Localizer["DefaultTransactionType", "Field"];
        CmbDefaultTransactionType.Items.Add(_controller.Localizer["Income"]);
        CmbDefaultTransactionType.Items.Add(_controller.Localizer["Expense"]);
        LblSystemCurrencyDescription.Text = _controller.Localizer["ReportedCurrency"];
        LblSystemCurrency.Text = _controller.ReportedCurrencyString;
        TglUseCustomCurrency.OffContent = _controller.Localizer["UseCustomCurrency", "Field"];
        TglUseCustomCurrency.OnContent = _controller.Localizer["UseCustomCurrency", "Field"];
        TxtCustomSymbol.Header = _controller.Localizer["CustomCurrencySymbol", "Field"];
        TxtCustomSymbol.PlaceholderText = _controller.Localizer["CustomCurrencySymbol", "Placeholder"];
        TxtCustomCode.Header = _controller.Localizer["CustomCurrencyCode", "Field"];
        TxtCustomCode.PlaceholderText = _controller.Localizer["CustomCurrencyCode", "Placeholder"];
        CardPassword.Header = _controller.Localizer["ManagePassword"];
        CardPassword.Description = _controller.Localizer["ManagePassword", "Description"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        LblPasswordBack.Text = _controller.Localizer["Back"];
        LblPasswordRemove.Text = _controller.Localizer["Remove"];
        TxtPasswordNew.Header = _controller.Localizer["NewPassword", "Field"];
        TxtPasswordNew.PlaceholderText = _controller.Localizer["Password", "Placeholder"];
        TxtPasswordConfirm.Header = _controller.Localizer["ConfirmPassword", "Field"];
        TxtPasswordConfirm.PlaceholderText = _controller.Localizer["Password", "Placeholder"];
        LblPasswordWarning.Text = _controller.Localizer["ManagePassword", "Warning"];
        TxtPasswordErrors.Text = _controller.Localizer["NonMatchingPasswords"];
        //Load Metadata
        TxtName.Text = _controller.Metadata.Name;
        CmbAccountType.SelectedIndex = (int)_controller.Metadata.AccountType;
        CmbDefaultTransactionType.SelectedIndex = (int)_controller.Metadata.DefaultTransactionType;
        TglUseCustomCurrency.IsOn = _controller.Metadata.UseCustomCurrency;
        if (_controller.Metadata.UseCustomCurrency)
        {
            TxtCustomSymbol.Text = _controller.Metadata.CustomCurrencySymbol;
            TxtCustomCode.Text = _controller.Metadata.CustomCurrencyCode;
        }
        BtnPasswordRemove.Visibility = _controller.IsEncrypted ? Visibility.Visible : Visibility.Collapsed;
        TxtName_TextChanged(null, null);
        TglUseCustomCurrency_Toggled(null, new RoutedEventArgs());
        Validate();
        ViewStack.ChangePage("Main");
        _constructing = false;
    }

    /// <summary>
    /// Shows the AccountSettingsDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            if (_controller.NeedsSetup)
            {
                return await ShowAsync();
            }
            else
            {
                _controller.Accepted = false;
                return false;
            }
        }
        _controller.Accepted = true;
        return true;
    }

    /// <summary>
    /// Validate the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateMetadata(TxtName.Text, (AccountType)CmbAccountType.SelectedIndex, TglUseCustomCurrency.IsOn, TxtCustomSymbol.Text, TxtCustomCode.Text, (TransactionType)CmbDefaultTransactionType.SelectedIndex, TxtPasswordNew.Password, TxtPasswordConfirm.Password);
        TxtName.Header = _controller.Localizer["Name", "Field"];
        TxtCustomSymbol.Header = _controller.Localizer["CustomCurrencySymbol", "Field"];
        TxtCustomCode.Header = _controller.Localizer["CustomCurrencyCode", "Field"];
        if (checkStatus == AccountMetadataCheckStatus.Valid)
        {
            if(_controller.NewPassword != null)
            {
                InfoBadgePassword.Visibility = Visibility.Visible;
                InfoBadgePassword.Style = (Style)App.Current.Resources["SuccessDotInfoBadgeStyle"];
            }
            else
            {
                InfoBadgePassword.Visibility = Visibility.Collapsed;
            }
            TxtErrors.Visibility = Visibility.Collapsed;
            TxtPasswordErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
        else
        {
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyName))
            {
                TxtName.Header = _controller.Localizer["Name", "Empty"];
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyCurrencySymbol))
            {
                TxtCustomSymbol.Header = _controller.Localizer["CustomCurrencySymbol", "Empty"];
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyCurrencyCode))
            {
                TxtCustomCode.Header = _controller.Localizer["CustomCurrencyCode", "Empty"];
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.NonMatchingPasswords))
            {
                InfoBadgePassword.Visibility = Visibility.Visible;
                InfoBadgePassword.Style = (Style)App.Current.Resources["CriticalDotInfoBadgeStyle"];
                TxtPasswordErrors.Visibility = Visibility.Visible;
            }
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
    }

    /// <summary>
    /// Occurs when the name field's text is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">TextChangedEventArgs?</param>
    private void TxtName_TextChanged(object? sender, TextChangedEventArgs? e)
    {
        if (TxtName.Text.Length == 0)
        {
            LblId.Text = _controller.Localizer["NotAvailable"];
        }
        else
        {
            var split = TxtName.Text.Split(' ');
            if (split.Length == 1)
            {
                LblId.Text = split[0].Substring(0, split[0].Length > 1 ? 2 : 1);
            }
            else
            {
                if (string.IsNullOrEmpty(split[0]) && string.IsNullOrEmpty(split[1]))
                {
                    LblId.Text = _controller.Localizer["NotAvailable"];
                }
                else if (string.IsNullOrEmpty(split[0]))
                {
                    LblId.Text = split[1].Substring(0, split[1].Length > 1 ? 2 : 1);
                }
                else if (string.IsNullOrEmpty(split[1]))
                {
                    LblId.Text = split[0].Substring(0, split[0].Length > 1 ? 2 : 1);
                }
                else
                {
                    var emojiPattern = @"\p{Cs}.";
                    if (Regex.Match(split[0], emojiPattern).Success && Regex.Match(split[1], emojiPattern).Success)
                    {
                        LblId.Text = $"{split[0][0]}{split[0][1]}{split[1][0]}{split[1][1]}";
                    }
                    else if (Regex.Match(split[0], emojiPattern).Success)
                    {
                        LblId.Text = $"{split[0][0]}{split[0][1]}{split[1][0]}";
                    }
                    else if (Regex.Match(split[1], emojiPattern).Success)
                    {
                        LblId.Text = $"{split[0][0]}{split[1][0]}{split[1][1]}";
                    }
                    else
                    {
                        LblId.Text = $"{split[0][0]}{split[1][0]}";
                    }
                }
            }
        }
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the account type combobox's selection is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbAccountType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var bgColorString = _controller.GetColorForAccountType((AccountType)CmbAccountType.SelectedIndex);
        var bgColorStrArray = new Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
        var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
        BorderId.Background = new SolidColorBrush((Color)ColorHelpers.FromRGBA(bgColorString)!);
        LblId.Foreground = new SolidColorBrush(luma < 0.5 ? Colors.White : Colors.Black);
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the default transaction type combobox's selection is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbDefaultTransactionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the use custom currency toggle is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">RoutedEventArgs</param>
    private void TglUseCustomCurrency_Toggled(object? sender, RoutedEventArgs e)
    {
        if (TglUseCustomCurrency.IsOn)
        {
            TxtCustomSymbol.IsEnabled = true;
            TxtCustomCode.IsEnabled = true;
        }
        else
        {
            TxtCustomSymbol.IsEnabled = false;
            TxtCustomCode.IsEnabled = false;
        }
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the custom symbol textbox is changed
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtCustomSymbol_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the custom code textbox is changed
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtCustomCode_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the password card is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void CardPassword_Click(object sender, RoutedEventArgs e)
    {
        Title = $"{_controller.Localizer["AccountSettings"]} - {_controller.Localizer["Password", "Field"]}";
        ViewStack.ChangePage("Password");
    }

    /// <summary>
    /// Occurs when the back button on the password page is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void BtnPasswordBack_Click(object sender, RoutedEventArgs e)
    {
        Title = _controller.Localizer["AccountSettings"];
        ViewStack.ChangePage("Main");
    }

    /// <summary>
    /// Occurs when the remove button on the password page is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void BtnPasswordRemove_Click(object sender, RoutedEventArgs e)
    {
        _controller.SetRemovePassword();
        InfoBadgePassword.Visibility = Visibility.Visible;
        InfoBadgePassword.Style = (Style)App.Current.Resources["SuccessDotInfoBadgeStyle"];
        Title = _controller.Localizer["AccountSettings"];
        ViewStack.ChangePage("Main");
        BtnPasswordRemove.Visibility = Visibility.Collapsed;
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