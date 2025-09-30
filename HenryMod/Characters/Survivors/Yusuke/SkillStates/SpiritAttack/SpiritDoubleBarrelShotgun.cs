using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static YusukeMod.Modules.BaseStates.YusukeMain;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    internal class SpiritDoubleBarrelShotgun : BaseSkillState
    {
        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;

        public static GameObject spiritTracerEffect = YusukeAssets.spiritShotGunTracerEffect;
        public GameObject spiritShotGunExplosionHitEffect = YusukeAssets.spiritShotGunHitEffect;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        public float charge;

        public List<HurtBox> targets;
        private int damageDivision;

        private float barrageStopWatch;
        private int numberOfShots = 0;

        private YusukeMain mainState;

        private readonly string muzzleCenter = "muzzleCenter";
        private GameObject spiritGunMuzzleFlashPrefab;

        public override void OnEnter()
        {
            base.OnEnter();

            spiritGunMuzzleFlashPrefab = YusukeAssets.spiritGunMuzzleFlashEffect;
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

            

            barrageStopWatch = 0f;
            SpawnMuzzleEffect();
            SpawnTracerAndExplosionEffect();

        }

        private void SpawnTracerAndExplosionEffect()
        {
            // destroy timer will destroy the object effect after the duration of 2 seconds, the creation of effects had this by default when adding to the effects list, but this allows flexibility 
            //EffectComponent component = spiritShotGunExplosionHitEffect.GetComponent<EffectComponent>();
            spiritShotGunExplosionHitEffect.AddComponent<DestroyOnTimer>().duration = 2;

        }

        private void SpawnMuzzleEffect()
        {
            // destroy timer will destroy the object effect after the duration of 2 seconds, the creation of effects had this by default when adding to the effects list, but this allows flexibility 
            EffectComponent component = spiritGunMuzzleFlashPrefab.GetComponent<EffectComponent>();
            spiritGunMuzzleFlashPrefab.AddComponent<DestroyOnTimer>().duration = 2;

            if (component)
            {
                // toggling the parent
                component.parentToReferencedTransform = true;

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            SwitchAnimationLayer();
        }

        // switching the animation layer within unity. This will perform the spirit gun animations that is synced to the body animations instead. 
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
                    // goes through the animation layers and switches them within the main state.
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.ShotgunCharge, false);

                }

            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            barrageStopWatch += Time.fixedDeltaTime;   // start count 
            if (base.fixedAge >= this.fireTime)
            {
                if (barrageStopWatch > 0.15)
                {
                    Log.Info("FIRING");
                    this.Fire();
                    barrageStopWatch = 0;
                    numberOfShots++;

                }

            }

            if (numberOfShots == 2 && this.isAuthority)
            {
                Log.Info($"Returning from DB shotgun barrage");
                this.outer.SetNextStateToMain();
                return;
            }
        }



        private void Fire()
        {
            characterBody.AddSpreadBloom(1.5f);
            EffectManager.SimpleMuzzleFlash(spiritGunMuzzleFlashPrefab, gameObject, muzzleCenter, false);
            PlayAnimation("BothHands, Override", "ShootSpiritShotgun", "ShootGun.playbackRate", 1f);

            Util.PlaySound("HenryShootPistol", gameObject);

            Ray aimRay = GetAimRay();

            damageDivision = targets.Count;

            foreach (HurtBox enemy in targets)
            {
                Vector3 aimVector = (enemy.gameObject.transform.position - transform.position).normalized;

                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimVector,
                    origin = aimRay.origin,
                    damage = (damageCoefficient / damageDivision) * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.SlowOnHit,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = range,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = RollCrit(),
                    owner = gameObject,
                    muzzleName = muzzleCenter,
                    smartCollision = true,
                    procChainMask = default,
                    procCoefficient = procCoefficient,
                    radius = 1f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = spiritTracerEffect,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = spiritShotGunExplosionHitEffect,

                }.Fire();


            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
