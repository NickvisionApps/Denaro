using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using System.Threading.Tasks;
using System;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.WinUI.Helpers;
using Windows.UI;
using NickvisionMoney.Shared.Models;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing account metadata
/// </summary>
public sealed partial class AccountSettingsDialog : ContentDialog
{
    private readonly AccountSettingsDialogController _controller;

    /// <summary>
    /// Constructs an AccountSettingsDialog
    /// </summary>
    /// <param name="controller">The AccountSettingsDialogController</param>
    public AccountSettingsDialog(AccountSettingsDialogController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        Title = $"{_controller.Localizer["AccountSettings"]}";
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer["OK"];
        TxtName.Header = _controller.Localizer["Name", "Field"];
        TxtName.PlaceholderText = _controller.Localizer["Name", "Placeholder"];
        CmbAccountType.Header = _controller.Localizer["AccountType", "Field"];
        CmbAccountType.Items.Add(_controller.Localizer["AccountType", "Checking"]);
        CmbAccountType.Items.Add(_controller.Localizer["AccountType", "Savings"]);
        CmbAccountType.Items.Add(_controller.Localizer["AccountType", "Business"]);
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        CmbDefaultTransactionType.Header = _controller.Localizer["DefaultTransactionType", "Field"];
        CmbDefaultTransactionType.Items.Add(_controller.Localizer["Income"]);
        CmbDefaultTransactionType.Items.Add(_controller.Localizer["Expense"]);
        //Load Metadata
        TxtName.Text = _controller.Metadata.Name;
        CmbAccountType.SelectedIndex = (int)_controller.Metadata.AccountType;
        CmbDefaultTransactionType.SelectedIndex = (int)_controller.Metadata.DefaultTransactionType;
        TxtName_TextChanged(null, null);
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
            if(_controller.IsFirstTimeSetup)
            {
                return await ShowAsync();
            }
            else
            {
                _controller.Accepted = false;
                return false;
            }
        }
        else if (result == ContentDialogResult.Primary)
        {
            var checkStatus = _controller.UpdateMetadata(TxtName.Text, (AccountType)CmbAccountType.SelectedIndex, false, null, null, (TransactionType)CmbDefaultTransactionType.SelectedIndex);
            if (checkStatus != AccountMetadataCheckStatus.Valid)
            {
                //Reset UI
                TxtName.Header = _controller.Localizer["Name", "Field"];
                if (checkStatus == AccountMetadataCheckStatus.EmptyName)
                {
                    TxtName.Header = _controller.Localizer["Name", "Empty"];
                }
                TxtErrors.Visibility = Visibility.Visible;
                return await ShowAsync();
            }
            else
            {
                _controller.Accepted = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Occurs when the name field's text is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">TextChangedEventArgs?</param>
    private void TxtName_TextChanged(object? sender, TextChangedEventArgs? e)
    {
        if(TxtName.Text.Length == 0)
        {
            LblId.Text = _controller.Localizer["NotAvailable"];
        }
        else
        {
            var split = TxtName.Text.Split(' ');
            if(split.Length == 1)
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
                    LblId.Text = $"{split[0][0]}{split[1][0]}";
                }
            }
        }
    }

    /// <summary>
    /// Occurs when the account type combobox's selection is changed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbAccountType_SelectionChanged(object? sender, SelectionChangedEventArgs e) => BorderId.Background = new SolidColorBrush((Color)ColorHelpers.FromRGBA(_controller.GetColorForAccountType((AccountType)CmbAccountType.SelectedIndex))!);
}
