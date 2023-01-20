using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

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
        PrimaryButtonText = _controller.Localizer["OK"];
        LblDescription.Text = _controller.Localizer["TransferDescription"];
        ToolTipService.SetToolTip(BtnSelectAccount, _controller.Localizer["SelectAccount"]);
        TxtDestinationAccount.Header = _controller.Localizer["DestinationAccount", "Field"];
        TxtDestinationAccount.PlaceholderText = _controller.Localizer["DestinationAccount", "Placeholder"];
        TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} - {_controller.CultureForNumberString.NumberFormat.CurrencySymbol} ({_controller.CultureForNumberString.NumberFormat.NaNSymbol})";
        TxtAmount.PlaceholderText = _controller.Localizer["Amount", "Placeholder"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        //Load Transfer
        TxtAmount.Text = _controller.Transfer.SourceAmount.ToString("N2", _controller.CultureForNumberString);
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
        var checkStatus = _controller.UpdateTransfer(TxtDestinationAccount.Text, TxtAmount.Text);
        TxtDestinationAccount.Header = _controller.Localizer["DestinationAccount", "Field"];
        TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} - {_controller.CultureForNumberString.NumberFormat.CurrencySymbol} {(string.IsNullOrEmpty(_controller.CultureForNumberString.NumberFormat.NaNSymbol) ? "" : $"({_controller.CultureForNumberString.NumberFormat.NaNSymbol})")}";
        if (checkStatus == TransferCheckStatus.Valid)
        {
            TxtErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
        else
        {
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidDestPath))
            {
                TxtDestinationAccount.Header = _controller.Localizer["DestinationAccount", "Invalid"];
            }
            if (checkStatus.HasFlag(TransferCheckStatus.InvalidAmount))
            {
                TxtAmount.Header = _controller.Localizer["Amount", "Invalid"];
            }
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
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
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the amount textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e) => Validate();
}
