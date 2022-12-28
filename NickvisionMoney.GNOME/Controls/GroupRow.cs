using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a group
/// </summary>
public partial class GroupRow : Adw.ActionRow
{
    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_preferences_row_set_use_markup(nint row, [MarshalAs(UnmanagedType.I1)] bool use_markup);

    private readonly Group _group;
    private readonly Gtk.CheckButton _chkFilter;
    private readonly Gtk.Label _lblAmount;
    private readonly Gtk.Button _btnEdit;
    private readonly Gtk.Button _btnDelete;
    private readonly Gtk.Box _box;

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

    /// <summary>
    /// Constructs a group row 
    /// </summary>
    /// <param name="group">The Group to display</param>
    /// <param name="localizer">The Localizer for the app</param>
    /// <param name="filterActive">Whether or not the filter checkbutton should be active</param>
    public GroupRow(Group group, Localizer localizer, bool filterActive)
    {
        _group = group;
        //Row Settings
        adw_preferences_row_set_use_markup(Handle, false);
        SetTitle(_group.Name);
        SetSubtitle(_group.Description);
        //Filter Checkbox
        _chkFilter = Gtk.CheckButton.New();
        _chkFilter.SetActive(filterActive);
        _chkFilter.AddCssClass("selection-mode");
        _chkFilter.OnToggled += FilterToggled; 
        AddPrefix(_chkFilter);
        //Amount Label
        _lblAmount = Gtk.Label.New($"{(_group.Balance >= 0 ? "+  " : "-  ")}{Math.Abs(_group.Balance).ToString("C")}");
        _lblAmount.AddCssClass(_group.Balance >= 0 ? "success" : "error");
        _lblAmount.AddCssClass(_group.Balance >= 0 ? "money-income" : "money-expense");
        _lblAmount.SetValign(Gtk.Align.Center);
        //Edit Button
        _btnEdit = Gtk.Button.NewFromIconName("document-edit-symbolic");
        _btnEdit.SetValign(Gtk.Align.Center);
        _btnEdit.AddCssClass("flat");
        _btnEdit.SetTooltipText(localizer["Edit", "GroupRow"]);
        _btnEdit.OnClicked += Edit;
        SetActivatableWidget(_btnEdit);
        //Delete Button
        _btnDelete = Gtk.Button.NewFromIconName("user-trash-symbolic");
        _btnDelete.SetValign(Gtk.Align.Center);
        _btnDelete.AddCssClass("flat");
        _btnDelete.SetTooltipText(localizer["Delete", "GroupRow"]);
        _btnDelete.OnClicked += Delete;
        //Box
        _box = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _box.Append(_lblAmount);
        if(_group.Id != 0)
        {
            _box.Append(_btnEdit);
            _box.Append(_btnDelete);
        }
        AddSuffix(_box);
    }

    /// <summary>
    /// Occurs when the filter checkbutton is toggled
    /// </summary>
    /// <param name="sender">Gtk.CheckButton</param>
    /// <param name="e">EventArgs</param>
    private void FilterToggled(Gtk.CheckButton sender, EventArgs e) => FilterChanged?.Invoke(this, ((int)_group.Id == 0 ? -1 : (int)_group.Id, _chkFilter.GetActive()));

    /// <summary>
    /// Occurs when the edit button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Edit(Gtk.Button sender, EventArgs e) => EditTriggered?.Invoke(this, _group.Id);

    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Delete(Gtk.Button sender, EventArgs e) => DeleteTriggered?.Invoke(this, _group.Id);
}