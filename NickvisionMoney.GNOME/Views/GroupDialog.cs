using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

public class GroupDialog
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SignalCallback(nint gObject, [MarshalAs(UnmanagedType.LPStr)] string response, nint data);
    [DllImport("adwaita-1")]
    private static extern ulong g_signal_connect_data(nint instance, [MarshalAs(UnmanagedType.LPStr)] string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)]SignalCallback c_handler, nint data, nint destroy_data, int connect_flags);
    [DllImport("adwaita-1")]
    private static extern bool g_main_context_iteration(nint context, bool may_block);
    [DllImport("adwaita-1")]
    private static extern nint g_main_context_default();

    [DllImport("adwaita-1")]
    private static extern nint adw_entry_row_new();
    [DllImport("adwaita-1")]
    private static extern void adw_message_dialog_add_response(nint dialog, [MarshalAs(UnmanagedType.LPStr)] string id, [MarshalAs(UnmanagedType.LPStr)] string label);
    [DllImport("adwaita-1")]
    private static extern nint adw_message_dialog_new(nint parent, [MarshalAs(UnmanagedType.LPStr)] string heading, [MarshalAs(UnmanagedType.LPStr)] string body);
    [DllImport("adwaita-1")]
    private static extern void adw_message_dialog_set_close_response(nint dialog, [MarshalAs(UnmanagedType.LPStr)] string response);
    [DllImport("adwaita-1")]
    private static extern void adw_message_dialog_set_default_response(nint dialog, [MarshalAs(UnmanagedType.LPStr)] string response);
    [DllImport("adwaita-1")]
    private static extern void adw_message_dialog_set_extra_child(nint dialog, nint child);
    [DllImport("adwaita-1")]
    private static extern void adw_message_dialog_set_response_appearance(nint dialog, [MarshalAs(UnmanagedType.LPStr)] string response, int appearance);
    [DllImport("adwaita-1")]
    private static extern void adw_preferences_group_add(nint group, nint child);
    [DllImport("adwaita-1")]
    private static extern void adw_preferences_row_set_title(nint row, [MarshalAs(UnmanagedType.LPStr)] string title);
    [DllImport("adwaita-1")]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern string gtk_editable_get_text(nint editable);
    [DllImport("adwaita-1")]
    private static extern void gtk_editable_set_text(nint editable, [MarshalAs(UnmanagedType.LPStr)] string text);
    [DllImport("adwaita-1")]
    private static extern void gtk_widget_add_css_class(nint widget, [MarshalAs(UnmanagedType.LPStr)] string cssClass);
    [DllImport("adwaita-1")]
    private static extern void gtk_widget_grab_focus(nint widget);
    [DllImport("adwaita-1")]
    private static extern bool gtk_widget_is_visible(nint widget);
    [DllImport("adwaita-1")]
    private static extern void gtk_widget_remove_css_class(nint widget, [MarshalAs(UnmanagedType.LPStr)] string cssClass);
    [DllImport("adwaita-1")]
    private static extern void gtk_widget_show(nint widget);
    [DllImport("adwaita-1")]
    private static extern void gtk_window_close(nint window);
    [DllImport("adwaita-1")]
    private static extern void gtk_window_set_default_size(nint window, int x, int y);
    [DllImport("adwaita-1")]
    private static extern void gtk_window_set_hide_on_close(nint window, bool setting);
    [DllImport("adwaita-1")]
    private static extern void gtk_window_set_modal(nint window, bool modal);

    private string _response;
    private readonly GroupDialogController _controller;
    private readonly Localizer _localizer;
    private readonly nint _dialog;
    private readonly Adw.PreferencesGroup _grpGroup;
    private readonly nint _rowName;
    private readonly nint _rowDescription;

    public GroupDialog(MainWindow parentWindow, GroupDialogController controller, Localizer localizer)
    {
        _controller = controller;
        _localizer = localizer;
        //Dialog Settings
        _dialog = adw_message_dialog_new(parentWindow.Handle, _localizer["Group"], null);
        gtk_window_set_default_size(_dialog, 450, -1);
        gtk_window_set_hide_on_close(_dialog, true);
        adw_message_dialog_add_response(_dialog, "cancel", _localizer["Cancel"]);
        adw_message_dialog_set_close_response(_dialog, "cancel");
        adw_message_dialog_add_response(_dialog, "ok", _localizer["OK"]);
        adw_message_dialog_set_default_response(_dialog, "ok");
        adw_message_dialog_set_response_appearance(_dialog, "ok", 1); // ADW_RESPONSE_SUGGESTED
        g_signal_connect_data(_dialog, "response", (nint sender, [MarshalAs(UnmanagedType.LPStr)] string response, nint data) => { _response = response; }, IntPtr.Zero, IntPtr.Zero, 0);
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

    public bool Run()
    {
        gtk_widget_show(_dialog);
        gtk_window_set_modal(_dialog, true);
        gtk_widget_grab_focus(_rowName);
        while(gtk_widget_is_visible(_dialog))
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        if(_response == "ok")
        {
            gtk_window_set_modal(_dialog, false);
            var name = gtk_editable_get_text(_rowName);
            var desc = gtk_editable_get_text(_rowDescription);
            var status = _controller.UpdateGroup(name, desc);
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
        }
        return _response == "ok";
    }
}