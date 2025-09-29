using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking.Types;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    internal class SwitchSkills : BaseSkillState
    {

        private EntityStateMachine stateMachine;
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        private bool switchedSkill;
        public int switchID;

        public override void OnEnter()
        {
            // depending on the ID determins the switch
            if(switchID == 1) SwitchToMazokuSkills();
            if(switchID == 2) RevertMazokuSkills();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (switchedSkill) outer.SetNextStateToMain();
        }

        private void SwitchToMazokuSkills()
        {
            Log.Info("Switching to mazoku skills");
            switch (skillLocator.primary.skillNameToken)
            {
                case prefix + "PRIMARY_SLASH_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.primaryMelee, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.mazokuMelee, GenericSkill.SkillOverridePriority.Contextual); //mazMelee
                    break;
                case prefix + "PRIMARY_GUN_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.primarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.mazokuDemonGunPrimary, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.secondary.skillNameToken)
            {
                case prefix + "SECONDARY_GUN_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.mazokuDemonGun, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "SECONDARY_SHOTGUN_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritShotgun, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.backToBackStrikes, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.utility.skillNameToken)
            {
                case prefix + "UTILITY_SLIDEDASH_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.utilityDash, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.demonDash, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "UTILITY_WAVE_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.utilityWave, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.swingCombo, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.special.skillNameToken)
            {
                case prefix + "SPECIAL_SPIRITMEGA_NAME":
                    skillLocator.special.UnsetSkillOverride(gameObject, YusukeSurvivor.specialSpiritGunMega, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.special.SetSkillOverride(gameObject, YusukeSurvivor.demonGunMega, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "SPECIAL_SPIRITCUFF_NAME":
                    //skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.utilityWave, GenericSkill.SkillOverridePriority.Contextual);
                    //skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.swingCombo, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }


            characterBody.GetComponent<SingleTracking>().TurnOn();    // enabling the tracker for the specific move needed. 

            switchedSkill = true;
        }


        private void RevertMazokuSkills()
        {
            Log.Info("Reverting mazoku skills");
            switch (skillLocator.primary.skillNameToken)
            {
                case prefix + "PRIMARY_MAZOKUMELEE_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.mazokuMelee, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.primaryMelee, GenericSkill.SkillOverridePriority.Contextual); //mazMelee
                    break;
                case prefix + "PRIMARY_MAZOKUGUN_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.mazokuDemonGunPrimary, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.primarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.secondary.skillNameToken)
            {
                case prefix + "SECONDARY_MAZOKUGUN_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.mazokuDemonGun, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "SECONDARY_MAZBACKTOBACK_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.backToBackStrikes, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritShotgun, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.utility.skillNameToken)
            {
                case prefix + "UTILITY_MAZDEMONDASH_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.demonDash, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.utilityDash, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "UTILTY_MAZSWINGCOMBO_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.swingCombo, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.utilityWave, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.special.skillNameToken)
            {
                case prefix + "SPECIAL_MAZ_MEGA_NAME":
                    if (inputBank.skill4.down)
                    {
                        break;
                    }
                    else
                    {
                        skillLocator.special.UnsetSkillOverride(gameObject, YusukeSurvivor.demonGunMega, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.special.SetSkillOverride(gameObject, YusukeSurvivor.specialSpiritGunMega, GenericSkill.SkillOverridePriority.Contextual);
                        break;
                    }
                case prefix + "SPECIAL_SPIRITCUFF_NAME":
                    //skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.utilityWave, GenericSkill.SkillOverridePriority.Contextual);
                    //skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.swingCombo, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }

            characterBody.GetComponent<SingleTracking>().TurnOff(); // disabling the tracker. 

            switchedSkill = true;
        }
    }
}
