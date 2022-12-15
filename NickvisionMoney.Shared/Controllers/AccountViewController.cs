using NickvisionMoney.Shared.Models;
using System.IO;

namespace NickvisionMoney.Shared.Controllers;

public class AccountViewController
{
    private readonly Account _account;

    public string AccountTitle => Path.GetFileNameWithoutExtension(_account.Path);

    public AccountViewController(string path)
    {
        _account = new Account(path);
    }
}
