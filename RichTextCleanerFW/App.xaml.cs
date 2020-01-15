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

            string logdir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "RichTextCleaner");
            Logging.Logger.Initialize(new DirectoryInfo(logdir));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Logging.Logger.Shutdown();
        }
    }
}
