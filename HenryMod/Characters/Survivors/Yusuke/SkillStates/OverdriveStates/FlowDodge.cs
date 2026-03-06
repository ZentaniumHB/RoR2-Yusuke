using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using EntityStates;
using static YusukeMod.Characters.Survivors.Yusuke.Components.SacredComponent;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Survivors.Yusuke.Components;
using YusukeMod.Survivors.Yusuke;
using Random = UnityEngine.Random;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    public class FlowDodge : BaseSkillState
    {

        private float duration = 0.1f;
        private float FlowDodgeDuration;
        private float FlowDodgeFullDuration = 0.4f;

        private Vector3 aimDirection;
        private Transform modelTransform;
        private PitchYawControl pitchYawControl;
        private YusukeWeaponComponent yusukeWeaponComponent;

        private AimAnimator aimAnim;
        private HealthComponent yusukeHealth;

        private bool shouldSkip;


        private string dodgeString = "FlowDodge";
        private bool hasPlayedImageEffect;
        private TemporaryOverlay temporaryOverlay;
        private FlowImageMeshTrail flowImageMeshTrail;

        private GameObject flowImagePrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            Log.Info("On enter in flow dodge");

            yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            flowImageMeshTrail = characterBody.gameObject.GetComponent<FlowImageMeshTrail>();

            shouldSkip = CheckForStates();
            if (shouldSkip)
            {
                Log.Info("Skipped");
                outer.SetNextStateToMain();
                return;
            }
            else
            {
                flowImagePrefab = YusukeAssets.flowObjEffect;
                characterBody.GetComponent<FlowImageMeshTrail>().enabled = true;

                SetUpEffects();

                PlayTheDodgeAnimation();

                //Util.PlaySound("Play_VoiceOverdriveWave", gameObject);
                aimDirection = GetAimRay().direction;


                modelTransform = GetModelTransform();
                pitchYawControl = gameObject.GetComponent<PitchYawControl>();
                pitchYawControl.ChangePitchAndYawRange(true, modelTransform, aimAnim);

                
                yusukeWeaponComponent.SetDodgeState(true);

                yusukeHealth = characterBody.GetComponent<HealthComponent>();
                if (characterBody && NetworkServer.active)
                {
                    if (yusukeHealth)
                    {
                        CleanseSystem.CleanseBodyServer(characterBody, true, false, true, true, true, true);
                        characterBody.AddBuff(YusukeBuffs.overdriveSlowBuff);
                        yusukeHealth.godMode = true;
                    }

                }
            }
            

        }

        private void MoveVelocity()
        {
            if (characterMotor)
            {
                Vector3 move = inputBank.moveVector;
                characterMotor.moveDirection = move;
            }

            // since its being called within the main state, the aim direciton needs to be called
            if (characterDirection && inputBank)
            {
                characterDirection.forward = inputBank.aimDirection;
            }

        }

        private bool CheckForStates()
        {
            EntityState state = EntityStateMachine.FindByCustomName(gameObject, "Weapon").state;
            string stateName = state.GetType().Name.ToString();

            Log.Info("The current state name found in flow" + stateName);
            if (state.GetType() == null || state.GetType() == typeof(Idle))
            {
                return false;
            }

            state = EntityStateMachine.FindByCustomName(gameObject, "Body").state;
            if (state.GetType() == null)
            {
                return false;
            }

            state = EntityStateMachine.FindByCustomName(gameObject, "Weapon2").state;
            if (state.GetType() == null)
            {
                return false;
            }

            return true;

        }

        private void PlayTheDodgeAnimation()
        {

            byte value = (byte)Random.Range(1, 4);
            byte valueAir = (byte)Random.Range(1, 3);

            if (yusukeWeaponComponent.GetDodgeDirection())
            {
                dodgeString += "Right";
            }
            else
            {
                dodgeString += "Left";
            }

            if (!isGrounded)
            {
                if(value == 3)
                {
                    dodgeString += value;
                }
                else
                {
                    dodgeString += valueAir + "Air";
                }
            }
            else
            {
                dodgeString += value;
            }

            Log.Info("Dodge animation played: " + dodgeString);

            PlayAnimation("FullBody, Override", dodgeString, "Slide.playbackRate", duration);

        }

        private void SetUpEffects()
        {
            // the flow image effect will need to have the hair prefab currently active. 
            GameObject hairReference = new GameObject();
            Transform activeHair = FindModelChild("HairLocation");
            Transform hairTransform = new Transform();

            // looping through the hairs to check which one is enabled (should be one). 
            foreach (Transform child in activeHair)
            {
                if (child.gameObject.activeSelf)
                {
                    hairTransform = child.gameObject.transform;
                    hairReference = child.gameObject;
                }
            }

            // if the effects are already instantiated, then just use the effectManagerVersion
            if ((bool)transform && (bool)flowImagePrefab && (bool)flowImageMeshTrail)
            {
                if (!EffectManager.ShouldUsePooledEffect(flowImagePrefab))
                {
                    flowImageMeshTrail.SetFlowImagePrefab(YusukePlugin.CreateEffectObject(flowImagePrefab, FindModelChild("mainPosition")));
                }
                else
                {
                    flowImageMeshTrail.SetFlowImageEMH(EffectManager.GetAndActivatePooledEffect(flowImagePrefab, transform.position, transform.rotation));
                    
                }

                if (hairReference)
                {
                    if (!EffectManager.ShouldUsePooledEffect(hairReference))
                    {
                        flowImageMeshTrail.SetHairImagePrefab(YusukePlugin.CreateEffectObject(hairReference, hairTransform), hairTransform);
                    }
                    else
                    {
                        flowImageMeshTrail.SetHairImageEMH(EffectManager.GetAndActivatePooledEffect(hairReference, hairTransform.position, hairTransform.rotation), hairTransform);

                    }
                }
                
            }
            else
            {
                Log.Warning("No flowImageMeshTrail found");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (shouldSkip)
            {
                outer.SetNextStateToMain();
                return;
            }
            else
            {
                FlowDodgeDuration += GetDeltaTime();
                if(characterBody) characterBody.SetAimTimer(0.5f);
                MoveVelocity();
                PlayImageEffect();

                if (isAuthority && fixedAge >= duration)
                {
                    if (FlowDodgeDuration > FlowDodgeFullDuration)
                    {
                        outer.SetNextStateToMain();
                    }
                }
            }
            

        }

        // places the overlay over the character (flow lightting effect).
        private void PlayImageEffect()
        {
            if (!hasPlayedImageEffect)
            {
                hasPlayedImageEffect = true;
                // gives them a very pale blue effect around the body
                Transform modelTransform = GetModelTransform();
                if ((bool)modelTransform)
                {
                    temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.duration = FlowDodgeFullDuration;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
                    temporaryOverlay.destroyComponentOnEnd = false;
                    temporaryOverlay.originalMaterial = YusukeAssets.flowMaterial; 
                    temporaryOverlay.enabled = true;
                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());

                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            yusukeWeaponComponent.SetDodgeState(false);
            if (!shouldSkip)
            {
                characterBody.GetComponent<FlowImageMeshTrail>().enabled = false;

                pitchYawControl.ChangePitchAndYawRange(false, modelTransform, aimAnim);
                yusukeWeaponComponent.SwapDodgeDirection();
                if (NetworkServer.active)
                {
                    if (yusukeHealth)
                    {
                        yusukeHealth.godMode = false;
                        characterBody.RemoveBuff(YusukeBuffs.overdriveSlowBuff);
                        characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 1f * duration);

                    }

                }

                if ((bool)modelTransform)
                {
                    temporaryOverlay.RemoveFromCharacterModel();
                }
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
