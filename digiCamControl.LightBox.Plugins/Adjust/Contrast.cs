using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using ImageMagick;

namespace digiCamControl.LightBox.Plugins.Adjust
{
    public class Contrast: IAdjustPlugin
    {
        public Profile Session => ServiceProvider.Instance.Profile;
        public ValueItemCollection Variables { get; set; }

        public int Brightness => Variables.GetInt("Brightness");

        public int Saturation => Variables.GetInt("Saturation");

        public int Hue => Variables.GetInt("Hue");

        public int ContrastValue => Variables.GetInt("Contrast");

        public bool Normalize => Variables.GetBool("Normalize");

        public int BlackPoint => Variables.GetInt("BlackPoint");

        public int WhitePoint => Variables.GetInt("WhitePoint", 100);

        public int MidPoint => Variables.GetInt("MidPoint");

        public bool AutoLevel => Variables.GetBool("AutoLevel");


        public IMagickImage Execute(IMagickImage image, ValueItemCollection values)
        {
            Variables = values;
            if (Normalize)
                image.Normalize();
            if (Brightness != 0 || ContrastValue != 0)
                image.BrightnessContrast(new Percentage(Brightness), new Percentage(ContrastValue));
            if (Saturation != 0 || Hue != 0)
                image.Modulate(new Percentage(100), new Percentage(Saturation + 100), new Percentage(Hue + 100));
            if (AutoLevel)
            {
                image.AutoLevel(Channels.All);
            }
            else
            {
                if (BlackPoint != 0 || WhitePoint != 100 || MidPoint != 0)
                {
                    var midpoint = 1.0;
                    if (MidPoint < 0)
                    {
                        midpoint = -MidPoint / 10.0;
                        midpoint++;
                    }
                    if (MidPoint > 0)
                    {
                        midpoint = (100 - MidPoint) / 100.0;
                    }
                    image.Level(new Percentage(BlackPoint), new Percentage(WhitePoint), midpoint);
                }
            }
            return image;
        }

    }
}
