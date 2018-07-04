using System;
using System.Threading.Tasks;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.Plugins
{
    public class CapturePanelViewModel:ViewModelBase
    {
        public RelayCommand CaptureCommand { get; set; }

        public CapturePanelViewModel()
        {
            CaptureCommand = new RelayCommand(Capture);
        }

        private void Capture()
        {
            Task.Factory.StartNew(CaptureThread);
        }

        private void CaptureThread()
        {
            try
            {
                ServiceProvider.Instance.OnMessage(Messages.StopLiveView);
                ServiceProvider.Instance.DeviceManager.SelectedCameraDevice.CapturePhoto();
            }
            catch (Exception e)
            {
                Log.Error("Capture error", e);
            }
        }

    }
}
