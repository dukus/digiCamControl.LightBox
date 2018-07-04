using System;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class Utils
    {

        public static void CreateFolder(string filename)
        {
            var folder = Path.GetDirectoryName(filename);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public static void WaitForFile(string file)
        {
            if (!File.Exists(file))
                return;
            var retry = 15;
            while (IsFileLocked(file) && retry > 0)
            {
                Thread.Sleep(100);
                retry--;
            }
        }

        public static bool IsFileLocked(string file)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }

        public static BitmapSource LoadImage(string filename, int width = 0)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            if (width > 0)
                bi.DecodePixelWidth = width;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(filename);
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
    }
}
