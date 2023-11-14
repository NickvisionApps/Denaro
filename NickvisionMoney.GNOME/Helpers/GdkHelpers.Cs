using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Helpers;

/// <summary>
/// Helper methods for GDK
/// </summary>
public static partial class GdkHelpers
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)] // Using "gdk" doesn't work here for some reason
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(out RGBA rgba, string spec);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)] // Using "gdk" doesn't work here for some reason
    private static partial string gdk_rgba_to_string(ref RGBA rgba);

    /// <summary>
    /// Helper RGBA struct. Used instead of Gdk.RGBA
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBA
    {
        /// <summary>
        /// Red channel (0.0-1.0)
        /// </summary>
        public float Red;
        /// <summary>
        /// Green channel (0.0-1.0)
        /// </summary>
        public float Green;
        /// <summary>
        /// Blue channel (0.0-1.0)
        /// </summary>
        public float Blue;
        /// <summary>
        /// Alpha channel (0.0-1.0)
        /// </summary>
        public float Alpha;

        /// <summary>
        /// Helper method to parse color string to GdkExt.RGBA struct
        /// </summary>
        /// <param name="colorRGBA">Struct to write to</param>
        /// <param name="spec">Color string</param>
        /// <returns>Whether or not the string was parsed successfully</returns>
        public static bool Parse(out RGBA? colorRGBA, string spec)
        {
            if (gdk_rgba_parse(out var val, spec))
            {
                colorRGBA = val;
                return true;
            }
            colorRGBA = null;
            return false;
        }

        /// <summary>
        /// Gets a string representation of the RGBA
        /// </summary>
        /// <returns>The string representation of the RGBA</returns>
        public override string ToString() => gdk_rgba_to_string(ref this);
    }
}