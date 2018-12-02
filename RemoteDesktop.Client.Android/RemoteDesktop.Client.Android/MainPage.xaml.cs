using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using RemoteDesktop.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
    //public partial class MainPage : ContentPage
    //{
    //    public MainPage()
    //    {
    //        InitializeComponent();
    //    }
    //}

    enum UIStates
    {
        Stopped,
        Streaming,
        Paused
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private NetworkDiscovery networkDiscovery;
        private DataSocket socket;
        //private WriteableBitmap bitmap;
        private IntPtr bitmapBackbuffer;
        private MetaData metaData;
        private MemoryStream gzipStream;
        private bool skipImageUpdate, processingFrame, isDisposed, connectedToLocalPC;
        private UIStates uiState = UIStates.Stopped;
        private Thickness lastImageThickness;

        private Timer inputTimer;
        private bool mouseUpdate;
        private Point mousePoint;
        private sbyte mouseScroll;
        private byte mouseScrollCount, inputMouseButtonPressed;
        //private Xamarin.Forms.Image image;
        private Picture picture;
        private Random rnd = new Random();

        public MainPage()
        {
            var image = new Xamarin.Forms.Image();

            //long ww = 411;
            //long hh = 659;

            //var width = 128;
            //var height = 128;
            //var width = 411;
            //var height = 659;

            // for ...x86_Oreo(1) emulator
            var width = 1440;
            var height = 2400; //display size is 2560

            //var colorInfo = new Dictionary<(int,int),(byte,byte,byte,byte)>();
            //for(int h = 0;h < height; h++)
            //{
            //    for(int w = 0; w < width; w++)
            //    {
            //        colorInfo[(h, w)] = (255, (byte)w,(byte)h, (byte)(w * h));
            //    }
            //}

            //long ww = 128;
            //long hh = 128;
            //var colorInfo = new Dictionary<(long,long),(byte,byte,byte,byte)>();
            //for(int h = 0;h < hh; h++)
            //{
            //    for(int w = 0; w < ww; w++)
            //    {
            //        colorInfo[(h, w)] = (255, (byte)(w % 128),(byte)(h % 128), (byte)((w * h) % 128));
            //    }
            //}

            var colorInfo = new Dictionary<(int,int),(byte,byte,byte,byte)>();
            var r = rnd.Next(256);
            var g = rnd.Next(256);
            var b = rnd.Next(256);
            for (int h = 0;h < height; h++)
            {
                for(int w = 0; w < width; w++)
                {
                    colorInfo[(h, w)] = (255, (byte)r,(byte)g, (byte)b);
                }
            }
            picture = new Picture(colorInfo, width, height);

            image.Source = picture.GetImageSource();
            image.Source.BindingContext = picture;

            //picture.updateContent(colorInfo, width, height);
            //updateImageContentRandom();

            //image.Source = picture.GetImageSource();

            //var image = new Xamarin.Forms.Image
            //{
            //    HeightRequest = 200,
            //    Source = ImageSource.FromResource("RemoteDesktop.Client.Android.Images.NotConnected.png") //2
            //};


            var gr = new TapGestureRecognizer();
            gr.Tapped += (s, e) => {
                updateImageContentRandom();
                DisplayAlert("", "Tap", "OK");
            };
            image.GestureRecognizers.Add(gr);

            Content = new StackLayout
            {
              //iOSで上余白を確保
                Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0),
                Children = { image }
            };


            //Device.StartTimer(
            //        TimeSpan.FromSeconds(3),
            //        () =>
            //        {
            //            updateImageContentRandom();
            //            return true;
            //        }
            //);

            //InitializeComponent();

            ////settingsOverlay.ApplyCallback += SettingsOverlay_ApplyCallback;
            //SetConnectionUIStates(uiState);
            //inputTimer = new Timer(InputUpdate, null, 1000, 1000 / 15);
            //image.MouseMove += Image_MouseMove;
            //image.MouseDown += Image_MousePress;
            //image.MouseUp += Image_MousePress;
            ////image.MouseWheel += Image_MouseWheel;
            ////KeyDown += Window_KeyDown;
        }

        public void updateImageContentRandom()
        {
            // for ...x86_Oreo(1) emulator
            var width = 1440;
            var height = 2400; //display size is 2560

            var colorInfo = new Dictionary<(int,int),(byte,byte,byte,byte)>();
            var r = rnd.Next(256);
            var g = rnd.Next(256);
            var b = rnd.Next(256);
            for (int h = 0;h < height; h++)
            {
                for(int w = 0; w < width; w++)
                {
                    colorInfo[(h, w)] = (255, (byte)r,(byte)g, (byte)b);
                }
            }
            picture.updateContent(colorInfo, width, height);
        }

        protected override void OnSizeAllocated(double width, double height) {
            base.OnSizeAllocated(width, height);

            //var ww = 128;
            //var hh = 128;
            //var colorInfo = new Dictionary<(int,int),(byte,byte,byte,byte)>();
            //for(int h = 0;h < hh; h++)
            //{
            //    for(int w = 0; w < ww; w++)
            //    {
            //        colorInfo[(h, w)] = (255, (byte)w,(byte)h, (byte)(w * h));
            //    }
            //}
            //picture.updateContent(colorInfo, ww, hh);

            //long ww = (long)width;
            //long hh = (long)height;
            //var colorInfo = new Dictionary<(long,long),(byte,byte,byte,byte)>();
            //for(int h = 0;h < hh; h++)
            //{
            //    for(int w = 0; w < ww; w++)
            //    {
            //        colorInfo[(h, w)] = (255, (byte)(w % 255),(byte)(h % 255), (byte)((w * h) % 255));
            //    }
            //}

            //var picture = new Picture(colorInfo, ww, hh);

            //image.Source = picture.GetImageSource();
        }

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    isDisposed = true;

        //    if (networkDiscovery != null)
        //    {
        //        networkDiscovery.Dispose();
        //        networkDiscovery = null;
        //    }

        //    lock (this)
        //    {
        //        if (inputTimer != null)
        //        {
        //            inputTimer.Dispose();
        //            inputTimer = null;
        //        }

        //        if (socket != null)
        //        {
        //            socket.Dispose();
        //            socket = null;
        //        }
        //    }

        //    if (gzipStream != null)
        //    {
        //        gzipStream.Dispose();
        //        gzipStream = null;
        //    }

        //    //settingsOverlay.SaveSettings();
        //    base.OnClosing(e);
        //}

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    skipImageUpdate = true;
        //    base.OnRenderSizeChanged(sizeInfo);
        //}

        //protected override void OnLocationChanged(EventArgs e)
        //{
        //    skipImageUpdate = true;
        //    base.OnLocationChanged(e);
        //}

        //private void InputUpdate(object state)
        //{

        //    lock (this)
        //    {
        //        if (!mouseUpdate) return;
        //        mouseUpdate = false;

        //        if (connectedToLocalPC || isDisposed || uiState != UIStates.Streaming || socket == null || bitmap == null) return;

        //        Dispatcher.InvokeAsync(delegate ()
        //        {
        //            if (isDisposed || uiState != UIStates.Streaming || socket == null || bitmap == null) return;

        //            var metaData = new MetaData()
        //            {
        //                type = MetaDataTypes.UpdateMouse,
        //                mouseX = (short)((mousePoint.X / image.ActualWidth) * this.metaData.screenWidth),
        //                mouseY = (short)((mousePoint.Y / image.ActualHeight) * this.metaData.screenHeight),
        //                mouseScroll = mouseScroll,
        //                mouseButtonPressed = inputMouseButtonPressed,
        //                dataSize = -1
        //            };

        //            socket.SendMetaData(metaData);
        //        });

        //        if (mouseScrollCount == 0) mouseScroll = 0;
        //        else --mouseScrollCount;
        //    }
        //}

        //private void ApplyCommonMouseEvent(MouseEventArgs e)
        //{
        //    mousePoint = e.GetPosition(image);
        //    inputMouseButtonPressed = 0;
        //    if (e.LeftButton == MouseButtonState.Pressed) inputMouseButtonPressed = 1;
        //    else if (e.RightButton == MouseButtonState.Pressed) inputMouseButtonPressed = 2;
        //    else if (e.MiddleButton == MouseButtonState.Pressed) inputMouseButtonPressed = 3;
        //}

        //private void Image_MouseMove(object sender, MouseEventArgs e)
        //{
        //    lock (this)
        //    {
        //        ApplyCommonMouseEvent(e);
        //        mouseScroll = 0;
        //        mouseScrollCount = 0;
        //        mouseUpdate = true;
        //    }
        //}

        //private void Image_MousePress(object sender, MouseButtonEventArgs e)
        //{
        //    lock (this)
        //    {
        //        ApplyCommonMouseEvent(e);
        //        mouseScroll = 0;
        //        mouseScrollCount = 0;
        //        mouseUpdate = true;
        //    }
        //}

        //private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    lock (this)
        //    {
        //        ApplyCommonMouseEvent(e);
        //        mouseScroll = (sbyte)(e.Delta / 120);
        //        ++mouseScrollCount;
        //        mouseUpdate = true;
        //    }
        //}

        //private void Window_KeyDown(object sender, KeyEventArgs e)
        //{
        //    lock (this)
        //    {
        //        if (connectedToLocalPC || isDisposed || uiState != UIStates.Streaming || socket == null) return;

        //        byte specialKeyCode = 0, keycode = (byte)e.Key;

        //        // get special key
        //        if (Keyboard.IsKeyDown(Key.LeftShift)) specialKeyCode = (byte)Key.LeftShift;
        //        else if (Keyboard.IsKeyDown(Key.RightShift)) specialKeyCode = (byte)Key.RightShift;
        //        else if (Keyboard.IsKeyDown(Key.LeftCtrl)) specialKeyCode = (byte)Key.LeftCtrl;
        //        else if (Keyboard.IsKeyDown(Key.RightCtrl)) specialKeyCode = (byte)Key.RightCtrl;
        //        else if (Keyboard.IsKeyDown(Key.LeftAlt)) specialKeyCode = (byte)Key.LeftAlt;
        //        else if (Keyboard.IsKeyDown(Key.RightAlt)) specialKeyCode = (byte)Key.RightAlt;

        //        // make sure special key isn't the same as normal key
        //        if (specialKeyCode == keycode) specialKeyCode = 0;

        //        // send key event
        //        var metaData = new MetaData()
        //        {
        //            type = MetaDataTypes.UpdateKeyboard,
        //            keyCode = (byte)keycode,
        //            specialKeyCode = specialKeyCode,
        //            dataSize = -1
        //        };

        //        socket.SendMetaData(metaData);
        //        e.Handled = true;
        //    }
        //}

