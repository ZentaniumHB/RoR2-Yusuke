using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Survivors.Yusuke.Components;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    internal class OverdriveSpiritShotgunAA12 : BaseSkillState
    {

        private OverlapAttack attack;
        private Vector3 aimDirection;

        private float duration = 1f;
        private float overdriveTimeDuration;
        private float shotGunAA12StartupTime = 4.15f;   // second anim: 5.4, final : 6.6
        private float increaseFireRateTime = 5.4f;
        private float shotgunFireResetTime = 0.1f;
        private float shotgunLastShotTimer = 2.4f;
        private float overdriveFullDuration = 9f;
        private float spiritShotgunPotentDisplay = 1.72f;
        private float shotGunFireStopwatch;
        private bool hasPlayedAnim;
        private bool hasFiredFinalShot;

        private AimAnimator aimAnim;
        private Transform modelTransform;
        private PitchYawControl pitchYawControl;

        public List<HurtBox> previousHurtBoxes = new List<HurtBox>();
        public List<HurtBox> currentTargets = new List<HurtBox>();
        private float trackerUpdateStopwatch;
        private float trackerUpdateFrequency = 20f;
        private BullseyeSearch search;
        
        public float maxTrackingDistance = 80f;
        public float maxTrackingAngle = 30f;
        private int damageDivision;


        public static float damageCoefficient = YusukeStaticValues.overdriveShotgunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;

        private GameObject spiritGunMuzzleFlashPrefab;

        private GameObject overdriveShotgunChargePrefab = YusukeAssets.overdriveSpiritShotgunBeginEffect;
        private GameObject overdriveSpiritShotGunSinglePrefab = YusukeAssets.overdriveShotgunSingleShotEffect;
        private GameObject overdriveSpiritShotGunFinalPrefab = YusukeAssets.overdriveShotgunFinalShotEffect;
        private GameObject finalHitEffectPrefab;

        public static GameObject spiritTracerEffect = YusukeAssets.spiritShotGunTracerEffect;
        public GameObject spiritShotGunExplosionHitEffect = YusukeAssets.spiritShotGunHitEffect;
        private bool hasCreatedShotGunCharge;
        private readonly string mainPosition = "mainPosition";
        private readonly string muzzleCenter = "muzzleCenter";
        private readonly string handRPosition = "HandR";

        private YusukeWeaponComponent yusukeWeaponComponent;

        public override void OnEnter()
        {
            base.OnEnter();
            SetUpEffects();
            PlayAnimation("FullBody, Override", "OverdriveSpiritShotgunAA12Begin", "Slide.playbackRate", duration);
            //Util.PlaySound("Play_VoiceOverdriveShotgun", gameObject);

            aimDirection = GetAimRay().direction;
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
            pitchYawControl = new PitchYawControl();
            pitchYawControl.ChangePitchAndYawRange(true, modelTransform, aimAnim);
            shotGunFireStopwatch = shotgunFireResetTime;
            hasFiredFinalShot = false;
            search = new BullseyeSearch();

            yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetOverdriveState(true);

        }

        private void SetUpEffects()
        {
            overdriveShotgunChargePrefab.AddComponent<DestroyOnTimer>().duration = 2.7f;

            EffectComponent component = overdriveShotgunChargePrefab.GetComponent<EffectComponent>();
            if (component)
            {
                component.parentToReferencedTransform = true;
            }

            overdriveSpiritShotGunSinglePrefab.AddComponent<DestroyOnTimer>().duration = 1f;
            overdriveSpiritShotGunFinalPrefab.AddComponent<DestroyOnTimer>().duration = 1f;

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            overdriveTimeDuration += GetDeltaTime();
            trackerUpdateStopwatch += Time.fixedDeltaTime;

            Log.Info("Overdrive timer: "+ overdriveTimeDuration);
            if (overdriveTimeDuration > spiritShotgunPotentDisplay) CreateSpiritShotGunCharge();

            if (overdriveTimeDuration >= shotGunAA12StartupTime) {
                SwitchPitchYaw();
                shotGunFireStopwatch += GetDeltaTime();
                if(shotGunFireStopwatch > shotgunFireResetTime && !hasFiredFinalShot) 
                {
                    characterDirection.forward = GetAimRay().direction;
                    currentTargets = ScanForEnemies();
                    FireShotGunAA12(false);
                    shotGunFireStopwatch = 0;
                }

                if(overdriveTimeDuration > shotgunLastShotTimer + shotGunAA12StartupTime)
                {
                    if(!hasFiredFinalShot) FireShotGunAA12(true);
                    hasFiredFinalShot = true;
                }

                if (overdriveTimeDuration >= increaseFireRateTime) shotgunFireResetTime = 0.05f;


            }
            if (isAuthority && fixedAge >= duration)
            {
                if(overdriveTimeDuration > overdriveFullDuration) 
                {
                    outer.SetNextStateToMain();
                }
                
            }

        }

        private List<HurtBox> ScanForEnemies()
        {
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                trackerUpdateStopwatch = 0f;

                //CleanTargetsList(previousHurtBoxes);
                SearchForTarget(out var currentTargets);
                return currentTargets;
 
            }
            return previousHurtBoxes;
        }


        private void CreateSpiritShotGunCharge()
        {
            if (!hasCreatedShotGunCharge)
            {
                hasCreatedShotGunCharge = true;
                EffectManager.SimpleMuzzleFlash(overdriveShotgunChargePrefab, gameObject, handRPosition, true);
            }
        }

        private void SearchForTarget(out List<HurtBox> currentHurtbox)
        {

            Ray aimRay = GetAimRay();
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            // for everything that was found

            List<HurtBox> totalEnemies = new List<HurtBox>();
            foreach (HurtBox result in search.GetResults())
            {
                // if it has a healthbar and they are alive
                if ((bool)result.healthComponent && result.healthComponent.alive)
                {
                    totalEnemies.Add(result);

                }

            }

            currentHurtbox = totalEnemies;
            previousHurtBoxes = currentHurtbox;
            return;


        }

        private void SwitchPitchYaw()
        {
            if (!hasPlayedAnim)
            {
                hasPlayedAnim = true;
                pitchYawControl.ChangePitchAndYawRange(false, modelTransform, aimAnim);
                characterDirection.enabled = true;
            }
        }

        private void FireShotGunAA12(bool isFinalShot)
        {

            if (!isFinalShot)
            {
                EffectManager.SimpleMuzzleFlash(overdriveSpiritShotGunSinglePrefab, gameObject, mainPosition, false);
            }
            else
            {
                hasFiredFinalShot = true;
                characterDirection.enabled = false;
                EffectManager.SimpleMuzzleFlash(overdriveSpiritShotGunFinalPrefab, gameObject, mainPosition, false);

            }

            characterBody.AddSpreadBloom(1.5f);


            Util.PlaySound("HenryShootPistol", gameObject);

            Ray aimRay = GetAimRay();

            damageDivision = currentTargets.Count;

            foreach (HurtBox enemy in currentTargets)
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

        public override void OnExit()
        {
            base.OnExit();

            if(yusukeWeaponComponent) yusukeWeaponComponent.SetOverdriveState(false);
            pitchYawControl.ChangePitchAndYawRange(false, modelTransform, aimAnim);

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
