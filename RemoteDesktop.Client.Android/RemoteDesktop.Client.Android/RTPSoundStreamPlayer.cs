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
		public RTPConfiguration config = new RTPConfiguration();
		//private SoundManager.Stopwatch m_Stopwatch = new SoundManager.Stopwatch();
        private AudioDecodingPlayerManager m_DPlayer;
        private MemoryStream encoded_frame_ms;
        private MyDpcmCodec dpcmDecoder;
        private OpusDecoder concentusOpusDecoder;
        //private long totalWroteSoundData = 0;

        private void Init()
		{
            //WinSoundServer
            if (!RTPConfiguration.isUseLossySoundDecoder || RTPConfiguration.isEncodeWithOpus)
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
                sdsock.Connect(System.Net.IPAddress.Parse(RTPConfiguration.ServerAddress), config.SoundServerPort); 
			}
        }

        private void Socket_StartDataRecievedCallback(PacketHeader pktHdr)
        {
            Console.WriteLine("Socket_StartDataRecievedCallback called compressed data size is " + pktHdr.dataSize.ToString() + " bytes");
            RTPConfiguration.SamplesPerSecond = pktHdr.SamplesPerSecond;
            config.BitsPerSample = pktHdr.BitsPerSample;
            config.Channels = pktHdr.Channels;
            config.isConvertMulaw = pktHdr.isConvertMulaw;
            if (RTPConfiguration.isUseLossySoundDecoder || RTPConfiguration.isUseDPCM)
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
            if (RTPConfiguration.isUseLossySoundDecoder)
            {
                //var data = mp3data_ms.ToArray();

                encoded_frame_ms.Position = 0;
                if (m_DPlayer == null)
                {

                    //byte[] csd_data = new byte[2];
                    //mp3data_ms.Read(csd_data, 0, 2);

                    //m_DPlayer.Open("hoge", config.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
                    m_DPlayer = new AudioDecodingPlayerManager();
                    if (RTPConfiguration.isEncodeWithAAC)
                    {
                        byte[] csd_data = new byte[7];
                        encoded_frame_ms.Read(csd_data, 0, 7);
                        m_DPlayer.setup(RTPConfiguration.SamplesPerSecond, config.Channels, -1, csd_data, "aac");

                        if (!(encoded_frame_ms.Length > 7))
                        {
                            return;
                        }
                        //最初のフレームのヘッダは取り除かずに流す (rdts形式の場合)
                        encoded_frame_ms.Position = 0;

                        //m_DPlayer.setup(RTPConfiguration.SamplesPerSecond, config.Channels, -1, null);
                        Console.WriteLine("sound device opened.");
                        //m_DPlayer.mCallback.addEncodedSamplesData(csd_data, csd_data.Length);
                    }
                    else if (RTPConfiguration.isEncodeWithOpus)
                    {
                        if (m_Player.Opened == false)
                        {
                            m_Player.Open("hoge", RTPConfiguration.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
                            m_Player.Play();
                        }

                        concentusOpusDecoder = OpusDecoder.Create(RTPConfiguration.SamplesPerSecond, config.Channels);
                        //m_DPlayer.setup(RTPConfiguration.SamplesPerSecond, config.Channels, -1, csd_0, "opus");
                    }
                    else if (RTPConfiguration.isEncodeWithOggOpus)
                    {
                        // データを渡しておかないとMediaExtractor生成時に固まるので setup に先立って渡す
                        byte[] encoded_buf = new byte[encoded_frame_ms.Length];
                        encoded_frame_ms.Read(encoded_buf, 0, encoded_buf.Length);
                        Console.WriteLine(Utils.getFormatedCurrentTime() + " Socket_EndDataRecievedCallback and addEncodeSamplesData " + encoded_buf.Length.ToString() + " bytes");
                        m_DPlayer.mCallback.addEncodedSamplesData(encoded_buf, encoded_buf.Length);

                        //Device.BeginInvokeOnMainThread(() =>
                        //{
                        //    m_DPlayer.setup(RTPConfiguration.SamplesPerSecond, config.Channels, -1, null, "ogg_opus");
                        //    Console.WriteLine("ogg_opus: encoder setuped and sound device opened.");
                        //});
                        var task = Task.Run(() =>
                        {
                            m_DPlayer.setup(RTPConfiguration.SamplesPerSecond, config.Channels, -1, null, "ogg_opus");
                            Console.WriteLine("ogg_opus: encoder setuped and sound device opened.");
                        });
                        return;
                    }
                    else
                    {
                        throw new Exception("illigal flag setting on RTPConfiguration.");
                    }
                }

                byte[] data_buf = new byte[encoded_frame_ms.Length - encoded_frame_ms.Position];
                encoded_frame_ms.Read(data_buf, 0, data_buf.Length);

                if (RTPConfiguration.isEncodeWithOpus)
                {
                    int frameSize = RTPConfiguration.samplesPerPacket; // must be same as framesize used in input, you can use OpusPacketInfo.GetNumSamples() to determine this dynamically
                    short[] outputBuffer = new short[frameSize];

                    int thisFrameSize = concentusOpusDecoder.Decode(data_buf, 0, data_buf.Length, outputBuffer, 0, frameSize, false);
                    m_Player.WriteData(Utils.convertShortArrToBytes(outputBuffer), false);
                    return;
                }

                Console.WriteLine(Utils.getFormatedCurrentTime() + " Socket_EndDataRecievedCallback and addEncodeSamplesData " + data_buf.Length.ToString() + " bytes");
                m_DPlayer.mCallback.addEncodedSamplesData(data_buf, data_buf.Length);
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
                if (RTPConfiguration.isUseDPCM)
                {
                    if(dpcmDecoder == null)
                    {
                        dpcmDecoder = new MyDpcmCodec();
                    }

                    linearBytes = dpcmDecoder.Decode(linearBytes);
                }
                if (!RTPConfiguration.isUseLossySoundDecoder && m_Player.Opened == false)
                {
                    m_Player.Open("hoge", RTPConfiguration.SamplesPerSecond, config.BitsPerSample, config.Channels, config.BufferCount);
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
            if (RTPConfiguration.isUseLossySoundDecoder)
            {
                encoded_frame_ms.Write(data, 0, dataSize);
            }
            else
            {
                encoded_frame_ms.Write(data, 0, dataSize);
                //Byte[] justSound_buf = new byte[dataSize];
                //Array.Copy(data, 0, justSound_buf, 0, dataSize);
                //Byte[] linearBytes = justSound_buf;
                //if (config.isConvertMulaw)
                //{
                //    linearBytes = SoundUtils.MuLawToLinear(justSound_buf, config.BitsPerSample, config.Channels);
                //}
                //m_Player.PlayData(linearBytes, false);
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
