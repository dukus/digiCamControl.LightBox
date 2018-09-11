using System;
using System.Windows;
using System.Windows.Threading;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Plugins.Adjust;
using digiCamControl.LightBox.Plugins.ExportPlugins;

namespace digiCamControl.LightBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                ServiceProvider.Instance.Configure();
                ServiceProvider.Instance.PreAdjustPlugins.Add(new Crop());
                ServiceProvider.Instance.AdjustPlugins.Add(new Contrast());
                ServiceProvider.Instance.AdjustPlugins.Add(new RemoveBackground());
                ServiceProvider.Instance.ExportPlugins.Add(new CopyPlugin());
            }
            catch (Exception exception)
            {
                MessageBox.Show("Startup error " + exception.Message);
            }
        }
        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show("Unhadled error. Exiting\n"+e.Exception.Message);
            Log.Error("Unhadled ", e.Exception);
            ExitApp();
        }

        public void ExitApp()
        {
            Current?.Shutdown();
        }
    }


}
