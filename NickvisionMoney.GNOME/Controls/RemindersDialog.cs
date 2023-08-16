
using NickvisionMoney.GNOME.Helpers;
using System.Collections.Generic;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A dialog showing the list of corrupted files
/// </summary>
public partial class RemindersDialog : Adw.Window
{
    [Gtk.Connect] private readonly Gtk.Label _descriptionLabel;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStack;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _scrolledWindow;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _remindersGroup;

    /// <summary>
    /// Constructs a RemindersDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="description">The description of the reminders</param>
    /// <param name="reminders">The list of reminders</param>
    private RemindersDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, string description, List<(string Title, string Subtitle)> reminders) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        _descriptionLabel.SetLabel(description);
        _viewStack.SetVisibleChildName(reminders.Count > 0 ? "reminders" : "no-reminders");
        foreach (var reminder in reminders)
        {
            var row = new Adw.ActionRow();
            row.SetTitle(reminder.Title);
            row.SetSubtitle(reminder.Subtitle);
            _remindersGroup.Add(row);
        }
    }

    /// <summary>
    /// Constructs a RemindersDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="description">The description of the reminders</param>
    /// <param name="reminders">The list of reminders</param>
    public RemindersDialog(Gtk.Window parent, string iconName, string description, List<(string Title, string Subtitle)> reminders) : this(Builder.FromFile("reminders_dialog.ui"), parent, iconName, description, reminders)
    {
    }
}