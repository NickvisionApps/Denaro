using NickvisionMoney.GNOME.Views;
using NickvisionMoney.Shared.Helpers;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

public partial class MessageDialog
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
    private static partial void adw_message_dialog_add_response(nint dialog, string id, string label);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint adw_message_dialog_new(nint parent, string heading, string body);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_close_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_default_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_response_appearance(nint dialog, string response, uint appearance);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gtk_widget_is_visible(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_show(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_destroy(nint window);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_hide_on_close(nint window, [MarshalAs(UnmanagedType.I1)] bool setting);

    public enum MessageDialogResponse
    {
        Suggested,
        Destructive,
        Cancel
    }

    private enum _responseAppearance
    {
        Default,
        Suggested,
        Destructive
    }

    private readonly nint _dialog;
    private MessageDialogResponse _response;

    public MessageDialog(MainWindow parentWindow, string title, string description, string? cancelText, string? destructiveText, string? suggestedText)
    {
        _dialog = adw_message_dialog_new(parentWindow.Handle, title, description);
        gtk_window_set_hide_on_close(_dialog, true);
        if(!string.IsNullOrEmpty(cancelText))
        {
            adw_message_dialog_add_response(_dialog, "cancel", cancelText);
            adw_message_dialog_set_default_response(_dialog, "cancel");
            adw_message_dialog_set_close_response(_dialog, "cancel");
        }
        if(!string.IsNullOrEmpty(destructiveText))
        {
            adw_message_dialog_add_response(_dialog, "destructive", destructiveText);
            adw_message_dialog_set_response_appearance(_dialog, "destructive", (uint)_responseAppearance.Destructive);
        }
        if(!string.IsNullOrEmpty(suggestedText))
        {
            adw_message_dialog_add_response(_dialog, "suggested", suggestedText);
            adw_message_dialog_set_response_appearance(_dialog, "suggested", (uint)_responseAppearance.Suggested);
        }
        g_signal_connect_data(_dialog, "response", (nint sender, [MarshalAs(UnmanagedType.LPStr)] string response, nint data) => SetResponse(response), IntPtr.Zero, IntPtr.Zero, 0);
    }

    public MessageDialogResponse Run()
    {
        gtk_widget_show(_dialog);
        while(gtk_widget_is_visible(_dialog))
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        gtk_window_destroy(_dialog);
        return _response;
    }

    private void SetResponse(string response)
    {
        switch(response)
        {
            case "suggested":
                _response = MessageDialogResponse.Suggested;
                break;
            case "destructive":
                _response = MessageDialogResponse.Destructive;
                break;
            default:
                _response = MessageDialogResponse.Cancel;
                break;
        }
    }
}