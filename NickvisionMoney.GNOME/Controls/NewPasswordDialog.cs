using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using System;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A dialog for creating a password
/// </summary>
public partial class NewPasswordDialog : Adw.Window
{
    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _newPasswordEntry;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _confirmPasswordEntry;
    [Gtk.Connect] private readonly Gtk.Button _addButton;

    public string? Password;

    private NewPasswordDialog(Gtk.Builder builder, Gtk.Window parent, string title) : base(builder.GetPointer("_root"), false)
    {
        Password = null;
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        _titleLabel.SetLabel(title);
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
        _addButton.SetSensitive(false);
        _addButton.OnClicked += (sender, e) =>
        {
            SetVisible(false);
            Password = _newPasswordEntry.GetText();
            _newPasswordEntry.SetText("");
            _confirmPasswordEntry.SetText("");
            Close();
        };
    }

    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="title">The title of the dialog</param>
    public NewPasswordDialog(Gtk.Window parent, string title, Localizer localizer) : this(Builder.FromFile("new_password_dialog.ui", localizer), parent, title)
    {
    }

    /// <summary>
    /// Validates the user input
    /// </summary>
    private void Validate()
    {
        if (_newPasswordEntry.GetText() != _confirmPasswordEntry.GetText() || string.IsNullOrEmpty(_newPasswordEntry.GetText()))
        {
            _addButton.SetSensitive(false);
        }
        else
        {
            _addButton.SetSensitive(true);
        }
    }
}