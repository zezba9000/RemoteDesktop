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
using RemoteDesktop.Android.Core.Sound;
using System.Net;
using NAudio.MediaFoundation;
using System.Threading;

namespace RemoteDesktop.Server.XamaOK
{

    public sealed class AudioOutputWriter : IDisposable
    {

        #region イベント

        //public event EventHandler<WaveInEventArgs> DataAvailable;

        #endregion

        #region フィールド

        private WasapiLoopbackCapture _WaveIn;
        private UDPSender usender;
        private SoundDataSocket sdsock;
        private MMDevice m_device;
        public bool IsRecording = false;
        private RTPConfiguration rtp_config;

        private WinSound.JitterBuffer m_JitterBuffer;
        private uint m_JitterBufferCount = 20; // max buffering num of RTPPacket at jitter buffer
        private uint m_Milliseconds = 20; // time period of jitter buffer (msec)
        private MemoryStream debug_ms = new MemoryStream();

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

            //MediaFounDataionを利用する前のおまじない
            MediaFoundationApi.Startup();

            rtp_config = config;
            m_JitterBufferCount = rtp_config.JitterBuffer;
            m_Milliseconds = rtp_config.JitterBufferTimerPeriodMsec;

            m_device = device;
            this._WaveIn = new WasapiLoopbackCapture(m_device);
            this._WaveIn.DataAvailable += this.WaveInOnDataAvailable;
            this._WaveIn.RecordingStopped += this.WaveInOnRecordingStopped;
            WaveFormat wfmt = this._WaveIn.WaveFormat;

            if(rtp_config.protcol_mode == RTPConfiguration.ProtcolMode.UDP)
            {
                usender = new UDPSender(Utils.getLocalIP().ToString(), 10000, 10);
                InitJitterBuffer();
                m_JitterBuffer.Start();
            }
            else //TCP
            {
                sdsock = new SoundDataSocket(NetworkTypes.Server);
                //dispatcher = Dispatcher.CurrentDispatcher;
                sdsock.ConnectedCallback += Socket_ConnectedCallback;
                sdsock.DisconnectedCallback += Socket_DisconnectedCallback;
                sdsock.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
                sdsock.DataRecievedCallback += Socket_DataRecievedCallback;
                sdsock.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
                sdsock.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
                sdsock.Listen(IPAddress.Parse(RTPConfiguration.ServerAddress), rtp_config.SoundServerPort);
            }

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
            // UDPSender や SoundDataSocketはそのまま使いまわす

            //this.DataAvailable -= this.AudioOutputWriterOnDataAvailable;
            //this._WaveFileWriter.Close();
            //this._WaveFileWriter.Dispose();
            //this._WaveFileWriter = null;

            if(rtp_config.protcol_mode == RTPConfiguration.ProtcolMode.UDP)
            {
                this.DisposeJitterBuffer();
            }


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

            if (rtp_config.protcol_mode == RTPConfiguration.ProtcolMode.UDP)
            {
                InitJitterBuffer();
                m_JitterBuffer.Start();
            }
            else
            {
                sdsock.ReListen();
            }

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

        private void saveIEEE32bitFloatToMP3File(byte[] data, int length)
        {
            if(length == 0)
            {
                return;
            }


            debug_ms.Write(data, 0, length);
            int required_len = 48000 * 4 * 2 * 30;
            if (debug_ms.Length < required_len) // 30sec
            {
                return;
            }

            try
            {
                //// 生データを再生可能なデータに変換
                var waveBufferProvider = new BufferedWaveProvider(this._WaveIn.WaveFormat);
                waveBufferProvider.BufferLength = 48000 * 4 * 2 * 30;

                waveBufferProvider.DiscardOnBufferOverflow = true;
                waveBufferProvider.ReadFully = false;  // leave a buffer?
                var sampleProvider = waveBufferProvider.ToSampleProvider();

                var sampleStream = new WaveToSampleProvider(waveBufferProvider);

                // Downsample to 24KHz
                var resamplingProvider = new WdlResamplingSampleProvider(sampleStream, rtp_config.SamplesPerSecond);

                // Stereo to mono
                var monoProvider = new StereoToMonoSampleProvider(resamplingProvider)
                {
                    LeftVolume = 1f,
                    RightVolume = 1f
                };

                // Convert to 32bit float to 16bit PCM
                var ieeeToPcm = new SampleToWaveProvider16(monoProvider);

                //Convert 16bit PCM to 8bit PCM
                var depthConvertProvider = new WaveFormatConversionProvider(new WaveFormat(rtp_config.SamplesPerSecond, 8, 1), ieeeToPcm);

                int pcm16_len = (int) (debug_ms.Length / (2 * 6 * 2));
                byte[] pcm16_buf = new byte[pcm16_len];

                // データを源流から流す
                waveBufferProvider.AddSamples(debug_ms.ToArray(), 0, (int)debug_ms.Length);

                ieeeToPcm.Read(pcm16_buf, 0, pcm16_len);

                var depthConvertStream = new WaveFormatConversionStream(new WaveFormat(rtp_config.SamplesPerSecond, 8, 1), new RawSourceWaveStream(pcm16_buf, 0, pcm16_len, new WaveFormat(rtp_config.SamplesPerSecond, 16, 1)));

                SoundEncodeUtil.encodePCMtoMP3(depthConvertStream);

                Thread.Sleep(60 * 1000);
                Environment.Exit(0);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("exit...");
                System.Windows.Forms.Application.Exit();
            }            
        }

