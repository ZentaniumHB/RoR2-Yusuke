using YusukeMod.Modules.BaseStates;
using RoR2;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using System;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{
    public class PunchCombo : BaseMeleeAttack
    {

        private SpiritCuffComponent cuffComponent;

        public override void OnEnter()
        {
            cuffComponent = gameObject.GetComponent<SpiritCuffComponent>();


            hitboxGroupName = "SwordGroup";

            damageType = DamageType.Generic;
            damageCoefficient = YusukeStaticValues.swordDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.2f;
            attackEndPercentTime = 0.4f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.6f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 4f;

            swingSoundString = "HenrySwordSwing";
            hitSoundString = "";
            muzzleString = swingIndex % 2 == 0 ? "SwingLeft" : "SwingRight";
            playbackRateParam = "Slash.playbackRate";
            swingEffectPrefab = YusukeAssets.swordSwingEffect;
            hitEffectPrefab = YusukeAssets.swordHitImpactEffect;

            impactSound = YusukeAssets.swordHitSoundEvent.index;

            base.OnEnter();
        }

        protected override void PlayAttackAnimation()
        {
            SwitchStep();
            return;
            //PlayCrossfade("Gesture, Override", "Slash" + (1 + swingIndex), playbackRateParam, duration, 0.1f * duration);
        }

        private void SwitchStep()
        {
            // switch casing for the attacks that are played
            switch (swingIndex)
            {
                case 0:
                    Log.Info("Attack 1");

                    swingIndex = 1;
                    isMainString = true;

                    break;
                case 1:
                    Log.Info("Attack 2");

                    swingIndex = 2;
                    isMainString = true;
                    
                    break;
                case 2:
                    Log.Info("Attack 3");

                    swingIndex = 3;
                    isMainString = true;


                    break;
                case 3:
                    Log.Info("Attack 4");

                    swingIndex = 4;
                    isMainString = true;

                    break;
            }

            isAttacking = true;
            if (isMainString) 
            {
                if (isGrounded)
                {
                    if (!animator.GetBool("isMoving")) 
                    {
                        PlayCrossfade("FullBody, Override", "FullBodyAttack" + swingIndex, playbackRateParam, duration, 0.1f * duration);
                    }

                    // if sprinting, it will do the sprint version instead. 
                    if (characterBody.isSprinting)
                    {
                        PlayCrossfade("Gesture, Override", "SprintAttack" + swingIndex, playbackRateParam, duration, 0.1f * duration);
                    }
                    else 
                    {
                        PlayCrossfade("Gesture, Override", "Attack" + swingIndex, playbackRateParam, duration, 0.1f * duration);
                    }

                }
                else
                {                    
                    PlayCrossfade("Gesture, Override", "Attack" + swingIndex, playbackRateParam, duration, 0.1f * duration);
   
                }
            }

        }

        

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            if (cuffComponent)
            {
                cuffComponent.IncreaseCuff(1);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}