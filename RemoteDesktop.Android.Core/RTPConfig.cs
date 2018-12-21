﻿using System;
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

			//Attribute
			public String ServerAddress = "192.168.0.11";
			public String SoundDeviceName = "";
			public int ServerPort = 10000; //Sound Server
			public int SamplesPerSecond = 48000;
			public short BitsPerSample = 32;
			public short Channels = 2;
			public Int32 PacketSize = 4096; //使われていない
			public Int32 BufferCount = 8;
			public uint JitterBuffer = 20;

            public bool isAlreadySetInfoFromSndCard = false;
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
