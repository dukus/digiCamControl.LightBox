using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;

namespace digiCamControl.LightBox.Plugins.AdjustPanel
{
    public class ContrastPanelViewModel:ViewModelBase
    {
        public Session Session => ServiceProvider.Instance.Session;


        public bool Brightness
        {
            get { return Session.Variables.GetBool("Brightness"); }
            set
            {
                Session.Variables["Brightness"] = value;
                RaisePropertyChanged();
            }
        }

        public bool Contrast
        {
            get { return Session.Variables.GetBool("Contrast"); }
            set
            {
                Session.Variables["Contrast"] = value;
                RaisePropertyChanged();
            }
        }
    }
}

