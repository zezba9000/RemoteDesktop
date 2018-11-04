using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudioDemo.Models;
using NAudioDemo.Services.Interfaces;
using NAudioDemo.ViewModels.Interfaces;
using System.Runtime.InteropServices;

namespace NAudioDemo.ViewModels
{
    public static class NativeMethods
    {
        public static int SW_HIDE = 0;

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr handle, int command);
    }

    public sealed class MainViewModel : ViewModelBase, IMainViewModel
    {

        #region イベント
        #endregion

        #region フィールド

        private readonly IAudioService _AudioService;

        private AudioOutputWriter _AudioOutputWriter;

        #endregion

        #region コンストラクタ

        public MainViewModel(IAudioService audioService)
        {
            IntPtr cmdWnd = NativeMethods.GetConsoleWindow();
            NativeMethods.ShowWindow(cmdWnd, NativeMethods.SW_HIDE);

            if (audioService == null)
                throw new ArgumentNullException(nameof(audioService));

            this._AudioService = audioService;
            this._OutputDevices = new ObservableCollection<MMDevice>(audioService.GetActiveRender());
            this._Logs = new ObservableCollection<string>();
            this.SelectedOutputDevice = this._OutputDevices.FirstOrDefault();
        }

        #endregion

        #region プロパティ

        private string _FileName;

        public string FileName
        {
            get
            {
                return this._FileName;
            }
            set
            {
                this._FileName = value;
            }
        }

        private readonly ObservableCollection<MMDevice> _OutputDevices;

        public ObservableCollection<MMDevice> OutputDevices
        {
            get
            {
                return this._OutputDevices;
            }
        }

        private MMDevice _SelectedOutputDevice;

        public MMDevice SelectedOutputDevice
        {
            get
            {
                return this._SelectedOutputDevice;
            }
            set
            {
                this._SelectedOutputDevice = value;
                this.RaisePropertyChanged();

                this.Stop();
            }
        }

        private RelayCommand<MMDevice> _StartCommand;

        public RelayCommand<MMDevice> StartCommand
        {
            get
            {
                return this._StartCommand ?? new RelayCommand<MMDevice>(device =>
                {
                    try
                    {
                        this.Stop();

                        this._AudioOutputWriter = new AudioOutputWriter(this._FileName, device);
                        this._AudioOutputWriter.DataAvailable += this.AudioOutputWriterOnDataAvailable;
                        this._AudioOutputWriter.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        }

        private RelayCommand<MMDevice> _StopCommand;

        public RelayCommand<MMDevice> StopCommand
        {
            get
            {
                return this._StopCommand ?? new RelayCommand<MMDevice>(device =>
                {
                    try
                    {
                        this.Stop();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        }

        private RelayCommand _ClearCommand;

        public RelayCommand ClearCommand
        {
            get
            {
                return this._ClearCommand ?? new RelayCommand(() =>
                {
                    this._Logs.Clear();
                });
            }
        }

        private readonly ObservableCollection<string> _Logs;

        public ObservableCollection<string> Logs
        {
            get
            {
                return this._Logs;
            }
        }

        #endregion

        #region メソッド

        #region オーバーライド
        #endregion

        #region イベントハンドラ

        private void AudioOutputWriterOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this._Logs.Insert(0, $"{DateTime.Now:yyyy/MM/dd hh:mm:ss.fff} : {waveInEventArgs.BytesRecorded} bytes");
            });
        }

        #endregion

        #region ヘルパーメソッド

        private void Stop()
        {
            if (this._AudioOutputWriter != null)
            {
                this._AudioOutputWriter.DataAvailable -= this.AudioOutputWriterOnDataAvailable;

                if (this._AudioOutputWriter.IsRecording)
                {
                    this._AudioOutputWriter.Stop();
                }

                this._AudioOutputWriter.Dispose();
            }
        }

        #endregion

        #endregion

    }

}