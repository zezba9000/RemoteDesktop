using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{

    public class AudioDecoderCallback
    {
        private Queue<byte[]> mEncodedFrameQ;

        public AudioDecoderCallback(Queue<byte[]> encoded_frame_q)
        {
            mEncodedFrameQ = encoded_frame_q;
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


    public interface IPlatformAudioDecodingPlayer
    {
        bool setup(AudioDecoderCallback callback_ob, int samplingRate, int ch, int bitrate);
        void Close();
    }

    public static class AudioDecodingPlayerFactory
    {
        public static IPlatformAudioDecodingPlayer getInstance()
        {
            return DependencyService.Get<IPlatformAudioDecodingPlayer>();
        }
    }
}