        private byte[] convertIEEE32bitFloatTo8bitPCMAndEncodeToMP3(WaveInEventArgs e)
        {
            byte[] recorded_buf = e.Buffer;
            int recorded_length = e.BytesRecorded;
            if(recorded_length == 0)
            {
                return null;
            }

            //byte[] depthConv_buf = null;
            //int depthConvBytes = -1;

            byte[] pcm16_buf = null;
            int pcm16_len = -1;

            //byte[] pcm8_buf = null;

            byte[] mp3_buf = null;
            try
            {
                //// 生データを再生可能なデータに変換
                var waveBufferResample = new BufferedWaveProvider(this._WaveIn.WaveFormat);
                waveBufferResample.DiscardOnBufferOverflow = true;
                waveBufferResample.ReadFully = false;  // leave a buffer?
                waveBufferResample.BufferLength = recorded_length;
                var sampleStream = new WaveToSampleProvider(waveBufferResample);

                // Downsample to 8000
                var resamplingProvider = new WdlResamplingSampleProvider(sampleStream, rtp_config.SamplesPerSecond);

                // Stereo to mono
                var monoProvider = new StereoToMonoSampleProvider(resamplingProvider)
                {
                    LeftVolume = 1f,
                    RightVolume = 1f
                };

                // Convert to 32bit float to 16bit PCM
                var ieeeToPcm = new SampleToWaveProvider16(monoProvider);

                waveBufferResample.AddSamples(recorded_buf, 0, recorded_length);

                mp3_buf = SoundEncodeUtil.encodePCMtoMP3(ieeeToPcm);

                ////Convert 16bit PCM to 8bit PCM
                //var depthConvertProvider = new WaveFormatConversionProvider(new WaveFormat(rtp_config.SamplesPerSecond, 8, 1), ieeeToPcm);

                //pcm16_len = recorded_length / (2 * 6 * 2);
                //pcm16_buf = new byte[pcm16_len];
                //ieeeToPcm.Read(pcm16_buf, 0, pcm16_len);
                //var depthConvertStream = new WaveFormatConversionStream(new WaveFormat(rtp_config.SamplesPerSecond, 8, 1), new RawSourceWaveStream(pcm16_buf, 0, pcm16_len, new WaveFormat(rtp_config.SamplesPerSecond, 16, 1)));

                //// データを源流から流す
                //waveBufferResample.AddSamples(recorded_buf, 0, recorded_length);

                //int pcm8_len = pcm16_len / 2;
                //pcm8_buf = new byte[pcm8_len];
                //depthConvertStream.Read(pcm8_buf, 0, pcm8_len);

                //mp3_buf = SoundEncodeUtil.encodePCMtoMP3(depthConvertProvider);


                /*
                var waveBufferResample = new BufferedWaveProvider(this._WaveIn.WaveFormat);
                waveBufferResample.AddSamples(recorded_buf, 0, recorded_length);
                mp3_buf = SoundEncodeUtil.encodePCMtoMP3(waveBufferResample);
                */
                Console.WriteLine(Utils.getFormatedCurrentTime() + " converted 32bit float 64KHz stereo " + recorded_length.ToString()  + " bytes to 16bit PCM 8KHz mono and encode it to mp3 compressed data " + mp3_buf.Length.ToString() + " bytes");
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("exit...");
                System.Windows.Forms.Application.Exit();
            }

            //return pcm16_buf;
            //return pcm8_buf;
            return mp3_buf;
        }

