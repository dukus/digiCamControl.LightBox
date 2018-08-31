using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace digiCamControl.LightBox.Plugins
{
    public class CameraPanelViewModel:ViewModelBase
    {
        public Profile Session => ServiceProvider.Instance.Profile;


        public RelayCommand FocusFFFCommand { get; set; }
        public RelayCommand FocusFFCommand { get; set; }
        public RelayCommand FocusFCommand { get; set; }
        public RelayCommand FocusNNNCommand { get; set; }
        public RelayCommand FocusNNCommand { get; set; }
        public RelayCommand FocusNCommand { get; set; }

        public bool CaptureWithNoAf
        {
            get { return Session.Variables.GetBool("CaptureWithNoAf"); }
            set
            {
                Session.Variables["CaptureWithNoAf"] = value;
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
    }
}
