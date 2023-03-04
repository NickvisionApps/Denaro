using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using System.Runtime.InteropServices;
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
public partial class PasswordDialog : Adw.MessageDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    [Gtk.Connect] private readonly Adw.StatusPage _statusPage;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _passwordEntry;
    private PasswordDialogResponse _response;

    /// <summary>
    /// The password of the dialog
    /// </summary>
    public string Password => _passwordEntry.GetText();

    private PasswordDialog(Gtk.Builder builder, Gtk.Window parent, string accountTitle, Localizer localizer) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        _response = PasswordDialogResponse.Cancel;
        AddResponse("cancel", localizer["Cancel"]);
        AddResponse("suggested", localizer["Unlock"]);
        SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        SetCloseResponse("cancel");
        SetDefaultResponse("suggested");
        OnResponse += (sender, e) => SetResponse(e.Response);
        _statusPage.SetDescription(accountTitle);
    }


    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="accountTitle">The title of the account requiring the password</param>
    /// <param name="localizer">The localizer for the app</param>
    public PasswordDialog(Gtk.Window parent, string accountTitle, Localizer localizer) : this(Builder.FromFile("password_dialog.ui", localizer), parent, accountTitle, localizer)
    {
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    public async Task<string?> RunAsync()
    {
        Show();
        while (GetVisible())
        {
            g_main_context_iteration(g_main_context_default(), false);
            await Task.Delay(50);
        }
        string? password = null;
        if (_response == PasswordDialogResponse.Suggested)
        {
            password = _passwordEntry.GetText();
        }
        Destroy();
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