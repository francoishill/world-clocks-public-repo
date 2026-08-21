using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WorldClocks;

/// <summary>
/// Procedurally draws the app's clock glyph via GDI+ instead of shipping a binary .ico asset.
/// GDI+ (not a WPF DrawingImage/RenderTargetBitmap) is used deliberately: H.NotifyIcon.Wpf's
/// TaskbarIcon.Icon (a System.Drawing.Icon) is what actually drives the shell tray icon —
/// IconSource is just a convenience wrapper around it that only accepts a handful of concrete
/// ImageSource types (BitmapImage/BitmapFrame backed by a resolvable Uri), and throws for
/// anything else, including a rendered DrawingImage or RenderTargetBitmap.
/// </summary>
public static class IconRenderer
{
    public static Icon CreateIcon(int pixelSize)
    {
        using var bitmap = new Bitmap(pixelSize, pixelSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            Draw(g, pixelSize);
        }

        // Bitmap.GetHicon() only produces a 1-bit transparency mask, which mangles our
        // anti-aliased edges. Encoding as PNG and wrapping in a minimal ICO container instead
        // preserves full 8-bit alpha — Windows has supported PNG-compressed icon frames since Vista.
        return CreateIconFromPng(bitmap);
    }

    /// <summary>Converts the same drawn icon into a WPF BitmapSource, for use as a Window.Icon.</summary>
    public static BitmapSource ToBitmapSource(Icon icon)
    {
        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        bitmapSource.Freeze();
        return bitmapSource;
    }

    private static Icon CreateIconFromPng(Bitmap bitmap)
    {
        using var pngStream = new MemoryStream();
        bitmap.Save(pngStream, ImageFormat.Png);
        var pngBytes = pngStream.ToArray();

        const int iconDirSize = 6;
        const int iconDirEntrySize = 16;

        using var icoStream = new MemoryStream();
        using (var writer = new BinaryWriter(icoStream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            // ICONDIR
            writer.Write((short)0); // reserved
            writer.Write((short)1); // type: icon
            writer.Write((short)1); // image count

            // ICONDIRENTRY (dimensions >= 256 are encoded as 0 per the ICO format spec)
            writer.Write((byte)(bitmap.Width >= 256 ? 0 : bitmap.Width));
            writer.Write((byte)(bitmap.Height >= 256 ? 0 : bitmap.Height));
            writer.Write((byte)0);   // color palette
            writer.Write((byte)0);   // reserved
            writer.Write((short)1);  // color planes
            writer.Write((short)32); // bits per pixel
            writer.Write(pngBytes.Length);
            writer.Write(iconDirSize + iconDirEntrySize); // offset to image data
        }

        icoStream.Write(pngBytes, 0, pngBytes.Length);
        icoStream.Position = 0;
        return new Icon(icoStream);
    }

    private static void Draw(Graphics g, int size)
    {
        var faceMargin = size * 0.03f;
        using (var faceBrush = new SolidBrush(Color.FromArgb(255, 0x2F, 0x6F, 0xED)))
        {
            g.FillEllipse(faceBrush, faceMargin, faceMargin, size - 2 * faceMargin, size - 2 * faceMargin);
        }

        using (var ringPen = new Pen(Color.White, size * 0.06f))
        {
            var inset = ringPen.Width / 2 + faceMargin;
            g.DrawEllipse(ringPen, inset, inset, size - 2 * inset, size - 2 * inset);
        }

        // Hands point to roughly 10-and-2 for a balanced look.
        var center = new PointF(size / 2f, size / 2f);
        using (var handPen = new Pen(Color.White, size * 0.09f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            DrawHand(g, handPen, center, size * 0.26f, -55);
            DrawHand(g, handPen, center, size * 0.34f, 55);
        }

        using (var pinBrush = new SolidBrush(Color.White))
        {
            var pinRadius = size * 0.045f;
            g.FillEllipse(pinBrush, center.X - pinRadius, center.Y - pinRadius, pinRadius * 2, pinRadius * 2);
        }
    }

    private static void DrawHand(Graphics g, Pen pen, PointF center, float length, float angleDegrees)
    {
        var radians = angleDegrees * Math.PI / 180.0;
        var end = new PointF(
            center.X + (float)(length * Math.Sin(radians)),
            center.Y - (float)(length * Math.Cos(radians)));
        g.DrawLine(pen, center, end);
    }
}
