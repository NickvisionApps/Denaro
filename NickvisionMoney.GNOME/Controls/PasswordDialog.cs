using NickvisionMoney.Shared.Helpers;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// Responses for the PasswordDialog
/// </summary>
public enum PasswordDialogResponse
{
    Suggested,
    Cancel
}

/// <summary>
/// A dialog for receiving a password
/// </summary>
public partial class PasswordDialog
{
    private readonly Adw.MessageDialog _dialog;
    private readonly Adw.StatusPage _statusPassword;
    private readonly Adw.PasswordEntryRow _passwordEntry;
    private PasswordDialogResponse _response;

    /// <summary>
    /// The password of the dialog
    /// </summary>
    public string Password => _passwordEntry.GetText();

    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="accountTitle">The title of the account requiring the password</param>
    /// <param name="localizer">The localizer for the app</param>
    public PasswordDialog(Gtk.Window parentWindow, string accountTitle, Localizer localizer)
    {
        //Dialog Settings
        _dialog = Adw.MessageDialog.New(parentWindow, "", "");
        _dialog.SetDefaultSize(500, -1);
        _dialog.SetHideOnClose(true);
        _response = PasswordDialogResponse.Cancel;
        _dialog.AddResponse("cancel", localizer["Cancel"]);
        _dialog.AddResponse("suggested", localizer["OK"]);
        _dialog.SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        _dialog.SetCloseResponse("cancel");
        _dialog.SetDefaultResponse("suggested");
        _dialog.OnResponse += (sender, e) => SetResponse(e.Response);
        //Password Page
        _statusPassword = Adw.StatusPage.New();
        _statusPassword.SetTitle(localizer["EnterPassword"]);
        _statusPassword.SetDescription(accountTitle);
        _statusPassword.SetIconName("dialog-password-symbolic");
        _dialog.SetExtraChild(_statusPassword);
        //Password Entry
        _passwordEntry = Adw.PasswordEntryRow.New();
        _passwordEntry.AddCssClass("card");
        _passwordEntry.SetActivatesDefault(true);
        _passwordEntry.SetTitle(localizer["Password", "Field"]);
        _statusPassword.SetChild(_passwordEntry);
    }

    public event GObject.SignalHandler<Adw.MessageDialog, Adw.MessageDialog.ResponseSignalArgs> OnResponse
    {
        add
        {
            _dialog.OnResponse += value;
        }
        remove
        {
            _dialog.OnResponse -= value;
        }
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    public async Task<string?> Run()
    {
        _dialog.Show();
        while (_dialog.GetVisible())
        {
            await Task.Delay(100);
        }
        string? password = null;
        if (_response == PasswordDialogResponse.Suggested)
        {
            password = _passwordEntry.GetText();
        }
        _dialog.Destroy();
        return password;
    }

    /// <summary>
    /// Sets the response of the dialog as a MessageDialogResponse
    /// </summary>
    /// <param name="response">The string response of the dialog</param>
    private void SetResponse(string response)
    {
        _response = response switch
        {
            "suggested" => PasswordDialogResponse.Suggested,
            _ => PasswordDialogResponse.Cancel
        };
    }
}