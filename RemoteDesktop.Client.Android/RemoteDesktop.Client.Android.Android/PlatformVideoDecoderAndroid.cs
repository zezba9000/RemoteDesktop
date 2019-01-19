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

[assembly: Dependency(typeof(PlatformVideoDecoderAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{

    public class AviFileContentDataSource : MediaDataSource
    {
        private MemoryStream content_ms;

        public AviFileContentDataSource(byte[] content_buf)
        {
            content_ms = new MemoryStream(content_buf);
            content_ms.Position = 0;
        }

        public override long Size
        {
            get
            {
                return content_ms.Length;
            }
        }

        public override void Close()
        {
            content_ms.Close();
            content_ms.Dispose();
        }

        public override int ReadAt(long position, byte[] buffer, int offset, int size)
        {
            content_ms.Seek(position, SeekOrigin.Begin);
            return content_ms.Read(buffer, offset, size);
        }
    }

    public delegate void DecodedBitmapHandler(byte[] decoded_data);

    public class MyCallback : MediaCodec.Callback
    {
        MediaCodec mDecoder;
        MediaFormat mOutputFormat;
        DecoderCallback mCallbackObj;
        int frameCounter = 0;

        public event DecodedBitmapHandler encodedDataGenerated;

        public MyCallback(MediaCodec decoder, DecoderCallback callback_obj)
        {
            mCallbackObj = callback_obj;
            mDecoder = decoder;
        }
        public override void OnError(MediaCodec codec, CodecException e)
        {
            Console.WriteLine(e);
        }

        override public void OnInputBufferAvailable(MediaCodec mc, int inputBufferId)
        {
            byte[] encoded_data; // = mCallbackObj.getEncodedFrameData();
            while ((encoded_data = mCallbackObj.getEncodedFrameData()) == null)
            {
                Thread.Sleep(1000);
            }
            if (encoded_data != null)
            {
                int sampleSize = encoded_data.Length;
                if (sampleSize > 0)
                {
                    ByteBuffer inputBuffer = mDecoder.GetInputBuffer(inputBufferId);
                    inputBuffer.Put(encoded_data);

                    if (frameCounter == 0)
                    {
                        Console.WriteLine("feed a frame contains SSP and PSP");
                        mDecoder.QueueInputBuffer(inputBufferId, 0, sampleSize, 0, MediaCodec.BufferFlagCodecConfig);
                    }else
                    {
                        Console.WriteLine("QueueInputBuffer inputIndex=" + inputBufferId.ToString());
                        mDecoder.QueueInputBuffer(inputBufferId, 0, sampleSize, frameCounter * 1000 /* 1FPS */, 0);
                    }
                }
                else
                {
                    Console.WriteLine("QueueInputBuffer set MediaCodec.BufferFlagEndOfStream");
                    mDecoder.QueueInputBuffer(inputBufferId, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
                }
                frameCounter++;
            }
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

            mCallbackObj.OnDecodeFrame(decoded_data);
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

    public class PlatformVideoDecoderAndroid : IPlatformVideoDecoder
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

        private long CurrentTimeMillisSharp()
        {
            return (long)(new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds);
        }

        public bool setup(DecoderCallback callback_obj) //format_hint is aviFileContent 
        {
            HandlerThread callbackThread = new HandlerThread("H264DecoderHandler");
            callbackThread.Start();
            Handler handler = new Handler(callbackThread.Looper);

            mDecoder = MediaCodec.CreateDecoderByType(MIME);
            mCallbackObj = callback_obj;
            mDecoder.SetCallback(new MyCallback(mDecoder, mCallbackObj), handler);

            //mOutputFormat = mDecoder.GetOutputFormat(); // option B
            inputFormat = MediaFormat.CreateVideoFormat(MIME, 540, 960);
            inputFormat.SetInteger(MediaFormat.KeyMaxInputSize, 540 * 960);
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