using BepInEx.Configuration;

namespace EnhancedLockpicker
{
    public class Config
    {
        public static ConfigEntry<bool> doorLPEnabled;
        public static ConfigEntry<float> doorLPTime;
        public static ConfigEntry<bool> doorLockLPEnabled;

        public static void Load()
        {
            doorLPEnabled = Plugin.config.Bind<bool>("Door", "DoorLPEnabled", true, "Will Lockpicker have custom time for unlocking door?\n[All Client]");
            doorLPTime = Plugin.config.Bind<float>("Door", "DoorLPTime", 10f, "How long will it take for Lockpicker to open door.\nVanilla value 30. Suggested values between 1-60.");
            doorLockLPEnabled = Plugin.config.Bind<bool>("Door", "DoorLockLPEnabled", true, "Will Lockpicker being able to jam a door lock (locking door)?\n[All Client]");
        }
    }
}
