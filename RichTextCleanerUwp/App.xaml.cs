using RichTextCleaner.Common.Logging;
using RichTextCleanerUwp.Forms;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RichTextCleanerUwp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            this.UnhandledException += this.App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.Log(LogLevel.Error, nameof(App), "Unhandled exception: " + e.Message, e.Exception);
            e.Handled = false; // true means that the app doesn't exit
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.EnableLogging();

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // N/A? - saved in Settings" that will be loaded automatically.
                }
                else if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
                {
                    Models.CleanerSettings.Instance.HtmlSource = string.Empty;
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            Logger.Log(LogLevel.Information, nameof(App), $"Started from previous state {args.PreviousExecutionState}.");

            if (!args.PrelaunchActivated)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), args.Arguments);
                }

                float DPI = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

                // Height="450" Width="800" 
                var desiredSize = new Windows.Foundation.Size(800f * 96.0f / DPI, 450f * 96.0f / DPI);
                ApplicationView.PreferredLaunchViewSize = desiredSize;

                // Ensure the current window is active
                Window.Current.Activate();
                ApplicationView.GetForCurrentView().TryResizeView(desiredSize);

                //var main = (MainPage)((Frame)Window.Current.Content).Content;
                //rootFrame.KeyDown += main.Page_KeyDown;
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new InvalidOperationException("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Logger.Log(LogLevel.Debug, nameof(App), "Suspending app");

            Logger.Shutdown();

            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        private void OnResuming(object sender, object e)
        {
            this.EnableLogging();
            Logger.Log(LogLevel.Debug, nameof(App), "Resumed app");
        }

        private void EnableLogging()
        {
            string logdir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Logging");
            Logger.Initialize(new DirectoryInfo(logdir));
#if DEBUG
            Logger.MinLogLevel = LogLevel.Debug;
#else
            Logger.MinLogLevel = LogLevel.Information;
#endif
        }
    }
}
