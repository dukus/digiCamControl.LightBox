using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using Canon.Eos.Framework;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Plugins;
using digiCamControl.LightBox.Plugins.Panel;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ImageMagick;

namespace digiCamControl.LightBox.ViewModels
{
    public class CaptureViewModel : ViewModelBase, IInit
    {
        private LiveViewData _liveViewData;
        private object _locker = new object();
        private DispatcherTimer _Livetimer = new DispatcherTimer(DispatcherPriority.Render);
        private BitmapSource _bitmapSource;
        private ContentControl _panelControl;
        private FileItem _selectedItem;
        private bool _panelVisible;
        private bool _isLiveViewEnable;
        private bool _isLiveViewPaused;
        private string _overlayFileName;

        public List<IPanelItem> PanelItems { get; set; }

        public bool CaptureWithNoAf
        {
            get { return Session.Variables.GetBool("CaptureWithNoAf"); }
            set
            {
                Session.Variables["CaptureWithNoAf"] = value;
                RaisePropertyChanged();
            }
        }


        public ContentControl PanelControl
        {
            get { return _panelControl; }
            set
            {
                _panelControl = value;
                RaisePropertyChanged(() => PanelControl);
            }
        }

        public FileItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                //  _selectedItem?.Variables.CopyFrom(Session.Variables);
                _selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);

