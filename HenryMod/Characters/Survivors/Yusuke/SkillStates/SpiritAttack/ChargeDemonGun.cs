using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.SkillStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    public class ChargeDemonGun : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }
        public bool isMaxCharge;

        public virtual int attackID { get; set; } = 2;

        private bool hasIconSwitch;
        public int bullets;


        public override void OnEnter()
        {
            base.OnEnter();

            Log.Info("attack ID: " + attackID);
            cuffComponent = characterBody.GetComponent<SpiritCuffComponent>();
            // starting value, max value and how fast to increment
            chargeValue = 0.0f;
            chargeLimit = 100.0f;

            if (cuffComponent)
            {
                if (cuffComponent.hasReleased)
                {
                    baseChargeDuration = 3.0f;
                }
                else
                {
                    baseChargeDuration = 5.0f;
                }
            }


            chargeDuration = baseChargeDuration;

        }

        public override void OnExit()
        {
            base.OnExit();
            if (isMaxCharge || cuffComponent.hasReleased) RevertIconSwitch(2);


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            chargeIncrement = chargeLimit / baseChargeDuration * Time.fixedDeltaTime; // takes 'chargeDuration' seconds to get to chargeLimit
            if (!isMaxCharge) chargeValue += chargeIncrement;
            //totalCharge = Mathf.Lerp(0.0f, chargeLimit, fixedAge / baseChargeDuration);
            totalCharge = Mathf.Clamp(chargeValue, 0.0f, chargeLimit);
            //if (!isMaxCharge) Log.Info($"Spirit gun charge: " + totalCharge);


            if (fixedAge >= chargeDuration)
            {
                if (!isMaxCharge)
                {
                    Chat.AddMessage("Max charge.");
                    Log.Info($"Total charge (regular): " + totalCharge);
                    isMaxCharge = true;
                }
            }

            if (isMaxCharge)
            {
                characterBody.isSprinting = false;
                base.characterBody.SetAimTimer(1f);

                if (!hasIconSwitch)
                {
                    hasIconSwitch = true;

                    /* checks whether the cuff state is released and changes the icon accordingly 
                     * This is mainly done for visual purposes. SO the player knows what type of spirit gun they are doing
                    */

                    if (cuffComponent)
                    {
                        if (cuffComponent.hasReleased)
                        {
                            //IconSwitch(true, attackID);
                        }
                        else
                        {
                            //IconSwitch(false, attackID);
                        }
                    }

                }


            }

            if (!IsKeyDown() && isAuthority)
            {
                totalCharge = RoundTheFloat(totalCharge);

                int bulletCount = (int)Mathf.Floor(totalCharge / 10);
                Log.Info($"Total charge (rounded (regular)): " + totalCharge);
                Log.Info($"total bullets:" + bulletCount);
                bullets = bulletCount;

                outer.SetNextState(SpiritNextState());

            }


        }

        public override void Update()
        {
            // changing the crosshair for charge state.
            base.Update();
            characterBody.SetSpreadBloom(age / chargeDuration);

        }

        protected virtual bool IsKeyDown()
        {
            return IsKeyDownAuthority();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        protected virtual EntityState SpiritNextState()
        {
            if(bullets == 0) bullets = 1;   // just so a bullet is shot when a player just taps the skill
            return new FireDemonGunBarrage
            {
                charge = totalCharge,
                totalBullets = bullets,
            };
        }


    }
}
