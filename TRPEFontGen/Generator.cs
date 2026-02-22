using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TRPEFontGen;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public static class Generator
{
    public static FontFile Generate(Font font, char[] chars, int minusKerning = 5,
        int compressSize = 2, int lineSpacing = 24, float spacing = 0, char defaultChar = '*')
    {
        const int size = 1024;
        var fontFile = new FontFile(chars.Length)
        {
            LineSpacing = lineSpacing,
            Spacing = spacing,
            HasDefaultChar = true,
            DefaultChar = defaultChar
        };

        var png = new Bitmap(size, size);
        var g = CreateGraphics(png);

        float xNow = 1;
        float yNow = 1;
        float yMax = 0;
        byte pages = 0;

        foreach (var c in chars)
        {
            var charSize = g.MeasureString(c.ToString(), font);
            var drawWidth = charSize.Width - compressSize;
            var drawHeight = charSize.Height - compressSize;

            if (xNow + drawWidth + 10 > size)
            {
                yNow += yMax;
                xNow = 1;
                yMax = 0;
            }

            if (yNow + drawHeight > size)
            {
                fontFile.Textures.Add(png);

                g.Dispose();
                png = new Bitmap(size, size);
                g = CreateGraphics(png);

                xNow = 1;
                yNow = 1;
                yMax = 0;
                pages++;
            }

            if (drawHeight > yMax)
                yMax = drawHeight;

            g.DrawString(c.ToString(), font, Brushes.White,
                new RectangleF(xNow, yNow, drawWidth, drawHeight));

            var fontChar = new FontChar
            {
                Char = c,
                Cropping = new Rectangle(0, 0, (int)drawWidth, (int)drawHeight + 2),
                Glyph = new Rectangle((int)xNow, (int)yNow, (int)drawWidth, (int)drawHeight),
                Kerning = new Vector3(0, drawWidth - minusKerning, 0),
                Page = pages
            };
            fontFile.Chars.Add(fontChar);
            xNow += drawWidth;
        }

        g.Dispose();
        fontFile.Textures.Add(png);
        fontFile.TotalPages = (byte)fontFile.Textures.Count;

        return fontFile;
    }

    private static Graphics CreateGraphics(Bitmap bmp)
    {
        var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        return g;
    }
}