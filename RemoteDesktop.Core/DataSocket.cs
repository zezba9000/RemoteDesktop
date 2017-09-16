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

namespace RemoteDesktop.Core
{
	struct ReceiveState
	{
		public int size, bytesRead;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MetaData
	{
		public int size;
		public int width, height;
		public PixelFormat format;
	}

	public class DataSocket : IDisposable
	{
		public delegate void ConnectionCallbackMethod();
		public delegate void ConnectionFailedCallbackMethod(string error);
		public delegate void DataRecievedCallbackMethod(byte[] data, int dataSize, int offset);
		public delegate void StartDataRecievedCallbackMethod(MetaData metaData);
		public delegate void EndDataRecievedCallbackMethod();

		public event ConnectionCallbackMethod ConnectedCallback;
		public event ConnectionFailedCallbackMethod ConnectionFailedCallback;
		public event DataRecievedCallbackMethod DataRecievedCallback;
		public event StartDataRecievedCallbackMethod StartDataRecievedCallback;
		public event EndDataRecievedCallbackMethod EndDataRecievedCallback;

		private NetworkTypes type;
		private Dispatcher dispatcher;
		private Socket listenSocket, socket;
		private bool isDisposed, disconnected;
		
		private byte[] receiveBuffer, sendBuffer, metaDataSizeBuffer, metaDataBuffer;
		private int segmentSizeBufferRead;
		private readonly int metaDataSize;
		private MetaData metaData;

		public DataSocket(NetworkTypes type, Dispatcher dispatcher)
		{
			this.type = type;
			this.dispatcher = dispatcher;
			
			receiveBuffer = new byte[1024];
			sendBuffer = new byte[1024];
			metaDataSize = Marshal.SizeOf<MetaData>();
			metaDataSizeBuffer = new byte[metaDataSize];
			metaDataBuffer = new byte[metaDataSize];
		}

		public void Dispose()
		{
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

				// null types
				receiveBuffer = null;
				metaDataSizeBuffer = null;
				metaDataBuffer = null;
			}
		}

		public void Listen(IPAddress ipAddress, int port)
		{
			if (type != NetworkTypes.Server) throw new Exception("Only allowed for server!");

			listenSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listenSocket.Bind(new IPEndPoint(ipAddress, port));
			listenSocket.Listen(1);
			listenSocket.BeginAccept(ConnectionEstablishedCallback, null);
		}

		public void Connect(IPAddress ipAddress, int port)
		{
			Connect(new IPEndPoint(ipAddress, port));
		}

		public void Connect(IPEndPoint endPoint)
		{
			socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.BeginConnect(endPoint, ConnectionEstablishedCallback, null);
		}

		private void ConnectionEstablishedCallback(IAsyncResult ar)
		{
			lock (this)
			{
				if (isDisposed) return;

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
						return;
					}
					catch (Exception e)
					{
						string error = "socket.EndConnect failed: " + e.Message;
						FireConnectionFailedCallback(error);
						DebugLog.LogError(error);
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
						return;
					}
					catch (Exception e)
					{
						string error = "socket.EndConnect failed: " + e.Message;
						FireConnectionFailedCallback(error);
						DebugLog.LogError(error);
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
							socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, state);
							return;
						}
						else
						{
							ReceiveBufferShiftDown(count);
							bytesRead -= count;
						}
					}

