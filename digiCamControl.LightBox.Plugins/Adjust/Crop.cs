using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using ImageMagick;

namespace digiCamControl.LightBox.Plugins.Adjust
{
    public class Crop : IAdjustPlugin
    {
        public Profile Session => ServiceProvider.Instance.Profile;

        public ValueItemCollection Variables { get; set; }


        public int CropX
        {
            get { return Variables.GetInt("CropX"); }
            set
            {
                Variables["CropX"] = value;
            }
        }

        public int CropWidth
        {
            get { return Variables.GetInt("CropWidth"); }
            set
            {
                Variables["CropWidth"] = value;
            }
        }

        public int CropY
        {
            get { return Variables.GetInt("CropY"); }
            set
            {
                Variables["CropY"] = value;
            }
        }

        public int CropHeight
        {
            get { return Variables.GetInt("CropHeight"); }
            set
            {
                Variables["CropHeight"] = value;
            }
        }

        public bool CropVisible
        {
            get { return Variables.GetBool("CropVisible"); }
            set
            {
                Variables["CropVisible"] = value;
            }
        }

        public IMagickImage Execute(IMagickImage image, ValueItemCollection values)
        {
            Variables = values;

            if (!CropVisible)
                return image;

            var dw = image.Width / 1000.0;
            var dh = image.Height / 1000.0;

            image.Crop((int)(CropX * dw), (int)(CropY * dh), (int)(CropWidth * dw),
                (int)(CropHeight * dh));
            image.RePage();
            return image;
        }
    }
}
