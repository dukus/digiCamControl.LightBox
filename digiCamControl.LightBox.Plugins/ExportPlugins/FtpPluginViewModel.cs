using System;
using System.Text;
using digiCamControl.LightBox.Core.Clasess;

namespace digiCamControl.LightBox.Plugins.ExportPlugins
{
    public class FtpPluginViewModel: BaseExportPluginViewModel
    {

        public string FtpServer
        {
            get { return ExportItem.Variables.GetString("FtpServer"); }
            set
            {
                ExportItem.Variables["FtpServer"] = value;
            }
        }

        public int FtpPort
        {
            get { return ExportItem.Variables.GetInt("FtpPort",22); }
            set
            {
                ExportItem.Variables["FtpPort"] = value;
            }
        }

        public string FtpUser
        {
            get { return ExportItem.Variables.GetString("FtpUser"); }
            set
            {
                ExportItem.Variables["FtpUser"] = value;
            }
        }

        public string FtpPass
        {
            get { return Encoding.UTF8.GetString(Convert.FromBase64String(ExportItem.Variables.GetString("FtpPass"))); }
            set
            {
                ExportItem.Variables["FtpPass"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) ;
            }
        }

        public string FtpFolder
        {
            get { return ExportItem.Variables.GetString("FtpFolder"); }
            set
            {
                ExportItem.Variables["FtpFolder"] = value;
            }
        }

        public string FileNameTemplate
        {
            get { return ExportItem.Variables["FileNameTemplate"]?.ToString(); }
            set
            {
                ExportItem.Variables["FileNameTemplate"] = value;
            }
        }

        public FtpPluginViewModel()
        {
            ExportItem = new ExportItem();
        }

        public FtpPluginViewModel(ExportItem item)
        {
            ExportItem = item;
        }

        public void SetDefault()
        {
            SetBaseDefault();
            FtpPort = 22;
            FtpFolder = "/";
            FileNameTemplate = "DSC_{ImageNumber:D4}.jpg";
            FtpFolder = "/{SessionName}/{SessionCounter:D4}/";
        }
    }
}
