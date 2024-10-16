using EntityStates;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking.Types;
using YusukeMod.Characters.Survivors.Yusuke.Components;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{

    public class BaseChargeSpirit : BaseSkillState
    {

        public float chargeDuration;
        public float baseChargeDuration;
        public SpiritCuffComponent cuffComponent;
        public const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        // starting value, max value and how fast to increment
        public float chargeValue;
        public float chargeIncrement;
        public float chargeLimit;

        public override void OnEnter()
        {
            base.OnEnter();
            cuffComponent = characterBody.GetComponent<SpiritCuffComponent>();

        }
        public override void OnExit()
        {
            base.OnExit();
            
        }

        public float RoundTheFloat(float value)
        {
            return Mathf.Round(value);
        }

        // reverts the changes of the icons
        public void RevertIconSwitch(int attackID)
        {
            // getting the icons and changing them back 
            SkillDef primary = skillLocator.primary.skillDef;
            SkillDef secondary = skillLocator.secondary.skillDef;

            if(attackID == 2)
            {
                if (skillLocator.primary.skillNameToken == prefix + "PRIMARY_GUN_NAME")
                {
                    primary.icon = YusukeSurvivor.spiritGunIcon;
                }
                if (skillLocator.secondary.skillNameToken == prefix + "SECONDARY_GUN_NAME")
                {
                    secondary.icon = YusukeSurvivor.spiritGunIcon;
                }
            }
            if (attackID == 3)  // shotgun
            {
                if (skillLocator.secondary.skillNameToken == prefix + "SECONDARY_SHOTGUN_NAME")
                {
                    secondary.icon = YusukeSurvivor.spiritShotgunIcon;
                }
            }
        }

        // changes the icons when moves are charged, depending if it's maxed or not 
        public void IconSwitch(bool hasReleased, int attackID)
        {
            // getting the icons and changing them accordingly 
            /*SkillDef primary = skillLocator.primary.skillDef;
            SkillDef secondary = skillLocator.secondary.skillDef;*/
            switch (attackID)
            {
                case 1:
                    if (skillLocator.primary.skillNameToken == prefix + "PRIMARY_GUN_NAME")
                    {
                        if (hasReleased)
                        {
                            skillLocator.primary.skillDef.icon = YusukeSurvivor.spiritBeamIcon;
                        }
                        else
                        {
                            skillLocator.primary.skillDef.icon = YusukeSurvivor.spiritGunDoubleIcon;
                        }
                    }
                    break;
                case 2:
                    if (skillLocator.secondary.skillNameToken == prefix + "SECONDARY_GUN_NAME")
                    {
                        if (hasReleased)
                        {
                            skillLocator.secondary.skillDef.icon = YusukeSurvivor.spiritBeamIcon;
                        }
                        else
                        {
                            skillLocator.secondary.skillDef.icon = YusukeSurvivor.spiritGunDoubleIcon;
                        }

                    }
                    break;
                case 3:
                    if (attackID == 3) // shotgun
                    {
                        if (skillLocator.secondary.skillNameToken == prefix + "SECONDARY_SHOTGUN_NAME")
                        {
                            if (hasReleased)
                            {
                                skillLocator.secondary.skillDef.icon = YusukeSurvivor.spiritShotGunDoubleIcon;
                            }
                            else
                            {
                                skillLocator.secondary.skillDef.icon = YusukeSurvivor.spiritShotgunIcon;
                            }

                        }
                    }
                    break;
            }

        }

    }


}
