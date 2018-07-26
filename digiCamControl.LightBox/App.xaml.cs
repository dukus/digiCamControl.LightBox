using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Plugins.Adjust;

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
                ServiceProvider.Instance.Configure();
                ServiceProvider.Instance.AdjustPlugins.Add(new Crop());
            }
            catch (Exception exception)
            {
                MessageBox.Show("Startup error " + exception.Message);
            }
        }
    }
}
