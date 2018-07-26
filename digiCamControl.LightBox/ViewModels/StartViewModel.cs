﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace digiCamControl.LightBox.ViewModels
{
    public class StartViewModel : ViewModelBase, IInit
    {
        public Session Session => ServiceProvider.Instance.Session;

        public RelayCommand NextCommand { get; set; }

        public StartViewModel()
        {
            NextCommand = new RelayCommand(Next);
        }

        private void Next()
        {
            Session.Save();
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, "", ViewEnum.Capture);
        }

        public void Init()
        {
            
        }

        public void UnInit()
        {
            
        }
    }
}
