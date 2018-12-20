using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using RemoteDesktop.Android.Core;

namespace RemoteDesktop.Client.Android
{

    public class SoundManager
    {

        public class Player
        {
            public bool Opened = false;
            IPlatformSoundPlayer pplayer;

            public Player()
            {
                pplayer = SoundPlayerFactory.getInstance();
            }

            public void PlayData(byte[] data, bool flag)
            {
                pplayer.PlayData(data, flag);
            }

            public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
            {
                pplayer.Open(waveOutDeviceName, samplesPerSecond, bitsPerSample, channels, bufferCount);
                Opened = true;
                return true;
            }

            public void Close()
            {
                pplayer.Close();
                Opened = false;
            }
        }

        //public void Play() { }

        public class Stopwatch
        {
        }

        public class JitterBuffer
        {
            public delegate void DelegateDataAvailable(Object sender, RTPPacket packet);
            public event DelegateDataAvailable DataAvailable;

            public JitterBuffer(Object sender, uint maxRTPPackets, uint timerIntervalInMilliseconds)
            {
            }

            public void AddData(RTPPacket rtp)
            {

            }

            public void Start() { }

            public void Stop() { }
        }



        //public class Utils
        //{
        //    const int SIGN_BIT = (0x80);
        //    const int QUANT_MASK = (0xf);
        //    const int NSEGS = (8);
        //    const int SEG_SHIFT = (4);
        //    const int SEG_MASK = (0x70);
        //    const int BIAS = (0x84);
        //    const int CLIP = 8159;
        //    static short[] seg_uend = new short[] { 0x3F, 0x7F, 0xFF, 0x1FF, 0x3FF, 0x7FF, 0xFFF, 0x1FFF };

        //    public static Int32 MulawToLinear(Int32 ulaw)
        //    {
        //        ulaw = ~ulaw;
        //        int t = ((ulaw & QUANT_MASK) << 3) + BIAS;
        //        t <<= (ulaw & SEG_MASK) >> SEG_SHIFT;
        //        return ((ulaw & SIGN_BIT) > 0 ? (BIAS - t) : (t - BIAS));
        //    }

        //    public static Byte[] MuLawToLinear(Byte[] bytes, int bitsPerSample, int channels)
        //    {
        //        //Anzahl Spuren
        //        int blockAlign = channels * bitsPerSample / 8;

        //        //Für jeden Wert
        //        Byte[] result = new Byte[bytes.Length * blockAlign];
        //        for (int i = 0, counter = 0; i < bytes.Length; i++, counter += blockAlign)
        //        {
        //            //In Bytes umwandeln
        //            int value = MulawToLinear(bytes[i]);
        //            Byte[] values = BitConverter.GetBytes(value);

        //            switch (bitsPerSample)
        //            {
        //                case 8:
        //                    switch (channels)
        //                    {
        //                        //8 Bit 1 Channel
        //                        case 1:
        //                            result[counter] = values[0];
        //                            break;

        //                        //8 Bit 2 Channel
        //                        case 2:
        //                            result[counter] = values[0];
        //                            result[counter + 1] = values[0];
        //                            break;
        //                    }
        //                    break;

        //                case 16:
        //                    switch (channels)
        //                    {
        //                        //16 Bit 1 Channel
        //                        case 1:
        //                            result[counter] = values[0];
        //                            result[counter + 1] = values[1];
        //                            break;

        //                        //16 Bit 2 Channels
        //                        case 2:
        //                            result[counter] = values[0];
        //                            result[counter + 1] = values[1];
        //                            result[counter + 2] = values[0];
        //                            result[counter + 3] = values[1];
        //                            break;
        //                    }
        //                    break;
        //            }
        //        }

        //        //Fertig
        //        return result;
        //    }
        //}

        //    public class RTPPacket
        //    {
        //        /// <summary>
        //        /// Konstruktor
        //        /// </summary>
        //        public RTPPacket()
        //        {

        //        }
        //        /// <summary>
        //        /// Konstuktor
        //        /// </summary>
        //        /// <param name="_data"></param>
        //        public RTPPacket(byte[] data)
        //        {
        //            Parse(data);
        //        }

        //        //Attribute
        //        public static int MinHeaderLength = 12;
        //        public int HeaderLength = MinHeaderLength;
        //        public int Version = 0;
        //        public bool Padding = false;
        //        public bool Extension = false;
        //        public int CSRCCount = 0;
        //        public bool Marker = false;
        //        public int PayloadType = 0;
        //        public UInt16 SequenceNumber = 0;
        //        public uint Timestamp = 0;
        //        public uint SourceId = 0;
        //        public Byte[] Data;
        //        public UInt16 ExtensionHeaderId = 0;
        //        public UInt16 ExtensionLengthAsCount = 0;
        //        public Int32 ExtensionLengthInBytes = 0;

        //        /// <summary>
        //        /// Parse
        //        /// </summary>
        //        /// <param name="linearData"></param>
        //        private void Parse(Byte[] data)
        //        {
        //            if (data.Length >= MinHeaderLength)
        //            {
        //                Version = ValueFromByte(data[0], 6, 2);
        //                Padding = Convert.ToBoolean(ValueFromByte(data[0], 5, 1));
        //                Extension = Convert.ToBoolean(ValueFromByte(data[0], 4, 1));
        //                CSRCCount = ValueFromByte(data[0], 0, 4);
        //                Marker = Convert.ToBoolean(ValueFromByte(data[1], 7, 1));
        //                PayloadType = ValueFromByte(data[1], 0, 7);
        //                HeaderLength = MinHeaderLength + (CSRCCount * 4);

