using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Classes;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;


namespace digiCamControl.LightBox.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ContentControl _contentControl;
        private ICameraDevice _cameraDevice;
        private bool _transferInProgress;
        private string _title;
        private ViewEnum _currentLayout;

        public ContentControl ContentControl
        {
            get { return _contentControl; }
            set
            {
                _contentControl = value;
                RaisePropertyChanged(() => ContentControl);
            }
        }

        public bool TransferInProgress
        {
            get { return _transferInProgress; }
            set
            {
                _transferInProgress = value;
                RaisePropertyChanged(() => TransferInProgress);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        /// <summary>
        /// Gets or sets the current selected Camera .
        /// </summary>
        /// <value>
        /// The camera device.
        /// </value>
        public ICameraDevice CameraDevice => DeviceManager.SelectedCameraDevice;

        public CameraDeviceManager DeviceManager => ServiceProvider.Instance.DeviceManager;

        public MainWindowViewModel()
        {
            try
            {

                if (!IsInDesignMode)
                {
                    ServiceProvider.Instance.DeviceManager.LoadWiaDevices = false;
                    ServiceProvider.Instance.DeviceManager.AddFakeCamera();
                    ServiceProvider.Instance.DeviceManager.ConnectToCamera();
                    ServiceProvider.Instance.DeviceManager.CameraConnected += DeviceManager_CameraConnected;
                    ServiceProvider.Instance.DeviceManager.CameraDisconnected += DeviceManager_CameraDisconnected;
                    ServiceProvider.Instance.DeviceManager.CameraSelected += DeviceManager_CameraSelected;
                    ServiceProvider.Instance.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
                    ServiceProvider.Instance.Message += Instance_Message;
                    ChangeLayout(ViewEnum.Start);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unable to initialize the application ",e);
            }
        }

        private void DeviceManager_PhotoCaptured(object sender, CameraControl.Devices.Classes.PhotoCapturedEventArgs eventArgs)
        {
            var randomName = Path.GetRandomFileName() + Path.GetExtension(eventArgs.FileName);
            string tempFile = Path.Combine(Settings.Instance.TempFolder, randomName);

            try
            {
                TransferInProgress = true;
                Utils.CreateFolder(tempFile);

                Log.Debug("Transfer started");
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, tempFile);
                Log.Debug("Transfer finished " + tempFile);

                Utils.WaitForFile(tempFile);

                eventArgs.CameraDevice.DeleteObject(new DeviceObject() { Handle = eventArgs.Handle });
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);

                Log.Debug("Transfer prodedure finished ");
                if (eventArgs.CameraDevice != null)
                {
                    eventArgs.CameraDevice.IsBusy = false;
                    eventArgs.CameraDevice.TransferProgress = 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error transfer file", ex);
            }
            TransferInProgress = false;
        }

        private void Instance_Message(object sender, MessageArgs message)
        {
            switch (message.Message)
            {
                case Messages.SetBusy:
                    //IsBusy = true;
                    break;
                case Messages.ClearBusy:
                    //IsBusy = false;
                    break;
                case Messages.ChangeLayout:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ChangeLayout((ViewEnum)message.Param);
                    });
                    break;
            }
        }

        private void ChangeLayout(ViewEnum layoutEnum)
        {
            DisposeLayout();
            _currentLayout = layoutEnum;

            switch (layoutEnum)
            {

                case ViewEnum.Start:
                {
                    var view = new StartView();
                    ContentControl = view;
                    Title = "Start";
                }
                    break;
                case ViewEnum.Capture:
                {
                    var view = new CaptureView();
                    ContentControl = view;
                    Title = "Capture";
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(layoutEnum), layoutEnum, null);
            }
        }

        public void DisposeLayout()
        {
            if (ContentControl != null)
            {
                var model = ContentControl.DataContext as IDisposable;
                model?.Dispose();
            }
        }

        private void DeviceManager_CameraSelected(ICameraDevice oldcameradevice, ICameraDevice newcameradevice)
        {
            RaisePropertyChanged(() => CameraDevice);
        }

        private void DeviceManager_CameraDisconnected(ICameraDevice cameradevice)
        {
            RaisePropertyChanged(() => CameraDevice);
        }

        private void DeviceManager_CameraConnected(ICameraDevice cameradevice)
        {
            RaisePropertyChanged(() => CameraDevice);
        }
    }
}
