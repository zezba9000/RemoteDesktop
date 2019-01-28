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
using RemoteDesktop.Android.Core;

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
            _AudioOutputWriter = new AudioOutputWriter(devices.First(), new RTPConfiguration());
        }

        //private void startDataRecieveCaptureSound()
        //{
        //        try
        //        {
        //        //this.Stop();

        //        //var audioService = new AudioService();
        //        //this._AudioOutputWriter = new AudioOutputWriter(device);
        //        this._AudioOutputWriter.Start();
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //}

        private void StoptDataRecieveCaptureSound()
        {
            if (this._AudioOutputWriter != null)
            {
                this._AudioOutputWriter.Dispose();
            }
        }
    }
}
