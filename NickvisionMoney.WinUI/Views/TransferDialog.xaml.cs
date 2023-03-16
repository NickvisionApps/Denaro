using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Controls;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing a Transfer
/// </summary>
public sealed partial class TransferDialog : ContentDialog
{
    private readonly TransferDialogController _controller;
    private readonly Action<object> _initializeWithWindow;

    /// <summary>
    /// Constructs a TransferDialog
    /// </summary>
    /// <param name="controller">The TransferDialogController</param>
    /// <param name="initializeWithWindow">The Action<object> callback for InitializeWithWindow</param>
    public TransferDialog(TransferDialogController controller, Action<object> initializeWithWindow)
    {
        InitializeComponent();
        _controller = controller;
        _initializeWithWindow = initializeWithWindow;
        //Localize Strings
        Title = _controller.Localizer["Transfer"];
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer["Transfer"];
        LblDescription.Text = _controller.Localizer["TransferDescription"];
        ToolTipService.SetToolTip(BtnSelectAccount, _controller.Localizer["DestinationAccount", "Placeholder"]);
        ToolTipService.SetToolTip(BtnRecentAccounts, _controller.Localizer["RecentAccounts"]);
        LblRecentAccounts.Text = _controller.Localizer["RecentAccounts"];
        TxtDestinationAccount.Header = _controller.Localizer["DestinationAccount", "Field"];
        TxtDestinationAccount.PlaceholderText = _controller.Localizer["DestinationAccount", "Placeholder"];
        ToolTipService.SetToolTip(BtnApplyDestinationPassword, _controller.Localizer["Apply"]);
        TxtDestinationPassword.Header = _controller.Localizer["DestinationPassword", "Field"];
        TxtDestinationPassword.PlaceholderText = _controller.Localizer["Password", "Placeholder"];
        TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} - {_controller.CultureForSourceNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForSourceNumberString.NumberFormat.NaNSymbol})";
        TxtAmount.PlaceholderText = _controller.Localizer["Amount", "Placeholder"];
        TxtSourceCurrency.PlaceholderText = _controller.Localizer["EnterConversionRate"];
        TxtDestCurrency.PlaceholderText = _controller.Localizer["EnterConversionRate"];
        TxtConversionResult.Header = _controller.Localizer["Result"];
        TxtConversionResult.Text = _controller.Localizer["NotAvailable"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        //Load Transfer
        foreach (var recentAccount in _controller.RecentAccounts)
        {
            var bgColorString = _controller.GetColorForAccountType(recentAccount.Type);
            var bgColorStrArray = new Regex(@"[0-9]+,[0-9]+,[0-9]+").Match(bgColorString).Value.Split(",");
            var luma = int.Parse(bgColorStrArray[0]) / 255.0 * 0.2126 + int.Parse(bgColorStrArray[1]) / 255.0 * 0.7152 + int.Parse(bgColorStrArray[2]) / 255.0 * 0.0722;
            var actionRow = new ActionRow(recentAccount.Name, recentAccount.Path);
            var typeBox = new Border()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(-10, 0, 6, 0),
                Background = new SolidColorBrush((Color)ColorHelpers.FromRGBA(bgColorString)!),
                CornerRadius = new CornerRadius(12)
            };
            var typeLabel = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0),
                Foreground = new SolidColorBrush(luma < 0.5 ? Colors.White : Colors.Black),
                FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"],
                Text = "\uE8C7"
            };
            typeBox.Child = typeLabel;
            DockPanel.SetDock(typeBox, Dock.Left);
            actionRow.Children.Insert(0, typeBox);
            ListRecentAccounts.Items.Add(actionRow);
        }
        TxtAmount.Text = _controller.Transfer.SourceAmount.ToAmountString(_controller.CultureForSourceNumberString, false);
        Validate();
    }

    /// <summary>
    /// Shows the TransferDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            _controller.Accepted = false;
            return false;
        }
        _controller.Accepted = true;
        return true;
    }

    /// <summary>
    /// Validate the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateTransfer(TxtDestinationAccount.Text, TxtDestinationPassword.Password, TxtAmount.Text, TxtSourceCurrency.Text, TxtDestCurrency.Text);
        TxtDestinationAccount.Header = _controller.Localizer["DestinationAccount", "Field"];
        TxtDestinationPassword.Header = _controller.Localizer["DestinationPassword", "Field"];
        TxtAmount.Visibility = Visibility.Visible;
        TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} - {_controller.CultureForSourceNumberString.NumberFormat.CurrencySymbol} {(string.IsNullOrEmpty(_controller.CultureForSourceNumberString.NumberFormat.NaNSymbol) ? "" : $"({_controller.CultureForSourceNumberString.NumberFormat.NaNSymbol})")}";
        TxtSourceCurrency.Header = _controller.SourceCurrencyCode;
        TxtDestCurrency.Header = _controller.DestinationCurrencyCode ?? "";
        if (checkStatus == TransferCheckStatus.Valid)
        {
            TxtConversionResult.Text = _controller.Transfer.DestinationAmount.ToAmountString(_controller.CultureForDestNumberString);
            TxtErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
        else
        {
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidDestPath))
            {
                TxtDestinationAccount.Header = _controller.Localizer["DestinationAccount", "Invalid"];
                TxtAmount.Visibility = Visibility.Collapsed;
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountRequiresPassword))
            {
                BoxDestinationPassword.Visibility = Visibility.Visible;
                TxtDestinationPassword.Header = _controller.Localizer["DestinationPassword", "Required"];
                TxtAmount.Visibility = Visibility.Collapsed;
            }
            if (checkStatus.HasFlag(TransferCheckStatus.DestAccountPasswordInvalid))
            {
                TxtDestinationPassword.Header = _controller.Localizer["DestinationPassword", "Invalid"];
                TxtAmount.Visibility = Visibility.Collapsed;
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidAmount))
            {
                TxtAmount.Header = _controller.Localizer["Amount", "Invalid"];
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidConversionRate))
            {
                BoxConversionRate.Visibility = Visibility.Visible;
                TxtConversionResult.Visibility = Visibility.Visible;
                TxtSourceCurrency.Header = $"{_controller.SourceCurrencyCode} ({_controller.Localizer["ConversionNeeded"]})";
                TxtDestCurrency.Header = $"{_controller.DestinationCurrencyCode} ({_controller.Localizer["ConversionNeeded"]})";
                TxtConversionResult.Text = _controller.Localizer["NotAvailable"];
            }
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
        if (!checkStatus.HasFlag(TransferCheckStatus.DestAccountRequiresPassword) && !checkStatus.HasFlag(TransferCheckStatus.DestAccountPasswordInvalid))
        {
            TxtDestinationPassword.IsEnabled = false;
            BtnApplyDestinationPassword.IsEnabled = false;
        }
    }

    /// <summary>
    /// Occurs when the select account button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void SelectAccount(object sender, RoutedEventArgs e)
    {
        var fileOpenPicker = new FileOpenPicker();
        _initializeWithWindow(fileOpenPicker);
        fileOpenPicker.FileTypeFilter.Add(".nmoney");
        fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
        {
            TxtDestinationAccount.Text = file.Path;
            BoxDestinationPassword.Visibility = Visibility.Collapsed;
            TxtDestinationPassword.Password = "";
            TxtDestinationPassword.IsEnabled = true;
            BtnApplyDestinationPassword.IsEnabled = true;
            TxtAmount.Text = "";
            BoxConversionRate.Visibility = Visibility.Collapsed;
            TxtConversionResult.Visibility = Visibility.Collapsed;
            TxtSourceCurrency.Text = "";
            TxtDestCurrency.Text = "";
            Validate();
        }
    }

    /// <summary>
    /// Occurs when an account is selected from the list of recent accounts
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void ListRecentAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FlyoutRecentAccounts.Hide();
        if (ListRecentAccounts.SelectedIndex != -1)
        {
            TxtDestinationAccount.Text = _controller.RecentAccounts[ListRecentAccounts.SelectedIndex].Path;
            BoxDestinationPassword.Visibility = Visibility.Collapsed;
            TxtDestinationPassword.Password = "";
            TxtDestinationPassword.IsEnabled = true;
            BtnApplyDestinationPassword.IsEnabled = true;
            TxtAmount.Text = "";
            BoxConversionRate.Visibility = Visibility.Collapsed;
            TxtConversionResult.Visibility = Visibility.Collapsed;
            TxtSourceCurrency.Text = "";
            TxtDestCurrency.Text = "";
            Validate();
            ListRecentAccounts.SelectedIndex = -1;
        }
    }

    /// <summary>
    /// Occurs when the destination account password passwordbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void TxtDestinationPassword_PasswordChanged(object sender, RoutedEventArgs e) => BtnApplyDestinationPassword.IsEnabled = TxtDestinationPassword.Password.Length > 0 ? true : false;

    /// <summary>
    /// Occurs when the apply destination password button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ApplyDestinationPassword(object sender, RoutedEventArgs e)
    {
        BtnApplyDestinationPassword.IsEnabled = false;
        Validate();
    }

    /// <summary>
    /// Occurs when a key is pressed on the amount textbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxtAmount_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (e.Key == VirtualKey.Decimal || e.Key == VirtualKey.Separator || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (e.Key == (VirtualKey)188 || e.Key == (VirtualKey)190)))
            {
                if(!TxtAmount.Text.Contains(_controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator))
                {
                    var position = TxtAmount.SelectionStart;
                    TxtAmount.Text = TxtAmount.Text.Remove(position - 1, 1);
                    TxtAmount.Text = TxtAmount.Text.Insert(position - 1, _controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator);
                    TxtAmount.SelectionLength = 0;
                    TxtAmount.SelectionStart = position + Math.Min(_controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator.Length, 2);
                }
                e.Handled = true;
            }
        }
        e.Handled = false;
    }

    /// <summary>
    /// Occurs when the amount textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e) => Validate();

    /// <summary>
    /// Occurs when a key is pressed on the source currency textbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxtSourceCurrency_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (e.Key == VirtualKey.Decimal || e.Key == VirtualKey.Separator || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (e.Key == (VirtualKey)188 || e.Key == (VirtualKey)190)))
            {
                if (!TxtSourceCurrency.Text.Contains(_controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator))
                {
                    var position = TxtSourceCurrency.SelectionStart;
                    TxtSourceCurrency.Text = TxtSourceCurrency.Text.Remove(position - 1, 1);
                    TxtSourceCurrency.Text = TxtSourceCurrency.Text.Insert(position - 1, _controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator);
                    TxtSourceCurrency.SelectionLength = 0;
                    TxtSourceCurrency.SelectionStart = position + Math.Min(_controller.CultureForSourceNumberString.NumberFormat.CurrencyDecimalSeparator.Length, 2);
                }
                e.Handled = true;
            }
        }
        e.Handled = false;
    }

    /// <summary>
    /// Occurs when the source currency textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtSourceCurrency_TextChanged(object sender, TextChangedEventArgs e) => Validate();

    /// <summary>
    /// Occurs when a key is pressed on the destination currency textbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxtDestCurrency_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (_controller.InsertSeparator != InsertSeparator.Off)
        {
            if (e.Key == VirtualKey.Decimal || e.Key == VirtualKey.Separator || (_controller.InsertSeparator == InsertSeparator.PeriodComma && (e.Key == (VirtualKey)188 || e.Key == (VirtualKey)190)))
            {
                if (!TxtDestCurrency.Text.Contains(_controller.CultureForDestNumberString!.NumberFormat.CurrencyDecimalSeparator))
                {
                    var position = TxtDestCurrency.SelectionStart;
                    TxtDestCurrency.Text = TxtDestCurrency.Text.Remove(position - 1, 1);
                    TxtDestCurrency.Text = TxtDestCurrency.Text.Insert(position - 1, _controller.CultureForDestNumberString.NumberFormat.CurrencyDecimalSeparator);
                    TxtDestCurrency.SelectionLength = 0;
                    TxtDestCurrency.SelectionStart = position + Math.Min(_controller.CultureForDestNumberString.NumberFormat.CurrencyDecimalSeparator.Length, 2);
                }
                e.Handled = true;
            }
        }
        e.Handled = false;
    }

    /// <summary>
    /// Occurs when the destination currency textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtDestCurrency_TextChanged(object sender, TextChangedEventArgs e) => Validate();
}
