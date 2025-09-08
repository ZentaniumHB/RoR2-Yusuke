using EntityStates;
using RoR2;
using System;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp
{
    public class ReleaseSpiritCuff : BaseSkillState
    {
        public static float baseDuration = 1.40f;

        private float duration = 1f;
        private float startUpDuration = 1.10f;
        private float animationTimer;

        private bool hasSpawnedCuffEffect;

        private GameObject spiritCuffReleasePrefab;
        private GameObject spiritCuffEffectPrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("FullBody, Override", "SpiritCuffRelease", "ThrowBomb.playbackRate", duration);

            spiritCuffReleasePrefab = YusukeAssets.spiritCuffReleaseEffect;
            spiritCuffEffectPrefab = YusukeAssets.spiritCuffEffect;

            CreateAndEditEffect();

        }

        private void CreateAndEditEffect()
        {
            if(spiritCuffEffectPrefab) spiritCuffReleasePrefab.AddComponent<DestroyOnTimer>().duration = 3f;
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            animationTimer += GetDeltaTime();

            if (animationTimer > startUpDuration)
            {
                PlayEffect();
                
            }
            if (isAuthority && fixedAge >= baseDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void PlayEffect()
        {
            
            if (!hasSpawnedCuffEffect)
            {
                Log.Info("Spawning effect");
                hasSpawnedCuffEffect = true;
                EffectManager.SpawnEffect(spiritCuffReleasePrefab, new EffectData
                {
                    origin = FindModelChild("mainPosition").position,
                    rotation = FindModelChild("mainPosition").rotation,
                    scale = 1f
                }, transmit: true);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            SpiritCuffComponent cuffComponent = gameObject.GetComponent<SpiritCuffComponent>();
            if (cuffComponent != null)
            {
                cuffComponent.hasReleased = true;

            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