//        private void Refresh()
//        {
//            networkDiscovery = new NetworkDiscovery(NetworkTypes.Client);
//            var hosts = networkDiscovery.Find("SimpleRemoteDesktop");
//            Dispatcher.InvokeAsync(delegate ()
//            {
//                foreach (var host in hosts)
//                {
//                    serverComboBox.Items.Add(host);
//                }

//                if (hosts.Count != 0) serverComboBox.SelectedIndex = 0;
//                refreshingGrid.Visibility = Visibility.Hidden;
//            });
//        }

//        private void SetConnectionUIStates(UIStates state)
//        {
//            uiState = state;
//            fullscreenButton.IsEnabled = state == UIStates.Streaming;
//            serverComboBox.IsEnabled = state == UIStates.Stopped;
//            serverComboBox.Visibility = settingsOverlay.settings.customSocketAddress.enabled ? Visibility.Hidden : Visibility.Visible;
//            serverTextBox.Visibility = settingsOverlay.settings.customSocketAddress.enabled ? Visibility.Visible : Visibility.Hidden;
//            connectButton.Content = state != UIStates.Stopped ? (state == UIStates.Streaming ? "Pause" : "Play") : "Connect";
//            refreshButton.Content = state != UIStates.Stopped ? "Stop" : "Refresh";
//            notConnectedImage.Visibility = state == UIStates.Stopped ? Visibility.Visible : Visibility.Hidden;
//            if (state == UIStates.Stopped)
//            {
//                while (processingFrame && !isDisposed) Thread.Sleep(1);
//                if (bitmap != null)
//                {
//                    bitmap.Lock();
//                    Utils.memset(bitmap.BackBuffer, 255, (IntPtr)metaData.imageDataSize);
//                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
//                    bitmap.Unlock();
//                }
//            }
//        }

