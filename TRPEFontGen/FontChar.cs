using Microsoft.Xna.Framework;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace TRPEFontGen;

public class FontChar(char c, Rectangle glyph, Rectangle cropping, Vector3 kerning, byte page)
{
    public Rectangle Glyph = glyph;
    public Rectangle Cropping = cropping;
    public char Char = c;
    public Vector3 Kerning = kerning;
    public byte Page = page;
}