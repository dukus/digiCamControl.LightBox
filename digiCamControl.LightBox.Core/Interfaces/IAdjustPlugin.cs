using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using digiCamControl.LightBox.Core.Clasess;
using ImageMagick;

namespace digiCamControl.LightBox.Core.Interfaces
{
    public interface IAdjustPlugin
    {
        void Execute(MagickImage image);
    }
}
