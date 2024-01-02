using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace EnhancedLockpicker
{
    public class HarmonyPatches
    {
        private static readonly Type patchType;

        static HarmonyPatches()
        {
            patchType = typeof(HarmonyPatches);
            Harmony val = new Harmony("LethalCompany.MrHydralisk.EnhancedLockpicker");
            if (Config.doorLPEnabled?.Value ?? true)
            {
                val.Patch(AccessTools.Method(typeof(DoorLock), "Awake", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "DL_Awake_Postfix", (Type[])null));
                val.Patch(AccessTools.Method(typeof(DoorLock), "LockDoor", (Type[])null, (Type[])null), transpiler: new HarmonyMethod(patchType, "DL_LockDoor_Transpiler", (Type[])null));
            }
        }

        public static void DL_Awake_Postfix(DoorLock __instance)
        {
            __instance.maxTimeLeft = Config.doorLPTime?.Value ?? 60f;
            __instance.lockPickTimeLeft = __instance.maxTimeLeft;
        }

        public static float ChangeLockDoorTimeToLockPick(float timeToLockPick)
        {
            Debug.LogError($"ChangeLockDoorTimeToLockPick {timeToLockPick}");
            return Config.doorLPTime?.Value ?? timeToLockPick;
        }

        public static IEnumerable<CodeInstruction> DL_LockDoor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            codes.Insert(7, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "ChangeLockDoorTimeToLockPick"))); 
            return codes.AsEnumerable();
        }
    }
}
