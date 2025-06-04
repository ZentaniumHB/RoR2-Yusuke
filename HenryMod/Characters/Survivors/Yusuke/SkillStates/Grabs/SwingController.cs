using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.UIElements;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    internal class SwingController : MonoBehaviour
    {
        public Transform pivotTransform;
        public Transform modelTransform;

        private CharacterMotor motor;

        private CharacterDirection direction;
        private Rigidbody rigidbody;
        private RigidbodyMotor rigidMotor;

        private ModelLocator modelLocator;
        public CharacterBody yusukeBody;

        private Collider collider;
        private SphereCollider sphCollider;
        private CapsuleCollider capCollider;

        private Vector3 oldMoveVec;
        private Quaternion originalRotation;

        public bool hasStringEnded;
        public bool hasLanded;
        public bool hasRevertedRotation;
        public float changeInY = 0;
        private bool setBounds;
        private Quaternion finalRotation;

        private void Start()
        {
            Log.Info("Running in awake");

            motor = GetComponent<CharacterMotor>();
            direction = GetComponent<CharacterDirection>();
            rigidMotor = gameObject.GetComponent<RigidbodyMotor>();
            modelLocator = GetComponent<ModelLocator>();
            collider = gameObject.GetComponent<Collider>();
            sphCollider = gameObject.GetComponent<SphereCollider>();
            capCollider = gameObject.GetComponent<CapsuleCollider>();
            rigidbody = gameObject.GetComponent<Rigidbody>();

            if (collider)
            {
                collider.enabled = false;
            }
            if (sphCollider)
            {
                sphCollider.enabled = false;
            }
            if (capCollider)
            {
                capCollider.enabled = false;
            }
            if (rigidMotor)
            {
                oldMoveVec = rigidMotor.moveVector;
            }
            if (direction) direction.enabled = false;
            if (modelLocator)
            {
                modelTransform = modelLocator.modelTransform;
                originalRotation = modelTransform.rotation;
            }
            
            hasRevertedRotation = false;
            setBounds = false;
            Log.Info("Setup complete");
        }
        private void FixedUpdate()
        {
            if (pivotTransform.position != Vector3.zero)
            {
                if (modelTransform)
                {
                    modelTransform.position = pivotTransform.position;

                }

                if (motor)
                {
                    if(direction) 
                    //Log.Info("This character has a motor");
                    motor.disableAirControlUntilCollision = true;
                    motor.velocity = Vector3.zero;
                    motor.rootMotion = Vector3.zero;
                    motor.Motor.SetPosition(pivotTransform.position, true);

                    // Checks for the yusukeBody, if exist it will get the forward direction, this currently doesn't make the enemies completely flat with the x and z axis (need to figure it out)
                    if (yusukeBody) 
                    {
                        // look vector makes sure the character model is always facing yusukes model
                        Vector3 look = yusukeBody.characterDirection.forward;
                        // Quaternion.Look rotation attempts to face the characters forward vector (issue above)
                        motor.Motor.SetRotation(Quaternion.LookRotation(-look) * Quaternion.AngleAxis(-90f, Vector3.right), true);
                    } 
                }

                if (rigidMotor)
                {
                    rigidMotor.moveVector = Vector3.zero;
                    rigidMotor.rootMotion = Vector3.zero;
                    
                }

                if (rigidbody)
                {
                    rigidbody.position = pivotTransform.position;
                    rigidbody.velocity = Vector3.zero;
                }


            }
            else
            {
                Log.Info(" pivot Doesn't exist, destory");
                Destroy(this);

            }
        }

        public void Remove()
        {
            Log.Info("Enabling and destroying");

            if (modelLocator)modelLocator.enabled = true;
            if (modelTransform) modelTransform.rotation = originalRotation;
            if (direction) direction.enabled = true;
            if (collider) collider.enabled = true;
            if (sphCollider) sphCollider.enabled = true;
            if (capCollider) capCollider.enabled = true;
            if (rigidMotor) rigidMotor.moveVector = oldMoveVec;

            if (motor) motor.disableAirControlUntilCollision = false;

            Destroy(this);
        }

        
    }
}