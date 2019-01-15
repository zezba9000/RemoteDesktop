using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{

    public interface IPlatformVideoDecoder
    {
        bool setup(byte[] format_hint);
        byte[] getDecodedFrame();
        void addEncodedFrame(byte[] frame_data);
        void Close();
    }

    public static class VideoDecoderFactory
    {
        public static IPlatformVideoDecoder getInstance()
        {
            return DependencyService.Get<IPlatformVideoDecoder>();
        }
    }
}
