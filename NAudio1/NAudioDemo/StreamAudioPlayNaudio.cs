using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace NAudioDemo
{
    //...
    class StreamAudioPlayNAudio
    {

        static void Main(string[] args)
        {
            //一般的な44.1kHz, 16bit, ステレオサウンドの音源を想定
            //var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));  // for sample.wav
            var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(8000, 8, 1));

            //ボリューム調整をするために上のBufferedWaveProviderをデコレータっぽく包む
            //var wavProvider = new VolumeWaveProvider16(bufferedWaveProvider);
            //wavProvider.Volume = 0.1f;

            //再生デバイスと出力先を設定
            var mmDevice = new MMDeviceEnumerator()
                .GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            //外部からの音声入力を受け付け開始
            Task t = StartDummySoundSource(bufferedWaveProvider);

            using (IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 200))
            {
                //出力に入力を接続して再生開始
                //wavPlayer.Init(wavProvider);
                wavPlayer.Init(bufferedWaveProvider);
                wavPlayer.Play();

                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();

                wavPlayer.Stop();
            }
        }

        //外部入力のダミーとしてデスクトップにある"sample.wav"あるいは"sample.mp3"を用いて音声を入力する
        static async Task StartDummySoundSource(BufferedWaveProvider provider)
        {
            ////外部入力のダミーとして適当な音声データを用意して使う
            //string wavFilePath = Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            //    "sample.wav"
            //    );
            ////mp3を使うならこう。
            //string mp3FilePath = Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            //    "sample.mp3"
            //    );
            //外部入力のダミーとして適当な音声データを用意して使う
            string wavFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "check_8bit_mono.pcm"
                );

            //if (!(File.Exists(wavFilePath) || File.Exists(mp3FilePath)))
            if (!(File.Exists(wavFilePath)))
            {
                Console.WriteLine("Target sound files were not found. Wav file or MP3 file is needed for this program.");
                Console.WriteLine($"expected wav file: {wavFilePath}");
                Console.WriteLine($"expected mp3 file: {wavFilePath}");
                Console.WriteLine("(note: ONE file is enough, two files is not needed)");
                return;
            }

            ////mp3しかない場合、先にwavへ変換を行う
            //if (!File.Exists(wavFilePath))
            //{
            //    using (var mp3reader = new Mp3FileReader(mp3FilePath))
            //    using (var pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3reader))
            //    {
            //        WaveFileWriter.CreateWaveFile(wavFilePath, pcmStream);
            //    }
            //}

            byte[] data = File.ReadAllBytes(wavFilePath);

            // do not need raw data only file
            ////若干効率が悪いがヘッダのバイト数を確実に割り出して削る
            //using (var r = new WaveFileReader(wavFilePath))
            //{
            //    int headerLength = (int)(data.Length - r.Length);
            //    data = data.Skip(headerLength).ToArray();
            //}

            int bufsize = 24000;
            for (int i = 0; i + bufsize < data.Length; i += bufsize)
            {
                provider.AddSamples(data, i, bufsize);
                await Task.Delay(100);
            }
        }
    }

}