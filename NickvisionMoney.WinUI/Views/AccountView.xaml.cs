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
        LblTitle.Text = _controller.AccountTitle;
    }
}
