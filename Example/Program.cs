using System.Text;
using SixLabors.Fonts;
using TRPEFontGen;

namespace Example;

internal static class Program
{
    public static void Main()
    {
        GenFont();
    }
    
    private static void GenFont()
    {
        var chars = new HashSet<char>();

        // 字符集
        AddChar(chars, 0x0020, 0x007E);
        AddChar(chars, 0x4E00, 0x9FA5);
        AddChar(chars, 0xFF01, 0xFF5E);
        
        // 字体，字体大小，字体样式 (需要自己电脑安装)
        var font = SystemFonts.CreateFont("Microsoft YaHei", 15, FontStyle.Regular);
        var fontFile = Generator.Generate(font, chars.ToArray(),"Death_Text");

        var savePath = Path.Combine("font", "Death_Text");
        
        fontFile.SaveMetaData(savePath);
        fontFile.SaveTextures(savePath);
    }

    private static void AddChar(HashSet<char> chars, int start, int end)
    {
        for (var i = start; i <= end; i++) chars.Add(Encoding.Unicode.GetChars(BitConverter.GetBytes(i))[0]);
    }
}