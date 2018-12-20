using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using RemoteDesktop.Server.XamaOK;
using RemoteDesktop.Core;

namespace NAudio
{

    public sealed class AudioOutputWriter : IDisposable
    {

        #region イベント

        public event EventHandler<WaveInEventArgs> DataAvailable;

        #endregion

        #region フィールド

        //private readonly string _FileName;

        private WasapiLoopbackCapture _WaveIn;

        private UDPSender usender;

        private WaveFileWriter _WaveFileWriter;

        #endregion

        #region コンストラクタ

        // this call block until recieved client connection request
        public AudioOutputWriter(MMDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

//            this._FileName = fileName;
            this._WaveIn = new WasapiLoopbackCapture(device);
            this._WaveIn.DataAvailable += this.WaveInOnDataAvailable;
            this._WaveIn.RecordingStopped += this.WaveInOnRecordingStopped;

            usender = new UDPSender(RemoteDesktop.Core.Utils.getLocalIP().ToString(), 10000, 10);
            //this._Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
            this._WaveFileWriter = new WaveFileWriter(usender, this._WaveIn.WaveFormat);

            Start(); // start capture and send stream
        }

        #endregion

        #region プロパティ

        public bool IsRecording
        {
            get;
            private set;
        }

        #endregion

        #region メソッド

        public void Start()
        {
            this.IsRecording = true;
            this._WaveIn.StartRecording();

        }

        public void Stop()
        {
            this.IsRecording = false;
            this._WaveIn.StopRecording();
        }

        #region オーバーライド
        #endregion

        #region イベントハンドラ

        private void WaveInOnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (this._WaveFileWriter != null)
            {
                this._WaveFileWriter.Close();
                this._WaveFileWriter = null;
            }

            if (this.usender != null)
            {
                this.usender.Close();
                this.usender = null;
            }

            this.Dispose();
        }

        private void WaveInOnDataAvailable(object sender, WaveInEventArgs e)
        {
            this._WaveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            this.DataAvailable?.Invoke(this, e);
        }

        #endregion

        #region ヘルパーメソッド
        #endregion

        #endregion

        #region IDisposable メンバー

        public void Dispose()
        {
            this._WaveIn.DataAvailable -= this.WaveInOnDataAvailable;
            this._WaveIn.RecordingStopped -= this.WaveInOnRecordingStopped;

            this._WaveIn?.Dispose();
            this._WaveFileWriter?.Dispose();
            this.usender?.Dispose();
        }

        #endregion

    }

}
