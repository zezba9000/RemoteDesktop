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

        private byte[] getPosValueForEncoder(int x)
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

            byte[] abc = { a, b, c };

            return abc;
        }

        // adptive
        public byte[] Encode(byte[] samples)
        {
            origSamples = samples;
            // TODO: 4bit値を扱えるようにしないとダメ
            byte[] errorArray = new byte[samples.Length / 2];

            byte encoded_sample = 0; ;
            for (int x = 0; x < samples.Length; x++)
            {
                byte[] posValues = getPosValueForEncoder(x);
                byte a = posValues[0];
                byte b = posValues[1];
                byte c = posValues[2];

                byte currPix = samples[x];
                byte prevPix;

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

        private byte[] getPastDecodedValueForDecoder(int x)
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

            byte[] abc = { a, b, c };

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
                byte[] posValues = getPastDecodedValueForDecoder(x);
                byte a = posValues[0];
                byte b = posValues[1];
                byte c = posValues[2];

                byte error = errorArray[arr_idx];
                if(x % 2 == 0)
                {
                    error = (byte) (error >> 4);
                }
                else
                {
                    arr_idx++;
                }

                int int_4bit = 0;
                if((error & 0b1000) > 0)
                {
                    int_4bit -= error & 0b0111;
                }
                else
                {
                    int_4bit += error & 0b0111;
                }


                int prevPix;
                if (Math.Abs(a - c) < Math.Abs(b - c))
                {
                    prevPix = posValues[1];
                }
                else
                {
                    prevPix = posValues[0];
                }

                byte sample = (byte) (prevPix + error);

                pastDecodedSamples[x] = sample;
            }

            return pastDecodedSamples;
        }

    }
}
