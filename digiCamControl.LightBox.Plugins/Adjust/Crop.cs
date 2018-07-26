using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using ImageMagick;

namespace digiCamControl.LightBox.Plugins.Adjust
{
    public class Crop : IAdjustPlugin
    {
        public Session Session => ServiceProvider.Instance.Session;

        public int CropX
        {
            get { return Session.Variables.GetInt("CropX"); }
            set
            {
                Session.Variables["CropX"] = value;
            }
        }

        public int CropWidth
        {
            get { return Session.Variables.GetInt("CropWidth"); }
            set
            {
                Session.Variables["CropWidth"] = value;
            }
        }

        public int CropY
        {
            get { return Session.Variables.GetInt("CropY"); }
            set
            {
                Session.Variables["CropY"] = value;
            }
        }

        public int CropHeight
        {
            get { return Session.Variables.GetInt("CropHeight"); }
            set
            {
                Session.Variables["CropHeight"] = value;
            }
        }

        public bool CropVisible
        {
            get { return Session.Variables.GetBool("CropVisible"); }
            set
            {
                Session.Variables["CropVisible"] = value;
            }
        }

        public void Execute(MagickImage image)
        {
            if (!CropVisible)
                return;

            var dw = image.Width / 1000.0;
            var dh = image.Height / 1000.0;

            image.Crop((int)(CropX * dw), (int)(CropY * dh), (int)(CropWidth * dw),
                (int)(CropHeight * dh));
            image.RePage();

        }
    }
}
