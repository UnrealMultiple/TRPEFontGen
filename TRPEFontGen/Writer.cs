using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Text;

namespace TRPEFontGen;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public static class Writer
{
    private const string SavePath = "./font";

    public static void Write(string name, FontFile fontFile)
    {
        var fontPath = Path.Combine(SavePath, name);

        if (Directory.Exists(fontPath))
        {
            Directory.Delete(fontPath, true);
        }
        Directory.CreateDirectory(fontPath);
        
        
        using var fs = new FileStream(Path.Combine(fontPath, $"{name}.txt"), FileMode.Create);
        using var bw = new BinaryWriter(fs, Encoding.Unicode);
        
        bw.Write(fontFile.TotalPages);
        bw.Write(fontFile.CharCount);

        foreach (var c in fontFile.Chars)
        {
            bw.Write(c.Glyph);
            bw.Write(c.Cropping);
            bw.Write(c.Char);
            bw.Write(c.Kerning);
            bw.Write(c.Page);
        }

        bw.Write(fontFile.LineSpacing);
        bw.Write(fontFile.Spacing);
        bw.Write(fontFile.HasDefaultChar);
        if (fontFile.HasDefaultChar)
            bw.Write(fontFile.DefaultChar);

        if (fontFile.Textures.Count == 0)
            return;

        for (var i = 0; i < fontFile.Textures.Count; i++)
            fontFile.Textures[i].Save(Path.Combine(fontPath, $"{name}_{i + 1}_A.png"), ImageFormat.Png);
    }
}