using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Media.Imaging;
using CameraControl.Devices;
using CameraControl.Devices.Classes;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class Utils
    {
        /// <summary>
        /// Create folder structure if not exist for the specified file name (not folder !)
        /// </summary>
        /// <param name="filename"></param>
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

        /// <summary>
        /// Try to safe delete the specifie file
        /// </summary>
        public static void DeleteFile(string file)
        {
            if (!File.Exists(file))
                return;
            WaitForFile(file);
            File.Delete(file);
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

        public static void ExecuteWithRetry(Action action, int maxRetry = 15)
        {
            bool retry = false;
            int retryNum = 0;
            do
            {
                try
                {
                    action.Invoke();
                    Thread.Sleep(200);
                }
                catch (DeviceException deviceException)
                {
                    if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                        deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                    {
                        Thread.Sleep(250);
                        retry = true;
                        retryNum++;
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (retry && retryNum < maxRetry);
        }
    }
}
