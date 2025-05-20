using EntityStates;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Survivors.Yusuke.SkillStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Modules.BaseStates;
using static YusukeMod.Modules.BaseStates.YusukeMain;
using YusukeMod.Characters.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    public class FireDemonGunMega : BaseState
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

        public static GameObject prefab = YusukeAssets.spiritGunMegaPrefab;

        public bool tier1Wave;
        public bool tier2Wave;

        public float penaltyTime;
        private EntityStateMachine stateMachine;
        private YusukeMain mainState;

        public override void OnEnter()
        {
            base.OnEnter();

            stateMachine = characterBody.GetComponent<EntityStateMachine>();
            characterBody.SetAimTimer(1f);
            PauseVelocity();
            duration = 0.6f;
            fireTime = 0.5f;

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);

            if (tier1Wave)
            {
                Wave wave = new Wave
                {
                    amplitude = 0.5f,
                    frequency = 30f,
                    cycleOffset = 0f
                };
                ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 1.5f, 20f, true);
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
                ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 1.5f, 20f, true);
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            SwitchAnimationLayer();
            PlayAnimation("BothHands, Override", "BufferEmpty", "ShootGun.playbackRate", 1f);

        }

        // the animation switching is done once the YusukeMain state is taken
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
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.MegaCharge, false);


                    // need to re-enable the mazoku layer since the transformation it's still active
                    MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
                    if (maz.hasTransformed)
                    {
                        mainState.SwitchMovementAnimations((int)AnimationLayerIndex.Mazoku, true);
                    }

                }

            }
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                Log.Info("Firing gun");
                //base.characterBody.AddSpreadBloom(1.5f);
                //EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isGrounded)
                {
                    PlayAnimation("FullBody, Override", "ShootSpiritMegaGrounded", "ShootGun.playbackRate", 1f);
                }
                else
                {
                    PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpAir", "ShootGun.playbackRate", 1f);
                }


                if (isAuthority)
                {
                    Ray aimRay = GetAimRay();
                    AddRecoil(-1f * recoil, -2f * FireSpiritShotgun.recoil, -0.5f * FireSpiritShotgun.recoil, 0.5f * FireSpiritShotgun.recoil);


                    DamageType value = (DamageType.AOE);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = prefab,
                        position = aimRay.origin,
                        rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                        owner = gameObject,
                        damage = damageStat * damageCoefficient + (charge),
                        damageTypeOverride = value,
                        force = force,
                        crit = Util.CheckRoll(critStat, characterBody.master),

                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);


                }
            }
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Log.Info("fixedAge:" + fixedAge);
            if (fixedAge >= fireTime)
            {
                ResumeVelocity();
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                CheckPenaltyTimer();
                outer.SetNextStateToMain();
                return;

            }

        }

        private void CheckPenaltyTimer()
        {
            if (stateMachine == null)
            {
                Log.Error("No State machine found");
            }
            else
            {
                Type currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    Log.Info("Yusuke State machine found");
                    YusukeMain targetState = (YusukeMain)stateMachine.state;
                    targetState.penaltyTimer = Mathf.Round(penaltyTime);

                }
                else
                {
                    Log.Error("This is not the YusukeMain state.");

                }


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
            return InterruptPriority.Frozen;
        }
    }
}
