//using Plugin.ImageResizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
//using DevKit.Xamarin.ImageKit;
//using DevKit.Xamarin.ImageKit.Abstractions;


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
            for (int ii = 0; ii < len; ii++)
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
            catch (KeyNotFoundException ex)
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
            catch (KeyNotFoundException ex)
            {
                throw new Exception("specified Stopwatch not found!");
            }
            sw.Stop();
            var ret = sw.ElapsedMilliseconds;
            return ret;
        }

        public static long getUnixTime()
        {
            var now = DateTime.UtcNow;
            long unixtime = (long)(now - new DateTime(1970, 1, 1)).TotalSeconds;
            return unixtime;
        }

        public static IPAddress getLocalIP()
        {
            String hostName = Dns.GetHostName();    // 自身のホスト名を取得
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress address in addresses)
            {
                // IPv4 のみプリント
                if ( address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                {
                    Console.WriteLine("getLocalIP func got IP Address: " + address.ToString());
                }
            }
            return addresses[0];
        }

        //public static byte[] scaleBitmapDataAsync(byte[] bitmap, int width, int height)
        //{
        //    var tcs = new TaskCompletionSource<byte[]>();
        //    innerScaleBitmapDataAsync(bitmap, width, height, tcs);
        //    var task = tcs.Task;
        //    task.Wait();
        //    return task.Result;
        //}


        //private async static void innerScaleBitmapDataAsync(byte[] bitmap, int width, int height, TaskCompletionSource<byte[]> tcs)
        //{
        //    try
        //    {
        //        width = 100;
        //        height = 100;
        //        byte[] resizedImage = await CrossImageResizer.Current.ResizeImageWithAspectRatioAsync(bitmap, width, height);
        //        tcs.SetResult(resizedImage);
        //    }
        //    catch (Exception ex)
        //    {
        //        tcs.SetException(ex);
        //    }
        //}

    }
}
