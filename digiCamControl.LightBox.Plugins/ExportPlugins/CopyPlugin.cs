using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using ImageMagick;
using SmartFormat;

namespace digiCamControl.LightBox.Plugins.ExportPlugins
{
    public class CopyPlugin : CopyPluginViewModel, IExportPlugin
    {
        public string Name => "Disk";
        public string Icon => "Folder";
        public string Description => "Copy images to a specified folder";
        public string Id => "{F1EE5D0E-9D09-4C66-8CA7-B1B59F3E70DA}";


        public bool Export(ExportItem item, FileItem fileItem, Profile profile)
        {
            ExportItem = item;
            IMagickImage magickImage = null;
            Smart.Default.Settings.ConvertCharacterStringLiterals = false;
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("ImageNumber", fileItem.ImageNumber);
            values.Add("SessionName", profile.SessionName);
            values.Add("SessionCounter", profile.SessionCounter);
            string outFilename = Smart.Format(FileNameTemplate, values);
            if (string.IsNullOrWhiteSpace(Path.GetExtension(outFilename)))
                outFilename += ".jpg";
            outFilename = Path.Combine(Folder, outFilename);
            Utils.CreateFolder(outFilename);

            // check if the file is safe for open
            // not used by any other application
            Utils.WaitForFile(fileItem.TempFile);
            try
            {
                switch (ImageSource)
                {
                    // original
                    case 0:
                        if (!Resize)
                        {
                            File.Copy(fileItem.TempFile,outFilename);
                            return true;
                        }
                        magickImage = new MagickImage(fileItem.TempFile); 
                        break;
                    // edited
                    case 1:
                        magickImage = new MagickImage(fileItem.TempFile);
                        magickImage = ServiceProvider.Instance.PreAdjustPlugins.Aggregate(magickImage, (current, plugin) => plugin.Execute(current, profile.Variables));
                        magickImage = ServiceProvider.Instance.AdjustPlugins.Aggregate(magickImage, (current, plugin) => plugin.Execute(current, fileItem.Variables));
                        break;
                    // combined
                    case 2:
                        break;

                }

                if (Resize)
                {
                    MagickGeometry geometry = new MagickGeometry(Width, Height);
                    geometry.IgnoreAspectRatio = false;
                    magickImage.Resize(geometry);
                }
                magickImage.Write(outFilename);
            }
            catch (Exception e)
            {
                Log.Debug("Export error ", e);
                return false;
            }
            return true;
        }

        public ContentControl GetConfig(ExportItem item)
        {
            return new CopyPluginView {DataContext = new CopyPluginViewModel(item)};
        }

        public ExportItem GetDefault()
        {
            ExportItem = new ExportItem();
            SetDefault();
            return ExportItem;
        }
    }
}
