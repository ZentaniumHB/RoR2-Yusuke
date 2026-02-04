using EntityStates;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    internal class OverdriveSpiritWaveImpactFist : BaseSkillState
    {

        protected string hitboxGroupName = "overdriveWaveGroup";
        protected DamageType damageType = DamageType.Generic;
        protected float damageCoefficient = 40f;
        protected float procCoefficient = 1f;
        protected float pushForce = 3000f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float baseDuration = 1f;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound = NetworkSoundEventIndex.Invalid;
        private OverlapAttack attack;
        private Vector3 aimDirection;

        private float duration = 1f;
        private float overdriveTimeDuration;
        private float impactTime = 2.82f;
        private float impactDuration = 3.9f;

        private bool attackComplete;
        private bool hasFired;

        private GameObject overdriveWaveFinishPrefab;
        private GameObject overdriveSpiritWaveBeginPrefab;
        private GameObject finalHitEffectPrefab;

        private AimAnimator aimAnim;
        private Transform modelTransform;
        private PitchYawControl pitchYawControl;

        private bool hasCreatedWaveCharge;

        private readonly string mainPosition = "mainPosition";
        private readonly string muzzleCenter = "muzzleCenter";
        private readonly string handRPosition = "HandR";

        private YusukeWeaponComponent yusukeWeaponComponent;

        public override void OnEnter()
        {
            base.OnEnter();
            SetUpEffects();
            PlayAnimation("FullBody, Override", "OverdriveSpiritWaveImpactFistBegin", "Slide.playbackRate", duration);
            //Util.PlaySound("Play_VoiceOverdriveWave", gameObject);
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

            yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetOverdriveState(true);
        }

        private void SetUpEffects()
        {
            overdriveWaveFinishPrefab = YusukeAssets.overdriveWaveFinishEffect;
            overdriveWaveFinishPrefab.AddComponent<DestroyOnTimer>().duration = 5f;

            overdriveSpiritWaveBeginPrefab = YusukeAssets.overdriveSpiritWaveBeginEffect;
            overdriveSpiritWaveBeginPrefab.AddComponent<DestroyOnTimer>().duration = impactTime - 0.5f;

            finalHitEffectPrefab = YusukeAssets.finalHitEffect;
            finalHitEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;

            EffectComponent component = overdriveSpiritWaveBeginPrefab.GetComponent<EffectComponent>();
            if (component)
            {
                component.parentToReferencedTransform = true;
            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            overdriveTimeDuration += GetDeltaTime();
            Log.Info("overdrivetime: " + overdriveTimeDuration);

            if (overdriveTimeDuration > 0.5f) CreateSpiritWaveCharge();

            if (overdriveTimeDuration > impactTime) CreateImpactWave();

            if (overdriveTimeDuration > impactTime + impactDuration) attackComplete = true;

            if (isAuthority && fixedAge >= duration)
            {
                if (attackComplete)
                {
                    outer.SetNextStateToMain();
                }
                
            }

        }

        private void CreateSpiritWaveCharge()
        {
            if (!hasCreatedWaveCharge)
            {
                hasCreatedWaveCharge = true;
                EffectManager.SimpleMuzzleFlash(overdriveSpiritWaveBeginPrefab, gameObject, handRPosition, false);
            }
        }

        private void CreateImpactWave()
        {
            if (!hasFired)
            {
                hasFired = true;

                attack = new OverlapAttack();
                attack.damageType = damageType;
                attack.attacker = gameObject;
                attack.inflictor = gameObject;
                attack.teamIndex = GetTeam();
                attack.damage = damageCoefficient * damageStat;
                attack.procCoefficient = procCoefficient;
                attack.hitEffectPrefab = hitEffectPrefab;
                attack.forceVector = bonusForce;
                attack.pushAwayForce = pushForce;
                attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
                attack.isCrit = RollCrit();
                attack.impactSound = impactSound;
                attack.Fire();
                Log.Info("Fired attack");

                EffectManager.SimpleMuzzleFlash(overdriveWaveFinishPrefab, gameObject, mainPosition, true);
                EffectManager.SimpleMuzzleFlash(finalHitEffectPrefab, gameObject, muzzleCenter, true);

                PlayAnimation("FullBody, Override", "OverdriveSpiritWaveImpactFistFinish", "Slide.playbackRate", duration);
            }
            

        }

        public override void OnExit()
        {
            base.OnExit();

            if(yusukeWeaponComponent)yusukeWeaponComponent.SetOverdriveState(false);
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
