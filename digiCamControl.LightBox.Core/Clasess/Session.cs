using System.Collections.ObjectModel;
using CameraControl.Devices.Classes;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class Session
    {
        public string Name { get; set; }
        public AsyncObservableCollection<FileItem> Files { get; set; }

        public Session()
        {
            Files = new AsyncObservableCollection<FileItem>();
        }

    }
}
