using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog : Adw.Window
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_button_set_rgba(nint button, ref Color rgba);

    [DllImport("libadwaita-1.so.0")]
    static extern ref Color gtk_color_dialog_button_get_rgba(nint button);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_button_set_dialog(nint button, nint dialog);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_color_dialog_new();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_set_with_alpha(nint dialog, [MarshalAs(UnmanagedType.I1)] bool with_alpha);

    private bool _constructing;
    private readonly GroupDialogController _controller;
    private readonly nint _colorDialog;

    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Adw.EntryRow _nameRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Gtk.Widget _colorButton;
    [Gtk.Connect] private readonly Gtk.Button _deleteButton;
    [Gtk.Connect] private readonly Gtk.Button _applyButton;

    private readonly Gtk.EventControllerKey _nameKeyController;
    private readonly Gtk.EventControllerKey _descriptionKeyController;
    private readonly Gtk.ShortcutController _shortcutController;

    public event EventHandler? OnApply;
    public event EventHandler? OnDelete;

    private GroupDialog(Gtk.Builder builder, GroupDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _constructing = true;
        _controller = controller;
        //Build UI
        builder.Connect(this);
        var idString = _controller.Group.Id.ToString();
        var nativeDigits = CultureInfo.CurrentCulture.NumberFormat.NativeDigits;
        if(_controller.UseNativeDigits && "0" != nativeDigits[0])
        {
            idString = idString.Replace("0", nativeDigits[0])
                               .Replace("1", nativeDigits[1])
                               .Replace("2", nativeDigits[2])
                               .Replace("3", nativeDigits[3])
                               .Replace("4", nativeDigits[4])
                               .Replace("5", nativeDigits[5])
                               .Replace("6", nativeDigits[6])
                               .Replace("7", nativeDigits[7])
                               .Replace("8", nativeDigits[8])
                               .Replace("9", nativeDigits[9]);
        }
        _titleLabel.SetLabel($"{_("Group")} — {idString}");
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        //Name
        _nameRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _nameKeyController = Gtk.EventControllerKey.New();
        _nameKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _nameKeyController.OnKeyPressed += (sender, e) => { if (e.Keyval == 59) { return true; } return false; };
        _nameRow.AddController(_nameKeyController);
        //Description
        _descriptionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _descriptionKeyController = Gtk.EventControllerKey.New();
        _descriptionKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _descriptionKeyController.OnKeyPressed += (sender, e) => { if (e.Keyval == 59) { return true; } return false; };
        _descriptionRow.AddController(_descriptionKeyController);
        //Color
        _colorDialog = gtk_color_dialog_new();
        gtk_color_dialog_set_with_alpha(_colorDialog, false);
        gtk_color_dialog_button_set_dialog(_colorButton.Handle, _colorDialog);
        _colorButton.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "rgba")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        //Buttons
        _applyButton.SetLabel(_controller.IsEditing ? _("Apply") : _("Add"));
        _applyButton.OnClicked += (sender, e) => OnApply?.Invoke(this, EventArgs.Empty);
        _deleteButton.SetVisible(_controller.IsEditing);
        _deleteButton.OnClicked += (sender, e) => OnDelete?.Invoke(this, EventArgs.Empty);
        //Shortcut Controller
        _shortcutController = Gtk.ShortcutController.New();
        _shortcutController.SetScope(Gtk.ShortcutScope.Managed);
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("Escape"), Gtk.CallbackAction.New((sender, e) =>
        {
            Close();
            return true;
        })));
        AddController(_shortcutController);
        //Load Group
        _nameRow.SetText(_controller.Group.Name);
        _descriptionRow.SetText(_controller.Group.Description);
        var color = new Color();
        gdk_rgba_parse(ref color, _controller.Group.RGBA);
        gtk_color_dialog_button_set_rgba(_colorButton.Handle, ref color);
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public GroupDialog(GroupDialogController controller, Gtk.Window parent) : this(Builder.FromFile("group_dialog.ui"), controller, parent)
    {
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var color = gtk_color_dialog_button_get_rgba(_colorButton.Handle);
        var checkStatus = _controller.UpdateGroup(_nameRow.GetText(), _descriptionRow.GetText(), gdk_rgba_to_string(ref color));
        _nameRow.RemoveCssClass("error");
        _nameRow.SetTitle(_("Name"));
        if (checkStatus == GroupCheckStatus.Valid)
        {
            _applyButton.SetSensitive(true);
        }
        else
        {
            if (checkStatus == GroupCheckStatus.EmptyName)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_("Name (Empty)"));
            }
            else if (checkStatus == GroupCheckStatus.NameExists)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_("Name (Exists)"));
            }
            _applyButton.SetSensitive(false);
        }
    }
}