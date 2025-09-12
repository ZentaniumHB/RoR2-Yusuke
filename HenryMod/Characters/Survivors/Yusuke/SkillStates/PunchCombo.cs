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

        private GameObject meleeSwingEffect1Prefab = YusukeAssets.meleeSwingEffect1;
        private GameObject meleeSwingEffect2Prefab = YusukeAssets.meleeSwingEffect2;
        private GameObject meleeSwingEffect3Prefab = YusukeAssets.meleeSwingEffect3;
        private GameObject meleeSwingEffect4Prefab = YusukeAssets.meleeSwingEffect4;

        private GameObject hitImpactEffectPrefab = YusukeAssets.hitImpactEffect;

        private readonly string dashCenter = "dashCenter";

        public override void OnEnter()
        {
            cuffComponent = gameObject.GetComponent<SpiritCuffComponent>();


            hitboxGroupName = "MeleeGroup";

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
            muzzleString = "mainPosition";
            playbackRateParam = "Slash.playbackRate";


            swingEffectPrefab = YusukeAssets.swordSwingEffect;

            meleeSwingEffect1Prefab = YusukeAssets.meleeSwingEffect1;
            meleeSwingEffect2Prefab = YusukeAssets.meleeSwingEffect2;
            meleeSwingEffect3Prefab = YusukeAssets.meleeSwingEffect3;
            meleeSwingEffect4Prefab = YusukeAssets.meleeSwingEffect4;

            hitEffectPrefab = YusukeAssets.swordHitImpactEffect;

            impactSound = YusukeAssets.swordHitSoundEvent.index;

            EditEffects();

            base.OnEnter();
        }

        private void EditEffects()
        {
            meleeSwingEffect1Prefab.AddComponent<DestroyOnTimer>().duration = 1;
            meleeSwingEffect2Prefab.AddComponent<DestroyOnTimer>().duration = 1;
            meleeSwingEffect3Prefab.AddComponent<DestroyOnTimer>().duration = 1;
            meleeSwingEffect4Prefab.AddComponent<DestroyOnTimer>().duration = 1;
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
            //EffectManager.SimpleMuzzleFlash(swingEffectPrefab, gameObject, muzzleString, false);
            switch (swingIndex)
            {
                case 1:
                    EffectManager.SimpleMuzzleFlash(meleeSwingEffect1Prefab, gameObject, muzzleString, false);
                    break;
                case 2:
                    EffectManager.SimpleMuzzleFlash(meleeSwingEffect2Prefab, gameObject, muzzleString, false);
                    break;
                case 3:
                    if(characterBody.isSprinting && isGrounded)
                    {
                        EffectManager.SimpleMuzzleFlash(meleeSwingEffect1Prefab, gameObject, muzzleString, false);
                    }
                    else
                    {
                        EffectManager.SimpleMuzzleFlash(meleeSwingEffect3Prefab, gameObject, muzzleString, false);
                    }
                    break;
                case 4:
                    if (characterBody.isSprinting && isGrounded)
                    {
                        EffectManager.SimpleMuzzleFlash(meleeSwingEffect2Prefab, gameObject, muzzleString, false);
                    }
                    else
                    {
                        EffectManager.SimpleMuzzleFlash(meleeSwingEffect4Prefab, gameObject, muzzleString, false);
                    }
                    break;
            }
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            EffectManager.SimpleMuzzleFlash(hitImpactEffectPrefab, gameObject, dashCenter, false);
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