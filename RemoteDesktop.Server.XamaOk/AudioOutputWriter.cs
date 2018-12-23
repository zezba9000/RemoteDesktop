using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using RemoteDesktop.Server.XamaOK;
using RemoteDesktop.Android.Core;
using NAudio.Wave.Compression;
using System.Media;
using NAudio.Wave.SampleProviders;

namespace RemoteDesktop.Server.XamaOK
{

    public sealed class AudioOutputWriter : IDisposable
    {

        #region イベント

        //public event EventHandler<WaveInEventArgs> DataAvailable;

        #endregion

        #region フィールド

        //private readonly string _FileName;

        private WasapiLoopbackCapture _WaveIn;
        private UDPSender usender;
        //private WaveFileWriter _WaveFileWriter;
        private MMDevice m_device;
        public bool IsRecording = false;
        private RTPConfiguration rtp_config;

        private WinSound.JitterBuffer m_JitterBuffer;
        private uint m_JitterBufferCount = 20; // max buffering num of RTPPacket at jitter buffer
        private uint m_Milliseconds = 20; // time period of jitter buffer (msec)

        //private BufferedStream checkFileStream;

        //private int m_CurrentRTPBufferPos = 0;
        //private int m_RTPPartsLength = 0;
        //private byte[] m_FilePayloadBuffer;
        //private byte[] m_PartByte;
        //private int m_RTPPPartsLength = 0;

        #endregion

        #region コンストラクタ

        // this call block until recieved client connection request
        public AudioOutputWriter(MMDevice device, RTPConfiguration config)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            //string wavFilePath = Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            //    "check_8bit_mono.pcm"
            //    );
            //checkFileStream = new BufferedStream(new FileStream(wavFilePath, FileMode.Append));

            rtp_config = config;
            m_JitterBufferCount = (uint)rtp_config.BufferCount;
            m_Milliseconds = rtp_config.JitterBufferTimerPeriodMsec;

            m_device = device;
//            this._FileName = fileName;
            this._WaveIn = new WasapiLoopbackCapture(m_device);
            this._WaveIn.DataAvailable += this.WaveInOnDataAvailable;
            this._WaveIn.RecordingStopped += this.WaveInOnRecordingStopped;

            usender = new UDPSender(Utils.getLocalIP().ToString(), 10000, 10);
            //this._Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
            //this._WaveFileWriter = new WaveFileWriter(usender, this._WaveIn.WaveFormat);

            // キャプチャしたサウンドを読み込んだ際のハンドラ
            //DataAvailable += AudioOutputWriterOnDataAvailable;

            WaveFormat wfmt = this._WaveIn.WaveFormat;
            Console.WriteLine(wfmt.ToString());

            InitJitterBuffer();
            m_JitterBuffer.Start();
            Start(); // start capture and send stream
        }

        #endregion

        #region プロパティ

        //public bool IsRecording
        //{
        //    get {
        //        return IsRecording;
        //    }

        //    //private set;
        //}

        #endregion

        #region メソッド

        private void InitJitterBuffer()
        {
            //Wenn vorhanden
            if (m_JitterBuffer != null)
            {
                m_JitterBuffer.DataAvailable -= new WinSound.JitterBuffer.DelegateDataAvailable(OnDataAvailableForJitterBuffer);
            }

            //Neu erstellen
            m_JitterBuffer = new WinSound.JitterBuffer(null, m_JitterBufferCount, m_Milliseconds);
            m_JitterBuffer.DataAvailable += new WinSound.JitterBuffer.DelegateDataAvailable(OnDataAvailableForJitterBuffer);
        }

        private void DisposeJitterBuffer()
        {
            if (m_JitterBuffer != null)
            {
                m_JitterBuffer.DataAvailable -= new WinSound.JitterBuffer.DelegateDataAvailable(OnDataAvailableForJitterBuffer);
            }
            m_JitterBuffer.Stop();
            m_JitterBuffer = null;
        }

        private void updateRTPConfiguration()
        {
            //throw new Exception();
        }

        public void Start()
        {
            this.IsRecording = true;
            this._WaveIn.StartRecording();

        }

        //public void Stop()
        //{
        //    this.IsRecording = false;
        //    this._WaveIn.StopRecording();
        //}

