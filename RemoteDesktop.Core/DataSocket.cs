using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO.Compression;
using System.Threading;

namespace RemoteDesktop.Core
{
	struct ReceiveState
	{
		public int size, bytesRead;
	}

	public enum MetaDataTypes
	{
		None,
		UpdateSettings,
		StartCapture,
		PauseCapture,
		ResumeCapture,
		ImageData
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MetaData
	{
		public MetaDataTypes type;
		public bool compressed;
		public int dataSize, imageDataSize;
		public int width, height, screenIndex;
		public PixelFormat format;
	}

	public class DataSocket : IDisposable
	{
		public delegate void ConnectionCallbackMethod();
		public delegate void DisconnectedCallbackMethod();
		public delegate void ConnectionFailedCallbackMethod(string error);
		public delegate void DataRecievedCallbackMethod(byte[] data, int dataSize, int offset);
		public delegate void StartDataRecievedCallbackMethod(MetaData metaData);
		public delegate void EndDataRecievedCallbackMethod();

		public event ConnectionCallbackMethod ConnectedCallback;
		public event DisconnectedCallbackMethod DisconnectedCallback;
		public event ConnectionFailedCallbackMethod ConnectionFailedCallback;
		public event DataRecievedCallbackMethod DataRecievedCallback;
		public event StartDataRecievedCallbackMethod StartDataRecievedCallback;
		public event EndDataRecievedCallbackMethod EndDataRecievedCallback;

		private NetworkTypes type;
		private Socket listenSocket, socket;
		private bool isDisposed, disconnected;
		private Timer disconnectionTimer;
		
		private byte[] receiveBuffer, sendBuffer, metaDataSizeBuffer, metaDataBuffer;
		private int segmentSizeBufferRead;
		private readonly int metaDataSize;
		private MetaData metaData;

