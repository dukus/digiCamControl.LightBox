using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace digiCamControl.LightBox.Plugins.Panel
{
    public class CameraPanelViewModel:ViewModelBase,IInit
    {
        private CameraProfile _cameraProfile;
        public Profile Session => ServiceProvider.Instance.Profile;

        public ObservableCollection<CameraProfile> CameraProfiles
        {
            get { return _cameraProfiles; }
            set
            {
                _cameraProfiles = value;
                RaisePropertyChanged();
            }
        }

        private CameraProfile _noneProfile = new CameraProfile() {Id = "", Name = "(None)"};
        private ObservableCollection<CameraProfile> _cameraProfiles;

        public RelayCommand FocusFFFCommand { get; set; }
        public RelayCommand FocusFFCommand { get; set; }
        public RelayCommand FocusFCommand { get; set; }
        public RelayCommand FocusNNNCommand { get; set; }
        public RelayCommand FocusNNCommand { get; set; }
        public RelayCommand FocusNCommand { get; set; }

        public RelayCommand AddProfileCommand { get; set; }
        public RelayCommand EditProfileCommand { get; set; }
        public RelayCommand DelProfileCommand { get; set; }

        public bool CaptureWithNoAf
        {
            get { return Session.Variables.GetBool("CaptureWithNoAf"); }
            set
            {
                Session.Variables["CaptureWithNoAf"] = value;
                RaisePropertyChanged();
            }
        }

        public CameraProfile CameraProfile
        {
            get
            {
                foreach (var cameraProfile in CameraProfiles)
                {
                    if (Session.CameraProfileId == cameraProfile.Id)
                        _cameraProfile = cameraProfile;
                }
                return _cameraProfile;
            }
            set
            {
                _cameraProfile = value;
                if (_cameraProfile != null)
                {
                    Session.CameraProfileId = _cameraProfile.Id;
                    Task.Factory.StartNew(SetProfile);
                }
                RaisePropertyChanged();
            }
        }


        public ICameraDevice SelectedCameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;

        public CameraPanelViewModel()
        {
            FocusFCommand = new RelayCommand(() => Focus(FocusDirection.Far, FocusAmount.Small));
            FocusFFCommand = new RelayCommand(() => Focus(FocusDirection.Far, FocusAmount.Medium));
            FocusFFFCommand = new RelayCommand(() => Focus(FocusDirection.Far, FocusAmount.Large));
            FocusNCommand = new RelayCommand(() => Focus(FocusDirection.Near, FocusAmount.Small));
            FocusNNCommand = new RelayCommand(() => Focus(FocusDirection.Near, FocusAmount.Medium));
            FocusNNNCommand = new RelayCommand(() => Focus(FocusDirection.Near, FocusAmount.Large));

            AddProfileCommand = new RelayCommand(AddProfile);
            EditProfileCommand = new RelayCommand(EditProfile);
            DelProfileCommand = new RelayCommand(DelProfile);

            CameraProfiles = new ObservableCollection<CameraProfile>();
            try
            {
                CameraProfiles.Clear();
                CameraProfiles.Add(_noneProfile);
                var files = Directory.GetFiles(Settings.Instance.CameraProfileFolder, "*.json");
                foreach (var file in files)
                {
                    var p = CameraProfile.Load(file);
                    if (p != null)
                        CameraProfiles.Add(p);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load camera profile list", e);
            }
        }

        private void SetProfile()
        {
            if (CameraProfile != null)
            {
                ServiceProvider.Instance.OnMessage(Messages.PauseLiveView);
                ServiceProvider.Instance.OnMessage(Messages.SetBusy, "Setting camera ...");
                _cameraProfile.Set(SelectedCameraDevice);
                ServiceProvider.Instance.OnMessage(Messages.ClearBusy);
                ServiceProvider.Instance.OnMessage(Messages.ResumeLiveView);
            }
        }

        private void DelProfile()
        {
            if (string.IsNullOrWhiteSpace(CameraProfile?.Id))
                return;
            if (MessageBox.Show("Do you realy want to delete the selected profile", "Warning", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CameraProfile.Delete();
                CameraProfiles.Remove(CameraProfile);
                CameraProfile = CameraProfiles[0];
            }

        }

        private void EditProfile()
        {
            if(string.IsNullOrWhiteSpace(CameraProfile?.Id))
                return;
            var dlg = new CameraProfilerView { DataContext = new CameraProfilerViewModel(CameraProfile) };
            if (dlg.ShowDialog() == true)
            {
                CameraProfile.Save();
                RaisePropertyChanged(()=>CameraProfile);
            }
        }

        private void AddProfile()
        {
            var profile = new CameraProfile(SelectedCameraDevice) {Name = "CameraProfile"};
            var dlg = new CameraProfilerView {DataContext = new CameraProfilerViewModel(profile)};
            if (dlg.ShowDialog() == true)
            {
                profile.Save();
                CameraProfiles.Add(profile);
                CameraProfile = profile;
            }
        }

        public void Focus(FocusDirection direction, FocusAmount amount)
        {
            try
            {
                SelectedCameraDevice.Focus(direction, amount);
                CaptureWithNoAf = true;
            }
            catch (Exception e)
            {
                Log.Error("Focus error", e);
            }
        }

        public void Init()
        {
            try
            {
                CameraProfiles.Clear();
                CameraProfiles.Add(_noneProfile);

                var files = Directory.GetFiles(Settings.Instance.CameraProfileFolder, "*.json");
                foreach (var file in files)
                {
                    var p = CameraProfile.Load(file);
                    if (p != null)
                        CameraProfiles.Add(p);
                }
                RaisePropertyChanged(() => CameraProfile);
                Task.Factory.StartNew(SetProfile);
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load camera profile list", e);
            }
        }

        public void UnInit()
        {
            
        }
    }
}
