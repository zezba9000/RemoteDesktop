using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
    public delegate void DecodedBitmapHandler(byte[] decoded_data, int width, int height);

    public class DecoderCallback
    {
        private Queue<byte[]> mEncodedFrameQ;
        public event DecodedBitmapHandler encodedDataGenerated;

        public DecoderCallback(Queue<byte[]> encoded_frame_q)
        {
            mEncodedFrameQ = encoded_frame_q;
        }

        public void OnDecodeFrame(byte[] frame_data, int width, int height)
        {
            Console.WriteLine("OnDecodeFrame callback called!");
            byte[] copied_buf = new byte[frame_data.Length];
            Array.Copy(frame_data, 0, copied_buf, 0, frame_data.Length);
            Device.BeginInvokeOnMainThread(() =>
            {
                encodedDataGenerated(copied_buf, width, height);
            });
        }

        public void addEncodedFrameData(byte[] encoded_data, int length)
        {
            lock (this)
            {
                byte[] copied_data = new byte[length];
                Array.Copy(encoded_data, 0, copied_data, 0, length);
                mEncodedFrameQ.Enqueue(copied_data);
            };
        }

        // i frame data is not prepared, return NULL
        // for Decoder
        public byte[] getEncodedFrameData()
        {
            lock (this)
            {
                try
                {
                    return mEncodedFrameQ.Dequeue();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("encoded frame deque missed.");
                    return null;
                }
            }
        }
    }


    public interface IPlatformVideoDecoder
    {        
        bool setup(DecoderCallback callback_obj, int width, int heightj);
        void Close();
    }

    public static class VideoDecoderFactory
    {
        public static IPlatformVideoDecoder getInstance()
        {
            return DependencyService.Get<IPlatformVideoDecoder>();
        }
    }
}
