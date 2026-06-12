using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: PngToIco.exe <input.png> <output.ico>");
            return;
        }

        string inputPng = args[0];
        string outputIco = args[1];

        using (var src = new Bitmap(inputPng))
        {
            int[] sizes = { 256, 128, 64, 48, 32, 16 };
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // ICO header
                bw.Write((short)0);          // reserved
                bw.Write((short)1);          // type: icon
                bw.Write((short)sizes.Length); // count

                var imageDatas = new byte[sizes.Length][];
                long dataOffset = 6 + 16 * sizes.Length;

                for (int i = 0; i < sizes.Length; i++)
                {
                    int s = sizes[i];
                    Bitmap resized;
                    using (var _tmp = new Bitmap(s, s, PixelFormat.Format32bppArgb))
                    using (var g = Graphics.FromImage(_tmp))
                    {
                        g.Clear(Color.Transparent);
                        g.InterpolationMode = s > 48 ? System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic : System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.DrawImage(src, 0, 0, s, s);
                        resized = (Bitmap)_tmp.Clone();
                    }

                    var pngData = BitmapToPngBytes(resized);
                    resized.Dispose();
                    imageDatas[i] = pngData;

                    // DIRENTRY
                    bw.Write((byte)(s >= 256 ? 0 : s));   // width
                    bw.Write((byte)(s >= 256 ? 0 : s));   // height
                    bw.Write((byte)0);                       // color palette
                    bw.Write((byte)0);                       // reserved
                    bw.Write((short)1);                      // color planes
                    bw.Write((short)32);                     // bits per pixel
                    bw.Write(pngData.Length);                 // size of data
                    bw.Write((int)dataOffset);               // offset

                    dataOffset += pngData.Length;
                }

                // Write image data
                foreach (var data in imageDatas)
                    bw.Write(data);

                File.WriteAllBytes(outputIco, ms.ToArray());
                Console.WriteLine($"ICO saved: {outputIco} ({new FileInfo(outputIco).Length} bytes)");
            }
        }
    }

    static byte[] BitmapToPngBytes(Bitmap bmp)
    {
        using (var ms = new MemoryStream())
        {
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
