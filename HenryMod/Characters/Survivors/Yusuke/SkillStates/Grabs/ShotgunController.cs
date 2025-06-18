using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using EntityStates.BrotherMonster;
using MonoMod.RuntimeDetour;
using RoR2;
using UnityEngine;
using static UnityEngine.UIElements.ListViewDragger;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class ShotgunController : MonoBehaviour
    {

        public Transform pivotTransform;
        public Vector3 centerOfCollider;
        public Transform modelTransform;
        public CharacterBody yusukeBody;
        private Vector3 look;

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

            if (pivotTransform.position != Vector3.zero)
            {
                if (!setBounds)
                {
                    setBounds = true;
                    EnemyRotation(modelTransform);
                }

                if (motor)
                {
                    motor.disableAirControlUntilCollision = true;
                    motor.velocity = Vector3.zero;
                    motor.rootMotion = Vector3.zero;
                    motor.Motor.SetPosition(pivotTransform.position, true);
                    
                }

                if (rigidMotor)
                {
                    //Log.Info("motor exists");
                    rigidMotor.moveVector = Vector3.zero;
                    rigidMotor.rootMotion = Vector3.zero;

                }

                if (rigidbody)
                {
                    rigidbody.position = pivotTransform.position;
                    rigidbody.velocity = Vector3.zero;
                }

                if (modelTransform)
                {
                    modelTransform.position = pivotTransform.position;
                }

            }
            

        }


        public void EnemyRotation(Transform model)
        {

            if (modelTransform)
            {
                /* Look is the direction yusukes model is facing, so the inverse of look will make the model face yusuke instead of facing the same direction
                 * Look rotation needs to be done first before rotating the character on the X axis, this will also need to be placed in fixedUpdate as yusukes body keeps updating
                 * when moving.
                 */
                if (yusukeBody)
                {
                    look = yusukeBody.characterDirection.forward;
                }

                if (rigidbody && motor) model.rotation = Quaternion.LookRotation(-look) * Quaternion.Euler(-70f, 0f, 0f);


            }
            else
            {
                Log.Info("No model.");
            }
        }

        public void Remove()
        {
            Log.Info("[Shotgun Controller] enabling and destroying");
            if (modelLocator) modelLocator.enabled = true;
            if (rigidbody && motor) modelTransform.rotation = originalRotation;
            if (direction) direction.enabled = true;
            if (collider) collider.enabled = true;
            if (sphCollider) sphCollider.enabled = true;
            if (capCollider) capCollider.enabled = true;
            if (rigidMotor) rigidMotor.moveVector = oldMoveVec;

            Destroy(this);
        }


    }
}