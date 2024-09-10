using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YusukeMod.Modules.BaseStates
{
    public class YusukeMain : GenericCharacterMain
    {
        Vector3 currentPosition;
        Vector3 latestGroundPosition;

        private float timer;
        private bool hasIdleBegun;
        private bool hasIdleEnded;
        private int interval = 0;

        // Y-axis ray (for spirit gun mega)
        private float maxDistance;
        private Ray yDistanceRay;
        private RaycastHit hit;


        // Idle stuff for Yusuke
        public override void OnEnter()
        {
            base.OnEnter();
            // setting the y-distance ray for spirit gun mega


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            currentPosition = transform.position;
            if (isGrounded) latestGroundPosition = transform.position;

            //checking for any movement, including skill activations 
            if (CheckIdle() && rigidbody.velocity == Vector3.zero) StartIdleTime();
            if (!CheckIdle() || !rigidbody.velocity.Equals(Vector3.zero)) ResetIdleTime();

            if (hasIdleBegun)
            {
                timer += Time.deltaTime;
                //Chat.AddMessage("Time standing still: " +timer);
            }


            if (timer > 5)
            {

                Chat.AddMessage("*whistling*");

            }

        }

        public override void OnExit()
        {
            base.OnExit();

        }

        private bool CheckIdle()
        {
            if (inputBank.skill1.down || inputBank.skill2.down || inputBank.skill3.down || inputBank.skill4.down || inputBank.interact.down || inputBank.jump.down || inputBank.sprint.down || inputBank.activateEquipment.down)
                return false;
            else
            {
                return true;
            }

        }
        private void StartIdleTime()
        {
            if (!hasIdleBegun)
            {
                hasIdleEnded = false;
                hasIdleBegun = true;
                //Chat.AddMessage("Idle...");
            }
        }

        private void ResetIdleTime()
        {
            if (!hasIdleEnded)
            {
                hasIdleEnded = true;
                hasIdleBegun = false;
                //Chat.AddMessage("Movement...");
            }

            timer = 0f;
            interval = 0;
        }

        public bool CompareYAxis()
        {
            yDistanceRay = new Ray(transform.position, Vector3.down);
            maxDistance = 5000f;
            float value = 0;
            if (Physics.Raycast(yDistanceRay, out hit, maxDistance))
            {
                value = hit.distance;
            }

            //Math.Abs(value);
            //Chat.AddMessage("Y-Axis distance: " + value);

            // 0 in this case means there is no ground underneath Yusuke (skybox?)
            if (value >= 40.0 || value == 0)
            {
                return true;
            }
            else
            {
                return false;
            }



        }

    }
}
