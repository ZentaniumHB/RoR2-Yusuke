using EntityStates;
using RoR2;
using System;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp
{
    internal class BeginMazokuTransformation : BaseSkillState
    {
        public static float baseDuration = 1.25f;
        
        private float duration = 12f;
        private float startUpDuration = 8.2f;
        private float animationTimer;
        private bool hasSpawnedExplosionEffect;

        private GameObject mazokuTransformPrefab;
        private GameObject mazokuTransformExplosionPrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            SetUpEffects();
            // if the transform count is greater than 2 (meaning raizen has passed) then do the other animation
            PlayAnimation("FullBody, Override", "MazokuTransformRaizen", "ThrowBomb.playbackRate", duration);
            EffectManager.SpawnEffect(mazokuTransformPrefab, new EffectData
            {
                origin = FindModelChild("mainPosition").position,
                scale = 1f
            }, transmit: true);
        }

        private void SetUpEffects()
        {
            mazokuTransformPrefab = YusukeAssets.mazokuTransformationRaizenStartupEffect;
            mazokuTransformExplosionPrefab = YusukeAssets.maokuTansformationExplosionEffect;

            mazokuTransformPrefab.AddComponent<DestroyOnTimer>().duration = startUpDuration;
            mazokuTransformExplosionPrefab.AddComponent<DestroyOnTimer>().duration = duration - startUpDuration;

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            animationTimer += GetDeltaTime();
            Log.Info("Animation timer: " + animationTimer);

            if (animationTimer > startUpDuration)
            {
                PlayExplosion();
            }
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextState(SkillSwitch(1));
            }

        }

        private void PlayExplosion()
        {
            if (!hasSpawnedExplosionEffect)
            {
                hasSpawnedExplosionEffect = true;
                EffectManager.SpawnEffect(mazokuTransformExplosionPrefab, new EffectData
                {
                    origin = FindModelChild("mainPosition").position,
                    scale = 1f
                }, transmit: true);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
            if (maz != null)
            {
                maz.hasTransformed = true;
            }

            // AFTER switch to the other animation set (mazoku)
        }

        protected virtual EntityState SkillSwitch(int ID)
        {
            // use of an ID is needed to decide which move gets swapped in
            return new SwitchSkills
            {
                switchID = ID,
            };
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
