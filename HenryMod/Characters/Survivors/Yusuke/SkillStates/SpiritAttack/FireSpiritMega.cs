using EntityStates;
using EntityStates.Treebot.Weapon;
using RoR2;
using RoR2.Projectile;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.SkillStates
{
    public class FireSpiritMega : BaseState
    {
        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 2f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); // "Prefabs/Effects/Tracers/TracerGoldGat"

        private float duration;
        private float fireTime;
        private bool hasFired;
        public string muzzleString;
        public float charge;

        public bool isMaxCharge;
        public static GameObject prefab = YusukeAssets.spiritGunMegaPrefab;

        public bool tier1Wave;
        public bool tier2Wave;


        public override void OnEnter()
        {
            base.OnEnter();

            base.characterBody.SetAimTimer(1f);
            PauseVelocity();
            this.duration = 0.6f;
            this.fireTime = 0.5f;  

            base.PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);

            if(tier1Wave)
            {
                Wave wave = new Wave
                {
                    amplitude = 0.5f,
                    frequency = 30f,
                    cycleOffset = 0f
                };
                RoR2.ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 1.5f, 20f, true);
            }

            if (tier2Wave)
            {
                Log.Info("Tier2wave is true");
                Wave wave = new Wave
                {
                    amplitude = 0.8f,
                    frequency = 31f,
                    cycleOffset = 0f
                };
                RoR2.ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 1.5f, 20f, true);
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            

        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                //base.characterBody.AddSpreadBloom(1.5f);
                //EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryShootPistol", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    base.AddRecoil(-1f * FireSpiritShotgun.recoil, -2f * FireSpiritShotgun.recoil, -0.5f * FireSpiritShotgun.recoil, 0.5f * FireSpiritShotgun.recoil);
                    

                    DamageType value = (DamageType.AOE);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = prefab,
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
        }
        

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            Log.Info("fixedAge:" +fixedAge);
            if (base.fixedAge >= this.fireTime)
            {
                ResumeVelocity();
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;

            }

        }

        private void PauseVelocity()
        {
            characterMotor.enabled = false;
        }

        private void ResumeVelocity()
        {
            characterMotor.enabled = true;
        }



        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}