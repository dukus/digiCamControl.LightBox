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
    public class RemoveBackground: IAdjustPlugin
    {
        public Session Session => ServiceProvider.Instance.Session;

        public int RemoveBackgroundThreshold
        {
            get { return Session.Variables.GetInt("RemoveBackgroundThreshold"); }
            set
            {
                Session.Variables["RemoveBackgroundThreshold"] = value;
            }
        }

        public bool RemoveBackgroundActive
        {
            get { return Session.Variables.GetBool("RemoveBackgroundActive"); }
            set
            {
                Session.Variables["RemoveBackgroundActive"] = value;
            }
        }

        public double RemoveBackgroundFeather
        {
            get { return Session.Variables.GetDouble("RemoveBackgroundFeather"); }
            set
            {
                Session.Variables["RemoveBackgroundFeather"] = value;
            }
        }

        public IMagickImage Execute(IMagickImage image)
        {
            if (RemoveBackgroundActive)
            {

                image.ColorAlpha(MagickColors.White);

                image.ColorFuzz = new Percentage(RemoveBackgroundThreshold);
                image.TransparentChroma(new MagickColor("#FFFFFF"), new MagickColor("#F0F0F0"));
                image.Transparent(MagickColor.FromRgb(255, 255, 255));
                if (RemoveBackgroundFeather > 0)
                    image.Blur(0, RemoveBackgroundFeather, Channels.Alpha);

                image.Alpha(AlphaOption.Background);
                using (MagickImageCollection images = new MagickImageCollection())
                {
                    var back = new MagickImage(MagickColors.White, image.Width, image.Height);
                    images.Add(back);
                    images.Add(image);
                    image= images.Flatten().Clone();
                }
            }
            //            image.Threshold(new Percentage(RemoveBackgroundThreshold));

            //            image.Transparent(MagickColor.FromRgb(255, 255, 255));
            return image;

        }
    }
}
