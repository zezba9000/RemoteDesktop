using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteDesktop.Android.Core
{
		public class RTPConfiguration
		{
			/// <summary>
			/// Config
			/// </summary>
			public RTPConfiguration()
			{

			}

        	public enum ProtcolMode
            {
                UDP,
                TCP
            }

        //Attribute
            public static int bmpFileHeaderBytes = 54;
            public static String ServerAddress = "192.168.0.11";
            //public static String ServerAddress = "192.168.137.1";
            public String SoundDeviceName = "";
			public int SoundServerPort = 10000;
            public static int ImageServerPort = 8889;

            public static bool isRunCapturedSoundDataHndlingWithoutConn = true;
            //public static int SamplesPerSecond = 8000;
            public static bool isCheckAdtsFrameNum = true;
            //public static int SamplesPerSecond = 44100;
            //public static int SamplesPerSecond = 24000;
            public static int SamplesPerSecond = 48000; // sound card native (for opus test)
            public short BitsPerSample = 16;
            //public short BitsPerSample = 16;
            //public short BitsPerSample = 32;  // sound card native
            public short Channels = 1;
			public Int32 PacketSize = 4096; //使われていない
			public Int32 BufferCount = 1024 * 128; // AndroidのAudioTrackに指定するバッファ長
			public uint JitterBuffer = 20; // max buffering num of RTPPacket at jitter buffer
            public uint JitterBufferTimerPeriodMsec = 20; // time period of jitter buffer (msec)
            public bool UseJitterBuffer = true;
            public bool isAlreadySetInfoFromSndCard = false;
            public ProtcolMode protcol_mode = ProtcolMode.TCP;
            public bool compress = false;
            public bool isConvertMulaw = false;
            public static bool isUseDPCM = false; // flag for server and client
            public static bool isEncodeWithDpcmOrUseRawPCM = false; // flag for server
            public static bool isEncodeWithAAC = false; // flag for server
            public static bool isEncodeWithOpus = false;
            public static bool isEncodeWithOggOpus = true;
            public static bool isUseOggfilePkg = true;
            public static int caputuedPcmBufferSamples = 0; //128; // AACの adtsフォーマットだったら 1024 * N (100とか) にする
            public static int h246EncoderBitPerSec = 5 * 1024 * 8;
            //public static float h264EncoderFrameRate = 1.0f;
            public static float h264EncoderKeyframeInterval = 60.0f;
            public static bool isStdOutOff = false;
            public static bool isUseFFMPEG = false;
            public static int ffmpegStdoutFirstSendBytes = 512; //1024 * 8; //1024 * 2; //最初はためて送ってみる
            public static bool isUseLossySoundDecoder = true;
            public static int encoderBps = 8 * 1024;

            // for 流用元コード. Xamarin対応版では利用されない
            public String FileName = "";
            public String localAddress = "";
            public int localPort = 0;
            public bool Loop = false;

            public static bool isStreamRawH264Data = true;
            public static bool isConvJpeg = false; // 今は関係ない
            public static int jpegEncodeQuality = 50;
            public static int initialSkipCaptureNums = 0;

            // FormMainとかにあったフィールド
            //private uint m_RecorderFactor = 4;
            //private uint m_JitterBufferCount = 20;
            public long SequenceNumber = 4596;
            public long TimeStamp = 0;
            //private int m_Version = 2;
            //private bool m_Padding = false;
            //private bool m_Extension = false;
            //private int m_CSRCCount = 0;
            //private bool m_Marker = false;
            //private int m_PayloadType = 0;
            //private uint m_SourceId = 0;

            // from RTPPacket
            //public int HeaderLength = RTPPacket.MinHeaderLength;
            //public int Version = 0;
            //public bool Padding = false;
            //public bool Extension = false;
            //public int CSRCCount = 0;
            //public bool Marker = false;
            //public int PayloadType = 0;
            //public UInt16 SequenceNumber = 0;
            //public uint Timestamp = 0;
            //public uint SourceId = 0;
            //public Byte[] Data;
            //public UInt16 ExtensionHeaderId = 0;
            //public UInt16 ExtensionLengthAsCount = 0;
            //public Int32 ExtensionLengthInBytes = 0;
        }
}

