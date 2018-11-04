using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace NAudioDemo.Models
{

    public sealed class AudioOutputWriter : IDisposable
    {

        #region イベント

        public event EventHandler<WaveInEventArgs> DataAvailable;

        #endregion

        #region フィールド

        private readonly string _FileName;

        private WasapiLoopbackCapture _WaveIn;

        private Stream _Stream;

        private WaveFileWriter _WaveFileWriter;

        #endregion

        #region コンストラクタ

        public AudioOutputWriter(string fileName, MMDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            this._FileName = fileName;
            this._WaveIn = new WasapiLoopbackCapture(device);
            this._WaveIn.DataAvailable += this.WaveInOnDataAvailable;
            this._WaveIn.RecordingStopped += this.WaveInOnRecordingStopped;

            this._Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
            this._WaveFileWriter = new WaveFileWriter(this._Stream, this._WaveIn.WaveFormat);

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

            if (this._Stream != null)
            {
                this._Stream.Close();
                this._Stream = null;
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
            this._Stream?.Dispose();
        }

        #endregion

    }

}
