using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RemoteDesktop.Android.Core
{
    public class MyDpcmCodec
    {
        private byte[] origSamples;
        private byte[] pastDecodedSamples;

        private sbyte convertByteToSbyte(byte val)
        {
            return (sbyte) (val - 128);
        }

        private byte convertSbyteToByte(sbyte val)
        {
            return (byte) (val + 128);
        }

        private sbyte[] getPosValueForEncoder(int x)
        {
            int posA = x - 1;
            int posB = x - 2;
            int posC = x - 3;
            byte a, b, c = 0;

            if (x < 3)
            {
                a = c = 128;
            }
            else
            {
                a = origSamples[posA];
            }

            if (x < 3)
            {
                b = c = 128;
            }
            else
            {
                b = origSamples[posB];
            }
            if (c != 128)
            {
                c = origSamples[posC];

            }

            sbyte[] abc = { convertByteToSbyte(a), convertByteToSbyte(b), convertByteToSbyte(c) };

            return abc;
        }

        // adptive
        public byte[] Encode(byte[] samples)
        {
            origSamples = samples;
            int toNotOdd = samples.Length % 2 == 0 ? 0 : 1;
            int sampleLength = samples.Length - toNotOdd;
            int resultLength = sampleLength / 2;
            byte[] errorArray = new byte[resultLength];

            byte encoded_sample = 0; ;
            for (int x = 0; x < sampleLength; x++)
            {
                sbyte[] posValues = getPosValueForEncoder(x);
                sbyte a = posValues[0];
                sbyte b = posValues[1];
                sbyte c = posValues[2];

                sbyte currPix = convertByteToSbyte(samples[x]);
                sbyte prevPix;

                if (Math.Abs(a - c) < Math.Abs(b - c))
                {
                    prevPix = posValues[1];
                }
                else
                {
                    prevPix = posValues[0];
                }

                int error = currPix - prevPix;
                byte sign = error >= 0 ? (byte)0 : (byte)1;


                byte tmpMag = (byte) Math.Abs(error);
                byte encodedMag = tmpMag > 7 ? (byte) 7 : tmpMag;
                if(x % 2 == 0)
                {
                    encoded_sample = 0;
                    encoded_sample += (byte) (sign << 7);
                    encoded_sample += (byte) (encodedMag << 4);
                    continue;
                }
                else
                {
                    encoded_sample += (byte)(sign << 3);
                    encoded_sample += encodedMag;
                }
                errorArray[x/2] = encoded_sample;
            }
            return errorArray;
        }

        private sbyte[] getPastDecodedValueForDecoder(int x)
        {
            int posA = x - 1;
            int posB = x - 2;
            int posC = x - 3;
            byte a, b, c = 0;

            if (x < 3)
            {
                a = c = 128;
            }
            else
            {
                a = pastDecodedSamples[posA];
            }
            if (x < 3)
            {
                b = c = 128;
            }
            else
            {
                b = pastDecodedSamples[posB];
            }
            if (c != 128)
            {
                c = pastDecodedSamples[posC];
            }

            sbyte[] abc = { convertByteToSbyte(a), convertByteToSbyte(b), convertByteToSbyte(c) };

            return abc;
        }

        // adaptive
        public byte[] Decode(byte[] encoded_data)
        {
            pastDecodedSamples = new byte[encoded_data.Length * 2]; // 4bitで1サンプルなので1byteに2サンプル入っている
            // TODO: 4bit値を扱えるようにしないとダメ
            byte[] errorArray = encoded_data;

            for (int x = 0, arr_idx = 0; x < pastDecodedSamples.Length; x++)
            {
                sbyte[] posValues = getPastDecodedValueForDecoder(x);
                sbyte a = posValues[0];
                sbyte b = posValues[1];
                sbyte c = posValues[2];

                byte error = errorArray[arr_idx];
                if(x % 2 == 0)
                {
                    error = (byte) (error >> 4);
                }
                else
                {
                    arr_idx++;
                }

                sbyte signed_error = 0;
                if((error & 0b1000) > 0)
                {
                    signed_error -= (sbyte) (error & 0b0111);
                }
                else
                {
                    signed_error += (sbyte) (error & 0b0111);
                }


                sbyte prevPix;
                if (Math.Abs(a - c) < Math.Abs(b - c))
                {
                    prevPix = posValues[1];
                }
                else
                {
                    prevPix = posValues[0];
                }

                byte sample = convertSbyteToByte((sbyte)(prevPix + signed_error));

                pastDecodedSamples[x] = sample;
            }

            return pastDecodedSamples;
        }

    }
}
