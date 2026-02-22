using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TRPEFontGen;

public static class Extensions
{
    extension(BinaryWriter bw)
    {
        public void Write(Rectangle rect)
        {
            bw.Write(rect.X);
            bw.Write(rect.Y);
            bw.Write(rect.Width);
            bw.Write(rect.Height);
        }

        public void Write(Vector3 vec)
        {
            bw.Write(vec.X);
            bw.Write(vec.Y);
            bw.Write(vec.Z);
        }
    }

    public static Rectangle Convert(this RectangleF rect)
    {
        return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }

    public static RectangleF Convert(this Rectangle rect)
    {
        return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }
}