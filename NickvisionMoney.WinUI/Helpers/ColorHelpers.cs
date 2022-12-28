using CommunityToolkit.WinUI.Helpers;
using Windows.UI;

namespace NickvisionMoney.WinUI.Helpers;

public static class ColorHelpers
{
    public static Color? FromRGBA(string rgba)
    {
        if(string.IsNullOrEmpty(rgba))
        {
            return null;
        }
        try
        {
            return ColorHelper.ToColor(rgba);
        }
        catch
        {
            if (rgba.StartsWith("rgb("))
            {
                rgba = rgba.Remove(0, 4);
                rgba = rgba.Remove(rgba.Length - 1);
                var fields = rgba.Split(',');
                try
                {
                    return Color.FromArgb(255, byte.Parse(fields[0]), byte.Parse(fields[1]), byte.Parse(fields[2]));
                }
                catch
                {
                    return null;
                }
            }
            else if (rgba.StartsWith("rgba("))
            {
                rgba = rgba.Remove(0, 5);
                rgba = rgba.Remove(rgba.Length - 1);
                var fields = rgba.Split(',');
                try
                {
                    return Color.FromArgb(byte.Parse(fields[3]), byte.Parse(fields[0]), byte.Parse(fields[1]), byte.Parse(fields[2]));
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }

    public static string ToRGBA(this Color color)
    {
        if(color.A == 255)
        {
            return $"rgb({color.R},{color.G},{color.B})";
        }
        else
        {
            return $"rgba({color.R},{color.G},{color.B},{color.A})";
        }
    }
}
