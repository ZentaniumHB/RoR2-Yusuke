using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.Components;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using static YusukeMod.Survivors.Yusuke.Components.YusukeWeaponComponent;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    internal class KnockedState : BaseSkillState
    {

        private bool hasAppliedForce;
        Vector3 vector;
        private float duration = 1f;
        private bool hasLanded;
        private float onGroundDuration;
        private float exponent;

        private float spinMinSpeed = 0.1f;
        private float spinMaxSpeed = 60f;
        HealthComponent yusukeHealth;

        public byte NearDeathType;
        private bool hasPlayedAnimation;
        YusukeWeaponComponent yusukeWeapon;
        private Transform baseBoneTransform;

        public override void OnEnter()
        {
            base.OnEnter();

            Log.Info("Entered knockback state ");
            yusukeWeapon = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            vector = Vector3.up * 10f;
            if(characterMotor)
            {
                vector += characterMotor.velocity;

            }
            PlayAnimation("FullBody, Override", "KnockedSpinOut", "ThrowBomb.playbackRate", duration);
            yusukeHealth = characterBody.GetComponent<HealthComponent>();

            if (NetworkServer.active)
            {
                //characterBody.AddBuff(RoR2Content.Buffs.Immune);
                if (yusukeHealth) 
                { 
                    yusukeHealth.health = 1;
                    yusukeHealth.godMode = true;
                }
                    
            }


            ModelLocator modelLocator = gameObject.GetComponent<ModelLocator>();
            ChildLocator modelChildLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();

            if (modelChildLocator)
            {
                baseBoneTransform = modelChildLocator.FindChild("Base");

            }


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //if(!hasLanded) SpinOut();

            if (yusukeHealth) yusukeHealth.health = 1;

            if (isGrounded)
            {
                hasLanded = true;
                
            }

            if (hasLanded)
            {
                if (!hasPlayedAnimation)
                {
                    if (characterMotor)
                    {
                        characterMotor.enabled = false;

                    }
                    hasPlayedAnimation = true;
                    switch (NearDeathType)
                    {
                        case (byte)NearDeathIndex.Mazoku:
                            PlayAnimation("FullBody, Override", "KnockedLandedBack", "ThrowBomb.playbackRate", duration);
                            break;
                        case (byte)NearDeathIndex.Sacred:
                            PlayAnimation("FullBody, Override", "KnockedLandedFront", "ThrowBomb.playbackRate", duration);
                            break;

                    }
                }
                onGroundDuration += GetDeltaTime();
                
            }

            if (isAuthority && fixedAge >= duration)
            {
                if (onGroundDuration > 2f) 
                {
                    switch (NearDeathType)
                    {
                        case (byte)NearDeathIndex.Mazoku:
                            outer.SetNextState(new MazokuResurrect());
                            break;
                        case (byte)NearDeathIndex.Sacred:
                            outer.SetNextState(new SacredEnergyRelease());
                            break;
                    }

                }
                    
            }
        }

        private void SpinOut()
        {

            exponent = Mathf.Lerp(spinMaxSpeed, spinMinSpeed, exponent * GetDeltaTime());

            // lerp will slowly increase to the max value in exponent time
            float currentValue = Mathf.Lerp(spinMinSpeed, spinMaxSpeed, 2);

            Quaternion finalRotation = Quaternion.AngleAxis(currentValue, Vector3.right);
            characterDirection.forward = finalRotation * characterDirection.forward;

            
        }

        public override void OnExit()
        {
            if (yusukeHealth) yusukeHealth.health = yusukeHealth.fullHealth;

            if (characterMotor)
            {
                characterMotor.enabled = true;

            }

            if (NetworkServer.active)
            {
                if (yusukeHealth)
                {
                    yusukeHealth.godMode = false;
                }

            }

            if(yusukeWeapon) yusukeWeapon.SetKnockedBoolean(false);


            PlayAnimation("FullBody, Override", "BufferEmpty", "Roll.playbackRate", 1f);
            Log.Info("Mazoku revive (after): " + yusukeWeapon.GetMazokuRevive());
            Log.Info("Sacred revive (after): " + yusukeWeapon.GetSacredEnergyRevive());
            Log.Info("KnockBack state (after): " + yusukeWeapon.GetKnockedState());

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