        private void resetAllInstanseState()
        {
            // UDPSenderをそのまま使いまわすため、this_WaveFileWriterはDispose的なことしない

            //this.DataAvailable -= this.AudioOutputWriterOnDataAvailable;
            //this._WaveFileWriter.Close();
            //this._WaveFileWriter.Dispose();
            //this._WaveFileWriter = null;

            this.DisposeJitterBuffer();

            this._WaveIn.DataAvailable -= this.WaveInOnDataAvailable;
            this._WaveIn.RecordingStopped -= this.WaveInOnRecordingStopped;
            this._WaveIn.StopRecording();
            this._WaveIn.Dispose();
            this._WaveIn = null;

            //------

            this._WaveIn = new WasapiLoopbackCapture(m_device);
            this._WaveIn.DataAvailable += this.WaveInOnDataAvailable;
            this._WaveIn.RecordingStopped += this.WaveInOnRecordingStopped;

            //usender = new UDPSender(Utils.getLocalIP().ToString(), 10000, 10);
            //this._Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
            //this._WaveFileWriter = new WaveFileWriter(usender, this._WaveIn.WaveFormat);

            // キャプチャしたサウンドを読み込んだ際のハンドラ
            //this.DataAvailable += this.AudioOutputWriterOnDataAvailable;

            InitJitterBuffer();
            m_JitterBuffer.Start();
            Start(); // start capture and send stream
        }

        #region オーバーライド
        #endregion

        #region イベントハンドラ

