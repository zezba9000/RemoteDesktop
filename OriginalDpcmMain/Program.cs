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
            String input_path = "F:\\work\\tmp\\capturedPCM_8000Hz_8bit_mono.raw";
            String output_path = "F:\\work\\tmp\\capturedPCM_8000Hz_8bit_mono_my8bitDPCM_en_de_code_pass_use_sbyte.raw";
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
            Utils.saveByteArrayToFile(decoder.Decode(encoder.Encode(ms.ToArray())), output_path);
        }
    }
}
