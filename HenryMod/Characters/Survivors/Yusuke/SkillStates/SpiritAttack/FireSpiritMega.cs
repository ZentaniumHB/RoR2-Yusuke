using EntityStates;
using EntityStates.Treebot.Weapon;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static YusukeMod.Modules.BaseStates.YusukeMain;

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

        private float animationTime;
        private float maxAnimationTime = 2.5f;
        private Ray fixedAimRay;

        private YusukeMain mainState;
        private Transform modelTransform;
        private AimAnimator aimAnim;
        private float originalGiveUpDuration = 0f;

        public GameObject spiritGunMegaChargeEffectObject;
        public GameObject spiritGunMegaChargeEffectPotentObject;
        public GameObject chargeWindObject;

        private readonly string fingerTipString = "fingerTipR";
        private readonly string mainPosition = "mainPosition";

        private readonly string thighL = "thighShadowCastL";
        private readonly string thighR = "thighShadowCastR";
        private readonly string handL = "HandL";
        private readonly string handR = "HandR";
        private readonly string footL = "FootL";
        private readonly string footR = "FootR";
        private readonly string upperArmL = "UpperArmL";
        private readonly string upperArmR = "UpperArmR";
        private readonly string lowerArmL = "LowerArmL";
        private readonly string lowerArmR = "LowerArmR";
        private readonly string calfL = "CalfL";
        private readonly string calfR = "CalfL";

        private List<string> bodyParts = new List<string>();

        private GameObject spiritGunMegaMuzzleFlashPrefab;
        private GameObject megaWindEffectPrefab;
        private GameObject blackCastShadowFadedPrefab;
        private IgnoreParentRotation rotationIgnore;

        private PivotRotation pivotRotation;

        public override void OnEnter()
        {
            base.OnEnter();

            spiritGunMegaMuzzleFlashPrefab = YusukeAssets.spiritGunMegaMuzzleFlashEffect;
            megaWindEffectPrefab = YusukeAssets.megaWindEffect;
            blackCastShadowFadedPrefab = YusukeAssets.blackCastShadowEffect;

            base.characterBody.SetAimTimer(1f);
            modelTransform = GetModelTransform();
            aimAnim = modelTransform.GetComponent<AimAnimator>();

            PauseVelocity();
            this.duration = 0.6f;
            this.fireTime = 0.5f;

            if (tier1Wave)
            {
                Wave wave = new Wave
                {
                    amplitude = 0.5f,
                    frequency = 10f,
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
                    frequency = 20f,
                    cycleOffset = 0f
                };
                RoR2.ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 1.5f, 20f, true);
            }

            SpawnMuzzleEffect();

            pivotRotation = GetComponent<PivotRotation>();


            bodyParts.Add(thighL);
            bodyParts.Add(thighR);
            bodyParts.Add(handL);
            bodyParts.Add(handR);
            bodyParts.Add(footL);
            bodyParts.Add(footR);
            bodyParts.Add(upperArmL);
            bodyParts.Add(upperArmR);
            bodyParts.Add(lowerArmL);
            bodyParts.Add(lowerArmR);
            bodyParts.Add(calfL);
            bodyParts.Add(calfR);

        }

        private void SpawnMuzzleEffect()
        {
            // destroy timer will destroy the object effect after the duration of 2 seconds, the creation of effects had this by default when adding to the effects list, but this allows flexibility 
            EffectComponent component = spiritGunMegaMuzzleFlashPrefab.GetComponent<EffectComponent>();
            spiritGunMegaMuzzleFlashPrefab.AddComponent<DestroyOnTimer>().duration = 2;
            megaWindEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;
            blackCastShadowFadedPrefab.AddComponent<DestroyOnTimer>().duration = 1.2f;
            rotationIgnore = blackCastShadowFadedPrefab.AddComponent<IgnoreParentRotation>();
            rotationIgnore.SetLookRotation(GetAimRay().direction);
            rotationIgnore.referenceTransform = FindModelChild("thighShadowCastR"); // needs at least one of the thights to rotate the rest since it's an empty object, doesn't matter which.

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
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.MegaCharge, false);

                }

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeWindObject) Log.Info("THE MEGA WIND STILL EXISTS!");
            //aimAnim.giveupDuration = originalGiveUpDuration;
            aimAnim.enabled = true;

            characterMotor.enabled = true;
            characterDirection.enabled = true;

            pivotRotation = GetComponent<PivotRotation>();
            pivotRotation.ResetOnlyVFXRotation();
            pivotRotation.SetRotations(Vector3.zero, false, false, false);
            SwitchAnimationLayer();


        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                



                PlayAnimation("BothHands, Override", "BufferEmpty", "ShootGun.playbackRate", 1f);

                if (spiritGunMegaChargeEffectObject) EntityState.Destroy(spiritGunMegaChargeEffectObject);
                if (spiritGunMegaChargeEffectPotentObject) EntityState.Destroy(spiritGunMegaChargeEffectPotentObject);
                if (chargeWindObject) EntityState.Destroy(chargeWindObject);

                EffectManager.SimpleMuzzleFlash(spiritGunMegaMuzzleFlashPrefab, gameObject, fingerTipString, false);
                

                SpawnEffectOnBodyParts();
                /*EffectManager.SimpleMuzzleFlash(blackCastShadowFadedPrefab, gameObject, thighL, false);
                EffectManager.SimpleMuzzleFlash(blackCastShadowFadedPrefab, gameObject, thighR, false);
                EffectManager.SimpleMuzzleFlash(blackCastShadowFadedPrefab, gameObject, handR, false);*/

                Util.PlaySound("HenryShootPistol", base.gameObject);

                if (isGrounded)
                {
                    PlayAnimation("FullBody, Override", "ShootSpiritMegaGrounded", "ShootGun.playbackRate", 1f);
                    EffectManager.SimpleMuzzleFlash(megaWindEffectPrefab, gameObject, mainPosition, false);
                }
                else
                {
                    PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpAir", "ShootGun.playbackRate", 1f);
                }

                if (base.isAuthority)
                {
                    fixedAimRay = base.GetAimRay();

                    if (characterDirection)
                    {
                        characterDirection.forward = fixedAimRay.direction;
                        characterDirection.moveVector = fixedAimRay.direction;

                    }


                    if (isGrounded)
                    {
                        pivotRotation.SetOnlyVFXRotation();
                        pivotRotation.SetRotations(fixedAimRay.direction, true, true, false);
                        characterMotor.enabled = false;
                        characterDirection.enabled = false;
                    }
                    else
                    {
                        pivotRotation.SetRotations(fixedAimRay.direction, true, true, false);
                    }


                    base.AddRecoil(-1f * FireSpiritShotgun.recoil, -2f * FireSpiritShotgun.recoil, -0.5f * FireSpiritShotgun.recoil, 0.5f * FireSpiritShotgun.recoil);
                    

                    DamageType value = (DamageType.AOE);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = prefab,
                        position = fixedAimRay.origin,
                        rotation = Util.QuaternionSafeLookRotation(fixedAimRay.direction),
                        owner = base.gameObject,
                        damage = damageStat * damageCoefficient,
                        damageTypeOverride = value,
                        force = force,
                        crit = Util.CheckRoll(critStat, base.characterBody.master),

                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);


                }

                // I can either set the giveupDuration to 0f or I can simple disable and re-enable it, or even both.
                /*originalGiveUpDuration = aimAnim.giveupDuration;
                aimAnim.giveupDuration = 0f;*/
                aimAnim.enabled = false;

                // animation time in the air is shorter due to the animation itself being shorter. 
                if (!isGrounded) maxAnimationTime = 1.2f;

            }
        }

        private void SpawnEffectOnBodyParts()
        {
            foreach(string bodyPart in bodyParts)
            {
                EffectManager.SimpleMuzzleFlash(blackCastShadowFadedPrefab, gameObject, bodyPart, false);
            }
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            animationTime += GetDeltaTime();

            Log.Info("fixedAge:" +fixedAge);
            if (base.fixedAge >= fireTime)
            {
                characterBody.SetAimTimer(0.1f);
                Fire();
                //inputBank.moveVector = Vector3.zero;
                characterDirection.moveVector = Vector3.zero;
                characterMotor.moveDirection = Vector3.zero;

                if (!isGrounded)
                {
                    ResumeVelocity();
                    characterDirection.moveVector = Vector3.zero;   // prevents character input movement 

                    // reverse the direction, so it seems it has a knockback effect.
                    Vector3 awayFromDirection = (-fixedAimRay.direction).normalized;
                    Vector3 backWardSpeed = awayFromDirection * moveSpeedStat * (0.9f);
                    // Apply the velocity to the character's motor
                    characterMotor.velocity = backWardSpeed;
                }
            }

            if (fixedAge >= duration && isAuthority)
            {
                if(animationTime > maxAnimationTime)
                {
                    ResumeVelocity();
                    outer.SetNextStateToMain();
                    return;
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
            return InterruptPriority.Death;
        }
    }
}