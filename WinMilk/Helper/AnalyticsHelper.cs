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
using Microsoft.WebAnalytics;
using System.ComponentModel.Composition;

namespace WinMilk.Helper
{
    public class AnalyticsHelper
    {
        // Injected by MEF   
        [Import("Log")]
        public Action<AnalyticsEvent> Log { get; set; }

        public AnalyticsHelper()
        {
            // Inject
            CompositionInitializer.SatisfyImports(this);
        }

        public void Track(string category, string name)
        {
            // Track analytics event
            SmartDispatcher.BeginInvoke(() =>
            {
                Log(new AnalyticsEvent { Category = category, Name = name, });
            });
        }
    }
}
