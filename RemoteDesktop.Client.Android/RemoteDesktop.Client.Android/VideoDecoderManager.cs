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

        public bool setup(DecoderCallback callback_obj, int width, int height)
        {
            return pdecoder.setup(callback_obj, width, height);
        }

        public void Close()
        {
            pdecoder.Close();
            pdecoder = null;
            Opened = false;
        }
    }
}
