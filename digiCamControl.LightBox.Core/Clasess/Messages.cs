namespace digiCamControl.LightBox.Core.Clasess
{
    public class Messages
    {
        public const string ChangeLayout = "ChangeLayout";
        public const string SetBusy = "SetBusy";
        public const string ClearBusy = "ClearBusy";
        public const string Message = "Message";
        public const string StatusMessage = "StatusMessage";
        /// <summary>
        /// Raised when new photo was transfered, the StringParam contain the path to file
        /// </summary>
        public const string ImageCaptured = "ImageCaptured";
        public const string StopLiveView = "StopLiveView";
        public const string StartLiveView = "StartLiveView";
        public const string CreatePreview = "CreatePreview";
        public const string RefreshThumb = "RefreshThumb";
        public const string ItemChanged = "ItemChanged";
        /// <summary>
        /// Will pause displaying the live view without closing it
        /// </summary>
        public const string PauseLiveView = "PauseLiveView";
        /// <summary>
        /// Will resume displaying the live view, but the live view should be already started 
        /// </summary>
        public const string ResumeLiveView = "ResumeLiveView";

    }
}
