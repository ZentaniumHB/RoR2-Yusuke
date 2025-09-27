using EntityStates;
using EntityStates.Treebot.Weapon;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using YusukeMod;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static YusukeMod.Modules.BaseStates.YusukeMain;

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

        private readonly string fingerTipString = "fingerTipR";
        private GameObject spiritGunMuzzleFlashPrefab;

        private YusukeMain mainState;

        public override void OnEnter()
        {
            base.OnEnter();

            spiritGunMuzzleFlashPrefab = YusukeAssets.spiritGunMuzzleFlashEffect;

            

            // get the stateMachine related to the customName Body
            EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            if (entityStateMachine.state is Roll)
            {
                // means the roll state is currently active on that activation state, change the animations playing accordingly
                if (!isGrounded)
                {
                    PlayAnimation("FullBody, Override", "BufferEmpty", "anim.interruptPlaybackRate", 1f);
                }

            }

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

            SpawnChargeEffect();

        }

        private void SpawnChargeEffect()
        {
            // destroy timer will destroy the object effect after the duration of 2 seconds, the creation of effects had this by default when adding to the effects list, but this allows flexibility 
            EffectComponent component = spiritGunMuzzleFlashPrefab.GetComponent<EffectComponent>();
            spiritGunMuzzleFlashPrefab.AddComponent<DestroyOnTimer>().duration = 2;

            if (component)
            {
                component.parentToReferencedTransform = true;
            }
        }

        private void SwitchAnimationLayer()
        {
            EntityStateMachine stateMachine = characterBody.GetComponent<EntityStateMachine>();
            if (stateMachine == null)
            {
                Log.Error("No State machine found");
            }
            else
            {
                Type currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    mainState = (YusukeMain)stateMachine.state;
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.GunCharge, false);
                    // make the ReleaseAnimation index true
                }

            }

        }

        public override void OnExit()
        {
            base.OnExit();
            ResumeVelocity();
            SwitchAnimationLayer();
            // make the ReleaseAnimation index false here

            numberOfShots = 0;
        }

        private void Fire()
        {
            if (isMaxCharge)
            {

                //base.characterBody.AddSpreadBloom(1.5f);
                //EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                EffectManager.SimpleMuzzleFlash(spiritGunMuzzleFlashPrefab, gameObject, fingerTipString, false);
                PlayAnimation("BothHands, Override", "ShootSpiritGun", "ShootGun.playbackRate", 1.8f);
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