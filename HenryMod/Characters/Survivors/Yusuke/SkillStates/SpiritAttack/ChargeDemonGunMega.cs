using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using YusukeMod.Modules.BaseStates;
using YusukeMod.SkillStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;
using RoR2;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using static YusukeMod.Modules.BaseStates.YusukeMain;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    public class ChargeDemonGunMega : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }

        private EntityStateMachine stateMachine;
        private bool hasSlowVelocity;
        private float slowVelocityDuration;

        private float duration;

        private ShakeEmitter shakeEmitter;
        private Wave wave;
        private bool tier1Wave;
        private bool tier2Wave;

        public bool isNoLongerTransformed;
        public float penaltyTimer = 0f;

        private YusukeMain mainState;


        private GameObject spiritGunMegaChargeEffectPrefab;
        private GameObject spiritGunMegaChargeEffectObject;
        private bool hasRegularEffectSpawned;

        private GameObject spiritGunMegaChargeEffectPotentPrefab;
        private GameObject spiritGunMegaChargeEffectPotentObject;
        private bool hasMaxChargeEffectSpawned;

        private GameObject chargeWindEffectPrefab;
        private GameObject chargeWindObject;

        private GameObject mazokuSparkElectricityPrefab;
        private GameObject mazokuSparkElectricityObject;
        private bool hasShownElectricityEffect;
        private readonly string fingerTipString = "fingerTipR";

        public override void OnEnter()
        {
            base.OnEnter();

            SwitchAnimationLayer();

            spiritGunMegaChargeEffectPrefab = YusukeAssets.spiritGunMegaChargeEffect;
            spiritGunMegaChargeEffectPotentPrefab = YusukeAssets.spiritGunMegaChargePotentEffect;
            chargeWindEffectPrefab = YusukeAssets.chargeWindEffect;
            mazokuSparkElectricityPrefab = YusukeAssets.mazokuElectricChargeEffect;

            // starting value, max value and how long to it takes to reach charge limit (in seconds)
            chargeValue = 0.0f;
            duration = 0.0f;


            chargeDuration = baseChargeDuration;

            var booleanTuple = CheckYAxis();

            if (booleanTuple.Item1)
                if (booleanTuple.Item2)
                    SlowVelocity();

            // slows down ground speed whilst charging 
            if (NetworkServer.active)
            {
                characterBody.AddBuff(YusukeBuffs.spiritMegaSlowDebuff);
                characterBody.AddBuff(YusukeBuffs.spiritMegaArmourBuff);
            }

            PlayAnimation("BothHands, Override", "SpiritMegaHandPose", "ShootGun.playbackRate", 1f);

            if (chargeWindEffectPrefab != null) chargeWindObject = YusukePlugin.CreateEffectObject(chargeWindEffectPrefab, FindModelChild("mainPosition"));
            SpawnChargeEffect();


        }

        private void SpawnChargeEffect()
        {
            if (spiritGunMegaChargeEffectPrefab != null) spiritGunMegaChargeEffectObject = YusukePlugin.CreateEffectObject(spiritGunMegaChargeEffectPrefab, FindModelChild("fingerTipR"));
            if (spiritGunMegaChargeEffectPotentPrefab != null) spiritGunMegaChargeEffectPotentObject = YusukePlugin.CreateEffectObject(spiritGunMegaChargeEffectPotentPrefab, FindModelChild("fingerTipR"));
            if (mazokuSparkElectricityPrefab != null) mazokuSparkElectricityObject = YusukePlugin.CreateEffectObject(mazokuSparkElectricityPrefab, FindModelChild(fingerTipString));

            spiritGunMegaChargeEffectPotentObject.SetActive(false);
            mazokuSparkElectricityObject.SetActive(false);
            hasRegularEffectSpawned = true;
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


                    // since one of the sync layers are already active (mazoku layer), it needs to be turned of temporarily so the sync layer can be used
                    MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
                    if (maz.hasTransformed)
                    {
                        mainState.SwitchMovementAnimations((int)AnimationLayerIndex.Mazoku, false);
                    }

                }

            }

        }

        public override void OnExit()
        {
            base.OnExit();
            hasSlowVelocity = false;

            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(YusukeBuffs.spiritMegaSlowDebuff);
                characterBody.RemoveBuff(YusukeBuffs.spiritMegaArmourBuff);
            }


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!isNoLongerTransformed) CheckTransformState();   // this is used to check whether the mazoku is still avtive, if not then it will notify the penalty
            if (isNoLongerTransformed)
            {
                penaltyTimer += Time.fixedDeltaTime;
                //Log.Info("Penalty timer: " + penaltyTimer);
            }

            chargeIncrement += Time.fixedDeltaTime * 10; // takes 'chargeDuration' seconds to get to chargeLimit
            totalCharge = chargeIncrement;
            //Log.Info($"Spirit Mega charge: " + totalCharge);

            characterBody.SetAimTimer(1f);

            if (hasSlowVelocity)
            {
                slowVelocityDuration += Time.fixedDeltaTime;

                float decelerateValue = 0.4f; // 50f  // 150

                characterMotor.velocity *= decelerateValue;
                float x = characterMotor.velocity.x;
                float y = characterMotor.velocity.y;
                float z = characterMotor.velocity.z;

                //y = Mathf.MoveTowards(y, decelerateValue, accelerateValue * Time.fixedDeltaTime);

                characterMotor.velocity = new Vector3(x, y, z);


            }


            if (totalCharge >= 100)
            {
                tier1Wave = false;
                tier2Wave = true;
                
                DestroyCurrentEffect();
            }

            if(totalCharge >= 200)
            {
                if (!hasShownElectricityEffect)
                {
                    hasShownElectricityEffect = true;
                    mazokuSparkElectricityObject.SetActive(true);
                }
            }

            ChangeWave();


            if (!IsKeyDown() && isAuthority)
            {
                totalCharge = RoundTheFloat(totalCharge);
                outer.SetNextState(SpiritMega());
                Log.Info($"Total charge (rounded (mega)): " + totalCharge);

            }


        }

        private void DestroyCurrentEffect()
        {
            if (hasRegularEffectSpawned)
            {
                hasRegularEffectSpawned = false;
                EntityState.Destroy(spiritGunMegaChargeEffectObject);
                spiritGunMegaChargeEffectPotentObject.SetActive(true);
            }
        }

        private void CheckTransformState()
        {
            MazokuComponent mazokuComponent = characterBody.master.GetComponent<MazokuComponent>();
            if (!mazokuComponent.hasTransformed)
            {
                isNoLongerTransformed = true;
            }
        }

        public override void Update()
        {
            // changing the crosshair for charge state.
            base.Update();
            characterBody.SetSpreadBloom(age / chargeDuration);
            characterBody.isSprinting = false;

        }

        protected virtual bool IsKeyDown()
        {

            return IsKeyDownAuthority();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Vehicle;
        }

        private void SlowVelocity()
        {

            characterBody.SetAimTimer(2f); // facing camera direction duration
            hasSlowVelocity = true;

        }

        // checks the y axis difference, determines PauseVelocity()
        (bool, bool) CheckYAxis()
        {
            stateMachine = characterBody.GetComponent<EntityStateMachine>();
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
                    mainState = (YusukeMain)stateMachine.state;
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.MegaCharge, true);
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

            return new FireDemonGunMega
            {
                charge = totalCharge,
                tier1Wave = tier1Wave,
                tier2Wave = tier2Wave,
                penaltyTime = penaltyTimer,
                spiritGunMegaChargeEffectObject = spiritGunMegaChargeEffectObject,
                spiritGunMegaChargeEffectPotentObject = spiritGunMegaChargeEffectPotentObject,
                chargeWindObject = chargeWindObject,
                mazokuSparkElectricityObject = mazokuSparkElectricityObject

            };
        }

    }
}
