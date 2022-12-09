using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a PreferencesView
/// </summary>
public class PreferencesViewController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }

    /// <summary>
    /// Constructs a PreferencesViewController
    /// </summary>
    public PreferencesViewController(Localizer localizer)
    {
        Localizer = localizer;
    }

    /// <summary>
    /// The prefered theme of the application
    /// </summary>
    public Theme Theme
    {
        get => Configuration.Current.Theme;

        set => Configuration.Current.Theme = value;
    }

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    public void SaveConfiguration() => Configuration.Current.Save();
}