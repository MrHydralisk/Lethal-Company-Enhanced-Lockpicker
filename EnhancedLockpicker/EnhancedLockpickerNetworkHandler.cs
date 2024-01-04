using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace EnhancedLockpicker
{
    public class EnhancedLockpickerNetworkHandler : NetworkBehaviour
    {
        public static EnhancedLockpickerNetworkHandler instance;

        void Awake()
        {
            instance = this;
        }

        public void PlaceLockPickerRpc(EnhancedLockpickerComp eLockpicker, DoorLock doorScript, bool lockPicker1)
        {
            if (IsHost || IsServer)
            {
                PlaceLockPickerClientRpc(eLockpicker.NetworkObject, doorScript.NetworkObject, lockPicker1);
            }
            else
            {
                PlaceLockPickerServerRpc(eLockpicker.NetworkObject, doorScript.NetworkObject, lockPicker1);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaceLockPickerServerRpc(NetworkObjectReference eLockpickerObject, NetworkObjectReference doorObject, bool lockPicker1)
        {
            PlaceLockPickerClientRpc(eLockpickerObject, doorObject, lockPicker1);
        }

        [ClientRpc]
        public void PlaceLockPickerClientRpc(NetworkObjectReference eLockpickerObject, NetworkObjectReference doorObject, bool lockPicker1)
        {
            if (doorObject.TryGet(out var networkObject1) && eLockpickerObject.TryGet(out var networkObject2))
            {
                DoorLock doorLock = networkObject1.gameObject.GetComponentInChildren<DoorLock>();
                EnhancedLockpickerComp eLockpicker = networkObject2.gameObject.GetComponent<EnhancedLockpickerComp>();
                eLockpicker.PlaceLockPicker(doorLock, lockPicker1);
            }
        }

        public void FinishPickingRpc(EnhancedLockpickerComp eLockpicker)
        {
            if (IsHost || IsServer)
            {
                FinishPickingClientRpc(eLockpicker.NetworkObject);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void FinishPickingServerRpc(NetworkObjectReference eLockpickerObject)
        {
            FinishPickingClientRpc(eLockpickerObject);
        }

        [ClientRpc]
        public void FinishPickingClientRpc(NetworkObjectReference eLockpickerObject)
        {
            if (eLockpickerObject.TryGet(out var networkObject2))
            {
                EnhancedLockpickerComp eLockpicker = networkObject2.gameObject.GetComponent<EnhancedLockpickerComp>();
                eLockpicker.FinishPickingLock();
            }
        }
    }
}
