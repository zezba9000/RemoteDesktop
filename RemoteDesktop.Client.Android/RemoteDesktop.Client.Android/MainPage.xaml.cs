using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Xamarin.Forms;

using RemoteDesktop.Core;
//using System.Windows;
//using System.Windows.Input;
//using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Net;
using System.Threading.Tasks;
//using System.Runtime.InteropServices;
//using System.IO.Compression;
//using System.Text.RegularExpressions;

namespace RemoteDesktop.Client.Android
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
    public partial class MainPage : ContentPage
    {
        //private NetworkDiscovery networkDiscovery;
        private DataSocket socket;
        //private WriteableBitmap bitmap;
        //private IntPtr bitmapBackbuffer;
        private int curBitmapBufOffset = 0;
        private MetaData metaData;
        private MemoryStream gzipStream;
        private bool skipImageUpdate, processingFrame, isDisposed, connectedToLocalPC;
        private UIStates uiState = UIStates.Stopped;
        private Thickness lastImageThickness;

        //private Timer inputTimer;
        //private bool mouseUpdate;
        //private Point mousePoint;
        //private sbyte mouseScroll;
        //private byte mouseScrollCount, inputMouseButtonPressed;
        private Xamarin.Forms.Image image = new Xamarin.Forms.Image();
        private Picture bitmap = null;
        private byte[] bitmapBuffer = null;
        public static Random rnd = new Random();
        // for ...x86_Oreo(1) emulator
        private int width = 1440;
        private int height = 2400; //display size is 2560
        private const string SERVER_ADDR = "192.168.0.11";
        private const int SERVER_PORT = 8888;

        public MainPage()
        {
            //var colorInfo = new Dictionary<(int,int),(byte,byte,byte,byte)>();
            //var r = rnd.Next(256);
            //var g = rnd.Next(256);
            //var b = rnd.Next(256);
            //for (int h = 0;h < height; h++)
            //{
            //    for(int w = 0; w < width; w++)
            //    {
            //        colorInfo[(h, w)] = (255, (byte)r,(byte)g, (byte)b);
            //    }
            //}

            Dictionary<(int, int), (byte, byte, byte, byte)> colorInfo = null;
            var local_bitmap = new Picture(colorInfo, width, height);

            //imgSrc = picture.GetImageSource();
            //InitializeComponent();

            image.Source = local_bitmap.GetImageSource();
            //image.BindingContext = bitmap;
            //image.SetBinding(Xamarin.Forms.Image.SourceProperty, "Source");

            var gr = new TapGestureRecognizer();
            gr.Tapped += (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    updateImageContentRandom();
                });

                //DisplayAlert("", "Tap", "OK");
            };
            image.GestureRecognizers.Add(gr);

            Content = new StackLayout
            {
                //iOSで上余白を確保
                Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0),
                Children = { image }
            };

            // this Image content update code worked collectly
            //Device.StartTimer(
            //        TimeSpan.FromSeconds(3),
            //        () =>
            //        {
            //            updateImageContentRandom();
            //            return true;
            //        }
            //);


            ////settingsOverlay.ApplyCallback += SettingsOverlay_ApplyCallback;
            //SetConnectionUIStates(uiState);
            //inputTimer = new Timer(InputUpdate, null, 1000, 1000 / 15);
            //image.MouseMove += Image_MouseMove;
            //image.MouseDown += Image_MousePress;
            //image.MouseUp += Image_MousePress;
            ////image.MouseWheel += Image_MouseWheel;
            ////KeyDown += Window_KeyDown;

            connectToServer();
        }

        public void updateImageContentRandom()
        {
            Dictionary<(int, int), (byte, byte, byte, byte)> colorInfo = null;
            bitmap.updateContent(colorInfo, width, height);
        }

        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            updateImageContentRandom();
            return true;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
        }

        private void SetConnectionUIStates(UIStates state)
        {
            uiState = state;
            //fullscreenButton.IsEnabled = state == UIStates.Streaming;
            //serverComboBox.IsEnabled = state == UIStates.Stopped;
            //serverComboBox.Visibility = settingsOverlay.settings.customSocketAddress.enabled ? Visibility.Hidden : Visibility.Visible;
            //serverTextBox.Visibility = settingsOverlay.settings.customSocketAddress.enabled ? Visibility.Visible : Visibility.Hidden;
            //connectButton.Content = state != UIStates.Stopped ? (state == UIStates.Streaming ? "Pause" : "Play") : "Connect";
            //refreshButton.Content = state != UIStates.Stopped ? "Stop" : "Refresh";
            //notConnectedImage.Visibility = state == UIStates.Stopped ? Visibility.Visible : Visibility.Hidden;
            if (state == UIStates.Stopped)
            {
                while (processingFrame && !isDisposed) Thread.Sleep(1);
                if (bitmap != null)
                {
                    //bitmap.Lock();
                    //Utils.memset(bitmap.BackBuffer, 255, (IntPtr)metaData.imageDataSize);
                    //bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    //bitmap.Unlock();

                    Utils.fillValueByteArray(bitmapBuffer, 255, Picture.headerSize);
                    bitmap.setStateUpdated();
                }
            }
        }

        private void connectToServer()
        {
            // handle connect
            SetConnectionUIStates(UIStates.Streaming);

            socket = new DataSocket(NetworkTypes.Client);
            socket.ConnectedCallback += Socket_ConnectedCallback;
            socket.DisconnectedCallback += Socket_DisconnectedCallback;
            socket.ConnectionFailedCallback += Socket_ConnectionFailedCallback;
            socket.DataRecievedCallback += Socket_DataRecievedCallback;
            socket.StartDataRecievedCallback += Socket_StartDataRecievedCallback;
            socket.EndDataRecievedCallback += Socket_EndDataRecievedCallback;
            //socket.Connect(host.endpoints[0]);
            socket.Connect(IPAddress.Parse(SERVER_ADDR), SERVER_PORT);
        }

        private void Socket_StartDataRecievedCallback(MetaData metaData)
        {
            if (metaData.type != MetaDataTypes.ImageData) throw new Exception("Invalid meta data type: " + metaData.type);
            this.metaData = metaData;

            processingFrame = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                // create bitmap
                if (bitmap == null)
                {
                    bitmap = new Picture(null, metaData.width, metaData.height);
                    bitmapBuffer = bitmap.getInternalBuffer();
                    image.BindingContext = bitmap;
                    image.SetBinding(Xamarin.Forms.Image.SourceProperty, "Source");
                    width = metaData.width;
                    height = metaData.height;
                }
                //bitmap.setStateUpdated();
            });

            //// init compression
            //if (metaData.compressed)
            //{
            //    if (gzipStream == null) gzipStream = new MemoryStream();
            //    else gzipStream.SetLength(0);
            //}

            //// invoke UI thread
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    // create bitmap
            //    if (bitmap == null || bitmap.Width != metaData.width || bitmap.Height != metaData.height || ConvertPixelFormat(bitmap.Format) != metaData.format)
            //    {
            //        bitmap = new WriteableBitmap(metaData.width, metaData.height, 96, 96, ConvertPixelFormat(metaData.format), null);
            //        image.Source = bitmap;
            //    }

            //    // lock bitmap
            //    bitmap.Lock();
            //    bitmapBackbuffer = bitmap.BackBuffer;
            //});
        }

        //private unsafe void Socket_EndDataRecievedCallback()
        private void Socket_EndDataRecievedCallback()
        {
            //if (metaData.compressed && uiState == UIStates.Streaming)
            //{
            //    try
            //    {
            //        gzipStream.Position = 0;
            //        using (var bitmapStream = new UnmanagedMemoryStream((byte*)bitmapBackbuffer, metaData.imageDataSize, metaData.imageDataSize, FileAccess.Write))
            //        using (var gzip = new GZipStream(gzipStream, CompressionMode.Decompress, true))
            //        {
            //            gzip.CopyTo(bitmapStream);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        DebugLog.LogError("Bad compressed image: " + e.Message);
            //    }
            //}

            Device.BeginInvokeOnMainThread(() =>
            {
                if (!skipImageUpdate)
                {
                    //notify need display update area to bitmap instance
                    //bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                }
                else
                {
                    skipImageUpdate = false;
                }

                //bitmap.Unlock();
                //bitmapBackbuffer = IntPtr.Zero;

                // notify data update to Image component
                bitmap.setStateUpdated();

                curBitmapBufOffset = 0;

                Console.WriteLine("new capture image received and update bitmap object!");
                processingFrame = false;
            });
            Console.WriteLine("image update Invoked at EndDataRecievedCallback!");
        }

        //public static Task BeginInvokeOnMainThreadAsync(Action a)
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    Device.BeginInvokeOnMainThread(() => 
        //    {
        //        try {
        //            a();
        //            tcs.SetResult(true);
        //        } catch (Exception ex) {
        //            tcs.SetException(ex);
        //        }
        //    });
        //    return tcs.Task;
        //}


        private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
        {
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(() => 
            {
                try {
                    while ((!processingFrame) && uiState == UIStates.Streaming && !isDisposed) Thread.Sleep(1);
                    if (uiState != UIStates.Streaming || isDisposed) return;

                    if (curBitmapBufOffset == 0)
                    {
                        curBitmapBufOffset = Picture.headerSize;
                    }

                    Array.Copy(data, 0, bitmapBuffer, curBitmapBufOffset + offset, dataSize);
                    tcs.SetResult(true);
                } catch (Exception ex) {
                    tcs.SetException(ex);
                }
            });

            // wait until codes passed to Device.BeginInvokeOnMainThread func
            var task = tcs.Task;
            try
            {
                task.Wait();
            }
            catch { }
            //Console.WriteLine("wait task on DataReceieveCallback finished!");
        }

        //private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
        //{
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        //while ((!processingFrame || bitmapBackbuffer == IntPtr.Zero) && uiState == UIStates.Streaming && !isDisposed) Thread.Sleep(1);
        //        while ((!processingFrame) && uiState == UIStates.Streaming && !isDisposed) Thread.Sleep(1);
        //        if (uiState != UIStates.Streaming || isDisposed) return;

        //        //if (metaData.compressed)
        //        //{
        //        //    gzipStream.Write(data, 0, dataSize);
        //        //}
        //        //else
        //        //{
        //        //    Marshal.Copy(data, 0, bitmapBackbuffer + offset, dataSize);
        //        //}

        //        if (curBitmapBufOffset == 0)
        //        {
        //            curBitmapBufOffset = Picture.headerSize;
        //        }

        //        //Marshal.Copy(data, 0, curBitmapBufOffset + offset, dataSize);
        //        Array.Copy(data, 0, bitmapBuffer, curBitmapBufOffset + offset, dataSize);
        //    });
        //}

        private void Socket_ConnectionFailedCallback(string error)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //lock (this)
                //{
                socket.Dispose();
                socket = null;
                //}

                SetConnectionUIStates(UIStates.Stopped);
            });
        }

        private void ApplySettings(MetaDataTypes type)
        {
            //lock (this)
            //{
            if (isDisposed || socket == null) return;

            var metaData = new MetaData()
            {
                type = type,
                compressed = false,
                resolutionScale = .8f,
                screenIndex = 0,
                //format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
                //format = PixelFormatXama.Format24bppRgb,
                targetFPS = (byte)1,
                dataSize = -1
            };

            socket.SendMetaData(metaData);
            //}
        }

        private void Socket_ConnectedCallback()
        {
            ApplySettings(MetaDataTypes.StartCapture);
        }

        private void Socket_DisconnectedCallback()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //lock (this)
                //{
                socket.Dispose();
                socket = null;
                //}
                SetConnectionUIStates(UIStates.Stopped);
            });
        }

        //private PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat format)
        //{
        //    switch (format)
        //    {
        //        case System.Drawing.Imaging.PixelFormat.Format24bppRgb: return PixelFormats.Bgr24;
        //        case System.Drawing.Imaging.PixelFormat.Format16bppRgb565: return PixelFormats.Bgr565;
        //        default: throw new Exception("Unsuported format: " + format);
        //    }
        //}

        //private System.Drawing.Imaging.PixelFormat ConvertPixelFormat(PixelFormat format)
        //{
        //    if (format == PixelFormats.Bgr24) return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        //    else if (format == PixelFormats.Bgr565) return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
        //    else throw new Exception("Unsuported format: " + format);
        //}


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
    }
}