        private void handleDataWithUDP(byte[] pcm8_buf)
        {
                int pcm8_len = pcm8_buf.Length;

                //次のコネクションが来ていないかチェックする
                usender.checkNextClient();
                if (usender.disconnected == true) // 次のコネクションが来ていたら (前の接続は切れている想定)
                {
                    //resetFileStreamingState();
                    resetAllInstanseState();
                    usender.disconnected = false;
                }

                // こういうケースがあるようだ
                if(pcm8_buf.Length == 0)
                {
                　　return;
                }

                //Wenn noch aktiv
                if (!usender.disconnected)
                {
                    //Wenn JitterBuffer
                    if (rtp_config.UseJitterBuffer)
                    {
                        //Sounddaten in kleinere Einzelteile zerlegen
                        int bytesPerInterval = SoundUtils.GetBytesPerInterval((uint)rtp_config.SamplesPerSecond, rtp_config.BitsPerSample, rtp_config.Channels);
                        int count = pcm8_len / bytesPerInterval;
                        int currentPos = 0;
                        for (int i = 0; i < count; i++)
                        {
                            //Teilstück in RTP Packet umwandeln
                            Byte[] partBytes = new Byte[bytesPerInterval];
                            Array.Copy(pcm8_buf, currentPos, partBytes, 0, bytesPerInterval);
                            currentPos += bytesPerInterval;
                            var rtp = SoundUtils.ToRTPPacket(partBytes, rtp_config);
                            //In Buffer legen
                            m_JitterBuffer.AddData(rtp);
                        }
                        // 余りデータがあれば送信する
                        int leftData_len = pcm8_len - currentPos;
                        if(leftData_len > 0)
                        {
                            Byte[] leftData_buf = new Byte[leftData_len];
                            Array.Copy(pcm8_buf, currentPos, leftData_buf, 0, leftData_len);
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

                        usender.SendBytes(SoundUtils.ToRTPData(pcm8_buf, rtp_config));
                    }

                }
        }

        public void handleDataWithTCP(byte[] pcm8_buf)
        {
            Console.WriteLine("call handleDataWithTcp");
            if (!sdsock.IsConnected())
            {
                return;
            }
            // こういうケースがあるようだ
            if(pcm8_buf.Length == 0)
            {
                return;
            }
            Console.WriteLine("call SoundUtils.ToRTPPacket");
            RTPPacket rtp = SoundUtils.ToRTPPacket(pcm8_buf, rtp_config);
            Console.WriteLine("call sdsock.SendRTPPacket");
            sdsock.SendRTPPacket(rtp, rtp_config.compress, rtp_config.SamplesPerSecond, rtp_config.BitsPerSample, rtp_config.Channels, rtp_config.isConvertMulaw);
        }

        private void WaveInOnDataAvailable(object sender, WaveInEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd hh:mm:ss.fff} : {e.BytesRecorded} bytes");

            //saveIEEE32bitFloatToMP3File(e.Buffer, e.BytesRecorded);

            //if (sdsock.IsConnected() == false)
            //{
            //    return;
            //}


            if (RTPConfiguration.isUseFFMPEG)
            {
                debug_ms.Write(e.Buffer, 0, e.BytesRecorded);
                if (debug_ms.Length > 2 * 1024 * 1024)
                {
                    Utils.saveByteArrayToFile(debug_ms.ToArray(), "F:\\work\\tmp\\capturedPCM.raw");
                    Environment.Exit(0);
                }

                //MainApplicationContext.ffmpegProc.StandardInput.BaseStream.Write(e.Buffer, 0, e.BytesRecorded);	
                //MainApplicationContext.ffmpegProc.StandardInput.BaseStream.Flush();
                //return;
            }
            else
            {
                byte[] mp3_buf = convertIEEE32bitFloatTo8bitPCMAndEncodeToMP3(e);
                if(mp3_buf == null)
                {
                    return;
                }

                try
                {
                    //if (rtp_config.isAlreadySetInfoFromSndCard == false)
                    //{
                    //    // キャプチャした音声データについて情報を設定
                    //    updateRTPConfiguration();
                    //    //m_RTPPartsLength = SoundUtils.GetBytesPerInterval((uint)rtp_config.SamplesPerSecond, rtp_config.SamplesPerSecond, rtp_config.Channels);
                    //    rtp_config.isAlreadySetInfoFromSndCard = true;
                    //}

                    if(rtp_config.protcol_mode == RTPConfiguration.ProtcolMode.UDP)
                    {
                        handleDataWithUDP(mp3_buf);
                    }
                    else
                    {
                        handleDataWithTCP(mp3_buf);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    resetAllInstanseState();
                }
            }
        }

		private void Socket_StartDataRecievedCallback(PacketHeader pktHdr)
		{
            // do nothing
		}

		private void Socket_EndDataRecievedCallback()
		{
			// do nothing
		}

		private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			// do nothing
		}

        // only used on Client
		private void Socket_ConnectionFailedCallback(string error)
		{
			DebugLog.LogError("Failed to connect: " + error);
		}

		private void Socket_ConnectedCallback()
		{
			DebugLog.Log("Connected to client");
		}

		private void Socket_DisconnectedCallback()
		{
			DebugLog.Log("Disconnected from client");
			MainApplicationContext.dispatcher.InvokeAsync(delegate()
			{
				//sdsock.ReListen(); // resetAllInstanseStateでやるので不要
                resetAllInstanseState();
			});
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
