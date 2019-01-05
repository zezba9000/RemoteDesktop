using RemoteDesktop.Android.Core;
using RemoteDesktop.Server.XamaOK;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;
using OpenH264.Encoder;

namespace RemoteDesktop.Server
{
	public class MainApplicationContext : ApplicationContext
	{
		private bool isDisposed;
		private NotifyIcon trayIcon;
		private DataSocket socket;
		private Rectangle screenRect;
		private Bitmap bitmap, scaledBitmap;
		private Graphics graphics, scaledGraphics;
        //System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
        System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        //System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
        int screenIndex, currentScreenIndex;
        float targetFPS = 1f;
        float fixedTargetFPS = 20f;
        bool compress; //, currentCompress;
        bool isFixedParamUse = true; // use server side hard coded parameter on running
        bool fixedCompress = true;
        float resolutionScale = 1.0F; // DEBUG INFO: current jpeg encoding implementation is not work with not value 1.0
        float fixedResolutionScale = 0.5F; // if this value is not 1, this value is used at scaling always
		private System.Windows.Forms.Timer timer;
		public static Dispatcher dispatcher;

		//private InputSimulator input;
        private bool receivedMetaData = false;
		//private byte inputLastMouseState;
        private CaptureSoundStreamer cap_streamer;
        private Process ffmpegProc = null;
        //byte[] tmp_buf = new byte[540 * 960 * 2];

        //private ExtractedH264Encoder encoder;
        private int timestamp = 0; // equal frame number

        private string ffmpegPath = "C:\\Program Files\\ffmpeg-20181231-51b356e-win64-static\\bin\\ffmpeg.exe";
        private static string outPathBase = "F:\\work\\tmp\\gen_HLS_files_from_h264_avi_file_try\\";
        //private string ffmpegForHLSArgs = "-y -i - -codec copy -map 0 -flags +cgop+global_header -f hls -hls_time 1 -hls_list_size 3 -hls_allow_cache 1 -hls_segment_filename " + outPathBase + "stream_%d.ts -hls_flags delete_segments " + outPathBase + "test.m3u8";
        //private string ffmpegForHLSArgs = "-y -i - -codec copy -map 0 -flags +cgop+global_header -f hls -hls_time 1 -hls_list_size 3 -hls_allow_cache 0 -hls_segment_filename " + outPathBase + "stream_%d.ts -hls_flags delete_segments " + outPathBase + "test.m3u8";
        //private string ffmpegForHLSArgs = "-y -f image2pipe -framerate 1 -i - -c:v libx264 -r 1 -vf format=yuv420p -f hls -r 1 -g 10 -hls_time 10 -hls_list_size 3 -hls_allow_cache 0 -hls_segment_filename " + outPathBase + "stream_%d.ts -hls_flags delete_segments " + outPathBase + "test.m3u8";
        //private string ffmpegForHLSArgs = "-y -f image2pipe -framerate 1 -i - -c:v libx264 -r 1 -vf format=yuv420p -f hls -r 1 -g 10 -hls_time 1 -hls_list_size 3 -hls_allow_cache 0 -hls_segment_filename " + outPathBase + "stream_single.ts -hls_flags delete_segments -hls_flags single_file " + outPathBase + "test.m3u8";
        private string ffmpegForHLSArgs = "-y -f image2pipe -framerate 1 -i - -c:v libx264 -r 1 -vf format=yuv420p -f hls -r 1 -g 1 -hls_time 1 -hls_list_size 4 -hls_allow_cache 0 -hls_segment_filename " + outPathBase + "stream_%d.ts -hls_flags delete_segments " + outPathBase + "test.m3u8";

        public MainApplicationContext(int cap_image_serv_port)
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
				Visible = true,
				Text = "Remote Desktop Server v0.1.0"
			};

            dispatcher = Dispatcher.CurrentDispatcher;

            // 一旦音声配信は止める
            //cap_streamer = new CaptureSoundStreamer();

            //// init input simulation
            //input = new InputSimulator();

            //// 余計なデータがstdinにあったらクリアする
            //TextReader input;
            //input = Console.In;
            //string line;
            //while ((line = input.ReadLine()) != null) { }
            //input.Dispose();

