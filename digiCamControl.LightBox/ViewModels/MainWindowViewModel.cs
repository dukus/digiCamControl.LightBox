using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;


namespace digiCamControl.LightBox.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ContentControl _contentControl;
        private bool _transferInProgress;
        private string _title;
        private ViewEnum _currentLayout;
        private Dictionary<ViewEnum, ContentControl> _contentControls = new Dictionary<ViewEnum, ContentControl>();

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
                    _contentControls.Add(ViewEnum.Start, new StartView());
                    _contentControls.Add(ViewEnum.Capture, new CaptureView());
                    ChangeLayout(ViewEnum.Start);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unable to initialize the application ", e);
            }
        }

        private void DeviceManager_PhotoCaptured(object sender, CameraControl.Devices.Classes.PhotoCapturedEventArgs eventArgs)
        {
            //var randomName = Path.GetRandomFileName() + Path.GetExtension(eventArgs.FileName);
            //string tempFile = Path.Combine(Settings.Instance.TempFolder, randomName);

            //try
            //{
            //    TransferInProgress = true;
            //    Utils.CreateFolder(tempFile);

            //    Log.Debug("Transfer started");
            //    eventArgs.CameraDevice.TransferFile(eventArgs.Handle, tempFile);
            //    Log.Debug("Transfer finished " + tempFile);

            //    Utils.WaitForFile(tempFile);

            //    eventArgs.CameraDevice.DeleteObject(new DeviceObject() { Handle = eventArgs.Handle });
            //    eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);

            //    Log.Debug("Transfer prodedure finished ");
            //    if (eventArgs.CameraDevice != null)
            //    {
            //        eventArgs.CameraDevice.IsBusy = false;
            //        eventArgs.CameraDevice.TransferProgress = 0;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("Error transfer file", ex);
            //}
            //TransferInProgress = false;
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
                case Messages.Message:
                    MessageBox.Show(message.ParamString);
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
                    ContentControl = _contentControls[ViewEnum.Start];
                    Title = "Start";
                }
                    break;
                case ViewEnum.Capture:
                {
                        ContentControl = _contentControls[ViewEnum.Capture];
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
