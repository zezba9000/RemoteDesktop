using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using NAudio.CoreAudioApi;
using NAudioDemo.ViewModels;
using NAudioDemo.ViewModels.Interfaces;

namespace NAudioDemo.DesignTimes
{

    public sealed class MainViewModel : IMainViewModel
    {

        public string FileName
        {
            get;
            set;
        }

        public ObservableCollection<MMDevice> OutputDevices
        {
            get;
        }

        public MMDevice SelectedOutputDevice
        {
            get;
            set;
        }

        public RelayCommand<MMDevice> StartCommand
        {
            get;
        }

        public RelayCommand<MMDevice> StopCommand
        {
            get;
        }

        public ObservableCollection<string> Logs
        {
            get;
        }

        public RelayCommand ClearCommand
        {
            get;
        }

    }

}