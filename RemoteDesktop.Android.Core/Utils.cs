//using Plugin.ImageResizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
//using DevKit.Xamarin.ImageKit;
//using DevKit.Xamarin.ImageKit.Abstractions;


namespace RemoteDesktop.Android.Core
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
                Console.WriteLine(ex);
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
                // IPv4 のみ
                if ( address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                {
                    return address;
                    ///Console.WriteLine("getLocalIP func got IP Address: " + address.ToString());
                }
            }
            return null;
            
        }

        // 渡したバイト配列はバッファとして内包していない
        public static MemoryStream getAddHeaderdBitmapStreamByPixcels(byte[] pixels, int width, int height)
        {
            //buffer作成
            var numPixels = width * height; 
            //var numPixelBytes = 2 * numPixels; // RGB565
            //var numPixelBytes = 3 * numPixels; // RGB24
            var numPixelBytes = 4 * numPixels; // RGB32
            var headerSize = 54;
            var filesize = headerSize + numPixelBytes;


            //bufferにheader情報を書き込む
            var memoryStream = new MemoryStream(filesize);
            var writer = new BinaryWriter(memoryStream, Encoding.UTF8);
            writer.Write(new char[] { 'B', 'M' });
            writer.Write(filesize);
            writer.Write((short)0);
            writer.Write((short)0);
            writer.Write(headerSize);

            writer.Write(40);
            writer.Write(width);
            writer.Write(height);
            writer.Write((short)1);
            //writer.Write((short)16); //RGB565 = 16bit
            //writer.Write((short)24); //RGB24
            writer.Write((short)32); //RGB32
            writer.Write(0);
            writer.Write(numPixelBytes);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            writer.Write(pixels);

            writer.Flush();

            return memoryStream;
        }

        public static void saveByteArrayToFile(byte[] data, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }

        //// for canvas setting is Argb8888
        //public static byte[] convertBitmapBGR24toBGRA32(byte[] bitmap)
        //
        //    int pixels = bitmap.Length / 3;
        //    byte[] conved = new byte[pixels * 4];
        //    for (int idx = 0; idx < pixels; idx++)
        //    {
        //        conved[idx * 4] = bitmap[idx * 3];
        //        conved[idx * 4 + 1] = bitmap[idx * 3 + 1];
        //        conved[idx * 4 + 2] = bitmap[idx * 3 + 2];
        //        conved[idx * 4 + 3] = 0xFF;
        //    }

        //    return conved;
        //}

        //// for canvas setting is Argb8888
        //public static byte[] convertBitmapAbgr16_1555toBGR32(byte[] bitmap)
        //{
        //    //byte[] toCheck = new byte[10];
        //    //if (bitmap.Length > toCheck.Length)
        //    //{
        //    //    Array.Copy(bitmap, 0, toCheck, 0, toCheck.Length);
        //    //    BitArray ba2 = new BitArray(toCheck);
        //    //    var cnt2 = 0;
        //    //    foreach (Boolean bit in ba2)
        //    //    {
        //    //        if (cnt2 % 16 == 0)
        //    //        {
        //    //            Console.WriteLine("");
        //    //        }
        //    //        Console.Write(bit == true ? 1 : 0);
        //    //        cnt2++;
        //    //    }
        //    //}


        //    int pixels = bitmap.Length / 2;
        //    byte[] conved = new byte[pixels * 4];
        //    for (int idx = 0; idx < pixels; idx++)
        //    {
        //        byte[] pixel_buf = new byte[2];
        //        byte[] rgb_5bit = new byte[3];
        //        rgb_5bit[0] = 0; //r
        //        rgb_5bit[1] = 0; //g
        //        rgb_5bit[2] = 0; //b

        //        Array.Copy(bitmap, idx * 2, pixel_buf, 0, 2);
        //        var ba = new BitArray(pixel_buf);
        //        //var enumer = ba.GetEnumerator();
        //        int cur_color = 0; // 0->R, 1->G, 2->B
        //        int cnt = -1;
        //        foreach(Boolean bit in ba)
        //        {
        //            //if (cnt == -1)
        //            //{
        //            //    cnt = 0;
        //            //    continue;
        //            //}
        //            if (cnt == 15) // アルファ値の1bitが末尾にあることを想定
        //            {
        //                break;
        //            }
        //            if (cnt != 0 && cnt % 5 == 0) { cur_color++; }
        //            //int shifts = 7 - (cnt % 5);
        //            int shifts = (cnt % 5) + 3;
        //            if (bit) { rgb_5bit[cur_color] += (byte) (1 << shifts); }
        //            //Console.WriteLine("double_image: idx=" + idx.ToString() + " cnt=" + cnt.ToString() + " bit=" + bit.ToString() + 
        //            //    " rgb_5bit[" + cur_color.ToString() + "]=" + rgb_5bit[cur_color].ToString() + " 1 << " + shifts.ToString() + 
        //            //    " -> " + (1 << shifts).ToString());
        //            cnt++;
        //        }
        //        conved[idx * 4] = rgb_5bit[0];
        //        conved[idx * 4 + 1] = rgb_5bit[1];
        //        conved[idx * 4 + 2] = rgb_5bit[2];
        //        conved[idx * 4 + 3] = 0xFF;
        //    }

        //    return conved;
        //}

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
