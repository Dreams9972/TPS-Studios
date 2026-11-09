using System.Reflection;
using System.Windows.Forms;

namespace TPSStudios;

internal class LecteurWmp : AxHost
{
	public object Ocx
	{
		get
		{
			try
			{
				return GetOcx();
			}
			catch
			{
				return null;
			}
		}
	}

	public LecteurWmp()
		: base("6bf52a52-394a-11d3-b153-00c04f79faa6")
	{
	}

	public static void Fixer(object o, string nom, object v)
	{
		o.GetType().InvokeMember(nom, BindingFlags.SetProperty, null, o, new object[1] { v });
	}

	public static object Lire(object o, string nom)
	{
		return o.GetType().InvokeMember(nom, BindingFlags.GetProperty, null, o, null);
	}

	public static void Appeler(object o, string nom)
	{
		o.GetType().InvokeMember(nom, BindingFlags.InvokeMethod, null, o, null);
	}
}
