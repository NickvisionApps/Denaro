﻿using NickvisionMoney.GNOME.Helpers;
using System.Threading.Tasks;
using Adw.Internal;

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

    private NewPasswordDialog(Gtk.Builder builder, Gtk.Window parent, string title, TaskCompletionSource<string?> tcs) : base(builder.GetObject("_root").Handle as WindowHandle)
    {
        var setPassword = false;
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
            setPassword = true;
            Close();
        };
        OnCloseRequest += (sender, e) =>
        {
            tcs.SetResult(setPassword ? _newPasswordEntry.GetText() : null);
            return false;
        };
    }

    /// <summary>
    /// Constructs a NewPasswordDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="tcs">TaskCompletionSource used to pass result to the controller</param>
    public NewPasswordDialog(Gtk.Window parent, string title, TaskCompletionSource<string?> tcs) : this(Builder.FromFile("new_password_dialog.ui"), parent, title, tcs)
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