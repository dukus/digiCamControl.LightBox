﻿using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class FileItem:ViewModelBase
    {
        private BitmapSource _thumb;
        public string FileName { get; set; }
        public string TempFile { get; set; }

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
            Thumb = Utils.LoadImage(file, 240);
        }
    }
}