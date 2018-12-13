using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace RemoteDesktop.Core
{
	public static class Utils
	{
        private static Stopwatch sw = null;

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

        public static void startTimeMeasure()
        {
            if(sw == null)
            {
                sw = new Stopwatch();
            }
            else
            {
                sw.Reset();
            }

            sw.Start();
        }

        public static long stopMeasureAndGetElapsedMilliSeconds()
        {
            sw.Stop();
            var ret = sw.ElapsedMilliseconds;
            sw = null;
            return ret;
        }

	}
}
