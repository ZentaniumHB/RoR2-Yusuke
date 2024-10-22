using EntityStates;
using IL.RoR2.Achievements.Bandit2;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Survivors.Yusuke.SkillStates;


namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class MazBackToBackStrikes : BaseSkillState
    {
        public static float baseDuration = 1.25f;

        private float duration = 0.5f;
        private HurtBox enemyHurtBox = null;
        public SingleTracking tracking;
        private bool hasTargetBeenFound;
        private bool isEnemyKilled;
        private bool isSelectionMade;

        public override void OnEnter()
        {
            base.OnEnter();
            tracking = gameObject.GetComponent<SingleTracking>();
            isEnemyKilled = false;
            isSelectionMade = false;
        }

        private void TeleportToTarget()
        {
           
            characterMotor.rootMotion += enemyHurtBox.gameObject.transform.position - transform.position;
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(enemyHurtBox == null)
            {
                // get the enemies model that is marked and teleport to them.
                enemyHurtBox = tracking.GetTrackingTarget();
                TeleportToTarget();
            }

            if (!hasTargetBeenFound) 
            {
                // once teleported, find the matching hurtbox
                FindMatchingHurtbox();
            }
            else
            {
                // if the target is found, then attack the grabbed enemy if they are not killed or a followup selection is made.
                if(isEnemyKilled || isSelectionMade) ConsecutiveAttack();
            }

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void ConsecutiveAttack()
        {
            // if the character is in the list or not, do different things. 

        }

        private void FindMatchingHurtbox()
        {
            // grab the enemy collders that are nearby
            Collider[] collisions;
            collisions = Physics.OverlapSphere(transform.position, 2, LayerIndex.entityPrecise.mask);
            List<Collider> colliders = collisions.ToList();

            foreach (Collider collider in colliders)
            {
                // get the colliders hurtbox and compare whether they equal to the marked enemies hurtbox
                HurtBox capturedHurtbox = collider.GetComponent<HurtBox>();
                if (capturedHurtbox == enemyHurtBox)
                {
                    Log.Info("Enemy hurtbox exists. Now grabbing...");
                    GrabEnemy();
                    hasTargetBeenFound = true; break;

                }
            }
        }

        private void GrabEnemy()
        {
            
        }

        public override void OnExit()
        {
            base.OnExit();
            
        }

        

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
