using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a group
/// </summary>
public partial class GroupRow : Adw.ActionRow, IGroupRowControl
{
    private delegate bool GSourceFunc(nint data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_invoke(nint context, GSourceFunc function, nint data);

    private Group _group;
    private bool _filterActive;
    private CultureInfo _cultureAmount;
    private CultureInfo _cultureDate;

    [Gtk.Connect] private readonly Gtk.CheckButton _filterCheckButton;
    [Gtk.Connect] private readonly Gtk.Label _amountLabel;
    [Gtk.Connect] private readonly Gtk.Button _editButton;
    [Gtk.Connect] private readonly Gtk.Button _deleteButton;
    [Gtk.Connect] private readonly Gtk.FlowBox _flowBox;

    private GSourceFunc _updateCallback;

    /// <summary>
    /// The Id of the Group the row represents
    /// </summary>
    public uint Id => _group.Id;

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    public event EventHandler<(uint Id, bool Filter)>? FilterChanged;
    /// <summary>
    /// Occurs when the edit button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    private GroupRow(Gtk.Builder builder, Group group, CultureInfo cultureAmount, CultureInfo cultureDate, Localizer localizer, bool filterActive) : base(builder.GetPointer("_root"), false)
    {
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        _updateCallback = (x) =>
        {
            //Row Settings
            SetTitle(_group.Name);
            SetSubtitle(_group.Description);
            //Filter Checkbox
            _filterCheckButton.SetActive(_filterActive);
            //Amount Label
            _amountLabel.SetLabel($"{(_group.Balance >= 0 ? "+  " : "-  ")}{Math.Abs(_group.Balance).ToAmountString(_cultureAmount)}");
            _amountLabel.AddCssClass(_group.Balance >= 0 ? "denaro-income" : "denaro-expense");
            if (_group.Id == 0)
            {
                _editButton.SetVisible(false);
                _deleteButton.SetVisible(false);
                _flowBox.SetValign(Gtk.Align.Center);
            }
            return false;
        };
        //Build UI
        builder.Connect(this);
        //Filter Checkbox
        _filterCheckButton.OnToggled += FilterToggled;
        //Edit Button
        _editButton.OnClicked += Edit;
        //Delete Button
        _deleteButton.OnClicked += Delete;
        UpdateRow(group, cultureAmount, cultureDate, filterActive);
    }

    /// <summary>
    /// Constructs a group row 
    /// </summary>
    /// <param name="group">The Group to display</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    /// <param name="localizer">The Localizer for the app</param>
    /// <param name="filterActive">Whether or not the filter checkbutton should be active</param>
    public GroupRow(Group group, CultureInfo cultureAmount, CultureInfo cultureDate, Localizer localizer, bool filterActive) : this(Builder.FromFile("group_row.ui", localizer), group, cultureAmount, cultureDate, localizer, filterActive)
    {
    }

    /// <summary>
    /// Whether or not the filter checkbox is checked
    /// </summary>
    public bool FilterChecked
    {
        get => _filterCheckButton.GetActive();

        set => _filterCheckButton.SetActive(value);
    }

    /// <summary>
    /// Updates the row with the new model
    /// </summary>
    /// <param name="group">The new Group model</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="cultureDate">The culture to use for displaying date strings</param>
    /// <param name="filterActive">Whether or not the filter checkbox is active</param>
    public void UpdateRow(Group group, CultureInfo cultureAmount, CultureInfo cultureDate, bool filterActive)
    {
        _group = group;
        _filterActive = filterActive;
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        g_main_context_invoke(IntPtr.Zero, _updateCallback, IntPtr.Zero);
    }

    /// <summary>
    /// Occurs when the filter checkbutton is toggled
    /// </summary>
    /// <param name="sender">Gtk.CheckButton</param>
    /// <param name="e">EventArgs</param>
    private void FilterToggled(Gtk.CheckButton sender, EventArgs e) => FilterChanged?.Invoke(this, (Id, _filterCheckButton.GetActive()));

    /// <summary>
    /// Occurs when the edit button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Edit(Gtk.Button sender, EventArgs e) => EditTriggered?.Invoke(this, Id);

    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Delete(Gtk.Button sender, EventArgs e) => DeleteTriggered?.Invoke(this, Id);
}