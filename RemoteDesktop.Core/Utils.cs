using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktop.Core
{
	public static class Utils
	{
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void RtlZeroMemory(IntPtr dst, IntPtr length);
	}
}
