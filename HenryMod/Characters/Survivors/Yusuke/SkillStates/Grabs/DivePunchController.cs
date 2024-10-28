using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using UnityEngine;
using static UnityEngine.UIElements.ListViewDragger;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class DivePunchController : MonoBehaviour
    {

        public Transform pivotTransform;
        public Vector3 centerOfCollider;
        public Transform modelTransform;

        private CharacterBody body;
        private CharacterMotor motor;
        private CharacterDirection direction;
        private Rigidbody rigidbody;
        private RigidbodyMotor rigidMotor;
        private ModelLocator modelLocator;
        
        private Quaternion originalRotation;
        private Collider collider;
        private SphereCollider sphCollider;
        private CapsuleCollider capCollider;
        private Vector3 centerOfMass;
        private BaseState state;

        private bool setBounds;
        public bool Pinnable;

        private Vector3 oldMoveVec;
        private Quaternion oldModelRotation;

        public bool hasStringEnded;
        public bool hasLanded;
        public bool hasRevertedRotation;
        public float changeInY = 0;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            motor = GetComponent<CharacterMotor>();
            direction = GetComponent<CharacterDirection>();
            rigidMotor = gameObject.GetComponent<RigidbodyMotor>();
            modelLocator = GetComponent<ModelLocator>();
            collider = gameObject.GetComponent<Collider>();
            sphCollider = gameObject.GetComponent<SphereCollider>();
            capCollider = gameObject.GetComponent<CapsuleCollider>();
            rigidbody = gameObject.GetComponent<Rigidbody>();

            state = gameObject.GetComponent<BaseState>();

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
                if (modelLocator.modelTransform)
                {
                    modelTransform = modelLocator.modelTransform;
                    originalRotation = modelTransform.rotation;

                    if (modelLocator.gameObject.name == "GreaterWispBody(Clone)")
                    {
                        modelLocator.dontDetatchFromParent = true;
                        modelLocator.dontReleaseModelOnDeath = true;
                    }

                    modelLocator.enabled = false;
                }
            }

            hasRevertedRotation = false;
            setBounds = false;
            Log.Info("Setup complete");

        }

        private void FixedUpdate()
        {

            if(pivotTransform.position != Vector3.zero)
            {
                if (!setBounds && Pinnable)
                {
                    //Log.Info("Setting bounds");
                    setBounds = true;
                    EnemyRotation(modelTransform, true);

                }


                //Log.Info("Checking motor");
                if (motor)
                {
                    //Log.Info("motor exists");
                    motor.disableAirControlUntilCollision = true;
                    motor.velocity = Vector3.zero;
                    motor.rootMotion = Vector3.zero;
                    motor.Motor.SetPosition(pivotTransform.position, true);

                }

                //Log.Info("checking rigidMotor");
                if (rigidMotor)
                {
                    //Log.Info("motor exists");
                    rigidMotor.moveVector = Vector3.zero;
                    rigidMotor.rootMotion = Vector3.zero;
                    if (rigidbody)
                    {
                        rigidbody.position = pivotTransform.position;
                        rigidbody.velocity = Vector3.zero;
                    }
                }

                if (hasStringEnded)
                {

                    if(hasRevertedRotation) Remove();
                }

                if (hasLanded)
                {
                    pivotTransform = gameObject.transform;
                }

                if (modelTransform)
                {
                    modelTransform.position = pivotTransform.position;


                }


            }
            else
            {
                Log.Info(" pivot Doesn't exist, destory");
                Destroy(this);

            }
            


        }


        public void EnemyRotation(Transform model, bool pinned)
        {
            if (modelTransform)
            {
                Log.Info("Rotating character");
                // ----------Rotating the character
                oldModelRotation = model.localRotation; 

                Vector3 forwardDirection = model.forward;
                Quaternion worldRotation = model.rotation;

                // convert the world rotation to local rotation (relative to the current forward direction)
                Quaternion localRotation = Quaternion.Inverse(Quaternion.LookRotation(forwardDirection)) * worldRotation;

                Log.Info("Pinned = " + pinned + " Model local rotation (befire): " + model.localRotation);

                Quaternion faceTheSky = Quaternion.identity;
                if(pinned) faceTheSky = Quaternion.Euler(-90f, 0f, 0f); // rotating so the enemy faces the sky
                if(!pinned) faceTheSky = Quaternion.Euler(90f, 0f, 0f); // rotating so the enemy faces the sky

                Quaternion newLocalRotation = localRotation * faceTheSky;
                //from local to world space
                Quaternion finalWorldRotation = Quaternion.LookRotation(forwardDirection) * newLocalRotation;

                model.localRotation = finalWorldRotation;

                Log.Info("Pinned = " +pinned+ " Model local rotation (after): " + model.localRotation);

                if(pinned)
                {

                    float pivotX = pivotTransform.position.x;
                    float colliderY = centerOfCollider.x;

                    if (pivotX > colliderY)
                    {
                        changeInY = pivotX - colliderY;
                    }

                    if (colliderY > pivotX)
                    {
                        changeInY = colliderY - pivotX;
                    }

                    

                }

                if (!pinned) 
                { 

                    hasRevertedRotation = true;
                    
                }
                    

            }
            else
            {
                Log.Info("No model.");
            }
        }

        public void Remove()
        {
            Log.Info("[Dive punch] enabling and destroying");
            if (modelLocator) modelLocator.enabled = true;
            if (modelTransform) modelTransform.rotation = originalRotation;
            if (direction) direction.enabled = true;
            if (collider) collider.enabled = true;
            if (sphCollider) sphCollider.enabled = true;
            if (capCollider) capCollider.enabled = true;
            if (rigidMotor) rigidMotor.moveVector = oldMoveVec;


            
            Destroy(this);
        }


    }
}
