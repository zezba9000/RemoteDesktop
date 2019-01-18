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
            byte[] uvStart = new byte[width * height * 2];
            Array.Copy(yuvBuffer, width * height, uvStart, 0, uvStart.Length);
            byte[] y = new byte[2]{ 0, 0 };
            byte u = 0;
            byte v = 0;
            int r = 0;
            int g = 0;
            int b = 0;
            int uv_idx = 0;
            for (int rowCnt = 0; rowCnt < height; rowCnt++)
            {
                for (int colCnt = 0; colCnt < width; colCnt += 2)
                {
                    u = uvStart[uv_idx + colCnt + 0];
                    v = uvStart[uv_idx + colCnt + 1];

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

        public static int[] YUV420SPtoRGBA8888(byte[] yuv420_data, int width, int height)
        {
            int len = yuv420_data.Length;

            int[] rgba8888_data = new int[width * height];
            int frameSize = width * height;


            // define variables before loops (+ 20-30% faster algorithm o0`)
            int r, g, b, y1192, y, i, uvp, u, v;
            for (int j = 0, yp = 0; j < height; j++)
            {
                uvp = frameSize + (j >> 1) * width;
                u = 0;
                v = 0;
                for (i = 0; i < width; i++, yp++)
                {
                    y = (0xff & ((int)yuv420_data[yp])) - 16;
                    if (y < 0)
                        y = 0;
                    if ((i & 1) == 0)
                    {
                        v = (0xff & yuv420_data[uvp++]) - 128;
                        u = (0xff & yuv420_data[uvp++]) - 128;
                    }

                    y1192 = 1192 * y;
                    r = (y1192 + 1634 * v);
                    g = (y1192 - 833 * v - 400 * u);
                    b = (y1192 + 2066 * u);

                    // Java's functions are faster then 'IFs'
                    r = Math.Max(0, Math.Min(r, 262143));
                    g = Math.Max(0, Math.Min(g, 262143));
                    b = Math.Max(0, Math.Min(b, 262143));

                    //rgba8888_data[yp] = (int)((r << 14) & 0xff000000) | ((g << 6) & 0xff0000) | ((b >> 2) | 0xff00);
                    rgba8888_data[yp] = (int)((0xff000000) | ((r << 6) & 0xff0000) | ((g >> 2) | (b & 0xff00)));
                }
            }

            return rgba8888_data;
        }


        //#define CLAMP(t) (((t)>255)?255:(((t)<0)?0:(t)))
        //// Color space conversion for RGB
        //#define GET_R_FROM_YUV(y, u, v) ((298*y+409*v+128)>>8)
        //#define GET_G_FROM_YUV(y, u, v) ((298*y-100*u-208*v+128)>>8)
        //#define GET_B_FROM_YUV(y, u, v) ((298*y+516*u+128)>>8)

        // 注: 得られるRGB88のビットマップはBGRの順でデータが並んでいる、場合もあるかも
        public static byte[] YUV422toRGB888(byte[] yuv422_data)
        {
            int len = yuv422_data.Length;

            byte[] rgb888_data = new byte[(int)(len * 1.5)];
            int ii = 0;
            int jj = 0;
            int y0, u0, y2, v, t;
            while (ii < len)
            {
                y0 = yuv422_data[ii++] - 16;
                u0 = yuv422_data[ii++] - 128;
                y2 = yuv422_data[ii++] - 16;
                v = yuv422_data[ii++] - 128;

                //u0 = yuv422_data[ii++] - 128;
                //y0 = yuv422_data[ii++] - 16;
                //v = yuv422_data[ii++] - 128;
                //y2 = yuv422_data[ii++] - 16;

                //これが一番近そう？
                //v = yuv422_data[ii++] - 128;
                //y0 = yuv422_data[ii++] - 16;
                //u0 = yuv422_data[ii++] - 128;
                //y2 = yuv422_data[ii++] - 16;

                //y0 = yuv422_data[ii++] - 16;
                //v = yuv422_data[ii++] - 128;
                //y2 = yuv422_data[ii++] - 16;
                //u0 = yuv422_data[ii++] - 128;

                //こっちが一番？
                //y0 = yuv422_data[ii++] - 16;
                //y2 = yuv422_data[ii++] - 16;
                //u0 = yuv422_data[ii++] - 128;
                //v = yuv422_data[ii++] - 128;

                //上と同じようなもんか
                //y0 = yuv422_data[ii++] - 16;
                //y2 = yuv422_data[ii++] - 16;
                //v = yuv422_data[ii++] - 128;
                //u0 = yuv422_data[ii++] - 128;

                //// BGR
                //t = (298*y0+516*u0+128) >> 8;
                //rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                //t = (298*y0-100*u0-208*v+128)>> 8;
                //rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                //t = (298*y0+409*v+128) >> 8;
                //rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));

                //// BGR
                //t = (298*y2+516*u0+128) >> 8;
                //rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                //t = (298*y2-100*u0-208*v+128)>> 8;
                //rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                //t = (298*y2+409*v+128) >> 8;
                //rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));

                // RGB
                t = (298 * y0 + 409 * v + 128) >> 8;
                rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                t = (298 * y0 - 100 * u0 - 208 * v + 128) >> 8;
                rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                t = (298 * y0 + 516 * u0 + 128) >> 8;
                rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));

                // RGB
                t = (298 * y2 + 409 * v + 128) >> 8;
                rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                t = (298 * y2 - 100 * u0 - 208 * v + 128) >> 8;
                rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
                t = (298 * y2 + 516 * u0 + 128) >> 8;
                rgb888_data[jj++] = (byte)(((t) > 255) ? 255 : (((t) < 0) ? 0 : (t)));
            }
            Console.WriteLine("YUV422toRGB888: YUV422 -> " + len.ToString() + " bytes , RGB888 -> " + rgb888_data.Length.ToString() + " bytes");

            return rgb888_data;
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
