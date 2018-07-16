using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;

namespace digiCamControl.LightBox.Plugins
{
    public class CropPanelViewModel : ViewModelBase
    {
        public Session Session => ServiceProvider.Instance.Session;
        public List<string> AspectList => new List<string>() {"Free", "Custom", "1 : 1", "4 : 3", "3 : 2", "16 : 9"};
        public List<double> AspectListWidth => new List<double>() {0, 0, 1, 4, 3, 16};
        public List<double> AspectListHeigh => new List<double>() {0, 0, 1, 3, 2, 9};

        public bool CropVisible
        {
            get { return Session.Variables.GetBool("CropVisible"); }
            set
            {
                Session.Variables["CropVisible"] = value;
                RaisePropertyChanged();
            }
        }

        public int CropAspect
        {
            get { return Session.Variables.GetInt("CropAspect"); }
            set
            {
                Session.Variables["CropAspect"] = value;
                RaisePropertyChanged();
            }
        }

        public int CropWidth
        {
            get { return Session.Variables.GetInt("CropWidth"); }
            set
            {
                Session.Variables["CropWidth"] = value;
                RaisePropertyChanged(() => CropWidth);
            }
        }

        public int CropHeight
        {
            get { return Session.Variables.GetInt("CropHeight"); }
            set
            {
                Session.Variables["CropHeight"] = value;
                RaisePropertyChanged(() => CropHeight);
            }
        }

        public CropPanelViewModel()
        {
            if (!IsInDesignMode)
            {
                Session.Variables.ValueChangedEvent += Variables_ValueChangedEvent;
            }
        }

        private void Variables_ValueChangedEvent(object sender, ValueItem item)
        {
            if (CropAspect > 1)
            {
                if (item.Name == "CropAspect")
                {
                    var dw = Session.Variables.GetInt("CropImageWidth") / 1000.0;
                    var dh = Session.Variables.GetInt("CropImageHeight") / 1000.0;
                    var iw = CropWidth * dw;
                    var ih = CropHeight * dh;
                    iw = ih / (AspectListHeigh[CropAspect] / AspectListWidth[CropAspect]);
                    CropWidth = (int) (iw / dw);
                    CropHeight = (int) (ih / dh);
                }
                if (item.Name == "CropHeight")
                {
                    var dw = Session.Variables.GetInt("CropImageWidth") / 1000.0;
                    var dh = Session.Variables.GetInt("CropImageHeight") / 1000.0;
                    var iw = CropWidth * dw;
                    var ih = CropHeight * dh;
                    iw = ih / (AspectListHeigh[CropAspect] / AspectListWidth[CropAspect]);
                    CropWidth = (int) (iw / dw);
                }
                if (item.Name == "CropWidth")
                {
                    var dw = Session.Variables.GetInt("CropImageWidth") / 1000.0;
                    var dh = Session.Variables.GetInt("CropImageHeight") / 1000.0;
                    var iw = CropWidth * dw;
                    var ih = CropHeight * dh;
                    ih = iw * (AspectListHeigh[CropAspect] / AspectListWidth[CropAspect]);
                    CropHeight = (int) (ih / dh);
                }
            }
        }


    }
}
