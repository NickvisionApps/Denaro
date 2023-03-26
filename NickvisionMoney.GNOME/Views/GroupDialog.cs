using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controllers;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Group
/// </summary>
public partial class GroupDialog : Adw.MessageDialog
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
    private static partial void gtk_color_chooser_get_rgba(nint chooser, ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_chooser_set_rgba(nint chooser, ref Color rgba);

    private bool _constructing;
    private readonly GroupDialogController _controller;

    [Gtk.Connect] private readonly Adw.EntryRow _nameRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Gtk.ColorButton _colorButton;

    private readonly Gtk.EventControllerKey _nameKeyController;
    private readonly Gtk.EventControllerKey _descriptionKeyController;

    private GroupDialog(Gtk.Builder builder, GroupDialogController controller, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        _constructing = true;
        _controller = controller;
        //Build UI
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        AddResponse("cancel", _controller.Localizer["Cancel"]);
        SetCloseResponse("cancel");
        AddResponse("ok", _controller.Localizer[_controller.IsEditing ? "Apply" : "Add"]);
        SetDefaultResponse("ok");
        SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
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
        _colorButton.OnColorSet += (sender, e) => Validate();
        //Load Group
        _nameRow.SetText(_controller.Group.Name);
        _descriptionRow.SetText(_controller.Group.Description);
        var groupColor = new Color();
        gdk_rgba_parse(ref groupColor, _controller.Group.RGBA);
        gtk_color_chooser_set_rgba(_colorButton.Handle, ref groupColor);
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Constructs a GroupDialog
    /// </summary>
    /// <param name="controller">GroupDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public GroupDialog(GroupDialogController controller, Gtk.Window parent) : this(Builder.FromFile("group_dialog.ui", controller.Localizer), controller, parent)
    {
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var color = new Color();
        gtk_color_chooser_get_rgba(_colorButton.Handle, ref color);
        var checkStatus = _controller.UpdateGroup(_nameRow.GetText(), _descriptionRow.GetText(), gdk_rgba_to_string(ref color));
        _nameRow.RemoveCssClass("error");
        _nameRow.SetTitle(_controller.Localizer["Name", "Field"]);
        if (checkStatus == GroupCheckStatus.Valid)
        {
            SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus == GroupCheckStatus.EmptyName)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_controller.Localizer["Name", "Empty"]);
            }
            else if (checkStatus == GroupCheckStatus.NameExists)
            {
                _nameRow.AddCssClass("error");
                _nameRow.SetTitle(_controller.Localizer["Name", "Exists"]);
            }
            SetResponseEnabled("ok", false);
        }
    }
}