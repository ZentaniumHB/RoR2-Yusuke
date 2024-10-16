using EntityStates;
using EntityStates.Treebot.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using YusukeMod;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.SkillStates
{
    public class SpiritGunDouble : BaseSkillState
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
        //private bool hasFired;
        //private bool hasBarrageDirection;

        public float charge;
        public bool isMaxCharge;
        private float barrageStopWatch;
        private int numberOfShots = 0;

        private Ray aimRay;

        public bool isPrimary;

        public static GameObject regularSpiritGunPrefab;
        public static GameObject spiritGunPierceProjectile;

        public override void OnEnter()
        {
            base.OnEnter();

            barrageStopWatch = 0f;

            this.duration = FireSpiritShotgun.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.2f * this.duration;  // delay before the shot
            characterDirection.turnSpeed = 2000f;
            base.characterBody.SetAimTimer(2f);

            if(isPrimary == true)
            {
                regularSpiritGunPrefab = YusukeAssets.basicSpiritGunPrefabPrimary;
            }
            else
            {
                regularSpiritGunPrefab = YusukeAssets.basicSpiritGunPrefab;
            }
            
            spiritGunPierceProjectile = YusukeAssets.spiritGunPiercePrefab;
            base.PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
            ResumeVelocity();
            numberOfShots = 0;
        }

        private void Fire()
        {
            if (isMaxCharge)
            {
                
                //base.characterBody.AddSpreadBloom(1.5f);
                //EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryShootPistol", base.gameObject);

                if (base.isAuthority)
                {
                    //base.AddRecoil(-1f * Shoot.recoil, -2f * Shoot.recoil, -0.5f * Shoot.recoil, 0.5f * Shoot.recoil);
                    AimDirection();


                    if (numberOfShots == 0)
                    {
                        DamageType value = (DamageType.BypassBlock);
                        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                        {
                            projectilePrefab = regularSpiritGunPrefab,
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

                    if(numberOfShots == 1)
                    {
                        DamageType value = (DamageType.Generic);
                        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                        {
                            projectilePrefab = regularSpiritGunPrefab,
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
        }
        

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            barrageStopWatch += Time.fixedDeltaTime;   // start count 
            Log.Info($"watch: " + barrageStopWatch);

            if (base.fixedAge >= this.fireTime)
            {
                if(barrageStopWatch > 0.15)
                {
                    this.Fire();
                    barrageStopWatch = 0;
                    numberOfShots++;
                    PauseVelocity();

                }
                
            }

            if (numberOfShots == 2 && this.isAuthority)
            {
                Log.Info($"Returning from barrage");
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void PauseVelocity()
        {

            base.characterBody.SetAimTimer(2f); // facing camera direction duration
            characterMotor.enabled = false;
            //characterDirection.enabled = false;

        }

        private void ResumeVelocity()
        {
            characterMotor.enabled = true;
            characterDirection.enabled = true;
        }

        private void AimDirection()
        {
            aimRay = base.GetAimRay();
            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}