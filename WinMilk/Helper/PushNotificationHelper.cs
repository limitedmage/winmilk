using System;
using Microsoft.Phone.Notification;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace WinMilk.Helper
{
    public class PushNotificationHelper
    {
        public HttpNotificationChannel myChannel;

        public int Count { get; set; }

        public PushNotificationHelper(int count)
        {
            Count = count;
            CreatingANotificationChannel();
        }

        public void CreatingANotificationChannel()
        {
            myChannel = HttpNotificationChannel.Find("WinMilk");

            if (myChannel == null)
            {
                // Only one notification channel name is supported per application.
                myChannel = new HttpNotificationChannel("WinMilk", "winmilk.julianapena.com");

                SetUpDelegates();

                // After myChannel.Open() is called, the notification channel URI will be sent to the application through the ChannelUriUpdated delegate.
                // If your application requires a timeout for setting up a notification channel, start it after the myChannel.Open() call. 
                myChannel.Open();
            }
            else // Found an existing notification channel.
            {
                SetUpDelegates();

                // The URI that the application sends to its web service.
                Debug.WriteLine("Notification channel URI:" + myChannel.ChannelUri.ToString());

                if (myChannel.ChannelUri == null)
                {
                    // The notification channel URI has not been sent to the client. Wait for the ChannelUriUpdated delegate to fire.
                    // If your application requires a timeout for setting up a notification channel, start it here.
                }
            }

            // An application is expected to send its notification channel URI to its corresponding web service each time it launches.
            // The notification channel URI is not guaranteed to be the same as the last time the application ran. 
            if (myChannel.ChannelUri != null)
            {
                // SendURIToService(myChannel.ChannelUri);

                RequestTileUpdate(myChannel.ChannelUri, Count);
            }
        }

        public void RequestTileUpdate(Uri channelUri, int count)
        {
            BindingANotificationsChannelToALiveTileNotification();

            WebClient wc = new WebClient();
            string uri = "http://winmilk.julianapena.com/send_push.php?device_url=" + Uri.EscapeUriString(channelUri.ToString())
                + "&count=" + count;
            wc.DownloadStringCompleted += (s, e) =>
                {
                    if (e.Error != null)
                    {
                        SmartDispatcher.BeginInvoke(() => MessageBox.Show("Error with push"));
                    }
                    else
                    {
                        SmartDispatcher.BeginInvoke(() => MessageBox.Show(e.Result));
                    }
                };
            wc.DownloadStringAsync(new Uri(uri));
        }

        public void SetUpDelegates()
        {
            myChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(myChannel_ChannelUriUpdated);
            myChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(myChannel_HttpNotificationReceived);
            myChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(myChannel_ShellToastNotificationReceived);
            myChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(myChannel_ErrorOccurred);
        }

        void myChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            // The URI that the application will send to its corresponding web service.
            Debug.WriteLine("Notification channel URI:" + e.ChannelUri.ToString());
            // SendURIToService(e.ChannelUri);
            RequestTileUpdate(myChannel.ChannelUri, Count);
        }

        void myChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            switch (e.ErrorType)
            {
                case ChannelErrorType.ChannelOpenFailed:
                    // ...
                    break;
                case ChannelErrorType.MessageBadContent:
                    // ...
                    break;
                case ChannelErrorType.NotificationRateTooHigh:
                    // ...
                    break;
                case ChannelErrorType.PayloadFormatError:
                    // ...
                    break;
                case ChannelErrorType.PowerLevelChanged:
                    // ...
                    break;
            }
        }

        // Receiving a toast notification. 
        // Toast notifications are only delivered to the device when the application is not running in the foreground. 
        // If the application is running in the foreground, the toast notification is instead routed to the application.
        void myChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            if (e.Collection != null)
            {
                Dictionary<string, string> collection = (Dictionary<string, string>)e.Collection;
                System.Text.StringBuilder messageBuilder = new System.Text.StringBuilder();

                foreach (string elementName in collection.Keys)
                {
                    //...
                }
            }
        }

        // Receiving a raw notification. 
        // Raw notifications are only delivered to the application when it is running in the foreground. 
        // If the application is not running in the foreground, the raw notification message 
        // is dropped on the Push Notification Service and is not delivered to the device.
        void myChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            if (e.Notification.Body != null && e.Notification.Headers != null)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(e.Notification.Body);
            }
        }



        // If you do not want to use remote resources for your Tile's background image, use the following code to bind a notification channel to a local resource. 
        // A Tile notification is always delivered to the Tile, regardless of whether the application is running in the foreground.  
        private void BindingANotificationsChannelToATileNotification()
        {
            if (!myChannel.IsShellTileBound)
            {
                myChannel.BindToShellTile();
            }
        }


        // If you want to use remote or local resources for your Tile's background image, 
        // use the following code to bind a notification channel to either a remote resource that is in the approved list, or to a local resource. 
        // A Tile notification is always delivered to the Tile, regardless of whether the application is running in the foreground.  
        private void BindingANotificationsChannelToALiveTileNotification()
        {
            if (myChannel.IsShellTileBound)
                myChannel.UnbindToShellTile();

            // The approved list of URIs that will be verified on every push notification that contains a URI reference.
            Collection<Uri> ListOfAllowedDomains = new Collection<Uri> { new Uri("http://winmilk.julianapena.com") };
            myChannel.BindToShellTile(ListOfAllowedDomains);
        }

        // Binding a notification channel to a toast notification.
        private void BindingANotificationsChannelToAToastNotification()
        {
            if (!myChannel.IsShellToastBound)
            {
                myChannel.BindToShellToast();
            }
        }
    }
}
