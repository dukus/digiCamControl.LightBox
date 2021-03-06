﻿using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using ImageMagick;
using Newtonsoft.Json;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class FileItem:ViewModelBase
    {
        private BitmapSource _thumb;
        private bool _isBusy;
        public string FileName { get; set; }
        public string TempFile { get; set; }
        public string PreviewFile { get; set; }
        public string PreviewProsessedFile { get; set; }
        public int ImageNumber { get; set; }
        public bool ReloadRequired { get; set; }
        public DateTime DateTime { get; set; }

        [JsonIgnore]
        public PointCollection LuminanceHistogramPoints { get; set; }
        
        public ValueItemCollection Variables { get; set; }


        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public BitmapSource Thumb
        {
            get { return _thumb; }
            set
            {
                _thumb = value;
                RaisePropertyChanged(() => Thumb);
            }
        }


        public FileItem(string file)
        {
            TempFile = file;
            DateTime=DateTime.Now;
            Variables = new ValueItemCollection();
        }

        public FileItem()
        {
            DateTime = DateTime.Now;
            Variables = new ValueItemCollection();
        }

        public void CleanUp()
        {
            Utils.DeleteFile(TempFile);
            Utils.DeleteFile(PreviewFile);
            Utils.DeleteFile(PreviewProsessedFile);
        }


    }
}
