using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Plugins.Adjust;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.Plugins.AdjustPanel
{
    public class ContrastPanelViewModel:ViewModelBase, IInit
    {
        public Session Session => ServiceProvider.Instance.Session;

        public int Brightness
        {
            get { return Session.Variables.GetInt("Brightness"); }
            set
            {
                Session.Variables["Brightness"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int Contrast
        {
            get { return Session.Variables.GetInt("Contrast"); }
            set
            {
                Session.Variables["Contrast"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int Saturation
        {
            get { return Session.Variables.GetInt("Saturation"); }
            set
            {
                Session.Variables["Saturation"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int Hue
        {
            get { return Session.Variables.GetInt("Hue"); }
            set
            {
                Session.Variables["Hue"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public bool Normalize
        {
            get { return Session.Variables.GetBool("Normalize"); }
            set
            {
                Session.Variables["Normalize"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public RelayCommand ResetCommand { get; set; }

        public ContrastPanelViewModel()
        {
            ResetCommand = new RelayCommand(Reset);
        }

        private void Reset()
        {
            Brightness = 0;
            Saturation = 0;
            Hue = 0;
            Contrast = 0;
            Normalize = false;
            ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
        }

        public void Init()
        {
            Session.Variables.ValueChangedEvent += Variables_ValueChangedEvent;
        }

        public void UnInit()
        {
            Session.Variables.ValueChangedEvent -= Variables_ValueChangedEvent;
        }

        private void Variables_ValueChangedEvent(object sender, ValueItem item)
        {
            RaisePropertyChanged(item.Name);
        }

    }
}

