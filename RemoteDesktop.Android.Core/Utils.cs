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
                //throw new Exception("specified Stopwatch not found!");
                Console.WriteLine("ERROR: specified Stopwatch not found. But running keeps (DEBUG)");
                return -1;
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
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
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
            var numPixelBytes = 3 * numPixels; // RGB24
            //var numPixelBytes = 4 * numPixels; // RGB32
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
            writer.Write((short)24); //RGB24
            //writer.Write((short)32); //RGB32
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


        public static int CLIP(int x)
        {
            int ret = x;
            if (x < 0)
            {
                ret = 0;
            }
            else if (x > 255)
            {
                ret = 255;
            }
            return ret;
        }

        public static int CONVERT_R(int Y, int V)
        {
            return ((298 * (Y - 16) + 409 * (V - 128) + 128) >> 8);
        }

        public static int CONVERT_G(int Y, int U, int V)
        {
            return ((298 * (Y - 16) - 100 * (U - 128) - 208 * (V - 128) + 128) >> 8);
        }

        public static int CONVERT_B(int Y, int U)
        {
            return ((298 * (Y - 16) + 516 * (U - 128) + 128) >> 8);
        }

        public static byte[] NV12ToRGBA8888(byte[] yuvBuffer, int width, int height)
        {
            byte[] rgbBuffer = new byte[width * height * 4];
            byte[] y = new byte[2]{ 0, 0 };
            byte u = 0;
            byte v = 0;
            int r = 0;
            int g = 0;
            int b = 0;
            int uv_idx = width * height;
            for (int rowCnt = 0; rowCnt < height; rowCnt++)
            {
                for (int colCnt = 0; colCnt < width; colCnt += 2)
                {
                    u = yuvBuffer[uv_idx + colCnt + 0];
                    v = yuvBuffer[uv_idx + colCnt + 1];

                    for (int cnt = 0; cnt < 2; cnt++)
                    {
                        y[cnt] = yuvBuffer[rowCnt * width + colCnt + cnt];

                        r = CONVERT_R(y[cnt], v);
                        r = CLIP(r);
                        g = CONVERT_G(y[cnt], u, v);
                        g = CLIP(g);
                        b = CONVERT_B(y[cnt], u);
                        b = CLIP(b);
                        rgbBuffer[(rowCnt * width + colCnt + cnt) * 4 + 0] = (byte)r;
                        rgbBuffer[(rowCnt * width + colCnt + cnt) * 4 + 1] = (byte)g;
                        rgbBuffer[(rowCnt * width + colCnt + cnt) * 4 + 2] = (byte)b;
                        rgbBuffer[(rowCnt * width + colCnt + cnt) * 4 + 3] = (byte)0xFF;
                    }
                }

                uv_idx += width * (rowCnt % 2);
            }
            return rgbBuffer;
        }


        //// for canvas setting is Argb8888
        public static byte[] convertBitmapBGR24toBGRA32(byte[] bitmap)
        {
            int pixels = bitmap.Length / 3;
            byte[] conved = new byte[pixels * 4];
            for (int idx = 0; idx<pixels; idx++)
            {
                conved[idx * 4] = bitmap[idx * 3];
                conved[idx * 4 + 1] = bitmap[idx * 3 + 1];
                conved[idx * 4 + 2] = bitmap[idx * 3 + 2];
                conved[idx * 4 + 3] = 0xFF;
            }

            return conved;
        }

    }
}
