using EntityStates;
using YusukeMod.Survivors.Yusuke;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using YusukeMod.Survivors.Yusuke.SkillStates;
using YusukeMod.Modules.BaseStates;
using System;
using static YusukeMod.Modules.BaseStates.YusukeMain;
using YusukeMod.Characters.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    public class FireDemonGunBarrage : BaseChargeSpirit
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

        private YusukeMain mainState;


        public override void OnEnter()
        {
            base.OnEnter();

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

            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";

            if (isPrimary == true)
            {
                projectilePrefab = YusukeAssets.basicSpiritGunPrefabPrimary;
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

                    // since one of the sync layers are already active (mazoku layer), it needs to be turned of temporarily so the sync layer can be used
                    MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
                    if (maz.hasTransformed)
                    {
                        mainState.SwitchMovementAnimations((int)AnimationLayerIndex.Mazoku, true);
                    }

                }

            }

        }

        public override void OnExit()
        {
            base.OnExit();
            SwitchAnimationLayer();

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

            PlayAnimation("BothHands, Override", "ShootSpiritGun", "ShootGun.playbackRate", 1.8f);
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
