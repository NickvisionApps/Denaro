using NickvisionMoney.GNOME.Helpers;
using System;
using System.Collections.Generic;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A dialog for showing autocomplete suggestions
/// </summary>
public class AutocompleteBox<T> : Gtk.Box
{
    private readonly List<Gtk.Widget> _rows;
    
    [Gtk.Connect] private readonly Adw.PreferencesGroup _group;
    
    public event EventHandler<(string, T)>? SuggestionAccepted;
    
    /// <summary>
    /// Constructs an AutocompleteDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    private AutocompleteBox(Gtk.Builder builder) : base(builder.GetPointer("_root"), false)
    {
        _rows = new List<Gtk.Widget>();
        //Build UI
        builder.Connect(this);
    }
    
    /// <summary>
    /// Constructs an AutocompleteDialog
    /// </summary>
    public AutocompleteBox() : this(Builder.FromFile("autocomplete_box.ui"))
    {
    }
    
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