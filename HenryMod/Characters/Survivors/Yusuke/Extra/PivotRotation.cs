using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using UnityEngine.PlayerLoop;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    internal class PivotRotation : MonoBehaviour
    {

        private Vector3 lookDirection;

        private Quaternion originalBoneRotation;

        private Quaternion originalPivotRotation;

        private Transform baseBoneTransform;

        private Transform basePivotTransform;

        private bool hasCapturedOrigRotations;

        private bool shouldUpdateVFXRotationsOvertime;

        private bool hasRotatedVFX;

        public bool shouldRotate = false;

        public bool shouldRotatePivotVFX = false;


        private void Start()
        {
            ModelLocator modelLocator = gameObject.GetComponent<ModelLocator>();
            ChildLocator modelChildLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();

            if (modelChildLocator)
            {
               
                /*int childIndex = modelChildLocator.FindChildIndex("base");
                baseBoneTransform = modelChildLocator.FindChild(childIndex);
                childIndex = modelChildLocator.FindChildIndex("basePivot");
                basePivotTransform = modelChildLocator.FindChild(childIndex);*/

                baseBoneTransform = modelChildLocator.FindChild("Base");
                basePivotTransform = modelChildLocator.FindChild("basePivot");

            }

        }

        // late update used for the rotation aspect
        private void LateUpdate()
        {
            // the pivotRotation is using the local rotation as its parented to the model, the base doesn't need to do that since it's being overrided
            if(lookDirection != Vector3.zero && shouldRotate) 
            {
                if (!hasCapturedOrigRotations)
                {
                    // grab the previous rotations first
                    hasCapturedOrigRotations = true;
                    //originalBoneRotation = baseBoneTransform.rotation;
                    originalPivotRotation = basePivotTransform.localRotation;
                }

                Rotate();
                if(!shouldUpdateVFXRotationsOvertime) hasRotatedVFX = true;
            }

            if (!shouldRotate && hasRotatedVFX)
            {
                // resets the rotations back to normal
                hasRotatedVFX = false;
                hasCapturedOrigRotations = false;
                shouldUpdateVFXRotationsOvertime = false;
                //baseBoneTransform.rotation = originalBoneRotation;
                basePivotTransform.localRotation = originalPivotRotation;

            }
        }

        private void Rotate()
        {
            
            if(baseBoneTransform && basePivotTransform)
            {
                //Rotating
                baseBoneTransform.rotation *= Quaternion.AngleAxis((lookDirection.y * -90f), Vector3.right);
                if (!hasRotatedVFX) basePivotTransform.localRotation *= Quaternion.AngleAxis((lookDirection.y * -90f), Vector3.right);
                if (shouldUpdateVFXRotationsOvertime) basePivotTransform.localRotation = Quaternion.Inverse(baseBoneTransform.rotation);
            }
            // transforms have not been set properly otherwise

        }


        // setting the rotations of the character and visual effects if needed.
        public void SetRotations(Vector3 forwardDirection, bool bodyRotation, bool vfxRotation, bool rotateOvertime)
        {
            lookDirection = forwardDirection;
            shouldRotate = bodyRotation;
            shouldRotatePivotVFX = vfxRotation;
            shouldUpdateVFXRotationsOvertime = rotateOvertime;

        }
    }
}
