using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using System.IO;

namespace NickvisionMoney.WinUI.Views;

public sealed partial class AccountView : UserControl
{
    private readonly AccountViewController _controller;

    public AccountView(AccountViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        LblTotalTitle.Text = $"{_controller.Localizer["Total"]}:";
        BtnNewTransaction.Label = _controller.Localizer["NewTransaction"];
        ToolTipService.SetToolTip(BtnNewTransaction, _controller.Localizer["NewTransaction", "Tooltip"]);
        BtnNewGroup.Label = _controller.Localizer["NewGroup"];
        ToolTipService.SetToolTip(BtnNewGroup, _controller.Localizer["NewGroup", "Tooltip"]);
        BtnTransferMoney.Label = _controller.Localizer["TransferMoney"];
        ToolTipService.SetToolTip(BtnTransferMoney, _controller.Localizer["TransferMoney", "Tooltip"]);
        BtnImportFromFile.Label = _controller.Localizer["ImportFromFile"];
        ToolTipService.SetToolTip(BtnImportFromFile, _controller.Localizer["ImportFromFile", "Tooltip"]);
        BtnExportToFile.Label = _controller.Localizer["ExportToFile"];
        ToolTipService.SetToolTip(BtnExportToFile, _controller.Localizer["ExportToFile", "Tooltip"]);
        //Load Account
        LblTitle.Text = _controller.AccountTitle;
        LblTotalAmount.Text = _controller.AccountTotal;
    }
}
