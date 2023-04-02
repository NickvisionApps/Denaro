using NickvisionMoney.Shared.Helpers;
using System.Collections.Generic;

namespace NickvisionMoney.Shared.Controllers;

public class DashboardViewController
{
    private List<AccountViewController> _openAccounts;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }

    /// <summary>
    /// Constructs a DashboardViewController
    /// </summary>
    ///  <param name="localizer">The Localizer of the app</param>
    public DashboardViewController(List<AccountViewController> openAccounts, Localizer localizer)
    {
        _openAccounts = openAccounts;
        Localizer = localizer;
    }
}
