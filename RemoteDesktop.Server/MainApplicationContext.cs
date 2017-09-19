using RemoteDesktop.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace RemoteDesktop.Server
{
	public class MainApplicationContext : ApplicationContext
	{
		private bool isDisposed;
		private NotifyIcon trayIcon;
		private NetworkDiscovery networkDiscovery;
		private DataSocket socket;

		private Rectangle screenRect;
		private Bitmap bitmap, scaledBitmap;
		private Graphics graphics, scaledGraphics;
		PixelFormat format = PixelFormat.Format24bppRgb;
		int screenIndex;
		bool compress;
		float resolutionScale = 1;
		private Timer timer;
		private Dispatcher dispatcher;

		private InputSimulator input;
		private byte inputLastMouseState;

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

			// init input simulation
			input = new InputSimulator();

			// star socket
			dispatcher = Dispatcher.CurrentDispatcher;
			socket = new DataSocket(NetworkTypes.Server);
			socket.ConnectedCallback += Socket_ConnectedCallback;
			socket.DisconnectedCallback += Socket_DisconnectedCallback;
			socket.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
			socket.DataRecievedCallback += Socket_DataRecievedCallback;
			socket.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
			socket.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
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

		private VirtualKeyCode ConvertKeyCode(Key keycode)
		{
			switch (keycode)
			{
				case Key.A: return VirtualKeyCode.VK_A;
				case Key.B: return VirtualKeyCode.VK_B;
				case Key.C: return VirtualKeyCode.VK_C;
				case Key.D: return VirtualKeyCode.VK_D;
				case Key.E: return VirtualKeyCode.VK_E;
				case Key.F: return VirtualKeyCode.VK_F;
				case Key.G: return VirtualKeyCode.VK_G;
				case Key.H: return VirtualKeyCode.VK_H;
				case Key.I: return VirtualKeyCode.VK_I;
				case Key.J: return VirtualKeyCode.VK_J;
				case Key.K: return VirtualKeyCode.VK_K;
				case Key.L: return VirtualKeyCode.VK_L;
				case Key.M: return VirtualKeyCode.VK_M;
				case Key.N: return VirtualKeyCode.VK_N;
				case Key.O: return VirtualKeyCode.VK_O;
				case Key.P: return VirtualKeyCode.VK_P;
				case Key.Q: return VirtualKeyCode.VK_Q;
				case Key.R: return VirtualKeyCode.VK_R;
				case Key.S: return VirtualKeyCode.VK_S;
				case Key.T: return VirtualKeyCode.VK_T;
				case Key.U: return VirtualKeyCode.VK_U;
				case Key.V: return VirtualKeyCode.VK_V;
				case Key.W: return VirtualKeyCode.VK_W;
				case Key.X: return VirtualKeyCode.VK_X;
				case Key.Y: return VirtualKeyCode.VK_Y;
				case Key.Z: return VirtualKeyCode.VK_Z;

				case Key.D0: return VirtualKeyCode.VK_0;
				case Key.D1: return VirtualKeyCode.VK_1;
				case Key.D2: return VirtualKeyCode.VK_2;
				case Key.D3: return VirtualKeyCode.VK_3;
				case Key.D4: return VirtualKeyCode.VK_4;
				case Key.D5: return VirtualKeyCode.VK_5;
				case Key.D6: return VirtualKeyCode.VK_6;
				case Key.D7: return VirtualKeyCode.VK_7;
				case Key.D8: return VirtualKeyCode.VK_8;
				case Key.D9: return VirtualKeyCode.VK_9;

				case Key.NumPad0: return VirtualKeyCode.NUMPAD0;
				case Key.NumPad1: return VirtualKeyCode.NUMPAD1;
				case Key.NumPad2: return VirtualKeyCode.NUMPAD2;
				case Key.NumPad3: return VirtualKeyCode.NUMPAD3;
				case Key.NumPad4: return VirtualKeyCode.NUMPAD4;
				case Key.NumPad5: return VirtualKeyCode.NUMPAD5;
				case Key.NumPad6: return VirtualKeyCode.NUMPAD6;
				case Key.NumPad7: return VirtualKeyCode.NUMPAD7;
				case Key.NumPad8: return VirtualKeyCode.NUMPAD8;
				case Key.NumPad9: return VirtualKeyCode.NUMPAD9;

				case Key.Subtract: return VirtualKeyCode.SUBTRACT;
				case Key.Add: return VirtualKeyCode.ADD;
				case Key.Multiply: return VirtualKeyCode.MULTIPLY;
				case Key.Divide: return VirtualKeyCode.DIVIDE;
				case Key.Decimal: return VirtualKeyCode.DECIMAL;

				case Key.F1: return VirtualKeyCode.F1;
				case Key.F2: return VirtualKeyCode.F2;
				case Key.F3: return VirtualKeyCode.F3;
				case Key.F4: return VirtualKeyCode.F4;
				case Key.F5: return VirtualKeyCode.F5;
				case Key.F6: return VirtualKeyCode.F6;
				case Key.F7: return VirtualKeyCode.F7;
				case Key.F8: return VirtualKeyCode.F8;
				case Key.F9: return VirtualKeyCode.F9;
				case Key.F10: return VirtualKeyCode.F10;
				case Key.F11: return VirtualKeyCode.F11;
				case Key.F12: return VirtualKeyCode.F12;

				case Key.LeftShift: return VirtualKeyCode.LSHIFT;
				case Key.RightShift: return VirtualKeyCode.RSHIFT;
				case Key.LeftCtrl: return VirtualKeyCode.LCONTROL;
				case Key.RightCtrl: return VirtualKeyCode.RCONTROL;
				case Key.LeftAlt: return VirtualKeyCode.LMENU;
				case Key.RightAlt: return VirtualKeyCode.RMENU;

				case Key.Back: return VirtualKeyCode.BACK;
				case Key.Space: return VirtualKeyCode.SPACE;
				case Key.Return: return VirtualKeyCode.RETURN;
				case Key.Tab: return VirtualKeyCode.TAB;
				case Key.CapsLock: return VirtualKeyCode.CAPITAL;
				case Key.Oem1: return VirtualKeyCode.OEM_1;
				case Key.Oem2: return VirtualKeyCode.OEM_2;
				case Key.Oem3: return VirtualKeyCode.OEM_3;
				case Key.Oem4: return VirtualKeyCode.OEM_4;
				case Key.Oem5: return VirtualKeyCode.OEM_5;
				case Key.Oem6: return VirtualKeyCode.OEM_6;
				case Key.Oem7: return VirtualKeyCode.OEM_7;
				case Key.Oem8: return VirtualKeyCode.OEM_8;
				case Key.OemComma: return VirtualKeyCode.OEM_COMMA;
				case Key.OemPeriod: return VirtualKeyCode.OEM_PERIOD;
				case Key.Escape: return VirtualKeyCode.ESCAPE;

				case Key.Home: return VirtualKeyCode.HOME;
				case Key.End: return VirtualKeyCode.END;
				case Key.PageUp: return VirtualKeyCode.PRIOR;
				case Key.PageDown: return VirtualKeyCode.NEXT;
				case Key.Insert: return VirtualKeyCode.INSERT;
				case Key.Delete: return VirtualKeyCode.DELETE;

				case Key.Left: return VirtualKeyCode.LEFT;
				case Key.Right: return VirtualKeyCode.RIGHT;
				case Key.Down: return VirtualKeyCode.DOWN;
				case Key.Up: return VirtualKeyCode.UP;

				default: return 0;
			}
		}

		private void Socket_StartDataRecievedCallback(MetaData metaData)
		{
			lock (this)
			{
				if (isDisposed) return;

				// update settings
				if (metaData.type == MetaDataTypes.UpdateSettings || metaData.type == MetaDataTypes.StartCapture)
				{
					DebugLog.Log("Updating settings");
					format = metaData.format;
					screenIndex = metaData.screenIndex;
					compress = metaData.compressed;
					resolutionScale = metaData.resolutionScale;
				}
				
				// start / stop
				if (metaData.type == MetaDataTypes.StartCapture)
				{
					dispatcher.InvokeAsync(delegate()
					{
						if (timer == null)
						{
							timer = new Timer();
							timer.Interval = 1000 / 15;
							timer.Tick += Timer_Tick;
						}
					
						timer.Start();
					});
				}
				else if (metaData.type == MetaDataTypes.PauseCapture)
				{
					dispatcher.InvokeAsync(delegate()
					{
						timer.Stop();
					});
				}
				else if (metaData.type == MetaDataTypes.ResumeCapture)
				{
					dispatcher.InvokeAsync(delegate()
					{
						timer.Start();
					});
				}
				else if (metaData.type == MetaDataTypes.UpdateMouse)
				{
					// mouse pos
					Cursor.Position = new Point(metaData.mouseX, metaData.mouseY);
					
					// mouse clicks
					if (inputLastMouseState != metaData.mouseButtonPressed)
					{
						// handle state changes
						if (inputLastMouseState == 1) input.Mouse.LeftButtonUp();
						else if (inputLastMouseState == 2) input.Mouse.RightButtonUp();
						else if (inputLastMouseState == 3) input.Mouse.XButtonUp(2);

						// handle new state
						if (metaData.mouseButtonPressed == 1) input.Mouse.LeftButtonDown();
						else if (metaData.mouseButtonPressed == 2) input.Mouse.RightButtonDown();
						else if (metaData.mouseButtonPressed == 3) input.Mouse.XButtonDown(2);
					}

					// mouse scroll wheel
					if (metaData.mouseScroll != 0) input.Mouse.VerticalScroll(metaData.mouseScroll);
					
					// finish
					inputLastMouseState = metaData.mouseButtonPressed;
				}
				else if (metaData.type == MetaDataTypes.UpdateKeyboard)
				{
					VirtualKeyCode specialKey = 0;
					if (metaData.specialKeyCode != 0)
					{
						specialKey = ConvertKeyCode((Key)metaData.specialKeyCode);
						if (specialKey != 0) input.Keyboard.KeyDown(specialKey);
					}

					if (metaData.keyCode != 0)
					{
						var key = ConvertKeyCode((Key)metaData.keyCode);
						if (key != 0) input.Keyboard.KeyPress(key);
						if (specialKey != 0) input.Keyboard.KeyUp(specialKey);
					}
				}
			}
		}

		private void Socket_EndDataRecievedCallback()
		{
			// do nothing
		}

		private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
		{
			// do nothing
		}

		private void Socket_ConnectionFailedCallback(string error)
		{
			DebugLog.LogError("Failed to connect: " + error);
		}

		private void Socket_ConnectedCallback()
		{
			DebugLog.Log("Connected to client");
		}

		private void Socket_DisconnectedCallback()
		{
			DebugLog.Log("Disconnected from client");
			dispatcher.InvokeAsync(delegate()
			{
				if (timer != null) timer.Stop();
				socket.ReListen();
			});
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			lock (this)
			{
				if (isDisposed) return;

				CaptureScreen();
				if (resolutionScale == 1) socket.SendImage(bitmap, screenIndex, compress);
				else socket.SendImage(scaledBitmap, screenIndex, compress);
			}
		}

		private void CaptureScreen()
		{
			if (bitmap == null || bitmap.PixelFormat != format)
			{
				// get screen to catpure
				var screens = Screen.AllScreens;
				var screen = (screenIndex < screens.Length) ? screens[screenIndex] : screens[0];
				screenRect = screen.Bounds;

				// create bitmap resources
				if (bitmap != null) bitmap.Dispose();
				if (graphics != null) graphics.Dispose();
				bitmap = new Bitmap(screenRect.Width, screenRect.Height, format);
				graphics = Graphics.FromImage(bitmap);

				if (resolutionScale != 1)
				{
					if (scaledBitmap != null) scaledBitmap.Dispose();
					if (scaledGraphics != null) scaledGraphics.Dispose();
					scaledBitmap = new Bitmap((int)(screenRect.Width * resolutionScale), (int)(screenRect.Height * resolutionScale), format);
					scaledGraphics = Graphics.FromImage(scaledBitmap);
				}
			}

			// capture screen
			graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
			if (resolutionScale != 1) scaledGraphics.DrawImage(bitmap, 0, 0, scaledBitmap.Width, scaledBitmap.Height);
		}
	}
}
