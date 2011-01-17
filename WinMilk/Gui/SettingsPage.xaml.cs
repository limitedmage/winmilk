using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Windows.Data;

namespace WinMilk.Gui
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            // Defining the binding in code and not in XAML fixes an issue where the current selection
            // doesn't show when first navigating to the page.
            AppSettings settings = new AppSettings();
            Binding binding = new Binding("StartPageSetting");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = settings;
            StartSetting.SetBinding(ListPicker.SelectedIndexProperty, binding);
        }

        #region AboutPageEvents

        private void Author_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com";
            page.Show();
        }

        private void Site_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://julianapena.com/projects/winmilk";
            page.Show();
        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://winmilk.codeplex.com/license";
            page.Show();
        }

        private void Codeplex_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://winmilk.codeplex.com/";
            page.Show();
        }

        private void RTM_Click(object senter, RoutedEventArgs e)
        {
            WebBrowserTask page = new WebBrowserTask();
            page.URL = "http://rememberthemilk.com";
            page.Show();
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "winmilk@julianapena.com";
            emailComposeTask.Subject = "WinMilk bug or suggestion";
            emailComposeTask.Show();
        }

        private void Review_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask r = new MarketplaceReviewTask();
            r.Show();
        }

        #endregion 

        /// <summary>
        ///     Will select the pivot item that was specified in the Uri.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            const string page = "Page";
            IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            if (parameters.ContainsKey(page))
            {
                string pageValue = parameters[page];
                if (string.Equals(pageValue, "About", StringComparison.OrdinalIgnoreCase))
                {
                    // lets navigate to the about page since it was selected in the URI
                    HelpPivot.SelectedItem = About;
                }
                else if (string.Equals(pageValue, "Settings", StringComparison.OrdinalIgnoreCase))
                {
                    // lets navigate to the about page since it was selected in the URI
                    HelpPivot.SelectedItem = Settings;
                }
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (StartSetting.ListPickerMode == ListPickerMode.Expanded)
            {
                StartSetting.ListPickerMode = ListPickerMode.Normal;
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }
    }

    /// <summary>
    ///  Class used to store settings for the application.  Settings are stored in isolated storage.
    ///  Derived from MSDN sample "How to: Create a Settings Page for Windows Phone"
    /// </summary>
    public class AppSettings
    {
        IsolatedStorageSettings isolatedStore;

        const string StartPageSettingKeyName = "StartPageSetting";

        const int StartPageSettingDefault = 0;

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettings()
        {
            try
            {
                // Get the settings for this application.
                isolatedStore = IsolatedStorageSettings.ApplicationSettings;

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while using IsolatedStorageSettings: " + e.ToString());
            }
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            try
            {
                // if new value is different, set the new value.
                if (isolatedStore[Key] != value)
                {
                    isolatedStore[Key] = value;
                    valueChanged = true;
                }
            }
            catch (KeyNotFoundException)
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }
            catch (ArgumentException)
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while using IsolatedStorageSettings: " + e.ToString());
            }

            return valueChanged;
        }


        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            valueType value;

            try
            {
                value = (valueType)isolatedStore[Key];
            }
            catch (KeyNotFoundException)
            {
                value = defaultValue;
            }
            catch (ArgumentException)
            {
                value = defaultValue;
            }

            return value;
        }


        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            isolatedStore.Save();
        }

        /// <summary>
        /// Property to get and set a ListBox Setting Key.
        /// </summary>
        public int StartPageSetting
        {
            get
            {
                return GetValueOrDefault<int>(StartPageSettingKeyName, StartPageSettingDefault);
            }
            set
            {
                AddOrUpdateValue(StartPageSettingKeyName, value);
                Save();
            }
        }
    }

}
