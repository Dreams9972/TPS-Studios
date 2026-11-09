using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenVisionneuse : Form, IPage
{
	private List<Image> imgs;

	private int idx;

	private ZoneDessin zone;

	private Bitmap cache;

	private int cIdx = -1;

	private int cW;

	private int cH;

	private static readonly Font fFl = new Font("Segoe MDL2 Assets", Zx.P(22f));

	private static readonly Font fCap = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fAide = new Font("Segoe UI", Zx.P(9f));

	public FenVisionneuse(List<Image> images, int depart)
	{
		imgs = images;
		idx = Math.Max(0, Math.Min(images.Count - 1, depart));
		base.FormBorderStyle = FormBorderStyle.None;
		base.StartPosition = FormStartPosition.CenterParent;
		base.ShowInTaskbar = false;
		base.KeyPreview = true;
		BackColor = Color.FromArgb(4, 8, 16);
		DoubleBuffered = true;
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
		try
		{
			workingArea = Screen.FromControl((Application.OpenForms.Count > 0) ? Application.OpenForms[0] : this).WorkingArea;
		}
		catch
		{
		}
		base.Size = new Size((int)((double)workingArea.Width * 0.9), (int)((double)workingArea.Height * 0.9));
		zone = new ZoneDessin();
		zone.Dock = DockStyle.Fill;
		zone.Dessiner = Dessiner;
		zone.Cursor = Cursors.Hand;
		base.Controls.Add(zone);
		zone.MouseClick += delegate(object o, MouseEventArgs e)
		{
			if ((e.X > zone.Width - 70 || e.X < 70) && e.Y < 70)
			{
				Close();
			}
			else if (e.X < 110)
			{
				Aller(-1);
			}
			else
			{
				Aller(1);
			}
		};
		base.KeyDown += delegate(object o, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
			else if (e.KeyCode == Keys.Left)
			{
				Aller(-1);
			}
			else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Space)
			{
				Aller(1);
			}
		};
	}

	private void Aller(int d)
	{
		if (imgs.Count >= 2)
		{
			idx = (idx + d + imgs.Count) % imgs.Count;
			zone.Invalidate();
		}
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Image image = imgs[idx];
		int num = 70;
		int num2 = 56;
		Rectangle rectangle = new Rectangle(num, num2, Math.Max(Zx.P(10), w - 2 * num), Math.Max(Zx.P(10), h - 2 * num2 - Zx.P(30)));
		double num3 = Math.Min((double)rectangle.Width / (double)image.Width, (double)rectangle.Height / (double)image.Height);
		int num4 = (int)((double)image.Width * num3);
		int num5 = (int)((double)image.Height * num3);
		if (cache == null || cIdx != idx || cW != num4 || cH != num5)
		{
			if (cache != null)
			{
				cache.Dispose();
			}
			Bitmap image2 = new Bitmap(Math.Max(1, num4), Math.Max(1, num5), PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(image2))
			{
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.DrawImage(image, 0, 0, num4, num5);
			}
			cache = image2;
			cIdx = idx;
			cW = num4;
			cH = num5;
		}
		g.DrawImageUnscaled(cache, rectangle.X + (rectangle.Width - num4) / 2, rectangle.Y + (rectangle.Height - num5) / 2);
		if (imgs.Count > 1)
		{
			Lang.DrawText(g, "\ue76b", fFl, new Rectangle(0, 0, Zx.P(90), h), Color.FromArgb(200, 255, 255, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Lang.DrawText(g, "\ue76c", fFl, new Rectangle(w - Zx.P(90), 0, Zx.P(90), h), Color.FromArgb(200, 255, 255, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		Lang.DrawText(g, "\ue711", fFl, new Rectangle(w - Zx.P(70), Zx.P(10), Zx.P(50), Zx.P(50)), Color.FromArgb(210, 255, 255, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(g, "←", new Font("Segoe UI", Zx.P(18f)), new Rectangle(Zx.P(20), Zx.P(10), Zx.P(50), Zx.P(50)), Color.FromArgb(210, 255, 255, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(g, idx + 1 + " / " + imgs.Count, fCap, new Rectangle(0, h - Zx.P(44), w, Zx.P(22)), C.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(g, "← → pour naviguer  ·  Échap pour fermer", fAide, new Rectangle(0, h - Zx.P(24), w, Zx.P(18)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
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
}
