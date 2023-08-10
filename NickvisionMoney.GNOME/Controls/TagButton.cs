using System;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// Tag toggle button
/// </summary>
public class TagButton : Gtk.ToggleButton
{
    /// <summary>
    /// Tag index in a list of all tags in an account
    /// </summary>
    public int Index { get; init; }
    /// <summary>
    /// Tag string
    /// </summary>
    public string Tag { get; init; }
    
    /// <summary>
    /// Occurs when toggle state has changed
    /// </summary>
    public event EventHandler<(int Index, bool Filter)> FilterChanged;
    
    /// <summary>
    /// Construct TagButton
    /// </summary>
    public TagButton(int index, string tag)
    {
        Index = index;
        Tag = tag;
        SetLabel(tag);
        OnToggled += (sender, e) => FilterChanged?.Invoke(this, (Index, GetActive()));
    }
}