using BepInEx;
using System.Runtime.CompilerServices;
using System;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
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
        public GameObject enhancedLockpickerNetworkManager;

        private void Awake()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

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
            LoadBundle();
            MLogS.LogInfo($"Plugin is loaded!");
        }

        private void LoadBundle()
        {
            AssetBundle bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("EnhancedLockpicker.Assets.enhancedlockpicker"));
            enhancedLockpickerNetworkManager = bundle.LoadAsset<GameObject>("Assets/Mods/EnhancedLockpicker/EnhancedLockpickerNetworkManager.prefab");
            enhancedLockpickerNetworkManager.AddComponent<EnhancedLockpickerNetworkHandler>();
        }
    }
}