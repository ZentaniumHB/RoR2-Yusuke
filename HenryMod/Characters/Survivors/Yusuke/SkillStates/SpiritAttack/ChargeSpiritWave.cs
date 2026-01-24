using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static YusukeMod.Modules.BaseStates.YusukeMain;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.SkillStates
{
    public class ChargeSpiritWave : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }
        private bool isMaxCharge;
        private YusukeMain mainState;


        private GameObject spiritWaveChargeEffectPrefab;
        private GameObject spiritWaveChargeEffectObject;
        private bool hasRegularEffectSpawned;

        private GameObject spiritWaveChargeEffectPotentPrefab;
        private GameObject spiritWaveChargeEffectPotentObject;
        private bool hasMaxChargeEffectSpawned;

        public override void OnEnter()
        {
            base.OnEnter();

            spiritWaveChargeEffectPrefab = YusukeAssets.spiritWaveChargeEffect;
            spiritWaveChargeEffectPotentPrefab = YusukeAssets.spiritWaveChargePotentEffect;

            SwitchAnimationLayer();

            // starting value, max value and how fast to increment
            chargeValue = 0.0f;
            chargeLimit = 100.0f;
            baseChargeDuration = 5.0f;

            chargeDuration = baseChargeDuration;

            SpawnChargeEffect(false);

        }

        private void SpawnChargeEffect(bool isMaxCharge)
        {
            if (!isMaxCharge)
            {
                hasRegularEffectSpawned = true;
                if (spiritWaveChargeEffectPrefab != null) spiritWaveChargeEffectObject = YusukePlugin.CreateEffectObject(spiritWaveChargeEffectPrefab, FindModelChild("mainPosition"));
            }
            else
            {
                hasMaxChargeEffectSpawned = true;
                if (spiritWaveChargeEffectPotentPrefab != null) spiritWaveChargeEffectPotentObject = YusukePlugin.CreateEffectObject(spiritWaveChargeEffectPotentPrefab, FindModelChild("HandR"));

            }
        }

        // switching the animation layer within unity. This will perform the spirit gun animations that is synced to the body animations instead. 
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
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.WaveCharge, true);

                }

            }

        }

        public override void OnExit()
        {
            base.OnExit();

            YusukeWeaponComponent yusukeWeaponComponent = characterBody.GetComponent<YusukeWeaponComponent>();
            if (yusukeWeaponComponent.GetKnockedBoolean())
            {
                if (spiritWaveChargeEffectObject) EntityState.Destroy(spiritWaveChargeEffectObject);
                if (spiritWaveChargeEffectPotentObject) EntityState.Destroy(spiritWaveChargeEffectPotentObject);
            }
            

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            chargeIncrement = chargeLimit / baseChargeDuration * Time.fixedDeltaTime; // takes 'chargeDuration' seconds to get to chargeLimit
            if (!isMaxCharge) chargeValue += chargeIncrement;
            //totalCharge = Mathf.Lerp(0.0f, chargeLimit, fixedAge / baseChargeDuration);
            totalCharge = Mathf.Clamp(chargeValue, 0.0f, chargeLimit);
            if (!isMaxCharge) Log.Info($"Spirit wave charge: " + totalCharge);


            if (fixedAge >= chargeDuration)
            {
                if (!isMaxCharge)
                {
                    Chat.AddMessage("Max charge.");
                    Log.Info($"Total charge (regular): " +totalCharge);
                    isMaxCharge = true;
                }
            }

            if (isMaxCharge)
            {
                characterBody.isSprinting = false;
                base.characterBody.SetAimTimer(1f);

                DestroyCurrentEffect();
                if (!hasMaxChargeEffectSpawned) SpawnChargeEffect(true);
            }

            if (!IsKeyDown() && isAuthority)
            {
                totalCharge = RoundTheFloat(totalCharge);
                outer.SetNextState(SpiritWaveState());

                Log.Info($"Total charge (rounded (regular)): " + totalCharge);
            }


        }

        private void DestroyCurrentEffect()
        {
            if (hasRegularEffectSpawned)
            {
                hasRegularEffectSpawned = false;
                Log.Info("Removing effect:");
                EntityState.Destroy(spiritWaveChargeEffectObject);
            }
        }

        public override void Update()
        {
            // changing the crosshair for charge state.
            base.Update();
            characterBody.SetSpreadBloom(age / chargeDuration);

        }

        protected virtual bool IsKeyDown() {

            return IsKeyDownAuthority();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }


        protected virtual EntityState SpiritWaveState()
        {
            return new SpiritWave2
            {
                charge = totalCharge,
                isMaxCharge = isMaxCharge,
                spiritWaveChargeEffectObject = spiritWaveChargeEffectObject,
                spiritWaveEffectPotentObject = spiritWaveChargeEffectPotentObject
            };
        }



    }
}
