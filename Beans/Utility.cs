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
using System.IO.IsolatedStorage;

namespace Beans
{
    public class Utility
    {
        public static Boolean IsPlayBackgoundMusic
        {
            get
            {
                Boolean bOK = true;
                if (IsolatedStorageSettings.ApplicationSettings.Contains(Constants.BEANS_SETTING_PLAY_BACKGROUNDMUSIC))
                {
                    bOK = (Boolean)IsolatedStorageSettings.ApplicationSettings[Constants.BEANS_SETTING_PLAY_BACKGROUNDMUSIC];
                }
                else
                {
                    bOK = true;
                }
                return bOK;
            }
        }

        public static void SetCloseBackgroundMusic()
        {
            IsolatedStorageSettings.ApplicationSettings[Constants.BEANS_SETTING_PLAY_BACKGROUNDMUSIC] = false;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static void SetOpenBackgroundMusic()
        {
            IsolatedStorageSettings.ApplicationSettings[Constants.BEANS_SETTING_PLAY_BACKGROUNDMUSIC] = true;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static void SetKeyValue(String key, int value)
        {
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static int GetKeyValue(String key, int defValue)
        {
            int nRes = 0;

            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                nRes = (int)IsolatedStorageSettings.ApplicationSettings[key];
            }
            else
            {
                SetKeyValue(key, defValue);
                nRes = defValue;
            }

            return nRes;
        }
    }
}
