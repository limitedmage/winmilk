// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WinMilk.Gui.Controls.ProgressBar
{
    public partial class ProgressBarWithText : UserControl
    {
        public ProgressBarWithText()
        {
            InitializeComponent();

            stackPanel.DataContext = this;
        }

        #region ShowProgress

        /// <summary>
        /// ShowProgress Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShowProgressProperty =
            DependencyProperty.Register("ShowProgress", typeof(bool), typeof(ProgressBarWithText),
                new PropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the ShowProgress property. This dependency property 
        /// indicates whether to show the progress bar.
        /// </summary>
        public bool ShowProgress
        {
            get { return (bool)GetValue(ShowProgressProperty); }
            set { SetValue(ShowProgressProperty, value); }
        }

        #endregion

        #region Text

        /// <summary>
        /// Text Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ProgressBarWithText),
                new PropertyMetadata((string)""));

        /// <summary>
        /// Gets or sets the Text property. This dependency property 
        /// indicates what is the text that appears above the progress bar.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion



    }
}