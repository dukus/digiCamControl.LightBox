using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Core.Interfaces;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class ServiceProvider
    {
        public const string AppName = "digiCamControl.LightBox";
        private static readonly ILog _log = LogManager.GetLogger(AppName);

        public delegate void MessageEventHandler(object sender, MessageArgs message);
        public event MessageEventHandler Message;

        private static ServiceProvider _instance;

        public static ServiceProvider Instance => _instance ?? (_instance = new ServiceProvider());
        public CameraDeviceManager DeviceManager { get; set; }

        public Profile Profile { get; set; }

        public List<IAdjustPlugin> AdjustPlugins { get; set; }
        public List<IExportPlugin> ExportPlugins { get; set; }

        public void Configure()
        {
            var LogFile = Path.Combine(Settings.Instance.DataFolder, "Logs", "app.log");
            Configure(LogFile);
            DeviceManager = new CameraDeviceManager();
            Profile = new Profile();
            AdjustPlugins = new List<IAdjustPlugin>();
            ExportPlugins = new List<IExportPlugin>();
        }

        public static void Configure(string logFile)
        {
            Utils.CreateFolder(logFile);
            Configure(AppName, logFile);
            Log.LogDebug += Log_LogDebug;
            Log.LogError += Log_LogError;
            Log.Debug(
                "--------------------------------===========================Application starting===========================--------------------------------");
            try
            {
                Log.Debug("Application version : " + Assembly.GetEntryAssembly().GetName().Version);
                Directory.CreateDirectory(Settings.Instance.ProfileFolder);
            }
            catch { }
        }

        private static void Log_LogError(LogEventArgs e)
        {
            _log.Error(e.Message, e.Exception);
        }

        private static void Log_LogDebug(LogEventArgs e)
        {
            _log.Debug(e.Message, e.Exception);
        }

        public static void Configure(string appfolder, string logFile)
        {
            bool isConfigured = _log.Logger.Repository.Configured;
            if (!isConfigured)
            {
                // Setup RollingFileAppender
                var fileAppender = new RollingFileAppender
                {
                    Layout =
                        new PatternLayout(
                            "%d [%t]%-5p %c [%x] - %m%n"),
                    MaximumFileSize = "9000KB",
                    MaxSizeRollBackups = 5,
                    RollingStyle = RollingFileAppender.RollingMode.Size,
                    AppendToFile = true,
                    File = logFile,
                    ImmediateFlush = true,
                    LockingModel = new FileAppender.MinimalLock(),
                    Name = "XXXRollingFileAppender"
                };
                fileAppender.ActivateOptions(); // IMPORTANT, creates the file
                BasicConfigurator.Configure(fileAppender);
#if DEBUG
                // Setup TraceAppender
                TraceAppender ta = new TraceAppender();
                ta.Layout = new PatternLayout("%d [%t]%-5p %c [%x] - %m%n");
                BasicConfigurator.Configure(ta);
#endif
            }
        }

        public virtual void OnMessage(string message)
        {
            var handler = Message;
            handler?.Invoke(this, new MessageArgs() { Message = message });
        }

        public virtual void OnMessage(string message, string param)
        {
            var handler = Message;
            handler?.Invoke(this, new MessageArgs() { Message = message, ParamString = param });
        }

        public virtual void OnMessage(string message, string param, object o)
        {
            var handler = Message;
            handler?.Invoke(this, new MessageArgs() { Message = message, ParamString = param, Param = o });
        }

        public virtual void OnMessage(string message, string param, object o, object o2)
        {
            var handler = Message;
            handler?.Invoke(this, new MessageArgs() { Message = message, ParamString = param, Param = o, Param2 = o2 });
        }

        public virtual void OnMessage(string message, string param, object o, object o2, object o3)
        {
            var handler = Message;
            handler?.Invoke(this,
                new MessageArgs() { Message = message, ParamString = param, Param = o, Param2 = o2, Param3 = o3 });
        }
    }
}
