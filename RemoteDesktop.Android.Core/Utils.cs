using System;
using System.Runtime.InteropServices;
using System.Security;

namespace RemoteDesktop.Core
{
	public static class Utils
	{
		// [SuppressUnmanagedCodeSecurity]
		// [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		// public static extern void RtlZeroMemory(IntPtr dst, IntPtr length);
		//
		// [SuppressUnmanagedCodeSecurity]
		// [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		// public static extern IntPtr memset(IntPtr dest, int c, IntPtr count);

        public static void fillValueByteArray(byte[] buf, byte value, int offset)
        {
            int len = buf.Length;
            for(int ii = 0; ii < len; ii++)
            {
                buf[offset + ii] = value;
            }
        }

	}
}
