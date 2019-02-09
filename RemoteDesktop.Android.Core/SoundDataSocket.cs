using System;

using System.Net.Sockets;
using System.Net;
using System.IO;

using System.IO.Compression;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace RemoteDesktop.Android.Core.Sound
{

    [Serializable()]
    public enum PacketTypes
	{
		NotifySetting,
		SoundData,
		Reserved1,
		Reserved2
	}

    //[StructLayout(LayoutKind.Sequential)]
    [Serializable()]
	public struct PacketHeader
	{
		public PacketTypes type;
		public bool compressed;
		public int dataSize, soundDataSize;

        public int SamplesPerSecond;
        public short BitsPerSample;
        public short Channels;
        public bool isConvertMulaw;
	}

	public class SoundDataSocket : IDisposable
	{
		public delegate void ConnectionCallbackMethod();
		public delegate void DisconnectedCallbackMethod();
		public delegate void ConnectionFailedCallbackMethod(string error);
		public delegate void DataRecievedCallbackMethod(byte[] data, int dataSize, int offset);
		public delegate void StartDataRecievedCallbackMethod(PacketHeader pktHdr);
		public delegate void EndDataRecievedCallbackMethod();

		public event ConnectionCallbackMethod ConnectedCallback;
		public event DisconnectedCallbackMethod DisconnectedCallback;
		public event ConnectionFailedCallbackMethod ConnectionFailedCallback;
		public event DataRecievedCallbackMethod DataRecievedCallback;
		public event StartDataRecievedCallbackMethod StartDataRecievedCallback;
		public event EndDataRecievedCallbackMethod EndDataRecievedCallback;

		private NetworkTypes type;
		private Socket listenSocket, socket;
		private bool isDisposed = false, disconnected = false;
		private Timer disconnectionTimer;

		private byte[] receiveBuffer, sendBuffer, pktHdrBuffer;
		private int pktHdrBufferRead;
		private readonly int pktHdrSize;
		private PacketHeader pktHdr;
		private MemoryStream compressedStream;

        private const int BUF_SIZE = 2048;

		public SoundDataSocket(NetworkTypes type)
		{
			this.type = type;

			receiveBuffer = new byte[BUF_SIZE];
			sendBuffer = new byte[BUF_SIZE];
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, new PacketHeader()); // for get Binary Seriazed data size
            pktHdrSize = (int) ms.Length;
            Console.WriteLine("Serialized PacketHeader Class object binary size is berow");
            Console.WriteLine(pktHdrSize);
            Console.WriteLine(type);
			pktHdrBuffer = new byte[1024];
		}

		public void Dispose()
		{
			disconnected = true;

			// dispose timer
			if (disconnectionTimer != null)
			{
				disconnectionTimer.Dispose();
				disconnectionTimer = null;
			}

			lock (this)
			{
				isDisposed = true;

				// dispose listener socket
				if (listenSocket != null)
				{
					try
					{
						listenSocket.Shutdown(SocketShutdown.Both);
					}
					catch { }

					listenSocket.Close();
					listenSocket = null;
				}

				// dispose normal socket
				if (socket != null)
				{
					try
					{
						socket.Shutdown(SocketShutdown.Both);
					}
					catch { }

					try
					{
						if (IsConnected(socket)) socket.Disconnect(false);
					}
					catch { }

					socket.Close();
					socket.Dispose();
					socket = null;
				}

				// dispose compression stream
				if (compressedStream != null)
				{
					compressedStream.Dispose();
					compressedStream = null;
				}

				// null types
				receiveBuffer = null;
				sendBuffer = null;
				pktHdrBuffer = null;
			}
		}

        private void StartDisconnectionTimer()
        {
            disconnectionTimer = new Timer(timerTic, null, 100, 1000);
        }

        private void timerTic(object state)
        {
            lock (this)
            {
                if (isDisposed) return;

                if (!IsConnected())
                {
                    if (disconnectionTimer != null)
                    {
                        disconnectionTimer.Dispose();
                        disconnectionTimer = null;
                    }

                    pktHdrBufferRead = 0;
                    Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                    Array.Clear(sendBuffer, 0, sendBuffer.Length);
                    Array.Clear(pktHdrBuffer, 0, pktHdrBuffer.Length);

                    FireDisconnectedCallback();
                }
            }
        }

        public void Listen(IPAddress ipAddress, int port)
		{
			if (type != NetworkTypes.Server) throw new Exception("Only allowed for server!");

			lock (this)
			{
				listenSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				listenSocket.Bind(new IPEndPoint(ipAddress, port));
				listenSocket.Listen(1);
				listenSocket.BeginAccept(ConnectionEstablishedCallback, null);
			}
		}

		public void ReListen()
		{
			if (type != NetworkTypes.Server) throw new Exception("Only allowed for server!");
			lock (this)
			{
				if (listenSocket == null) throw new Exception("Must call listen before this method.");
				if (IsConnected()) throw new Exception("Socket already connected");
				listenSocket.BeginAccept(ConnectionEstablishedCallback, null);
			}
		}

		public void Connect(IPAddress ipAddress, int port)
		{
			lock (this) Connect(new IPEndPoint(ipAddress, port));
		}

		public void Connect(IPEndPoint endPoint)
		{
			lock (this)
			{
				socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.BeginConnect(endPoint, ConnectionEstablishedCallback, null);
			}
		}

		private void ConnectionEstablishedCallback(IAsyncResult ar)
		{
			lock (this)
			{
				if (isDisposed) return;
				disconnected = false;

				StartDisconnectionTimer();

				// connect
				if (type == NetworkTypes.Server)
				{
					try
					{
						socket = listenSocket.EndAccept(ar);
					}
					catch (SocketException e)
					{
						string error = string.Format("socket.EndConnect failed: {0}\n{1}", e.SocketErrorCode, e.Message);
						FireConnectionFailedCallback(error);
						DebugLog.LogError(error);
						disconnected = true;
						return;
					}
					catch (Exception e)
					{
						string error = "socket.EndConnect failed: " + e.Message;
						FireConnectionFailedCallback(error);
						DebugLog.LogError(error);
						disconnected = true;
						return;
					}

					FireConnectedCallback();
				}
				else
				{
					try
					{
						socket.EndConnect(ar);
					}
					catch (SocketException e)
					{
						string error = string.Format("socket.EndConnect failed: {0}\n{1}", e.SocketErrorCode, e.Message);
						FireConnectionFailedCallback(error);
						DebugLog.LogError(error);
						disconnected = true;
						return;
					}
					catch (Exception e)
					{
						string error = "socket.EndConnect failed: " + e.Message;
						FireConnectionFailedCallback(error);
						DebugLog.LogError(error);
						disconnected = true;
						return;
					}

					FireConnectedCallback();
				}

				// begin recieve listen
				try
				{
					socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, new ReceiveState());
				}
				catch (Exception e)
				{
					DebugLog.LogError("Failed to BeginReceive SocketConnection: " + e.Message);
					disconnected = true;
					return;
				}
			}
		}

        //private void debugPrintByteArray4ElemSpan(byte[] buf)
        //{
        //    string debugStr = "contents of buffer 4 elem span :";
        //    for (int i = 0; i < buf.Length; i += 4)
        //    {
        //        debugStr += buf[i].ToString() + ", ";
        //    }
        //    Console.WriteLine(debugStr);
        //}

		private void ReceiveBufferShiftDown(int atIndex)
		{
			for (int i = 0, i2 = atIndex; i2 < receiveBuffer.Length; ++i, ++i2)
			{
				receiveBuffer[i] = receiveBuffer[i2];
				receiveBuffer[i2] = 0;
			}
		}

		private void RecieveDataCallback(IAsyncResult ar)
		{
            lock (this)
			{
				// validate socket
				if (isDisposed || socket == null || !socket.Connected) return;

				// handle failed reads
				int bytesRead;
				try
				{
					bytesRead = socket.EndReceive(ar); // this retuened larger than buffer size? (it means total read byte size?) => maybe No
				}
				catch
				{
					return;
				}

				// write data to stream
				var state = (ReceiveState)ar.AsyncState;
				if (bytesRead > 0)
				{
					EXTRA_STREAM:;
					int overflow = 0;

					// read meta data
					if (pktHdrBufferRead < pktHdrSize)
					{
						int count = Math.Min(pktHdrSize - pktHdrBufferRead, bytesRead);
						Array.Copy(receiveBuffer, 0, pktHdrBuffer, pktHdrBufferRead, count);
						pktHdrBufferRead += count;

                        // DEBUG: if EndReceive func do not return total read bytes, calc sum code may be needed!!!
						//if (bytesRead < metaDataSize)
                        if (pktHdrBufferRead < pktHdrSize)
                            {
							try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, state);} catch {}
							return;
						}
						else
						{
							ReceiveBufferShiftDown(count);
							bytesRead -= count;
							overflow = bytesRead; // overflow and current bytesRead means bitmap data already read (if value > 0)

                            //debugPrintByteArray4ElemSpan(pktHdrBuffer);
                            BinaryFormatter bf = new BinaryFormatter();
                            pktHdr = (PacketHeader) bf.Deserialize(new MemoryStream(pktHdrBuffer));

							//if (pktHdr.dataSize == 0) throw new Exception("Invalid data size");
                            if(pktHdr.dataSize == 0)
                            {
                                Console.WriteLine("SoundDataSocket::RecieveDataCallback: pktHdr.datasize is 0 !!!");
                                pktHdrBufferRead = 0;
                                try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, new ReceiveState());} catch {}
                                return;
                            }

							// fire start callback
							FireStartDataRecievedCallback(pktHdr);

							// check if message type (if so finish and exit)
							if (pktHdr.dataSize == -1)
							{
								FireEndDataRecievedCallback();
								pktHdrBufferRead = 0;
								state = new ReceiveState();
							}
							else // server write data after PacketHeader object and read the data
							{
								state.size = pktHdr.dataSize; // bitmap data size
							}

							if (overflow > 0) // this means already read but not used data left on receive buffer
							{
								goto EXTRA_STREAM;
							}
							else // go to read yet not received data (bitmap data)
							{
								try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, state);} catch {}
								return;
							}
						}
					} // not through code from inner of this block to below!!!

                    // --- after PacketHeader object is read ---

					// read data chunk
					int offset = state.bytesRead;
					state.bytesRead += bytesRead;
					overflow = Math.Max(state.bytesRead - state.size, 0); // overflow > 0 means already read next frame data
					state.bytesRead = Math.Min(state.bytesRead, state.size);
					int byteCount = bytesRead - overflow; // calc data size of current frame on receiveBuffer
					FireDataRecievedCallback(receiveBuffer, byteCount, offset);

                    //if (state.bytesRead != state.size) // did not read all data of current frame yet
                    if (state.bytesRead < state.size) // did not read all data of current frame yet
                        {
						try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, state);} catch {}
						return;
					}
					else // already read all data of current frame
					{
						FireEndDataRecievedCallback();
					}

					
					if (overflow > 0) // process remaining data (already read next frame data)
					{
						state = new ReceiveState();
						ReceiveBufferShiftDown(bytesRead - overflow); // remove current frame data
						bytesRead = overflow;
						pktHdrBufferRead = 0;
						goto EXTRA_STREAM;
					}
					else // finish read all data of current frame. then start wait data arrive of next frame
					{
                        pktHdrBufferRead = 0;
						try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, new ReceiveState());} catch {}
						return;
					}
				}
				else // read request to socket failed (some failue should occured) 
				{
					disconnected = true;
				}
			}
		}

		//private unsafe void SendBinary(byte* data, int dataLength)
		private void SendBinary(byte[] data, int dataLength)
		{
            try
            {
                Console.WriteLine("call SendBinary func: data.Length, dataLength = " + data.Length.ToString() + ", " + dataLength.ToString());
                if (data == null || dataLength == 0) throw new Exception("Invalid data size");
                int size = dataLength, offset = 0;
                do
                {
                    int dataWrite = socket.Send(data, offset, size, SocketFlags.None);
                    if (dataWrite == 0) break;
                    offset += dataWrite;
                    size -= dataWrite;
                }
                while (size != 0);
            }catch(Exception ex){
                Console.WriteLine(ex);
            }
        }

		public void SendRTPPacket(RTPPacket packet, bool compress, int samplePerSecond, short bitsPerSample, short channel, bool isConvertMulaw)		
		{
			try
			{
				// get data length
				int dataLength, soundDataSize;
                soundDataSize = packet.Data.Length;
                dataLength = soundDataSize;

                /*
                if (compress)
                {
                    if (compressedStream == null) compressedStream = new MemoryStream();
                    else compressedStream.SetLength(0);

                    var soundStream = new MemoryStream(packet.Data);

                    using (var gzip = new GZipStream(compressedStream, CompressionMode.Compress, true))
                    {
                        soundStream.CopyTo(gzip);
                    }

                    compressedStream.Flush();
                    dataLength = (int)compressedStream.Length;
                }
                */

                // send packet header
                var pktHeader = new PacketHeader()
                {
                    type = PacketTypes.SoundData,
                    compressed = compress,
                    dataSize = dataLength,
                    soundDataSize = soundDataSize,
                    BitsPerSample = bitsPerSample,
                    Channels = channel,
                    SamplesPerSecond = samplePerSecond,
                    isConvertMulaw = isConvertMulaw
				};

				SendPacketHeaderInternal(pktHeader);

                //if (compress)
                if (false)
                {
                    SendBinary(compressedStream.GetBuffer(), dataLength);
                }
                else
                {
                    Console.WriteLine("call SendBinary for commpressed mp3 data");
                    SendBinary(packet.Data, dataLength);
                }
            }
			catch (Exception e)
			{
                Console.WriteLine(e);
			}
			finally
			{
				// do nothing
			}
		}


		//private unsafe void SendPacketHeaderInternal(PacketHeader metaData)
		private void SendPacketHeaderInternal(PacketHeader pktHdr)
		{
            Console.WriteLine("call SendPacketHeaderInternal");
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, pktHdr);
            byte[] buf = ms.ToArray();
            if(pktHdrSize != ms.Length)
            {
                throw new Exception("serialized PacketHeader binary size is not constant...");
            }
            SendBinary(buf, pktHdrSize);
		}

		public void SendPacketHeader(PacketHeader pktHdr)
		{
			try
			{
				SendPacketHeaderInternal(pktHdr);
			}
			catch {}
		}

		private static bool IsConnected(Socket socket)
		{
			if (socket == null || !socket.Connected) return false;

            return true;
			//try
			//{
			//	return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
			//}
			//catch
			//{
			//	return false;
			//}
		}

		public bool IsConnected()
		{
			lock (this)
			{
				return !disconnected && IsConnected(socket);
			}
		}

		private void FireConnectionFailedCallback(string error)
		{
			if (ConnectionFailedCallback != null) ConnectionFailedCallback(error);
		}

		private void FireConnectedCallback()
		{
			if (ConnectedCallback != null) ConnectedCallback();
		}

		private void FireDisconnectedCallback()
		{
			if (DisconnectedCallback != null) DisconnectedCallback();
		}

		private void FireDataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			if (DataRecievedCallback != null) DataRecievedCallback(data, dataSize, offset);
		}

		private void FireStartDataRecievedCallback(PacketHeader pktHdr)
		{
			if (StartDataRecievedCallback != null) StartDataRecievedCallback(pktHdr);
		}

		private void FireEndDataRecievedCallback()
		{
			if (EndDataRecievedCallback != null) EndDataRecievedCallback();
		}
	}
}