                if (SelectedItem != null)
                {
                    //  Session.Variables.CopyFrom(SelectedItem.Variables);
                    LoadImage(SelectedItem);
                }
            }
        }


        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set
            {
                _bitmapSource = value;
                if (_bitmapSource != null)
                {
                    Session.Variables["CropImageWidth"] = _bitmapSource.PixelWidth;
                    Session.Variables["CropImageHeight"] = _bitmapSource.PixelHeight;
                }
                RaisePropertyChanged(() => BitmapSource);
            }
        }

        public double LiveViewOverlayTransparency
        {
            get { return Session.Variables.GetDouble("LiveViewOverlayTransparency", 1); }
            set
            {
                Session.Variables["LiveViewOverlayTransparency"] = value;
                RaisePropertyChanged(() => LiveViewOverlayTransparency);
            }
        }

        public string OverlayFileName => Session.Variables.GetString("OverlayFileName");

        public int CropX
        {
            get { return Session.Variables.GetInt("CropX", 400); }
            set
            {
                Session.Variables["CropX"] = value;
                RaisePropertyChanged(() => CropX);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropWidth
        {
            get { return Session.Variables.GetInt("CropWidth", 200); }
            set
            {
                Session.Variables["CropWidth"] = value;
                RaisePropertyChanged(() => CropWidth);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropY
        {
            get { return Session.Variables.GetInt("CropY", 400); }
            set
            {
                Session.Variables["CropY"] = value;
                RaisePropertyChanged(() => CropY);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropHeight
        {
            get { return Session.Variables.GetInt("CropHeight", 200); }
            set
            {
                Session.Variables["CropHeight"] = value;
                RaisePropertyChanged(() => CropHeight);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public bool CropVisible
        {
            get { return Session.Variables.GetBool("CropVisible"); }
            set
            {
                Session.Variables["CropVisible"] = value;
                RaisePropertyChanged();
            }
        }

        public bool PanelVisible
        {
            get { return _panelVisible; }
            set
            {
                _panelVisible = value;
                RaisePropertyChanged(() => PanelVisible);
            }
        }

        public bool IsLiveViewEnable
        {
            get { return _isLiveViewEnable; }
            set
            {
                _isLiveViewEnable = value;
                RaisePropertyChanged();
            }
        }


        public Rect CropRect => new Rect(CropX, CropY, CropWidth, CropHeight);
        public ICameraDevice CameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;

        public Profile Session => ServiceProvider.Instance.Profile;

        public RelayCommand<IPanelItem> ItemCommand { get; set; }

        public RelayCommand BackCommand { get; set; }
        public RelayCommand NextCommand { get; set; }

        public RelayCommand DeleteAllCommand { get; set; }
        public RelayCommand<FileItem> DeleteCommand { get; set; }

        public CaptureViewModel()
        {
            _Livetimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _Livetimer.Tick += _Livetimer_Tick;

            PanelItems = new List<IPanelItem>();
            PanelItems.Add(new LiveViewPanel());
            PanelItems.Add(new CapturePanel());
            PanelItems.Add(new CropPanel());
            PanelItems.Add(new CameraPanel());
            PanelItems.Add(new LiveViewOverlay());

            ItemCommand = new RelayCommand<IPanelItem>(ExecuteItem);
            BackCommand = new RelayCommand(Back);
            NextCommand = new RelayCommand(Next);
            DeleteAllCommand = new RelayCommand(DeleteAll);
            DeleteCommand = new RelayCommand<FileItem>(Delete);
            ExecuteItem(PanelItems[1]);

        }

        private void Delete(FileItem obj)
        {
            if (SelectedItem == obj)
            {
                var i = Session.Files.IndexOf(obj) - 1;
                if (i < 0)
                    i = 0;
                SelectedItem = Session.Files.Count > 0 ? Session.Files[i] : null;
            }
            Session.Files.Remove(obj);
            obj.CleanUp();
        }

        private void DeleteAll()
        {
            if (MessageBox.Show("Do you want to delete all images from current session ?", "Warning", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Session.CleanUp();
                ServiceProvider.Instance.OnMessage(Messages.StartLiveView);
            }
        }

        private void Next()
        {
            Session.Save();
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Adjust);
        }

        private void Back()
        {
            Session.Save();
            if (Session.Files.Count > 0 &&
                MessageBox.Show("You are sure do you want to continue ?\nCurrent session captured images will be deleted!!!",
                    "Warning",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No,
                    MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
            {
                Session.CleanUp();
                Session.Save();
                ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Start);
            }
            if (Session.Files.Count == 0)
            {
                ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Start);
            }

        }

        private void Variables_ValueChangedEvent(object sender, ValueItem item)
        {
            RaisePropertyChanged(item.Name);
            RaisePropertyChanged(() => CropRect);
        }

        private void LoadImage(FileItem item)
        {
            try
            {
                if (File.Exists(item.TempFile))
                {
                    ServiceProvider.Instance.OnMessage(Messages.StopLiveView);
                    if (!File.Exists(item.PreviewFile))
                    {
                        using (MagickImage image = new MagickImage(item.TempFile))
                        {
                            string fileName = Path.Combine(Settings.Instance.TempFolder,
                                Path.GetRandomFileName() + ".jpg");
                            MagickGeometry geometry = new MagickGeometry(1090, 0);
                            geometry.IgnoreAspectRatio = false;
                            image.Sample(geometry);
                            image.Write(fileName);
                            item.PreviewFile = fileName;
                            geometry.Width = 200;
                            image.Sample(geometry);
                            item.Thumb = image.ToBitmapSource();
                            item.Thumb.Freeze();
                        }
                    }
                    BitmapSource = Utils.LoadImage(item.PreviewFile);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load image", e);
            }
        }

        private void Instance_Message(object sender, MessageArgs message)
        {
            try
            {
                switch (message.Message)
                {
                    case Messages.ImageCaptured:
                        {
                            FileItem item = new FileItem(message.ParamString);
                            item.Variables.CopyFrom(Session.Variables);
                            Session.Files.Add(item);
                            SelectedItem = item;
                        }
                        break;
                    case Messages.StopLiveView:
                        {
                            StopLiveViewThread();
                        }
                        break;
                    case Messages.StartLiveView:
                        {
                            StartLiveViewThread();
                        }
                        break;
                    case Messages.PauseLiveView:
                        {
                            _isLiveViewPaused = true;
                        }
                        break;
                    case Messages.ResumeLiveView:
                        {
                            _isLiveViewPaused = false;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Message processing error", e);
            }
        }

        private void ExecuteItem(IPanelItem obj)
        {
            try
            {

                if (obj.Panel != null)
                {
                    if (PanelControl == obj.Panel)
                    {
                        PanelControl = null;
                        PanelVisible = false;
                        return;
                    }
                    PanelControl = obj.Panel;
                    PanelVisible = true;
                }
                else
                {
                    PanelControl = null;
                    PanelVisible = false;
                    obj.Execute();
                }
            }
            catch (Exception e)
            {
                Log.Error("Eecute error", e);
            }
        }

        private void _Livetimer_Tick(object sender, EventArgs e)
        {
            GetLiveView();
        }


        public void StartLiveView()
        {
            try
            {
                string resp = CameraDevice.GetProhibitionCondition(OperationEnum.LiveView);
                if (string.IsNullOrEmpty(resp))
                {
                    Task.Factory.StartNew(StartLiveViewThread);
                }
                else
                {
                    Log.Error("Error starting live view " + resp);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error starting live view ", e);
            }
        }

        private void StartLiveViewThread()
        {
            lock (_locker)
            {
                try
                {
                    ServiceProvider.Instance.OnMessage(Messages.ClearBusy);
                    bool retry = false;
                    int retryNum = 0;
                    Log.Debug("LiveView: Liveview started");
                    do
                    {
                        try
                        {
                            CameraDevice.StartLiveView();
                            if (!CaptureWithNoAf)
                                CameraDevice.AutoFocus();
                        }
                        catch (DeviceException deviceException)
                        {
                            if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                                deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                            {
                                Thread.Sleep(100);
                                Log.Debug("Retry live view :" + deviceException.ErrorCode.ToString("X"));
                                retry = true;
                                retryNum++;
                            }
                            else
                            {
                                throw;
                            }
                        }
                        catch (EosException eosException)
                        {
                            if (eosException.EosErrorCode == EosErrorCode.DeviceBusy)
                            {
                                Thread.Sleep(100);
                                Log.Debug("Retry live view :");
                                retry = true;
                                retryNum++;
                            }
                            else
                            {
                                throw;
                            }

                        }
                    } while (retry && retryNum < 35);
                    _isLiveViewPaused = false;
                    _Livetimer.Start();
                    Log.Debug("LiveView: Liveview start done");
                    IsLiveViewEnable = true;
                }
                catch (Exception exception)
                {
                    Log.Error("Unable to start liveview !", exception);
                    StaticHelper.Instance.SystemMessage = "Unable to start liveview ! " + exception.Message;
                }
            }
        }

        public virtual void StopLiveView()
        {
            if (!_Livetimer.IsEnabled)
                return;
            Task.Factory.StartNew(StopLiveViewThread);
        }

        private void StopLiveViewThread()
        {
            if (!_Livetimer.IsEnabled)
                return;
            lock (_locker)
            {
                try
                {
                    bool retry = false;
                    int retryNum = 0;
                    Log.Debug("LiveView: Liveview stopping");
                    _Livetimer.Stop();
                    Thread.Sleep(200);
                    do
                    {
                        try
                        {
                            CameraDevice.StopLiveView();
                            Thread.Sleep(200);
                        }
                        catch (DeviceException deviceException)
                        {
                            if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                                deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                            {
                                Thread.Sleep(250);
                                Log.Debug("Retry live view stop:" + deviceException.ErrorCode.ToString("X"));
                                retry = true;
                                retryNum++;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    } while (retry && retryNum < 5);
                    IsLiveViewEnable = false;
                }
                catch (Exception exception)
                {
                    Log.Error("Unable to stop liveview !", exception);
                    StaticHelper.Instance.SystemMessage = "Unable to stop liveview ! " + exception.Message;
                }
            }
        }

        private void GetLiveView()
        {
            try
            {
                if (_isLiveViewPaused)
                    return;

                _liveViewData = CameraDevice.GetLiveViewImage();
                if (_liveViewData != null && _liveViewData.ImageData != null)
                {
                    using (
                        MemoryStream stream = new MemoryStream(_liveViewData.ImageData, _liveViewData.ImageDataPosition,
                            _liveViewData.ImageData.Length - _liveViewData.ImageDataPosition))
                    {
                        //Rotation = LiveViewData.Rotation;

                        //if (CameraDevice is CanonSDKBase)
                        //    Rotation = 90;

                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = stream;
                        bi.EndInit();
                        //bi.Freeze();
                        var bitmap = BitmapFactory.ConvertToPbgra32Format(bi);

                        bitmap.Freeze();
                        BitmapSource = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Live view error ", ex);
            }
        }

        public void Init()
        {
            RaisePropertyChanged(() => Session);
            Task.Factory.StartNew(LoadImageThumbs);

            foreach (var item in PanelItems)
            {
                (item.Panel?.DataContext as IInit)?.Init();
            }
            ServiceProvider.Instance.Message += Instance_Message;
            ServiceProvider.Instance.DeviceManager.CameraSelected += DeviceManager_CameraSelected;
            Session.Variables.ValueChangedEvent += Variables_ValueChangedEvent;
            StartLiveView();
            RaisePropertyChanged(() => CropRect);
            RaisePropertyChanged(() => CropWidth);
            RaisePropertyChanged(() => CropHeight);
            RaisePropertyChanged(() => CropX);
            RaisePropertyChanged(() => CropY);
            RaisePropertyChanged(() => CropVisible);
        }

        private void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            if (newcameraDevice != null && newcameraDevice.IsConnected)
                StartLiveView();
        }

        public void LoadImageThumbs()
        {
            ServiceProvider.Instance.OnMessage(Messages.SetBusy, "Loading images ...");
            try
            {
                foreach (var item in Session.Files)
                {
                    Utils.DeleteFile(item.PreviewFile);
                    LoadImage(item);
                }
            }
            catch (Exception e)
            {
                Log.Error("Unable to load thumbs", e);
            }
            ServiceProvider.Instance.OnMessage(Messages.ClearBusy);
        }

        public void UnInit()
        {
            ServiceProvider.Instance.Message -= Instance_Message;
            Session.Variables.ValueChangedEvent -= Variables_ValueChangedEvent;
            ServiceProvider.Instance.DeviceManager.CameraSelected -= DeviceManager_CameraSelected;
            StopLiveView();
            foreach (var item in PanelItems)
            {
                (item.Panel?.DataContext as IInit)?.UnInit();
            }

        }
    }
}
