using System.Windows;

namespace digiCamControl.LightBox.Views
{
    /// <summary>
    /// Interaction logic for Fullscreen.xaml
    /// </summary>
    public partial class Fullscreen : Window
    {
        public Fullscreen()
        {
            InitializeComponent();
        }

        private void Fullscreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            ZoomAndPanControl.AnimatedScaleToFit();
        }
    }
}
