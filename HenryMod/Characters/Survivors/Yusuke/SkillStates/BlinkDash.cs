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
using YusukeMod.Survivors.Yusuke.Components;
using static YusukeMod.Modules.BaseStates.YusukeMain;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates
{
    public class BlinkDash : BaseSkillState
    {

        private float dashSpeedMultiplier = 6f;

        private float duration = 0.4f;

        private float dashStopWatch;

        private float armourBuffDuration = 3f;

        private float hiddenInvincibilityDuration = 0.5f;

        private CharacterModel characterModel;

        private HurtBoxGroup hurtboxGroup;

        private Transform modelTransform;

        private GameObject modelFace;

        private Vector3 aimVector;

        private YusukeWeaponComponent yusukeWeaponComponent;
        private YusukeMain mainState;
        
        private GameObject dashStartSmallEffectPrefab;
        private GameObject vanishLinesWhitePrefab;
        private bool hasSpawnedEffects;
        private readonly string mainPosition = "mainPosition";
        private readonly string chestLocation = "Chest";

        private bool shouldSkip;
        private List<Type> AvoidedStates;

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
                vanishLinesWhitePrefab = YusukeAssets.vanishLinesWhite;
                yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();

                modelTransform = GetModelTransform();
                modelFace = FindModelChild("ModelFace").gameObject;
                if ((bool)modelTransform)
                {
                    characterModel = modelTransform.GetComponent<CharacterModel>();
                    hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
                }

                if ((bool)characterModel)
                {
                    characterModel.invisibilityCount++;
                    modelFace.SetActive(false);
                }
                aimVector = inputBank.aimDirection;
                characterDirection.forward = GetAimRay().direction;
                characterMotor.Motor.ForceUnground();
                EditEffects();

                PlayAnimation("FullBody, Override", "DashAirLoop", "Slide.playbackRate", duration);
                EffectManager.SimpleMuzzleFlash(dashStartSmallEffectPrefab, gameObject, mainPosition, false);

                if (NetworkServer.active)
                {
                    characterBody.AddTimedBuff(YusukeBuffs.armorBuff, armourBuffDuration * duration);
                    characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, hiddenInvincibilityDuration * duration);
                }

                yusukeWeaponComponent.ShowChargeObject(false);
            }
            
        }

        private bool ListAndCheckAllAvoidedStates()
        {
            AvoidedStates = new List<Type>
            {
                typeof(ChargeDemonGunMega),
                typeof(ChargeSpiritGunMega),
                typeof(FireDemonGunBarrage),
                typeof(FireDemonGunMega),
                typeof(FireSpiritMega),
                typeof(FireSpiritBeam)
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

            AvoidedStates = new List<Type>
            {
                typeof(MazBackToBackStrikes),
                typeof(FireDemonGunBarrage),
                typeof(FireDemonGunMega),

            };

            state = EntityStateMachine.FindByCustomName(gameObject, "MazokuWeapon").state;
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

        private void EditEffects()
        {
            dashStartSmallEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;
            vanishLinesWhitePrefab.AddComponent<DestroyOnTimer>().duration = 1;
            EffectComponent component = vanishLinesWhitePrefab.GetComponent<EffectComponent>();
            if (component)
            {
                component.parentToReferencedTransform = false;

            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (!shouldSkip)
            {
                PlayAnimation("FullBody, Override", "BufferEmpty", "Slide.playbackRate", duration);

                EffectComponent component = vanishLinesWhitePrefab.GetComponent<EffectComponent>();
                if (component)
                {
                    component.parentToReferencedTransform = true;
                    EffectManager.SimpleMuzzleFlash(vanishLinesWhitePrefab, gameObject, chestLocation, false);
                    component.parentToReferencedTransform = false;
                }


                modelTransform = GetModelTransform();
                modelFace = FindModelChild("ModelFace").gameObject;
                if ((bool)modelTransform)
                {

                }

                if ((bool)characterModel)
                {
                    characterModel.invisibilityCount--;
                    modelFace.SetActive(true);
                }

                yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
                yusukeWeaponComponent.ShowChargeObject(true);
                SwitchAnimationLayer();
            }
            
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            dashStopWatch += GetDeltaTime();

            if (!shouldSkip)
            {
                if (!hasSpawnedEffects)
                {
                    hasSpawnedEffects = true;
                    EffectManager.SimpleMuzzleFlash(vanishLinesWhitePrefab, gameObject, chestLocation, false);

                }

                if (characterMotor && characterDirection)
                {
                    //characterMotor.velocity = aimVector * (moveSpeedStat * dashSpeedMultiplier);
                    characterMotor.velocity = Vector3.zero;
                    characterMotor.rootMotion += aimVector * (moveSpeedStat * dashSpeedMultiplier * GetDeltaTime());
                }

                if (dashStopWatch >= duration && isAuthority)
                {
                    outer.SetNextStateToMain();
                }
            }


        }


        private void SwitchAnimationLayer()
        {
            EntityStateMachine stateMachine = characterBody.GetComponent<EntityStateMachine>();
            if (stateMachine == null)
            {
                Log.Error("No State machine found");
            }
            else
            {
                Type currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    mainState = (YusukeMain)stateMachine.state;
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.GunCharge, false);
                    // make the ReleaseAnimation index true
                }

            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }


    }
}
