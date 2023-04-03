using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;

namespace NickvisionMoney.WinUI.Views;

public sealed partial class DashboardPage : UserControl
{
    private DashboardViewController _controller;

    public DashboardPage(DashboardViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        LblTitle.Text = _controller.Localizer["Dashboard"];
    }
}
