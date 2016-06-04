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
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnOptionButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/OptionPage.xaml", UriKind.Relative));
        }

        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }

        private void OnTutorialButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TutorialPage.xaml", UriKind.Relative));
        }

        private void OnScoresButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ScorePage.xaml", UriKind.Relative));
        }
    }
}