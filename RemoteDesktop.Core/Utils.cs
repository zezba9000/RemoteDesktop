using System;
using System.Runtime.InteropServices;
using System.Security;

namespace RemoteDesktop.Core
{
	public static class Utils
	{
		[SuppressUnmanagedCodeSecurity]
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void RtlZeroMemory(IntPtr dst, IntPtr length);
	}
}
