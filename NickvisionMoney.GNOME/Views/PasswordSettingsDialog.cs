namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// Responses for the PasswordDialog
/// </summary>
public enum PasswordSettingsDialogResponse
{
    Suggested,
    Destructive,
    Cancel
}

/// <summary>
/// A dialog for showing a message
/// </summary>
public partial class PasswordSettingsDialog
{
    private readonly Adw.MessageDialog _dialog;
    private readonly Gtk.Box _boxMain;
    private readonly Gtk.Label _lblWarning;
    private readonly Adw.PreferencesGroup _grpNewPassword;
    private readonly Adw.PasswordEntryRow _passwordNew;
    private readonly Adw.PasswordEntryRow _passwordNewConfirm;

    public PasswordSettingsDialogResponse Response { get; private set; }

    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="cancelText">The text of the cancel button</param>
    /// <param name="destructiveText">The text of the destructive button</param>
    /// <param name="suggestedText">The text of the suggested button</param>
    public PasswordSettingsDialog(Gtk.Window parentWindow)
    {
        _dialog = Adw.MessageDialog.New(parentWindow, "Password Settings", "");
        _dialog.SetDefaultSize(400, -1);
        _dialog.SetHideOnClose(true);
        Response = PasswordSettingsDialogResponse.Cancel;
        _dialog.AddResponse("cancel", "Cancel");
        _dialog.SetDefaultResponse("cancel");
        _dialog.SetCloseResponse("cancel");
        _dialog.AddResponse("destructive", "Remove");
        _dialog.SetResponseAppearance("destructive", Adw.ResponseAppearance.Destructive);
        _dialog.AddResponse("suggested", "OK");
        _dialog.SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) => SetResponse(e.Response);

        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 24);
        _dialog.SetExtraChild(_boxMain);
        _lblWarning = Gtk.Label.New("<b>Warning</b>: if the password is lost, there is no way to restore your account!");
        _lblWarning.AddCssClass("warning");
        _lblWarning.SetUseMarkup(true);
        _lblWarning.SetWrap(true);
        _lblWarning.SetJustify(Gtk.Justification.Center);
        _boxMain.Append(_lblWarning);
        _grpNewPassword = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpNewPassword);
        _passwordNew = Adw.PasswordEntryRow.New();
        _passwordNew.SetTitle("New Password");
        _grpNewPassword.Add(_passwordNew);
        _passwordNewConfirm = Adw.PasswordEntryRow.New();
        _passwordNewConfirm.SetTitle("Confirm New Password");
        _grpNewPassword.Add(_passwordNewConfirm);
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
            "suggested" => PasswordSettingsDialogResponse.Suggested,
            "destructive" => PasswordSettingsDialogResponse.Destructive,
            _ => PasswordSettingsDialogResponse.Cancel
        };
    }
}