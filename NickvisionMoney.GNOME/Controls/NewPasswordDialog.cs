using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A dialog for creating a password
/// </summary>
public partial class NewPasswordDialog : Adw.MessageDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    [Gtk.Connect] private readonly Adw.PasswordEntryRow _newPasswordEntry;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _confirmPasswordEntry;
    private PasswordDialogResponse _response;

    private NewPasswordDialog(Gtk.Builder builder, Gtk.Window parent, string title, Localizer localizer) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetTitle(title);
        SetTransientFor(parent);
        _response = PasswordDialogResponse.Cancel;
        AddResponse("cancel", localizer["Cancel"]);
        AddResponse("suggested", localizer["Add"]);
        SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        SetCloseResponse("cancel");
        SetDefaultResponse("suggested");
        OnResponse += (sender, e) => SetResponse(e.Response);
        _newPasswordEntry.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        _confirmPasswordEntry.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                Validate();
            }
        };
        SetResponseEnabled("suggested", false);
    }

    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="localizer">The localizer for the app</param>
    public NewPasswordDialog(Gtk.Window parent, string title, Localizer localizer) : this(Builder.FromFile("new_password_dialog.ui", localizer), parent, title, localizer)
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
            password = _newPasswordEntry.GetText();
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

    /// <summary>
    /// Validates the user input
    /// </summary>
    private void Validate()
    {
        if (_newPasswordEntry.GetText() != _confirmPasswordEntry.GetText() || string.IsNullOrEmpty(_newPasswordEntry.GetText()))
        {
            SetResponseEnabled("suggested", false);
        }
        else
        {
            SetResponseEnabled("suggested", true);
        }
    }
}