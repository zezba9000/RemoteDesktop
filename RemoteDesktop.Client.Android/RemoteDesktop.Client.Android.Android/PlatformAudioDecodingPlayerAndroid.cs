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
using System.Collections.Generic;
using System.Diagnostics;

[assembly: Dependency(typeof(PlatformAudioDecodingPlayerAndroid))]

namespace RemoteDesktop.Client.Android.Droid
{

    public delegate void DecodedPCMHandler(byte[] decoded_data);

    public class OggOpusLiveStreamingMediaDataSource : MediaDataSource
    {
        private AudioDecodingPlayerCallback cbk;
        private ByteFifo bfifo = new ByteFifo();
        private long currentPosition = 0;
        private long allPassedDataBytes = 0;
        private long allReadCount = 0;
        private long DEBUG_CONTENTS_LENGTH = 1024 * 30;
        

        public OggOpusLiveStreamingMediaDataSource(AudioDecodingPlayerCallback cbk)
        {
            this.cbk = cbk;
        }

        public override long Size
        {
            get {
                Console.WriteLine("OggOpusLiveStreamingMediaDataSource::Size");
                //return long.MaxValue;
                //return bfifo.Count;
                //return allPassedDataBytes;
                //return -1; // size is unknown
                return DEBUG_CONTENTS_LENGTH;
            }
        }

        public override void Close()
        {
            //throw new NotImplementedException();
        }

        public override int ReadAt(long position, byte[] buffer, int offset, int size)
        {
            Console.WriteLine("OggOpusLiveStreamingMediaDataSource::ReadAt called position = {0}, offset = {1}, size = {2}, pastReadCount = {3}", position, offset, size, allReadCount);
            allReadCount++;
            //if (allReadCount == 20)
            //{
            //    return -1; // エンコーダの初期化のためにいったん終わらせる
            //}
            if (position + size > DEBUG_CONTENTS_LENGTH) //コンテンツの終わりに見せる
            {
                return -1;
            }

            var currentHaveFirst = bfifo.Count;
            byte[] ret_buf;
            if(currentHaveFirst >= size)
            {
                ret_buf = bfifo.Pop(size);
                currentPosition += size;
                Array.Copy(ret_buf, 0, buffer, offset, size);
                return size;
            }

            byte[] encoded_data = null;
            Console.WriteLine("OggOpusLiveStreamingMediaDataSource::ReadAt before while loop.");
            while ((encoded_data = cbk.getEncodedSamplesData()) == null)
            {
                Thread.Sleep(1);
            }
            Console.WriteLine("OggOpusLiveStreamingMediaDataSource::ReadAt after while loop.");
            Console.Out.Flush();
            allPassedDataBytes += encoded_data.Length;
            bfifo.Push(encoded_data);
            var currentHaveAfterRead = bfifo.Count;
            var ret_size = currentHaveAfterRead >= size ? size : currentHaveAfterRead;
            ret_buf = bfifo.Pop(ret_size);
            currentPosition += ret_size;
            Array.Copy(ret_buf, 0, buffer, offset, ret_size);
            return ret_size;
        }
    }


    public class AudioDecoderCallback : MediaCodec.Callback
    {
        MediaCodec mDecoder;
        MediaFormat mOutputFormat;
        AudioDecodingPlayerCallback mCallbackObj;
        PlatformAudioDecodingPlayerAndroid mADP;
        MediaExtractor mExtractor;
        int frameCounter = 0;
        public byte[] CSD0;

        //private Queue<DateTime> debug_time_Q = new Queue<DateTime>();
        private int total_decode_msec = 0;
        private int decoded_frame_conter = 0;

        public event DecodedBitmapHandler encodedDataGenerated;

