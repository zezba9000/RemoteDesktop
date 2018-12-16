using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{

    public interface IPlatformSoundPlayer
    {
        void PlayData(byte[] data, bool flag);
        bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount);
        void Close();
    }

    public static class SoundPlayerFactory
    {
        public static IPlatformSoundPlayer getInstance()
        {
            return DependencyService.Get<IPlatformSoundPlayer>();
        }
    }
}
