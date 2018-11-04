using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudioDemo.Services.Interfaces;

namespace NAudioDemo.Services
{

    public sealed class AudioService : IAudioService
    {

        public MMDeviceCollection GetActiveRender()
        {
            var collection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            return collection;
        }

    }

}
