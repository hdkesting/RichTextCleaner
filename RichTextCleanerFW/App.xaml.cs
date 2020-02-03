using RichTextCleanerFW.Common.Logging;
using System.IO;
using System.Windows;

namespace RichTextCleanerFW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string logdir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Hans_Kesting\\RichTextCleaner");
            Logger.Initialize(new DirectoryInfo(logdir));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Logger.Shutdown();
        }
    }
}
