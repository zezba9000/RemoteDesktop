using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;

namespace NAudioDemo.Services.Interfaces
{

    public interface IAudioService
    {

        MMDeviceCollection GetActiveRender();

    }

}
