using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace TRPEFontGen;

public class FontFile(int charCount, string name, int lineSpacing, float spacing, bool hasDefaultChar, char defaultChar)
{
    public string Name = name;
    
    public int CharCount = charCount;

    public int LineSpacing = lineSpacing;

    public float Spacing = spacing;

    public bool HasDefaultChar = hasDefaultChar;

    public char DefaultChar = defaultChar;

    public List<FontChar> Chars = [];

    public List<Image> Textures = [];

    public byte PageCount;

    public void WriteMetaData(Stream stream)
    {
        using var bw = new BinaryWriter(stream, Encoding.Unicode);

        bw.Write(PageCount);
        bw.Write(CharCount);

        foreach (var c in Chars)
        {
            bw.Write(c.Glyph);
            bw.Write(c.Cropping);
            bw.Write(c.Char);
            bw.Write(c.Kerning);
            bw.Write(c.Page);
        }

        bw.Write(LineSpacing);
        bw.Write(Spacing);
        bw.Write(HasDefaultChar);
        if (HasDefaultChar)
            bw.Write(DefaultChar);
    }

    public void SaveTextures(string path)
    {
        Directory.CreateDirectory(path);

        for (var i = 0; i < Textures.Count; i++)
        {
            using var file = File.Open(Path.Combine(path, $"{Name}_{i + 1}_A.png"), FileMode.Create);
            Textures[i].Save(file, new PngEncoder());
        }
    }

    public void SaveMetaData(string path)
    {
        Directory.CreateDirectory(path);
        using var fs = new FileStream(Path.Combine(path, $"{Name}.txt"), FileMode.Create);
        WriteMetaData(fs);
    }
}