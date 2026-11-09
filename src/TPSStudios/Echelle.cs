using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Echelle
{
	public static void Peindre(Graphics g, Control c, float ech, Action<Graphics> peint, ref Bitmap tampon)
	{
		if (Math.Abs(ech - 1f) < 0.002f)
		{
			peint(g);
			return;
		}
		int num = Math.Max(1, c.Width);
		int num2 = Math.Max(1, c.Height);
		if (tampon == null || tampon.Width != num || tampon.Height != num2)
		{
			if (tampon != null)
			{
				tampon.Dispose();
			}
			tampon = new Bitmap(num, num2, PixelFormat.Format32bppPArgb);
		}
		using (Graphics graphics = Graphics.FromImage(tampon))
		{
			graphics.Clear(Color.Transparent);
			peint(graphics);
		}
		InterpolationMode interpolationMode = g.InterpolationMode;
		PixelOffsetMode pixelOffsetMode = g.PixelOffsetMode;
		g.InterpolationMode = InterpolationMode.HighQualityBilinear;
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;
		float num3 = (float)num * ech;
		float num4 = (float)num2 * ech;
		g.DrawImage(tampon, new RectangleF(((float)num - num3) / 2f, ((float)num2 - num4) / 2f, num3, num4));
		g.InterpolationMode = interpolationMode;
		g.PixelOffsetMode = pixelOffsetMode;
	}
}
