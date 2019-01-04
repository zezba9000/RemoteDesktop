using OpenH264Sample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace OpenH264.Encoder
{
    public delegate void H264AVIDataHandler(byte[] encodedMediaFileData);

    //このプロジェクトのコードから必要な機能だけを抽出したクラス
    public class ExtractedH264Encoder
    {
        private OpenH264Lib.Encoder encoder;
        public event H264AVIDataHandler aviDataGenerated;
        private int timestamp = 0; // equal frame number
        private H264Writer writer = null;
        private int width;
        private int height;
        private string hlsBasePath = "F:\\work\\tmp\\gen_HLS_files_from_h264_avi_file_try\\";
        //private GCHandle pinnedArray = GCHandle.Alloc(new byte[1] { 0 }, GCHandleType.Pinned);
        //private byte[] bufForEncoder = null;
        //IntPtr pointerOfEncoderInternalBuf = IntPtr.Zero;

        public ExtractedH264Encoder(int width, int height, int bps, float fps, float keyFrameInterval)
        {
            this.width = width;
            this.height = height;

            // H264エンコーダーを作成
            encoder = new OpenH264Lib.Encoder("openh264-1.7.0-win32.dll");

            //// この領域は意図的にFreeしない
            //bufForEncoder = new byte[54 + width * height * 4];
            //pinnedArray = GCHandle.Alloc(bufForEncoder, GCHandleType.Pinned);
            //pointerOfEncoderInternalBuf = pinnedArray.AddrOfPinnedObject();

            // 1フレームエンコードするごとにライターに書き込み
            OpenH264Lib.Encoder.OnEncodeCallback onEncode = (data, length, frameType) =>
            {
                var keyFrame = (frameType == OpenH264Lib.Encoder.FrameType.IDR) || (frameType == OpenH264Lib.Encoder.FrameType.I);
                //var ms = new MemoryStream();
                //var writer = new H264Writer(ms, width, height, fps); 

                if(timestamp == 0)
                {
                    writer = new H264Writer(new FileStream(hlsBasePath + "avi-" + ((int)(timestamp / 60)).ToString() + ".avi", FileMode.Create), width, height, fps);
                }
                if(timestamp % 60 == 0 && timestamp != 0)
                {
                    writer.Close();
                    Console.WriteLine("a avi file stream closed");
                    //writer = new H264Writer(new FileStream("F:\\work\\tmp\\gen_HLS_files_from_h264_avi_file_try\\avi-" + ((int)(timestamp/2)).ToString() + "-" + ((frameType == OpenH264Lib.Encoder.FrameType.I) ? "I" : "IDR") + ".avi", FileMode.Create), width, height, fps);
                    writer = new H264Writer(new FileStream(hlsBasePath + "avi-" + ((int)(timestamp/60)).ToString() + ".avi", FileMode.Create), width, height, fps);
                }
                writer.AddImage(data, keyFrame);
                timestamp++;

                //byte[] ms_buf = ms.ToArray();
                ////Array.Resize<byte>(ref ms_buf, (int)ms.Length);
                //byte[] tmp_buf = new byte[ms.Length];
                //Array.Copy(ms_buf, 0, tmp_buf, 0, ms.Length);
                //aviDataGenerated(tmp_buf);
                //ms.Close();

                Console.WriteLine("Encord {0} bytes, data.Length: {1} bytes, KeyFrame:{2} timestamp:{3} " + frameType.ToString(), length, data.Length, keyFrame, timestamp);
            };

            // H264エンコーダーの設定
            //encoder.Setup(width, height, 5000000, fps, 10.0f, onEncode);
            encoder.Setup(width, height, bps, fps, keyFrameInterval, onEncode);
        }

        public void addBitmapFrame(byte[] data, int frameNumber)
        {
            //var bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);

            //Marshal.Copy(data, 0, pointerOfEncoderInternalBuf, 54 + width * height * 4);
            //var bmp = new System.Drawing.Bitmap(width, height, 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, pointerOfEncoderInternalBuf);
            //encoder.Encode(bmp, frameNumber);
            //bmp.Dispose();
            //RemoteDesktop.Android.Core.Utils.saveByteArrayToFile(data, hlsBasePath + "homobrewBmpFile" + frameNumber.ToString() + ".bmp");
            //encoder.Encode(data, frameNumber);

            byte[] copy_buf = new byte[data.Length];
            Array.Copy(data, 0, copy_buf, 0, data.Length);
            encoder.Encode(copy_buf, frameNumber);

            //pinnedArray.Free();            
        }
    }
}
