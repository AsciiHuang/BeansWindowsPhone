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
using Microsoft.Phone.Controls;

namespace Beans
{
    public partial class ScorePage : PhoneApplicationPage
    {
        public ScorePage()
        {
            InitializeComponent();
        }

        private void OnPhoneApplicationPageLoaded(object sender, RoutedEventArgs e)
        {
            int easyScore = Utility.GetKeyValue(Constants.BEANS_SETTING_EASYMODE_SCORE, 0);
            int hardScore = Utility.GetKeyValue(Constants.BEANS_SETTING_HARDMODE_SCORE, 0);

            HardScore.Text = hardScore.ToString();
            EasyScore.Text = easyScore.ToString();
        }
    }
}