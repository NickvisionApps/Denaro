using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using System;
using System.Threading.Tasks;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public sealed partial class GroupDialog : ContentDialog
{
    private readonly GroupDialogController _controller;

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">The GroupDialogController</param>
    public GroupDialog(GroupDialogController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        Title = $"{_controller.Localizer["Group"]} - {_controller.Group.Id}";
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer["OK"];
        TxtName.Header = _controller.Localizer["Name", "Field"];
        TxtName.PlaceholderText = _controller.Localizer["Name", "Placeholder"];
        TxtDescription.Header = _controller.Localizer["Description", "Field"];
        TxtDescription.PlaceholderText = _controller.Localizer["Description", "Placeholder"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        //Load Group
        TxtName.Text = _controller.Group.Name;
        TxtDescription.Text = _controller.Group.Description;
        if(string.IsNullOrEmpty(_controller.Group.Name))
        {
            TxtName.Header = _controller.Localizer["Name", "Empty"];
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
    }

    /// <summary>
    /// Shows the GroupDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        if(result == ContentDialogResult.None)
        {
            _controller.Accepted = false;
            return false;
        }
        _controller.Accepted = true;
        return true;
    }

    /// <summary>
    /// Occurs when the name textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
    {
        var checkStatus = _controller.UpdateGroup(TxtName.Text, TxtDescription.Text);
        if(checkStatus == GroupCheckStatus.Valid)
        {
            TxtName.Header = _controller.Localizer["Name", "Field"];
            TxtErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
        else if(checkStatus == GroupCheckStatus.EmptyName)
        {
            TxtName.Header = _controller.Localizer["Name", "Empty"];
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
        else if(checkStatus == GroupCheckStatus.NameExists)
        {
            TxtName.Header = _controller.Localizer["Name", "Exists"];
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
    }
}
