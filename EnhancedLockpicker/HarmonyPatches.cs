using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace EnhancedLockpicker
{
    public class HarmonyPatches
    {
        private static readonly Type patchType;

        private static FieldInfo RayHit = typeof(LockPicker).GetField("hit", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo DoorTrigger = typeof(DoorLock).GetField("doorTrigger", BindingFlags.Public | BindingFlags.Instance);

        static HarmonyPatches()
        {
            patchType = typeof(HarmonyPatches);
            Harmony val = new Harmony("LethalCompany.MrHydralisk.EnhancedLockpicker");
            if (Config.doorLPEnabled?.Value ?? true)
            {
                val.Patch(AccessTools.Method(typeof(DoorLock), "Awake", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "DL_Awake_Postfix", (Type[])null));
                val.Patch(AccessTools.Method(typeof(DoorLock), "LockDoor", (Type[])null, (Type[])null), transpiler: new HarmonyMethod(patchType, "DL_LockDoor_Transpiler", (Type[])null));
            }
            val.Patch(AccessTools.Method(typeof(GameNetworkManager), "Start", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "GNM_Start_Postfix", (Type[])null));
            val.Patch(AccessTools.Method(typeof(StartOfRound), "Start", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "SOR_Start_Postfix", (Type[])null));
            if (Config.doorLockLPEnabled?.Value ?? true)
            {
                val.Patch(AccessTools.Method(typeof(LockPicker), "Start", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "LP_Start_Postfix", (Type[])null));
                val.Patch(AccessTools.Method(typeof(LockPicker), "ItemActivate", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "LP_ItemActivate_Postfix", (Type[])null));
                val.Patch(AccessTools.Method(typeof(LockPicker), "RetractClaws", (Type[])null, (Type[])null), prefix: new HarmonyMethod(patchType, "LP_RetractClaws_Prefix", (Type[])null));
                val.Patch(AccessTools.Method(typeof(DoorLock), "Update", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "DL_Update_Postfix", (Type[])null));
            }
        }

        public static void DL_Awake_Postfix(DoorLock __instance)
        {
            __instance.maxTimeLeft = Config.doorLPTime?.Value ?? 60f;
            __instance.lockPickTimeLeft = __instance.maxTimeLeft;
        }

        public static float ChangeLockDoorTimeToLockPick(float timeToLockPick)
        {
            return Config.doorLPTime?.Value ?? timeToLockPick;
        }

        public static IEnumerable<CodeInstruction> DL_LockDoor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            codes.Insert(codes.FindIndex((cd) => cd.ToString().Contains("timeToHold")), new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "ChangeLockDoorTimeToLockPick"))); 
            return codes.AsEnumerable();
        }

        public static void GNM_Start_Postfix(GameNetworkManager __instance)
        {
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(Plugin.instance.enhancedLockpickerNetworkManager);
        }

        public static void SOR_Start_Postfix(StartOfRound __instance)
        {
            if (__instance.IsHost)
            {
                GameObject ELPNMObject = GameObject.Instantiate(Plugin.instance.enhancedLockpickerNetworkManager);
                ELPNMObject.GetComponent<NetworkObject>().Spawn(true);
            }
        }

        public static void LP_Start_Postfix(LockPicker __instance)
        {
            EnhancedLockpickerComp ELPNH = __instance.gameObject.GetComponent<EnhancedLockpickerComp>();
            if (ELPNH == null)
            {
                ELPNH = __instance.gameObject.AddComponent<EnhancedLockpickerComp>();
                ELPNH.Start();
            }
        }

        public static void LP_ItemActivate_Postfix(LockPicker __instance, bool used, bool buttonDown = true)
        {
            RaycastHit raycastHit = (RaycastHit)RayHit.GetValue(__instance);
            EnhancedLockpickerComp ELPNH = __instance.gameObject.GetComponent<EnhancedLockpickerComp>();

            if ((__instance.playerHeldBy == null) || raycastHit.Equals(default(RaycastHit)) || (raycastHit.transform.parent == null)) 
            { 
                return; 
            }
            DoorLock component = raycastHit.transform.GetComponent<DoorLock>();
            if (component != null && !component.isLocked && (component.GetComponent<NavMeshObstacle>()?.enabled ?? false))
            {
                bool placeOnLockPicker1 = true;
                Vector3 placePos;
                if (Vector3.Distance(component.lockPickerPosition.position, __instance.playerHeldBy.transform.position) < Vector3.Distance(component.lockPickerPosition2.position, __instance.playerHeldBy.transform.position))
                {
                    placeOnLockPicker1 = true;
                    placePos = component.lockPickerPosition.localPosition;
                }
                else
                {
                    placeOnLockPicker1 = false;
                    placePos = component.lockPickerPosition2.localPosition;
                }
                __instance.playerHeldBy.DiscardHeldObject(placeObject: true, component.NetworkObject, placePos);
                EnhancedLockpickerNetworkHandler.instance.PlaceLockPickerRpc(ELPNH, component, placeOnLockPicker1);
            }
        }

        public static void LP_RetractClaws_Prefix(LockPicker __instance)
        {
            EnhancedLockpickerComp ELPNH = __instance.gameObject.GetComponent<EnhancedLockpickerComp>();
            if (ELPNH.isLocking)
            {
                ELPNH.isLocking = false;
                InteractTrigger doorTrigger = __instance.currentlyPickingDoor.gameObject.GetComponent<InteractTrigger>();
                doorTrigger.interactable = true;
            }
        }

        public static bool GetDoorOpened(DoorLock doorScript)
        {
            return (bool)typeof(DoorLock).GetField("isDoorOpened", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(doorScript);
        }

        public static void DL_Update_Postfix(DoorLock __instance)
        {
            if (!__instance.isLocked && __instance.isPickingLock)
            {
                bool isDoorOpened = GetDoorOpened(__instance);
                if (isDoorOpened)
                {
                    __instance.lockPickTimeLeft = -1;
                }
                InteractTrigger doorTrigger = (InteractTrigger)DoorTrigger.GetValue(__instance);
                __instance.lockPickTimeLeft -= Time.deltaTime;
                doorTrigger.disabledHoverTip = $"Jamming lock: {(int)__instance.lockPickTimeLeft} sec.";
                if (__instance.lockPickTimeLeft < 0f)
                {
                    if(!isDoorOpened)
                    {
                        EnhancedLockpickerNetworkHandler.instance.LockDoorRpc(__instance);
                    }
                    else
                    {
                        doorTrigger.interactable = true;
                        __instance.UnlockDoorServerRpc();
                    }
                }
            }
        }
    }
}
