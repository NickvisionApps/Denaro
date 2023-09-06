using NickvisionMoney.GNOME.Helpers;

namespace NickvisionMoney.GNOME.Controls;

public class CurrencyConverterDialog : Adw.Window
{
    /// <summary>
    /// Constructs a CurrencyConverterDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon for the dialog</param>
    private CurrencyConverterDialog(Gtk.Builder builder, Gtk.Window parent, string iconName) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(iconName);
    }
    
    /// <summary>
    /// Constructs a CurrencyConverterDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon for the dialog</param>
    public CurrencyConverterDialog(Gtk.Window parent, string iconName) : this(Builder.FromFile("currency_converter_dialog.ui"), parent, iconName)
    {
    }
}