using NickvisionMoney.Shared.Controllers;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog : Adw.MessageDialog
{
    private bool _constructing;
    private readonly GroupDialogController _controller;
    private readonly Adw.PreferencesGroup _grpGroup;
    private readonly Adw.EntryRow _rowName;
    private readonly Adw.EntryRow _rowDescription;
    private readonly Gtk.EventControllerKey _nameKeyController;
    private readonly Gtk.EventControllerKey _descriptionKeyController;

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public GroupDialog(GroupDialogController controller, Gtk.Window parentWindow)
    {
        _constructing = true;
        _controller = controller;
        //Dialog Settings
        SetTitle(_controller.Localizer["Group"]);
        SetTransientFor(parentWindow);
        SetDefaultSize(360, -1);
        SetHideOnClose(true);
        SetModal(true);
        AddResponse("cancel", _controller.Localizer["Cancel"]);
        SetCloseResponse("cancel");
        AddResponse("ok", _controller.Localizer[_controller.IsEditing ? "Apply" : "Add"]);
        SetDefaultResponse("ok");
        SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
        //Preferences Group
        _grpGroup = Adw.PreferencesGroup.New();
        //Name
        _rowName = Adw.EntryRow.New();
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        _rowName.SetInputHints(Gtk.InputHints.Spellcheck);
        _rowName.SetActivatesDefault(true);
        _rowName.OnNotify += (sender, e) =>
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
        _rowName.AddController(_nameKeyController);
        _grpGroup.Add(_rowName);
        //Description
        _rowDescription = Adw.EntryRow.New();
        _rowDescription.SetTitle(_controller.Localizer["Description", "Field"]);
        _rowDescription.SetInputHints(Gtk.InputHints.Spellcheck);
        _rowDescription.SetActivatesDefault(true);
        _rowDescription.OnNotify += (sender, e) =>
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
        _rowDescription.AddController(_descriptionKeyController);
        _grpGroup.Add(_rowDescription);
        //Layout
        SetExtraChild(_grpGroup);
        //Load Group
        _rowName.SetText(_controller.Group.Name);
        _rowDescription.SetText(_controller.Group.Description);
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var checkStatus = _controller.UpdateGroup(_rowName.GetText(), _rowDescription.GetText());
        _rowName.RemoveCssClass("error");
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        if (checkStatus == GroupCheckStatus.Valid)
        {
            SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus == GroupCheckStatus.EmptyName)
            {
                _rowName.AddCssClass("error");
                _rowName.SetTitle(_controller.Localizer["Name", "Empty"]);
            }
            else if (checkStatus == GroupCheckStatus.NameExists)
            {
                _rowName.AddCssClass("error");
                _rowName.SetTitle(_controller.Localizer["Name", "Exists"]);
            }
            SetResponseEnabled("ok", false);
        }
    }
}