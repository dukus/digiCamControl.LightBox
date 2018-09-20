using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    public class EditViewModel : ViewModelBase, IInit
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

                if (value != null)
                {
                    _selectedItem = value;
                    RaisePropertyChanged(() => SelectedItem);

                    ServiceProvider.Instance.OnMessage(Messages.ItemChanged, null, _selectedItem);
                    EditIsEnabled = true;
                    // Session.Variables.CopyFrom(SelectedItem.Variables);
                    LoadImage(SelectedItem);
                }
                else
                {
                    //EditIsEnabled = false;
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
        public RelayCommand ApplyAllCommand { get; set; }
        public RelayCommand SetDefaultCommand { get; set; }
        public RelayCommand<FileItem> DeleteCommand { get; set; }
        

        public EditViewModel()
        {
            BackCommand = new RelayCommand(Back);
            NextCommand = new RelayCommand(Next);
            ItemCommand = new RelayCommand<IPanelItem>(ExecuteItem);
            ApplyAllCommand = new RelayCommand(ApplyAll);
            SetDefaultCommand = new RelayCommand(SetDefault);
            PanelItems = new List<IPanelItem> { new ContrastPanel(), new RemoveBackgroundPanel() };
            DeleteCommand=new RelayCommand<FileItem>(Delete);
        }

        private void Delete(FileItem obj)
        {
            if (SelectedItem == obj)
            {
                var i = Session.Files.IndexOf(obj)-1;
                if (i < 0)
                    i = 0;
                SelectedItem = Session.Files.Count > 0 ? Session.Files[i] : null;
            }
            Session.Files.Remove(obj);
            obj.CleanUp();
        }

        private void SetDefault()
        {
            if (SelectedItem != null)
                Session.Variables.CopyFrom(SelectedItem.Variables);
        }

        private void ApplyAll()
        {
            if (SelectedItem == null)
                return;
            foreach (var item in Session.Files)
            {
                item.Variables.CopyFrom(SelectedItem.Variables);
            }
            Task.Factory.StartNew(() => LoadImageThumbs(true));
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
                Stopwatch wach = new Stopwatch();
                wach.Start();
                force = item.ReloadRequired || force;
                if (File.Exists(item.TempFile))
                {
                    item.IsBusy = true;
                    if (!File.Exists(item.PreviewProsessedFile))
                    {
                        item.PreviewProsessedFile = Path.Combine(Settings.Instance.TempFolder,
                            Path.GetRandomFileName() + ".jpg");
                        IMagickImage magickImage = new MagickImage(item.TempFile);

                        foreach (var plugin in ServiceProvider.Instance.PreAdjustPlugins)
                        {
                            magickImage = plugin.Execute(magickImage, Session.Variables);
                        }
                        Console.WriteLine(wach.Elapsed);
                        MagickGeometry geometry = new MagickGeometry(1090, 0);
                        geometry.IgnoreAspectRatio = false;
                        magickImage.Sample(geometry);
                        magickImage.Write(item.PreviewProsessedFile);
                        Console.WriteLine(wach.Elapsed);
                    }
                    wach.Reset();
                    wach.Start();
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

                        var Blue = new int[256];
                        var Green = new int[256];
                        var Red = new int[256];
                        var Luminance = new int[256];
                        Dictionary<MagickColor, int> h = image.Histogram();
                        foreach (var i in h)
                        {
                            byte R = i.Key.R;
                            byte G = i.Key.G;
                            byte B = i.Key.B;
                            Blue[B] += i.Value;
                            Green[G] += i.Value;
                            Red[R] += i.Value;
                            int lum = (R + R + R + B + G + G + G + G) >> 3;
                            Luminance[lum] += i.Value;
                        }
                        //fileInfo.HistogramBlue = Blue;
                        //fileInfo.HistogramGreen = Green;
                        //fileInfo.HistogramRed = Red;
                        item.LuminanceHistogramPoints = ConvertToPointCollection(Luminance);
                        //fileInfo.IsLoading = false;
                        //item.FileInfo = fileInfo;


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
                        Console.WriteLine(wach.Elapsed);
                    }
                    else
                    {
                        BitmapSource = Utils.LoadImage(item.PreviewFile);
                    }
                    Console.WriteLine(wach.Elapsed);
                }
                item.IsBusy = false;
                RaisePropertyChanged(() => SelectedItem);
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load image", e);
            }
        }

        public static PointCollection ConvertToPointCollection(int[] values)
        {
            PointCollection points = new PointCollection();
            if (values == null)
            {
                points.Freeze();
                return points;
            }

            //values = SmoothHistogram(values);

            int max = values.Max();


            // first point (lower-left corner)
            points.Add(new Point(0, max));
            // middle points
            for (int i = 0; i < values.Length; i++)
            {
                points.Add(new Point(i, max - values[i]));
            }
            // last point (lower-right corner)
            points.Add(new Point(values.Length - 1, max));
            points.Freeze();
            return points;
        }

        private static int[] SmoothHistogram(int[] originalValues)
        {
            int[] smoothedValues = new int[originalValues.Length];

            double[] mask = new double[] { 0.25, 0.5, 0.25 };

            for (int bin = 1; bin < originalValues.Length - 1; bin++)
            {
                double smoothedValue = 0;
                for (int i = 0; i < mask.Length; i++)
                {
                    smoothedValue += originalValues[bin - 1 + i] * mask[i];
                }
                smoothedValues[bin] = (int)smoothedValue;
            }

            return smoothedValues;
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
            Task.Factory.StartNew(() => LoadImageThumbs(false));
        }

        private void LoadImageThumbs(bool fast)
        {
            ServiceProvider.Instance.OnMessage(Messages.SetBusy, "Loading images ...");
            try
            {
                int counter = 1;
                if (Session.Files.Count > 0)
                {
                    foreach (var item in Session.Files)
                    {
                        ServiceProvider.Instance.OnMessage(Messages.SetBusy,
                            $"Loading images ... {counter}/{Session.Files.Count}");

                        if (!fast)
                            Utils.DeleteFile(item.PreviewProsessedFile);
                        Utils.DeleteFile(item.PreviewFile);
                        LoadImage(item, true, false);
                        counter++;
                    }
                    if (!fast)
                        SelectedItem = Session.Files[0];
                }

            }
            catch (Exception e)
            {
                Log.Error("Unable to load image ", e);
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
                LoadImage(SelectedItem, true);
            _loadInProgress = false;
            _loadRequest = false;
        }
    }
}
