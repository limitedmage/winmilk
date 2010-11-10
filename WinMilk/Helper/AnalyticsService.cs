using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel.Composition.Hosting;

namespace WinMilk.Helper
{
    public class AnalyticsService : IApplicationService
    {

        public void StartService(ApplicationServiceContext context)
        {
            CompositionHost.Initialize(
                new AssemblyCatalog(Application.Current.GetType().Assembly),
                new AssemblyCatalog(typeof(Microsoft.WebAnalytics.AnalyticsEvent).Assembly),
                new AssemblyCatalog(typeof(Microsoft.WebAnalytics.Behaviors.TrackAction).Assembly)
            );
        }

        public void StopService()
        {
            
        }
    }
}
