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
using digiCamControl.LightBox.Classes;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Plugins;
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
        private int _cropX;
        private int _cropWidth;
        private int _cropY;
        private int _cropHeight;
        private ContentControl _panelControl;


        public List<IPanelItem> PanelItems { get; set; }

        public ContentControl PanelControl
        {
            get { return _panelControl; }
            set
            {
                _panelControl = value;
                RaisePropertyChanged(()=>PanelControl);
            }
        }


        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set
            {
                _bitmapSource = value;
                RaisePropertyChanged(() => BitmapSource);
            }
        }

        public int CropX
        {
            get { return _cropX; }
            set
            {
                _cropX = value;
                RaisePropertyChanged(()=>CropX);
                RaisePropertyChanged(()=>CropRect);
            }
        }

        public int CropWidth
        {
            get { return _cropWidth; }
            set
            {
                _cropWidth = value;
                RaisePropertyChanged(()=>CropWidth);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropY
        {
            get { return _cropY; }
            set
            {
                _cropY = value;
                RaisePropertyChanged(() => CropY);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropHeight
        {
            get { return _cropHeight; }
            set
            {
                _cropHeight = value;
                RaisePropertyChanged(() => CropHeight);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public Rect CropRect => new Rect(CropX, CropY, CropWidth, CropHeight);
        public ICameraDevice CameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;
        public Session Session => ServiceProvider.Instance.Session;
        public RelayCommand<IPanelItem> ItemCommand { get; set; }


        public CaptureViewModel()
        {
            _Livetimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _Livetimer.Tick += _Livetimer_Tick;
            StartLiveView();
            CropX = 400;
            CropY = 400;
            CropWidth = 200;
            CropHeight= 200;
            PanelItems = new List<IPanelItem>();
            PanelItems.Add(new LiveViewPanel());
            ItemCommand=new RelayCommand<IPanelItem>(ExecuteItem);
        }

        private void ExecuteItem(IPanelItem obj)
        {
            
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
                                Thread.Sleep(500);
                                Log.Debug("Retry live view stop:" + deviceException.ErrorCode.ToString("X"));
                                retry = true;
                                retryNum++;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    } while (retry && retryNum < 35);
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
