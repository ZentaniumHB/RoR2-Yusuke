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
using YusukeMod.Characters.Survivors.Yusuke.Extra;

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

        private float animationTime;
        private float maxAnimationTime = 2.5f;
        private Ray fixedAimRay;

        public float penaltyTime;
        private EntityStateMachine stateMachine;
        private YusukeMain mainState;
        private Transform modelTransform;
        private AimAnimator aimAnim;

        public GameObject spiritGunMegaChargeEffectObject;
        public GameObject spiritGunMegaChargeEffectPotentObject;
        public GameObject chargeWindObject;
        public GameObject mazokuSparkElectricityObject;

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

            stateMachine = characterBody.GetComponent<EntityStateMachine>();

            characterBody.SetAimTimer(1f);
            modelTransform = GetModelTransform();
            aimAnim = modelTransform.GetComponent<AimAnimator>();

            PauseVelocity();
            duration = 0.6f;
            fireTime = 0.5f;


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
            rotationIgnore.referenceTransform = FindModelChild("thighShadowCastR");

            if (component)
            {
                component.parentToReferencedTransform = true;

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            SwitchAnimationLayer();
            aimAnim.enabled = true;

            characterMotor.enabled = true;
            characterDirection.enabled = true;

            pivotRotation = GetComponent<PivotRotation>();
            pivotRotation.ResetOnlyVFXRotation();
            pivotRotation.SetRotations(Vector3.zero, false, false, false);

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
                PlayAnimation("BothHands, Override", "BufferEmpty", "ShootGun.playbackRate", 1f);

                if (spiritGunMegaChargeEffectObject) EntityState.Destroy(spiritGunMegaChargeEffectObject);
                if (spiritGunMegaChargeEffectPotentObject) EntityState.Destroy(spiritGunMegaChargeEffectPotentObject);
                if (chargeWindObject) EntityState.Destroy(chargeWindObject);
                if (mazokuSparkElectricityObject) EntityState.Destroy(mazokuSparkElectricityObject);

                EffectManager.SimpleMuzzleFlash(spiritGunMegaMuzzleFlashPrefab, gameObject, fingerTipString, false);
                

                SpawnEffectOnBodyParts();

                //base.characterBody.AddSpreadBloom(1.5f);
                //EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isGrounded)
                {
                    PlayAnimation("FullBody, Override", "ShootSpiritMegaGrounded", "ShootGun.playbackRate", 1f);
                    EffectManager.SimpleMuzzleFlash(megaWindEffectPrefab, gameObject, mainPosition, false);
                }
                else
                {
                    PlayAnimation("FullBody, Override", "ShootSpiritGunFollowUpAir", "ShootGun.playbackRate", 1f);
                }


                if (isAuthority)
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

                    AddRecoil(-1f * recoil, -2f * FireSpiritShotgun.recoil, -0.5f * FireSpiritShotgun.recoil, 0.5f * FireSpiritShotgun.recoil);


                    DamageType value = (DamageType.AOE);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = prefab,
                        position = fixedAimRay.origin,
                        rotation = Util.QuaternionSafeLookRotation(fixedAimRay.direction),
                        owner = gameObject,
                        damage = damageStat * damageCoefficient + (charge),
                        damageTypeOverride = value,
                        force = force,
                        crit = Util.CheckRoll(critStat, characterBody.master),

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
            foreach (string bodyPart in bodyParts)
            {
                EffectManager.SimpleMuzzleFlash(blackCastShadowFadedPrefab, gameObject, bodyPart, false);
            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            animationTime += GetDeltaTime();

            //Log.Info("fixedAge:" + fixedAge);
            if (fixedAge >= fireTime)
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
                if (animationTime > maxAnimationTime)
                {
                    ResumeVelocity();
                    CheckPenaltyTimer();
                    outer.SetNextStateToMain();
                    return;
                }

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
            return InterruptPriority.Death;
        }
    }
}
