using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine.Networking;
using YusukeMod.Survivors.Yusuke.Components;
using UnityEngine;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Characters.Survivors.Yusuke.Extra;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp
{
    internal class MazokuResurrect : BaseSkillState
    {

        private float duration = 7f;
        private float animationTimer;
        private float startExplosion = 4.6f;
        private bool hasCreatedExplosionBlast;

        YusukeWeaponComponent yusukeWeapon;
        HealthComponent yusukeHealth;

        private GameObject mazokuResurrectExplosionPrefab;
        private GameObject mazokuResurrectWindPrefab;

        private AimAnimator aimAnim;
        private Transform modelTransform;
        private PitchYawControl pitchYawControl;

        public BlastAttack blastAttack;

        public override void OnEnter()
        {
            base.OnEnter();

            yusukeWeapon = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeHealth = characterBody.GetComponent<HealthComponent>();

            modelTransform = GetModelTransform();

            SetUpEffects();

            EffectManager.SpawnEffect(mazokuResurrectExplosionPrefab, new EffectData
            {
                origin = FindModelChild("Base").position,
                scale = 1f
            }, transmit: true);

            EffectManager.SpawnEffect(mazokuResurrectWindPrefab, new EffectData
            {
                origin = FindModelChild("mainPosition").position,
                scale = 1f
            }, transmit: true);

            pitchYawControl = gameObject.GetComponent<PitchYawControl>();
            pitchYawControl.ChangePitchAndYawRange(true, modelTransform, aimAnim);

            PlayAnimation("FullBody, Override", "MazokuResurrect", "Roll.playbackRate", duration);

        }

        private void SetUpEffects()
        {
            mazokuResurrectExplosionPrefab = YusukeAssets.mazokuResurrectExplosion;
            mazokuResurrectWindPrefab = YusukeAssets.mazokuResurrectWind;

            mazokuResurrectExplosionPrefab.AddComponent<DestroyOnTimer>().duration = duration;
            mazokuResurrectWindPrefab.AddComponent<DestroyOnTimer>().duration = duration;
        }

        private void CreateBlastAttack()
        {
            if (!hasCreatedExplosionBlast)
            {
                hasCreatedExplosionBlast = true;
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

        }

        // this will temporarily change the pitch and yaw for the transformation cutscene, so the aim yaw won't affect the model when playing the animation


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            animationTimer += GetDeltaTime();
            characterMotor.velocity = new Vector3(0, 0, 0);
            if(animationTimer > startExplosion)
            {
                CreateBlastAttack();
            }
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }

        }

        public override void OnExit()
        {

            pitchYawControl.ChangePitchAndYawRange(false, modelTransform, aimAnim);

            if (yusukeHealth) yusukeHealth.health = yusukeHealth.fullHealth;

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

            if (yusukeWeapon) yusukeWeapon.SetKnockedBoolean(false);
            

            base.OnExit();

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
