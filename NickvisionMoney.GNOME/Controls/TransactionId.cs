using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using System.Globalization;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A widget showing transaction Id (or only color in compact mode)
/// </summary>
public partial class TransactionId : Gtk.Overlay
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

    private readonly Gtk.SizeGroup _sizeGroup;
    private readonly GdkPixbuf.Pixbuf _pixbuf;
    private readonly uint _id;

    [Gtk.Connect] private readonly Gtk.Image _colorImage;
    [Gtk.Connect] private readonly Gtk.Label _idLabel;

    private TransactionId(Gtk.Builder builder, uint id) : base(builder.GetPointer("_root"), false)
    {
        _id = id;
        builder.Connect(this);
        _pixbuf = GdkPixbuf.Pixbuf.New(GdkPixbuf.Colorspace.Rgb, false, 8, 1, 1);
        _sizeGroup = Gtk.SizeGroup.New(Gtk.SizeGroupMode.Horizontal);
        _sizeGroup.AddWidget(this);
        _sizeGroup.AddWidget(_idLabel);
    }

    /// <summary>
    /// Constructs a TransactionId widget
    /// </summary>
    /// <param name="id">Transaction Id</param>
    /// <param name="localizer">The Localizer for the app</param>
    public TransactionId(uint id, Localizer localizer) : this(Builder.FromFile("transaction_id.ui", localizer), id)
    {
    }

    /// <summary>
    /// Updates the color of widget
    /// </summary>
    /// <param name="colorString">Transaction color</param>
    /// <param name="defaultColor">A default color</param>
    public void UpdateColor(string colorString, string defaultColor, bool useNativeDigits)
    {
        var color = new Color();
        if (!gdk_rgba_parse(ref color, colorString))
        {
            gdk_rgba_parse(ref color, defaultColor);
        }
        var red = (int)(color.Red * 255);
        var green = (int)(color.Green * 255);
        var blue = (int)(color.Blue * 255);
        var idString = _id.ToString();
        var nativeDigits = CultureInfo.CurrentCulture.NumberFormat.NativeDigits;
        if(useNativeDigits && "0" != nativeDigits[0])
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
        _idLabel.SetLabel($"<span size=\"10pt\" weight=\"bold\" color=\"#{red.ToString("x2")}{green.ToString("x2")}{blue.ToString("x2")}\">{idString}</span>");
        uint colorPixbuf;
        if (uint.TryParse(red.ToString("X2") + green.ToString("X2") + blue.ToString("X2") + "FF", System.Globalization.NumberStyles.HexNumber, null, out colorPixbuf))
        {
            _pixbuf.Fill(colorPixbuf);
            _colorImage.SetFromPixbuf(_pixbuf);
        }
    }

    /// <summary>
    /// Occurs when the row toggles compact mode
    /// </summary>
    /// <param name="isSmall">Whether the compact mode is required</param>
    public void SetCompact(bool isSmall)
    {
        _idLabel.SetVisible(!isSmall);
        if (isSmall)
        {
            _colorImage.SetSizeRequest(12, 12);
            _colorImage.SetOpacity(1.0);
            _sizeGroup.RemoveWidget(_idLabel);
        }
        else
        {
            _sizeGroup.AddWidget(_idLabel);
            _colorImage.SetSizeRequest(34, 34);
            _colorImage.SetOpacity(0.1);
        }
    }
}