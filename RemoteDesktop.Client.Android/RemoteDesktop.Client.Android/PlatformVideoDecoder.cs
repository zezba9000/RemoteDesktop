using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
    public delegate void DecodedBitmapHandler(byte[] decoded_data);

    public class DecoderCallback
    {
        private Queue<byte[]> mEncodedFrameQ;
        public event DecodedBitmapHandler encodedDataGenerated;

        public DecoderCallback(Queue<byte[]> encoded_frame_q)
        {
            mEncodedFrameQ = encoded_frame_q;
        }

        public void OnDecodeFrame(byte[] frame_data)
        {
            Console.WriteLine("OnDecodeFrame callback called!");
            encodedDataGenerated(frame_data);
        }

        public void addEncodedFrameData(byte[] encoded_data, int length)
        {
            byte[] copied_data = new byte[length];
            Array.Copy(encoded_data, 0, copied_data, 0, length);
            mEncodedFrameQ.Enqueue(copied_data);
        }

        // i frame data is not prepared, return NULL
        // for Decoder
        public byte[] getEncodedFrameData()
        {
            try
            {
                return mEncodedFrameQ.Dequeue();
            }
            catch(InvalidOperationException ex)
            {
                Console.WriteLine("encoded frame deque missed.");
                return null;
            }
        }
    }


    public interface IPlatformVideoDecoder
    {
        bool setup(DecoderCallback callback_obj);
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
