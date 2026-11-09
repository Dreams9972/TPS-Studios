using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TPSStudios;

internal class Vignette : Control
{
	public Image Img;

	private bool survol;

	private Bitmap cache;

	private Image src;

	private int cw;

	private int ch;

	private static readonly Font fPh = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fLoupe = new Font("Segoe MDL2 Assets", Zx.P(26f));

	public Vignette()
	{
		Cursor = Cursors.Hand;
		BackColor = C.Carte;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		survol = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void Dispose(bool d)
	{
		if (d && cache != null)
		{
			cache.Dispose();
			cache = null;
		}
		base.Dispose(d);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		Rectangle r = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
		if (r.Width < 8 || r.Height < 8)
		{
			return;
		}
		using GraphicsPath path = Bouton.Rond(r, Zx.P(10));
		GraphicsState gstate = graphics.Save();
		graphics.SetClip(path, CombineMode.Intersect);
		if (Img != null)
		{
			if (cache == null || src != Img || cw != base.Width || ch != base.Height)
			{
				if (cache != null)
				{
					cache.Dispose();
				}
				Bitmap image = new Bitmap(base.Width, base.Height, PixelFormat.Format32bppPArgb);
				using (Graphics graphics2 = Graphics.FromImage(image))
				{
					graphics2.InterpolationMode = InterpolationMode.HighQualityBicubic;
					double num = Math.Max((double)base.Width / (double)Img.Width, (double)base.Height / (double)Img.Height);
					int num2 = (int)((double)Img.Width * num);
					int num3 = (int)((double)Img.Height * num);
					graphics2.DrawImage(Img, (base.Width - num2) / 2, (base.Height - num3) / 2, num2, num3);
				}
				cache = image;
				src = Img;
				cw = base.Width;
				ch = base.Height;
			}
			graphics.DrawImageUnscaled(cache, 0, 0);
		}
		else
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, base.Width, base.Height + 1), C.Ligne, C.Champ, 60f))
			{
				graphics.FillRectangle(brush, 0, 0, base.Width, base.Height);
			}
			Lang.DrawText(graphics, "Chargement…", fPh, new Rectangle(0, 0, base.Width, base.Height), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}
		if (survol && Img != null)
		{
			using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(70, 5, 12, 24)))
			{
				graphics.FillRectangle(brush2, 0, 0, base.Width, base.Height);
			}
			Lang.DrawText(graphics, "\ue71e", fLoupe, new Rectangle(0, 0, base.Width, base.Height), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		graphics.Restore(gstate);
		using Pen pen = new Pen(survol ? C.Jade : C.Ligne, survol ? Zx.P(2f) : 1f);
		graphics.DrawPath(pen, path);
	}
}
