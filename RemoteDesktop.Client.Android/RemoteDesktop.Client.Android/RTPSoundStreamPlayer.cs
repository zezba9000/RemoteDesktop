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

namespace RemoteDesktop.Client.Android
{
	public class RTPSoundStreamPlayer
	{

		public RTPSoundStreamPlayer()
		{
			Init();
		}

		RTPReceiver m_Receiver;
        SoundDataSocket sdsock;
		SoundManager.Player m_Player;
		public RTPConfiguration config = new RTPConfiguration();
		private SoundManager.Stopwatch m_Stopwatch = new SoundManager.Stopwatch();
        private AudioDecodingPlayerManager m_DPlayer;
        private MemoryStream mp3data_ms;

        private void Init()
		{
            //WinSoundServer
			//m_Player = new SoundManager.Player();
		}

        private void OnDataReceivedUDP(RTPReceiver rtr, Byte[] bytes)
		{
			try
			{
				//Wenn der Player gestartet wurde
				if (m_Player.Opened && m_Receiver.Connected)
				{
					//RTP Header auslesen
					RTPPacket rtp = new RTPPacket(bytes);
                    Console.WriteLine("RTP Packet received: " + bytes.Length.ToString() + " bytes");

					if (rtp.Data != null)
					{
                        Byte[] linearBytes = SoundUtils.MuLawToLinear(rtp.Data, config.BitsPerSample, config.Channels);
                        //Byte[] linearBytes = rtp.Data;

                        Console.WriteLine("call PlayData func at OnDataReceived: " + linearBytes.Length.ToString() + "bytes");
                        m_Player.PlayData(linearBytes, false);
					}
				}
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex);
			}
		}

		/// <summary>
		/// OnDisconnected
		/// </summary>
		private void OnDisconnectedUDP(string reason)
		{
			try
			{
				m_Player.Close();
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex);
			}
		}

        public void togglePlayingUDP()
        {
			if (m_Player.Opened)
            {
				m_Receiver.Disconnect();
				m_Player.Close();
			}
			else
			{
                m_Player.Open("hoge", config.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);

                // 1 to 1 Receivr over UDP
                config.PacketSize = SoundUtils.GetBytesPerInterval((uint)config.SamplesPerSecond, config.BitsPerSample, config.Channels);
				m_Receiver = new RTPReceiver(config.PacketSize);
				m_Receiver.DataReceived2 += new RTPReceiver.DelegateDataReceived2(OnDataReceivedUDP);
				m_Receiver.Disconnected += new RTPReceiver.DelegateDisconnected(OnDisconnectedUDP);
				m_Receiver.Connect(RTPConfiguration.ServerAddress, config.SoundServerPort);
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
                //m_Player.Open("hoge", config.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);

                sdsock = new SoundDataSocket(NetworkTypes.Client);
                sdsock.ConnectedCallback += Socket_ConnectedCallback;
                sdsock.DisconnectedCallback += Socket_DisconnectedCallback;
                sdsock.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
                sdsock.DataRecievedCallback += Socket_DataRecievedCallback;
                sdsock.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
                sdsock.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
                sdsock.Connect(System.Net.IPAddress.Parse(RTPConfiguration.ServerAddress), config.SoundServerPort); 
			}
        }

        private void Socket_StartDataRecievedCallback(PacketHeader pktHdr)
        {
            // TCPの場合のみこのタイミングまでサウンドデバイスのOpenを遅らせる
            config.SamplesPerSecond = pktHdr.SamplesPerSecond;
            config.BitsPerSample = pktHdr.BitsPerSample;
            config.Channels = pktHdr.Channels;
            config.isConvertMulaw = pktHdr.isConvertMulaw;
            if (m_DPlayer == null)
            {
                //m_DPlayer.Open("hoge", config.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
                m_DPlayer = new AudioDecodingPlayerManager();
                m_DPlayer.setup(config.SamplesPerSecond, config.Channels, -1);
                Console.WriteLine("sound device opened.");
            }
            
            if(mp3data_ms == null)
            {
                mp3data_ms = new MemoryStream();
            }
            else
            {
                mp3data_ms.Position = 0;
            }

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    if (!m_Player.Opened)
            //    {
            //        m_Player.Open("hoge", config.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
            //    }

            //});
        }


        private void Socket_EndDataRecievedCallback()
        {
            var data = mp3data_ms.ToArray();
            m_DPlayer.mCallback.addEncodedSamplesData(data, data.Length);
        }


        private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
        {
            Console.WriteLine("Socket_DataRecievedCallback: recieved sound data = " + dataSize.ToString());
            /*
            Byte[] justSound_buf = new byte[dataSize];
            Array.Copy(data, 0, justSound_buf, 0, dataSize);
            Byte[] linearBytes = justSound_buf;
            if (config.isConvertMulaw)
            {
                linearBytes = SoundUtils.MuLawToLinear(justSound_buf, config.BitsPerSample, config.Channels);
            }
            m_Player.PlayData(linearBytes, false);
            */
            mp3data_ms.Write(data, offset, dataSize);
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
