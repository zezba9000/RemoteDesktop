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

//[assembly: Dependency(typeof(PlatformSoundPlayerAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{
    public class VideoDecoder// : IPlatformSoundPlayer
    {
        private static String MIME = "video/avc";
        private static String TAG = "VideoDecoder";
        //	    private MediaExtractor mExtractor;
        private MediaCodec mDecoder;

        private bool eosReceived;
        private Java.Nio.ByteBuffer[] inputBuffers = null;


        private bool isInput = true;
        private bool first = false;
        private long startWhen = 0;
        private MediaCodec.BufferInfo info = null;
        int inputIndex = -1;

        private long CurrentTimeMillisSharp()
        {
            return (long)(new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds);
        }

        //public boolean init(Surface surface, String filePath)
        public bool setup()
        {
            eosReceived = false;
            try
            {
                //mExtractor = new MediaExtractor();
                //mExtractor.setDataSource(filePath);

                //for (int i = 0; i < mExtractor.getTrackCount(); i++)
                //{
                //    MediaFormat format = mExtractor.getTrackFormat(i);

                //    String mime = format.getString(MediaFormat.KEY_MIME);
                //    if (mime.startsWith(VIDEO))
                //    {
                //        mExtractor.selectTrack(i);
                mDecoder = MediaCodec.CreateDecoderByType(MIME);
                try
                {
                    //mDecoder.configure(format, surface, null, 0 /* Decoder */);
                    MediaFormat format = new MediaFormat();
                    format.SetString(MediaFormat.KeyMime, MIME);
                    mDecoder.Configure(format, new Surface(new SurfaceTexture(true)), null, 0);

                }
                catch (Exception ex)
                {
                    return false;
                }

                mDecoder.Start();
                //        break;
                //    }
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            info = new MediaCodec.BufferInfo();
            inputBuffers = mDecoder.GetInputBuffers();
            mDecoder.GetOutputBuffers();

            return true;
        }

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
        public bool addEncodedFrame()
        {

            //ByteBuffer[] inputBuffers = mDecoder.getInputBuffers();
            //mDecoder.getOutputBuffers();

            //boolean isInput = true;
            //boolean first = false;
            //long startWhen = 0;

            if (isInput)
            {
                inputIndex = mDecoder.DequeueInputBuffer(10000);
                if (inputIndex >= 0)
                {
                    // fill inputBuffers[inputBufferIndex] with valid data
                    Java.Nio.ByteBuffer inputBuffer = inputBuffers[inputIndex];

                    int sampleSize = mExtractor.ReadSampleData(inputBuffer, 0);

                    if (mExtractor.Advance() && sampleSize > 0)
                    {
                        mDecoder.QueueInputBuffer(inputIndex, 0, sampleSize, mExtractor.getSampleTime(), 0);

                    }
                    else
                    {
                        mDecoder.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
                        isInput = false;
                    }
                }
            }
        }

        public void getDecodedFrame() { 
            int outIndex = mDecoder.DequeueOutputBuffer(info, 10000);
            switch (outIndex)
            {
                case (int)MediaCodec.InfoOutputBuffersChanged:
                    mDecoder.GetOutputBuffers();
                    break;

                case (int)MediaCodec.InfoOutputFormatChanged:
                    break;

                case (int)MediaCodec.InfoTryAgainLater:
                    //				Log.d(TAG, "INFO_TRY_AGAIN_LATER");
                    break;

                default:
                    if (!first)
                    {
                        startWhen = CurrentTimeMillisSharp();
                        first = true;
                    }
                    try
                    {
                        int sleepTime = (int)((info.PresentationTimeUs / 1000) - (CurrentTimeMillisSharp() - startWhen);

                        if (sleepTime > 0)
                            Thread.Sleep(sleepTime);
                    }
                    catch (Exception ex)
                    {
                        // TODO Auto-generated catch block
                        Console.WriteLine(ex);
                    }

                    mDecoder.ReleaseOutputBuffer(outIndex, true /* Surface init */);
                    //break;
                    return false;
            }

            // All decoded frames have been rendered, we can stop playing now
            if ((info.Flags & MediaCodec.BufferFlagEndOfStream) != 0)
            {
                //break;
                return false;
            }

            return eosReceived == true;
        }

        public void close()
        {
            mDecoder.Stop();
            mDecoder.Release();
            //            mExtractor.Release();
            eosReceived = true;
        }
    }
}