        private void OnDataAvailableForJitterBuffer(Object sender, RTPPacket rtp)
        {
            try
            {
                if (usender != null)
                {
                        //RTP Packet in Bytes umwandeln
                        Byte[] rtpBytes = rtp.ToBytes();
                        //Absenden
                        usender.SendBytes(rtpBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //private void AudioOutputWriterOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        //{
        //}

        private void WaveInOnRecordingStopped(object sender, StoppedEventArgs e)
        {
            //if (this._WaveFileWriter != null)
            //{
            //    this._WaveFileWriter.Close();
            //    this._WaveFileWriter = null;
            //}

            //if (this.usender != null)
            //{
            //    this.usender.Close();
            //    this.usender = null;
            //}

            //this.Dispose();
        }

        private void WaveInOnDataAvailable(object sender, WaveInEventArgs e)
        {
            //this._WaveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            //this.DataAvailable?.Invoke(this, e);

            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd hh:mm:ss.fff} : {e.BytesRecorded} bytes");

            byte[] recorded_buf = e.Buffer;
            int recorded_length = e.BytesRecorded;

            //byte[] converted_buf = null;
            //int converted_len = -1;

            //byte[] depthConv_buf = null;
            //int depthConvBytes = -1;

            byte[] pcm16_buf = null;
            int pcm16_len = -1;

            //byte[] pcm8_buf = null;
            //int pcm8_len = -1;

            try
            {
                //// 生データを再生可能なデータに変換
                var waveBufferResample = new BufferedWaveProvider(this._WaveIn.WaveFormat);
                waveBufferResample.DiscardOnBufferOverflow = true;
                waveBufferResample.ReadFully = false;  // leave a buffer?
                var sampleStream = new WaveToSampleProvider(waveBufferResample);

                // Downsample to 8000
                var resamplingProvider = new WdlResamplingSampleProvider(sampleStream, rtp_config.SamplesPerSecond);

                // Stereo to mono
                var monoStream = new StereoToMonoSampleProvider(resamplingProvider)
                {
                    LeftVolume = 1f,
                    RightVolume = 1f
                };

                // Convert to 32bit float to 16bit PCM
                var ieeeToPcm = new SampleToWaveProvider16(monoStream);
                pcm16_len = recorded_length / (2 * 6 * 2);
                pcm16_buf = new byte[pcm16_len];

                waveBufferResample.AddSamples(recorded_buf, 0, recorded_length);
                ieeeToPcm.Read(pcm16_buf, 0, pcm16_len);

                //var depthConvStream = new AcmStream(new WaveFormat(rtp_config.SamplesPerSecond, 16, 1), new WaveFormat(rtp_config.SamplesPerSecond, rtp_config.BitsPerSample, 1));
                //Buffer.BlockCopy(pcm16_buf, 0, depthConvStream.SourceBuffer, 0, pcm16_len);
                //int sourceBytesDepthConverted = 0;

                //pcm8_len = depthConvStream.Convert(pcm16_len, out sourceBytesDepthConverted);
                //pcm8_buf = new byte[pcm8_len];
                //Buffer.BlockCopy(depthConvStream.DestBuffer, 0, pcm8_buf, 0, pcm8_len);

                Console.WriteLine("convert 32bit float 64KHz stereo to 16bit PCM 8KHz mono success");
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("exit...");
                System.Windows.Forms.Application.Exit();
            }

            try
            {
                if (rtp_config.isAlreadySetInfoFromSndCard == false)
                {
                    // キャプチャした音声データについて情報を設定
                    updateRTPConfiguration();
                    //m_RTPPartsLength = SoundUtils.GetBytesPerInterval((uint)rtp_config.SamplesPerSecond, rtp_config.SamplesPerSecond, rtp_config.Channels);
                    rtp_config.isAlreadySetInfoFromSndCard = true;
                }

                //次のコネクションが来ていないかチェックする
                usender.checkNextClient();
                if (usender.disconnected == true) // 次のコネクションが来ていたら (前の接続は切れている想定)
                {
                    //resetFileStreamingState();
                    resetAllInstanseState();
                    usender.disconnected = false;
                }

                //Wenn noch aktiv
                if (!usender.disconnected)
                {
                    //Wenn JitterBuffer
                    if (rtp_config.UseJitterBuffer)
                    {
                        //Sounddaten in kleinere Einzelteile zerlegen
                        int bytesPerInterval = SoundUtils.GetBytesPerInterval((uint)rtp_config.SamplesPerSecond, rtp_config.BitsPerSample, rtp_config.Channels);
                        int count = pcm16_len / bytesPerInterval;
                        int currentPos = 0;
                        for (int i = 0; i < count; i++)
                        {
                            //Teilstück in RTP Packet umwandeln
                            Byte[] partBytes = new Byte[bytesPerInterval];
                            Array.Copy(pcm16_buf, currentPos, partBytes, 0, bytesPerInterval);
                            currentPos += bytesPerInterval;
                            var rtp = SoundUtils.ToRTPPacket(partBytes, rtp_config);
                            //In Buffer legen
                            m_JitterBuffer.AddData(rtp);
                        }
                        // 余りデータがあれば送信する
                        int leftData_len = pcm16_len - currentPos;
                        if(leftData_len > 0)
                        {
                            Byte[] leftData_buf = new Byte[leftData_len];
                            Array.Copy(pcm16_buf, currentPos, leftData_buf, 0, leftData_len);
                            var rtp = SoundUtils.ToRTPPacket(leftData_buf, rtp_config);
                            m_JitterBuffer.AddData(rtp);
                        }
                    }
                    else
                    {
                        //checkFileStream.Write(depthConv_buf, 0, depthConvBytes);
                        //if (checkFileStream.Length > (8000 * 120))
                        //{
                        //    checkFileStream.Flush();
                        //    checkFileStream.Close();
                        //    System.Windows.Forms.Application.Exit();
                        //}

                        //usender.SendBytes(SoundUtils.ToRTPData(converted_buf, rtp_config));
                        //usender.SendBytes(SoundUtils.ToRTPData(depthConv_buf, rtp_config));
                        usender.SendBytes(SoundUtils.ToRTPData(pcm16_buf, rtp_config));
                        //usender.SendBytes(SoundUtils.ToRTPData(pcm8_buf, rtp_config));
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                resetAllInstanseState();
            }
        }

        #endregion

        #region ヘルパーメソッド
        #endregion

        #endregion

        #region IDisposable メンバー

        public void Dispose()
        {

            //this.DataAvailable -= this.AudioOutputWriterOnDataAvailable;

            if(this._WaveIn != null)
            {
                this._WaveIn.StopRecording();
                this._WaveIn.DataAvailable -= this.WaveInOnDataAvailable;
                this._WaveIn.RecordingStopped -= this.WaveInOnRecordingStopped;
                this._WaveIn.Dispose();
            }

            //this._WaveFileWriter?.Dispose();
            this.usender?.Dispose();
        }

        #endregion

    }

}
