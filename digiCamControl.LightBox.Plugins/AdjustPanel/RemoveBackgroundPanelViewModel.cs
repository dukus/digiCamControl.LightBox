using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using GalaSoft.MvvmLight;

namespace digiCamControl.LightBox.Plugins.AdjustPanel
{
    public class RemoveBackgroundPanelViewModel:ViewModelBase,IInit
    {
        public FileItem FileItem { get; set; }


        public bool RemoveBackgroundActive
        {
            get { return FileItem.Variables.GetBool("RemoveBackgroundActive"); }
            set
            {
                FileItem.Variables["RemoveBackgroundActive"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int RemoveBackgroundThreshold
        {
            get { return FileItem.Variables.GetInt("RemoveBackgroundThreshold"); }
            set
            {
                FileItem.Variables["RemoveBackgroundThreshold"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public double RemoveBackgroundFeather
        {
            get { return FileItem.Variables.GetDouble("RemoveBackgroundFeather"); }
            set
            {
                FileItem.Variables["RemoveBackgroundFeather"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public RemoveBackgroundPanelViewModel()
        {
            FileItem = new FileItem();
        }


        public void Init()
        {
            FileItem.Variables.ValueChangedEvent += Variables_ValueChangedEvent;
            ServiceProvider.Instance.Message += Instance_Message;
        }

        private void Instance_Message(object sender, MessageArgs message)
        {
            if (message.Message == Messages.ItemChanged)
            {
                FileItem = (FileItem)message.Param;
                foreach (var name in Utils.GetPropertieNames(this.GetType()))
                {
                    RaisePropertyChanged(name);
                }
            }
        }

        public void UnInit()
        {
            FileItem.Variables.ValueChangedEvent -= Variables_ValueChangedEvent;
            ServiceProvider.Instance.Message -= Instance_Message;
        }

        private void Variables_ValueChangedEvent(object sender, ValueItem item)
        {
            RaisePropertyChanged(item.Name);
        }
    }
}

