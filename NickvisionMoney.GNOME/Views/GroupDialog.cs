using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SignalCallback(nint gObject, [MarshalAs(UnmanagedType.LPStr)] string response, nint data);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)]SignalCallback c_handler, nint data, nint destroy_data, int connect_flags);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool may_block);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint adw_entry_row_new();

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
    private static partial void adw_preferences_group_add(nint group, nint child);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_preferences_row_set_title(nint row, string title);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gtk_editable_get_text(nint editable);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_editable_set_text(nint editable, string text);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_add_css_class(nint widget, string cssClass);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_grab_focus(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gtk_widget_is_visible(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_remove_css_class(nint widget, string cssClass);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_show(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_close(nint window);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_default_size(nint window, int x, int y);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_hide_on_close(nint window, [MarshalAs(UnmanagedType.I1)] bool setting);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_modal(nint window, [MarshalAs(UnmanagedType.I1)] bool modal);

    private readonly GroupDialogController _controller;
    private readonly Localizer _localizer;
    private readonly nint _dialog;
    private readonly Adw.PreferencesGroup _grpGroup;
    private readonly nint _rowName;
    private readonly nint _rowDescription;

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">MainWindow</param>
    /// <param name="localizer">Localizer</param>
    public GroupDialog(GroupDialogController controller, MainWindow parentWindow, Localizer localizer)
    {
        _controller = controller;
        _localizer = localizer;
        //Dialog Settings
        _dialog = adw_message_dialog_new(parentWindow.Handle, _localizer["Group"], "");
        gtk_window_set_default_size(_dialog, 450, -1);
        gtk_window_set_hide_on_close(_dialog, true);
        adw_message_dialog_add_response(_dialog, "cancel", _localizer["Cancel"]);
        adw_message_dialog_set_close_response(_dialog, "cancel");
        adw_message_dialog_add_response(_dialog, "ok", _localizer["OK"]);
        adw_message_dialog_set_default_response(_dialog, "ok");
        adw_message_dialog_set_response_appearance(_dialog, "ok", 1); // ADW_RESPONSE_SUGGESTED
        g_signal_connect_data(_dialog, "response", (nint sender, [MarshalAs(UnmanagedType.LPStr)] string response, nint data) => _controller.Accepted = response == "ok", IntPtr.Zero, IntPtr.Zero, 0);
        //Preferences Group
        _grpGroup = Adw.PreferencesGroup.New();
        //Name
        _rowName = adw_entry_row_new();
        adw_preferences_row_set_title(_rowName, _localizer["Name", "Field"]);
        adw_preferences_group_add(_grpGroup.Handle, _rowName);
        //Description
        _rowDescription = adw_entry_row_new();
        adw_preferences_row_set_title(_rowDescription, _localizer["Description", "Field"]);
        adw_preferences_group_add(_grpGroup.Handle, _rowDescription);
        //Layout
        adw_message_dialog_set_extra_child(_dialog, _grpGroup.Handle);
        //Load Group
        gtk_editable_set_text(_rowName, _controller.Group.Name);
        gtk_editable_set_text(_rowDescription, _controller.Group.Description);
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public bool Run()
    {
        gtk_widget_show(_dialog);
        gtk_window_set_modal(_dialog, true);
        gtk_widget_grab_focus(_rowName);
        while(gtk_widget_is_visible(_dialog))
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        if(_controller.Accepted)
        {
            gtk_window_set_modal(_dialog, false);
            var status = _controller.UpdateGroup(gtk_editable_get_text(_rowName), gtk_editable_get_text(_rowDescription));
            if(status != GroupCheckStatus.Valid)
            {
                gtk_widget_remove_css_class(_rowName, "error");
                adw_preferences_row_set_title(_rowName, _localizer["Name", "Field"]);
                gtk_widget_remove_css_class(_rowDescription, "error");
                adw_preferences_row_set_title(_rowDescription, _localizer["Description", "Field"]);
                //Mark Error
                if(status == GroupCheckStatus.EmptyName)
                {
                    gtk_widget_add_css_class(_rowName, "error");
                    adw_preferences_row_set_title(_rowName, _localizer["Name", "Empty"]);
                }
                else if(status == GroupCheckStatus.EmptyDescription)
                {
                    gtk_widget_add_css_class(_rowDescription, "error");
                    adw_preferences_row_set_title(_rowDescription, _localizer["Description", "Empty"]);
                }
                else if(status == GroupCheckStatus.NameExists)
                {
                    gtk_widget_add_css_class(_rowName, "error");
                    adw_preferences_row_set_title(_rowName, _localizer["Name", "Exists"]);
                }
                return Run();
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}