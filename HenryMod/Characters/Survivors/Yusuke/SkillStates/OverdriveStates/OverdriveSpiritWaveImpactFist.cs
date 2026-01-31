using EntityStates;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
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

        

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("FullBody, Override", "OverdriveSpiritWaveImpactFistBegin", "Slide.playbackRate", duration);
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



        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            overdriveTimeDuration += GetDeltaTime();
            Log.Info("overdrivetime: " + overdriveTimeDuration);

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
            }
            

        }

        public override void OnExit()
        {
            base.OnExit();
 

            if (characterDirection)
            {
                characterDirection.enabled = true;
            }

            if (characterMotor)
            {
                characterMotor.enabled = true;

            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
