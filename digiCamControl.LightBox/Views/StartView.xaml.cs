using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using digiCamControl.LightBox.ViewModels;

namespace digiCamControl.LightBox.Views
{
    /// <summary>
    /// Interaction logic for StartView.xaml
    /// </summary>
    public partial class StartView : UserControl
    {
        public StartView()
        {
            InitializeComponent();
        }

        private void DialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter is string)
            {
                ((StartViewModel) DataContext).Profile.Name = (string) eventArgs.Parameter;
                ((StartViewModel) DataContext).Profile.Save();
            }
        }
    }
}
