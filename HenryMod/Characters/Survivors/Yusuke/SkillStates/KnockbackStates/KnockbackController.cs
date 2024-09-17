using EntityStates.ClayBoss;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates
{
    public class KnockbackController : MonoBehaviour
    {

        public Transform pivotTransform;

        private CharacterBody body;
        private CharacterMotor motor;
        private CharacterDirection direction;
        private Rigidbody rigidbody;
        private RigidbodyMotor rigidMotor;
        private ModelLocator modelLocator;
        private Transform modelTransform;
        private Quaternion originalRotation;

        public Vector3 knockbackDirection;
        public float knockbackSpeed;

        private float knockbackDuration = 1f;
        private float knockbackStopwatch;
        private bool grabPosition;
        private Vector3 previousPosition;



        public int moveID; // used to check what move they were hit by (it will alter their rotations and speed)
        public bool setDirection;

        private void Awake()
        {
            body = this.GetComponent<CharacterBody>();
            motor = this.GetComponent<CharacterMotor>();
            direction = this.GetComponent<CharacterDirection>();
            rigidMotor = gameObject.GetComponent<RigidbodyMotor>();
            modelLocator = this.GetComponent<ModelLocator>();
            rigidbody = gameObject.GetComponent<Rigidbody>();


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


                }
            }

            /*if (this.rigidMotor)
            {


            }*/

        }

        private void FixedUpdate()
        {
            if (!grabPosition)
            {
                grabPosition = true;

                if (motor)
                {
                    motor.disableAirControlUntilCollision = true;
                    previousPosition = transform.position;
                }

                // prevent any disturbence when altering knockback
                if (rigidMotor)
                {
                    rigidMotor.moveVector = Vector3.zero;
                    rigidMotor.rootMotion = Vector3.zero;
                    if (rigidbody)
                    {
                        rigidbody.position = transform.position;
                        rigidbody.velocity = Vector3.zero;
                    }
                }
                if(body)
                {
                    //if(body.isFlying)
                        
                }
            }

            // timer for duration
            knockbackStopwatch += Time.fixedDeltaTime;

            if (direction)
            {
                direction.forward = new Vector3(-knockbackDirection.x, knockbackDirection.y, -knockbackDirection.z); // reverse the direction on x and z so they are facing yusuke (just for visual satisfaction)
            }

            Vector3 currentPosition = transform.position;
            Vector3 knockbackVelocity = knockbackDirection * knockbackSpeed;

            if (motor && direction)
            {

                if (body.isBoss || body.isChampion)
                {
                    motor.velocity = knockbackVelocity / 3f;
                }
                else
                {
                    motor.velocity = knockbackVelocity / 2f;
                }

            }

            previousPosition = currentPosition;


            if (body && moveID == 1)
            {
                // visually rotate the model to look as if they are actually flying off their feet
                //body.transform.rotation = Quaternion.Euler(-45f, modelTransform.localRotation.eulerAngles.y, 0f);

            }

            if (knockbackStopwatch > knockbackDuration)
            {
                Recover();
            }
        }

        private void Recover()
        {
            // reset everything back to "normal"
            if (modelLocator) modelLocator.enabled = true;
            if (modelTransform) modelTransform.rotation = originalRotation;
            if (direction) direction.enabled = true;
            if (motor) motor.disableAirControlUntilCollision = false;

            Destroy(this); 
        }


    }
}
