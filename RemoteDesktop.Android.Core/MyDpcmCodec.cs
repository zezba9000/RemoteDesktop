using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteDesktop.Android.Core
{
    class MyDpcmCodec
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
            int[] errorArray = new int[origPix.length];


            for (int x = 0; x < origPix.length; x++)
            {
                int[] posValues = getPosValue(x);
                int a = posValues[0];
                int b = posValues[1];
                int c = posValues[2];

                int currPix = (origPix[x] >> 16) & 0xFF;
                int prevPix;

                if (Math.abs(a - c) < Math.abs(b - c))
                {
                    prevPix = posValues[1];
                }
                else
                {
                    prevPix = posValues[0];
                }

                int error = currPix - prevPix;

                errorArray[x] = error;
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

            int[] errorArray = errorPix;

            for (int x = 0; x < endPix.length; x++)
            {
                int[] posValues = getPosValue(x);
                int a = posValues[0];
                int b = posValues[1];
                int c = posValues[2];

                int error = errorArray[x];

                int prevPix;

                if (Math.abs(a - c) < Math.abs(b - c))
                {
                    prevPix = posValues[1];
                }
                else
                {
                    prevPix = posValues[0];
                }

                int pixel = prevPix + error;

                endPix[x] = (0xFF << 24) | (pixel << 16) | (pixel << 8) | pixel;
            }

            return endPix;
        }

    }
}
