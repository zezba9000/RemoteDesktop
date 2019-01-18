using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{

    public class DecoderCallback
    {

        public void OnDecodeFrame(byte[] frame_data)
        {
            Console.WriteLine("OnDecodeFrame callback called!");
            // TODO: implement
        }
    }


    public interface IPlatformVideoDecoder
    {
        bool setup(DecoderCallback callback_obj);
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
