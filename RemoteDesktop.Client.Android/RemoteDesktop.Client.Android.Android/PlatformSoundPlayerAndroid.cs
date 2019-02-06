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

        public void WriteData(byte[] data, bool flag)
        {
            audioTrack.Write(data, 0, data.Length);
            //int len = data.Length / 4;
            //float[] fdata = new float[len];
            //for(int idx = 0; idx < len; idx++)
            //{
            //    fdata[idx] = BitConverter.ToSingle(data, idx * 4);
            //    //Console.WriteLine(fdata[idx].ToString());
            //}
            //const int WRITE_BLOCKING = 0x00000000;
            //audioTrack.Write(fdata, 0, len, WRITE_BLOCKING);
        }

        public bool Play()
        {
            if(audioTrack != null)
            {
                audioTrack.Play();
                return true;
            }
            else
            {
                throw new Exception("audioTrack is not opend");
            }
        }

        public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        {
            Encoding depthBits = Encoding.Pcm16bit;
            if (bitsPerSample == 16)
            {
                depthBits = Encoding.Pcm16bit;
            }
            else if (bitsPerSample == 8)
            {
                depthBits = Encoding.Pcm8bit;
            }

            ChannelOut ch = ChannelOut.Mono;
            if (channels == 1)
            {
                ch = ChannelOut.Mono;
            }
            else
            {
                ch = ChannelOut.Stereo;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            audioTrack = new AudioTrack(
            // Stream type
            Stream.Music,
            // Frequency
            samplesPerSecond, //samplesPerSecond,
            // Mono or stereo
            ch,
            // Audio encoding
            depthBits,
            //Encoding.PcmFloat,
            //Encoding.Pcm8bit,
            // Length of the audio clip.
            //1024 * 1024,
            bufferCount,
            // Mode. Stream or static.
            AudioTrackMode.Stream);
#pragma warning restore CS0618 // Type or member is obsolete
            
            return true;
        }

        public void Close()
        {
            audioTrack.Stop();
            audioTrack.Release();
        }
    }
}