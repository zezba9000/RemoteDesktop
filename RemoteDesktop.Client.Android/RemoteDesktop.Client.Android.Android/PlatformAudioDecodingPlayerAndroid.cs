using Android.Media;
using RemoteDesktop.Client.Android;
using RemoteDesktop.Client.Android.Droid;
using System;
using System.Threading;
using Xamarin.Forms;

using Android.Views;
using Android.Graphics;
using Java.Nio;
using static Android.Media.MediaCodec;
using System.IO;
using Android.OS;
using System.Threading.Tasks;
using Stream = Android.Media.Stream;

[assembly: Dependency(typeof(PlatformVideoDecoderAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{

    public delegate void DecodedPCMHandler(byte[] decoded_data);

    public class AudioDecoderCallback : MediaCodec.Callback
    {
        MediaCodec mDecoder;
        MediaFormat mOutputFormat;
        DecoderCallback mCallbackObj;
        int frameCounter = 0;

        public event DecodedBitmapHandler encodedDataGenerated;

        public AudioDecoderCallback(MediaCodec decoder, DecoderCallback callback_obj)
        {
            mCallbackObj = callback_obj;
            mDecoder = decoder;
        }
        public override void OnError(MediaCodec codec, CodecException e)
        {
            Console.WriteLine(e);
        }

        private void OnInputBufferAvailableInner(MediaCodec mc, int inputBufferId)
        {
            byte[] encoded_data = null;
            while ((encoded_data = mCallbackObj.getEncodedFrameData()) == null)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("OnInputBufferAvailable: got encoded data!");
            
            if (encoded_data != null) {
                int sampleSize = encoded_data.Length;
                if (sampleSize > 0)
                {
                    ByteBuffer inputBuffer = mDecoder.GetInputBuffer(inputBufferId);
                    inputBuffer.Put(encoded_data);
                    Console.WriteLine("QueueInputBuffer inputIndex=" + inputBufferId.ToString());
                    mDecoder.QueueInputBuffer(inputBufferId, 0, sampleSize, frameCounter * 1000 /* 1FPS */, 0);
                }
                else
                {
                    Console.WriteLine("QueueInputBuffer set MediaCodec.BufferFlagEndOfStream");
                    mDecoder.QueueInputBuffer(inputBufferId, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
                }
                frameCounter++;
            }
        }

        override public void OnInputBufferAvailable(MediaCodec mc, int inputBufferId)
        {
            Console.WriteLine("called OnInputBufferAvailable at Decoder");
            Task.Run(() =>
            {
                lock (this)
                {
                    OnInputBufferAvailableInner(mc, inputBufferId);
                }
            });
        }

        override public void OnOutputBufferAvailable(MediaCodec mc, int outputBufferId, BufferInfo info)
        {
            ByteBuffer outputBuffer = mDecoder.GetOutputBuffer(outputBufferId);
            MediaFormat bufferFormat = mDecoder.GetOutputFormat(outputBufferId); // option A
            Console.WriteLine("decoded buffer format:" + bufferFormat.ToString());

            // bufferFormat is equivalent to mOutputFormat
            // outputBuffer is ready to be processed or rendered.

            Console.WriteLine("OnOutputBufferAvailable: outputBufferId = " + outputBufferId.ToString());
            byte[] decoded_data = new byte[info.Size];
            outputBuffer.Position(info.Offset);
            outputBuffer.Get(decoded_data, 0, info.Size);
            mDecoder.ReleaseOutputBuffer(outputBufferId, false);
            Console.WriteLine("call OnDecodeFrame from decoder!");

            Console.WriteLine("bufferFormat.getInteger(MediaFormat.KeyWidth)=" + bufferFormat.GetInteger(MediaFormat.KeyWidth).ToString() + " bufferFormat.getInteger(MediaFormat.KeyHeight)=" + bufferFormat.GetInteger(MediaFormat.KeyHeight).ToString());
            mCallbackObj.OnDecodeFrame(decoded_data, bufferFormat.GetInteger(MediaFormat.KeyWidth), bufferFormat.GetInteger(MediaFormat.KeyHeight));
        }

        override public void OnOutputFormatChanged(MediaCodec mc, MediaFormat format)
        {
            // Subsequent data will conform to new format.
            // Can ignore if using getOutputFormat(outputBufferId)
            mOutputFormat = format; // option B
        }

        void onError()
        {
        }
    }

    public class PlatformAudioDecodingPlayerAndroid : IPlatformAudioDecodingPlayer
    {
        private static String VIDEO = "video/";
        private static String MIME = "video/avc";
        private static String TAG = "VideoDecoder";
        //private MediaExtractor mExtractor;
        private MediaCodec mDecoder;

        private bool eosReceived;
        private Java.Nio.ByteBuffer[] inputBuffers = null;


        //private bool isInput = true;
        private bool first = false;
        private long startWhen = 0;
        private MediaCodec.BufferInfo info = null;
        int inputIndex = -1;
        private MediaFormat mOutputFormat; // member variable
        private MediaFormat inputFormat;
        private DecoderCallback mCallbackObj;

        AudioTrack audioTrack;

        public void PlayData(byte[] data, bool flag)
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

        public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        {
            Encoding depthBits = Encoding.Mp3;
            //if(bitsPerSample == 16)
            //{
            //    depthBits = Encoding.Pcm16bit;
            //}else if (bitsPerSample == 8)
            //{
            //    depthBits = Encoding.Pcm8bit;
            //}

            ChannelOut ch = ChannelOut.Mono;
            //if(channels == 1)
            //{
            //    ch = ChannelOut.Mono;
            //}
            //else
            //{
            //    ch = ChannelOut.Stereo;
            //}
#pragma warning disable CS0618 // Type or member is obsolete
           audioTrack = new AudioTrack(
            // Stream type
            Stream.Music,
            // Frequency
            24000, //samplesPerSecond,
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
            audioTrack.Play();
            return true;
        }

        public void Close()
        {
            audioTrack.Stop();
            audioTrack.Release();
        }

        private long CurrentTimeMillisSharp()
        {
            return (long)(new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds);
        }

        public bool setup(AudioDecoderCallback callback_ob, int samplingRate, int ch, int bitrate)
        {
            HandlerThread callbackThread = new HandlerThread("H264DecoderHandler");
            callbackThread.Start();
            Handler handler = new Handler(callbackThread.Looper);

            mDecoder = MediaCodec.CreateDecoderByType(MIME);
            mCallbackObj = callback_obj;
            mDecoder.SetCallback(new MyCallback(mDecoder, mCallbackObj), handler);

            //mOutputFormat = mDecoder.GetOutputFormat(); // option B
            inputFormat = MediaFormat.CreateVideoFormat(MIME, width, height);
            inputFormat.SetInteger(MediaFormat.KeyMaxInputSize, width * height);
            inputFormat.SetInteger("durationUs", 63446722);
            try
            {
                mDecoder.Configure(inputFormat, null, null, 0 /* Decoder */);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Console.WriteLine("before mDecoder.Start()");
            mDecoder.Start();
            Console.WriteLine("after mDecoder.Start()");

            return true;
        }

        public void Close()
        {
            mDecoder.Stop();
            mDecoder.Release();
            eosReceived = true;
        }
    }
}