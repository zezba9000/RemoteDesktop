using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktop.Server.XamaOK
{

    class SoundEncodeUtil
    {
        public static byte[] encodePCMtoMP3(IWaveProvider waveIn)
        {
            //  どこかの初期化処理でMediaFoundationのおまじないを書かないと
            MediaFoundationEncoder.EncodeToMp3(waveIn, "F:\\work\\tmp\\tmp.mp3");
            Console.WriteLine("before create MP3FileReader");
            Mp3FileReader reader = new Mp3FileReader("F:\\work\\tmp\\tmp.mp3");
            Console.WriteLine("after create MP3FileReader");
            MemoryStream ms = new MemoryStream();
            Mp3Frame ret;
            while((ret = reader.ReadNextFrame()) != null)
            {
                //Console.WriteLine("write frame data to MemoryStream size = " + ret.FrameLength);
                ms.Write(ret.RawData, 0, ret.FrameLength);
            }
            reader.Close();
            reader.Dispose();

            var ret_buf = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return ret_buf;
        }
    }
}
