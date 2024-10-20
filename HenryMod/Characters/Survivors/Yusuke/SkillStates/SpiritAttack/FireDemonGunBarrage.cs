﻿using EntityStates;
using YusukeMod.Survivors.Yusuke;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    public class FireDemonGunBarrage : BaseSkillState
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

        private float duration;
        private float fireTime;
        private string muzzleString;
        public float charge;

        public bool isPrimary;
        public int totalBullets;
        public bool hasBarrageEnded;
        private float barrageStopWatch;
        private int numberOfShots;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";

            if (isPrimary == true)
            {
                projectilePrefab = YusukeAssets.basicSpiritGunPrefabPrimary;
            }

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            barrageStopWatch += Time.fixedDeltaTime;   // start count 
            //Log.Info($"watch: " + barrageStopWatch);

            if (fixedAge >= fireTime)
            {
                if (barrageStopWatch > 0.2f)
                {
                    if(numberOfShots != totalBullets) Fire();
                    barrageStopWatch = 0;
                    numberOfShots++;

                }

            }

            if(numberOfShots == totalBullets) hasBarrageEnded = true;

            if (hasBarrageEnded && isAuthority)
            {
                Log.Info($"Returning from demon barrage");
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {

            characterBody.AddSpreadBloom(1.5f);
            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            Util.PlaySound("HenryShootPistol", gameObject);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                AddRecoil(-0.1f * recoil, -0.2f * recoil, -0.05f * recoil, 0.05f * recoil);

                DamageType value = (DamageType.Generic);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = base.gameObject,
                    damage = damageStat * damageCoefficient,
                    damageTypeOverride = value,
                    force = force,
                    crit = Util.CheckRoll(critStat, base.characterBody.master),

                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);

            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
