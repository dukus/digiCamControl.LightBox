using System;
using System.Collections.Concurrent;
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
        public ICameraDevice CameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;
        private ConcurrentQueue<PhotoCapturedEventArgs> _fileStack = new ConcurrentQueue<PhotoCapturedEventArgs>();
        private bool _transferInProgress;
        private int _totalFiles;
        private int _filesProgress;
        private int _captureProgress;
        private bool _captureInProgress;
        private double _waitProgress;

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

        public double WaitProgress
        {
            get { return _waitProgress; }
            set
            {
                _waitProgress = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Indicate if a capture process (whole series is started)
        /// </summary>
        public bool CaptureInProgress
        {
            get { return _captureInProgress; }
            set
            {
                _captureInProgress = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => IsFreeToCapture);
            }
        }

        public bool IsFreeToCapture => !CaptureInProgress;

        public bool CaptureTransferAfterCapture
        {
            get { return Session.Variables.GetBool("CaptureTransferAfterCapture"); }
            set
            {
                Session.Variables["CaptureTransferAfterCapture"] = value;
                RaisePropertyChanged();
            }
        }

        public bool LeaveFileAfterTransfer
        {
            get { return Session.Variables.GetBool("LeaveFileAfterTransfer"); }
            set
            {
                Session.Variables["LeaveFileAfterTransfer"] = value;
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
            else
            {
                ServiceProvider.Instance.Session = new Session();
            }

        }

        public bool TransferInProgress
        {
            get { return _transferInProgress; }
            set
            {
                _transferInProgress = value;
                RaisePropertyChanged();
            }
        }

        public int TotalFiles
        {
            get { return _totalFiles; }
            set
            {
                _totalFiles = value;
                RaisePropertyChanged();
            }
        }

        public int FilesProgress
        {
            get { return _filesProgress; }
            set
            {
                _filesProgress = value;
                RaisePropertyChanged();
            }
        }

        public int CaptureProgress
        {
            get { return _captureProgress; }
            set
            {
                _captureProgress = value;
                RaisePropertyChanged();
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
                CaptureInProgress = true;
                if (CaptureTransferAfterCapture)
                    ServiceProvider.Instance.DeviceManager.SelectedCameraDevice.CaptureInSdRam = false;

                ServiceProvider.Instance.OnMessage(Messages.StopLiveView);
                for (int i = 0; i < CaptureCount; i++)
                {
                    CaptureProgress =  i+1;
                    if (CaptureWithNoAf)
                        Utils.ExecuteWithRetry(
                            ServiceProvider.Instance.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf);
                    else
                        Utils.ExecuteWithRetry(ServiceProvider.Instance.DeviceManager.SelectedCameraDevice.CapturePhoto);
                    if (i < CaptureCount - 1 && CaptureWait > 0)
                    {
                        WaitProgress = 0;
                        while (WaitProgress < CaptureWait-0.05)
                        {
                            WaitProgress += 0.050;
                            Thread.Sleep((int) (Math.Min(0.050, CaptureWait - WaitProgress) * 1000));
                        }
                        WaitProgress = CaptureWait;
                    }
                }
            }
            catch (DeviceException  deviceException)
            {
                ServiceProvider.Instance.OnMessage(Messages.Message, deviceException.Message);
                Log.Error("Capture error", deviceException);
            }
            catch (Exception e)
            {
                Log.Error("Capture error", e);
            }
            CaptureProgress = 0;
            CaptureInProgress = false;
            TransferFiles();
        }

        private void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventargs)
        {
            _fileStack.Enqueue(eventargs);
            if (!CaptureTransferAfterCapture)
                TransferFiles();
        }



        private void TransferFiles()
        {
            TransferInProgress = true;
            PhotoCapturedEventArgs eventargs;
            TotalFiles = _fileStack.Count;
            while (_fileStack.TryDequeue(out eventargs))
            {
                try
                {
                    FilesProgress++;
                    string fileName = Path.Combine(Settings.Instance.TempFolder,
                        Path.GetRandomFileName() + Path.GetExtension(eventargs.FileName));
                    Utils.CreateFolder(fileName);
                    eventargs.CameraDevice.TransferFile(eventargs.Handle, fileName);
                    if (!LeaveFileAfterTransfer)
                        eventargs.CameraDevice.DeleteObject(new DeviceObject() {Handle = eventargs.Handle});
                    eventargs.CameraDevice.ReleaseResurce(eventargs.Handle);
                    ServiceProvider.Instance.OnMessage(Messages.ImageCaptured, fileName);
                }
                catch (Exception e)
                {
                    Log.Debug("File transffer error", e);
                }
            }
            TransferInProgress = false;
        }

    }
}
