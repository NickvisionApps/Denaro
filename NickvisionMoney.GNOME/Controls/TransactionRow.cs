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
    [DllImport("adwaita-1")]
    private static extern void adw_preferences_row_set_use_markup(nint row, bool use_markup);

    public readonly uint Id;

    private readonly Adw.ActionRow _row;
    private readonly Gtk.Button _btnId;
    private readonly Gtk.Label _lblAmount;
    private readonly Gtk.Button _btnEdit;
    private readonly Gtk.Button _btnDelete;
    private readonly Gtk.Box _boxButtons;
    private readonly Gtk.Box _boxSuffix;

    public TransactionRow(Transaction transaction, Localizer localizer)
    {
        Id = transaction.Id;
        var color = new Color();
        if(!gdk_rgba_parse(ref color, transaction.RGBA))
        {
            gdk_rgba_parse(ref color, "#3584e4");
        }
        //Row Settings
        _row = Adw.ActionRow.New();
        adw_preferences_row_set_use_markup(_row.Handle, false);
        _row.SetTitleLines(1);
        _row.SetTitle(transaction.Description);
        _row.SetSubtitle(transaction.Date.ToString("d") + (transaction.RepeatInterval != TransactionRepeatInterval.Never ? $"\nRepeat Interval: {Convert.ToString((int)transaction.RepeatInterval)}" : "")); // TODO
        _row.SetSizeRequest(300, 70);
        var rowCssProvider = Gtk.CssProvider.New();
        var rowCss = @"row {
            border-color: " + gdk_rgba_to_string(ref color) + "}" + char.MinValue;
        gtk_css_provider_load_from_data(rowCssProvider.Handle, rowCss, -1);
        _row.GetStyleContext().AddProvider(rowCssProvider, 800); // GTK_STYLE_PROVIDER_PRIORITY_USER
        //Button ID
        _btnId = Gtk.Button.New();
        _btnId.SetName("btnId");
        _btnId.AddCssClass("circular");
        _btnId.SetValign(Gtk.Align.Center);
        _btnId.SetLabel(transaction.Id.ToString());
        var btnCssProvider = Gtk.CssProvider.New();
        var btnCss = "#btnId { font-size: 14px; color: " + gdk_rgba_to_string(ref color) + "; }" + char.MinValue;
        gtk_css_provider_load_from_data(btnCssProvider.Handle, btnCss, -1);
        _btnId.GetStyleContext().AddProvider(btnCssProvider, 800); // GTK_STYLE_PROVIDER_PRIORITY_USER
        _row.AddPrefix(_btnId);
        //Amount Label
        _lblAmount = Gtk.Label.New((transaction.Type == TransactionType.Income ? "+ " : "- ") + transaction.Amount.ToString("C"));
        _lblAmount.SetHalign(Gtk.Align.End);
        _lblAmount.SetValign(Gtk.Align.Center);
        _lblAmount.SetMarginEnd(4);
        _lblAmount.AddCssClass(transaction.Type == TransactionType.Income ? "success" : "error");
        _lblAmount.AddCssClass(transaction.Type == TransactionType.Income ? "money-income" : "money-expense");
        //Edit Button
        _btnEdit = Gtk.Button.NewFromIconName("document-edit-symbolic");
        _btnEdit.SetValign(Gtk.Align.Center);
        _btnEdit.AddCssClass("flat");
        _btnEdit.SetTooltipText(localizer["Edit", "TransactionRow"]);
        //_btnEdit.OnClicked +=
        _row.SetActivatableWidget(_btnEdit);
        //DeleteButton
        _btnDelete = Gtk.Button.NewFromIconName("user-trash-symbolic");
        _btnDelete.SetValign(Gtk.Align.Center);
        _btnDelete.AddCssClass("flat");
        _btnDelete.SetTooltipText(localizer["Delete", "TransactionRow"]);
        //_btnDelete.OnClicked +=
        //Buttons Box
        _boxButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _boxButtons.Append(_btnEdit);
        _boxButtons.Append(_btnDelete);
        //Suffix Box
        _boxSuffix = Gtk.Box.New(Gtk.Orientation.Horizontal, 2);
        _boxSuffix.SetValign(Gtk.Align.Center);
        _boxSuffix.Append(_lblAmount);
        _boxSuffix.Append(_boxButtons);
        _row.AddSuffix(_boxSuffix);
        //Group Settings
        Add(_row);
    }

    public void ChangeStyle(bool smallWidth)
    {
        if(smallWidth)
        {
            _row.AddCssClass("row-small");
            _boxSuffix.SetOrientation(Gtk.Orientation.Vertical);
            _boxSuffix.SetMarginTop(4);
            _btnId.SetVisible(false);
        }
        else
        {
            _row.RemoveCssClass("row-small");
            _boxSuffix.SetOrientation(Gtk.Orientation.Horizontal);
            _boxSuffix.SetMarginTop(0);
            _btnId.SetVisible(true);
        }
    }
}