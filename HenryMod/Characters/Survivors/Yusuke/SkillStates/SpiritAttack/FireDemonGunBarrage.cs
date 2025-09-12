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

        public static GameObject projectilePrefab = YusukeAssets.demonGunProjectilePrefab;

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
        private float barrageStopWatch = 0.2f;
        private int numberOfShots;

        private YusukeMain mainState;


        private readonly string fingerTipString = "fingerTipR";
        private GameObject demonGunMuzzleFlashPrefab;


        public override void OnEnter()
        {
            base.OnEnter();

            demonGunMuzzleFlashPrefab = YusukeAssets.demonGunMuzzleFlashEffect;

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
                
            }

            SpawnMuzzleEffect();

        }

        private void SpawnMuzzleEffect()
        {
            // destroy timer will destroy the object effect after the duration of 2 seconds, the creation of effects had this by default when adding to the effects list, but this allows flexibility 
            EffectComponent component = demonGunMuzzleFlashPrefab.GetComponent<EffectComponent>();
            demonGunMuzzleFlashPrefab.AddComponent<DestroyOnTimer>().duration = 2;

            if (component)
            {
                // toggling the parent
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

            EffectManager.SimpleMuzzleFlash(demonGunMuzzleFlashPrefab, gameObject, fingerTipString, false);

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
