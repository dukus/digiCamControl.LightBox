using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.Plugins
{
    public class CapturePanelViewModel : ViewModelBase
    {
        public RelayCommand CaptureCommand { get; set; }
        public Session Session => ServiceProvider.Instance.Session;
        private List<PhotoCapturedEventArgs> _fileStack = new List<PhotoCapturedEventArgs>();
        private bool _captureDone = false;

        public bool CaptureWithNoAf
        {
            get { return Session.Variables.GetBool("CaptureWithNoAf"); }
            set
            {
                Session.Variables["CaptureWithNoAf"] = value;
                RaisePropertyChanged();
            }
        }

        public int CaptureCount
        {
            get { return Session.Variables.GetInt("CaptureCount"); }
            set
            {
                Session.Variables["CaptureCount"] = value;
                RaisePropertyChanged();
            }
        }

        public double CaptureWait
        {
            get { return Session.Variables.GetDouble("CaptureWait"); }
            set
            {
                Session.Variables["CaptureWait"] = value;
                RaisePropertyChanged();
            }
        }

        public bool CaptureTransferAfterCapture
        {
            get { return Session.Variables.GetBool("CaptureTransferAfterCapture"); }
            set
            {
                Session.Variables["CaptureTransferAfterCapture"] = value;
                RaisePropertyChanged();
            }
        }


        public CapturePanelViewModel()
        {
            CaptureCommand = new RelayCommand(Capture);
            if (!IsInDesignMode)
            {
                ServiceProvider.Instance.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
                if (CaptureCount < 1)
                    CaptureCount = 1;
                if (Math.Abs(CaptureWait) < 0.01)
                    CaptureWait = 1;
            }

        }

        private void Capture()
        {
            Task.Factory.StartNew(CaptureThread);
        }


        private void CaptureThread()
        {
            try
            {
                TransferFiles();
                if (CaptureTransferAfterCapture)
                    ServiceProvider.Instance.DeviceManager.SelectedCameraDevice.CaptureInSdRam = false;

                ServiceProvider.Instance.OnMessage(Messages.StopLiveView);
                for (int i = 0; i < CaptureCount; i++)
                {
                    _captureDone = false;
                    if (CaptureWithNoAf)
                        ServiceProvider.Instance.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
                    else
                        ServiceProvider.Instance.DeviceManager.SelectedCameraDevice.CapturePhoto();
                    if (i < CaptureCount - 1)
                        Thread.Sleep((int) (CaptureWait * 1000));
                    while (!_captureDone)
                    {
                        Thread.Sleep(1);
                    }
                }

                TransferFiles();
            }
            catch (Exception e)
            {
                Log.Error("Capture error", e);
            }
        }

        private void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventargs)
        {
            _fileStack.Add(eventargs);
            if (!CaptureTransferAfterCapture)
                TransferFiles();
            _captureDone = true;
        }



        private void TransferFiles()
        {
            foreach (var eventargs in _fileStack)
            {
                try
                {
                    string fileName = Path.Combine(Settings.Instance.TempFolder,
                        Path.GetRandomFileName() + Path.GetExtension(eventargs.FileName));
                    Utils.CreateFolder(fileName);
                    eventargs.CameraDevice.TransferFile(eventargs.Handle, fileName);
                    eventargs.CameraDevice.DeleteObject(new DeviceObject() { Handle = eventargs.Handle });
                    eventargs.CameraDevice.ReleaseResurce(eventargs.Handle);
                    ServiceProvider.Instance.OnMessage(Messages.ImageCaptured, fileName);
                }
                catch (Exception e)
                {
                    Log.Debug("File transffer error", e);
                }
            }
            _fileStack.Clear();
        }

    }
}
