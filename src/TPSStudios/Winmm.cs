using System.Runtime.InteropServices;

namespace TPSStudios;

internal static class Winmm
{
	[DllImport("winmm.dll")]
	public static extern uint timeBeginPeriod(uint ms);

	[DllImport("winmm.dll")]
	public static extern uint timeEndPeriod(uint ms);
}
