using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteDesktop.Android.Core;

namespace OriginalDpcmMain
{
    class Program
    {
        static void Main(string[] args)
        {
            String input_path = "F:\\work\\tmp\\capturedPCM_ffmpeg_stdout_converted_8000Hz_u8bit_1ch_120sec.raw";
            String output_path = "F:\\work\\tmp\\capturedPCM_ffmpeg_stdout_converted_8000Hz_u8bit_1ch_120sec_encoded_my_DPCM_codec.raw";
            MemoryStream ms = new MemoryStream();
            byte[] buf = new byte[1024];

            var reader = new FileStream(input_path, FileMode.Open);
            int readBytes;
            try
            {
                while ((readBytes = reader.Read(buf, 0, buf.Length)) > 0)
                {
                    ms.Write(buf, 0, readBytes);
                }
            }
            finally
            {
                ms.Flush();
                reader.Close();
            }

            var encoder = new MyDpcmCodec();
            var decoder = new MyDpcmCodec();
            //Utils.saveByteArrayToFile(decoder.Decode(encoder.Encode(ms.ToArray())), output_path);
            Utils.saveByteArrayToFile(decoder.Decode(encoder.Encode(ms.ToArray())), output_path);
        }
    }
}
