using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Activation;
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
        public ValueItemCollection Variables { get; set; }


        public int RemoveBackgroundThreshold
        {
            get { return Variables.GetInt("RemoveBackgroundThreshold"); }
            set
            {
                Variables["RemoveBackgroundThreshold"] = value;
            }
        }

        public bool RemoveBackgroundActive
        {
            get { return Variables.GetBool("RemoveBackgroundActive"); }
            set
            {
                Variables["RemoveBackgroundActive"] = value;
            }
        }

        public double RemoveBackgroundFeather
        {
            get { return Variables.GetDouble("RemoveBackgroundFeather"); }
            set
            {
                Variables["RemoveBackgroundFeather"] = value;
            }
        }

        public IMagickImage Execute(IMagickImage image, ValueItemCollection values)
        {
            Variables = values;
            if (RemoveBackgroundActive)
            {

                image.ColorAlpha(MagickColors.Transparent);

                image.Alpha(AlphaOption.Set);
                image.VirtualPixelMethod = VirtualPixelMethod.Transparent;

                //                image.ColorFuzz = new Percentage(RemoveBackgroundThreshold);
                //                image.TransparentChroma(new MagickColor("#FFFFFF"), new MagickColor("#F0F0F0"));
                //                image.Transparent(MagickColor.FromRgb(255, 255, 255));


                var clone = image.Clone();
                clone.ColorFuzz = new Percentage(RemoveBackgroundThreshold);
                clone.TransparentChroma(new MagickColor("#FFFFFF"), new MagickColor("#F0F0F0"));
                clone.Transparent(MagickColor.FromRgb(255, 255, 255));


                if (RemoveBackgroundFeather > 0)
                {
                    clone.Scale(new Percentage(10));
                    clone.Blur(0, RemoveBackgroundFeather, Channels.Alpha);
                    clone.Resize(image.Width, image.Width);

                }
                image.Composite(clone, CompositeOperator.CopyAlpha);

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
