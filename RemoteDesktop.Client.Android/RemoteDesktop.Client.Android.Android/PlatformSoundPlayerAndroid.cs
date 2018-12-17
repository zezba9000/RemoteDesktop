using Android.Media;
using RemoteDesktop.Client.Android;
using RemoteDesktop.Client.Android.Droid;
using Xamarin.Forms;

[assembly: Dependency (typeof (PlatformSoundPlayerAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{
        public class PlatformSoundPlayerAndroid: IPlatformSoundPlayer
    {
        AudioTrack audioTrack;

        public void PlayData(byte[] data, bool flag)
        {
            audioTrack.Write(data, 0, data.Length);
        }

        public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        {
#pragma warning disable CS0618 // Type or member is obsolete
           audioTrack = new AudioTrack(
            // Stream type
            Stream.Music,
            // Frequency
            samplesPerSecond,
            // Mono or stereo
            ChannelOut.Mono,
            // Audio encoding
            Encoding.Pcm16bit,
            // Length of the audio clip.
            1024 * 1024,
            // Mode. Stream or static.
            AudioTrackMode.Stream);
#pragma warning restore CS0618 // Type or member is obsolete
            audioTrack.Play();
            return true;
        }

        public void Close()
        {

        }
    }
}