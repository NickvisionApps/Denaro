using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

public class GroupRow : Adw.ActionRow
{
    [DllImport("adwaita-1")]
    private static extern void adw_preferences_row_set_use_markup(nint row, bool use_markup);

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    public event EventHandler<(int Id, bool Filter)>? FilterChanged;
    /// <summary>
    /// Occurs when the edit button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    private readonly Gtk.CheckButton _chkFilter;
    private readonly Gtk.Label _lblAmount;
    private readonly Gtk.Button _btnEdit;
    private readonly Gtk.Button _btnDelete;
    private readonly Gtk.Box _box;

    public GroupRow(Group group, Localizer localizer, bool filterActive)
    {
        //Row Settings
        adw_preferences_row_set_use_markup(this.Handle, false);
        SetTitle(group.Name);
        SetSubtitle(group.Description);
        //Filter Checkbox
        _chkFilter = Gtk.CheckButton.New();
        _chkFilter.SetActive(filterActive);
        _chkFilter.AddCssClass("selection-mode");
        _chkFilter.OnToggled += (Gtk.CheckButton sender, EventArgs e) => FilterChanged?.Invoke(this, ((int)group.Id == 0 ? -1 : (int)group.Id, _chkFilter.GetActive()));
        AddPrefix(_chkFilter);
        //Amount Label
        _lblAmount = Gtk.Label.New(group.Balance.ToString("C"));
        _lblAmount.AddCssClass(group.Balance >= 0 ? "success" : "error");
        _lblAmount.AddCssClass(group.Balance >= 0 ? "money-income" : "money-expense");
        _lblAmount.SetValign(Gtk.Align.Center);
        //Edit Button
        _btnEdit = Gtk.Button.NewFromIconName("document-edit-symbolic");
        _btnEdit.SetValign(Gtk.Align.Center);
        _btnEdit.AddCssClass("flat");
        _btnEdit.SetTooltipText(localizer["Edit", "GroupRow"]);
        //_btnEdit.OnClicked +=
        SetActivatableWidget(_btnEdit);
        //Delete Button
        _btnDelete = Gtk.Button.NewFromIconName("user-trash-symbolic");
        _btnDelete.SetValign(Gtk.Align.Center);
        _btnDelete.AddCssClass("flat");
        _btnDelete.SetTooltipText(localizer["Delete", "GroupRow"]);
        //_btnDelete.OnClick +=
        //Box
        _box = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _box.Append(_lblAmount);
        if(group.Id != 0)
        {
            _box.Append(_btnEdit);
            _box.Append(_btnDelete);
        }
        AddSuffix(_box);
    }
}