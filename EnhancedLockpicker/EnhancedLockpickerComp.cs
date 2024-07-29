using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace EnhancedLockpicker
{
    public class EnhancedLockpickerComp : NetworkBehaviour
    {
        private LockPicker LP;

        public bool isLocking;
        private AudioSource lockPickerAudio;
        private Coroutine setRotationCoroutine;

        public void Start()
        {
            LP = gameObject.GetComponent<LockPicker>();
            lockPickerAudio = LP.gameObject.GetComponent<AudioSource>();
        }

        public void PlaceLockPicker(DoorLock doorScript, bool lockPicker1)
        {
            if (!LP.isOnDoor)
            {
                base.gameObject.GetComponent<AudioSource>().PlayOneShot(LP.placeLockPickerClips[Random.Range(0, LP.placeLockPickerClips.Length)]);
                LP.armsAnimator.SetBool("mounted", value: true);
                LP.armsAnimator.SetBool("picking", value: true);
                lockPickerAudio.Play();
                lockPickerAudio.pitch = Random.Range(0.94f, 1.06f);
                LP.isOnDoor = true;
                isLocking = true;
                doorScript.isPickingLock = true;
                InteractTrigger doorTrigger = doorScript.gameObject.GetComponent<InteractTrigger>();
                doorTrigger.interactable = false;
                LP.currentlyPickingDoor = doorScript;
                if (setRotationCoroutine != null)
                {
                    StopCoroutine(setRotationCoroutine);
                }
                setRotationCoroutine = StartCoroutine(setRotationOnDoor(doorScript, lockPicker1));
            }
        }

        private IEnumerator setRotationOnDoor(DoorLock doorScript, bool lockPicker1)
        {
            float startTime = Time.timeSinceLevelLoad;
            yield return new WaitUntil(() => !LP.isHeld || Time.timeSinceLevelLoad - startTime > 10f);
            if (lockPicker1)
            {
                LP.transform.localEulerAngles = doorScript.lockPickerPosition.localEulerAngles;
            }
            else
            {
                LP.transform.localEulerAngles = doorScript.lockPickerPosition2.localEulerAngles;
            }
            setRotationCoroutine = null;
        }

        public void Update()
        {
            if ((isLocking && LP.currentlyPickingDoor != null && LP.currentlyPickingDoor.isLocked) || (LP.currentlyPickingDoor != null && HarmonyPatches.GetDoorOpened(LP.currentlyPickingDoor)))
            {
                EnhancedLockpickerNetworkHandler.instance.FinishPickingRpc(this);
            }
        }

        public void FinishPickingLock()
        {
            if (isLocking)
            {
                RetractClaws();
                LP.currentlyPickingDoor = null;
                Vector3 position = base.transform.position;
                base.transform.SetParent(null);
                LP.startFallingPosition = position;
                LP.FallToGround();
                lockPickerAudio.PlayOneShot(LP.finishPickingLockClips[Random.Range(0, LP.finishPickingLockClips.Length)]);
            }
        }

        private void RetractClaws()
        {
            LP.isOnDoor = false;
            isLocking = false;
            LP.armsAnimator.SetBool("mounted", value: false);
            LP.armsAnimator.SetBool("picking", value: false);
            if (LP.currentlyPickingDoor != null)
            {
                LP.currentlyPickingDoor.isPickingLock = false;
                LP.currentlyPickingDoor.lockPickTimeLeft = LP.currentlyPickingDoor.maxTimeLeft;
                LP.currentlyPickingDoor = null;
            }
            lockPickerAudio.Stop();
        }
    }
}
