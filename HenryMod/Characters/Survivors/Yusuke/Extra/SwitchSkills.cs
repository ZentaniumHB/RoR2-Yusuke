using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking.Types;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    internal class SwitchSkills : BaseSkillState
    {
        // enum for the skill switching
        public enum SwitchSkillIndex
        {
            MazokuSwitch,
            MazokuRevert,
            OverdriveSwitch
        }

        private EntityStateMachine stateMachine;
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        private bool switchedSkill;
        public int switchID;

        private YusukeWeaponComponent yusukeWeaponComponent;

        public override void OnEnter()
        {
            // depending on the ID determins the switch
            yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            if (switchID == (int)SwitchSkillIndex.MazokuSwitch) SwitchToMazokuSkills();
            if(switchID == (int)SwitchSkillIndex.MazokuRevert) RevertMazokuSkills();
            if(switchID == (int)SwitchSkillIndex.OverdriveSwitch) SwitchOverdriveSkills();

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


        private void SwitchOverdriveSkills()
        {
            switch (skillLocator.primary.skillNameToken)
            {
                case prefix + "PRIMARY_SLASH_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.primaryMelee, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.overdrive12Hook, GenericSkill.SkillOverridePriority.Contextual); 
                    break;
                case prefix + "OVERDRIVE_12HOOKS_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.overdrive12Hook, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.primaryMelee, GenericSkill.SkillOverridePriority.Contextual); 
                    break;
                case prefix + "PRIMARY_GUN_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.primarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritSnipe, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "OVERDRIVE_SPIRITSNIPE_NAME":
                    skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritSnipe, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.primarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.secondary.skillNameToken)
            {
                case prefix + "SECONDARY_GUN_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritSnipe, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "OVERDRIVE_SPIRITSNIPE_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritSnipe, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "SECONDARY_SHOTGUN_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritShotgun, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritShotgunAA12, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "OVERDRIVE_SHOTGUNAA12_NAME":
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritShotgunAA12, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritShotgun, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }
            switch (skillLocator.utility.skillNameToken)
            {
                case prefix + "UTILITY_SLIDEDASH_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.utilityDash, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritFlow, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "OVERDRIVE_SPIRITFLOW_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritFlow, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.utilityDash, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "UTILITY_WAVE_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.utilityWave, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritWaveImpactFist, GenericSkill.SkillOverridePriority.Contextual);
                    break;
                case prefix + "OVERDRIVE_SPIRITWAVE_IMPACT_NAME":
                    skillLocator.utility.UnsetSkillOverride(gameObject, YusukeSurvivor.overdriveSpiritWaveImpactFist, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(gameObject, YusukeSurvivor.utilityWave, GenericSkill.SkillOverridePriority.Contextual);
                    break;
            }

            // triggers for the mazoku component, so there is no confliction. Otherwise I can just do it manually within the YusukeMain state instead of here.
            if (yusukeWeaponComponent)
            {
                if (!yusukeWeaponComponent.GetOverDriveSkillsActivity())
                {
                    yusukeWeaponComponent.SetOverDriveSkillsActivity(true);
                }
                else
                {
                    yusukeWeaponComponent.SetOverDriveSkillsActivity(false);
                }
            }

            switchedSkill = true;
        }

    }
}
