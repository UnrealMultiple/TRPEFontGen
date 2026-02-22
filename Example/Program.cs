using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using TRPEFontGen;

namespace Example;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
internal static class Program
{
    public static void Main()
    {
        GenFont();
    }
    
    private static void GenFont()
    {
        var chars = new List<char>();

        // 字符集
        AddChar(chars, 0x0020, 0x007E);
        AddChar(chars, 0x4E00, 0x9FA5);
        AddChar(chars, 0xFF01, 0xFF5E);
        
        // 字体，字体大小，字体样式 (需要自己电脑安装)
        var font = new Font("微软雅黑", 15, FontStyle.Regular);
        var fontFile = Generator.Generate(font, chars.ToArray());
        
        // 字体名字
        Writer.Write("Death_Text", fontFile);
    }

    private static void AddChar(List<char> chars, int start, int end)
    {
        for (var i = start; i <= end; i++) chars.Add(Encoding.Unicode.GetChars(BitConverter.GetBytes(i))[0]);
    }
}