using NAudio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteDesktop.Server.XamaOK
{

    class CaptureSoundStreamer
    {
        private AudioOutputWriter _AudioOutputWriter;
        private AudioService aservice;

        // this call block until recieved client connection request
        public CaptureSoundStreamer()
        {
            aservice = new AudioService();
            var devices = aservice.GetActiveRender();            
            _AudioOutputWriter = new AudioOutputWriter(devices.First());
            this._AudioOutputWriter.DataAvailable += this.AudioOutputWriterOnDataAvailable;
        }

        private void AudioOutputWriterOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            //dispatcher.BeginInvoke(new Action(() =>
            //{
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd hh:mm:ss.fff} : {waveInEventArgs.BytesRecorded} bytes");
            //}));
        }

        private void startDataRecieveCaptureSound()
        {
                try
                {
                //this.Stop();

                //var audioService = new AudioService();
                //this._AudioOutputWriter = new AudioOutputWriter(device);
                this._AudioOutputWriter.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
        }

        private void StoptDataRecieveCaptureSound()
        {
            if (this._AudioOutputWriter != null)
            {
                this._AudioOutputWriter.DataAvailable -= this.AudioOutputWriterOnDataAvailable;

                if (this._AudioOutputWriter.IsRecording)
                {
                    this._AudioOutputWriter.Stop();
                }

                this._AudioOutputWriter.Dispose();
            }
        }
    }
}
