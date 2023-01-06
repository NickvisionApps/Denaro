using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// Responses for the MessageDialog
/// </summary>
public enum MessageDialogResponse
{
    Suggested,
    Destructive,
    Cancel
}

/// <summary>
/// Available response appearances for the MessageDialog
/// </summary>
file enum ResponseAppearance : uint
{
    Default,
    Suggested,
    Destructive
}

/// <summary>
/// A dialog for showing a message
/// </summary>
public partial class MessageDialog
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

    private readonly nint _dialog;
    private MessageDialogResponse _response;
    private ResponseSignal _responseSignal;

    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="cancelText">The text of the cancel button</param>
    /// <param name="destructiveText">The text of the destructive button</param>
    /// <param name="suggestedText">The text of the suggested button</param>
    public MessageDialog(Gtk.Window parentWindow, string title, string message, string? cancelText, string? destructiveText = null, string? suggestedText = null)
    {
        _dialog = adw_message_dialog_new(parentWindow.Handle, title, message);
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
            adw_message_dialog_set_response_appearance(_dialog, "destructive", (uint)ResponseAppearance.Destructive);
        }
        if(!string.IsNullOrEmpty(suggestedText))
        {
            adw_message_dialog_add_response(_dialog, "suggested", suggestedText);
            adw_message_dialog_set_response_appearance(_dialog, "suggested", (uint)ResponseAppearance.Suggested);
        }
        _responseSignal = (nint sender, string response, nint data) => SetResponse(response);
        g_signal_connect_data(_dialog, "response", _responseSignal, IntPtr.Zero, IntPtr.Zero, 0);
    }

    /// <summary>
    /// Displays the dialog
    /// </summary>
    /// <returns>MessageDialogResponse</returns>
    public async Task<MessageDialogResponse> RunAsync()
    {
        gtk_widget_show(_dialog);
        while(gtk_widget_is_visible(_dialog))
        {
            await Task.Delay(100);
        }
        gtk_window_destroy(_dialog);
        return _response;
    }

    /// <summary>
    /// Resets the destructive response appearance to default
    /// </summary>
    public void UnsetDestructiveApperance() => adw_message_dialog_set_response_appearance(_dialog, "destructive", (uint)ResponseAppearance.Default);

    /// <summary>
    /// Resets the suggested response appearance to default
    /// </summary>
    public void UnsetSuggestedApperance() => adw_message_dialog_set_response_appearance(_dialog, "suggested", (uint)ResponseAppearance.Default);

    /// <summary>
    /// Sets the response of the dialog as a MessageDialogResponse
    /// </summary>
    /// <param name="response">The string response of the dialog</param>
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