using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Survivors.Yusuke;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using Random = UnityEngine.Random;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    internal class FireSpiritBeam : BaseSkillState
    {

        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"

        private float duration;
        private float knockBackTime = 1f;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        public float charge;

        public bool isPrimary;
        private Ray aimRay;

        private int damageTypeDecider;
        private BulletAttack beamBullet;


        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            muzzleString = "Muzzle";

            damageTypeDecider = Random.Range(1, 3);     // makes it pick either 1 or 2
            Log.Info("type move: " + damageTypeDecider);

            if (isGrounded)
            {
                PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpGrounded", "ShootGun.playbackRate", duration);
            }
            else
            {
                PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpAir", "ShootGun.playbackRate", duration);
            }
            //PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);

        }

        public override void OnExit()
        {
            base.OnExit();
            characterDirection.enabled = true;
            
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime)
            {
                Fire();
            }

            if (hasFired)
            {
                characterDirection.moveVector = Vector3.zero;   // prevents character movement
                knockBackTime += GetDeltaTime();
                if (!isGrounded)
                {
                    //characterBody.gameObject.transform.rotation = Quaternion.LookRotation(GetAimRay().direction);
                    // reverse the direction, so it seems it has a knockback effect.
                    Vector3 awayFromDirection = (-aimRay.direction).normalized;
                    Vector3 backWardSpeed = awayFromDirection * moveSpeedStat;
                    // Apply the velocity to the character's motor
                    characterMotor.velocity = backWardSpeed;
                }
                
            }

            if (fixedAge >= duration && isAuthority)
            {
                if (knockBackTime > 1) 
                {
                    outer.SetNextStateToMain();
                    return;
                }
                
            }
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isAuthority)
                {
                    aimRay = GetAimRay();
                    //instantly look towards the direction
                    characterDirection.forward = aimRay.direction;
                    characterDirection.moveVector = aimRay.direction;
                    AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
                    //characterMotor.velocity = -aimRay.direction * 18f;  // pushback
                    

                    beamBullet = new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damageCoefficient * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = range,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = RollCrit(),
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default,
                        procCoefficient = procCoefficient,
                        radius = 2f,
                        sniper = true,
                        stopperMask = LayerIndex.world.mask,
                        weapon = null,
                        tracerEffectPrefab = tracerEffectPrefab,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    };

                    // randomiser determins the damage type
                    if(damageTypeDecider == 1)
                    {
                        beamBullet.damageType = DamageType.Generic;
                    }
                    else
                    {
                        beamBullet.damageType = DamageType.Shock5s;
                    }

                    gameObject.AddComponent<SkillTags>();
                    beamBullet.Fire();
                    
                }

            }



        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