					// read full segment size or conintue to write normal data chunks
					int overflow = 0;
					if (state.size == 0)
					{
						// create meta data object
						var handle = GCHandle.Alloc(metaDataSizeBuffer, GCHandleType.Pinned);
						metaData = Marshal.PtrToStructure<MetaData>(handle.AddrOfPinnedObject());
						handle.Free();
						FireStartDataRecievedCallback(metaData);

						// get data size
						state.size = metaData.size;//BitConverter.ToInt64(metaDataSizeBuffer, 0);
						if (state.size == 0) throw new Exception("Invalid chunk size");
					
						state.bytesRead += bytesRead;
						overflow = (int)(state.bytesRead - state.size);
						state.bytesRead = Math.Min(state.bytesRead, state.size);
						//receiveStream.Write(receiveBuffer, 0, (int)state.bytesRead);
						FireDataRecievedCallback(receiveBuffer, (int)state.bytesRead, 0);
					}
					else
					{
						int offset = state.bytesRead;
						state.bytesRead += bytesRead;
						overflow = (int)(state.bytesRead - state.size);
						state.bytesRead = Math.Min(state.bytesRead, state.size);
						int byteCount = (overflow > 0) ? bytesRead - overflow : bytesRead;
						//receiveStream.Write(receiveBuffer, 0, byteCount);
						FireDataRecievedCallback(receiveBuffer, byteCount, offset);
					}

					// check if stream segment finished
					if (state.bytesRead != state.size)
					{
						Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
						socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, state);
					}

					// check overflow for additional stream segment
					else if (overflow <= 0)
					{
						FireEndDataRecievedCallback();
						Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
						segmentSizeBufferRead = 0;
						socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, new ReceiveState());
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
					/*else if (receiveStream.Length != 0)
					{
						// finish current stream
						//receiveStream.Flush();
						//receiveStream.Position = 0;
						//FireDataRecievedCallback(receiveStream);
						//receiveStream.SetLength(0);

						// check overflow for additional stream segment
						if (overflow <= 0)
						{
							Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
							segmentSizeBufferRead = 0;
							socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, new ReceiveState());
						}
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
						throw new Exception("Invalid buffer state");
					}*/
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
		
		public unsafe void SendImage(Bitmap bitmap, int width, int height, PixelFormat format)
		{
			try
			{
				// send meta data
				int dataLength = bitmap.Width * bitmap.Height * 3;
				var metaData = new MetaData()
				{
					size = dataLength,
					width = width,
					height = height,
					format = format
				};

				var binaryMetaData = (byte*)&metaData;
				for (int i = 0; i != metaDataSize; ++i) metaDataBuffer[i] = binaryMetaData[i];
				SendBinary(metaDataBuffer);
				
				// send bitmap data
				var locked = bitmap.LockBits(Rectangle.Empty, ImageLockMode.ReadOnly, bitmap.PixelFormat);
				var data = (byte*)locked.Scan0;
				SendBinary(data, dataLength);
				bitmap.UnlockBits(locked);
			}
			catch {}
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
			if (dispatcher.CheckAccess())
			{
				if (ConnectionFailedCallback != null) ConnectionFailedCallback(error);
			}
			else
			{
				dispatcher.InvokeAsync(delegate()
				{
					if (ConnectionFailedCallback != null) ConnectionFailedCallback(error);
				});
			}
		}

		private void FireConnectedCallback()
		{
			if (dispatcher.CheckAccess())
			{
				if (ConnectedCallback != null) ConnectedCallback();
			}
			else
			{
				dispatcher.InvokeAsync(delegate()
				{
					if (ConnectedCallback != null) ConnectedCallback();
				});
			}
		}

		private void FireDataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			if (DataRecievedCallback != null) DataRecievedCallback(data, dataSize, offset);
		}

		private void FireStartDataRecievedCallback(MetaData metaData)
		{
			if (dispatcher.CheckAccess())
			{
				if (StartDataRecievedCallback != null) StartDataRecievedCallback(metaData);
			}
			else
			{
				dispatcher.InvokeAsync(delegate()
				{
					if (StartDataRecievedCallback != null) StartDataRecievedCallback(metaData);
				});
			}
		}

		private void FireEndDataRecievedCallback()
		{
			if (dispatcher.CheckAccess())
			{
				if (EndDataRecievedCallback != null) EndDataRecievedCallback();
			}
			else
			{
				dispatcher.InvokeAsync(delegate()
				{
					if (EndDataRecievedCallback != null) EndDataRecievedCallback();
				});
			}
		}
	}
}
