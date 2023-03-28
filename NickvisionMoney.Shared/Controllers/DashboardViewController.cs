using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;

namespace NickvisionMoney.Shared.Controllers;

public class DashboardViewController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }

    /// <summary>
    /// Constructs a DashboardViewController
    /// </summary>
    ///  <param name="localizer">The Localizer of the app</param>
    public DashboardViewController(Localizer localizer)
    {
        Localizer = localizer;
    }

    public void AddAccount(Account account)
    {

    }
}
