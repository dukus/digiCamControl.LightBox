using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using ImageMagick;

namespace digiCamControl.LightBox.Plugins.Adjust
{
    public class Contrast: IAdjustPlugin
    {
        public Session Session => ServiceProvider.Instance.Session;
        public ValueItemCollection Variables { get; set; }

        public int Brightness
        {
            get { return Variables.GetInt("Brightness"); }
            set
            {
                Variables["Brightness"] = value;

            }
        }

        public int Saturation
        {
            get { return Variables.GetInt("Saturation"); }
            set
            {
                Variables["Saturation"] = value;

            }
        }

        public int Hue
        {
            get { return Variables.GetInt("Hue"); }
            set
            {
                Variables["Hue"] = value;
            }
        }

        public int ContrastValue
        {
            get { return Variables.GetInt("Contrast"); }
            set
            {
                Variables["Contrast"] = value;
            }
        }

        public bool Normalize
        {
            get { return Variables.GetBool("Normalize"); }
            set
            {
                Variables["Normalize"] = value;
            }
        }


        public IMagickImage Execute(IMagickImage image, ValueItemCollection values)
        {
            Variables = values;
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
