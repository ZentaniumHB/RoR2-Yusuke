using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Survivors.Yusuke.Components;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using static YusukeMod.Characters.Survivors.Yusuke.Components.SacredComponent;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    internal class OverdriveSpiritFlow : BaseSkillState
    {

        private float duration = 1f;
        private float overdriveTimeDuration;
        private float overdriveFullDuration = 5.9f;

        private GameObject overdriveFlowEffectPrefab;

        private Vector3 aimDirection;
        private Transform modelTransform;
        private PitchYawControl pitchYawControl;
        private YusukeWeaponComponent yusukeWeaponComponent;

        private AimAnimator aimAnim;
        HealthComponent yusukeHealth;

        public override void OnEnter()
        {
            base.OnEnter();
            SetUpEffects();
            PlayAnimation("FullBody, Override", "OverdriveSpiritFlow", "Slide.playbackRate", duration);

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
            pitchYawControl = gameObject.GetComponent<PitchYawControl>();
            pitchYawControl.ChangePitchAndYawRange(true, modelTransform, aimAnim);

            yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetOverdriveState(true);


            yusukeHealth = characterBody.GetComponent<HealthComponent>();
            if (NetworkServer.active)
            {
                if (yusukeHealth)
                {
                    yusukeHealth.godMode = true;
                }

            }

            EffectManager.SpawnEffect(overdriveFlowEffectPrefab, new EffectData
            {
                origin = FindModelChild("mainPosition").position,
                scale = 1f
            }, transmit: true);
        }

        private void SetUpEffects()
        {
            overdriveFlowEffectPrefab = YusukeAssets.overdriveSpiritFlowEffect;
            overdriveFlowEffectPrefab.AddComponent<DestroyOnTimer>().duration = overdriveFullDuration;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            overdriveTimeDuration += GetDeltaTime();

            if (isAuthority && fixedAge >= duration)
            {
                if (overdriveTimeDuration > overdriveFullDuration)
                {
                    outer.SetNextStateToMain();
                }
            }

        }

        public override void OnExit()
        {
            base.OnExit();


            if (yusukeWeaponComponent) yusukeWeaponComponent.SetOverdriveState(false);
            pitchYawControl.ChangePitchAndYawRange(false, modelTransform, aimAnim);

            if (characterDirection)
            {
                characterDirection.enabled = true;
            }

            if (NetworkServer.active)
            {
                if (yusukeHealth)
                {
                    yusukeHealth.godMode = false;
                    characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 1f * duration);
                }

            }

            if (characterMotor)
            {
                characterMotor.enabled = true;
                characterMotor.velocity = new Vector3(0, 0, 0);

            }

            gameObject.GetComponent<SacredComponent>().UseOverdriveAbility((byte)OverdriveType.TIMED);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
