using RemoteDesktop.Core;
using RemoteDesktop.Server.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace RemoteDesktop.Server
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			// parse args
			bool isDebugMode = false;
			int port = 8888;
			foreach (var arg in args)
			{
				if (arg == "debug") isDebugMode = true;
				else if (arg.StartsWith("port=")) port = int.Parse(arg.Split('=')[1]);
			}

			// start app
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (isDebugMode) Application.Run(new MainForm());
			else Application.Run(new MainApplicationContext(port));
		}
	}

	public class MainApplicationContext : ApplicationContext
	{
		private bool isDisposed;
		private NotifyIcon trayIcon;
		private NetworkDiscovery networkDiscovery;
		private DataSocket socket;

		private Rectangle screenRect;
		private Bitmap bitmap;
		private Graphics graphics;
		private Timer timer;

		public MainApplicationContext(int port)
		{
			// init tray icon
			var menuItems = new MenuItem[]
			{
				new MenuItem("Exit", Exit),
			};
			
			trayIcon = new NotifyIcon()
			{
				Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
				ContextMenu = new ContextMenu(menuItems),
				Visible = true
			};

			// star socket
			socket = new DataSocket(NetworkTypes.Server, Dispatcher.CurrentDispatcher);
			socket.ConnectedCallback += Socket_ConnectedCallback;
			socket.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
			socket.DataRecievedCallback += Socket_DataRecievedCallback;
			socket.Listen(IPAddress.Any, port);

			// start network discovery
			networkDiscovery = new NetworkDiscovery(NetworkTypes.Server);
			networkDiscovery.Register("SimpleRemoteDesktop", port);
		}

		void Exit(object sender, EventArgs e)
		{
			// dispose
			lock (this)
			{
				isDisposed = true;

				if (timer != null)
				{
					timer.Stop();
					timer.Dispose();
					timer = null;
				}

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

				if (graphics != null)
				{
					graphics.Dispose();
					graphics = null;
				}

				if (bitmap != null)
				{
					bitmap.Dispose();
					bitmap = null;
				}
			}

			// exit
			trayIcon.Visible = false;
			Application.Exit();
		}

		private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			
		}

		private void Socket_ConnectionFailedCallback(string error)
		{
			
		}

		private void Socket_ConnectedCallback()
		{
			lock (this)
			{
				if (isDisposed) return;

				if (timer == null)
				{
					timer = new Timer();
					timer.Interval = 1000 / 30;
					timer.Tick += Timer_Tick;
					timer.Start();
				}
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			lock (this)
			{
				if (isDisposed) return;

				CaptureScreen();
				socket.SendImage(bitmap, screenRect.Width, screenRect.Height, bitmap.PixelFormat);
			}
		}

		private void CaptureScreen()
		{
			if (bitmap == null)
			{
				var screen = Screen.PrimaryScreen;
				screenRect = screen.Bounds;

				bitmap = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format24bppRgb);
				graphics = Graphics.FromImage(bitmap);
			}

			graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
		}
	}
}
