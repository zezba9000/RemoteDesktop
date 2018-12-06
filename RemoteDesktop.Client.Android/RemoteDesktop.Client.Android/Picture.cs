using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
    class Picture : INotifyPropertyChanged
    {
        public static int headerSize = 54;
        private byte[] buffer;

        public Picture(Dictionary<(int, int), (byte, byte, byte, byte)> colorInfo, int width, int height)
        {
            if(colorInfo == null)
            {
                // set data directly to buffer field
                MakeBufferRandomImageFilled(width, height);
            }
            else
            {
                buffer = MakeBuffer(width, height);
                foreach (var info in colorInfo)
                {
                    var (row, col) = info.Key;
                    var (a, r, g, b) = info.Value;
                    //SetPixel(row, col, width, r, g, b, a);
                    SetPixel(row, col, width, r, g, b);
                }
            }
        }

        public byte[] getInternalBuffer()
        {
            return buffer;
        }

        private void MakeBufferRandomImageFilled(int width, int height)
        {
            if(buffer == null)
            {
                buffer = MakeBuffer(width, height);
            }
            byte r = (byte)MainPage.rnd.Next(256);
            byte g = (byte)MainPage.rnd.Next(256);
            byte b = (byte)MainPage.rnd.Next(256);
            //byte a = (byte)255;
            long index = 0;
            for (int h = 0;h < height; h++)
            {
                for(int w = 0; w < width; w++)
                {
                    index = (h * width + w) * 3 + headerSize;
                    buffer[index + 0] = b;
                    buffer[index + 1] = g;
                    buffer[index + 2] = r;
                    //buffer[index + 3] = a;
                }
            }
        }

        private byte[] MakeBuffer(int width, int height)
        {
            //buffer作成
            var numPixels = width * height;
            var numPixelBytes = 3 * numPixels;
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
                    writer.Write((short)24); //RGB 8*3=24, alpha is not conatined
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

        //private void SetPixel(int row, int col, int width, int r, int g, int b, int a = 255)
        private void SetPixel(int row, int col, int width, int r, int g, int b)
        {
            long index = (row * width + col) * 3 + headerSize;
            buffer[index + 0] = (byte)b;
            buffer[index + 1] = (byte)g;
            buffer[index + 2] = (byte)r;
            //buffer[index + 3] = (byte)a;
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

        public ImageSource Source
        {
            get
            {
                MemoryStream memoryStream = new MemoryStream(buffer);

                ImageSource imageSource = ImageSource.FromStream(() =>
                {
                    return memoryStream;
                });

                return imageSource;
            }
        }

        public void setStateUpdated()
        {
            notifyPropertyChanged("Source");
        }

        public void updateContent(Dictionary<(int, int), (byte, byte, byte, byte)> colorInfo, int width, int height)
        {
            if(colorInfo == null)
            {
                // set data directly to buffer field
                MakeBufferRandomImageFilled(width, height);
            }
            else
            {
                buffer = MakeBuffer(width, height);
                foreach (var info in colorInfo)
                {
                    var (row, col) = info.Key;
                    var (a, r, g, b) = info.Value;
                    //SetPixel(row, col, width, r, g, b, a);
                    SetPixel(row, col, width, r, g, b);
                }
            }
            notifyPropertyChanged("Source");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void notifyPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
