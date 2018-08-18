﻿using System.Collections.Generic;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.ViewModels
{
    public class ExportViewModel:ViewModelBase
    {
        public List<IExportPlugin> ExportPlugins => ServiceProvider.Instance.ExportPlugins;
        public RelayCommand<IExportPlugin> AddCommand { get; set; }
        public Session Session => ServiceProvider.Instance.Session;

        public ExportViewModel()
        {
            AddCommand = new RelayCommand<IExportPlugin>(Add);
        }

        private void Add(IExportPlugin obj)
        {
            var item = obj.GetDefault();
            item.Name = "Export => "+obj.Name;
            item.Id = obj.Id;
            item.Icon = obj.Icon;
            Session.ExportItems.Add(item);
        }
    }
}
