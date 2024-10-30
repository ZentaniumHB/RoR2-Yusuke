using EntityStates;
using EntityStates.Jellyfish;
using IL.RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class SwingCombo : BaseSkillState
    {
        public static float baseDuration = 1.25f;
        private float dashSpeed;

        private float maxInitialSpeed = 6f;
        private float finalSpeed = 1f;

        private float dashTime; 
        private float duration = 1f;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private float dashFOV = EntityStates.Commando.DodgeState.dodgeFOV;


        private bool targetFound;

        public override void OnEnter()
        {
            base.OnEnter();

            forwardDirection = GetAimRay().direction;
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * dashSpeed;
            }
            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Dash();
            //if (!targetFound) SearchForBody();

            if (dashTime >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }

        }

        private void SearchForBody()
        {
            
        }

        private void Dash()
        {
            // still need to figure out how to create a better dash instead of relying on the template
            UpdateDashSpeed(maxInitialSpeed, finalSpeed);
            forwardDirection = GetAimRay().direction;
            characterDirection.moveVector = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dashFOV, 60f, fixedAge / duration);

            dashTime += Time.fixedDeltaTime;
            Vector3 normalized = (transform.position - previousPosition).normalized;
            if (characterMotor && characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * dashSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                vector = forwardDirection * d;
                characterMotor.velocity = vector;
            }

            
            previousPosition = transform.position;


        }

        private void UpdateDashSpeed(float max, float final)
        {
            dashSpeed = (moveSpeedStat * 1.2f) * Mathf.Lerp(max, final, fixedAge / duration);
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();
            
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