//        private void refreshButton_Click(object sender, RoutedEventArgs e)
//        {
//            // handle stop
//            if (uiState == UIStates.Streaming || uiState == UIStates.Paused)
//            {
//                SetConnectionUIStates(UIStates.Stopped);
//                var metaData = new MetaData()
//                {
//                    type = MetaDataTypes.PauseCapture,
//                    dataSize = -1
//                };

//                socket.SendMetaData(metaData);

//                Thread.Sleep(1000);
//                lock (this)
//                {
//                    socket.Dispose();
//                    socket = null;
//                }

//                return;
//            }

//            // handle refresh
//            refreshingGrid.Visibility = Visibility.Visible;
//            var thread = new Thread(Refresh);
//            thread.Start();
//        }

//        private void connectButton_Click(object sender, RoutedEventArgs e)
//        {
//            // handle pause
//            if (uiState == UIStates.Streaming || uiState == UIStates.Paused)
//            {
//                var state = uiState;
//                SetConnectionUIStates(state == UIStates.Streaming ? UIStates.Paused : UIStates.Streaming);

//                var metaData = new MetaData()
//                {
//                    type = state == UIStates.Streaming ? MetaDataTypes.PauseCapture : MetaDataTypes.ResumeCapture,
//                    dataSize = -1
//                };

