using NickvisionMoney.Shared.Controllers;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog
{
    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    private readonly GroupDialogController _controller;
    private readonly Adw.MessageDialog _dialog;
    private readonly Adw.PreferencesGroup _grpGroup;
    private readonly Adw.ActionRow _rowName;
    private readonly Gtk.Entry _txtName;
    private readonly Adw.ActionRow _rowDescription;
    private readonly Gtk.Entry _txtDescription;

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public GroupDialog(GroupDialogController controller, Gtk.Window parentWindow)
    {
        _controller = controller;
        //Dialog Settings
        _dialog = Adw.MessageDialog.New(parentWindow, _controller.Localizer["Group"], "");
        _dialog.SetDefaultSize(450, -1);
        _dialog.SetHideOnClose(true);
        _dialog.AddResponse("cancel", _controller.Localizer["Cancel"]);
        _dialog.SetCloseResponse("cancel");
        _dialog.AddResponse("ok", _controller.Localizer["OK"]);
        _dialog.SetDefaultResponse("ok");
        _dialog.SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
        //Preferences Group
        _grpGroup = Adw.PreferencesGroup.New();
        //Name
        _rowName = Adw.ActionRow.New();
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        _txtName = Gtk.Entry.New();
        _txtName.SetValign(Gtk.Align.Center);
        _txtName.SetPlaceholderText(_controller.Localizer["Name", "Placeholder"]);
        _txtName.SetActivatesDefault(true);
        _rowName.AddSuffix(_txtName);
        _grpGroup.Add(_rowName);
        //Description
        _rowDescription = Adw.ActionRow.New();
        _rowDescription.SetTitle(_controller.Localizer["Description", "Field"]);
        _txtDescription = Gtk.Entry.New();
        _txtDescription.SetValign(Gtk.Align.Center);
        _txtDescription.SetPlaceholderText(_controller.Localizer["Description", "Placeholder"]);
        _txtDescription.SetActivatesDefault(true);
        _rowDescription.AddSuffix(_txtDescription);
        _grpGroup.Add(_rowDescription);
        //Layout
        _dialog.SetExtraChild(_grpGroup);
        //Load Group
        _txtName.SetText(_controller.Group.Name);
        _txtDescription.SetText(_controller.Group.Description);
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public bool Run()
    {
        _dialog.Show();
        _dialog.SetModal(true);
        _rowName.GrabFocus();
        while(_dialog.IsVisible())
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        if(_controller.Accepted)
        {
            _dialog.SetModal(false);
            var status = _controller.UpdateGroup(_txtName.GetText(), _txtDescription.GetText());
            if(status != GroupCheckStatus.Valid)
            {
                _rowName.RemoveCssClass("error");
                _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
                //Mark Error
                if (status == GroupCheckStatus.EmptyName)
                {
                    _rowName.AddCssClass("error");
                    _rowName.SetTitle(_controller.Localizer["Name", "Empty"]);
                }
                else if(status == GroupCheckStatus.NameExists)
                {
                    _rowName.AddCssClass("error");
                    _rowName.SetTitle(_controller.Localizer["Name", "Exists"]);
                }
                return Run();
            }
        }
        _dialog.Destroy();
        return _controller.Accepted;
    }
}