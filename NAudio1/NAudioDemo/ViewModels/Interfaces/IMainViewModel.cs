using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using NAudio.CoreAudioApi;

namespace NAudioDemo.ViewModels.Interfaces
{

    public interface IMainViewModel
    {

        string FileName
        {
            get;
            set;
        }

        ObservableCollection<MMDevice> OutputDevices
        {
            get;
        }

        MMDevice SelectedOutputDevice
        {
            get;
            set;
        }

        RelayCommand ClearCommand
        {
            get;
        }

        RelayCommand<MMDevice> StartCommand
        {
            get;
        }

        RelayCommand<MMDevice> StopCommand
        {
            get;
        }

        ObservableCollection<string> Logs
        {
            get;
        }

    }

}