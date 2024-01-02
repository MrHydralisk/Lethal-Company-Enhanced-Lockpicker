using BepInEx;
using System.Runtime.CompilerServices;
using System;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace EnhancedLockpicker
{
    [BepInPlugin(MOD_GUID, MOD_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private const string MOD_GUID = "MrHydralisk.EnhancedLockpicker";
        private const string MOD_NAME = "Enhanced Lockpicker";

        public static Plugin instance;

        public static ManualLogSource MLogS;
        public static ConfigFile config;

        private void Awake()
        {
            MLogS = BepInEx.Logging.Logger.CreateLogSource(MOD_GUID);
            config = Config;
            EnhancedLockpicker.Config.Load();
            instance = this;
            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(HarmonyPatches).TypeHandle);
            }
            catch (Exception ex)
            {
                MLogS.LogError(string.Concat("Error in static constructor of ", typeof(HarmonyPatches), ": ", ex));
            }
            MLogS.LogInfo($"Plugin is loaded!");
        }
    }
}