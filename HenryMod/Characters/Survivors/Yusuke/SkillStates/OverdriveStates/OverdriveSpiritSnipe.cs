using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.Components;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static UnityEngine.ParticleSystem.PlaybackState;
using static YusukeMod.Characters.Survivors.Yusuke.Components.SacredComponent;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    public class OverdriveSpiritSnipe : BaseSkillState
    {

        private float duration = 1f;
        private bool shouldReturn;
        private Vector3 forwardDirection;


        private readonly SphereSearch sphereSearch = new SphereSearch();
        private SpiritSnipeTracking snipeTracking;
        private HurtBox enemyHurtBox;
        private CharacterBody enemyBody;
        private bool hasTargetBeenFound;

        private float overdriveTimeDuration;
        private float overdriveSnipeStartUp = 3f;
        private float overdriveFreezeMax = 5f;
        private float overdriveFullDuration = 5.0f;
        private float overdriveParticleStartUp = 5.0f;
        private float pitchStartUp = 2.0f;


        private GameObject overdriveSpiritSniperBeginPrefab;
        private GameObject heavyHitEffectPrefab;
        private GameObject spiritGunMuzzleFlashPrefab;

        private List<HurtBox> totalEnemies;

        private YusukeWeaponComponent yusukeWeaponComponent;
        private Transform modelTransform;
        private PitchYawControl pitchYawControl;
        private AimAnimator aimAnim;
        private Animator animator;
        private Vector3 aimDirection;
        private bool hasPlayedAnimation;

        public static float damageCoefficient = YusukeStaticValues.overdriveSpiritSnipeDamageCoefficient;
        public GameObject overdriveSpiritSniperTracerEffect = YusukeAssets.overdriveSpiritSniperEffect;
        public GameObject spiritShotGunExplosionHitEffect = YusukeAssets.spiritShotGunHitEffect;

        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        
        private readonly string fingerTipString = "fingerTipR";
        private readonly string muzzleCenter = "muzzleCenter";

        private bool hasRevertedPitch;
        private HealthComponent yusukeHealth;

        public override void OnEnter()
        {
            base.OnEnter();

            totalEnemies = new List<HurtBox>();
            if (!isGrounded)
            {
                Chat.AddMessage("Need to be grounded for the OVERDRIVE: SPIRIT SNIPE");
                Log.Warning("Need to be grounded for the OVERDRIVE: SPIRIT SNIPE");
                shouldReturn = true;

            }
            else
            {
                SetUpEffects();
                forwardDirection = GetAimRay().direction;

                snipeTracking = gameObject.GetComponent<SpiritSnipeTracking>();
                enemyHurtBox = snipeTracking.GetTrackingTarget();

                if (enemyHurtBox)
                {
                    enemyBody = enemyHurtBox.healthComponent.body;

                    if (enemyBody)
                    {
                        if (CheckIfEnemyisAlive())
                        {
                            hasTargetBeenFound = true;
                        }
                        else
                        {
                            shouldReturn = true;
                        }

                    }

                }
                else
                {
                    Log.Error("No enemy found. ");
                    shouldReturn = true;
                }

                if (hasTargetBeenFound)
                {
                    Log.Info("Sniping them.");
                    PlayAnimation("FullBody, Override", "OverdriveSpiritgunSniperBegin", "Slide.playbackRate", duration);
                    //Util.PlaySound("Play_SoundOverdrive12Hooks", gameObject);

                    EffectManager.SimpleMuzzleFlash(overdriveSpiritSniperBeginPrefab, gameObject, fingerTipString, true);

                    aimDirection = (enemyBody.transform.position - transform.position).normalized;
                    if (characterDirection)
                    {
                        characterDirection.forward = aimDirection;
                        characterDirection.enabled = false;
                    }

                    if (characterMotor)
                    {
                        characterMotor.velocity = new Vector3(0, 0, 0);
                        characterMotor.enabled = false;

                    }

                    modelTransform = GetModelTransform();
                    pitchYawControl = gameObject.GetComponent<PitchYawControl>();
                    animator = modelLocator.modelTransform.gameObject.GetComponent<Animator>();
                    // DELETE IF NEEDED

                    aimAnim = modelLocator.modelTransform.gameObject.GetComponent<AimAnimator>();
                    pitchYawControl.ChangePitchAndYawRange(true, modelTransform, aimAnim);  // changing the pitch/yaw to large value

                    yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
                    yusukeWeaponComponent.SetOverdriveState(true);

                    yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
                    yusukeWeaponComponent.SetOverdriveState(true);
                    ScanUsingSphere();

                    yusukeHealth = characterBody.GetComponent<HealthComponent>();
                    if (NetworkServer.active)
                    {
                        characterBody.AddTimedBuff(JunkContent.Buffs.IgnoreFallDamage, 1f);
                        if (yusukeHealth)
                        {
                            yusukeHealth.godMode = true;

                        }

                    }


                }

            }

        }

        private void StunThem(float overdriveFreezeMax, HurtBox enemyBox)
        {
            enemyBox.healthComponent.GetComponent<SetStateOnHurt>()?.SetStunInternal(overdriveFreezeMax + 1);
            EntityStateMachine component = enemyBox.healthComponent.body.GetComponent<EntityStateMachine>();
            if (component)
            {
                OverdriveTemporarySlow state = new OverdriveTemporarySlow
                {
                    duration = overdriveFreezeMax + 1
                };
                component.SetState(state);
            }
            
        }

        private void ScanUsingSphere()
        {
            
            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(characterBody.teamComponent.teamIndex),
                filterByLoS = false,
                searchOrigin = characterBody.corePosition,
                searchDirection = UnityEngine.Random.onUnitSphere,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = snipeTracking.GetMaxTrackingDistance() + 10f,
                maxAngleFilter = 360f
            };

            search.RefreshCandidates();
            search.FilterOutGameObject(characterBody.gameObject);

            List<HurtBox> target = search.GetResults().ToList<HurtBox>();
            foreach (HurtBox enemyTarget in target)
            {
                if (enemyTarget.healthComponent && enemyTarget.healthComponent.body)
                {
                    // stun and freeze within radius
                    StunThem(overdriveFreezeMax, enemyTarget);
                }
            }

        }

        private bool CheckIfEnemyisAlive()
        {
            if (enemyBody && enemyBody.healthComponent && enemyBody.healthComponent.alive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetUpEffects()
        {
            overdriveSpiritSniperBeginPrefab = YusukeAssets.overdriveSpiritGunSniperBeginEffect;
            overdriveSpiritSniperBeginPrefab.AddComponent<DestroyOnTimer>().duration = overdriveSnipeStartUp;


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();


            if (shouldReturn)
            {
                ReturnToMain();
            }
            else
            {
                overdriveTimeDuration += GetDeltaTime();
                overdriveFreezeMax -= GetDeltaTime();

                if (inputBank) inputBank.aimDirection = aimDirection;

                if (overdriveTimeDuration > pitchStartUp)
                {
                    if (!hasRevertedPitch)
                    {
                        //hasRevertedPitch = true;
                        RevertPitch(); 
                    }

                }

                if (overdriveTimeDuration > overdriveSnipeStartUp && !hasPlayedAnimation)
                {
                    hasPlayedAnimation = true;
                    PlayAnimation("FullBody, Override", "OverdriveSpiritgunSniperFinish", "Slide.playbackRate", duration);
                    ShootTarget();
                    //aimAnim.enabled = false;
                }

                if (isAuthority && fixedAge >= duration)
                {
                    if (overdriveTimeDuration > overdriveFullDuration) outer.SetNextStateToMain();
                }
            }

        }

        // attempts to slowly decrease the pitch/yaw value to its original
        private void RevertPitch()
        {
            pitchYawControl.RestorePitch(modelTransform, aimAnim, 1);
        }


        private void ReturnToMain()
        {
            skillLocator.primary.AddOneStock();
            outer.SetNextStateToMain();
            return;
        }

        private void ShootTarget()
        {
            characterBody.AddSpreadBloom(1.5f);
            EffectManager.SimpleMuzzleFlash(spiritGunMuzzleFlashPrefab, gameObject, muzzleCenter, false);
            Util.PlaySound("HenryShootPistol", gameObject);

            Ray aimRay = GetAimRay();


            new BulletAttack
            {
                bulletCount = 1,
                aimVector = aimDirection,
                origin = aimRay.origin,     // or FindModelChild(fingerTipString).position;
                damage = damageCoefficient * damageStat,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.SlowOnHit,
                falloffModel = BulletAttack.FalloffModel.None,
                maxDistance = range,
                force = force,
                hitMask = LayerIndex.CommonMasks.bullet,
                minSpread = 0f,
                maxSpread = 0f,
                isCrit = true,
                owner = gameObject,
                muzzleName = muzzleCenter,
                smartCollision = true,
                procChainMask = default,
                procCoefficient = procCoefficient,
                radius = YusukeStaticValues.shotgunPelletRadius,
                sniper = false,
                stopperMask = LayerIndex.CommonMasks.bullet,
                weapon = null,
                tracerEffectPrefab = overdriveSpiritSniperTracerEffect,
                spreadPitchScale = 1f,
                spreadYawScale = 1f,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                hitEffectPrefab = spiritShotGunExplosionHitEffect,

            }.Fire();


        }

        public override void OnExit()
        {
            base.OnExit();

            
            if (yusukeWeaponComponent) yusukeWeaponComponent.SetOverdriveState(false);

            // DELETE IF NEEDED
            aimAnim = modelLocator.modelTransform.gameObject.GetComponent<AimAnimator>();
            //pitchYawControl.RestorePitch(modelTransform, aimAnim);
            aimAnim.enabled = true;

            if (!shouldReturn)
            {
                pitchYawControl.ChangePitchAndYawRange(false, modelTransform, aimAnim);
                pitchYawControl.ResetElapsedTime(); // resets the time for next time usage.
                gameObject.GetComponent<SacredComponent>().UseOverdriveAbility((byte)OverdriveType.STANDARD);

                if (NetworkServer.active)
                {
                    if (yusukeHealth)
                    {
                        yusukeHealth.godMode = false;
                        characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 1f * duration);
                    }

                }
            }

            if (characterDirection)
            {
                characterDirection.enabled = true;
            }

            if (characterMotor)
            {
                characterMotor.enabled = true;
                characterMotor.velocity = new Vector3(0, 0, 0);

            }



        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
