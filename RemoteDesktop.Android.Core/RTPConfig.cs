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
            public static String ServerAddress = "192.168.0.11";
			public String SoundDeviceName = "";
			public int ServerPort = 10000; //Sound Server
            public int SamplesPerSecond = 8000;
            //public int SamplesPerSecond = 48000; // sound card native
            public short BitsPerSample = 8;
            //public short BitsPerSample = 32;  // sound card native
			public short Channels = 1;
			public Int32 PacketSize = 4096; //使われていない
			public Int32 BufferCount = 1024 * 16; // AndroidのAudioTrackに指定するバッファ長
			public uint JitterBuffer = 20; // max buffering num of RTPPacket at jitter buffer
            public uint JitterBufferTimerPeriodMsec = 20; // time period of jitter buffer (msec)
            public bool UseJitterBuffer = true;
            public bool isAlreadySetInfoFromSndCard = false;
            public ProtcolMode protcol_mode = ProtcolMode.TCP;
            public bool compress = false;
            public bool isConvertMulaw = false;

            // for 流用元コード. Xamarin対応版では利用されない
            public String FileName = "";
            public String localAddress = "";
            public int localPort = 0;
            public bool Loop = false;

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

