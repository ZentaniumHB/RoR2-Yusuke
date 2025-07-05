using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    public class DemonGunFollowUp : BaseSkillState
    {
        public GameObject projectilePrefab = YusukeAssets.basicSpiritGunPrefab;

        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"
        public GameObject spiritImpactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion"); //YusukeAssets.spiritGunExplosionEffect;


        private float fireTime;
        private string muzzleString;
        public float charge;

        private float bulletTime = 0.2f;
        public bool isPrimary;
        public int totalBullets = 3;
        public bool hasBarrageEnded;
        private float barrageStopWatch;
        private int numberOfShots;
        public HurtBox target;

        private Ray aimRay;
        private float knockBackTime;
        private float knockBackDuration = 1f;

        public override void OnEnter()
        {
            base.OnEnter();
            // pausing velocity so the character doesn't fall 
            characterMotor.velocity.y = 0f;
            characterMotor.enabled = true;
            

        }

        public override void OnExit()
        {
            base.OnExit();
            // resume velocity
            characterMotor.velocity.y = 0f;
            characterMotor.enabled = true;
            characterDirection.enabled = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            barrageStopWatch += Time.fixedDeltaTime;   // start count 
            //Log.Info($"watch: " + barrageStopWatch);

            if (!target.healthComponent.alive) 
            {
                outer.SetNextStateToMain();    // if the enemy is killed whilst the follow up is happening, then simply exit the state.
                return;
            }
                

            if (fixedAge >= fireTime)
            {
                KnockBackEffect();
                if (barrageStopWatch > bulletTime && numberOfShots != totalBullets)
                {
                    // fires a bullet then increments
                    if (isGrounded) // make sure the last bullet is normal speed, othewise it will look weird
                    {
                        PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpGrounded", "ShootGun.playbackRate", 1);
                    }
                    else
                    {
                        PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpAir", "ShootGun.playbackRate", 1);
                    }

                    Fire();

                    barrageStopWatch = 0;
                    numberOfShots++;
                    Log.Info("total bullets: " + totalBullets);
                    Log.Info("number of shots: " + numberOfShots);

                }
                
            }


            // once it reaches the max shot, it then sets the boolean to true and returns
            if (numberOfShots >= totalBullets) 
            {
                knockBackTime += GetDeltaTime();
                Log.Info("knockbackTimer: " + knockBackTime);
                if (knockBackTime > knockBackDuration) hasBarrageEnded = true;
            }
                
            if (hasBarrageEnded && isAuthority)
            {
                Log.Info($"Returning from demon barrage");
                outer.SetNextStateToMain();
                return;
            }



        }

        private void KnockBackEffect()
        {
            characterDirection.moveVector = Vector3.zero;   // prevents character input movement 
            if (!isGrounded)
            {
                // reverse the direction, so it seems it has a knockback effect.
                Vector3 awayFromDirection = (-aimRay.direction).normalized;
                Vector3 backWardSpeed = awayFromDirection * moveSpeedStat;
                // Apply the velocity to the character's motor
                characterMotor.velocity = backWardSpeed;
            }
        }

        private void Fire()
        {
   
            characterBody.AddSpreadBloom(1.5f);
            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            Util.PlaySound("HenryShootPistol", gameObject);

            aimRay = GetAimRay();

            EffectManager.SpawnEffect(spiritImpactEffect, new EffectData
            {
                origin = target.gameObject.transform.position,
                scale = 8f
            }, transmit: true);
            new BulletAttack
            {
                bulletCount = 1,
                aimVector = target.gameObject.transform.position - transform.position,
                origin = aimRay.origin,
                damage = (charge / 2) * damageCoefficient * damageStat,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
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
                radius = 0.75f,
                sniper = false,
                stopperMask = LayerIndex.CommonMasks.bullet,
                weapon = null,
                tracerEffectPrefab = tracerEffectPrefab,
                spreadPitchScale = 1f,
                spreadYawScale = 1f,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,

            }.Fire();  
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
