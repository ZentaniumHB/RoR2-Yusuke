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

        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"
        public GameObject spiritImpactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion"); //YusukeAssets.spiritGunExplosionEffect;

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

            barrageStopWatch = 0f;

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
            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);

            PlayAnimation("BothHands, Override", "ShootSpiritShotgun", "ShootGun.playbackRate", 1f);


            Util.PlaySound("HenryShootPistol", gameObject);

            Ray aimRay = GetAimRay();

            damageDivision = targets.Count;

            foreach (HurtBox enemy in targets)
            {
                //do a check if its a max charge, it SlowOnHit. If not, then regular.
                EffectManager.SpawnEffect(spiritImpactEffect, new EffectData
                {
                    origin = enemy.gameObject.transform.position,
                    scale = 8f
                }, transmit: true);
                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = enemy.gameObject.transform.position - transform.position,
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

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