//                socket.SendMetaData(metaData);
//                return;
//            }

//            // handle connect
//            SetConnectionUIStates(UIStates.Streaming);

//            NetworkHost host = null;
//            if (settingsOverlay.settings.customSocketAddress.enabled)
//            {
//                if (string.IsNullOrEmpty(serverTextBox.Text))
//                {
//#if DEBUG
//					connectedToLocalPC = true;
//					host = new NetworkHost("localhost")
//					{
//						endpoints = new List<IPEndPoint>() {new IPEndPoint(IPAddress.Loopback, 8888)}
//					};
//#else
//                    return;
//#endif
//                }
//                else
//                {
//                    host = new NetworkHost(serverTextBox.Text)
//                    {
//                        endpoints = new List<IPEndPoint>() { new IPEndPoint(IPAddress.Loopback, 8888) }
//                    };

//                    connectedToLocalPC = host.name == Dns.GetHostName() || host.name.ToLower() == "localhost" || host.name == "127.0.0.1";
//                }
//            }
//            else
//            {
//                if (serverComboBox.SelectedIndex == -1)
//                {
//#if DEBUG
//					connectedToLocalPC = true;
//					host = new NetworkHost("localhost")
//					{
//						endpoints = new List<IPEndPoint>() {new IPEndPoint(IPAddress.Loopback, 8888)}
//					};
//#else
//                    return;
//#endif
//                }
//                else
//                {
//                    host = (NetworkHost)serverComboBox.SelectedValue;
//                    connectedToLocalPC = host.name == Dns.GetHostName();
//                }
//            }

//            socket = new DataSocket(NetworkTypes.Client);
//            socket.ConnectedCallback += Socket_ConnectedCallback;
//            socket.DisconnectedCallback += Socket_DisconnectedCallback;
//            socket.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
//            socket.DataRecievedCallback += Socket_DataRecievedCallback;
//            socket.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
//            socket.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
//            socket.Connect(host.endpoints[0]);
//        }

//        private PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat format)
//        {
//            switch (format)
//            {
//                case System.Drawing.Imaging.PixelFormat.Format24bppRgb: return PixelFormats.Bgr24;
//                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565: return PixelFormats.Bgr565;
//                default: throw new Exception("Unsuported format: " + format);
//            }
//        }

//        private System.Drawing.Imaging.PixelFormat ConvertPixelFormat(PixelFormat format)
//        {
//            if (format == PixelFormats.Bgr24) return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
//            else if (format == PixelFormats.Bgr565) return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
//            else throw new Exception("Unsuported format: " + format);
//        }

//        private void Socket_StartDataRecievedCallback(MetaData metaData)
//        {
//            if (metaData.type != MetaDataTypes.ImageData) throw new Exception("Invalid meta data type: " + metaData.type);
//            this.metaData = metaData;

//            processingFrame = true;

//            // init compression
//            if (metaData.compressed)
//            {
//                if (gzipStream == null) gzipStream = new MemoryStream();
//                else gzipStream.SetLength(0);
//            }

//            // invoke UI thread
//            Dispatcher.InvokeAsync(delegate ()
//            {
//                    // create bitmap
//                    if (bitmap == null || bitmap.Width != metaData.width || bitmap.Height != metaData.height || ConvertPixelFormat(bitmap.Format) != metaData.format)
//                {
//                    bitmap = new WriteableBitmap(metaData.width, metaData.height, 96, 96, ConvertPixelFormat(metaData.format), null);
//                    image.Source = bitmap;
//                }

