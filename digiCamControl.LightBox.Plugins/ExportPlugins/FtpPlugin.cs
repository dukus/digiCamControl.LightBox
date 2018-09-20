using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
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
    public class FtpPlugin : FtpPluginViewModel, IExportPlugin
    {
        public string Name => "Ftp";
        public string Icon => "FolderNetwork";
        public string Description => "Copy images to a specified FTP location";
        public string Id => "{B9422DAA-3F7D-4717-82E8-8584CCFE8057}";

        public bool Export(ExportItem item, FileItem fileItem, Profile profile)
        {
            ExportItem = item;
            IMagickImage magickImage = null;
            Smart.Default.Settings.ConvertCharacterStringLiterals = false;

            string outFilename = Smart.Format(FileNameTemplate, GetFormatValues(item, fileItem, profile));
            if (string.IsNullOrWhiteSpace(Path.GetExtension(outFilename)))
                outFilename += ".jpg";
            string outFolder= Smart.Format(FtpFolder, GetFormatValues(item, fileItem, profile));
            // check if the file is safe for open
            // not used by any other application
            Utils.WaitForFile(fileItem.TempFile);

            try
            {
                var tempFile = Path.Combine(Settings.Instance.CacheFolder,
                    Path.GetRandomFileName() + Path.GetExtension(outFilename));

                if (ImageSource == 0 && !Resize)
                {
                    File.Copy(fileItem.TempFile, tempFile);

                }
                else
                {
                    magickImage = GetImage(item, fileItem, profile);
                    magickImage.Write(tempFile);
                }
                using (FtpClient conn = new FtpClient())
                {
                    conn.Host = FtpServer;
                    conn.Credentials = new NetworkCredential(FtpUser, FtpPass);
                    if (!string.IsNullOrWhiteSpace(outFolder))
                    {
                        conn.CreateDirectory(outFolder, true);
                        conn.SetWorkingDirectory(outFolder);
                    }

                    using (Stream ostream = conn.OpenWrite(outFilename,FtpDataType.Binary))
                    {
                        try
                        {
                            var data = File.ReadAllBytes(tempFile);
                            ostream.Write(data, 0, data.Length);
                        }
                        finally
                        {
                            ostream.Close();
                        }
                    }
                }

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
            return new FtpPluginView { DataContext = new FtpPluginViewModel(item) };
        }

        public ExportItem GetDefault()
        {
            ExportItem = new ExportItem();
            SetDefault();
            return ExportItem;
        }
    }
}
