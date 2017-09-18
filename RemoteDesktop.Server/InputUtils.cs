using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktop.Server
{
	[StructLayout(LayoutKind.Sequential)]
	struct MOUSEINPUT
	{
		int dx;
		int dy;
		int mouseData;
		public int dwFlags;
		int time;
		IntPtr dwExtraInfo;
	}   

	struct INPUT
	{
		public uint dwType;
		public MOUSEINPUT mi;
	}  

	static class InputUtils
	{
		[DllImport("user32.dll", SetLastError=true)]
		private static extern uint SendInput(uint cInputs, INPUT input, int size);

		public static void DoClickMouse(int mouseButton)
		{
			var input = new INPUT()
			{
				dwType = 0, // Mouse input
				mi = new MOUSEINPUT() { dwFlags = mouseButton }
			};

			if (SendInput(1, input, Marshal.SizeOf(input)) == 0)
			{ 
				throw new Exception("Failed to set input");
			}
		}
	}
}
