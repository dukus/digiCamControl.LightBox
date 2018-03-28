using System.Windows.Controls;
using GalaSoft.MvvmLight;

namespace digiCamControl.LightBox.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ContentControl _contentControl;

        public ContentControl ContentControl
        {
            get { return _contentControl; }
            set
            {
                _contentControl = value;
                RaisePropertyChanged(() => ContentControl);
            }
        }


    }
}
