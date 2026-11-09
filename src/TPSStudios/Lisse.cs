using System.Collections.Generic;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Lisse
{
	private static List<ILisse> actifs = new List<ILisse>();

	private static Timer tm;

	public static void Demande(ILisse c)
	{
		if (!actifs.Contains(c))
		{
			actifs.Add(c);
		}
		if (tm == null)
		{
			tm = new Timer();
			tm.Interval = 15;
			tm.Tick += delegate
			{
				for (int num = actifs.Count - 1; num >= 0; num--)
				{
					bool flag;
					try
					{
						flag = actifs[num].Pas();
					}
					catch
					{
						flag = false;
					}
					if (!flag)
					{
						actifs.RemoveAt(num);
					}
				}
				if (actifs.Count == 0)
				{
					tm.Stop();
				}
			};
		}
		if (!tm.Enabled)
		{
			tm.Start();
		}
	}
}
