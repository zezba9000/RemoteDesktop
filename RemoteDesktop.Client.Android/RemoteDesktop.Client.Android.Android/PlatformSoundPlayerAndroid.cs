using RemoteDesktop.Client.Android;
using RemoteDesktop.Client.Android.Droid;
using Xamarin.Forms;

[assembly: Dependency (typeof (PlatformSoundPlayerAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{
        public class PlatformSoundPlayerAndroid: IPlatformSoundPlayer
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