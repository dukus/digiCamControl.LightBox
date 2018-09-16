using System.Windows;

namespace digiCamControl.LightBox.Plugins.Panel
{
    /// <summary>
    /// Interaction logic for CameraProfilerView.xaml
    /// </summary>
    public partial class CameraProfilerView : Window
    {
        public CameraProfilerView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
