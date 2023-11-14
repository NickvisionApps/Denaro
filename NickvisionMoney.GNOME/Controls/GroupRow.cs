using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a group
/// </summary>
public partial class GroupRow : Adw.ActionRow
{
    private Group _group;
    private bool _filterActive;
    private string _defaultColor;
    private CultureInfo _cultureAmount;
    private bool _useNativeDigits;

    [Gtk.Connect] private readonly Gtk.Overlay _filterOverlay;
    [Gtk.Connect] private readonly Gtk.Image _filterCheckBackground;
    [Gtk.Connect] private readonly Gtk.CheckButton _filterCheckButton;
    [Gtk.Connect] private readonly Gtk.Label _amountLabel;
    [Gtk.Connect] private readonly Gtk.Button _editButton;

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

    private GroupRow(Gtk.Builder builder, Group group, CultureInfo cultureAmount, bool useNativeDigits, bool filterActive, string defaultColor) : base(builder.GetPointer("_root"), false)
    {
        _cultureAmount = cultureAmount;
        _defaultColor = defaultColor;
        _useNativeDigits = useNativeDigits;
        //Build UI
        builder.Connect(this);
        var sizeGroup = Gtk.SizeGroup.New(Gtk.SizeGroupMode.Both);
        sizeGroup.AddWidget(_filterOverlay);
        sizeGroup.AddWidget(_filterCheckButton);
        //Filter Checkbox
        _filterCheckButton.OnToggled += FilterToggled;
        _filterCheckButton.OnToggled += (sender, e) =>
        {
            if (_filterCheckButton.GetActive())
            {
                _filterCheckBackground.SetVisible(true);
                _filterCheckButton.RemoveCssClass("group-filter-disabled");
            }
            else
            {
                _filterCheckBackground.SetVisible(false);
                _filterCheckButton.AddCssClass("group-filter-disabled");
            }
        };
        //Buttons
        _editButton.SetVisible(group.Id != 0);
        _amountLabel.SetVexpand(group.Id == 0);
        _editButton.OnClicked += Edit;
        UpdateRow(group, defaultColor, cultureAmount, filterActive);
    }

    /// <summary>
    /// Constructs a group row 
    /// </summary>
    /// <param name="group">The Group to display</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="useNativeDigits">Whether to use native digits</param>
    /// <param name="filterActive">Whether or not the filter checkbutton should be active</param>
    /// <param name="defaultColor">The default group color</param>
    public GroupRow(Group group, CultureInfo cultureAmount, bool useNativeDigits, bool filterActive, string defaultColor) : this(Builder.FromFile("group_row.ui"), group, cultureAmount, useNativeDigits, filterActive, defaultColor)
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
    /// <param name="defaultColor">The default color for the row</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="filterActive">Whether or not the filter checkbox is active</param>
    public void UpdateRow(Group group, string defaultColor, CultureInfo cultureAmount, bool filterActive)
    {
        _group = group;
        _defaultColor = defaultColor;
        _filterActive = filterActive;
        _cultureAmount = cultureAmount;
        //Color
        if (!GdkHelpers.RGBA.Parse(out var color, _group.RGBA))
        {
            GdkHelpers.RGBA.Parse(out color, _defaultColor);
        }
        //Row Settings
        SetTitle(_group.Name);
        SetSubtitle(_group.Description);
        //Filter Checkbox
        var red = (int)(color!.Value.Red * 255);
        var green = (int)(color.Value.Green * 255);
        var blue = (int)(color.Value.Blue * 255);
        using var pixbuf = GdkPixbuf.Pixbuf.New(GdkPixbuf.Colorspace.Rgb, false, 8, 1, 1);
        if (uint.TryParse(red.ToString("X2") + green.ToString("X2") + blue.ToString("X2") + "FF", NumberStyles.HexNumber, null, out var colorPixbuf))
        {
            pixbuf.Fill(colorPixbuf);
            _filterCheckBackground.SetFromPixbuf(pixbuf);
        }
        var luma = color.Value.Red * 0.2126 + color.Value.Green * 0.7152 + color.Value.Blue * 0.0722;
        _filterCheckButton.AddCssClass(luma > 0.5 ? "group-filter-check-dark" : "group-filter-check-light");
        _filterCheckButton.RemoveCssClass(luma > 0.5 ? "group-filter-check-light" : "group-filter-check-dark");
        _filterCheckButton.SetActive(_filterActive);
        //Amount Label
        _amountLabel.SetLabel($"{(_group.Balance >= 0 ? "+  " : "−  ")}{_group.Balance.ToAmountString(_cultureAmount, _useNativeDigits)}");
        _amountLabel.AddCssClass(_group.Balance >= 0 ? "denaro-income" : "denaro-expense");
        _amountLabel.RemoveCssClass(_group.Balance >= 0 ? "denaro-expense" : "denaro-income");
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
}