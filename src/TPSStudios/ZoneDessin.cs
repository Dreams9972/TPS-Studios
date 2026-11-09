using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class ZoneDessin : Control
{
	public Action<Graphics, int, int> Dessiner;

	public ZoneDessin()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear((base.Parent != null) ? base.Parent.BackColor : C.Fond);
		if (Dessiner != null)
		{
			try
			{
				Dessiner(e.Graphics, base.Width, base.Height);
			}
			catch (Exception e2)
			{
				Journal.Une("dessin", e2);
			}
		}
	}
}
