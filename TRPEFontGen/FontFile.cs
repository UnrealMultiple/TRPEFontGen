using System.Collections.Generic;
using System.Drawing;

namespace TRPEFontGen;

public class FontFile(int charCount)
{
    public byte TotalPages { get; set; }
    public int CharCount { get; set; } = charCount;

    public int LineSpacing { get; set; }

    public float Spacing { get; set; }

    public bool HasDefaultChar { get; set; }

    public char DefaultChar { get; set; }

    public List<FontChar> Chars { get; set; } = [];

    public List<Image> Textures { get; set; } = [];
}