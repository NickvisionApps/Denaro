using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using System;
using System.Threading.Tasks;

namespace NickvisionMoney.WinUI.Views;

public sealed partial class GroupDialog : ContentDialog
{
    private readonly GroupDialogController _controller;

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
        //Load Group
        TxtName.Text = _controller.Group.Name;
        TxtDescription.Text = _controller.Group.Description;
    }

    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        if(result == ContentDialogResult.None)
        {
            _controller.Accepted = false;
            return false;
        }
        else if(result == ContentDialogResult.Primary)
        {
            var checkStatus = _controller.UpdateGroup(TxtName.Text, TxtDescription.Text);
            if (checkStatus != GroupCheckStatus.Valid)
            {
                //Reset UI
                TxtName.Header = _controller.Localizer["Name"];
                VisualStateManager.GoToState(TxtName, "ValidState", false);
                TxtDescription.Header = _controller.Localizer["Description"];
                VisualStateManager.GoToState(TxtDescription, "ValidState", false);
                if (checkStatus == GroupCheckStatus.EmptyName)
                {
                    TxtName.Header = _controller.Localizer["Name", "Empty"];
                    VisualStateManager.GoToState(TxtName, "InvalidState", false);
                }
                else if (checkStatus == GroupCheckStatus.EmptyDescription)
                {
                    TxtDescription.Header = _controller.Localizer["Description", "Empty"];
                    VisualStateManager.GoToState(TxtDescription, "InvalidState", false);
                }
                else if (checkStatus == GroupCheckStatus.NameExists)
                {
                    TxtName.Header = _controller.Localizer["Name", "Exists"];
                    VisualStateManager.GoToState(TxtName, "InvalidState", false);
                }
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
}
