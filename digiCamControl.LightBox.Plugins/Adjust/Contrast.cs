using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using ImageMagick;

namespace digiCamControl.LightBox.Plugins.Adjust
{
    public class Contrast: IAdjustPlugin
    {
        public Session Session => ServiceProvider.Instance.Session;



        public int Brightness
        {
            get { return Session.Variables.GetInt("Brightness"); }
            set
            {
                Session.Variables["Brightness"] = value;

            }
        }

        public int Saturation
        {
            get { return Session.Variables.GetInt("Saturation"); }
            set
            {
                Session.Variables["Saturation"] = value;

            }
        }

        public int Hue
        {
            get { return Session.Variables.GetInt("Hue"); }
            set
            {
                Session.Variables["Hue"] = value;
            }
        }

        public int ContrastValue
        {
            get { return Session.Variables.GetInt("Contrast"); }
            set
            {
                Session.Variables["Contrast"] = value;
            }
        }

        public bool Normalize
        {
            get { return Session.Variables.GetBool("Normalize"); }
            set
            {
                Session.Variables["Normalize"] = value;
            }
        }


        public IMagickImage Execute(IMagickImage image)
        {
            if (Normalize)
                image.Normalize();
            image.BrightnessContrast(new Percentage(Brightness), new Percentage(ContrastValue));
            image.Modulate(new Percentage(100),new Percentage(Saturation+100),new Percentage(Hue+100));
            return image;
        }

        public void Reset()
        {
            Brightness = 0;
            Saturation = 0;
            Hue = 0;
            ContrastValue = 0;
            Normalize = false;
        }
    }
}
