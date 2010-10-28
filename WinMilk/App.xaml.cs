using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using IronCow;
using IronCow.Rest;

namespace WinMilk
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        private static readonly string RtmApiKey = "624a977e8a0e4ce69dec0196ce6479dd";
        private static readonly string RtmSharedKey = "ea04c412d8a8ce87";

        public static Rtm RtmClient;
        public static Response ListsResponse;
        public static Response TasksResponse;

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        public static void LoadData()
        {
            string RtmAuthToken = Helper.IsolatedStorageHelper.GetObject<string>("RtmAuthToken");
            //DateTime LastSync = Helper.IsolatedStorageHelper.GetObject<DateTime>("LastSync");
            ListsResponse = Helper.IsolatedStorageHelper.GetObject<Response>("ListsResponse");
            TasksResponse = Helper.IsolatedStorageHelper.GetObject<Response>("TasksResponse");

            if (!string.IsNullOrEmpty(RtmAuthToken))
            {
                RtmClient = new Rtm(RtmApiKey, RtmSharedKey, RtmAuthToken);
                //RtmClient.TaskLists = RtmTasks;
            }
            else
            {
                RtmClient = new Rtm(RtmApiKey, RtmSharedKey);
            }

            RtmClient.CacheListsEvent += OnCacheLists;
            RtmClient.CacheTasksEvent += OnCacheTasks;

            if (ListsResponse != null)
            {
                RtmClient.LoadListsFromResponse(ListsResponse);
            }
            if (TasksResponse != null)
            {
                RtmClient.LoadTasksFromResponse(TasksResponse);
            }
        }

        public static void SaveData()
        {
            Helper.IsolatedStorageHelper.SaveObject<string>("RtmAuthToken", RtmClient.AuthToken);
            //Helper.IsolatedStorageHelper.SaveObject<DateTime>("LastSync", DateTime.Now);
            Helper.IsolatedStorageHelper.SaveObject<Response>("ListsResponse", ListsResponse);
            Helper.IsolatedStorageHelper.SaveObject<Response>("TasksResponse", TasksResponse);
        }

        public static void DeleteData()
        {
            Helper.IsolatedStorageHelper.DeleteObject("RtmAuthToken");
            //Helper.IsolatedStorageHelper.DeleteObject("LastSync");
            Helper.IsolatedStorageHelper.DeleteObject("ListsResponse");
            Helper.IsolatedStorageHelper.DeleteObject("TasksResponse");
            RtmClient = new Rtm(RtmApiKey, RtmSharedKey);
        }

        public static void OnCacheLists(Response response)
        {
            ListsResponse = response;
        }

        public static void OnCacheTasks(Response response)
        {
            TasksResponse = response;
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //DeleteData();
            LoadData();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            LoadData();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
            SaveData();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveData();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is RtmException)
            {
                var ex = (e.ExceptionObject as RtmException).ResponseError;
                RootFrame.Dispatcher.BeginInvoke(() => 
                {
                    MessageBox.Show(ex.Message, "Error " + ex.Code, MessageBoxButton.OK);
                });
                e.Handled = true;
            }
            else
            {

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            Rtm.Dispatcher = App.Current.RootVisual.Dispatcher;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}