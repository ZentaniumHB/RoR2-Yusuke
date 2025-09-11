using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using static YusukeMod.Modules.BaseStates.YusukeMain;
using Random = UnityEngine.Random;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    internal class FireSpiritBeam : BaseSkillState
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

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        public float charge;

        public bool isPrimary;
        private Ray aimRay;

        private int damageTypeDecider;
        private BulletAttack beamBullet;

        private YusukeMain mainState;

        private float knockBackTime;
        private float knockBackDuration = 1f;

        private PivotRotation pivotRotation;
        private Vector3 forwardDirection;

        private GameObject spiritGunMuzzleFlashPrefab;
        private GameObject spiritGunBeamPrefab;
        private GameObject dashBoomPrefab;

        private readonly string muzzleCenter = "muzzleCenter";
        private readonly string dashCenter = "muzzleCenter";

        public override void OnEnter()
        {
            base.OnEnter();

            spiritGunMuzzleFlashPrefab = YusukeAssets.spiritGunMuzzleFlashEffect;
            spiritGunBeamPrefab = YusukeAssets.spiritgunBeamEffect;
            dashBoomPrefab = YusukeAssets.dashBoomEffect;

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
            muzzleString = "Muzzle";

            damageTypeDecider = Random.Range(1, 3);     // makes it pick either 1 or 2
            Log.Info("type move: " + damageTypeDecider);

            pivotRotation = GetComponent<PivotRotation>();
            forwardDirection = GetAimRay().direction;

            if (isGrounded)
            {
                PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpGrounded", "ShootGun.playbackRate", duration);
                pivotRotation.SetOnlyVFXRotation();
                pivotRotation.SetRotations(forwardDirection, true, true, false);
            }
            else
            {
                PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpAir", "ShootGun.playbackRate", duration);
                pivotRotation.SetRotations(forwardDirection, true, true, false);
            }
            //PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
            SpawnChargeEffect();

        }

        private void SpawnChargeEffect()
        {
            spiritGunBeamPrefab.AddComponent<DestroyOnTimer>().duration = 2;
            spiritGunMuzzleFlashPrefab.AddComponent<DestroyOnTimer>().duration = 2;
            dashBoomPrefab.AddComponent<DestroyOnTimer>().duration = 2;
        }

        public override void OnExit()
        {
            base.OnExit();
            pivotRotation = GetComponent<PivotRotation>();
            pivotRotation.ResetOnlyVFXRotation();
            pivotRotation.SetRotations(Vector3.zero, false, false, false);
            SwitchAnimationLayer();

        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterDirection.forward = aimRay.direction;
            characterDirection.moveVector = aimRay.direction;

            if (fixedAge >= fireTime)
            {
                Fire();
            }

            if (hasFired)
            {
                characterDirection.moveVector = Vector3.zero;   // prevents character input movement 
                knockBackTime += GetDeltaTime();
                if (!isGrounded)
                {
                    // reverse the direction, so it seems it has a knockback effect.
                    Vector3 awayFromDirection = (-aimRay.direction).normalized;
                    Vector3 backWardSpeed = awayFromDirection * moveSpeedStat;
                    // Apply the velocity to the character's motor
                    characterMotor.velocity = backWardSpeed;
                }
                
            }

            if (fixedAge >= duration && isAuthority)
            {
                if (knockBackTime > knockBackDuration) 
                {
                    outer.SetNextStateToMain();
                    return;
                }
                
            }
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                characterBody.AddSpreadBloom(1.5f);
                
                EffectManager.SimpleMuzzleFlash(spiritGunMuzzleFlashPrefab, gameObject, muzzleCenter, false);
                EffectManager.SimpleMuzzleFlash(spiritGunBeamPrefab, gameObject, muzzleCenter, false);
                EffectManager.SimpleMuzzleFlash(dashBoomPrefab, gameObject, dashCenter, false);

                Util.PlaySound("HenryShootPistol", gameObject);

                if (isAuthority)
                {
                    aimRay = GetAimRay();
                    //instantly look towards the direction
                    characterDirection.forward = aimRay.direction;
                    characterDirection.moveVector = aimRay.direction;
                    AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
                    //characterMotor.velocity = -aimRay.direction * 18f;  // pushback
                    

                    beamBullet = new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damageCoefficient * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
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
                        radius = 2f,
                        sniper = true,
                        stopperMask = LayerIndex.world.mask,
                        weapon = null,
                        tracerEffectPrefab = tracerEffectPrefab,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    };

                    // randomiser determins the damage type
                    if(damageTypeDecider == 1)
                    {
                        beamBullet.damageType = DamageType.Generic;
                    }
                    else
                    {
                        beamBullet.damageType = DamageType.Shock5s;
                    }

                    gameObject.AddComponent<SkillTags>();
                    beamBullet.Fire();
                    
                }

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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
