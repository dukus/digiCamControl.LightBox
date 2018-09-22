using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Accord.IO;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.ViewModels
{
    public class ExportViewModel:ViewModelBase, IInit
    {
        public List<IExportPlugin> ExportPlugins => ServiceProvider.Instance.ExportPlugins;
        public Profile Profile => ServiceProvider.Instance.Profile;

        public RelayCommand<IExportPlugin> AddCommand { get; set; }
        public RelayCommand<IExportPlugin> ExportCommand { get; set; }
        public RelayCommand BackCommand { get; set; }

        public RelayCommand<ExportItem> DeleteCommand { get; set; }
        public RelayCommand<ExportItem> DuplicateCommand { get; set; }


        public ExportViewModel()
        {
            AddCommand = new RelayCommand<IExportPlugin>(Add);
            ExportCommand = new RelayCommand<IExportPlugin>(Export);
            BackCommand = new RelayCommand(Back);
            DeleteCommand = new RelayCommand<ExportItem>(Delete);
            DuplicateCommand = new RelayCommand<ExportItem>(Duplicate);
        }

        private void Duplicate(ExportItem obj)
        {
            Profile.ExportItems.Add(obj.Clone());
        }

        private void Delete(ExportItem obj)
        {
            if (MessageBox.Show("Do you realy want to delete this item ?", "Warning", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Profile.ExportItems.Remove(obj);
            }
        }

        private void Back()
        {
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Adjust);
        }

        private void Export(IExportPlugin obj)
        {
            if (Profile.ExportItems.Count == 0)
            {
                MessageBox.Show("No export action are defined !");
                return;
            }
            Task.Factory.StartNew(ExportThread);
        }

        private void ExportThread()
        {
            ServiceProvider.Instance.OnMessage(Messages.SetBusy, "Exporting images ...");
            try
            {
                foreach (var exportItem in Profile.ExportItems)
                {
                    IExportPlugin plugin = GetPlugin(exportItem.Id);
                    if (plugin == null)
                        continue;

                    for (int i = 0; i < Profile.Files.Count; i++)
                    {
                        ServiceProvider.Instance.OnMessage(Messages.SetBusy,
                            $"Loading images ... {i + 1}/{Profile.Files.Count}");
                        var fileItem = Profile.Files[i];
                        fileItem.ImageNumber = i + 1;
                        plugin.Export(exportItem, fileItem, Profile);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("Export error", e);
            }
            ServiceProvider.Instance.OnMessage(Messages.ClearBusy);
            Profile.SessionCounter++;
            Profile.CleanUp();
            Profile.Save();
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, null, ViewEnum.Start);
        }

        private IExportPlugin GetPlugin(string id)
        {
            foreach (var plugin in ServiceProvider.Instance.ExportPlugins)
            {
                if (plugin.Id == id)
                    return plugin;
            }
            return null;
        }

        private void Add(IExportPlugin obj)
        {
            var item = obj.GetDefault();
            item.Name = "Export => "+obj.Name;
            item.Id = obj.Id;
            item.Icon = obj.Icon;
            Profile.ExportItems.Add(item);
        }

        public void Init()
        {
            RaisePropertyChanged(() => Profile);
        }

        public void UnInit()
        {
            Profile.Save();
        }
    }
}
