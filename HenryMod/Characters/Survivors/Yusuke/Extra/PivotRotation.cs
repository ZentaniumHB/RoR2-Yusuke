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

        private bool endUpdateOvertime;

        private bool hasRotated;

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

            if(lookDirection != Vector3.zero && shouldRotate) 
            {
                if (!hasCapturedOrigRotations)
                {
                    // grab the previous rotations first
                    hasCapturedOrigRotations = true;
                    originalBoneRotation = baseBoneTransform.rotation;
                    originalPivotRotation = basePivotTransform.rotation;
                }

                Rotate();
                hasRotated = true;
            }

            if (!shouldRotate && hasRotated)
            {
                // resets the rotations back to normal
                hasRotated = false;
                baseBoneTransform.rotation = originalBoneRotation;
                basePivotTransform.rotation = originalPivotRotation;

            }
        }

        private void Rotate()
        {
            
            if(baseBoneTransform && basePivotTransform)
            {
                //Rotating
                baseBoneTransform.rotation *= Quaternion.AngleAxis((lookDirection.y * -90f), Vector3.right);
                if (shouldRotatePivotVFX) basePivotTransform.rotation *= Quaternion.AngleAxis((lookDirection.y * -90f), Vector3.right);
            }
            // transforms have not been set properly otherwise

        }


        // setting the rotations of the character and visual effects if needed.
        public void SetRotations(Vector3 forwardDirection, bool bodyRotation, bool vfxRotation)
        {
            lookDirection = forwardDirection;
            shouldRotate = bodyRotation;
            shouldRotatePivotVFX = vfxRotation;

        }
    }
}
