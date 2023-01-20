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
/// A dialog for showing a message
/// </summary>
public partial class PasswordDialog
{
    private readonly Adw.MessageDialog _dialog;
    private readonly Adw.StatusPage _statusPassword;
    private readonly Adw.PasswordEntryRow _passwordEntry;

    public PasswordDialogResponse Response { get; private set; }

    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="cancelText">The text of the cancel button</param>
    /// <param name="destructiveText">The text of the destructive button</param>
    /// <param name="suggestedText">The text of the suggested button</param>
    public PasswordDialog(Gtk.Window parentWindow)
    {
        _dialog = Adw.MessageDialog.New(parentWindow, "", "");
        _dialog.SetDefaultSize(500, -1);
        _dialog.SetHideOnClose(true);
        Response = PasswordDialogResponse.Cancel;
        _dialog.AddResponse("cancel", "Cancel");
        _dialog.SetDefaultResponse("cancel");
        _dialog.SetCloseResponse("cancel");
        _dialog.AddResponse("suggested", "OK");
        _dialog.SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) => SetResponse(e.Response);

        _statusPassword = Adw.StatusPage.New();
        _statusPassword.SetTitle("Enter password for the account");
        _statusPassword.SetDescription("ACCOUNT_NAME");
        _statusPassword.SetIconName("dialog-password-symbolic");
        _dialog.SetExtraChild(_statusPassword);

        _passwordEntry = Adw.PasswordEntryRow.New();
        _passwordEntry.AddCssClass("card");
        _passwordEntry.SetTitle("Password");
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
    /// Shows the dialog
    /// </summary>
    public void Show() => _dialog.Show();

    /// <summary>
    /// Destroys the dialog
    /// </summary>
    public void Destroy() => _dialog.Destroy();

    /// <summary>
    /// Sets the response of the dialog as a MessageDialogResponse
    /// </summary>
    /// <param name="response">The string response of the dialog</param>
    private void SetResponse(string response)
    {
        Response = response switch
        {
            "suggested" => PasswordDialogResponse.Suggested,
            _ => PasswordDialogResponse.Cancel
        };
    }
}