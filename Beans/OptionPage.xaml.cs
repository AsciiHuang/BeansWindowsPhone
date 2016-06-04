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
    public partial class OptionPage : PhoneApplicationPage
    {
        public OptionPage()
        {
            InitializeComponent();
        }

        private void OnPhoneApplicationPageLoaded(object sender, RoutedEventArgs e)
        {
            int gameMode = Utility.GetKeyValue(Constants.BEANS_SETTING_GAME_MODE, Constants.BEANS_COUNT_HARD);
            Boolean isPlayMusic = Utility.IsPlayBackgoundMusic;

            if (gameMode == Constants.BEANS_COUNT_EASY)
            {
                // 易
                IsEasy.Text = "On";
                switchEasyMode.IsChecked = true;
            }
            else
            {
                // 難
                IsEasy.Text = "Off";
                switchEasyMode.IsChecked = false;
            }

            if (isPlayMusic)
            {
                IsPlayMusic.Text = "On";
            }
            else
            {
                IsPlayMusic.Text = "Off";
            }

            switchPlayMusic.IsChecked = isPlayMusic;
        }

        private void OnGameModeSwitchChecked(object sender, RoutedEventArgs e)
        {
            Utility.SetKeyValue(Constants.BEANS_SETTING_GAME_MODE, Constants.BEANS_COUNT_EASY);
            IsEasy.Text = "On";
        }

        private void OnGameModeSwitchUnchecked(object sender, RoutedEventArgs e)
        {
            Utility.SetKeyValue(Constants.BEANS_SETTING_GAME_MODE, Constants.BEANS_COUNT_HARD);
            IsEasy.Text = "Off";
        }

        private void OnMusicSwitchChecked(object sender, RoutedEventArgs e)
        {
            Boolean isPlayMusic = Utility.IsPlayBackgoundMusic;
            if (isPlayMusic)
            {
                return;
            }

            if (Microsoft.Xna.Framework.Media.MediaPlayer.State == Microsoft.Xna.Framework.Media.MediaState.Playing)
            {
                if (MessageBox.Show("The audio output is being used by another application. Do you want to stop it?", "BOXING", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    Microsoft.Xna.Framework.Media.MediaPlayer.Stop();
                    Utility.SetOpenBackgroundMusic();
                    ((MediaElement)App.Current.Resources["mediaElement"]).Source = new Uri("Sound/bgm.mp3", UriKind.Relative);
                }
                else
                {
                    // 重設為 uncheck
                    IsPlayMusic.Text = "Off";
                    switchPlayMusic.IsChecked = false;
                }
            }
            else
            {
                Utility.SetOpenBackgroundMusic();
                IsPlayMusic.Text = "On";
                ((MediaElement)App.Current.Resources["mediaElement"]).Source = new Uri("Sound/bgm.mp3", UriKind.Relative);
            }
        }

        private void OnMusicSwitchUnchecked(object sender, RoutedEventArgs e)
        {
            ((MediaElement)App.Current.Resources["mediaElement"]).Stop();
            Utility.SetCloseBackgroundMusic();
            IsPlayMusic.Text = "Off";
        }
    }
}