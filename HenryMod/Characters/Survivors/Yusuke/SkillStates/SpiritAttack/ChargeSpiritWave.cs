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

namespace YusukeMod.SkillStates
{
    public class ChargeSpiritWave : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }
        private bool isMaxCharge;
        private YusukeMain mainState;

        public override void OnEnter()
        {
            base.OnEnter();

            SwitchAnimationLayer();

            // starting value, max value and how fast to increment
            chargeValue = 0.0f;
            chargeLimit = 100.0f;
            baseChargeDuration = 5.0f;

            chargeDuration = baseChargeDuration;

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
            }

            if (!IsKeyDown() && isAuthority)
            {
                totalCharge = RoundTheFloat(totalCharge);
                outer.SetNextState(SpiritWaveState());

                Log.Info($"Total charge (rounded (regular)): " + totalCharge);
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
            return InterruptPriority.Stun;
        }


        protected virtual EntityState SpiritWaveState()
        {
            return new SpiritWave
            {
                charge = totalCharge,
                isMaxCharge = isMaxCharge
            };
        }



    }
}
