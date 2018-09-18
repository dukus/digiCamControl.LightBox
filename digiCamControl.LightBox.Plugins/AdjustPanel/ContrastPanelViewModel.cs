using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace digiCamControl.LightBox.Plugins.AdjustPanel
{
    public class ContrastPanelViewModel:ViewModelBase, IInit
    {
       
        public FileItem FileItem { get; set; }


        public int Brightness
        {
            get { return FileItem.Variables.GetInt("Brightness"); }
            set
            {
                FileItem.Variables["Brightness"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int Contrast
        {
            get { return FileItem.Variables.GetInt("Contrast"); }
            set
            {
                FileItem.Variables["Contrast"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int Saturation
        {
            get { return FileItem.Variables.GetInt("Saturation"); }
            set
            {
                FileItem.Variables["Saturation"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int Hue
        {
            get { return FileItem.Variables.GetInt("Hue"); }
            set
            {
                FileItem.Variables["Hue"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public bool Normalize
        {
            get { return FileItem.Variables.GetBool("Normalize"); }
            set
            {
                FileItem.Variables["Normalize"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int BlackPoint
        {
            get { return FileItem.Variables.GetInt("BlackPoint"); }
            set
            {
                FileItem.Variables["BlackPoint"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int WhitePoint
        {
            get { return FileItem.Variables.GetInt("WhitePoint", 100); }
            set
            {
                FileItem.Variables["WhitePoint"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public int MidPoint
        {
            get { return FileItem.Variables.GetInt("MidPoint"); }
            set
            {
                FileItem.Variables["MidPoint"] = value;
                RaisePropertyChanged();
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public bool AutoLevel
        {
            get { return FileItem.Variables.GetBool("AutoLevel"); }
            set
            {
                FileItem.Variables["AutoLevel"] = value;
                RaisePropertyChanged();
                RaisePropertyChanged(()=>NotAutoLevel);
                ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
            }
        }

        public bool NotAutoLevel => !AutoLevel;


        public RelayCommand ResetCommand { get; set; }

        public ContrastPanelViewModel()
        {
            FileItem = new FileItem();
            ResetCommand = new RelayCommand(Reset);

        }

        private void Reset()
        {
            Brightness = 0;
            Saturation = 0;
            Hue = 0;
            Contrast = 0;
            Normalize = false;
            AutoLevel = false;
            BlackPoint = 0;
            WhitePoint = 100;
            MidPoint = 0;
            ServiceProvider.Instance.OnMessage(Messages.RefreshThumb);
        }

        public void Init()
        {
            FileItem.Variables.ValueChangedEvent += Variables_ValueChangedEvent;
            ServiceProvider.Instance.Message += Instance_Message;
        }

        private void Instance_Message(object sender, MessageArgs message)
        {
            if (message.Message == Messages.ItemChanged)
            {
                FileItem = (FileItem) message.Param;
                foreach (var name in Utils.GetPropertieNames(this.GetType()))
                {
                    RaisePropertyChanged(name);
                }
            }
        }

        public void UnInit()
        {
            FileItem.Variables.ValueChangedEvent -= Variables_ValueChangedEvent;
            ServiceProvider.Instance.Message -= Instance_Message;
        }

        private void Variables_ValueChangedEvent(object sender, ValueItem item)
        {
            RaisePropertyChanged(item.Name);
        }

    }
}

