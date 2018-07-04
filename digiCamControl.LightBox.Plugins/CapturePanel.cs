using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins
{
    public class CapturePanel: IPanelItem
    {
        public CapturePanelViewModel ViewModel { get; set; }

        public CapturePanel()
        {
            Panel = new CapturePanelView();
            ViewModel = (CapturePanelViewModel) Panel.DataContext;
            ServiceProvider.Instance.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
        }

        private void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventargs)
        {
            try
            {
                string fileName = Path.Combine(Settings.Instance.TempFolder,
                    Path.GetRandomFileName() + Path.GetExtension(eventargs.FileName));
                Utils.CreateFolder(fileName);
                eventargs.CameraDevice.TransferFile(eventargs.Handle, fileName);
                eventargs.CameraDevice.DeleteObject(new DeviceObject() { Handle = eventargs.Handle });
                eventargs.CameraDevice.ReleaseResurce(eventargs.Handle);
                ServiceProvider.Instance.OnMessage(Messages.ImageCaptured, fileName);
            }
            catch (Exception e)
            {
                Log.Error("Transfer error ", e);
            }
        }

        public string Name => "Capture";
        public string Id => "{D65AD198-1641-491B-872A-3C4EA367456A}";
        public string Icon => "CameraIris";
        public ContentControl Panel { get; }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