            kickFFMPEG();

            //encoder = new ExtractedH264Encoder(540, 960, 20 * 1024 * 8 , 1.0f, 10.0f);
            //encoder.aviDataGenerated += h264AVIDataHandler;

		    timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
			timer.Tick += Timer_Tick_for_ffmpeg_hls_test;
            timer.Start();

            // HLS using ffmpegのテストのためにコメントアウト
            //// start TCP socket listen for image server
            //socket.Listen(IPAddress.Any, cap_image_serv_port);
            //socket = new DataSocket(NetworkTypes.Server);
            //socket.ConnectedCallback += Socket_ConnectedCallback;
            //socket.DisconnectedCallback += Socket_DisconnectedCallback;
            //socket.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
            //socket.DataRecievedCallback += Socket_DataRecievedCallback;
            //socket.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
            //socket.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
            //socket.Listen(IPAddress.Parse(RTPConfiguration.ServerAddress), cap_image_serv_port);
        }

        // set ffmpegProc field
        private void kickFFMPEG()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false; //required to redirect standart input/output

            // redirects on your choice
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = ffmpegPath;

            // デバッグ出力が邪魔だから切った
            startInfo.Arguments = ffmpegForHLSArgs;

            ffmpegProc = new Process();
            ffmpegProc.StartInfo = startInfo;
            // リダイレクトした標準出力・標準エラーの内容を受信するイベントハンドラを設定する
            ffmpegProc.OutputDataReceived += PrintFFMPEGOutputData;
            ffmpegProc.ErrorDataReceived  += PrintFFMPEGErrorData;

            ffmpegProc.Start();

            // ffmpegが確実に起動状態になるまで待つ
            Thread.Sleep(3000);

            // 標準出力・標準エラーの非同期読み込みを開始する
            ffmpegProc.BeginOutputReadLine();
            ffmpegProc.BeginErrorReadLine();
        }

          private void PrintFFMPEGOutputData(object sender, DataReceivedEventArgs e)
          {
            Process p = (Process)sender;

            if (!string.IsNullOrEmpty(e.Data))
              Console.WriteLine("[{0}2;stdout] {1}", p.ProcessName, e.Data);
          }

          private void PrintFFMPEGErrorData(object sender, DataReceivedEventArgs e)
          {
            // 子プロセスの標準エラーから受信した内容を自プロセスの標準エラーに書き込む
            Process p = (Process)sender;

            if (!string.IsNullOrEmpty(e.Data))
              Console.Error.WriteLine("[{0}2;stderr] {1}", p.ProcessName, e.Data);
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
					timer.Tick -= Timer_Tick;
					timer.Dispose();
					timer = null;
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

		private void Socket_StartDataRecievedCallback(MetaData metaData)
		{
			lock (this)
			{
				if (isDisposed) return;

				void CreateTimer(bool recreate, int fps)
				{
					if (recreate && timer != null)
					{
						timer.Tick -= Timer_Tick;
						timer.Dispose();
						timer = null;
					}

					if (timer == null)
					{
						timer = new System.Windows.Forms.Timer();
                        timer.Interval = (int) (1000f / fps); // targetFPSは呼び出し時には適切に更新が行われていることを想定
						timer.Tick += Timer_Tick;
					}

					timer.Start();
				}

				// update settings
				if (metaData.type == MetaDataTypes.UpdateSettings || metaData.type == MetaDataTypes.StartCapture)
				{
					DebugLog.Log("Updating settings");
                    //format = metaData.format;
                    //format = System.Drawing.Imaging.PixelFormat.Format

                    format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                    screenIndex = metaData.screenIndex;
					compress = metaData.compressed;
					resolutionScale = metaData.resolutionScale;
					targetFPS = metaData.targetFPS;
                    if (isFixedParamUse)
                    {
                        compress = fixedCompress;
                        targetFPS = fixedTargetFPS;
                        resolutionScale = fixedResolutionScale;
                    }
                    receivedMetaData = true;
					if (metaData.type == MetaDataTypes.UpdateSettings)
					{
						dispatcher.InvokeAsync(delegate()
						{
							CreateTimer(true, (int)targetFPS);
						});
					}
				}

				// start / stop
				if (metaData.type == MetaDataTypes.StartCapture)
				{
					dispatcher.InvokeAsync(delegate()
					{
						CreateTimer(false, (int)targetFPS);
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
				//else if (metaData.type == MetaDataTypes.UpdateMouse)
				//{
				//	// mouse pos
				//	Cursor.Position = new Point(metaData.mouseX, metaData.mouseY);

				//	// mouse clicks
				//	if (inputLastMouseState != metaData.mouseButtonPressed)
				//	{
				//		// handle state changes
				//		if (inputLastMouseState == 1) input.Mouse.LeftButtonUp();
				//		else if (inputLastMouseState == 2) input.Mouse.RightButtonUp();
				//		else if (inputLastMouseState == 3) input.Mouse.XButtonUp(2);

				//		// handle new state
				//		if (metaData.mouseButtonPressed == 1) input.Mouse.LeftButtonDown();
				//		else if (metaData.mouseButtonPressed == 2) input.Mouse.RightButtonDown();
				//		else if (metaData.mouseButtonPressed == 3) input.Mouse.XButtonDown(2);
				//	}

				//	// mouse scroll wheel
				//	if (metaData.mouseScroll != 0) input.Mouse.VerticalScroll(metaData.mouseScroll);

				//	// finish
				//	inputLastMouseState = metaData.mouseButtonPressed;
				//}
				//else if (metaData.type == MetaDataTypes.UpdateKeyboard)
				//{
				//	VirtualKeyCode specialKey = 0;
				//	if (metaData.specialKeyCode != 0)
				//	{
				//		specialKey = ConvertKeyCode((Key)metaData.specialKeyCode);
				//		if (specialKey != 0) input.Keyboard.KeyDown(specialKey);
				//	}

				//	if (metaData.keyCode != 0)
				//	{
				//		var key = ConvertKeyCode((Key)metaData.keyCode);
				//		if (key != 0) input.Keyboard.KeyPress(key);
				//		if (specialKey != 0) input.Keyboard.KeyUp(specialKey);
				//	}
				//}
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

        // only used on Client
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
            receivedMetaData = false;
			dispatcher.InvokeAsync(delegate()
			{
                if (timer != null)
                {
					timer.Tick -= Timer_Tick;
					timer.Dispose();
					timer = null;
                }
				socket.ReListen();
			});
		}

        private unsafe BitmapXama convertToBitmapXamaAndRotate(Bitmap bmap)
        {
            bmap.RotateFlip(RotateFlipType.Rotate90FlipY);

            Rectangle rect = new Rectangle(0, 0, bmap.Width, bmap.Height);

            BitmapData bmpData = null;
            Bitmap bmap16 = null;
            long dataLength = -1;
            if (RTPConfiguration.isConvTo24bit)
            {
                bmap16 = bmap.Clone(new Rectangle(0, 0, bmap.Width, bmap.Height), PixelFormat.Format24bppRgb);
                bmpData = bmap16.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmap16.PixelFormat);
                dataLength = bmap.Width * bmap.Height * 3; // RGB24                
            }
            else if (RTPConfiguration.isConvTo16bit)
            {                
                bmap16 = bmap.Clone(new Rectangle(0, 0, bmap.Width, bmap.Height), PixelFormat.Format16bppRgb565);
                bmpData = bmap16.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmap16.PixelFormat);
                dataLength = bmap.Width * bmap.Height * 2; // RGB565
            }
            else
            {
                bmpData = bmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmap.PixelFormat);
                dataLength = bmap.Width * bmap.Height * 3; //RGB24
            }
            
            IntPtr ptr = bmpData.Scan0;
            MemoryStream ms = new MemoryStream();
            var bitmapStream = new UnmanagedMemoryStream((byte*)bmpData.Scan0, dataLength);
            bitmapStream.CopyTo(ms);
            if (RTPConfiguration.isConvTo24bit)
            {
                bmap16.UnlockBits(bmpData);
            }
            else if (RTPConfiguration.isConvTo16bit)
            {
                bmap16.UnlockBits(bmpData);
            }
            else
            {
                bmap.UnlockBits(bmpData);
            }


            //byte[] buf = ms.GetBuffer();
            byte[] buf = ms.ToArray();
            var retBmap = new BitmapXama(buf);
            retBmap.Height = bmap.Height;
            retBmap.Width = bmap.Width;

            return retBmap;
        }

        private void h264AVIDataHandler(byte[] data)
        {
            ffmpegProc.StandardInput.BaseStream.Write(data, 0, data.Length);
            ffmpegProc.StandardInput.BaseStream.Flush();
        }

		private void Timer_Tick_for_ffmpeg_hls_test(object sender, EventArgs e)
		{
			lock (this)
			{
				CaptureScreen();
                BitmapXama convedXBmap = null;
                convedXBmap = convertToBitmapXamaAndRotate(scaledBitmap);
                var tmp_buf = new byte[convedXBmap.Width * convedXBmap.Height * 3];
                if(convedXBmap.getInternalBuffer().Length == 0)
                {
                    return;
                }
                Array.Copy(convedXBmap.getInternalBuffer(), 0, tmp_buf, 0, tmp_buf.Length);
                //Utils.saveByteArrayToFile(tmp_buf, outPathBase + "rgb565-540x960.raw");
                //Application.Exit();

                //// 余計なデータがstdinにあったらクリアする
                //TextReader input;
                //input = Console.In;
                //string line;
                //while ((line = input.ReadLine()) != null) { Console.WriteLine(line); }
                //input.Dispose();

                //ffmpegProc1.StandardInput.WriteLine("hoge");

                //ffmpegProc1.StandardInput.Write(tmp_buf);
                //ffmpegProc1.StandardInput.Flush();

                //ffmpegProc1.StandardInput.Write(tmp_buf.ToString());
                //ffmpegProc1.StandardInput.Flush();
                //var bs = new BinaryWriter(ffmpegProc1.StandardInput.BaseStream);
                //bs.Write(tmp_buf);
                //bs.Flush();

                //var str = System.Text.Encoding.GetEncoding(932).GetString(tmp_buf);
                //ffmpegProc1.StandardInput.Write(str);
                //ffmpegProc1.StandardInput.Flush();

                var bitmap_ms = Utils.getAddHeaderdBitmapStreamByPixcels(tmp_buf, convedXBmap.Width, convedXBmap.Height);
                //Console.WriteLine("write data as bitmap file byte data to encoder " + bitmap_ms.Length.ToString() + "Bytes timestamp=" + timestamp.ToString());
                //byte[] bmpfFile_buf = bitmap_ms.ToArray();
                ////Array.Resize<byte>(ref bmpfFile_buf, 54 + convedXBmap.Width * convedXBmap.Height * 3);
                //encoder.addBitmapFrame(bmpfFile_buf, timestamp++);

                var br = bitmap_ms.ToArray();
                ffmpegProc.StandardInput.BaseStream.Write(br, 0, br.Length);
                ffmpegProc.StandardInput.BaseStream.Flush();
            }
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			lock (this)
			{
				if (isDisposed) return;
                if (!receivedMetaData) return;

				CaptureScreen();
                BitmapXama convedXBmap = null;
                if (resolutionScale == 1)
                {
                    convedXBmap = convertToBitmapXamaAndRotate(bitmap);
                    socket.SendImage(convedXBmap, screenRect.Width, screenRect.Height, screenIndex, compress, targetFPS);
                }
                else
                {
                    convedXBmap = convertToBitmapXamaAndRotate(scaledBitmap);
                    socket.SendImage(convedXBmap, screenRect.Width, screenRect.Height, screenIndex, compress, targetFPS);
                }
			}
		}

		private void CaptureScreen()
		{
            lock (this)
            {
                if (bitmap == null || bitmap.PixelFormat != format) // || screenIndex != currentScreenIndex) // || compress != currentCompress) // || resolutionScale != currentResolutionScale)
                {
                    currentScreenIndex = screenIndex;
                    //currentCompress = compress;
                    //currentResolutionScale = resolutionScale;

                    // get screen to catpure
                    var screens = Screen.AllScreens;
                    var screen = (screenIndex < screens.Length) ? screens[screenIndex] : screens[0];
                    screenRect = screen.Bounds;
                }

                // --- avoid noised bitmap sended due to lotate of convert to BitmapXama (not good solution) ---
                // create bitmap resources
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
                if (graphics != null)
                {
                    graphics.Dispose();
                    graphics = null;
                }
                bitmap = new Bitmap(screenRect.Width, screenRect.Height, format);
                graphics = Graphics.FromImage(bitmap);

                float localScale = 1;
                if (resolutionScale != 1 || fixedResolutionScale != 1)
                {
                    if (scaledBitmap != null)
                    {
                        scaledBitmap.Dispose();
                        scaledBitmap = null;
                    }
                    if (scaledGraphics != null)
                    {
                        scaledGraphics.Dispose();
                        scaledGraphics = null;
                    }
                    localScale = resolutionScale;
                    if(fixedResolutionScale != 1)
                    {
                        localScale = fixedResolutionScale;
                    }
                    scaledBitmap = new Bitmap((int)(screenRect.Width * localScale), (int)(screenRect.Height * localScale), format);
                    scaledGraphics = Graphics.FromImage(scaledBitmap);
                }
                // ---                                         end                                          ---

                // capture screen
                graphics.CopyFromScreen(screenRect.Left, screenRect.Top, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
                if (localScale != 1)
                {
                    scaledGraphics.DrawImage(bitmap, 0, 0, scaledBitmap.Width, scaledBitmap.Height);
                }
            }
        }

		//private VirtualKeyCode ConvertKeyCode(Key keycode)
		//{
		//	switch (keycode)
		//	{
		//		case Key.A: return VirtualKeyCode.VK_A;
		//		case Key.B: return VirtualKeyCode.VK_B;
		//		case Key.C: return VirtualKeyCode.VK_C;
		//		case Key.D: return VirtualKeyCode.VK_D;
		//		case Key.E: return VirtualKeyCode.VK_E;
		//		case Key.F: return VirtualKeyCode.VK_F;
		//		case Key.G: return VirtualKeyCode.VK_G;
		//		case Key.H: return VirtualKeyCode.VK_H;
		//		case Key.I: return VirtualKeyCode.VK_I;
		//		case Key.J: return VirtualKeyCode.VK_J;
		//		case Key.K: return VirtualKeyCode.VK_K;
		//		case Key.L: return VirtualKeyCode.VK_L;
		//		case Key.M: return VirtualKeyCode.VK_M;
		//		case Key.N: return VirtualKeyCode.VK_N;
		//		case Key.O: return VirtualKeyCode.VK_O;
		//		case Key.P: return VirtualKeyCode.VK_P;
		//		case Key.Q: return VirtualKeyCode.VK_Q;
		//		case Key.R: return VirtualKeyCode.VK_R;
		//		case Key.S: return VirtualKeyCode.VK_S;
		//		case Key.T: return VirtualKeyCode.VK_T;
		//		case Key.U: return VirtualKeyCode.VK_U;
		//		case Key.V: return VirtualKeyCode.VK_V;
		//		case Key.W: return VirtualKeyCode.VK_W;
		//		case Key.X: return VirtualKeyCode.VK_X;
		//		case Key.Y: return VirtualKeyCode.VK_Y;
		//		case Key.Z: return VirtualKeyCode.VK_Z;

		//		case Key.D0: return VirtualKeyCode.VK_0;
		//		case Key.D1: return VirtualKeyCode.VK_1;
		//		case Key.D2: return VirtualKeyCode.VK_2;
		//		case Key.D3: return VirtualKeyCode.VK_3;
		//		case Key.D4: return VirtualKeyCode.VK_4;
		//		case Key.D5: return VirtualKeyCode.VK_5;
		//		case Key.D6: return VirtualKeyCode.VK_6;
		//		case Key.D7: return VirtualKeyCode.VK_7;
		//		case Key.D8: return VirtualKeyCode.VK_8;
		//		case Key.D9: return VirtualKeyCode.VK_9;

		//		case Key.NumPad0: return VirtualKeyCode.NUMPAD0;
		//		case Key.NumPad1: return VirtualKeyCode.NUMPAD1;
		//		case Key.NumPad2: return VirtualKeyCode.NUMPAD2;
		//		case Key.NumPad3: return VirtualKeyCode.NUMPAD3;
		//		case Key.NumPad4: return VirtualKeyCode.NUMPAD4;
		//		case Key.NumPad5: return VirtualKeyCode.NUMPAD5;
		//		case Key.NumPad6: return VirtualKeyCode.NUMPAD6;
		//		case Key.NumPad7: return VirtualKeyCode.NUMPAD7;
		//		case Key.NumPad8: return VirtualKeyCode.NUMPAD8;
		//		case Key.NumPad9: return VirtualKeyCode.NUMPAD9;

		//		case Key.Subtract: return VirtualKeyCode.SUBTRACT;
		//		case Key.Add: return VirtualKeyCode.ADD;
		//		case Key.Multiply: return VirtualKeyCode.MULTIPLY;
		//		case Key.Divide: return VirtualKeyCode.DIVIDE;
		//		case Key.Decimal: return VirtualKeyCode.DECIMAL;

		//		case Key.F1: return VirtualKeyCode.F1;
		//		case Key.F2: return VirtualKeyCode.F2;
		//		case Key.F3: return VirtualKeyCode.F3;
		//		case Key.F4: return VirtualKeyCode.F4;
		//		case Key.F5: return VirtualKeyCode.F5;
		//		case Key.F6: return VirtualKeyCode.F6;
		//		case Key.F7: return VirtualKeyCode.F7;
		//		case Key.F8: return VirtualKeyCode.F8;
		//		case Key.F9: return VirtualKeyCode.F9;
		//		case Key.F10: return VirtualKeyCode.F10;
		//		case Key.F11: return VirtualKeyCode.F11;
		//		case Key.F12: return VirtualKeyCode.F12;

		//		case Key.LeftShift: return VirtualKeyCode.LSHIFT;
		//		case Key.RightShift: return VirtualKeyCode.RSHIFT;
		//		case Key.LeftCtrl: return VirtualKeyCode.LCONTROL;
		//		case Key.RightCtrl: return VirtualKeyCode.RCONTROL;
		//		case Key.LeftAlt: return VirtualKeyCode.LMENU;
		//		case Key.RightAlt: return VirtualKeyCode.RMENU;

		//		case Key.Back: return VirtualKeyCode.BACK;
		//		case Key.Space: return VirtualKeyCode.SPACE;
		//		case Key.Return: return VirtualKeyCode.RETURN;
		//		case Key.Tab: return VirtualKeyCode.TAB;
		//		case Key.CapsLock: return VirtualKeyCode.CAPITAL;
		//		case Key.Oem1: return VirtualKeyCode.OEM_1;
		//		case Key.Oem2: return VirtualKeyCode.OEM_2;
		//		case Key.Oem3: return VirtualKeyCode.OEM_3;
		//		case Key.Oem4: return VirtualKeyCode.OEM_4;
		//		case Key.Oem5: return VirtualKeyCode.OEM_5;
		//		case Key.Oem6: return VirtualKeyCode.OEM_6;
		//		case Key.Oem7: return VirtualKeyCode.OEM_7;
		//		case Key.Oem8: return VirtualKeyCode.OEM_8;
		//		case Key.OemComma: return VirtualKeyCode.OEM_COMMA;
		//		case Key.OemPeriod: return VirtualKeyCode.OEM_PERIOD;
		//		case Key.Escape: return VirtualKeyCode.ESCAPE;

		//		case Key.Home: return VirtualKeyCode.HOME;
		//		case Key.End: return VirtualKeyCode.END;
		//		case Key.PageUp: return VirtualKeyCode.PRIOR;
		//		case Key.PageDown: return VirtualKeyCode.NEXT;
		//		case Key.Insert: return VirtualKeyCode.INSERT;
		//		case Key.Delete: return VirtualKeyCode.DELETE;

		//		case Key.Left: return VirtualKeyCode.LEFT;
		//		case Key.Right: return VirtualKeyCode.RIGHT;
		//		case Key.Down: return VirtualKeyCode.DOWN;
		//		case Key.Up: return VirtualKeyCode.UP;

		//		default: return 0;
		//	}
		//}
	}
}
