using System;
using System.Collections.Generic;
using Xamarin.Forms;

using RemoteDesktop.Android.Core;
using System.Threading;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.IO.Compression;
using Xamarin.Forms.Xaml;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace RemoteDesktop.Client.Android
{
    enum UIStates
    {
        Stopped,
        Streaming,
        Paused
    }

    enum BITMAP_DISPLAY_COMPONENT_TAG
    {
        COMPONENT_1,
        COMPONENT_2,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private DataSocket socket;
        //private WriteableBitmap bitmap;
        //private IntPtr bitmapBackbuffer;
        private int curBitmapBufOffset = 0;
        private MetaData metaData;
        private MemoryStream gzipStream;
        //        private MemoryStream decompedStream;
        private bool skipImageUpdate, isDisposed; //, connectedToLocalPC;
        //private bool processingFrame = false;
        private UIStates uiState = UIStates.Stopped;
        //        private Thickness lastImageThickness;

        //private Timer inputTimer;
        //private bool mouseUpdate;
        //private Point mousePoint;
        //private sbyte mouseScroll;
        //private byte mouseScrollCount, inputMouseButtonPressed;

        private Xamarin.Forms.Image image1 = new Xamarin.Forms.Image();
        private Xamarin.Forms.Image image2 = new Xamarin.Forms.Image();

        private Picture bitmap1 = null;
        private Picture bitmap2 = null;
        private byte[] bitmapBuffer1 = null;
        private byte[] bitmapBuffer2 = null;
        public static Random rnd = new Random();
        // for ...x86_Oreo(1) emulator
        private int width = 1080;
        private int height = 1800; //display size is 2560
        private const string SERVER_ADDR = "192.168.0.11"; // not used
        private const int IMAGE_SERVER_PORT = 8888;

        private RTPSoundStreamPlayer player = null;
        private AbsoluteLayout layout;

        private BITMAP_DISPLAY_COMPONENT_TAG curUpdateTargetComoonentOrBuf = BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_2;
        //private IMAGE_COMPONENT_TAG curDisplayingImgComp = IMAGE_COMPONENT_TAG.IMAGE_COMPONENT_1;
//        private byte[] curBitmapBuffer = null;
        private bool isAppDisplaySizeGot = false;
        private bool isBitDisplayComponetsAdded = false;
        private int totalDisplayedFrames = 0;
        private bool isBitDisplayCompOrBufInited = false;

        private bool isUseSkia = true;
        private SKCanvasView canvas = null;
        //private SKBitmap[] bitmaps;
        private MemoryStream[] skiaBufStreams;

        public MainPage()
        {
            //InitializeComponent();

            //var gr = new TapGestureRecognizer();
            //gr.Tapped += (s, e) =>
            //{
            //    Device.BeginInvokeOnMainThread(() =>
            //    {
            //        //updateImageContentRandom();
            //    });
            //    //DisplayAlert("", "Tap", "OK");
            //};
            //image.GestureRecognizers.Add(gr);

            if (isUseSkia)
            {
                //<skia:SKCanvasView x:Name="canvasView"
                //                   VerticalOptions="FillAndExpand"
                //                   PaintSurface="OnCanvasViewPaintSurface" />
                canvas = new SKCanvasView
                {
                    VerticalOptions = LayoutOptions.FillAndExpand
                };
                canvas.PaintSurface += OnCanvasViewPaintSurface;
                // Skiaを利用する場合、ビットマップデータのバッファはここで用意してしまう
                skiaBufStreams = new MemoryStream[2];
                skiaBufStreams.SetValue(new MemoryStream(), 0);
                skiaBufStreams.SetValue(new MemoryStream(), 1);
            }
            else { 
                Dictionary<(int, int), (byte, byte, byte, byte)> colorInfo = null;
                var local_bitmap = new Picture(colorInfo, width, height);
                image1.Source = local_bitmap.GetImageSource();
                image1.Aspect = Aspect.AspectFit;
                local_bitmap = new Picture(colorInfo, width, height);
                image2.Source = local_bitmap.GetImageSource();
                image2.Aspect = Aspect.AspectFit;
            }


            //layout.Children.Add(image, new Rectangle(0, 0, width/2.5, height/2.5));
                
            layout = new AbsoluteLayout();
            Content = layout;

            ////settingsOverlay.ApplyCallback += SettingsOverlay_ApplyCallback;
            //SetConnectionUIStates(uiState);
            //inputTimer = new Timer(InputUpdate, null, 1000, 1000 / 15);
            //image.MouseMove += Image_MouseMove;
            //image.MouseDown += Image_MousePress;
            //image.MouseUp += Image_MousePress;
            ////image.MouseWheel += Image_MouseWheel;
            ////KeyDown += Window_KeyDown;


            //Utils.getLocalIP();
            //connectToSoundServer(); // start recieve sound data which playing on remote PC
            connectToImageServer(); // staart recieve captured bitmap image data 
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            if(!(isBitDisplayComponetsAdded && isBitDisplayCompOrBufInited))
            {
                return;
            }

            byte[] bitmap_data = null;
            long dataLength = -1;
            // curUpdateTargetComoonentOrBuf は既に更新中のものになっているはずなので、以下はその前提
            if(curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
            {
                bitmap_data = skiaBufStreams[1].ToArray();
                dataLength = skiaBufStreams[1].Length;
                skiaBufStreams[1].Position = 0;
            }
            else
            {
                bitmap_data = skiaBufStreams[0].ToArray();
                dataLength = skiaBufStreams[0].Length;
                skiaBufStreams[0].Position = 0;
            }
            //SKBitmap skbitmap = new SKBitmap(metaData.width, metaData.height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            SKBitmap skbitmap = new SKBitmap();

            float fit_width = metaData.width;
            float fit_height = metaData.height;

            float x_ratio = info.Height / metaData.height;
            float y_ratio = info.Width / metaData.width;
            if(x_ratio < y_ratio)
            {
                fit_width *= x_ratio;
                fit_height *= x_ratio;
            }
            else
            {
                fit_width *= y_ratio;
                fit_height *= y_ratio;
            }

            SKRect destRect = new SKRect(0, 0, fit_width, fit_height);
            //SKRect destRect = new SKRect(0, 0, metaData.width, metaData.height);
            SKRect sourceRect = new SKRect(0, 0, metaData.width, metaData.height);

            // pin the managed array so that the GC doesn't move it
            var gcHandle = GCHandle.Alloc(Utils.convertBitmapBGR24toBGRA32(bitmap_data), GCHandleType.Pinned);
 

            // install the pixels with the color type of the pixel data
            //var skinfo = new SKImageInfo(metaData.width, metaData.height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            var skinfo = new SKImageInfo(metaData.width, metaData.height, SKColorType.Bgra8888, SKAlphaType.Opaque);

            skbitmap.InstallPixels(skinfo, gcHandle.AddrOfPinnedObject(), skinfo.RowBytes, null, delegate { gcHandle.Free(); }, null);

            // Display the bitmap
            //canvas.DrawBitmap(skbitmap, sourceRect, destRect);
            canvas.Clear();
            //canvas.Scale(1, -1, 0, skbitmap.Height / 2);
            //canvas.Scale(1, -1, 0, skbitmap.Height);
            canvas.Scale(1, -1, 0, info.Height / 2);
            canvas.DrawBitmap(skbitmap, sourceRect, destRect);

            Console.WriteLine("double_image: canvas size =" + info.Width.ToString() + "x" + info.Height.ToString());
        }

        protected override void OnDisappearing()
        {
            player.togglePlayingUDP();
        }

        public void connectToSoundServer()
        {
            player = new RTPSoundStreamPlayer();
            if(player.config.protcol_mode == RTPConfiguration.ProtcolMode.TCP)
            {
                player.togglePlayingTCP();
            }
            else
            {
                player.togglePlayingUDP();
            }
        }

        //public void updateImageContentRandom()
        //{
        //    Dictionary<(int, int), (byte, byte, byte, byte)> colorInfo = null;
        //    bitmap.updateContent(colorInfo, width, height);
        //}

        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            //updateImageContentRandom();
            return true;
        }

        private void addBitmatDisplayComponentToLayout()
        {
            if (isUseSkia)
            {
                layout.Children.Add(canvas, new Rectangle(0, 0, width, height));
            }
            else
            {
                layout.Children.Add(image1, new Rectangle(0, 0, width, height));
                layout.Children.Add(image2, new Rectangle(0, 0, width, height));

                Console.WriteLine("addImageComponentToLayout: two image components added to layout");
                // 更新中対象のものは更新してからVisibleにする
                if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                {
                    //image1.Opacity = 0;
                    //image2.Opacity = 1.0;
                    image1.IsVisible = false;
                    image2.IsVisible = true;
                }
                else
                {
                    //image1.Opacity = 1.0;
                    //image2.Opacity = 0;
                    image1.IsVisible = true;
                    image2.IsVisible = false;
                }
            }
            isBitDisplayComponetsAdded = true;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            Console.WriteLine("OnSizeAllocated: " + width.ToString() + "x" + height.ToString());

            this.isAppDisplaySizeGot = true;
            this.width = (int)width;
            this.height = (int)height;
            base.OnSizeAllocated(width, height);
        }

        private void SetConnectionUIStates(UIStates state)
        {
            uiState = state;
            //if (state == UIStates.Stopped)
            //{
            //    while (processingFrame && !isDisposed) Thread.Sleep(1);
            //    if (bitmap1 != null)
            //    {
            //        Utils.fillValueByteArray(bitmapBuffer1, 255, Picture.headerSize);
            //        Utils.fillValueByteArray(bitmapBuffer2, 255, Picture.headerSize);
            //        bitmap1.setStateUpdated();
            //        bitmap2.setStateUpdated();
            //    }
            //}
        }

        private void connectToImageServer()
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
            socket.Connect(IPAddress.Parse(SERVER_ADDR), IMAGE_SERVER_PORT);
        }

        
        private void setNewBitmapDisplayComponentAndBitmap(BITMAP_DISPLAY_COMPONENT_TAG tag)
        {
            if (isUseSkia)
            {
                layout.Children.Add(canvas, new Rectangle(0, 0, width, height));
            }
            else
            {
                if (isBitDisplayComponetsAdded)
                {
                    if (tag == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                    {
                        layout.Children.Remove(image1);
                    }
                    else
                    {
                        layout.Children.Remove(image2);
                    }
                }

                if (tag == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                {
                    Console.WriteLine("double_image: image1 replaced.");
                    bitmap1 = new Picture(null, metaData.width, metaData.height);
                    bitmapBuffer1 = bitmap1.getInternalBuffer();
                    image1 = new Image
                    {
                        BindingContext = bitmap1,
                        Aspect = Aspect.AspectFit
                    };
                    image1.SetBinding(Xamarin.Forms.Image.SourceProperty, "Source");
                    image1.IsVisible = false;
                }
                else
                {
                    Console.WriteLine("double_image: image2 replaced.");
                    bitmap2 = new Picture(null, metaData.width, metaData.height);
                    bitmapBuffer2 = bitmap2.getInternalBuffer();
                    image2 = new Image
                    {
                        BindingContext = bitmap2,
                        Aspect = Aspect.AspectFit
                    };
                    image2.SetBinding(Xamarin.Forms.Image.SourceProperty, "Source");
                    image2.IsVisible = false;
                }

                if (isBitDisplayComponetsAdded)
                {
                    if (tag == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                    {
                        layout.Children.Add(image1, new Rectangle(0, 0, width, height));
                    }
                    else
                    {
                        layout.Children.Add(image2, new Rectangle(0, 0, width, height));
                    }
                }
            }
        }

        private void displayComponentOrBufferToggle()
        {
            if (isUseSkia)
            {
                // do nothing
            }
            else {
                if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                {
                    Console.WriteLine("double_image: set image2 visible @ displayImageComponentToggle");
                    image2.IsVisible = true;
                    image1.IsVisible = false;
                }
                else
                {
                    Console.WriteLine("double_image: set image1 visible @ displayImageComponentToggle");
                    image1.IsVisible = true;
                    image2.IsVisible = false;
                }
                Console.WriteLine("double_image: updateTarget=" + curUpdateTargetComoonentOrBuf.ToString() + " @ end of displayImageComponentToggle");
            }
        }

        // Imageコンポーネントへのデータ更新通知もここで行う
        private void dataUpdateTargetImageComponentToggle()
        {
            if (isUseSkia)
            {
                Console.WriteLine("double_image: updateTarget=" + curUpdateTargetComoonentOrBuf.ToString() + " @ start of dataUpdateTargetImageComponentToggle");
                if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                {
                    Console.WriteLine("double_image: rerender canvas, target <- COMPONENT2 @ dataUpdateTargetImageComponentToggle");
                    curUpdateTargetComoonentOrBuf = BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_2;
                }
                else
                {
                    Console.WriteLine("double_image: rerender canvas, target <- COMPONENT1 @ dataUpdateTargetImageComponentToggle");
                    curUpdateTargetComoonentOrBuf = BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1;
                }
                Console.WriteLine("double_image: call canvas.InvalidateSurface");
                canvas.InvalidateSurface();
            }
            else {
                Console.WriteLine("double_image: updateTarget=" + curUpdateTargetComoonentOrBuf.ToString() + " @ start of dataUpdateTargetImageComponentToggle");
                if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                {
                    Console.WriteLine("double_image: state update bitmap1, curBitmapBuffer <- bitmapBuffer2, target <- image2 @ dataUpdateTargetImageComponentToggle");
                    bitmap1.setStateUpdated();
                    //curBitmapBuffer = bitmapBuffer2;
                    curUpdateTargetComoonentOrBuf = BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_2;
                }
                else
                {
                    Console.WriteLine("double_image: state update bitmap2, curBitmapBuffer <- bitmapBuffer1, target <- image1 @ dataUpdateTargetImageComponentToggle");
                    bitmap2.setStateUpdated();
                    //curBitmapBuffer = bitmapBuffer1;
                    curUpdateTargetComoonentOrBuf = BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1;
                }
            }
        }

        private void Socket_StartDataRecievedCallback(MetaData metaData)
        {
            if (metaData.type != MetaDataTypes.ImageData) throw new Exception("Invalid meta data type: " + metaData.type);

            Console.WriteLine("recieved MetaData @ StartDataRecievedCallback");
            Console.WriteLine("double_image: frameNumber=" + metaData.mouseX.ToString());

            // 先に行われたImageコンポーネントへの更新通知によるImageコンポーネントの表示の更新が完了していない
            // 可能性があるので少し待つ
            //Thread.Sleep(200); 

            //Imageコンポーネントの差し替えにともなってバッファアドレスが変わるかもしれないので
            //待つ
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(() =>
            {
                lock (this)
                {
                    Utils.startTimeMeasure("Image_Transfer_Communication");

                    if (isAppDisplaySizeGot && (isBitDisplayComponetsAdded == false))
                    {
                        addBitmatDisplayComponentToLayout();
                    }

                    this.metaData = metaData;
                    try
                    {

                        // create bitmap
                        if (isBitDisplayCompOrBufInited == false)
                        {
                            setNewBitmapDisplayComponentAndBitmap(BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1);
                            setNewBitmapDisplayComponentAndBitmap(BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_2);

                            curUpdateTargetComoonentOrBuf = BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_2;
                            isBitDisplayCompOrBufInited = true;
                        }

                        if (isBitDisplayComponetsAdded)
                        {
                            displayComponentOrBufferToggle(); // 直前のデータ受信でデータを更新したデータもしくは、それに対応するImageコンポーネントを表示させる
                        }

                        // init compression
                        if (metaData.compressed)
                        {
                            if (gzipStream == null)
                            {
                                gzipStream = new MemoryStream();
                            }
                            else
                            {
                                gzipStream.SetLength(0);
                            }
                        }
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        tcs.SetException(ex);
                    }
                }
            });
            var task = tcs.Task;
            try
            {
                task.Wait();
            }
            catch { }
        }

        //private unsafe void Socket_EndDataRecievedCallback()
        private void Socket_EndDataRecievedCallback()
        {
            //Utils.startTimeMeasure("Image_Update");
            //var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(() =>
            {
                lock (this)
                {
                    Console.WriteLine("elapsed for image data transfer communication: " + Utils.stopMeasureAndGetElapsedMilliSeconds("Image_Transfer_Communication").ToString() + " msec");
                    try
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

                        if (metaData.compressed)
                        {
                            try
                            {
                                Utils.startTimeMeasure("Bitmap_decompress");
                                gzipStream.Position = 0;
                                using (var gzip = new GZipStream(gzipStream, CompressionMode.Decompress, true))
                                {
                                    var tmpDecompedStream = new MemoryStream();
                                    gzip.CopyTo(tmpDecompedStream);
                                    byte[] curBitmapBuffer = null;
                                    if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                                    {
                                        curBitmapBuffer = bitmapBuffer1;
                                    }
                                    else
                                    {
                                        curBitmapBuffer = bitmapBuffer2;
                                    }
                                    if (isUseSkia)
                                    {
                                        if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                                        {
                                            skiaBufStreams[0].Write(tmpDecompedStream.ToArray(), 0, metaData.imageDataSize);
                                        }
                                        else
                                        {
                                            skiaBufStreams[1].Write(tmpDecompedStream.ToArray(), 0, metaData.imageDataSize);
                                        }
                                    }
                                    else
                                    {
                                        Array.Copy(tmpDecompedStream.GetBuffer(), 0, curBitmapBuffer, Picture.headerSize, metaData.imageDataSize);
                                    }                                    

                                    Console.WriteLine("elapsed for bitmap decompress: " + Utils.stopMeasureAndGetElapsedMilliSeconds("Bitmap_decompress").ToString() + " msec"); ;
                                }
                            }
                            catch (Exception e)
                            {
                                DebugLog.LogError("Bad compressed image: " + e.Message);
                            }
                        }

                        // このメソッドの中でImageコンポーネントへの更新通知も行う
                        dataUpdateTargetImageComponentToggle();

                        curBitmapBufOffset = 0;

                        Console.WriteLine("new capture image received and update bitmap display!");
                    }
                    catch (Exception ex)
                    {
                        //tcs.SetException(ex);
                    }

                    if (isBitDisplayComponetsAdded)
                    {
                        totalDisplayedFrames += 1;
                    }

                }
            });
            //var task = tcs.Task;
            //try
            //{
            //    task.Wait();
            //}
            //catch { }
            //Console.WriteLine("elapsed for Image Update: " + Utils.stopMeasureAndGetElapsedMilliSeconds("Image_Update").ToString() + " msec");
            Console.WriteLine("image update Invoked at EndDataRecievedCallback!");
        }


        private void Socket_DataRecievedCallback(byte[] data, int dataSize, int offset)
        {
            //var tcs = new TaskCompletionSource<bool>();
            byte[] local_buf = new byte[dataSize];
            Array.Copy(data, 0, local_buf, 0, dataSize);
            Device.BeginInvokeOnMainThread(() => 
            {
                lock (this)
                {
                    try
                    {
                        //while ((!processingFrame) && uiState == UIStates.Streaming && !isDisposed) Thread.Sleep(1);
                        //if (uiState != UIStates.Streaming || isDisposed) return;

                        if (metaData.compressed)
                        {
                            gzipStream.Write(local_buf, 0, dataSize);
                        }
                        else
                        {
                            if (curBitmapBufOffset == 0)
                            {
                                curBitmapBufOffset = Picture.headerSize;
                            }

                            byte[] curBitmapBuffer = null;
                            if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                            {
                                curBitmapBuffer = bitmapBuffer1;
                            }
                            else
                            {
                                curBitmapBuffer = bitmapBuffer2;
                            }

                            if (isUseSkia)
                            {
                                if (curUpdateTargetComoonentOrBuf == BITMAP_DISPLAY_COMPONENT_TAG.COMPONENT_1)
                                {
                                    skiaBufStreams[0].Write(local_buf, 0, dataSize);
                                }
                                else
                                {
                                    skiaBufStreams[1].Write(local_buf, 0, dataSize);
                                }
                            }
                            else
                            {
                                Array.Copy(local_buf, 0, curBitmapBuffer, curBitmapBufOffset + offset, dataSize);
                            }

                        }
                        //tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        //tcs.SetException(ex);
                    }
                }
            });

            // wait until codes passed to Device.BeginInvokeOnMainThread func
            //var task = tcs.Task;
            //try
            //{
            //    task.Wait();
            //}
            //catch { }
        }

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
            if (isDisposed || socket == null) return;

            var metaData = new MetaData()
            {
                type = type,
                //compressed = false,
                compressed = true,
                resolutionScale = .5f, //.3f,
                screenIndex = 0,
                //format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
                //format = PixelFormatXama.Format24bppRgb,
                targetFPS = 1.0f,
                dataSize = -1
            };

            socket.SendMetaData(metaData);
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
