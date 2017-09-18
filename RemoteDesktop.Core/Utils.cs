using System;
using System.Runtime.InteropServices;

namespace RemoteDesktop.Core
{
	public static class Utils
	{
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void RtlZeroMemory(IntPtr dst, IntPtr length);
	}
}
