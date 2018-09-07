using System;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using digiCamControl.LightBox.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;

namespace digiCamControl.LightBox.ViewModels
{
    public class StartViewModel : ViewModelBase, IInit
    {


        public Profile Profile
        {
            get { return ServiceProvider.Instance.Profile;; }
            set
            {
                ServiceProvider.Instance.Profile  = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Profile> Profiles { get; set; }


        public RelayCommand NextCommand { get; set; }
        public RelayCommand AddProfileCommand { get; set; }
        public RelayCommand DelProfileCommand { get; set; }

        public StartViewModel()
        {
            NextCommand = new RelayCommand(Next);
            Profiles = new ObservableCollection<Profile>();
            AddProfileCommand = new RelayCommand(AddProfile);
            DelProfileCommand = new RelayCommand(DelProfile);
        }

        private void DelProfile()
        {
            if (Profiles.Count > 1)
            {
                if (MessageBox.Show("Do you want to delete the selected profile" + Profile.Name + " ? ",
                        "Delete profile", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Utils.DeleteFile(Profile.GetFileName());
                    Profiles.Remove(Profile);
                    Profile = Profiles[0];
                }
            }
            else
            {
                MessageBox.Show("Profile can't be deleted !");
            }
        }

        private void AddProfile()
        {
            var newP = GetNewProfile("NewProfile1");
            newP.Save();
            Profiles.Add(newP);
            Profile = newP;
            MessageBox.Show("New profile added");
        }

        private void Next()
        {
            Profile.Save();
            ServiceProvider.Instance.OnMessage(Messages.ChangeLayout, "", ViewEnum.Capture);
        }

        private Profile GetNewProfile(string name)
        {
            var newP = new Profile { Name = name, SessionName = "Session", SessionCounter = 1 };
            var obj = ServiceProvider.Instance.ExportPlugins[0];
            var item = obj.GetDefault();
            item.Name = "Export => " + obj.Name;
            item.Id = obj.Id;
            item.Icon = obj.Icon;
            newP.ExportItems.Add(item);
            return newP;
        }

        public void Init()
        {
            try
            {
                Profiles.Clear();
                var files = Directory.GetFiles(Settings.Instance.ProfileFolder, "*.json");
                foreach (var file in files)
                {
                    var p = Profile.Load(file);
                    if (p != null)
                        Profiles.Add(p);
                }
                if (Profiles.Count == 0)
                {
                    var newP = GetNewProfile("Profile1");
                    newP.Save();
                    Profiles.Add(newP);
                }
                Profile = Profiles[0];
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load profile list ",e);
            }   
        }

        public void UnInit()
        {
            
        }
    }
}
