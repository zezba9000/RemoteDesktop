using RemoteDesktop.Client.Android;
using RemoteDesktop.Client.Windows;
using Xamarin.Forms;

[assembly: Dependency (typeof (PlatformSoundPlayerWindows))]

namespace RemoteDesktop.Client.Windows
{
        public class PlatformSoundPlayerWindows: IPlatformSoundPlayer
    {
        public void PlayData(byte[] data, bool flag)
        {

        }

        public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        {
            return true;
        }

        public void Close()
        {

        }
    }
}