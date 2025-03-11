using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using System;
using System.Globalization;
using Adw.Internal;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog : Adw.Window
{
    private bool _constructing;
    private readonly GroupDialogController _controller;
    private readonly Gtk.ColorDialog _colorDialog;

    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Adw.EntryRow _nameRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Gtk.ColorDialogButton _colorButton;
    [Gtk.Connect] private readonly Gtk.Button _deleteButton;
    [Gtk.Connect] private readonly Gtk.Button _applyButton;

    private readonly Gtk.EventControllerKey _nameKeyController;
    private readonly Gtk.EventControllerKey _descriptionKeyController;

    /// <summary>
    /// Occurs when the apply button is clicked
    /// </summary>
    public event EventHandler? OnApply;
    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    public event EventHandler? OnDelete;

    private GroupDialog(Gtk.Builder builder, GroupDialogController controller, Gtk.Window parent) : base(builder.GetObject("_root").Handle as WindowHandle)
    {
        _constructing = true;
        _controller = controller;
        //Build UI
        builder.Connect(this);
        var idString = _controller.Group.Id.ToString();
        var nativeDigits = CultureInfo.CurrentCulture.NumberFormat.NativeDigits;
        if (_controller.UseNativeDigits && "0" != nativeDigits[0])
        {
            idString = idString.Replace("0", nativeDigits[0])
                               .Replace("1", nativeDigits[1])
                               .Replace("2", nativeDigits[2])
                               .Replace("3", nativeDigits[3])
                               .Replace("4", nativeDigits[4])
                               .Replace("5", nativeDigits[5])
                               .Replace("6", nativeDigits[6])
                               .Replace("7", nativeDigits[7])
                               .Replace("8", nativeDigits[8])
                               .Replace("9", nativeDigits[9]);
        }
        _titleLabel.SetLabel($"{_("Group")} — {idString}");
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        //Name
        _nameRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _nameKeyController = Gtk.EventControllerKey.New();
        _nameKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _nameKeyController.OnKeyPressed += (sender, e) => { if (e.Keyval == 59) { return true; } return false; };
        _nameRow.AddController(_nameKeyController);
        //Description
        _descriptionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _descriptionKeyController = Gtk.EventControllerKey.New();
        _descriptionKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _descriptionKeyController.OnKeyPressed += (sender, e) => { if (e.Keyval == 59) { return true; } return false; };
        _descriptionRow.AddController(_descriptionKeyController);
        //Color
        _colorDialog = Gtk.ColorDialog.New();
        _colorDialog.SetWithAlpha(false);
        _colorButton.SetDialog(_colorDialog);
        _colorButton.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "rgba")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        //Buttons
        _applyButton.SetLabel(_controller.IsEditing ? _("Apply") : _("Add"));
        _applyButton.OnClicked += (sender, e) =>
        {
            Close();
            OnApply?.Invoke(this, EventArgs.Empty);
        };
        _deleteButton.SetVisible(_controller.IsEditing);
        _deleteButton.OnClicked += (sender, e) =>
        {
            Close();
            OnDelete?.Invoke(this, EventArgs.Empty);
        };
        //Load Group
        _nameRow.SetText(_controller.Group.Name);
        _descriptionRow.SetText(_controller.Group.Description);
        GdkHelpers.RGBA.Parse(out var color, _controller.Group.RGBA);
        _colorButton.SetExtRgba(color!.Value);
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public GroupDialog(GroupDialogController controller, Gtk.Window parent) : this(Builder.FromFile("group_dialog.ui"), controller, parent)
    {
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var color = _colorButton.GetRgba();
        var checkStatus = _controller.UpdateGroup(_nameRow.GetText().Trim(), _descriptionRow.GetText().Trim(), color.ToString());
        _nameRow.RemoveCssClass("error");
        _nameRow.SetTitle(_("Name"));
        if (checkStatus == GroupCheckStatus.Valid)
        {
            _applyButton.SetSensitive(true);
        }
        else
        {
            if (checkStatus == GroupCheckStatus.EmptyName)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_("Name (Empty)"));
            }
            else if (checkStatus == GroupCheckStatus.NameExists)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_("Name (Exists)"));
            }
            _applyButton.SetSensitive(false);
        }
    }
}