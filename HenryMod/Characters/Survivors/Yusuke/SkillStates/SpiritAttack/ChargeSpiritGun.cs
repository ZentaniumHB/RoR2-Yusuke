using EntityStates;
using EntityStates.Captain.Weapon;
using RoR2;
using RoR2.UI;
using UnityEngine;
using YusukeMod;
using YusukeMod.SkillStates;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{

    public class ChargeSpiritGun : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }
        private bool isMaxCharge;

        public override void OnEnter()
        {
            base.OnEnter();

            // starting value, max value and how fast to increment
            chargeValue = 0.0f;
            chargeLimit = 100.0f;
            baseChargeDuration = 5.0f;

            chargeDuration = baseChargeDuration;

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
            if (!isMaxCharge) Log.Info($"Spirit gun charge: " + totalCharge);


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

                if (isMaxCharge)
                {
                    outer.SetNextState(DoubleNextState());
                }
                else
                {
                    outer.SetNextState(SpiritNextState());
                }
                
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
            return InterruptPriority.Skill;
        }

        protected virtual EntityState SpiritNextState()
        {
            return new Shoot
            {
                charge = totalCharge,
                
            };
        }

        protected virtual EntityState DoubleNextState()
        {
            return new SpiritGunDouble
            {
                charge = totalCharge,
                isMaxCharge = isMaxCharge
            };
        }



    }


}
