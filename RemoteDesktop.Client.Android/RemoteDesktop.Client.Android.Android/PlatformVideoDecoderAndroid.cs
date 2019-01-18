using Android.Media;
using RemoteDesktop.Client.Android;
using RemoteDesktop.Client.Android.Droid;
using System;
using System.Threading;
using Xamarin.Forms;

//using java.io.IOException;
//using java.nio.ByteBuffer;

//using Android.Media.MediaCodec.BufferInfo;
//using Android.Media.MediaExtractor;
//using Android.Media.MediaFormat;

using Android.Views;
using Android.Graphics;
using Java.Nio;
using static Android.Media.MediaCodec;
using System.IO;

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

    public class MyCallback : MediaCodec.Callback
    {
        MediaCodec mDecoder;
        MediaExtractor mExtractor;
        MediaFormat mOutputFormat;
        DecoderCallback mCallbackObj;

        public MyCallback(MediaCodec decoder, MediaExtractor extractor, DecoderCallback callback_obj)
        {
            mCallbackObj = callback_obj;
            mDecoder = decoder;
            mExtractor = extractor;
        }
        public override void OnError(MediaCodec codec, CodecException e)
        {
            Console.WriteLine(e);
        }

        override public void OnInputBufferAvailable(MediaCodec mc, int inputBufferId)
        {
            ByteBuffer inputBuffer = mDecoder.GetInputBuffer(inputBufferId);

            int sampleSize = mExtractor.ReadSampleData(inputBuffer, 0);

            if (mExtractor.Advance() && sampleSize > 0)
            {
                Console.WriteLine("QueueInputBuffer inputIndex=" + inputBufferId.ToString());
                mDecoder.QueueInputBuffer(inputBufferId, 0, sampleSize, mExtractor.SampleTime, 0);
            }
            else
            {
                Console.WriteLine("QueueInputBuffer set MediaCodec.BufferFlagEndOfStream");
                mDecoder.QueueInputBuffer(inputBufferId, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
                //isInput = false;
            }
        }

        override public void OnOutputBufferAvailable(MediaCodec mc, int outputBufferId, BufferInfo info)
        {
            ByteBuffer outputBuffer = mDecoder.GetOutputBuffer(outputBufferId);
            MediaFormat bufferFormat = mDecoder.GetOutputFormat(outputBufferId); // option A

            // bufferFormat is equivalent to mOutputFormat
            // outputBuffer is ready to be processed or rendered.

            Console.WriteLine("OnOutputBufferAvailable: outputBufferId = " + outputBufferId.ToString());
            mCallbackObj.OnDecodeFrame(null);
            //ここでコールバックを呼ぶ

            mDecoder.ReleaseOutputBuffer(outputBufferId, false);
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
        private MediaExtractor mExtractor;
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
            mCallbackObj = callback_obj;
            mExtractor = new MediaExtractor();
            //mExtractor.SetDataSource(new AviFileContentDataSource(format_hint));
            //mExtractor.SetDataSource("http://192.168.0.11:8890/rdp.mp4");
            mExtractor.SetDataSource("http://192.168.0.11/~ryo/hls/genFromBMPFilesWithFFMPEG.mp4");


            for (int i = 0; i < mExtractor.TrackCount; i++)
            {
                inputFormat = mExtractor.GetTrackFormat(i);

                String mime = inputFormat.GetString(MediaFormat.KeyMime);
                if (mime.StartsWith(VIDEO))
                {
                    mExtractor.SelectTrack(i);
                    mDecoder = MediaCodec.CreateDecoderByType(mime);
                    break;
                }
            }

             mDecoder.SetCallback(new MyCallback(mDecoder, mExtractor, mCallbackObj));

             //mOutputFormat = mDecoder.GetOutputFormat(); // option B

            try
            {
                //mDecoder.Configure(format, surface, null, 0 /* Decoder */);
                mDecoder.Configure(inputFormat, null, null, 0 /* Decoder */);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            Console.WriteLine("before mDecoder.Start()");
            mDecoder.Start();
            Console.WriteLine("after mDecoder.Start()");

            //Console.WriteLine("finalize decoder and extractor.");
            //mDecoder.Stop();
            //mDecoder.Release();
            //mExtractor.Release();

            return true;
        }

        //public bool setup(byte[] format_hint) //format_hint is aviFileContent 
        //{
        //    if(format_hint != null)
        //    {
        //        try
        //        {
        //            mExtractor = new MediaExtractor();
        //            //mExtractor.SetDataSource(new AviFileContentDataSource(format_hint));
        //            //mExtractor.SetDataSource("http://192.168.0.11/~ryo/hls/genFromBMPFilesWithFFMPEG.mp4");
        //            mExtractor.SetDataSource("http://192.168.0.11:8890/rdp.mp4");

        //            for (int i = 0; i < mExtractor.TrackCount; i++)
        //            {
        //                MediaFormat format = mExtractor.GetTrackFormat(i);

        //                String mime = format.GetString(MediaFormat.KeyMime);
        //                if (mime.StartsWith(VIDEO))
        //                {
        //                    mExtractor.SelectTrack(i);
        //                    mDecoder = MediaCodec.CreateDecoderByType(mime);
        //                    try
        //                    {
        //                        //mDecoder.Configure(format, surface, null, 0 /* Decoder */);
        //                        mDecoder.Configure(format, null, null, 0 /* Decoder */);

        //                    }
        //                    catch(Exception ex)
        //                    {
        //                        throw ex;
        //                    }

        //                    Console.WriteLine("before mDecoder.Start()");
        //                    mDecoder.Start();
        //                    Console.WriteLine("after mDecoder.Start()");
        //                    break;
        //                }
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }

        //        BufferInfo info = new BufferInfo();
        //        ByteBuffer[] inputBuffers = mDecoder.GetInputBuffers();
        //        mDecoder.GetOutputBuffers();

        //        bool isInput = true;
        //        bool first = false;
        //        long startWhen = 0;

        //        while (!eosReceived)
        //        {
        //            if (isInput)
        //            {
        //                int inputIndex = mDecoder.DequeueInputBuffer(10000);
        //                if (inputIndex >= 0)
        //                {
        //                    // fill inputBuffers[inputBufferIndex] with valid data
        //                    ByteBuffer inputBuffer = inputBuffers[inputIndex];

        //                    int sampleSize = mExtractor.ReadSampleData(inputBuffer, 0);

        //                    if (mExtractor.Advance() && sampleSize > 0)
        //                    {
        //                        Console.WriteLine("QueueInputBuffer inputIndex=" + inputIndex.ToString());
        //                        mDecoder.QueueInputBuffer(inputIndex, 0, sampleSize, mExtractor.SampleTime, 0);

        //                    }
        //                    else
        //                    {
        //                        mDecoder.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
        //                        isInput = false;
        //                    }
        //                }
        //            }

        //            int outIndex = mDecoder.DequeueOutputBuffer(info, 10000);
        //            Console.WriteLine("DequeueOutputBuffer outputIndex=" + outIndex.ToString());
        //            switch (outIndex)
        //            {
        //                case (int)MediaCodec.InfoOutputBuffersChanged:
        //                    Console.WriteLine("OutputBufferChanged");
        //                    var decoded_data = mDecoder.GetOutputBuffers();
        //                    Console.WriteLine("after GetOutputBuffers length=" + decoded_data.Length.ToString());
        //                    break;

        //                case (int)MediaCodec.InfoOutputFormatChanged:
        //                    Console.WriteLine("OutputFormatChanged");
        //                    break;

        //                case (int)MediaCodec.InfoTryAgainLater:
        //                    Console.WriteLine("TryAgainLater");
        //                    break;

        //                default:
        //                    if (!first)
        //                    {
        //                        startWhen = CurrentTimeMillisSharp();
        //                        first = true;
        //                    }
        //                    try
        //                    {
        //                        int sleepTime = (int)((info.PresentationTimeUs / 1000) - (CurrentTimeMillisSharp() - startWhen));

        //                        if (sleepTime > 0)
        //                        {
        //                            Thread.Sleep(sleepTime);
        //                        }

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine(ex);
        //                    }

        //                    mDecoder.ReleaseOutputBuffer(outIndex, true /* Surface init */);
        //                    break;
        //            }

        //            // All decoded frames have been rendered, we can stop playing now
        //            if ((info.Flags & MediaCodec.BufferFlagEndOfStream) != 0)
        //            {
        //                Console.WriteLine("all input decoded!");
        //                break;
        //            }
        //        }

        //        Console.WriteLine("finalize decoder and extractor.");
        //        mDecoder.Stop();
        //        mDecoder.Release();
        //        mExtractor.Release();
        //    }
        //    else
        //    {
        //        eosReceived = false;
        //        try
        //        {
        //            //mExtractor = new MediaExtractor();
        //            //mExtractor.setDataSource(filePath);

        //            //for (int i = 0; i < mExtractor.getTrackCount(); i++)
        //            //{
        //            //    MediaFormat format = mExtractor.getTrackFormat(i);

        //            //    String mime = format.getString(MediaFormat.KEY_MIME);
        //            //    if (mime.startsWith(VIDEO))
        //            //    {
        //            //        mExtractor.selectTrack(i);
        //            mDecoder = MediaCodec.CreateDecoderByType(MIME);
        //            try
        //            {
        //                //mDecoder.configure(format, surface, null, 0 /* Decoder */);
        //                MediaFormat format = new MediaFormat();
        //                format.SetString(MediaFormat.KeyMime, MIME);
        //                mDecoder.Configure(format, new Surface(new SurfaceTexture(true)), null, 0);

        //            }
        //            catch (Exception ex)
        //            {
        //                return false;
        //            }

        //            mDecoder.Start();
        //            //        break;
        //            //    }
        //            //}

        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }

        //        info = new MediaCodec.BufferInfo();
        //        inputBuffers = mDecoder.GetInputBuffers();
        //        mDecoder.GetOutputBuffers();
        //    }

        //    return true;
        //}

        //// if eosReceived == false, return false
        //public bool checkStatus()
        //{

        //    //ByteBuffer[] inputBuffers = mDecoder.getInputBuffers();
        //    //mDecoder.getOutputBuffers();

        //    //boolean isInput = true;
        //    //boolean first = false;
        //    //long startWhen = 0;

        //    if (isInput)
        //    {
        //        inputIndex = mDecoder.DequeueInputBuffer(10000);
        //        if (inputIndex >= 0)
        //        {
        //            // fill inputBuffers[inputBufferIndex] with valid data
        //            Java.Nio.ByteBuffer inputBuffer = inputBuffers[inputIndex];

        //            int sampleSize = mExtractor.ReadSampleData(inputBuffer, 0);

        //            if (mExtractor.Advance() && sampleSize > 0)
        //            {
        //                mDecoder.QueueInputBuffer(inputIndex, 0, sampleSize, mExtractor.getSampleTime(), 0);

        //            }
        //            else
        //            {
        //                mDecoder.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
        //                isInput = false;
        //            }
        //        }
        //    }

        //    int outIndex = mDecoder.DequeueOutputBuffer(info, 10000);
        //    switch (outIndex)
        //    {
        //        case (int)MediaCodec.InfoOutputBuffersChanged:
        //            mDecoder.GetOutputBuffers();
        //            break;

        //        case (int)MediaCodec.InfoOutputFormatChanged:
        //            break;

        //        case (int)MediaCodec.InfoTryAgainLater:
        //            //				Log.d(TAG, "INFO_TRY_AGAIN_LATER");
        //            break;

        //        default:
        //            if (!first)
        //            {
        //                startWhen = CurrentTimeMillisSharp();
        //                first = true;
        //            }
        //            try
        //            {
        //                int sleepTime = (int)((info.PresentationTimeUs / 1000) - (CurrentTimeMillisSharp() - startWhen);

        //                if (sleepTime > 0)
        //                    Thread.Sleep(sleepTime);
        //            }
        //            catch (Exception ex)
        //            {
        //                // TODO Auto-generated catch block
        //                Console.WriteLine(ex);
        //            }

        //            mDecoder.ReleaseOutputBuffer(outIndex, true /* Surface init */);
        //            //break;
        //            return false;
        //    }

        //    // All decoded frames have been rendered, we can stop playing now
        //    if ((info.Flags & MediaCodec.BufferFlagEndOfStream) != 0)
        //    {
        //        //break;
        //        return false;
        //    }

        //    return eosReceived == true;
        //}

        // if eosReceived == false, return false
        public void addEncodedFrame(byte[] frame_data)
        {
            int inputIndex = mDecoder.DequeueInputBuffer(10000);
            Java.Nio.ByteBuffer inputBuffer = inputBuffers[inputIndex];
            sbyte[] sbyte_frame = ((frame_data as Array) as sbyte[]);
            foreach (sbyte sb in sbyte_frame)
            {
                inputBuffer.Put(sb);
            }
            mDecoder.QueueInputBuffer(inputIndex, 0, frame_data.Length, 1000 /* 1sec */, 0);
            //mDecoder.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
        }

        public byte[] getDecodedFrame()
        {
            int outIndex = mDecoder.DequeueOutputBuffer(info, 10000);
            while (true)
            {
                switch (outIndex)
                {
                    case (int)MediaCodec.InfoOutputBuffersChanged:
                        Console.WriteLine("OutputBufferChanged");
                        return mDecoder.GetOutputBuffer(outIndex).ToArray<byte>();
                    //break;

                    case (int)MediaCodec.InfoOutputFormatChanged:
                        Console.WriteLine("OutputFormatChanged");
                        break;

                    case (int)MediaCodec.InfoTryAgainLater:
                        Console.WriteLine("TryAgainLater");
                        break;

                    default:
                        if (!first)
                        {
                            startWhen = CurrentTimeMillisSharp();
                            first = true;
                        }
                        try
                        {
                            int sleepTime = (int)((info.PresentationTimeUs / 1000) - (CurrentTimeMillisSharp() - startWhen));

                            if (sleepTime > 0)
                            {
                                Thread.Sleep(sleepTime);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        mDecoder.ReleaseOutputBuffer(outIndex, true /* Surface init */);
                        break;
                        //return false;
                }
            }

            //// All decoded frames have been rendered, we can stop playing now
            //if ((info.Flags & MediaCodec.BufferFlagEndOfStream) != 0)
            //{
            //    //break;
            //    return false;
            //}

            //return eosReceived == true;
        }

        public void Close()
        {
            mDecoder.Stop();
            mDecoder.Release();
            mExtractor.Release();
            eosReceived = true;
        }
    }
}