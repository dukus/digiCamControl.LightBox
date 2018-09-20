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

            string outFilename = Smart.Format(FileNameTemplate, GetFormatValues(item, fileItem, profile));
            if (string.IsNullOrWhiteSpace(Path.GetExtension(outFilename)))
                outFilename += ".jpg";
            outFilename = Path.Combine(Folder, outFilename);
            Utils.CreateFolder(outFilename);

            // check if the file is safe for open
            // not used by any other application
            Utils.WaitForFile(fileItem.TempFile);
            try
            {

                if (ImageSource == 0 && !Resize)
                {
                    File.Copy(fileItem.TempFile, outFilename);
                    return true;
                }

                magickImage = GetImage(item, fileItem, profile);
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
            return new CopyPluginView { DataContext = new CopyPluginViewModel(item) };
        }

        public ExportItem GetDefault()
        {
            ExportItem = new ExportItem();
            SetDefault();
            return ExportItem;
        }
    }
}
