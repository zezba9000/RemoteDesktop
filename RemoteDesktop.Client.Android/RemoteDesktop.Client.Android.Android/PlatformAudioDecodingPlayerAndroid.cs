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

[assembly: Dependency(typeof(PlatformAudioDecodingPlayerAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{

    public delegate void DecodedPCMHandler(byte[] decoded_data);

    public class AudioDecoderCallback : MediaCodec.Callback
    {
        MediaCodec mDecoder;
        MediaFormat mOutputFormat;
        AudioDecodingPlayerCallback mCallbackObj;
        PlatformAudioDecodingPlayerAndroid mADP;
        int frameCounter = 0;
        public byte[] CSD0;

        public event DecodedBitmapHandler encodedDataGenerated;

        public AudioDecoderCallback(MediaCodec decoder, AudioDecodingPlayerCallback callback_obj, PlatformAudioDecodingPlayerAndroid parent)
        {
            mCallbackObj = callback_obj;
            mDecoder = decoder;
            mADP = parent;
        }
        public override void OnError(MediaCodec codec, CodecException e)
        {
            Console.WriteLine(e);
        }

        private void OnInputBufferAvailableInner(MediaCodec mc, int inputBufferId)
        {
            byte[] encoded_data = null;
            while ((encoded_data = mCallbackObj.getEncodedSamplesData()) == null)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("OnInputBufferAvailable: got encoded data!");
            
            if (encoded_data != null) {
                int sampleSize = encoded_data.Length;
                if (sampleSize > 0)
                {
                    ByteBuffer inputBuffer = mDecoder.GetInputBuffer(inputBufferId);
                    inputBuffer.Position(0);

                    //inputBuffer.Put(encoded_data);
                    Console.WriteLine("QueueInputBuffer inputIndex=" + inputBufferId.ToString());
                    //if (frameCounter == 0)
                    //{
                    //    //inputBuffer.Put(CSD0);
                    //    inputBuffer.Put(encoded_data);
                    //    // 最初のフレームはcds-0の2byteになるようにしてある <- 今はADTSフレームの一番目が来る。データもくっついている。
                    //    mDecoder.QueueInputBuffer(inputBufferId, 0, sampleSize, 0, MediaCodec.BufferFlagCodecConfig);
                    //}
                    //else
                    //{
                        // remove adts header
                        //inputBuffer.Put(encoded_data, 9, encoded_data.Length - 9);
                        //mDecoder.QueueInputBuffer(inputBufferId, 0, encoded_data.Length - 9, 0, 0);

                        inputBuffer.Put(encoded_data);
                        mDecoder.QueueInputBuffer(inputBufferId, 0, sampleSize, 0, 0);
                    //}
                    frameCounter++;
                }
                else
                {
                    Console.WriteLine("QueueInputBuffer set MediaCodec.BufferFlagEndOfStream");
                    mDecoder.QueueInputBuffer(inputBufferId, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
                }
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

            //Console.WriteLine("bufferFormat.getInteger(MediaFormat.KeyWidth)=" + bufferFormat.GetInteger(MediaFormat.KeyWidth).ToString() + " bufferFormat.getInteger(MediaFormat.KeyHeight)=" + bufferFormat.GetInteger(MediaFormat.KeyHeight).ToString());
            //mCallbackObj.OnDecodeFrame(decoded_data, bufferFormat.GetInteger(MediaFormat.KeyWidth), bufferFormat.GetInteger(MediaFormat.KeyHeight));
            mADP.PlayData(decoded_data, true);
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
        //private static String AUDIO = "video/";
        //private static String MIME = "video/avc";
        //private static String TAG = "VideoDecoder";
        //private MediaExtractor mExtractor;
        private MediaCodec mDecoder;

        private bool eosReceived;
        private Java.Nio.ByteBuffer[] inputBuffers = null;


        //private bool isInput = true;
        private bool first = false;
        private long startWhen = 0;
        private MediaCodec.BufferInfo info = null;
        int inputIndex = -1;
        //private MediaFormat mOutputFormat; // member variable
        //private MediaFormat inputFormat;
        private AudioDecodingPlayerCallback mCallbackObj;

        AudioTrack audioTrack;

        public void PlayData(byte[] data, bool flag)
        {
            Console.WriteLine(" " + DateTime.Now.ToString("yyyy/MM/ dd hh: mm: ss.fff") + " call PlayData: " + data.Length.ToString() + " bytes, " + (data.Length / 24000.0 / 2.0).ToString() + " sec at playing time");
            audioTrack.Write(data, 0, data.Length);
        }

        public bool OpenDevice(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
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
            44100, //samplesPerSecond,
            // Mono or stereo
            ch,
            // Audio encoding
            depthBits,
            bufferCount,
            // Mode. Stream or static.
            AudioTrackMode.Stream);
#pragma warning restore CS0618 // Type or member is obsolete
            audioTrack.Play();
            return true;
        }

        public void PlayerClose()
        {
            audioTrack.Stop();
            audioTrack.Release();
        }

        private long CurrentTimeMillisSharp()
        {
            return (long)(new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds);
        }

        public bool setup(AudioDecodingPlayerCallback callback_obj, int samplingRate, int ch, int bitrate, byte[] csd_data)
        {
            OpenDevice("hoge", samplingRate, 16, ch, 32 * 1024);

            mCallbackObj = callback_obj;
            HandlerThread callbackThread = new HandlerThread("AACDecodingPlayerHandler");
            callbackThread.Start();
            Handler handler = new Handler(callbackThread.Looper);

            mDecoder = MediaCodec.CreateDecoderByType("audio/mp4a-latm");
            var mMediaFormat = MediaFormat.CreateAudioFormat("audio/mp4a-latm", samplingRate, ch);
            //byte[] bytes = new byte[] { (byte)0x12, (byte)0x12 };
            ByteBuffer csd = ByteBuffer.Wrap(csd_data);
            mMediaFormat.SetByteBuffer("csd-0", csd);
            mMediaFormat.SetInteger(MediaFormat.KeyIsAdts, 0);

            //int profile = (csd_data[2] & 0xC0) >> 6;
            //int srate = (csd_data[2] & 0x3C) >> 2;
            //int channel = ((csd_data[2] & 0x01) << 2) | ((csd_data[3] & 0xC0) >> 6);
            //sbyte csd0_0s = (sbyte)(((profile + 1) << 3) | srate >> 1);
            //sbyte csd0_1s = (sbyte)(((profile + 1) << 3) | srate >> 1);
            //byte csd0_0 = (byte)(((profile + 1) << 3) | srate >> 1);
            //byte csd0_1 = (byte)(((profile + 1) << 3) | srate >> 1);
            //byte[] bytes = new byte[] { csd0_0, csd0_1 };
            //ByteBuffer csd = ByteBuffer.Wrap(bytes);
            //mMediaFormat.SetInteger(MediaFormat.KeyIsAdts, 1);

            //ByteBuffer csd = ByteBuffer.Allocate(2);
            //csd.Put(0, csd0_0s);
            //csd.Put(1, csd0_1s);
            //mMediaFormat.SetByteBuffer("csd-0", csd);

            var cbk = new AudioDecoderCallback(mDecoder, mCallbackObj, this);
            //cbk.CSD0 = csd0_data;
            mDecoder.SetCallback(cbk, handler);
            mDecoder.Configure(mMediaFormat, null, null, 0);

   //         String audioCodecType = "audio/mp4a-latm";
   //         mDecoder = MediaCodec.CreateDecoderByType(audioCodecType);
   //         mCallbackObj = callback_obj;
   //         mDecoder.SetCallback(new AudioDecoderCallback(mDecoder, mCallbackObj, this), handler);
   //         MediaFormat format = MediaFormat.CreateAudioFormat(audioCodecType, samplingRate, ch);
			//mDecoder.Configure(format, null, null, 0);

            //mOutputFormat = mDecoder.GetOutputFormat(); // option B
            //inputFormat.SetInteger(MediaFormat.KeyMaxInputSize, width * height);
            //inputFormat.SetInteger("durationUs", 63446722);

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
            PlayerClose();
        }
    }
}