//                    // lock bitmap
//                    bitmap.Lock();
//                bitmapBackbuffer = bitmap.BackBuffer;
//            });
//        }

//        private unsafe void Socket_EndDataRecievedCallback()
//        {
//            if (metaData.compressed && uiState == UIStates.Streaming)
//            {
//                try
//                {
//                    gzipStream.Position = 0;
//                    using (var bitmapStream = new UnmanagedMemoryStream((byte*)bitmapBackbuffer, metaData.imageDataSize, metaData.imageDataSize, FileAccess.Write))
//                    using (var gzip = new GZipStream(gzipStream, CompressionMode.Decompress, true))
//                    {
//                        gzip.CopyTo(bitmapStream);
//                    }
//                }
//                catch (Exception e)
//                {
//                    DebugLog.LogError("Bad compressed image: " + e.Message);
//                }
//            }

//            Dispatcher.InvokeAsync(delegate ()
//            {
//                if (!skipImageUpdate) bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
//                else skipImageUpdate = false;

//                bitmap.Unlock();
//                bitmapBackbuffer = IntPtr.Zero;
//            });

//            processingFrame = false;
//        }

//        private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
//        {
//            while ((!processingFrame || bitmapBackbuffer == IntPtr.Zero) && uiState == UIStates.Streaming && !isDisposed) Thread.Sleep(1);
//            if (uiState != UIStates.Streaming || isDisposed) return;

//            if (metaData.compressed)
//            {
//                gzipStream.Write(data, 0, dataSize);
//            }
//            else
//            {
//                Marshal.Copy(data, 0, bitmapBackbuffer + offset, dataSize);
//            }
//        }

//        private void Socket_ConnectionFailedCallback(string error)
//        {
//            lock (this)
//            {
//                socket.Dispose();
//                socket = null;
//            }

//            Dispatcher.InvokeAsync(delegate ()
//            {
//                SetConnectionUIStates(UIStates.Stopped);
//            });
//        }

//        private void ApplySettings(MetaDataTypes type)
//        {
//            lock (this)
//            {
//                if (isDisposed || socket == null) return;

//                var metaData = new MetaData()
//                {
//                    type = type,
//                    compressed = settingsOverlay.settings.compressImageFrames,
//                    resolutionScale = settingsOverlay.settings.imageScale,
//                    screenIndex = 0,
//                    format = settingsOverlay.settings.imageBit == 24 ? System.Drawing.Imaging.PixelFormat.Format24bppRgb : System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
//                    targetFPS = (byte)settingsOverlay.settings.targetFPS,
//                    dataSize = -1
//                };

//                socket.SendMetaData(metaData);
//            }
//        }

//        private void Socket_ConnectedCallback()
//        {
//            ApplySettings(MetaDataTypes.StartCapture);
//        }

//        private void Socket_DisconnectedCallback()
//        {
//            lock (this)
//            {
//                socket.Dispose();
//                socket = null;
//            }

//            Dispatcher.InvokeAsync(delegate ()
//            {
//                SetConnectionUIStates(UIStates.Stopped);
//            });
//        }

        //private void SettingsOverlay_ApplyCallback()
        //{
        //    ApplySettings(MetaDataTypes.UpdateSettings);
        //    SetConnectionUIStates(uiState);
        //}

        //private void settingsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    settingsOverlay.Show();
        //}

        //private void fullscreenButton_Click(object sender, RoutedEventArgs e)
        //{
        //    WindowStyle = WindowStyle.None;
        //    WindowState = WindowState.Maximized;
        //    ResizeMode = ResizeMode.NoResize;
        //    fullscreenCloseButton.Visibility = Visibility.Visible;
        //    lastImageThickness = imageBorder.Margin;
        //    imageBorder.Margin = new Thickness();
        //    imageBorder.BorderThickness = new Thickness();
        //    toolGrid.Visibility = Visibility.Hidden;
        //}

        //private void fullscreenCloseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    WindowStyle = WindowStyle.SingleBorderWindow;
        //    WindowState = WindowState.Normal;
        //    ResizeMode = ResizeMode.CanResize;
        //    fullscreenCloseButton.Visibility = Visibility.Hidden;
        //    imageBorder.Margin = lastImageThickness;
        //    imageBorder.BorderThickness = new Thickness(1);
        //    toolGrid.Visibility = Visibility.Visible;
        //}
    }
}