		public DataSocket(NetworkTypes type)
		{
			this.type = type;
			
			receiveBuffer = new byte[1024];
			sendBuffer = new byte[1024];
			metaDataSize = Marshal.SizeOf<MetaData>();
			metaDataSizeBuffer = new byte[metaDataSize];
			metaDataBuffer = new byte[metaDataSize];
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

				// null types
				receiveBuffer = null;
				metaDataSizeBuffer = null;
				metaDataBuffer = null;
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

				// start diconnection timer
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
					bytesRead = socket.EndReceive(ar);
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

					// read meta data
					if (segmentSizeBufferRead < metaDataSize)
					{
						int count = Math.Min(metaDataSize - segmentSizeBufferRead, bytesRead);
						Array.Copy(receiveBuffer, 0, metaDataSizeBuffer, segmentSizeBufferRead, count);
						segmentSizeBufferRead += count;
					
						if (bytesRead < metaDataSize)
						{
							Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
							try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, state);} catch {}
							return;
						}
						else
						{
							ReceiveBufferShiftDown(count);
							bytesRead -= count;

							// create meta data object
							var handle = GCHandle.Alloc(metaDataSizeBuffer, GCHandleType.Pinned);
							metaData = Marshal.PtrToStructure<MetaData>(handle.AddrOfPinnedObject());
							handle.Free();

							// check if message type (if so finish and exit)
							if (metaData.dataSize == -1)
							{
								FireStartDataRecievedCallback(metaData);
								FireEndDataRecievedCallback();
								Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
								segmentSizeBufferRead = 0;
								try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, new ReceiveState());} catch {}
								return;
							}
						}
					}

					// read full segment size or conintue to write normal data chunks
					int overflow = 0;
					if (state.size == 0)
					{
						FireStartDataRecievedCallback(metaData);

						// get data size
						state.size = metaData.dataSize;
						if (state.size == 0) throw new Exception("Invalid chunk size");
					
						state.bytesRead += bytesRead;
						overflow = state.bytesRead - state.size;
						state.bytesRead = Math.Min(state.bytesRead, state.size);
						FireDataRecievedCallback(receiveBuffer, state.bytesRead, 0);
					}
					else
					{
						int offset = state.bytesRead;
						state.bytesRead += bytesRead;
						overflow = state.bytesRead - state.size;
						state.bytesRead = Math.Min(state.bytesRead, state.size);
						int byteCount = (overflow > 0) ? bytesRead - overflow : bytesRead;
						FireDataRecievedCallback(receiveBuffer, byteCount, offset);
					}

					// check if stream segment finished
					if (state.bytesRead != state.size)
					{
						Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
						try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, state);} catch {}
					}

					// check overflow for additional stream segment
					else if (overflow <= 0)
					{
						FireEndDataRecievedCallback();
						Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
						segmentSizeBufferRead = 0;
						try {socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, new ReceiveState());} catch {}
					}

					// process remaining data
					else
					{
						state = new ReceiveState();
						ReceiveBufferShiftDown(bytesRead - overflow);
						bytesRead = overflow;
						segmentSizeBufferRead = 0;
						goto EXTRA_STREAM;
					}
				}
				else
				{
					disconnected = true;
				}
			}
		}

		private void SendBinary(byte[] data)
		{
			if (data == null || data.Length == 0) throw new Exception("Invalid data size");
			int size = data.Length, offset = 0;
			do
			{
				int dataRead = socket.Send(data, offset, size, SocketFlags.None);
				if (dataRead == 0) break;
				offset += dataRead;
				size -= dataRead;
			}
			while (size != 0);
		}

		private unsafe void SendBinary(byte* data, int dataLength)
		{
			if (data == null || dataLength == 0) throw new Exception("Invalid data size");
			int size = dataLength, offset = 0;
			do
			{
				int writeSize = (size <= sendBuffer.Length) ? size : sendBuffer.Length;
				for (int i = 0; i != writeSize; ++i) sendBuffer[i] = data[i + offset];
				int dataRead = socket.Send(sendBuffer, 0, writeSize, SocketFlags.None);
				if (dataRead == 0) break;
				offset += dataRead;
				size -= dataRead;
			}
			while (size != 0);
		}
		
		public unsafe void SendImage(Bitmap bitmap, int screenIndex, bool compress)
		{
			BitmapData locked = null;
			MemoryStream compressedStream = null;
			try
			{
				// get data length
				int dataLength, imageDataSize;
				switch (bitmap.PixelFormat)
				{
					case PixelFormat.Format24bppRgb: imageDataSize = bitmap.Width * bitmap.Height * 3; break;
					case PixelFormat.Format16bppRgb565: imageDataSize = ((bitmap.Width * bitmap.Height * 16) / 8); break;
					default: throw new Exception("Unsuported format: " + bitmap.PixelFormat);
				}

				dataLength = imageDataSize;

				// lock bitmap
				locked = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

				// compress if needed
				if (compress)
				{
					compressedStream = new MemoryStream();
					using (var bitmapStream = new UnmanagedMemoryStream((byte*)locked.Scan0, dataLength))
					using (var gzip = new GZipStream(compressedStream, CompressionMode.Compress, true))
					{
						bitmapStream.CopyTo(gzip);
					}

					compressedStream.Flush();
					dataLength = (int)compressedStream.Length;
				}

				// send meta data
				var metaData = new MetaData()
				{
					type = MetaDataTypes.ImageData,
					compressed = compress,
					dataSize = dataLength,
					imageDataSize = imageDataSize,
					width = bitmap.Width,
					height = bitmap.Height,
					screenIndex = screenIndex,
					format = bitmap.PixelFormat
				};
				
				SendMetaData(metaData);

				// send bitmap data
				if (compress)
				{
					compressedStream.Position = 0;
					SendBinary(compressedStream.ToArray());// TODO: send stream directly
				}
				else
				{
					var data = (byte*)locked.Scan0;
					SendBinary(data, dataLength);
				}
			}
			catch
			{
				// do nothing...
			}
			finally
			{
				if (locked != null) bitmap.UnlockBits(locked);
				if (compressedStream != null) compressedStream.Dispose();
			}
		}

		public unsafe void SendMetaData(MetaData metaData)
		{
			var binaryMetaData = (byte*)&metaData;
			for (int i = 0; i != metaDataSize; ++i) metaDataBuffer[i] = binaryMetaData[i];
			SendBinary(metaDataBuffer);
		}

		private static bool IsConnected(Socket socket)
		{
			if (socket == null || !socket.Connected) return false;

			try
			{
				return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
			}
			catch
			{
				return false;
			}
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
			if (disconnectionTimer != null)
			{
				disconnectionTimer.Dispose();
				disconnectionTimer = null;
			}

			if (DisconnectedCallback != null) DisconnectedCallback();
		}

		private void FireDataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			if (DataRecievedCallback != null) DataRecievedCallback(data, dataSize, offset);
		}

		private void FireStartDataRecievedCallback(MetaData metaData)
		{
			if (StartDataRecievedCallback != null) StartDataRecievedCallback(metaData);
		}

		private void FireEndDataRecievedCallback()
		{
			if (EndDataRecievedCallback != null) EndDataRecievedCallback();
		}
	}
}