        public AudioDecoderCallback(MediaCodec decoder, AudioDecodingPlayerCallback callback_obj, PlatformAudioDecodingPlayerAndroid parent, MediaExtractor extractor)
        {
            mCallbackObj = callback_obj;
            mDecoder = decoder;
            mADP = parent;
            mExtractor = extractor;
        }
        public override void OnError(MediaCodec codec, CodecException e)
        {
            Console.WriteLine(e);
        }

        private void OnInputBufferAvailableInner(MediaCodec mc, int inputBufferId)
        {
            byte[] encoded_data = null;
            int sampleSize = -1;
            if(mExtractor != null) // ogg_opus
            {
                ByteBuffer inputBuffer = mDecoder.GetInputBuffer(inputBufferId);
                inputBuffer.Position(0);
                sampleSize = mExtractor.ReadSampleData(inputBuffer, 0);
                if (sampleSize < 0)
                {
                    // We shouldn't stop the playback at this point, just pass the EOS
                    // flag to decoder, we will get it again from the
                    // dequeueOutputBuffer
                    Console.WriteLine("InputBuffer BUFFER_FLAG_END_OF_STREAM");
                    mDecoder.QueueInputBuffer(inputBufferId, 0, 0, 0, MediaCodec.BufferFlagEndOfStream);
                }
                else
                {
                    Console.WriteLine("added encoded data got from MediaExtractor " + sampleSize.ToString() + "bytes");
                    mDecoder.QueueInputBuffer(inputBufferId, 0, sampleSize, mExtractor.SampleTime, 0);
                    mExtractor.Advance();
                    frameCounter++;
                }
            }
            else
            {
                while ((encoded_data = mCallbackObj.getEncodedSamplesData()) == null)
                {
                    Thread.Sleep(1);
                }

                Console.WriteLine("OnInputBufferAvailable: got encoded data!");

                if (encoded_data != null)
                {
                    sampleSize = encoded_data.Length;
                    if (sampleSize > 0)
                    {
                        //debug_time_Q.Enqueue(DateTime.Now);

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
            decoded_frame_conter++;

            // bufferFormat is equivalent to mOutputFormat
            // outputBuffer is ready to be processed or rendered.

            //if(debug_time_Q.Count > 0)
            //{
            //    decoded_frame_conter++;
            //    DateTime now = DateTime.Now;
            //    TimeSpan ts = DateTime.Now - debug_time_Q.Dequeue();
            //    total_decode_msec += ts.Milliseconds;
            //    Console.WriteLine(now.ToString("yyyy/MM/ dd hh: mm: ss.fff") + " DEBUG: current decode speed is " + ((total_decode_msec / 1000.0) / (float)decoded_frame_conter).ToString() + " fps. total decoded frame = " + decoded_frame_conter.ToString());
            //}
            //else
            //{
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/ dd hh: mm: ss.fff") + " OnOutputBufferAvailable: decoded_frame_counter = " + decoded_frame_conter.ToString());
            //}

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
        int samplesPerSecond;

        private AudioTrack audioTrack;
        private Stopwatch sw = new Stopwatch();
        private MediaFormat mMediaFormat;

        public void PlayData(byte[] data, bool flag)
        {
            if (sw.IsRunning)
            {
                sw.Stop();
                Console.WriteLine("elapsed " + sw.ElapsedMilliseconds.ToString() + " from before PlayData func call");
                sw.Reset();
            }
            Console.WriteLine(" " + DateTime.Now.ToString("yyyy/MM/ dd hh: mm: ss.fff") + " call PlayData: " + data.Length.ToString() + " bytes, " + ((float)data.Length / ((float)samplesPerSecond * 2.0)).ToString() + " sec at playing time");
            audioTrack.Write(data, 0, data.Length);
            sw.Start();
        }

        public bool OpenDevice(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        {
            this.samplesPerSecond = samplesPerSecond;
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
            samplesPerSecond,
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

        public bool setup(AudioDecodingPlayerCallback callback_obj, int samplingRate, int ch, int bitrate, byte[] csd_data, String codec)
        {
            OpenDevice("hoge", samplingRate, 16, ch, 128 * 1024);

            mCallbackObj = callback_obj;
            HandlerThread callbackThread = new HandlerThread("DecodingPlayerHandler");
            callbackThread.Start();
            Handler handler = new Handler(callbackThread.Looper);
            MediaExtractor extractor = null;

            if(codec == "aac")
            {
                mDecoder = MediaCodec.CreateDecoderByType("audio/mp4a-latm");
                mMediaFormat = MediaFormat.CreateAudioFormat("audio/mp4a-latm", samplingRate, ch);
                //byte[] bytes = new byte[] { (byte)0x12, (byte)0x12 };

                int profile = (csd_data[2] & 0xC0) >> 6;
                int srate = (csd_data[2] & 0x3C) >> 2;
                int channel = ((csd_data[2] & 0x01) << 2) | ((csd_data[3] & 0xC0) >> 6);
                sbyte csd0_0s = (sbyte)(((profile + 1) << 3) | srate >> 1);
                sbyte csd0_1s = (sbyte)(((srate << 7) & 0x80) | channel << 3 );
                byte csd0_0 = (byte)(((profile + 1) << 3) | srate >> 1);
                byte csd0_1 = (byte)(((srate << 7) & 0x80) | channel << 3 );
                byte[] bytes = new byte[] { csd0_0, csd0_1 };
                ByteBuffer csd0 = ByteBuffer.Wrap(bytes);
                mMediaFormat.SetInteger(MediaFormat.KeyIsAdts, 1);
                mMediaFormat.SetByteBuffer("csd-0", csd0);
            }
            else if(codec == "opus")
            {
                mDecoder = MediaCodec.CreateDecoderByType("audio/opus");
                mMediaFormat = MediaFormat.CreateAudioFormat("audio/opus", samplingRate, ch);
                ByteBuffer csd0 = ByteBuffer.Wrap(csd_data);
                mMediaFormat.SetByteBuffer("csd-0", csd0);
                //mMediaFormat.SetByteBuffer("csd-1", (ByteBuffer.Allocate(8).PutLong(0)));
                //mMediaFormat.SetByteBuffer("csd-2", (ByteBuffer.Allocate(8).PutLong(0)));
                //mMediaFormat.SetLong("csd-1", 0);
                //mMediaFormat.SetLong("csd-2", 0);
            }
            else if (codec == "ogg_opus")
            {
			    extractor = new MediaExtractor();
                //extractor.SetDataSourceAsync(new OggOpusLiveStreamingMediaDataSource(callback_obj)); // dame
                extractor.SetDataSource(new OggOpusLiveStreamingMediaDataSource(callback_obj));
                //extractor.SetDataSource("http://192.168.0.11/~ryo/hls/CapturedPCM_new_args.ogg");

                Console.WriteLine("after SetDataSource");

			    for (int ii = 0; ii < extractor.TrackCount; ii++) {
				    mMediaFormat = extractor.GetTrackFormat(ii);
				    String mime = mMediaFormat.GetString(MediaFormat.KeyMime);
                    Console.WriteLine("MediaFormat.KeyMime: " + mime);
				    if (mime.StartsWith("audio/")) {
					    extractor.SelectTrack(ii);
					    mDecoder = MediaCodec.CreateDecoderByType(mime);
					    //mDecoder.Configure(format, null, null, 0);
					    break;
				    }
			    }

			    if (mDecoder == null) {
				    Console.WriteLine("Can't find audio info!");
                    throw new Exception("Can't find audio info!");
			    }
            }
            else
            {
                throw new Exception("unsupported codec by RemoteDeskto Client app.");
            }

            var cbk = new AudioDecoderCallback(mDecoder, mCallbackObj, this, extractor);
            mDecoder.SetCallback(cbk, handler);
            mDecoder.Configure(mMediaFormat, null, null, 0);

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