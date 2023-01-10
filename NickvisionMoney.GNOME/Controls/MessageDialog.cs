using System.Runtime.InteropServices;

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
/// A dialog for showing a message
/// </summary>
public partial class MessageDialog
{
    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    private readonly Adw.MessageDialog _dialog;
    private MessageDialogResponse _response;

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
        _dialog = Adw.MessageDialog.New(parentWindow, title, message);
        _dialog.SetHideOnClose(true);
        if(!string.IsNullOrEmpty(cancelText))
        {
            _dialog.AddResponse("cancel", cancelText);
            _dialog.SetDefaultResponse("cancel");
            _dialog.SetCloseResponse("cancel");
        }
        if(!string.IsNullOrEmpty(destructiveText))
        {
            _dialog.AddResponse("destructive", destructiveText);
            _dialog.SetResponseAppearance("destructive", Adw.ResponseAppearance.Destructive);
        }
        if(!string.IsNullOrEmpty(suggestedText))
        {
            _dialog.AddResponse("suggested", suggestedText);
            _dialog.SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        }
        _dialog.OnResponse += (sender, e) => SetResponse(e.Response);
    }

    /// <summary>
    /// Displays the dialog
    /// </summary>
    /// <returns>MessageDialogResponse</returns>
    public MessageDialogResponse Run()
    {
        _dialog.Show();
        while(_dialog.IsVisible())
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        _dialog.Destroy();
        return _response;
    }

    /// <summary>
    /// Resets the destructive response appearance to default
    /// </summary>
    public void UnsetDestructiveApperance() => _dialog.SetResponseAppearance("destructive", Adw.ResponseAppearance.Default);

    /// <summary>
    /// Resets the suggested response appearance to default
    /// </summary>
    public void UnsetSuggestedApperance() => _dialog.SetResponseAppearance("suggested", Adw.ResponseAppearance.Default);

    /// <summary>
    /// Sets the response of the dialog as a MessageDialogResponse
    /// </summary>
    /// <param name="response">The string response of the dialog</param>
    private void SetResponse(string response)
    {
        _response = response switch
        {
            "suggested" => MessageDialogResponse.Suggested,
            "destructive" => MessageDialogResponse.Destructive,
            _ => MessageDialogResponse.Cancel
        };
    }
}