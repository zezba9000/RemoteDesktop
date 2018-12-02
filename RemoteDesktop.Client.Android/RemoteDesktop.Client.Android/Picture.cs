using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
    class Picture
    {
        private int headerSize = 54;
        private byte[] buffer;

        public Picture(Dictionary<(int, int), (byte, byte, byte, byte)> colorInfo, int width, int height)
        {
            buffer = MakeBuffer(width, height);
            foreach (var info in colorInfo)
            {
                var (row, col) = info.Key;
                var (a, r, g, b) = info.Value;
                SetPixel(row, col, width, r, g, b, a);
            }
        }

        private byte[] MakeBuffer(int width, int height)
        {
            //buffer作成
            var numPixels = width * height;
            var numPixelBytes = 4 * numPixels;
            var filesize = headerSize + numPixelBytes;
            var buffer = new byte[filesize];

            //bufferにheader情報を書き込む

            using (var memoryStream = new MemoryStream(buffer))
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8))
                {
                    writer.Write(new char[] { 'B', 'M' });
                    writer.Write(filesize);
                    writer.Write((short)0);
                    writer.Write((short)0);
                    writer.Write(headerSize);

                    writer.Write(40);
                    writer.Write(width);
                    writer.Write(height);
                    writer.Write((short)1);
                    writer.Write((short)32);
                    writer.Write(0);
                    writer.Write(numPixelBytes);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                }

                return buffer;
            }
        }

        private void SetPixel(int row, int col, int width, int r, int g, int b, int a = 255)
        {
            var index = (row * width + col) * 4 + headerSize;
            buffer[index + 0] = (byte)b;
            buffer[index + 1] = (byte)g;
            buffer[index + 2] = (byte)r;
            buffer[index + 3] = (byte)a;
        }

        public ImageSource GetImageSource()
        {
            MemoryStream memoryStream = new MemoryStream(buffer);

            ImageSource imageSource = ImageSource.FromStream(() =>
            {
                return memoryStream;
            });

            return imageSource;
        }
    }
}
