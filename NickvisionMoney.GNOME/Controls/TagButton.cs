using System;

namespace NickvisionMoney.GNOME.Controls;

public class TagButton : Gtk.ToggleButton
{
    public int Index;
    
    public event EventHandler<int> OnAddTag;
    public event EventHandler<int> OnRemoveTag;
    
    public TagButton(int index, string tag)
    {
        Index = index;
        SetLabel(tag);
        OnToggled += (sender, e) =>
        {
            if (GetActive())
            {
                OnAddTag?.Invoke(this, Index);
            }
            else
            {
                OnRemoveTag?.Invoke(this, Index);
            }
        };
    }
}