using Android.Media;
using RemoteDesktop.Client.Android;
using RemoteDesktop.Client.Android.Droid;
using System;
using Xamarin.Forms;

[assembly: Dependency (typeof (PlatformSoundPlayerAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{
        public class PlatformSoundPlayerAndroid: IPlatformSoundPlayer
    {
        AudioTrack audioTrack;

        public void PlayData(byte[] data, bool flag)
        {
            //audioTrack.Write(data, 0, data.Length);
            int len = data.Length / 4;
            float[] fdata = new float[len];
            for(int idx = 0; idx < len; idx++)
            {
                fdata[idx] = BitConverter.ToSingle(data, idx * 4);
                //Console.WriteLine(fdata[idx].ToString());
            }
            const int WRITE_BLOCKING = 0x00000000;
            audioTrack.Write(fdata, 0, len, WRITE_BLOCKING);
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
            ChannelOut.Stereo,
            // Audio encoding
            Encoding.PcmFloat,
            //Encoding.Pcm16bit,
            //Encoding.Pcm8bit,
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
            audioTrack.Stop();
            audioTrack.Release();
        }
    }
}