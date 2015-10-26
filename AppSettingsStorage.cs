using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project {
    public static class AppSettingsStorage {

        public static GameSettings GetGameSettings() {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            var gameSettings = new GameSettings();

            gameSettings.AccelerometerEnabled = (bool)(localSettings.Values["Setting_AccelerometerEnabled"] ?? true);
            gameSettings.TouchControlsEnabled = (bool)(localSettings.Values["Setting_TouchControlsEnabled"] ?? true);
            gameSettings.DebugEnabled = (bool)(localSettings.Values["Setting_DebugEnabled"] ?? false);

            return gameSettings;
        }

        public static bool SetGameSettings(GameSettings settings) {
            try {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["Setting_AccelerometerEnabled"] = settings.AccelerometerEnabled;
                localSettings.Values["Setting_TouchControlsEnabled"] = settings.TouchControlsEnabled;
                localSettings.Values["Setting_DebugEnabled"] = settings.DebugEnabled;
            }
            catch {
                return false;
            }
            return true;
        }
    }
}
