using NickvisionMoney.GNOME.Helpers;
using System;
using System.Collections.Generic;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A dialog for showing autocomplete suggestions
/// </summary>
public class AutocompleteBox<T> : Gtk.Box
{
    private readonly Adw.EntryRow _parent;
    private readonly List<Gtk.Widget> _rows;
    private readonly Gtk.EventControllerKey _parentKeyController;
    private bool _canHide;
    
    [Gtk.Connect] private readonly Adw.PreferencesGroup _group;
    
    public event EventHandler<(string, T)>? SuggestionAccepted;
    
    /// <summary>
    /// Constructs an AutocompleteDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Adw.EntryRow</param>
    private AutocompleteBox(Gtk.Builder builder, Adw.EntryRow parent) : base(builder.GetPointer("_root"), false)
    {
        _parent = parent;
        _rows = new List<Gtk.Widget>();
        _canHide = true;
        //Build UI
        builder.Connect(this);
        _parentKeyController = Gtk.EventControllerKey.New();
        _parentKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _parentKeyController.OnKeyPressed += (sender, e) =>
        {
            if(e.Keyval == 65293 || e.Keyval == 65421) //enter | keypad enter
            {
                if(GetVisible())
                {
                    AcceptSuggestion(0);
                    return true;
                }
            }
            if(e.Keyval == 65364) //down arrow
            {
                if(GetVisible())
                {
                    _canHide = false;
                    GrabFocus();
                    return true;
                }
            }
            return false;
        };
        _parent.AddController(_parentKeyController);
        _parent.OnStateFlagsChanged += (sender, e) =>
        {
            if(!_canHide)
            {
                _canHide = true;
            }
            else if(e.Flags.HasFlag(Gtk.StateFlags.FocusWithin) && !_parent.GetStateFlags().HasFlag(Gtk.StateFlags.FocusWithin))
            {
                _parent.SetActivatesDefault(true);
                SetVisible(false);
            }
        };
    }
    
    /// <summary>
    /// Constructs an AutocompleteDialog
    /// </summary>
    /// <param name="parent">Adw.EntryRow</param>
    public AutocompleteBox(Adw.EntryRow parent) : this(Builder.FromFile("autocomplete_box.ui"), parent)
    {
    }
    
    /// <summary>
    /// Grabs focus for the box
    /// </summary>
    public new void GrabFocus() => _rows[0].GrabFocus();

    /// <summary>
    /// Updates the list of suggestions
    /// </summary>
    /// <param name="suggestions">A list of suggestions and their subtext and models</param>
    public void UpdateSuggestions(List<(string, string, T)> suggestions)
    {
        foreach(var row in _rows)
        {
            _group.Remove(row);
        }
        _rows.Clear();
        foreach(var suggestion in suggestions)
        {
            var row = Adw.ActionRow.New();
            row.SetTitle(suggestion.Item1);
            row.SetSubtitle(suggestion.Item2);
            row.SetActivatable(true);
            row.OnActivated += (sender, e) =>
            {
                SuggestionAccepted?.Invoke(this, (suggestion.Item1, suggestion.Item3));
                SetVisible(false);
            };
            var keyController = Gtk.EventControllerKey.New();
            keyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
            keyController.OnKeyPressed += (sender, e) =>
            {
                if(e.Keyval == 65293 || e.Keyval == 65421) //enter | keypad enter
                {
                    row.Activate();
                    return true;
                }
                return false;
            };
            row.AddController(keyController);
            _rows.Add(row);
            _group.Add(row);
        }
    }

    /// <summary>
    /// Accepts a suggestion
    /// </summary>
    /// <param name="index">The index of the suggestion to accept</param>
    public void AcceptSuggestion(int index) => _rows[index].Activate();
}