using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;

namespace digiCamControl.LightBox.Plugins.AdjustPanel
{
    public class RemoveBackgroundPanelViewModel:ViewModelBase
    {
        public Profile Session => ServiceProvider.Instance.Profile;

        public bool RemoveBackgroundActive
        {
            get { return Session.Variables.GetBool("RemoveBackgroundActive"); }
            set
            {
                Session.Variables["RemoveBackgroundActive"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int RemoveBackgroundThreshold
        {
            get { return Session.Variables.GetInt("RemoveBackgroundThreshold"); }
            set
            {
                Session.Variables["RemoveBackgroundThreshold"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public double RemoveBackgroundFeather
        {
            get { return Session.Variables.GetDouble("RemoveBackgroundFeather"); }
            set
            {
                Session.Variables["RemoveBackgroundFeather"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }
    }
}