        //                //Sequence Nummer
        //                Byte[] seqNum = new Byte[2];
        //                seqNum[0] = data[3];
        //                seqNum[1] = data[2];
        //                SequenceNumber = System.BitConverter.ToUInt16(seqNum, 0);

        //                //TimeStamp
        //                Byte[] timeStmp = new Byte[4];
        //                timeStmp[0] = data[7];
        //                timeStmp[1] = data[6];
        //                timeStmp[2] = data[5];
        //                timeStmp[3] = data[4];
        //                Timestamp = System.BitConverter.ToUInt32(timeStmp, 0);

        //                //SourceId
        //                Byte[] srcId = new Byte[4];
        //                srcId[0] = data[8];
        //                srcId[1] = data[9];
        //                srcId[2] = data[10];
        //                srcId[3] = data[11];
        //                SourceId = System.BitConverter.ToUInt32(srcId, 0);

        //                //Wenn Extension Header
        //                if (Extension)
        //                {
        //                    //ExtensionHeaderId
        //                    Byte[] extHeaderId = new Byte[2];
        //                    extHeaderId[1] = data[HeaderLength + 0];
        //                    extHeaderId[0] = data[HeaderLength + 1];
        //                    ExtensionHeaderId = System.BitConverter.ToUInt16(extHeaderId, 0);

        //                    //ExtensionHeaderLength
        //                    Byte[] extHeaderLength16 = new Byte[2];
        //                    extHeaderLength16[1] = data[HeaderLength + 2];
        //                    extHeaderLength16[0] = data[HeaderLength + 3];
        //                    // TODO: need check
        //                    //ExtensionLengthAsCount = System.BitConverter.ToUInt16(extHeaderLength16.ToArray(), 0);
        //                    ExtensionLengthAsCount = System.BitConverter.ToUInt16(extHeaderLength16, 0);

        //                    //Header Länge anpassen (Länge mal 4 Bytes bzw. Int32)
        //                    ExtensionLengthInBytes = ExtensionLengthAsCount * 4;
        //                    HeaderLength += ExtensionLengthInBytes + 4;
        //                }

        //                //Daten kopieren
        //                Data = new Byte[data.Length - HeaderLength];
        //                Array.Copy(data, HeaderLength, this.Data, 0, data.Length - HeaderLength);
        //            }
        //        }
        //        /// <summary>
        //        /// GetValueFromByte
        //        /// </summary>
        //        /// <param name="value"></param>
        //        /// <param name="startPos"></param>
        //        /// <param name="length"></param>
        //        /// <returns></returns>
        //        private Int32 ValueFromByte(Byte value, int startPos, int length)
        //        {
        //            Byte mask = 0;
        //            //Maske erstellen
        //            for (int i = 0; i < length; i++)
        //            {
        //                mask = (Byte)(mask | 0x1 << startPos + i);
        //            }

        //            //Ergebnis
        //            Byte result = (Byte)((value & mask) >> startPos);
        //            //Fertig
        //            return Convert.ToInt32(result);
        //        }
        //        /// <summary>
        //        /// ToBytes
        //        /// </summary>
        //        /// <returns></returns>
        //        public Byte[] ToBytes()
        //        {
        //            //Ergebnis
        //            Byte[] bytes = new Byte[this.HeaderLength + Data.Length];

        //            //Byte 0
        //            bytes[0] = (Byte)(Version << 6);
        //            bytes[0] |= (Byte)(Convert.ToInt32(Padding) << 5);
        //            bytes[0] |= (Byte)(Convert.ToInt32(Extension) << 4);
        //            bytes[0] |= (Byte)(Convert.ToInt32(CSRCCount));

        //            //Byte 1
        //            bytes[1] = (Byte)(Convert.ToInt32(Marker) << 7);
        //            bytes[1] |= (Byte)(Convert.ToInt32(PayloadType));

        //            //Byte 2 + 3
        //            Byte[] bytesSequenceNumber = BitConverter.GetBytes(SequenceNumber);
        //            bytes[2] = bytesSequenceNumber[1];
        //            bytes[3] = bytesSequenceNumber[0];

        //            //Byte 4 bis 7
        //            Byte[] bytesTimeStamp = BitConverter.GetBytes(Timestamp);
        //            bytes[4] = bytesTimeStamp[3];
        //            bytes[5] = bytesTimeStamp[2];
        //            bytes[6] = bytesTimeStamp[1];
        //            bytes[7] = bytesTimeStamp[0];

        //            //Byte 8 bis 11
        //            Byte[] bytesSourceId = BitConverter.GetBytes(SourceId);
        //            bytes[8] = bytesSourceId[3];
        //            bytes[9] = bytesSourceId[2];
        //            bytes[10] = bytesSourceId[1];
        //            bytes[11] = bytesSourceId[0];

        //            //Daten
        //            Array.Copy(this.Data, 0, bytes, this.HeaderLength, this.Data.Length);

        //            //Fertig
        //            return bytes;
        //        }
        //    }
        //}
    }
}
