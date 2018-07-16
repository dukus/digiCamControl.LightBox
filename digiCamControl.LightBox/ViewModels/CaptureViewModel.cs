using System;
using System.Collections.Generic;
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
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.ViewModels
{
    public class CaptureViewModel : ViewModelBase
    {
        private LiveViewData _liveViewData;
        private object _locker = new object();
        private DispatcherTimer _Livetimer = new DispatcherTimer(DispatcherPriority.Render);
        private BitmapSource _bitmapSource;
        private ContentControl _panelControl;
        private FileItem _selectedItem;
        private bool _panelVisible;


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
                RaisePropertyChanged(()=>PanelControl);
            }
        }

        public FileItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);
                if(SelectedItem!=null)
                    LoadImage();
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

        public int CropX
        {
            get { return Session.Variables.GetInt("CropX"); }
            set
            {
                Session.Variables["CropX"] = value;
                RaisePropertyChanged(()=>CropX);
                RaisePropertyChanged(()=>CropRect);
            }
        }

        public int CropWidth
        {
            get { return Session.Variables.GetInt("CropWidth"); }
            set
            {
                Session.Variables["CropWidth"] = value;
                RaisePropertyChanged(()=>CropWidth);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropY
        {
            get { return Session.Variables.GetInt("CropY"); }
            set
            {
                Session.Variables["CropY"] = value;
                RaisePropertyChanged(() => CropY);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropHeight
        {
            get { return Session.Variables.GetInt("CropHeight"); }
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

        public Rect CropRect => new Rect(CropX, CropY, CropWidth, CropHeight);
        public ICameraDevice CameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;

        public Session Session => ServiceProvider.Instance.Session;

        public RelayCommand<IPanelItem> ItemCommand { get; set; }

        public RelayCommand BackCommand { get; set; }
        public RelayCommand NextCommand { get; set; }


        public CaptureViewModel()
        {
            _Livetimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _Livetimer.Tick += _Livetimer_Tick;
            StartLiveView();
            PanelItems = new List<IPanelItem>();
            PanelItems.Add(new LiveViewPanel());
            PanelItems.Add(new CapturePanel());
            PanelItems.Add(new CropPanel());
            PanelItems.Add(new CameraPanel());
            ItemCommand = new RelayCommand<IPanelItem>(ExecuteItem);
            BackCommand = new RelayCommand(Back);
            NextCommand=new RelayCommand(Next);
            if (!IsInDesignMode)
            {
                if (CropX == 0)
                    CropX = 400;
                if (CropY == 0)
                    CropY = 400;
                if (CropWidth == 0)
                    CropWidth = 200;
                if (CropHeight == 0)
                    CropHeight = 200;
                ServiceProvider.Instance.Message += Instance_Message;
                Session.Variables.ValueChangedEvent += Variables_ValueChangedEvent;
            }
        }

        private void Next()
        {
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout,null, ViewEnum.Adjust);
        }

        private void Back()
        {
            if ( 
                MessageBox.Show( "You are sure do you want to continue ?\nAll captured images will be lost!!!", "Warning",
                    MessageBoxButton.YesNo,MessageBoxImage.Warning,MessageBoxResult.No,MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
            {
                CleanUp();
                ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Start);
            }
        }

        private void CleanUp()
        {
            try
            {
                foreach (FileItem item in Session.Files)
                {
                    if (File.Exists(item.TempFile))
                    {
                        Utils.DeleteFile(item.TempFile);
                    }
                }
                Session.Files.Clear();
            }
            catch (Exception e)
            {
                Log.Debug("Cleanup error", e);
            }
        }

        private void Variables_ValueChangedEvent(object sender, ValueItem item)
        {
            RaisePropertyChanged(item.Name);
            RaisePropertyChanged(() => CropRect);
        }

        private void LoadImage()
        {
            try
            {
                if (File.Exists(SelectedItem.TempFile))
                {
                    ServiceProvider.Instance.OnMessage(Messages.StopLiveView);
                    BitmapSource = Utils.LoadImage(SelectedItem.TempFile, 1092);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load image",e);
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


        public  void StartLiveView()
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

                    _Livetimer.Start();

                    Log.Debug("LiveView: Liveview start done");
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
            Task.Factory.StartNew(StopLiveViewThread);
        }

        private void StopLiveViewThread()
        {
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
                        //if (Settings.Instance.OverlayShow)
                        //    DrawOverlay(bitmap);
                        //if (Settings.Instance.ShowInnerFrame)
                        //    DrawInnerFrame(bitmap);
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

        private void DrawInnerFrame(WriteableBitmap writeableBitmap)
        {
            //Color color = Colors.White;
            //color.A = 50;

            //int x1 = (int)(writeableBitmap.PixelWidth * ((Settings.Instance.InnerFrameWidth / 2.0) / 100));
            //int x2 = (int)(writeableBitmap.PixelWidth * ((100 - (Settings.Instance.InnerFrameWidth / 2.0)) / 100));
            //int y2 = (int)(writeableBitmap.PixelHeight * ((100 - (Settings.Instance.InnerFrameHeight / 2.0)) / 100));
            //int y1 = (int)(writeableBitmap.PixelHeight * ((Settings.Instance.InnerFrameHeight / 2.0) / 100));
            //Color frameColor = (Color)ColorConverter.ConvertFromString(Settings.Instance.InnerFrameColor);
            //writeableBitmap.DrawRectangle(x1, y1, x2, y2, frameColor);
            //writeableBitmap.DrawRectangle(x1 - 1, y1 - 1, x2 + 1, y2 + 1, frameColor);
            //writeableBitmap.DrawRectangle(x1 - 2, y1 - 2, x2 + 2, y2 + 2, frameColor);
            //writeableBitmap.DrawRectangle(x1 - 3, y1 - 3, x2 + 3, y2 + 3, frameColor);
            //writeableBitmap.DrawRectangle(x1 - 4, y1 - 4, x2 + 4, y2 + 4, frameColor);
        }

        private void DrawOverlay(WriteableBitmap writeableBitmap)
        {
            //Color color = Colors.White;
            //color.A = 50;

            //int x1 = (int)(writeableBitmap.PixelWidth * ((Settings.Instance.OverlayWidth / 2.0) / 100));
            //int x2 = (int)(writeableBitmap.PixelWidth * ((100 - (Settings.Instance.OverlayWidth / 2.0)) / 100));
            //int y2 = (int)(writeableBitmap.PixelHeight * ((100 - (Settings.Instance.Overlayheight / 2.0)) / 100));
            //int y1 = (int)(writeableBitmap.PixelHeight * ((Settings.Instance.Overlayheight / 2.0) / 100));
            //Color overlayColor = (Color)ColorConverter.ConvertFromString(Settings.Instance.OverlayColor);
            //overlayColor.A = (byte)(255 * (Settings.Instance.OverlayTransparency / 100.0));
            //var bm = writeableBitmap.Crop(x1, y1, x2 - x1, y2 - y1);
            //writeableBitmap.FillRectangle(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, WriteableBitmapExtensions.ConvertColor(overlayColor), true);
            //writeableBitmap.Blit(new Rect(x1, y1, x2 - x1, y2 - y1), bm, new Rect(0, 0, bm.PixelWidth, bm.PixelHeight));
        }
    }
}
