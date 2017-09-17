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
		private bool isStreaming, skipImageUpdate;

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

			if (socket != null)
			{
				socket.Dispose();
				socket = null;
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

		private void refreshButton_Click(object sender, RoutedEventArgs e)
		{
			var thread = new Thread(Refresh);
			thread.Start();
		}

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{
			if (isStreaming)
			{
				refreshButton.IsEnabled = true;
				serverComboBox.IsEnabled = true;
				connectButton.Content = "Connect";
				isStreaming = false;

				var metaData = new MetaData()
				{
					type = MetaDataTypes.StopCapture,
					dataSize = -1
				};
			
				socket.SendMetaData(metaData);
				return;
			}

			refreshButton.IsEnabled = false;
			serverComboBox.IsEnabled = false;
			connectButton.Content = "Stop";
			isStreaming = true;

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

			socket = new DataSocket(NetworkTypes.Client, Dispatcher);
			socket.ConnectedCallback += Socket_ConnectedCallback;
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

			if (skipImageUpdate)
			{
				bitmapBackbuffer = IntPtr.Zero;
				return;
			}

			// init compression
			if (metaData.compressed)
			{
				if (gzipStream == null) gzipStream = new MemoryStream();
				else gzipStream.SetLength(0);
			}

			// create bitmap
			if (bitmap == null || bitmap.Width != metaData.width || bitmap.Height != metaData.height || ConvertPixelFormat(bitmap.Format) != metaData.format)
			{
				bitmap = new WriteableBitmap(metaData.width, metaData.height, 96, 96, ConvertPixelFormat(metaData.format), null);
				image.Source = bitmap;
			}

			bitmap.Lock();
			bitmapBackbuffer = bitmap.BackBuffer;
		}

		private unsafe void Socket_EndDataRecievedCallback()
		{
			if (skipImageUpdate)
			{
				skipImageUpdate = false;
				if (bitmapBackbuffer != IntPtr.Zero) bitmap.Unlock();
				return;
			}

			if (metaData.compressed)
			{
				gzipStream.Position = 0;
				using (var bitmapStream = new UnmanagedMemoryStream((byte*)bitmapBackbuffer, metaData.imageDataSize, metaData.imageDataSize, FileAccess.Write))
				using (var gzip = new GZipStream(gzipStream, CompressionMode.Decompress, true))
				{
					gzip.CopyTo(bitmapStream);
				}
			}

			bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
			bitmap.Unlock();
		}

		private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			if (skipImageUpdate) return;

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
	}
}
