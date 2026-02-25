using SixLabors.ImageSharp;
using Microsoft.Xna.Framework;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using PointF = SixLabors.ImageSharp.PointF;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TRPEFontGen;

public static class Generator
{
    public static FontFile Generate(Font font, char[] chars, string name, int minusKerning = 5,
        int compressSize = 2, int lineSpacing = 24, float spacing = 0, char defaultChar = '*', 
        float latinMargin = 0.5f)
    {
        const int size = 1024;
        const int atlasPadding = 2;

        var textOption = new TextOptions(font)
        {
            KerningMode = KerningMode.None,
            HintingMode = HintingMode.None
        };

        var fontFile = new FontFile(chars.Length, name, lineSpacing, spacing, true, defaultChar);

        var fontImage = new Image<Rgba32>(size, size);

        float xNow = 1;
        float yNow = 1;
        float yMax = 0;
        byte pages = 0;

        foreach (var c in chars)
        {
            var charSize = TextMeasurer.MeasureSize(c.ToString(), textOption);

            var actualWidth = (float)Math.Ceiling(charSize.Width);
            var actualHeight = (float)Math.Ceiling(charSize.Height);

            var drawWidth = actualWidth - compressSize;
            var drawHeight = actualHeight - compressSize;

            if (xNow + actualWidth + atlasPadding > size)
            {
                yNow += yMax + atlasPadding;
                xNow = 1;
                yMax = 0;
            }

            if (yNow + actualHeight + atlasPadding > size)
            {
                fontFile.Textures.Add(fontImage);

                fontImage = new Image<Rgba32>(size, size);
                xNow = 1;
                yNow = 1;
                yMax = 0;
                pages++;
            }

            if (actualHeight > yMax)
                yMax = actualHeight;

            var textOptions = new RichTextOptions(font)
            {
                Origin = new PointF(xNow, yNow),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            fontImage.Mutate(ctx => ctx.DrawText(
                textOptions,
                c.ToString(),
                Color.White
            ));
            
            var isLatin = c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
            
            var currentLeftMargin = isLatin ? latinMargin : 0f;
            var currentRightMargin = isLatin ? latinMargin : 0f;

            var fontChar = new FontChar
            (
                c,
                new Rectangle(0, 0, (int)drawWidth, (int)drawHeight + 2),
                new Rectangle((int)xNow, (int)yNow, (int)drawWidth, (int)drawHeight),
                new Vector3(currentLeftMargin, drawWidth - minusKerning, currentRightMargin),
                pages
            );
            
            fontFile.Chars.Add(fontChar);
            xNow += actualWidth + atlasPadding;
        }

        fontFile.Textures.Add(fontImage);
        fontFile.PageCount = (byte)fontFile.Textures.Count;

        return fontFile;
    }
}