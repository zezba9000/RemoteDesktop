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
