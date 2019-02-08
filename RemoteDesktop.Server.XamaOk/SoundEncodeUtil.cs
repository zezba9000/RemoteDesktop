using FragLabs.Audio.Codecs;
using NAudio.MediaFoundation;
using NAudio.Wave;
using RemoteDesktop.Android.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktop.Server.XamaOK
{

    class SoundEncodeUtil
    {
        public static byte[] encodePCMtoMP3(IWaveProvider waveIn)
        {
            //  どこかの初期化処理でMediaFoundationのおまじないを書かないと
            MediaFoundationEncoder.EncodeToMp3(waveIn, "F:\\work\\tmp\\tmp.mp3");
            Console.WriteLine("before create MP3FileReader");
            Mp3FileReader reader = new Mp3FileReader("F:\\work\\tmp\\tmp.mp3");
            Console.WriteLine("after create MP3FileReader");
            MemoryStream ms = new MemoryStream();
            Mp3Frame ret;
            while((ret = reader.ReadNextFrame()) != null)
            {
                //Console.WriteLine("write frame data to MemoryStream size = " + ret.FrameLength);
                ms.Write(ret.RawData, 0, ret.FrameLength);
            }
            reader.Close();
            reader.Dispose();

            var ret_buf = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return ret_buf;
        }
    }

    // OpusWrapperの更なるラッパー。エンコーダーに関するステートを管理する。
    class OpusEncoderManager
    {
        private OpusEncoder mEncoder;
        private int _segmentFrames;
        private int _bytesPerSegment;
        private ulong _bytesSent;
        private byte[] _notEncodedBuffer = new byte[0];
        private AudioOutputWriter aout;

        public OpusEncoderManager(AudioOutputWriter aout, int sampleRate)
        {
            this.aout = aout;
            _bytesSent = 0;
            _segmentFrames = 960;
            mEncoder = OpusEncoder.Create(sampleRate, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
            mEncoder.Bitrate = 512 * 8; //8192;
            _bytesPerSegment = mEncoder.FrameByteCount(_segmentFrames);
        }


        public void addPCMSamples(byte[] pcm_data, int data_len)
        {
            //// エンディアンを反転
            //// 引数のデータを書き換えてしまう
            //EndianReverser.uint16_bytes_reverse(pcm_data);

            byte[] soundBuffer = new byte[data_len + _notEncodedBuffer.Length];
            for (int i = 0; i < _notEncodedBuffer.Length; i++)
                soundBuffer[i] = _notEncodedBuffer[i];
            for (int i = 0; i < data_len; i++)
                soundBuffer[i + _notEncodedBuffer.Length] = pcm_data[i];

            int byteCap = _bytesPerSegment;
            int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
            int segmentsEnd = segmentCount * byteCap;
            int notEncodedCount = soundBuffer.Length - segmentsEnd;
            _notEncodedBuffer = new byte[notEncodedCount];
            for (int i = 0; i < notEncodedCount; i++)
            {
                _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
            }

            if(segmentCount == 0)
            {
                return;
            }

            for (int i = 0; i < segmentCount; i++)
            {
                byte[] segment = new byte[byteCap];
                for (int j = 0; j < segment.Length; j++)
                    segment[j] = soundBuffer[(i*byteCap) + j];
                int len;
                byte[] buf = mEncoder.Encode(segment, segment.Length, out len);

                aout.handleDataWithTCP(buf);

                _bytesSent += (ulong)len;
            }
        }

        public void Dispose()
        {
            mEncoder.Dispose();
        }
    }
}
