using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

public class TransactionRow : Adw.PreferencesGroup
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [DllImport("adwaita-1", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gdk_rgba_parse(ref Color rgba, [MarshalAs(UnmanagedType.LPStr)] string spec);
    [DllImport("adwaita-1")]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern string gdk_rgba_to_string(ref Color rgba);
    [DllImport("adwaita-1")]
    private static extern void gtk_css_provider_load_from_data(nint provider, [MarshalAs(UnmanagedType.LPStr)] string data, int length);

    public readonly uint Id;

    private readonly Adw.ActionRow _row;
    private readonly Gtk.Button _btnId;
    private readonly Gtk.Label _lblAmount;
    private readonly Gtk.Button _btnEdit;
    private readonly Gtk.Button _btnDelete;
    private readonly Gtk.Box _box;

    public TransactionRow(Transaction transaction, Localizer localizer)
    {
        Id = transaction.Id;
        //Row Settings
        _row = Adw.ActionRow.New();
        _row.SetTitleLines(1);
        _row.SetTitle(transaction.Description);
        _row.SetSubtitle(transaction.Date.ToString("C") + (transaction.RepeatInterval != TransactionRepeatInterval.Never ? $"\nRepeat Interval: {Convert.ToString((int)transaction.RepeatInterval)}" : "")); // TODO
        _row.SetSizeRequest(300, 68);
        //Button ID
        _btnId = Gtk.Button.New();
        _btnId.SetName("btnId");
        _btnId.AddCssClass("circular");
        _btnId.SetValign(Gtk.Align.Center);
        _btnId.SetLabel(transaction.Id.ToString());
        var color = new Color();
        if(!gdk_rgba_parse(ref color, transaction.RGBA))
        {
            gdk_rgba_parse(ref color, "#3584e4");
        }
        var cssProvider = Gtk.CssProvider.New();
        var css = "#btnId { font-size: 14px; color: " + gdk_rgba_to_string(ref color) + "; }" + char.MinValue;
        gtk_css_provider_load_from_data(cssProvider.Handle, css, -1);
        _btnId.GetStyleContext().AddProvider(cssProvider, 800); // GTK_STYLE_PROVIDER_PRIORITY_USER
        _row.AddPrefix(_btnId);
        //Amount Label
        _lblAmount = Gtk.Label.New((transaction.Type == TransactionType.Income ? "+ " : "- ") + transaction.Amount.ToString("C"));
        _lblAmount.SetValign(Gtk.Align.Center);
        _lblAmount.AddCssClass(transaction.Type == TransactionType.Income ? "success" : "error");
        _lblAmount.AddCssClass(transaction.Type == TransactionType.Income ? "money-income" : "money-expense");
        //Edit Button
        _btnEdit = Gtk.Button.NewFromIconName("document-edit-symbolic");
        _btnEdit.SetValign(Gtk.Align.Center);
        _btnEdit.AddCssClass("flat");
        _btnEdit.SetTooltipText(localizer["Edit", "Transaction"]);
        //_btnEdit.OnClicked +=
        _row.SetActivatableWidget(_btnEdit);
        //DeleteButton
        _btnDelete = Gtk.Button.NewFromIconName("user-trash-symbolic");
        _btnDelete.SetValign(Gtk.Align.Center);
        _btnDelete.AddCssClass("flat");
        _btnDelete.SetTooltipText(localizer["Delete", "Transaction"]);
        //_btnDelete.OnClicked +=
        //Box
        _box = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _box.Append(_lblAmount);
        _box.Append(_btnEdit);
        _box.Append(_btnDelete);
        _row.AddSuffix(_box);
        //Group Settings
        Add(_row);
    }
}