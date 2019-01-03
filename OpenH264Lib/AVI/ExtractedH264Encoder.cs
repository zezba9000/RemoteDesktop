using OpenH264Sample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public ExtractedH264Encoder(int width, int height, int bps, float fps, float keyFrameInterval)
        {
            // H264エンコーダーを作成
            encoder = new OpenH264Lib.Encoder("openh264-1.7.0-win32.dll");
            
            // 1フレームエンコードするごとにライターに書き込み
            OpenH264Lib.Encoder.OnEncodeCallback onEncode = (data, length, frameType) =>
            {
                var keyFrame = (frameType == OpenH264Lib.Encoder.FrameType.IDR) || (frameType == OpenH264Lib.Encoder.FrameType.I);
                //var ms = new MemoryStream();
                //var writer = new H264Writer(ms, width, height, fps);
                var writer = new H264Writer(new FileStream("F:\\work\\tmp\\gen_HLS_files_from_h264_avi_file_try\\avi-" + timestamp.ToString() + ".avi" , FileMode.Create), width, height, fps);
                timestamp++;
                writer.AddImage(data, keyFrame);
                writer.Close();

                //byte[] ms_buf = ms.ToArray();
                ////Array.Resize<byte>(ref ms_buf, (int)ms.Length);

                //byte[] tmp_buf = new byte[ms.Length];
                //Array.Copy(ms_buf, 0, tmp_buf, 0, ms.Length);
                //aviDataGenerated(tmp_buf);
                //ms.Close();

                Console.WriteLine("Encord {0} bytes, KeyFrame:{1}", length, keyFrame);
            };

            // H264エンコーダーの設定
            //encoder.Setup(width, height, 5000000, fps, 10.0f, onEncode);
            encoder.Setup(width, height, bps, fps, keyFrameInterval, onEncode);
        }

        public void addBitmapFrame(byte[] data, int frameNumber)
        {
            encoder.Encode(data, frameNumber);
        }
    }
}
