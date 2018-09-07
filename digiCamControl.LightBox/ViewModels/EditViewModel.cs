using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Plugins.AdjustPanel;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ImageMagick;

namespace digiCamControl.LightBox.ViewModels
{
    public class EditViewModel:ViewModelBase, IInit
    {
        private bool _loadInProgress;
        private bool _loadRequest;

        private FileItem _selectedItem;
        private BitmapSource _bitmapSource;
        private bool _cropVisible;
        private bool _panelVisible;
        private ContentControl _panelControl;
        private bool _editIsEnabled;

        public List<IPanelItem> PanelItems { get; set; }
        public Rect CropRect => new Rect(CropX, CropY, CropWidth, CropHeight);
        public ICameraDevice CameraDevice => ServiceProvider.Instance.DeviceManager.SelectedCameraDevice;

        public Profile Session => ServiceProvider.Instance.Profile;

        public bool EditIsEnabled
        {
            get { return _editIsEnabled; }
            set
            {
                _editIsEnabled = value;
                RaisePropertyChanged();
            }
        }


        public FileItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                //_selectedItem?.Variables.CopyFrom(Session.Variables);
                _selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);

                if (SelectedItem != null)
                {
                    ServiceProvider.Instance.OnMessage(Messages.ItemChanged, null, _selectedItem);
                    EditIsEnabled = true;
                   // Session.Variables.CopyFrom(SelectedItem.Variables);
                    LoadImage(SelectedItem);
                }
                else
                {
                    EditIsEnabled = false;
                }
            }
        }

        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set
            {
                _bitmapSource = value;
                //if (_bitmapSource != null)
                //{
                //    Session.Variables["CropImageWidth"] = _bitmapSource.PixelWidth;
                //    Session.Variables["CropImageHeight"] = _bitmapSource.PixelHeight;
                //}
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

        public bool PanelVisible
        {
            get { return _panelVisible; }
            set
            {
                _panelVisible = value;
                RaisePropertyChanged(() => PanelVisible);
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


        public RelayCommand<IPanelItem> ItemCommand { get; set; }

        public RelayCommand BackCommand { get; set; }
        public RelayCommand NextCommand { get; set; }

        public EditViewModel()
        {
            BackCommand = new RelayCommand(Back);
            NextCommand = new RelayCommand(Next);
            ItemCommand = new RelayCommand<IPanelItem>(ExecuteItem);
            PanelItems = new List<IPanelItem> {new ContrastPanel(), new RemoveBackgroundPanel()};
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

        private void Next()
        {
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Export);
        }

        private void Back()
        {
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Capture);
        }

        private void LoadImage(FileItem item, bool force = false, bool loadPreview = true)
        {
            try
            {
                force = item.ReloadRequired || force;
                if (File.Exists(item.TempFile))
                {
                    item.IsBusy = true;
                    if (!File.Exists(item.PreviewProsessedFile))
                    {
                        item.PreviewProsessedFile = Path.Combine(Settings.Instance.TempFolder,
                            Path.GetRandomFileName() + ".png");
                        IMagickImage magickImage = new MagickImage(item.TempFile);

                        foreach (var plugin in ServiceProvider.Instance.PreAdjustPlugins)
                        {
                            magickImage = plugin.Execute(magickImage, Session.Variables);
                        }
                        MagickGeometry geometry = new MagickGeometry(1090, 0);
                        geometry.IgnoreAspectRatio = false;
                        magickImage.Resize(geometry);
                        magickImage.Write(item.PreviewProsessedFile);
                    }

                    if (!File.Exists(item.PreviewFile) || force)
                    {

                        IMagickImage image = new MagickImage(item.PreviewProsessedFile);

                        string fileName = Path.Combine(Settings.Instance.TempFolder,
                            Path.GetRandomFileName() + ".png");
                        item.PreviewFile = force && File.Exists(item.PreviewFile) ? item.PreviewFile : fileName;

                        foreach (var plugin in ServiceProvider.Instance.AdjustPlugins)
                        {
                            image = plugin.Execute(image, item.Variables);
                        }
                        MagickGeometry geometry = new MagickGeometry(1090, 0);
                        geometry.IgnoreAspectRatio = false;

                        image.Write(item.PreviewFile);
                        if (loadPreview)
                        {
                            var bitmap = image.ToBitmapSource();
                            bitmap.Freeze();
                            BitmapSource = bitmap;
                        }
                        geometry.Width = 200;
                        image.Sample(geometry);
                        item.Thumb = image.ToBitmapSource();
                        item.Thumb.Freeze();
                        item.ReloadRequired = false;

                    }
                    else
                    {
                        BitmapSource = Utils.LoadImage(item.PreviewFile);
                    }
                }
                item.IsBusy = false;
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load image", e);
            }
        }

        public void Init()
        {
            ServiceProvider.Instance.Message += Instance_Message;
            Session.Variables.ValueChangedEvent += Variables_ValueChangedEvent;
            RaisePropertyChanged(() => Session);
            foreach (var item in PanelItems)
            {
                (item.Panel?.DataContext as IInit)?.Init();
            }
            if (PanelItems.Count > 0)
                ExecuteItem(PanelItems[0]);
            Task.Factory.StartNew(LoadImageThumbs);
        }

        private void LoadImageThumbs()
        {
            ServiceProvider.Instance.OnMessage(Messages.SetBusy, "Loading images ...");
            try
            {
                if (Session.Files.Count > 0)
                {
                    foreach (var item in Session.Files)
                    {
                        Utils.DeleteFile(item.PreviewProsessedFile);
                        Utils.DeleteFile(item.PreviewFile);
                        LoadImage(item, true, false);
                    }
                    SelectedItem = Session.Files[0];
                }

            }
            catch (Exception e)
            {
                Log.Error("Unable to load image ",e);
            }
            ServiceProvider.Instance.OnMessage(Messages.ClearBusy);
        }

        private void Variables_ValueChangedEvent(object sender, ValueItem item)
        {
            RaisePropertyChanged(item.Name);
            RaisePropertyChanged(() => CropRect);
        }


        private void Instance_Message(object sender, MessageArgs message)
        {
            switch (message.Message)
            {
                case Messages.RefreshThumb:
                {
                    Task.Factory.StartNew(ReloadImages);
                }
                    break;
            }
        }

        public void UnInit()
        {
            ServiceProvider.Instance.Message -= Instance_Message;
            Session.Variables.ValueChangedEvent -= Variables_ValueChangedEvent;
            PanelControl = null;
            foreach (var item in PanelItems)
            {
                (item.Panel?.DataContext as IInit)?.UnInit();
            }
            foreach (var item in Session.Files)
            {
                Utils.DeleteFile(item.PreviewProsessedFile);
                Utils.DeleteFile(item.PreviewFile);
            }
        }

        private void ReloadImages()
        {
            if (_loadInProgress)
            {
                _loadRequest = true;
                return;
            }
            foreach (var item in Session.Files)
            {
                item.ReloadRequired = true;
            }
            _loadInProgress = true;
         //   SelectedItem.Variables.CopyFrom(Session.Variables);
            LoadImage(SelectedItem, true);
            if (_loadRequest)
                LoadImage(SelectedItem,true);
            _loadInProgress = false;
            _loadRequest = false;
        }
    }
}
