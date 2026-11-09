using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class PanelFond : Panel
{
	public Action<Graphics, int> Peindre;

	public PanelFond()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Peindre != null)
		{
			try
			{
				Peindre(e.Graphics, base.AutoScrollPosition.Y);
				return;
			}
			catch (Exception e2)
			{
				Journal.Une("peinture", e2);
				return;
			}
		}
		e.Graphics.Clear(BackColor);
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (m.Msg == 277 || m.Msg == 522)
		{
			Invalidate();
		}
	}
}
