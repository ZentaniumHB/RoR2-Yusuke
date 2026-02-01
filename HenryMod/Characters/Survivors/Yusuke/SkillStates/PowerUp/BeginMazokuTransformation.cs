using EntityStates;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
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

        private AimAnimator aimAnim;
        private Transform modelTransform;
        HealthComponent yusukeHealth;
        private PitchYawControl pitchYawControl;

        public BlastAttack blastAttack;

        public override void OnEnter()
        {
            base.OnEnter();
            SetUpEffects();
            // if the transform count is greater than 2 (meaning raizen has passed) then do the other animation
            PlayAnimation("FullBody, Override", "MazokuTransformRaizen", "ThrowBomb.playbackRate", duration);

            yusukeHealth = characterBody.GetComponent<HealthComponent>();
            if (NetworkServer.active)
            {
                if (yusukeHealth)
                {
                    yusukeHealth.godMode = true;
                }

            }

            if (characterMotor)
            {
                characterMotor.enabled = false;

            }


            modelTransform = GetModelTransform();
            pitchYawControl = new PitchYawControl();
            pitchYawControl.ChangePitchAndYawRange(true, modelTransform, aimAnim);
            //ChangePitchAndYawRange(true);

            EffectManager.SpawnEffect(mazokuTransformPrefab, new EffectData
            {
                origin = FindModelChild("mainPosition").position,
                scale = 1f
            }, transmit: true);

        }


        private void CreateBlastAttack()
        {
            blastAttack = new BlastAttack();
            blastAttack.damageType = DamageType.Generic;
            blastAttack.attacker = base.gameObject;
            blastAttack.inflictor = base.gameObject;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
            blastAttack.baseDamage = damageStat * YusukeStaticValues.transformationExplosionDamageCoefficient;
            blastAttack.procCoefficient = 1.0f;
            blastAttack.radius = 100f;
            blastAttack.position = transform.position;
            blastAttack.bonusForce = Vector3.up;
            blastAttack.baseForce = 14000f;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            blastAttack.crit = base.RollCrit();

            blastAttack.Fire();

        }

        private void SetUpEffects()
        {
            mazokuTransformPrefab = YusukeAssets.mazokuTransformationRaizenStartupEffect;
            mazokuTransformExplosionPrefab = YusukeAssets.maokuTansformationExplosionEffect;

            mazokuTransformPrefab.AddComponent<DestroyOnTimer>().duration = startUpDuration;
            mazokuTransformExplosionPrefab.AddComponent<DestroyOnTimer>().duration = duration - startUpDuration;

        }

        // this will temporarily change the pitch and yaw for the transformation cutscene, so the aim yaw won't affect the model when playing the animation
       

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

                CreateBlastAttack();

            }
        }

        public override void OnExit()
        {
            base.OnExit();

            pitchYawControl.ChangePitchAndYawRange(false, modelTransform, aimAnim);
            //ChangePitchAndYawRange(false);

            MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
            if (maz != null)
            {
                maz.hasTransformed = true;
            }

            if (characterMotor)
            {
                characterMotor.enabled = true;

            }

            if (NetworkServer.active)
            {
                if (yusukeHealth)
                {
                    yusukeHealth.godMode = false;
                    characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 1f * duration);
                }

            }

            // AFTER switch to the other animation set (mazoku)
        }

        protected virtual EntityState SkillSwitch(int ID)
        {
            // use of an ID is needed to decide which move gets swapped in
            return new SwitchSkills
            {
                switchID = (int)SwitchSkills.SwitchSkillIndex.MazokuSwitch,
            };
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
