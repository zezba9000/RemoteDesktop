using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using RemoteDesktop.Android.Core;

namespace RemoteDesktop.Client.Android
{

    public class SoundManager
    {

        public class Player
        {
            public bool Opened = false;
            public bool isPlayingStarted = false;
            IPlatformSoundPlayer pplayer;

            public Player()
            {
                pplayer = SoundPlayerFactory.getInstance();
            }

            public void WriteData(byte[] data, bool flag)
            {
                pplayer.WriteData(data, flag);
            }

            public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
            {
                pplayer.Open(waveOutDeviceName, samplesPerSecond, bitsPerSample, channels, bufferCount);
                Opened = true;
                return true;
            }

            public bool Play()
            {
                isPlayingStarted = true;
                return pplayer.Play();
            }

            public void Close()
            {
                pplayer.Close();
                Opened = false;
            }
        }

        //public void Play() { }

        public class Stopwatch
        {
        }

        public class JitterBuffer
        {
            public delegate void DelegateDataAvailable(Object sender, RTPPacket packet);
            public event DelegateDataAvailable DataAvailable;

            public JitterBuffer(Object sender, uint maxRTPPackets, uint timerIntervalInMilliseconds)
            {
            }

            public void AddData(RTPPacket rtp)
            {

            }

            public void Start() { }

            public void Stop() { }
        }
    }
}
