using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using RemoteDesktop.Android.Core;
using RemoteDesktop.Android.Core.Sound;
using Xamarin.Forms;
using System.Threading.Tasks;
using Concentus.Structs;

namespace RemoteDesktop.Client.Android
{
	public class RTPSoundStreamPlayer
	{

		public RTPSoundStreamPlayer()
		{
			Init();
		}

		//RTPReceiver m_Receiver;
        SoundDataSocket sdsock;
		SoundManager.Player m_Player;
		public GlobalConfiguration config = new GlobalConfiguration();
		//private SoundManager.Stopwatch m_Stopwatch = new SoundManager.Stopwatch();
        private MemoryStream encoded_frame_ms;
        private MyDpcmCodec dpcmDecoder;
        private OpusDecoder concentusOpusDecoder;
        //private long totalWroteSoundData = 0;

        private void Init()
		{
            //WinSoundServer
            if (!GlobalConfiguration.isUseLossySoundDecoder || GlobalConfiguration.isEncodeWithOpus)
            {
                m_Player = new SoundManager.Player();
            }
		}

        public void togglePlayingTCP()
        {
			if (sdsock != null)
            {
                sdsock.Dispose();
                sdsock = null;
				m_Player.Close();
			}
			else
			{
                sdsock = new SoundDataSocket(NetworkTypes.Client);
                sdsock.ConnectedCallback += Socket_ConnectedCallback;
                sdsock.DisconnectedCallback += Socket_DisconnectedCallback;
                sdsock.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
                sdsock.DataRecievedCallback += Socket_DataRecievedCallback;
                sdsock.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
                sdsock.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
                sdsock.Connect(System.Net.IPAddress.Parse(GlobalConfiguration.ServerAddress), config.SoundServerPort); 
			}
        }

        private void Socket_StartDataRecievedCallback(PacketHeader pktHdr)
        {
            Console.WriteLine("Socket_StartDataRecievedCallback called compressed data size is " + pktHdr.dataSize.ToString() + " bytes");
            GlobalConfiguration.SamplesPerSecond = pktHdr.SamplesPerSecond;
            config.BitsPerSample = pktHdr.BitsPerSample;
            config.Channels = pktHdr.Channels;
            config.isConvertMulaw = pktHdr.isConvertMulaw;
            if (GlobalConfiguration.isUseLossySoundDecoder || GlobalConfiguration.isUseDPCM)
            {
                if (encoded_frame_ms == null)
                {
                    encoded_frame_ms = new MemoryStream();
                }
                else
                {
                    encoded_frame_ms.Position = 0;
                    encoded_frame_ms.SetLength(0);
                }
            }
            else
            {
                //Device.BeginInvokeOnMainThread(() =>
                //{
                //    if (!m_Player.Opened)
                //    {
                //        m_Player.Open("hoge", RTPConfiguration.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
                //    }
                //});
            }
        }


        private void Socket_EndDataRecievedCallback()
        {
            if (GlobalConfiguration.isUseLossySoundDecoder)
            {
                //var data = mp3data_ms.ToArray();

                encoded_frame_ms.Position = 0;
                if (m_Player.Opened == false)
                {
                    if (GlobalConfiguration.isEncodeWithOpus)
                    {
                        m_Player.Open("hoge", GlobalConfiguration.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
                        m_Player.Play();

                        concentusOpusDecoder = OpusDecoder.Create(GlobalConfiguration.SamplesPerSecond, config.Channels);
                        //m_DPlayer.setup(RTPConfiguration.SamplesPerSecond, config.Channels, -1, csd_0, "opus");
                    }
                    else
                    {
                        throw new Exception("illigal flag setting on RTPConfiguration.");
                    }
                }

                byte[] data_buf = new byte[encoded_frame_ms.Length - encoded_frame_ms.Position];
                encoded_frame_ms.Read(data_buf, 0, data_buf.Length);

                int frameSize = GlobalConfiguration.samplesPerPacket; // must be same as framesize used in input, you can use OpusPacketInfo.GetNumSamples() to determine this dynamically
                short[] outputBuffer = new short[frameSize];

                int thisFrameSize = concentusOpusDecoder.Decode(data_buf, 0, data_buf.Length, outputBuffer, 0, frameSize, false);
                m_Player.WriteData(Utils.convertShortArrToBytes(outputBuffer), false);
                return;

            }
            else
            {
                Byte[] justSound_buf = encoded_frame_ms.ToArray();
                Byte[] linearBytes = justSound_buf;
                //if (!RTPConfiguration.isUseSoundDecoder && m_Player.Opened == false)
                //{
                //    m_Player.Open("hoge", RTPConfiguration.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
                //}
                if (config.isConvertMulaw)
                {
                    linearBytes = SoundUtils.MuLawToLinear(justSound_buf, config.BitsPerSample, config.Channels);
                }
                if (GlobalConfiguration.isUseDPCM)
                {
                    if(dpcmDecoder == null)
                    {
                        dpcmDecoder = new MyDpcmCodec();
                    }

                    linearBytes = dpcmDecoder.Decode(linearBytes);
                }
                if (!GlobalConfiguration.isUseLossySoundDecoder && m_Player.Opened == false)
                {
                    m_Player.Open("hoge", GlobalConfiguration.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
                    m_Player.Play();
                }
                m_Player.WriteData(linearBytes, false);
                //totalWroteSoundData += linearBytes.Length;
                //if(totalWroteSoundData > config.BufferCount && m_Player.isPlayingStarted == false)
                //{
                //    m_Player.Play();
                //}
            }
        }


        private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset /* do not use this value */)
        {
            Console.WriteLine("Socket_DataRecievedCallback: recieved sound data = " + dataSize.ToString());
            if (GlobalConfiguration.isUseLossySoundDecoder)
            {
                encoded_frame_ms.Write(data, 0, dataSize);
            }
            else
            {
                encoded_frame_ms.Write(data, 0, dataSize);
            }
        }

        private void Socket_ConnectionFailedCallback(string error)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                sdsock.Dispose();
                sdsock = null;
            });
        }

        private void Socket_ConnectedCallback()
        {
        }

        private void Socket_DisconnectedCallback()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                sdsock.Dispose();
                sdsock = null;
            });
        }
        
	}
}
