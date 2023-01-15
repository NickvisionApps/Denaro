using NickvisionMoney.Shared.Controllers;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    private readonly GroupDialogController _controller;
    private readonly Adw.MessageDialog _dialog;
    private readonly Adw.PreferencesGroup _grpGroup;
    private readonly Adw.EntryRow _rowName;
    private readonly Adw.EntryRow _rowDescription;

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
        _dialog.SetDefaultSize(360, -1);
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
        _rowName = Adw.EntryRow.New();
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        _rowName.SetActivatesDefault(true);
        _grpGroup.Add(_rowName);
        //Description
        _rowDescription = Adw.EntryRow.New();
        _rowDescription.SetTitle(_controller.Localizer["Description", "Field"]);
        _rowDescription.SetActivatesDefault(true);
        _grpGroup.Add(_rowDescription);
        //Layout
        _dialog.SetExtraChild(_grpGroup);
        //Load Group
        _rowName.SetText(_controller.Group.Name);
        _rowDescription.SetText(_controller.Group.Description);
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
            var status = _controller.UpdateGroup(_rowName.GetText(), _rowDescription.GetText());
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