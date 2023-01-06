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
/// A dialog for showing a message
/// </summary>
public partial class MessageDialog
{
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
    public async Task<MessageDialogResponse> RunAsync()
    {
        _dialog.Show();
        while(_dialog.IsVisible())
        {
            await Task.Delay(100);
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