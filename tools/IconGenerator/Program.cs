using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main()
    {
        int size = 256;
        using (var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 背景 - 深色渐变 (深蓝到深紫)
            using (var bgBrush = new LinearGradientBrush(
                new Rectangle(0, 0, size, size),
                Color.FromArgb(255, 15, 23, 42),
                Color.FromArgb(255, 30, 27, 75),
                LinearGradientMode.ForwardDiagonal))
            {
                g.FillRectangle(bgBrush, 0, 0, size, size);
            }

            // 圆角矩形背景卡片
            var cardRect = new Rectangle(12, 12, size - 24, size - 24);
            using (var cardPath = RoundedRect(cardRect, 28))
            using (var cardBrush = new LinearGradientBrush(
                cardRect,
                Color.FromArgb(255, 30, 41, 98),
                Color.FromArgb(255, 49, 46, 129),
                LinearGradientMode.BackwardDiagonal))
            using (var cardPen = new Pen(Color.FromArgb(80, 99, 102, 241), 2))
            {
                g.FillPath(cardBrush, cardPath);
                g.DrawPath(cardPen, cardPath);
            }

            // 内部发光效果
            using (var glowBrush = new SolidBrush(Color.FromArgb(20, 99, 102, 241)))
            {
                g.FillEllipse(glowBrush, 30, 35, size - 60, size - 70);
            }

            // "PCL" 文字 - 大号粗体白色
            using (var pclFont = new Font("Segoe UI Semibold", 72, FontStyle.Bold))
            using (var pclBrush = new SolidBrush(Color.FromArgb(255, 255, 255)))
            using (var pclShadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            {
                var pclSize = g.MeasureString("PCL", pclFont);
                g.DrawString("PCL", pclFont, pclShadowBrush, (size - pclSize.Width) / 2 + 2, 52); // shadow
                g.DrawString("PCL", pclFont, pclBrush, (size - pclSize.Width) / 2, 50);
            }

            // "WM" 标识 - 渐变色
            using (var wmFont = new Font("Segoe UI Semibold", 36, FontStyle.Bold))
            using (var wmBrush = new LinearGradientBrush(
                new RectangleF(40, 115, size - 80, 50),
                Color.FromArgb(255, 167, 139, 250),
                Color.FromArgb(255, 96, 165, 250),
                LinearGradientMode.Horizontal))
            {
                var wmSize = g.MeasureString("WM", wmFont);
                g.DrawString("WM", wmFont, wmBrush, (size - wmSize.Width) / 2, 118);
            }

            // 底部装饰线
            using (var linePen = new Pen(Color.FromArgb(150, 99, 102, 241), 3))
            using (var linePath = new GraphicsPath())
            {
                linePath.AddLine(55, 185, size - 55, 185);
                g.DrawPath(linePen, linePath);
            }

            // Minecraft 风格像素点缀
            DrawPixelBlock(g, 36, 195, 10, Color.FromArgb(255, 74, 222, 128));
            DrawPixelBlock(g, 52, 195, 8, Color.FromArgb(255, 34, 197, 94));
            DrawPixelBlock(g, 66, 195, 6, Color.FromArgb(255, 22, 163, 74));

            DrawPixelBlock(g, size - 56, 195, 6, Color.FromArgb(255, 251, 146, 60));
            DrawPixelBlock(g, size - 44, 195, 8, Color.FromArgb(255, 245, 158, 11));
            DrawPixelBlock(g, size - 30, 195, 10, Color.FromArgb(255, 217, 119, 6));

            // 保存 PNG
            string dir = AppContext.BaseDirectory ?? ".";
            string pngPath = Path.Combine(dir, "icon.png");
            bmp.Save(pngPath, ImageFormat.Png);
            Console.WriteLine($"PNG saved: {pngPath}");

            // 保存多尺寸 PNG 用于后续转换
            int[] sizes = { 256, 128, 64, 48, 32, 16 };
            foreach (int s in sizes)
            {
                using (var resized = new Bitmap(s, s, PixelFormat.Format32bppArgb))
                using (var rg = Graphics.FromImage(resized))
                {
                    rg.SmoothingMode = s > 48 ? SmoothingMode.HighQuality : SmoothingMode.None;
                    rg.InterpolationMode = s > 48 ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                    rg.DrawImage(bmp, 0, 0, s, s);
                    string sp = Path.Combine(dir, $"icon_{s}.png");
                    resized.Save(sp, ImageFormat.Png);
                    Console.WriteLine($"  Saved: {sp}");
                }
            }
        }
    }

    static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
    }

    static void DrawPixelBlock(Graphics g, int x, int y, int s, Color c)
    {
        using (var b = new SolidBrush(c))
            g.FillRectangle(b, x, y, s, s);
        // 高光
        using (var hl = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
        {
            g.FillRectangle(hl, x, y, s, 2);
            g.FillRectangle(hl, x, y, 2, s);
        }
        // 阴影
        using (var sh = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
        {
            g.FillRectangle(sh, x, y + s - 2, s, 2);
            g.FillRectangle(sh, x + s - 2, y, 2, s);
        }
    }
}
