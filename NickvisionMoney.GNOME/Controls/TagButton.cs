using System;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// Tag toggle button
/// </summary>
public class TagButton : Gtk.ToggleButton
{
    /// <summary>
    /// Tag string
    /// </summary>
    public string Tag { get; init; }

    /// <summary>
    /// Occurs when toggle state has changed
    /// </summary>
    public event EventHandler<(string Tag, bool Filter)>? FilterChanged;

    /// <summary>
    /// Construct TagButton
    /// </summary>
    /// <param name="tag">The tag</param>
    public TagButton(string tag)
    {
        Tag = tag;
        SetLabel(tag);
        SetCanShrink(true);
        OnToggled += (sender, e) => FilterChanged?.Invoke(this, (Tag, GetActive()));
    }
}