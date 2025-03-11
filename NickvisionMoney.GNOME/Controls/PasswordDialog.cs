using NickvisionMoney.GNOME.Helpers;
using System.Threading.Tasks;
using Adw.Internal;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A dialog for receiving a password
/// </summary>
public partial class PasswordDialog : Adw.Window
{
    [Gtk.Connect] private readonly Gtk.Label _filenameLabel;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _passwordEntry;
    [Gtk.Connect] private readonly Gtk.Button _unlockButton;

    private PasswordDialog(Gtk.Builder builder, Gtk.Window parent, string accountTitle, TaskCompletionSource<string?> tcs) : base(builder.GetObject("_root").Handle as WindowHandle)
    {
        var unlock = false;
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        _filenameLabel.SetLabel(accountTitle);
        _unlockButton.OnClicked += (sender, e) =>
        {
            unlock = true;
            Close();
        };
        OnCloseRequest += (sender, e) =>
        {
            tcs.SetResult(unlock ? _passwordEntry.GetText() : null);
            return false;
        };
    }

    /// <summary>
    /// Constructs a PasswordDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="accountTitle">The title of the account requiring the password</param>
    /// <param name="tcs">TaskCompletionSource used to pass result to the controller</param>
    public PasswordDialog(Gtk.Window parent, string accountTitle, TaskCompletionSource<string?> tcs) : this(Builder.FromFile("password_dialog.ui"), parent, accountTitle, tcs)
    {
    }
}