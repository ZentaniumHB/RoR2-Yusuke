using EntityStates;
using EntityStates.Captain.Weapon;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using YusukeMod;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;
using YusukeMod.SkillStates;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{

    public class ChargeSpiritGun : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }
        private bool isMaxCharge;
        private SpiritCuffComponent cuffComponent;
        private Sprite previousSprite;
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        private bool hasIconSwitch;


        public override void OnEnter()
        {
            base.OnEnter();


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
            if(isMaxCharge || cuffComponent.hasReleased) RevertIconSwitch();


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
                            IconSwitch(true);
                        }
                        else
                        {
                            IconSwitch(false);
                        }
                    }
                    
                }

                
            }

            if (!IsKeyDown() && isAuthority)
            {
                totalCharge = RoundTheFloat(totalCharge);

                if (isMaxCharge)
                {
                    if (cuffComponent)
                    {
                        // if spiritcuff is activated, do spirit beam
                        if (cuffComponent.hasReleased)
                        {

                            outer.SetNextState(BeamNextState());
                        }
                        else
                        {
                            // if not, do spirit gun double
                            outer.SetNextState(DoubleNextState());
                        }
                    }

                    
                }
                else
                {
                    // if neither, just do regular
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

        private void IconSwitch(bool hasReleased)
        {
            // getting the icons and changing them accordingly 
            SkillDef primary = skillLocator.primary.skillDef;
            SkillDef secondary = skillLocator.secondary.skillDef;
            Log.Info("ICON SWITCH hasReleased: "+hasReleased);
            if (skillLocator.primary.skillNameToken == prefix + "PRIMARY_GUN_NAME")
            {
                if (hasReleased)
                {
                    primary.icon = YusukeSurvivor.spiritBeamIcon;
                }
                else
                {
                    primary.icon = YusukeSurvivor.spiritGunDoubleIcon;
                }
                
                
            }
            if (skillLocator.secondary.skillNameToken == prefix + "SECONDARY_GUN_NAME")
            {
                if (hasReleased)
                {
                    secondary.icon = YusukeSurvivor.spiritBeamIcon;
                }
                else
                {
                    secondary.icon = YusukeSurvivor.spiritGunDoubleIcon;
                }
                
            }


            
        }


        private void RevertIconSwitch()
        {
            // getting the icons and changing them back 
            if (skillLocator.primary.skillNameToken == prefix + "PRIMARY_GUN_NAME")
            {
                SkillDef primary = skillLocator.primary.skillDef;
                primary.icon = YusukeSurvivor.spiritGunIcon;
            }
            if (skillLocator.secondary.skillNameToken == prefix + "SECONDARY_GUN_NAME")
            {
                SkillDef secondary = skillLocator.secondary.skillDef;
                secondary.icon = YusukeSurvivor.spiritGunIcon;
            }


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

        protected virtual EntityState BeamNextState()
        {

            return new FireSpiritBeam
            {
                charge = totalCharge,
                
            };
        }



    }


}
