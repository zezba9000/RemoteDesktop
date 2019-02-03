using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
    public class AudioDecodingPlayerCallback
    {
        private Queue<byte[]> mEncodedFrameQ;

        public AudioDecodingPlayerCallback(Queue<byte[]> encoded_frame_q)
        {
            mEncodedFrameQ = encoded_frame_q;
        }

        public void addEncodedSamplesData(byte[] encoded_data, int length)
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
        public byte[] getEncodedSamplesData()
        {
            lock (this)
            {
                try
                {
                    return mEncodedFrameQ.Dequeue();
                }
                catch (InvalidOperationException ex)
                {
                    //Console.WriteLine("encoded frame deque missed.");
                    return null;
                }
            }
        }
    }


    public interface IPlatformAudioDecodingPlayer
    {
        bool setup(AudioDecodingPlayerCallback callback_ob, int samplingRate, int ch, int bitrate, byte[] csd_data);
        void Close();
    }

    public class AudioDecodingPlayerManager
    {
        private IPlatformAudioDecodingPlayer mADP;
        public bool isOpened = false;
        public AudioDecodingPlayerCallback mCallback;


        private IPlatformAudioDecodingPlayer getInstance()
        {
            return DependencyService.Get<IPlatformAudioDecodingPlayer>();
        }

        // csd_data is first adts frame header
        public bool setup(int samplingRate, int ch, int bitrate, byte[] csd_data)
        {
            mADP = getInstance();
            isOpened = true;
            mCallback = new AudioDecodingPlayerCallback(new Queue<byte[]>());
            return mADP.setup(mCallback, samplingRate, ch, bitrate, csd_data);
        }

        public void Close()
        {
            isOpened = false;
            mADP.Close();
        }
    }
}
