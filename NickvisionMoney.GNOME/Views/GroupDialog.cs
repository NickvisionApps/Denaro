using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using System;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog : Adw.Window
{
    private bool _constructing;
    private readonly GroupDialogController _controller;

    [Gtk.Connect] private readonly Adw.EntryRow _nameRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Gtk.Button _applyButton;

    private readonly Gtk.EventControllerKey _nameKeyController;
    private readonly Gtk.EventControllerKey _descriptionKeyController;

    public event EventHandler? OnApply;

    private GroupDialog(Gtk.Builder builder, GroupDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _constructing = true;
        _controller = controller;
        //Build UI
        builder.Connect(this);
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
        //Apply button
        _applyButton.SetLabel(_controller.Localizer[_controller.IsEditing ? "Apply" : "Add"]);
        _applyButton.OnClicked += (sender, e) => {
            _controller.Accepted = true;
            OnApply?.Invoke(this, EventArgs.Empty);
        };
        //Load Group
        _nameRow.SetText(_controller.Group.Name);
        _descriptionRow.SetText(_controller.Group.Description);
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public GroupDialog(GroupDialogController controller, Gtk.Window parent) : this(Builder.FromFile("group_dialog.ui", controller.Localizer), controller, parent)
    {
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateGroup(_nameRow.GetText(), _descriptionRow.GetText());
        _nameRow.RemoveCssClass("error");
        _nameRow.SetTitle(_controller.Localizer["Name", "Field"]);
        if (checkStatus == GroupCheckStatus.Valid)
        {
            _applyButton.SetSensitive(true);
        }
        else
        {
            if (checkStatus == GroupCheckStatus.EmptyName)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_controller.Localizer["Name", "Empty"]);
            }
            else if (checkStatus == GroupCheckStatus.NameExists)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_controller.Localizer["Name", "Exists"]);
            }
            _applyButton.SetSensitive(false);
        }
    }
}