using EntityStates;
using EntityStates.SurvivorPod;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.Networking;
using YusukeMod.Modules.BaseStates;
using YusukeMod.SkillStates;
using static YusukeMod.Modules.BaseStates.YusukeMain;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{

    public class ChargeSpiritGunMega : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }
        private bool isMaxCharge;

        private EntityStateMachine stateMachine;
        private bool hasSlowVelocity;
        private float slowVelocityDuration;

        private float duration;

        private ShakeEmitter shakeEmitter;
        private Wave wave;
        private bool tier1Wave;
        private bool tier2Wave;

        private YusukeMain mainState;

        private GameObject spiritGunMegaChargeEffectPrefab;
        private GameObject spiritGunMegaChargeEffectObject;
        private bool hasRegularEffectSpawned;

        private GameObject spiritGunMegaChargeEffectPotentPrefab;
        private GameObject spiritGunMegaChargeEffectPotentObject;
        private bool hasMaxChargeEffectSpawned;


        public override void OnEnter()
        {
            
            spiritGunMegaChargeEffectPrefab = YusukeAssets.spiritGunMegaChargeEffect;
            spiritGunMegaChargeEffectPotentPrefab = YusukeAssets.spiritGunMegaChargePotentEffect;

            SwitchAnimationLayer();

            // starting value, max value and how long to it takes to reach charge limit (in seconds)
            chargeValue = 0.0f;
            chargeLimit = 100.0f;
            baseChargeDuration = 6.0f;
            duration = 0.0f;

            
            chargeDuration = baseChargeDuration;

            var booleanTuple = CheckYAxis();

            if (booleanTuple.Item1)
                if (booleanTuple.Item2)
                    SlowVelocity();

            // slows down ground speed whilst charging 
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(YusukeBuffs.spiritMegaSlowDebuff);
                base.characterBody.AddBuff(YusukeBuffs.spiritMegaArmourBuff);
            }

            PlayAnimation("BothHands, Override", "SpiritMegaHandPose", "ShootGun.playbackRate", 1f);
            SpawnChargeEffect(false);

            base.OnEnter();
        }

        private void SpawnChargeEffect(bool isMaxCharge) 
        {
            if (!isMaxCharge)
            {
                hasRegularEffectSpawned = true;
                if (spiritGunMegaChargeEffectPrefab == null) Log.Info("Mega effect does not exist........");
                if (spiritGunMegaChargeEffectPrefab != null) spiritGunMegaChargeEffectObject = YusukePlugin.CreateEffectObject(spiritGunMegaChargeEffectPrefab, FindModelChild("fingerTipR"));
            }
            else
            {
                hasMaxChargeEffectSpawned = true;
                if (spiritGunMegaChargeEffectPotentPrefab != null) spiritGunMegaChargeEffectPotentObject = YusukePlugin.CreateEffectObject(spiritGunMegaChargeEffectPotentPrefab, FindModelChild("fingerTipR"));

            }
        }

        // the animation switching is done once the YusukeMain state is taken
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
                    // goes through the animation layers and switches them within the main state.
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.MegaCharge, true);

                }

            }

        }

        public override void OnExit()
        {
            base.OnExit();
            hasSlowVelocity = false;
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(YusukeBuffs.spiritMegaSlowDebuff);
                base.characterBody.RemoveBuff(YusukeBuffs.spiritMegaArmourBuff);
            }


        }

        public override void FixedUpdate()
        {

            base.FixedUpdate();

            chargeIncrement = chargeLimit / baseChargeDuration * Time.fixedDeltaTime; // takes 'chargeDuration' seconds to get to chargeLimit
            if (!isMaxCharge) chargeValue += chargeIncrement;
            //totalCharge = Mathf.Lerp(0.0f, chargeLimit, fixedAge / baseChargeDuration);
            totalCharge = Mathf.Clamp(chargeValue, 0.0f, chargeLimit);
            //if (!isMaxCharge) //Log.Info($"Spirit Mega charge: " + totalCharge);

            base.characterBody.SetAimTimer(1f);

            if (hasSlowVelocity)
            {
                slowVelocityDuration += Time.fixedDeltaTime;

                float decelerateValue = 0.4f; // 50f  // 150

                base.characterMotor.velocity *= decelerateValue;
                float x = base.characterMotor.velocity.x;
                float y = base.characterMotor.velocity.y;
                float z = base.characterMotor.velocity.z;

                //y = Mathf.MoveTowards(y, decelerateValue, accelerateValue * Time.fixedDeltaTime);

                base.characterMotor.velocity = new Vector3(x, y, z);
                
                
            }


            if (totalCharge >= chargeLimit)
            {
                if (!isMaxCharge)
                {
                    Chat.AddMessage("Max charge.");
                    Log.Info($"Total charge (mega): " + totalCharge);
                    isMaxCharge = true;
                    tier1Wave = false;
                    tier2Wave = true;
                }

            }

            ChangeWave();

            if (isMaxCharge)
            {
                if (!hasMaxChargeEffectSpawned) SpawnChargeEffect(true);
                DestroyCurrentEffect();
            }

            if (!IsKeyDown() && isAuthority)
            {
                totalCharge = RoundTheFloat(totalCharge);
                outer.SetNextState(SpiritMega());
                Log.Info($"Total charge (rounded (mega)): " + totalCharge);

            }


        }

        // deleting the first effect so the second effect can be created and shown
        private void DestroyCurrentEffect()
        {
            
            if (hasRegularEffectSpawned)
            {
                hasRegularEffectSpawned = false;
                Log.Info("Removing effect:");
                EntityState.Destroy(spiritGunMegaChargeEffectObject);
            }
        }

        public override void Update()
        {
            // changing the crosshair for charge state.
            base.Update();
            characterBody.SetSpreadBloom(age / chargeDuration);
            characterBody.isSprinting = false;

        }

        protected virtual bool IsKeyDown() {

            return IsKeyDownAuthority();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        private void SlowVelocity()
        {

            base.characterBody.SetAimTimer(2f); // facing camera direction duration
            hasSlowVelocity = true;

        }

        // checks the y axis difference, determines PauseVelocity()
        (bool, bool) CheckYAxis()
        {
            stateMachine = base.characterBody.GetComponent<EntityStateMachine>();
            if (stateMachine == null)
            {
                Log.Error("No State machine found");
                return (false, false);
            }
            else
            {
                Type currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    YusukeMain targetState = (YusukeMain)stateMachine.state;
                    //Chat.AddMessage("Result: " + targetState.CompareYAxis());
                    return (true, targetState.CompareYAxis());
                }
                else
                {
                    Log.Error("This is not the YusukeMain state.");
                    return (false, false);
                }
                
                    
            }
        }

        // altering waves for ShakeEmitter
        private void ChangeWave()
        {
            if (totalCharge > 50.0 && totalCharge != 100.0f)
            {
                tier1Wave = true;
                
            }

            if (tier1Wave)
            {
                wave = new Wave
                {
                    amplitude = 0.5f,
                    frequency = 30f,
                    cycleOffset = 0f
                };
                RoR2.ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 0.1f, 20f, true);
            }

            if (tier2Wave)
            {
                wave = new Wave
                {
                    amplitude = 0.8f,
                    frequency = 31f,
                    cycleOffset = 0f
                };
                RoR2.ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 0.1f, 20f, true);
            }
        }


        protected virtual EntityState SpiritMega()
        {

            return new FireSpiritMega
            {
                charge = totalCharge,
                isMaxCharge = isMaxCharge,
                tier1Wave = tier1Wave,
                tier2Wave = tier2Wave,
                spiritGunMegaChargeEffectObject = spiritGunMegaChargeEffectObject,
                spiritGunMegaChargeEffectPotentObject = spiritGunMegaChargeEffectPotentObject
                

            };
        }



    }


}
