using EntityStates;
using YusukeMod.Survivors.Yusuke;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;
using System;
using YusukeMod.SkillStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{
    public class Roll : BaseSkillState
    {
        public static float duration = 0.5f;
        public static float initialSpeedCoefficient = 5f;
        public static float finalSpeedCoefficient = 2.5f;

        public static string dodgeSoundString = "HenryRoll";
        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Animator animator;
        private Vector3 previousPosition;

        private bool shouldSkip;

        private List<Type> AvoidedStates;

        private GameObject dashStartSmallEffectPrefab;
        private GameObject dashGroundedEffectPrefab;
        private GameObject dashAirEffectPrefab;

        private readonly string mainPosition = "mainPosition";

        public override void OnEnter()
        {
            base.OnEnter();
            shouldSkip = ListAndCheckAllAvoidedStates();
            if (shouldSkip)
            {
                skillLocator.utility.AddOneStock();
                outer.SetNextStateToMain();
                return;
            }
            else
            {

                dashStartSmallEffectPrefab = YusukeAssets.dashStartSmallEffect;
                dashGroundedEffectPrefab = YusukeAssets.dashGroundedEffect;
                dashAirEffectPrefab = YusukeAssets.dashAirEffect;

                animator = GetModelAnimator();
                PlayAnimation("FullBody, Override", "BufferEmpty", "Slide.playbackRate", duration);
                PlayAnimation("Gesture, Override", "BufferEmpty", "Slide.playbackRate", duration);

                if (isAuthority && inputBank && characterDirection)
                {
                    forwardDirection = (inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector).normalized;
                }

                Vector3 rhs = characterDirection ? characterDirection.forward : forwardDirection;
                Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);

                float num = Vector3.Dot(forwardDirection, rhs);
                float num2 = Vector3.Dot(forwardDirection, rhs2);

                RecalculateRollSpeed();

                if (characterMotor && characterDirection)
                {
                    characterMotor.velocity.y = 0f;
                    characterMotor.velocity = forwardDirection * rollSpeed;
                }

                Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
                previousPosition = transform.position - b;

                if (isGrounded)
                {
                    PlayAnimation("FullBody, Override", "Slide", "Slide.playbackRate", duration);
                }
                else
                {
                    PlayAnimation("FullBody, Override", "Dash", "Roll.playbackRate", duration);
                }

                AddEffect();

                EffectManager.SimpleMuzzleFlash(dashStartSmallEffectPrefab, gameObject, mainPosition, false);
                if (isGrounded) EffectManager.SimpleMuzzleFlash(dashGroundedEffectPrefab, gameObject, mainPosition, false);
                if (!isGrounded) EffectManager.SimpleMuzzleFlash(dashAirEffectPrefab, gameObject, mainPosition, false);
                
                Util.PlaySound(dodgeSoundString, gameObject);

                if (NetworkServer.active)
                {
                    characterBody.AddTimedBuff(YusukeBuffs.armorBuff, 3f * duration);
                    characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f * duration);
                }

                


            }

        }

        private void AddEffect()
        {
            
            dashStartSmallEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2;
            dashGroundedEffectPrefab.AddComponent<DestroyOnTimer>().duration = 0.5f;
            dashAirEffectPrefab.AddComponent<DestroyOnTimer>().duration = 0.5f;

        }

        private bool ListAndCheckAllAvoidedStates()
        {
            // these are the animations that should not be interrupted 
            AvoidedStates = new List<Type>
            {
                typeof(MazBackToBackStrikes),
                typeof(ChargeDemonGunMega),
                typeof(ChargeSpiritGunMega),
                typeof(FireDemonGunBarrage),
                typeof(FireDemonGunMega),
                typeof(FireSpiritBeam),
                typeof(FireSpiritMega),
                typeof(FireSpiritShotgun),
                typeof(SpiritDoubleBarrelShotgun),
                typeof(SpiritGunDouble),
                typeof(Shoot)
            };


            EntityState state = EntityStateMachine.FindByCustomName(gameObject, "Weapon").state;
            foreach (Type s in AvoidedStates) 
            {
                if (state.GetType() == s)
                {
                    characterDirection.forward = GetAimRay().direction;
                    characterDirection.moveVector = GetAimRay().direction;

                    Log.Info("cannot activate... ");
                    // means you cannot activate the roll when the skill is active
                    return true;
                }

            }
            return false;
           
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (shouldSkip) 
            {
                characterDirection.forward = GetAimRay().direction;
                characterDirection.moveVector = GetAimRay().direction;

                Log.Info("Roll is unavilable during move. ");
                outer.SetNextStateToMain();
                return;
            }
                

            RecalculateRollSpeed();

            if (characterDirection) characterDirection.forward = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);

            Vector3 normalized = (transform.position - previousPosition).normalized;
            if (characterMotor && characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * rollSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                vector = forwardDirection * d;
                vector.y = 0f;

                characterMotor.velocity = vector;
            }
            previousPosition = transform.position;

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();

            characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }
    }
}