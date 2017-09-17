using RemoteDesktop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace RemoteDesktop.Client
{
	enum UIStates
	{
		Stopped,
		Streaming,
		Paused
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private NetworkDiscovery networkDiscovery;
		private DataSocket socket;
		private WriteableBitmap bitmap;
		private IntPtr bitmapBackbuffer;
		private MetaData metaData;
		private MemoryStream gzipStream;
		private bool skipImageUpdate;
		private UIStates uiState = UIStates.Stopped;

		public MainWindow()
		{
			InitializeComponent();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (networkDiscovery != null)
			{
				networkDiscovery.Dispose();
				networkDiscovery = null;
			}

			lock (this)
			{
				if (socket != null)
				{
					socket.Dispose();
					socket = null;
				}
			}

			if (gzipStream != null)
			{
				gzipStream.Dispose();
				gzipStream = null;
			}

			base.OnClosing(e);
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			skipImageUpdate = true;
			base.OnRenderSizeChanged(sizeInfo);
		}

		protected override void OnLocationChanged(EventArgs e)
		{
			skipImageUpdate = true;
			base.OnLocationChanged(e);
		}

		private void Refresh()
		{
			networkDiscovery = new NetworkDiscovery(NetworkTypes.Client);
			var hosts = networkDiscovery.Find("SimpleRemoteDesktop");
			Dispatcher.InvokeAsync(delegate()
			{
				foreach (var host in hosts)
				{
					serverComboBox.Items.Add(host);
				}

				if (hosts.Count != 0) serverComboBox.SelectedIndex = 0;
			});
		}

		private void SetConnectionUIStates(UIStates state)
		{
			uiState = state;
			serverComboBox.IsEnabled = state == UIStates.Stopped;
			connectButton.Content = state != UIStates.Stopped ? (state == UIStates.Streaming ? "Pause" : "Play") : "Connect";
			refreshButton.Content = state != UIStates.Stopped ? "Stop" : "Refresh";
			notConnectedImage.Visibility = state == UIStates.Stopped ? Visibility.Visible : Visibility.Hidden;
			if (state == UIStates.Stopped)
			{
				unsafe
				{
					bitmap.Lock();
					var buffer = (byte*)bitmap.BackBuffer;
					for (int i = 0; i != metaData.imageDataSize; ++i) buffer[i] = 255;
					bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
					bitmap.Unlock();
				}
			}
		}

		private void refreshButton_Click(object sender, RoutedEventArgs e)
		{
			// handle stop
			if (uiState == UIStates.Streaming)
			{
				SetConnectionUIStates(UIStates.Stopped);
				var metaData = new MetaData()
				{
					type = MetaDataTypes.PauseCapture,
					dataSize = -1
				};
			
				socket.SendMetaData(metaData);

				Thread.Sleep(1000);
				lock (this)
				{
					socket.Dispose();
					socket = null;
				}

				return;
			}

			// handle refresh
			var thread = new Thread(Refresh);
			thread.Start();
		}

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{
			// handle pause
			if (uiState == UIStates.Streaming || uiState == UIStates.Paused)
			{
				var state = uiState;
				SetConnectionUIStates(state == UIStates.Streaming ? UIStates.Paused : UIStates.Streaming);

				var metaData = new MetaData()
				{
					type = state == UIStates.Streaming ? MetaDataTypes.PauseCapture : MetaDataTypes.ResumeCapture,
					dataSize = -1
				};
			
				socket.SendMetaData(metaData);
				return;
			}

			// handle connect
			SetConnectionUIStates(UIStates.Streaming);

			NetworkHost host = null;
			if (serverComboBox.SelectedIndex == -1)
			{
				#if DEBUG
				host = new NetworkHost("LoopBack")
				{
					endpoints = new List<IPEndPoint>() {new IPEndPoint(IPAddress.Loopback, 8888)}
				};
				#else
				return;
				#endif
			}
			else
			{
				host = (NetworkHost)serverComboBox.SelectedValue;
			}

			socket = new DataSocket(NetworkTypes.Client);
			socket.ConnectedCallback += Socket_ConnectedCallback;
			socket.DisconnectedCallback += Socket_DisconnectedCallback;
			socket.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
			socket.DataRecievedCallback += Socket_DataRecievedCallback;
			socket.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
			socket.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
			socket.Connect(host.endpoints[0]);
		}

		private PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat format)
		{
			switch (format)
			{
				case System.Drawing.Imaging.PixelFormat.Format24bppRgb: return PixelFormats.Rgb24;
				case System.Drawing.Imaging.PixelFormat.Format16bppRgb565: return PixelFormats.Bgr565;
				default: throw new Exception("Unsuported format: " + format);
			}
		}

		private System.Drawing.Imaging.PixelFormat ConvertPixelFormat(PixelFormat format)
		{
			if (format == PixelFormats.Rgb24) return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
			else if (format == PixelFormats.Bgr565) return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
			else throw new Exception("Unsuported format: " + format);
		}
		
		private void Socket_StartDataRecievedCallback(MetaData metaData)
		{
			if (metaData.type != MetaDataTypes.ImageData) throw new Exception("Invalid meta data type: " + metaData.type);
			this.metaData = metaData;

			// init compression
			if (metaData.compressed)
			{
				if (gzipStream == null) gzipStream = new MemoryStream();
				else gzipStream.SetLength(0);
			}

			// invoke UI thread
			Dispatcher.InvokeAsync(delegate()
			{
				// create bitmap
				if (bitmap == null || bitmap.Width != metaData.width || bitmap.Height != metaData.height || ConvertPixelFormat(bitmap.Format) != metaData.format)
				{
					bitmap = new WriteableBitmap(metaData.width, metaData.height, 96, 96, ConvertPixelFormat(metaData.format), null);
					image.Source = bitmap;
				}

				// lock bitmap
				bitmap.Lock();
				bitmapBackbuffer = bitmap.BackBuffer;
			});
		}

		private unsafe void Socket_EndDataRecievedCallback()
		{
			if (metaData.compressed)
			{
				gzipStream.Position = 0;
				using (var bitmapStream = new UnmanagedMemoryStream((byte*)bitmapBackbuffer, metaData.imageDataSize, metaData.imageDataSize, FileAccess.Write))
				using (var gzip = new GZipStream(gzipStream, CompressionMode.Decompress, true))
				{
					gzip.CopyTo(bitmapStream);
				}
			}

			bitmapBackbuffer = IntPtr.Zero;
			Dispatcher.InvokeAsync(delegate()
			{
				if (!skipImageUpdate) bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
				else skipImageUpdate = false;
				bitmap.Unlock();
			});
		}

		private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			while (bitmapBackbuffer == IntPtr.Zero && uiState == UIStates.Streaming) Thread.Sleep(1);
			if (uiState != UIStates.Streaming) return;

			if (metaData.compressed)
			{
				gzipStream.Write(data, 0, dataSize);
			}
			else
			{
				Marshal.Copy(data, 0, bitmapBackbuffer + offset, dataSize);
			}
		}

		private void Socket_ConnectionFailedCallback(string error)
		{
			DebugLog.LogError("Failed to connect: " + error);
		}

		private void Socket_ConnectedCallback()
		{
			var metaData = new MetaData()
			{
				type = MetaDataTypes.StartCapture,
				compressed = false,
				screenIndex = 0,
				format = System.Drawing.Imaging.PixelFormat.Format24bppRgb,
				dataSize = -1
			};
			
			socket.SendMetaData(metaData);
		}

		private void Socket_DisconnectedCallback()
		{
			lock (this)
			{
				socket.Dispose();
				socket = null;
			}

			Dispatcher.InvokeAsync(delegate()
			{
				SetConnectionUIStates(UIStates.Stopped);
			});
		}

		private void settingsButton_Click(object sender, RoutedEventArgs e)
		{
			settingsOverlay.Show();
		}
	}
}
