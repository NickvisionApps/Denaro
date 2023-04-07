using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Threading.Tasks;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public sealed partial class GroupDialog : ContentDialog
{
    private bool _constructing;
    private readonly GroupDialogController _controller;
    private Color _selectedColor;

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">The GroupDialogController</param>
    public GroupDialog(GroupDialogController controller)
    {
        InitializeComponent();
        _constructing = true;
        _controller = controller;
        //Localize Strings
        Title = _controller.Localizer["Group"];
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer[_controller.IsEditing ? "Apply" : "Add"];
        TxtName.Header = _controller.Localizer["Name", "Field"];
        TxtName.PlaceholderText = _controller.Localizer["Name", "Placeholder"];
        TxtDescription.Header = _controller.Localizer["Description", "Field"];
        TxtDescription.PlaceholderText = _controller.Localizer["Description", "Placeholder"];
        LblColor.Text = _controller.Localizer["Color", "Field"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        //Load Group
        TxtName.Text = _controller.Group.Name;
        TxtDescription.Text = _controller.Group.Description;
        _selectedColor = (Color)ColorHelpers.FromRGBA(_controller.Group.RGBA)!;
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// The selected color for the group
    /// </summary>
    public Color SelectedColor
    {
        get => _selectedColor;

        set
        {
            _selectedColor = value;
            if (!_constructing)
            {
                Validate();
            }
        }
    }

    /// <summary>
    /// Shows the GroupDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        return result != ContentDialogResult.None;
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateGroup(TxtName.Text, TxtDescription.Text, ColorHelpers.ToRGBA(SelectedColor));
        TxtName.Header = _controller.Localizer["Name", "Field"];
        if (checkStatus == GroupCheckStatus.Valid)
        {
            TxtErrors.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
        else
        {
            if (checkStatus == GroupCheckStatus.EmptyName)
            {
                TxtName.Header = _controller.Localizer["Name", "Empty"];
            }
            else if (checkStatus == GroupCheckStatus.NameExists)
            {
                TxtName.Header = _controller.Localizer["Name", "Exists"];
            }
            TxtErrors.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
    }

    /// <summary>
    /// Occurs when the name textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_constructing)
        {
            while (TxtName.Text.Contains(';'))
            {
                TxtName.Text = TxtName.Text.Remove(TxtName.Text.IndexOf(';'), 1);
                TxtName.Select(TxtName.Text.Length, 0);
            }
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the description textbox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtDescription_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_constructing)
        {
            while (TxtDescription.Text.Contains(';'))
            {
                TxtDescription.Text = TxtDescription.Text.Remove(TxtDescription.Text.IndexOf(';'), 1);
                TxtDescription.Select(TxtDescription.Text.Length, 0);
            }
            Validate();
        }
    }
}
