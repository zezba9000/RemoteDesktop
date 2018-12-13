using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace RemoteDesktop.Core
{
	public static class Utils
	{
        private static Dictionary<string, Stopwatch> sw_dic = new Dictionary<string, Stopwatch>(); 
        //private static Stopwatch sw = null;

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

        public static void startTimeMeasure(string sw_name)
        {
            Stopwatch sw = null;
            try
            {
                sw = sw_dic[sw_name];
                sw.Reset();
            }
            catch(KeyNotFoundException ex)
            {
                sw = new Stopwatch();
                sw_dic[sw_name] = sw;
            }

            sw.Start();
        }

        public static long stopMeasureAndGetElapsedMilliSeconds(string sw_name)
        {
            Stopwatch sw = null;
            try
            {
                sw = sw_dic[sw_name];
            }
            catch(KeyNotFoundException ex)
            {
                throw new Exception("specified Stopwatch not found!");
            }
            sw.Stop();
            var ret = sw.ElapsedMilliseconds;
            return ret;
        }

	}
}
