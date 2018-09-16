using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.Plugins.Panel
{
    public class CameraProfilerViewModel : ViewModelBase
    {
        private CameraProfile _cameraProfile;

        public CameraProfile CameraProfile
        {
            get { return _cameraProfile; }
            set
            {
                _cameraProfile = value;
                RaisePropertyChanged();
            }
        }


        public ICameraDevice CameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;
        public RelayCommand<string> ClearCommand { get; set; }


        public CameraProfilerViewModel()
        {
            ClearCommand = new RelayCommand<string>(Clear);
            CameraProfile = new CameraProfile();
        }

        private void Clear(string obj)
        {
            CameraProfile.Values[obj] = "";
            RaisePropertyChanged("CameraProfile");
        }

        public CameraProfilerViewModel(CameraProfile cameraProfile)
        {
            CameraProfile = cameraProfile;
            ClearCommand = new RelayCommand<string>(Clear);
        }

    }
}
