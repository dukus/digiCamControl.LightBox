using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ImageMagick;

namespace digiCamControl.LightBox.ViewModels
{
    public class EditViewModel:ViewModelBase, IInit
    {
        private FileItem _selectedItem;
        private BitmapSource _bitmapSource;
        private bool _cropVisible;

        public List<IPanelItem> PanelItems { get; set; }
        public Rect CropRect => new Rect(CropX, CropY, CropWidth, CropHeight);
        public ICameraDevice CameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;

        public Session Session => ServiceProvider.Instance.Session;


        public FileItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);
                if (SelectedItem != null)
                    LoadImage(SelectedItem);
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
                RaisePropertyChanged(() => CropX);
                RaisePropertyChanged(() => CropRect);
            }
        }

        public int CropWidth
        {
            get { return Session.Variables.GetInt("CropWidth"); }
            set
            {
                Session.Variables["CropWidth"] = value;
                RaisePropertyChanged(() => CropWidth);
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
            get { return _cropVisible; }
            set
            {
                _cropVisible = value;
                RaisePropertyChanged();
            }
        }


        public RelayCommand BackCommand { get; set; }
        public RelayCommand NextCommand { get; set; }

        public EditViewModel()
        {
            BackCommand = new RelayCommand(Back);
            NextCommand = new RelayCommand(Next);
        }

        private void Next()
        {
          //  ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Adjust);
        }

        private void Back()
        {
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Capture);
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
                        }
                    }
                    Stopwatch time = new Stopwatch();
                    time.Start();
                    BitmapSource = Utils.LoadImage(item.PreviewFile);
                    time.Stop();
                    Console.WriteLine(time);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load image", e);
            }
        }

        public void Init()
        {
            
        }

        public void UnInit()
        {
            
        }
    }
}
