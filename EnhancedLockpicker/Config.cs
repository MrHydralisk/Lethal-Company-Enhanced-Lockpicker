using BepInEx.Configuration;

namespace EnhancedLockpicker
{
    public class Config
    {
        public static ConfigEntry<bool> doorLPEnabled;
        public static ConfigEntry<float> doorLPTime;

        public static void Load()
        {
            doorLPEnabled = Plugin.config.Bind<bool>("Door", "DoorLPEnabled", true, "Will Lockpicker have custom time for unlocking door?\n[All Client]");
            doorLPTime = Plugin.config.Bind<float>("Door", "DoorLPTime", 10f, "How long will it take for Lockpicker to open door.\nVanilla value 30. Suggested values between 1-60.");        
        }
    }
}
