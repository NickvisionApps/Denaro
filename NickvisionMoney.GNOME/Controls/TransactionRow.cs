using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a transaction
/// </summary>
public partial class TransactionRow : Adw.PreferencesGroup
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_preferences_row_set_use_markup(nint row, [MarshalAs(UnmanagedType.I1)] bool use_markup);

    private const uint GTK_STYLE_PROVIDER_PRIORITY_USER = 800;

    private readonly Transaction _transaction;
    private bool _isSmall;
    private readonly Adw.ActionRow _row;
    private readonly Gtk.Button _btnId;
    private readonly Gtk.Label _lblAmount;
    private readonly Gtk.Button _btnEdit;
    private readonly Gtk.Button _btnDelete;
    private readonly Gtk.Box _boxButtons;
    private readonly Gtk.Box _boxSuffix;

    /// <summary>
    /// The id of the Transaction
    /// </summary>
    public uint Id { get; init; }

    /// <summary>
    /// Occurs when the edit button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    /// <summary>
    /// Constructs a TransactionRow
    /// </summary>
    /// <param name="transaction">The Transaction to display</param>
    /// <param name="localizer">The Localizer for the app</param>
    public TransactionRow(Transaction transaction, Localizer localizer)
    {
        _transaction = transaction;
        _isSmall = false;
        Id = _transaction.Id;
        //Color
        var color = new Color();
        if(!gdk_rgba_parse(ref color, _transaction.RGBA))
        {
            gdk_rgba_parse(ref color, "#3584e4");
        }
        //Row Settings
        _row = Adw.ActionRow.New();
        adw_preferences_row_set_use_markup(_row.Handle, false);
        _row.SetTitleLines(1);
        _row.SetTitle(_transaction.Description);
        _row.SetSubtitle($"{_transaction.Date.ToString("d")}{(_transaction.RepeatInterval != TransactionRepeatInterval.Never ? $"\nRepeat Interval: {localizer["RepeatInterval", _transaction.RepeatInterval.ToString()]}" : "")}");
        _row.SetSizeRequest(300, 70);
        var rowCssProvider = Gtk.CssProvider.New();
        var rowCss = @"row {
            border-color: " + gdk_rgba_to_string(ref color) + "; }" + char.MinValue;
        gtk_css_provider_load_from_data(rowCssProvider.Handle, rowCss, -1);
        _row.GetStyleContext().AddProvider(rowCssProvider, GTK_STYLE_PROVIDER_PRIORITY_USER);
        //Button ID
        _btnId = Gtk.Button.New();
        _btnId.SetName("btnId");
        _btnId.AddCssClass("circular");
        _btnId.SetValign(Gtk.Align.Center);
        _btnId.SetLabel(_transaction.Id.ToString());
        var btnCssProvider = Gtk.CssProvider.New();
        var btnCss = "#btnId { font-size: 14px; color: " + gdk_rgba_to_string(ref color) + "; }" + char.MinValue;
        gtk_css_provider_load_from_data(btnCssProvider.Handle, btnCss, -1);
        _btnId.GetStyleContext().AddProvider(btnCssProvider, GTK_STYLE_PROVIDER_PRIORITY_USER);
        _row.AddPrefix(_btnId);
        //Amount Label
        _lblAmount = Gtk.Label.New($"{(_transaction.Type == TransactionType.Income ? "+  " : "-  ")}{_transaction.Amount.ToString("C")}");
        _lblAmount.SetHalign(Gtk.Align.End);
        _lblAmount.SetValign(Gtk.Align.Center);
        _lblAmount.SetMarginEnd(4);
        _lblAmount.AddCssClass(_transaction.Type == TransactionType.Income ? "success" : "error");
        _lblAmount.AddCssClass(_transaction.Type == TransactionType.Income ? "money-income" : "money-expense");
        //Edit Button
        _btnEdit = Gtk.Button.NewFromIconName("document-edit-symbolic");
        _btnEdit.SetValign(Gtk.Align.Center);
        _btnEdit.AddCssClass("flat");
        _btnEdit.SetTooltipText(localizer["Edit", "TransactionRow"]);
        _btnEdit.OnClicked += Edit;
        _row.SetActivatableWidget(_btnEdit);
        //DeleteButton
        _btnDelete = Gtk.Button.NewFromIconName("user-trash-symbolic");
        _btnDelete.SetValign(Gtk.Align.Center);
        _btnDelete.AddCssClass("flat");
        _btnDelete.SetTooltipText(localizer["Delete", "TransactionRow"]);
        _btnDelete.OnClicked += Delete;
        //Buttons Box
        _boxButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        if(_transaction.RepeatFrom <= 0)
        {
            _boxButtons.Append(_btnEdit);
            _boxButtons.Append(_btnDelete);
        }
        //Suffix Box
        _boxSuffix = Gtk.Box.New(Gtk.Orientation.Horizontal, 2);
        _boxSuffix.SetValign(Gtk.Align.Center);
        _boxSuffix.Append(_lblAmount);
        _boxSuffix.Append(_boxButtons);
        _row.AddSuffix(_boxSuffix);
        //Group Settings
        Add(_row);
    }

    /// <summary>
    /// Whether or not the row uses a small style
    /// </summary>
    public bool IsSmall
    {
        get => _isSmall;

        set
        {
            _isSmall = value;
            if(_isSmall)
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

    /// <summary>
    /// Occurs when the edit button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Edit(Gtk.Button sender, EventArgs e) => EditTriggered?.Invoke(this, _transaction.Id);

    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Delete(Gtk.Button sender, EventArgs e) => DeleteTriggered?.Invoke(this, _transaction.Id);
}