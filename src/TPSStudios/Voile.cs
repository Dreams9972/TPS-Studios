using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class Voile : Control
{
	public Action<Graphics> Peindre;

	public Voile()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Peindre != null)
		{
			Peindre(e.Graphics);
		}
	}
}
