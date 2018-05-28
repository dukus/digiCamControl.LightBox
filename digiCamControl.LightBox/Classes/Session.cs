using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digiCamControl.LightBox.Classes
{
    public class Session
    {
        public string Name { get; set; }
        public ObservableCollection<FileItem> Files { get; set; }

        public Session()
        {
            Files = new ObservableCollection<FileItem>();
        }

    }
}
