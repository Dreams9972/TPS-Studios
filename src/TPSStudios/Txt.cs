using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Txt
{
	private const TextFormatFlags P = TextFormatFlags.PreserveGraphicsTranslateTransform;

	public static void Dessiner(IDeviceContext dc, string t, Font f, Point p, Color c)
	{
		Lang.DrawText(dc, t, f, p, c, TextFormatFlags.PreserveGraphicsTranslateTransform);
	}

	public static void Dessiner(IDeviceContext dc, string t, Font f, Point p, Color c, TextFormatFlags fl)
	{
		Lang.DrawText(dc, t, f, p, c, fl | TextFormatFlags.PreserveGraphicsTranslateTransform);
	}

	public static void Dessiner(IDeviceContext dc, string t, Font f, Rectangle r, Color c)
	{
		Lang.DrawText(dc, t, f, r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform);
	}

	public static void Dessiner(IDeviceContext dc, string t, Font f, Rectangle r, Color c, TextFormatFlags fl)
	{
		Lang.DrawText(dc, t, f, r, c, fl | TextFormatFlags.PreserveGraphicsTranslateTransform);
	}
}
