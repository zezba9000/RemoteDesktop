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

		public MainWindow()
		{
			InitializeComponent();
			
			bitmap = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Rgb24, null);
			image.Source = bitmap;

			unsafe
			{
				bitmap.Lock();
				var buffer = (byte*)bitmap.BackBuffer;
				//for (int i = 0; i != dataSize; ++i) buffer[i + offset] = data[i];
				for (int i = 0; i != bitmap.PixelWidth * bitmap.PixelHeight * 3; ++i) buffer[i] = 127;
				bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
				bitmap.Unlock();
			}
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

			base.OnClosing(e);
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

		private void actionButton_Click(object sender, RoutedEventArgs e)
		{
			var thread = new Thread(Refresh);
			thread.Start();
		}

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{
			if (serverComboBox.SelectedIndex == -1) return;	

			socket = new DataSocket(NetworkTypes.Client, Dispatcher);
			socket.ConnectedCallback += Socket_ConnectedCallback;
			socket.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
			socket.DataRecievedCallback += Socket_DataRecievedCallback;
			socket.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
			socket.EndDataRecievedCallback += Socket_EndDataRecievedCallback;

			var host = (NetworkHost)serverComboBox.SelectedValue;
			socket.Connect(host.endpoints[0]);
		}
		
		private void Socket_StartDataRecievedCallback(MetaData metaData)
		{
			bitmap.Lock();
			bitmapBackbuffer = bitmap.BackBuffer;

			unsafe
			{
				var buffer = (byte*)bitmapBackbuffer;
				for (int i = 0; i != bitmap.PixelWidth * bitmap.PixelHeight * 3; ++i) buffer[i] = 255;
			}
		}

		private void Socket_EndDataRecievedCallback()
		{
			bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
			bitmap.Unlock();
		}

		private unsafe void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			var buffer = (byte*)bitmapBackbuffer;
			for (int i = 0; i != dataSize; ++i) buffer[i + offset] = data[i];
			//for (int i = 0; i != bitmap.PixelWidth * bitmap.PixelHeight * 3; ++i) buffer[i] = 255;
		}

		private void Socket_ConnectionFailedCallback(string error)
		{
			
		}

		private void Socket_ConnectedCallback()
		{
			
		}
	}
}
