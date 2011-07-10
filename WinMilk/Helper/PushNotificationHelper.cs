using System;
using Microsoft.Phone.Notification;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using IronCow;

namespace WinMilk.Helper
{
    public class PushNotificationHelper
    {
        public delegate void NotificationCallback(string response);

        public HttpNotificationChannel myChannel;

        public TaskList List { get; set; }

        public PushNotificationHelper(TaskList list)
        {
            List = list;
            EnableNotifications();
        }

        public void EnableNotifications()
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
                RequestTileUpdate(myChannel.ChannelUri);
            }
        }

        public void RequestTileUpdate(Uri channelUri)
        {
            int count = List.Count;
            RequestTileUpdate(channelUri, count, (r) => { });
        }

        public void RequestTileUpdate(Uri channelUri, NotificationCallback response)
        {
            int count = List.Count;
            RequestTileUpdate(channelUri, count, response);
        }

        public void RequestTileUpdate(Uri channelUri, int count, NotificationCallback response)
        {
            BindLiveTile();

            WebClient wc = new WebClient();
            string uri = "http://winmilk.julianapena.com/send_push.php?device_url=" + Uri.EscapeUriString(channelUri.ToString())
                + "&count=" + count;

            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    SmartDispatcher.BeginInvoke(() => response("Error with push"));
                }
                else
                {
                    SmartDispatcher.BeginInvoke(() => response(e.Result));
                }
            };

            wc.DownloadStringAsync(new Uri(uri));
        }

        public void SetUpDelegates()
        {
            myChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(myChannel_ChannelUriUpdated);
            myChannel.ErrorOccurred     += new EventHandler<NotificationChannelErrorEventArgs>(myChannel_ErrorOccurred);
        }

        void myChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            // The URI that the application will send to its corresponding web service.
            Debug.WriteLine("Notification channel URI:" + e.ChannelUri.ToString());
            // SendURIToService(e.ChannelUri);
            RequestTileUpdate(myChannel.ChannelUri);
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

        // If you want to use remote or local resources for your Tile's background image, 
        // use the following code to bind a notification channel to either a remote resource that is in the approved list, or to a local resource. 
        // A Tile notification is always delivered to the Tile, regardless of whether the application is running in the foreground.  
        private void BindLiveTile()
        {
            if (myChannel.IsShellTileBound)
                myChannel.UnbindToShellTile();

            // The approved list of URIs that will be verified on every push notification that contains a URI reference.
            Collection<Uri> ListOfAllowedDomains = new Collection<Uri> { new Uri("http://winmilk.julianapena.com") };
            myChannel.BindToShellTile(ListOfAllowedDomains);
        }

        private void DisableNotifications()
        {
            RequestTileUpdate(myChannel.ChannelUri, 0, (r) =>
            {
                if (myChannel.IsShellTileBound)
                    myChannel.UnbindToShellTile();

                myChannel.Close();
            });
        }
    }
}
