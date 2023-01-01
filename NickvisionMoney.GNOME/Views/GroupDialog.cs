using NickvisionMoney.Shared.Controllers;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog
{
    private delegate void ResponseSignal(nint gObject, string response, nint data);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)] ResponseSignal c_handler, nint data, nint destroy_data, int connect_flags);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_add_response(nint dialog, string id, string label);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint adw_message_dialog_new(nint parent, string heading, string body);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_close_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_default_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_extra_child(nint dialog, nint child);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_response_appearance(nint dialog, string response, int appearance);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gtk_widget_is_visible(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_show(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_destroy(nint window);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_default_size(nint window, int x, int y);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_hide_on_close(nint window, [MarshalAs(UnmanagedType.I1)] bool setting);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_modal(nint window, [MarshalAs(UnmanagedType.I1)] bool modal);

    private readonly GroupDialogController _controller;
    private readonly nint _dialog;
    private readonly Adw.PreferencesGroup _grpGroup;
    private readonly Adw.ActionRow _rowName;
    private readonly Gtk.Entry _txtName;
    private readonly Adw.ActionRow _rowDescription;
    private readonly Gtk.Entry _txtDescription;
    private readonly ResponseSignal _responseSignal;

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public GroupDialog(GroupDialogController controller, Gtk.Window parentWindow)
    {
        _controller = controller;
        //Dialog Settings
        _dialog = adw_message_dialog_new(parentWindow.Handle, _controller.Localizer["Group"], "");
        gtk_window_set_default_size(_dialog, 450, -1);
        gtk_window_set_hide_on_close(_dialog, true);
        adw_message_dialog_add_response(_dialog, "cancel", _controller.Localizer["Cancel"]);
        adw_message_dialog_set_close_response(_dialog, "cancel");
        adw_message_dialog_add_response(_dialog, "ok", _controller.Localizer["OK"]);
        adw_message_dialog_set_default_response(_dialog, "ok");
        adw_message_dialog_set_response_appearance(_dialog, "ok", 1); // ADW_RESPONSE_SUGGESTED
        _responseSignal = (nint sender, string response, nint data) => _controller.Accepted = response == "ok";
        g_signal_connect_data(_dialog, "response", _responseSignal, IntPtr.Zero, IntPtr.Zero, 0);
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
        adw_message_dialog_set_extra_child(_dialog, _grpGroup.Handle);
        //Load Group
        _txtName.SetText(_controller.Group.Name);
        _txtDescription.SetText(_controller.Group.Description);
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public async Task<bool> RunAsync()
    {
        gtk_widget_show(_dialog);
        gtk_window_set_modal(_dialog, true);
        _rowName.GrabFocus();
        while(gtk_widget_is_visible(_dialog))
        {
            await Task.Delay(100);
        }
        if(_controller.Accepted)
        {
            gtk_window_set_modal(_dialog, false);
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
                return await RunAsync();
            }
        }
        gtk_window_destroy(_dialog);
        return _controller.Accepted;
    }
}