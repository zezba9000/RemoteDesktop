using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteDesktop.Client.Android
{
    class VideoDecoderManager
    {
        public bool Opened = false;
        IPlatformVideoDecoder pdecoder;

        public VideoDecoderManager()
        {
            pdecoder = VideoDecoderFactory.getInstance();
        }

        public bool setup(DecoderCallback callback_obj)
        {
            return pdecoder.setup(callback_obj);
        }

        public void Close()
        {
            pdecoder.Close();
            Opened = false;
        }
    }
}
