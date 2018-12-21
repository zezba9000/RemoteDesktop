using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteDesktop.Android.Core
{
        public class SoundUtils
        {
            const int SIGN_BIT = (0x80);
            const int QUANT_MASK = (0xf);
            const int NSEGS = (8);
            const int SEG_SHIFT = (4);
            const int SEG_MASK = (0x70);
            const int BIAS = (0x84);
            const int CLIP = 8159;
            static short[] seg_uend = new short[] { 0x3F, 0x7F, 0xFF, 0x1FF, 0x3FF, 0x7FF, 0xFFF, 0x1FFF };

            static short search(short val, short[] table, short size)
            {
                short i;
                int index = 0;
                for (i = 0; i < size; i++)
                {
                    if (val <= table[index])
                    {
                        return (i);
                    }
                    index++;
                }
                return (size);
            }


            public static Byte linear2ulaw(short pcm_val)
            {
                short mask = 0;
                short seg = 0;
                Byte uval = 0;

                /* Get the sign and the magnitude of the value. */
                pcm_val = (short)(pcm_val >> 2);
                if (pcm_val < 0)
                {
                    pcm_val = (short)-pcm_val;
                    mask = 0x7F;
                }
                else
                {
                    mask = 0xFF;
                }
                /* clip the magnitude */
                if (pcm_val > CLIP)
                {
                    pcm_val = CLIP;
                }
                pcm_val += (BIAS >> 2);

                /* Convert the scaled magnitude to segment number. */
                seg = search(pcm_val, seg_uend, (short)8);

                /*
                * Combine the sign, segment, quantization bits;
                * and complement the code word.
                */
                /* out of range, return maximum value. */
                if (seg >= 8)
                {
                    return (Byte)(0x7F ^ mask);
                }
                else
                {
                    uval = (Byte)((seg << 4) | ((pcm_val >> (seg + 1)) & 0xF));
                    return ((Byte)(uval ^ mask));
                }
            }

        //public static Byte[] ToRTPData(Byte[] data, RTPConfiguration config)
        //{
        //    //Neues RTP Packet erstellen
        //    RTPPacket rtp = ToRTPPacket(data, config);
        //    //RTPHeader in Bytes erstellen
        //    Byte[] rtpBytes = rtp.ToBytes();
        //    //Fertig
        //    return rtpBytes;
        //}

        public static Byte[] LinearToMulaw(Byte[] bytes, int bitsPerSample, int channels)
            {
                //Anzahl Spuren
                int blockAlign = channels * bitsPerSample / 8;

                //Ergebnis
                Byte[] result = new Byte[bytes.Length / blockAlign];
                int resultIndex = 0;
                for (int i = 0; i < result.Length; i++)
                {
                    //Je nach Auflösung
                    switch (bitsPerSample)
                    {
                        case 8:
                            switch (channels)
                            {
                                //8 Bit 1 Channel
                                case 1:
                                    result[i] = linear2ulaw(bytes[resultIndex]);
                                    resultIndex += 1;
                                    break;

                                //8 Bit 2 Channel
                                case 2:
                                    result[i] = linear2ulaw(bytes[resultIndex]);
                                    resultIndex += 2;
                                    break;
                            }
                            break;

                        case 16:
                            switch (channels)
                            {
                                //16 Bit 1 Channel
                                case 1:
                                    result[i] = linear2ulaw(BitConverter.ToInt16(bytes, resultIndex));
                                    resultIndex += 2;
                                    break;

                                //16 Bit 2 Channels
                                case 2:
                                    result[i] = linear2ulaw(BitConverter.ToInt16(bytes, resultIndex));
                                    resultIndex += 4;
                                    break;
                            }
                            break;
                    }
                }

                //Fertig
                return result;
            }

            public static RTPPacket ToRTPPacket(Byte[] linearData, RTPConfiguration config)
            {
                //Daten Nach MuLaw umwandeln
                //Byte[] mulaws = LinearToMulaw(linearData, config.BitsPerSample, config.Channels);
                Byte[] mulaws = linearData;

                //Neues RTP Packet erstellen
                RTPPacket rtp = new RTPPacket();

                //Werte übernehmen
                rtp.Data = mulaws;
                rtp.SourceId = 0;
                rtp.CSRCCount = 0;
                rtp.Extension = false;
                rtp.HeaderLength = RTPPacket.MinHeaderLength;
                rtp.Marker = false;
                rtp.Padding = false;
                rtp.PayloadType = 0;
                rtp.Version = 2;

                //RTP Header aktualisieren
                try
                {
                    rtp.SequenceNumber = Convert.ToUInt16(config.SequenceNumber);
                    config.SequenceNumber++;
                }
                catch (Exception)
                {
                    config.SequenceNumber = 0;
                }
                try
                {
                    rtp.Timestamp = Convert.ToUInt32(config.TimeStamp);
                    config.TimeStamp += mulaws.Length;
                }
                catch (Exception)
                {
                    config.TimeStamp = 0;
                }

                //Fertig
                return rtp;
            }

            public static int GetBytesPerInterval(uint SamplesPerSecond, int BitsPerSample, int Channels)
            {
                int blockAlign = ((BitsPerSample * Channels) >> 3);
                int bytesPerSec = (int)(blockAlign * SamplesPerSecond);
                uint sleepIntervalFactor = 1000 / 20; //20 Milliseconds
                int bytesPerInterval = (int)(bytesPerSec / sleepIntervalFactor);

                //Fertig
                return bytesPerInterval;
            }

            public static Int32 MulawToLinear(Int32 ulaw)
            {
                ulaw = ~ulaw;
                int t = ((ulaw & QUANT_MASK) << 3) + BIAS;
                t <<= (ulaw & SEG_MASK) >> SEG_SHIFT;
                return ((ulaw & SIGN_BIT) > 0 ? (BIAS - t) : (t - BIAS));
            }

            public static Byte[] MuLawToLinear(Byte[] bytes, int bitsPerSample, int channels)
            {
                //Anzahl Spuren
                int blockAlign = channels * bitsPerSample / 8;

                //Für jeden Wert
                Byte[] result = new Byte[bytes.Length * blockAlign];
                for (int i = 0, counter = 0; i < bytes.Length; i++, counter += blockAlign)
                {
                    //In Bytes umwandeln
                    int value = MulawToLinear(bytes[i]);
                    Byte[] values = BitConverter.GetBytes(value);

                    switch (bitsPerSample)
                    {
                        case 8:
                            switch (channels)
                            {
                                //8 Bit 1 Channel
                                case 1:
                                    result[counter] = values[0];
                                    break;

                                //8 Bit 2 Channel
                                case 2:
                                    result[counter] = values[0];
                                    result[counter + 1] = values[0];
                                    break;
                            }
                            break;

                        case 16:
                            switch (channels)
                            {
                                //16 Bit 1 Channel
                                case 1:
                                    result[counter] = values[0];
                                    result[counter + 1] = values[1];
                                    break;

                                //16 Bit 2 Channels
                                case 2:
                                    result[counter] = values[0];
                                    result[counter + 1] = values[1];
                                    result[counter + 2] = values[0];
                                    result[counter + 3] = values[1];
                                    break;
                            }
                            break;
                    }
                }

                //Fertig
                return result;
            }
        }
}
