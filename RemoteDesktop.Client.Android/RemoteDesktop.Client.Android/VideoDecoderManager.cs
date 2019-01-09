using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteDesktop.Client.Android
{
    class VideoDecoderManager
    {
        public bool Opened = false;
        IPlatformVideoDecoder pdecoder;

        public VideoDecoderManager()
        {
            pdecoder = VideoDecoderFactory.getInstance();
        }

        //public void PlayData(byte[] data, bool flag)
        //{
        //    pplayer.PlayData(data, flag);
        //}

        //public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        //{
        //    pplayer.Open(waveOutDeviceName, samplesPerSecond, bitsPerSample, channels, bufferCount);
        //    Opened = true;
        //    return true;
        //}

        public bool setup()
        {
            return pdecoder.setup();
        }

        public byte[] getDecodedFrame()
        {
            return pdecoder.getDecodedFrame();
        }

        public void addEncodedFrame(byte[] frame_data)
        {
            pdecoder.addEncodedFrame(frame_data);
        }

        public void Close()
        {
            pdecoder.Close();
            Opened = false;
        }
    }
}
