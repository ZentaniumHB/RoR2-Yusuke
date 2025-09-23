using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    public class IgnoreParentRotation : MonoBehaviour
    {

        public Transform referenceTransform;
        private Vector3 lookingRotation;
        private Quaternion originalRotation;

        private void Awake()
        {
            originalRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            transform.rotation = referenceTransform.rotation;

        }

        public void SetLookRotation(Vector3 rotationVector)
        {
            lookingRotation = rotationVector;
        }


    }
}
