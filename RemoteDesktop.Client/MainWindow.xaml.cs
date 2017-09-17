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
		
		private void Socket_StartDataRecievedCallback(MetaData metaData)
		{
			bitmap.Lock();
			bitmapBackbuffer = bitmap.BackBuffer;
		}

		private void Socket_EndDataRecievedCallback()
		{
			bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
			bitmap.Unlock();
		}

		private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			Marshal.Copy(data, 0, bitmapBackbuffer + offset, dataSize);
		}

		private void Socket_ConnectionFailedCallback(string error)
		{
			
		}

		private void Socket_ConnectedCallback()
		{
			
		}
	}
}
