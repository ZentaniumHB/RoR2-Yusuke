using EntityStates;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using YusukeMod;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    public class RevertSkills : BaseSkillState
    {


        private EntityStateMachine stateMachine;
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;
        public int moveID;
        private bool switchSkills;



        public override void OnEnter()
        {
            base.OnEnter();
            stateMachine = characterBody.GetComponent<EntityStateMachine>();
            SwitchSkillsBack(moveID);

        }

        

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (switchSkills) outer.SetNextStateToMain();
        }

        private void SwitchSkillsBack(int moveID)
        {

            /* ID 1 == Melee
               ID 2 == gun
               ID 3 == ShotGun
               ID 4 == none
            */

            Log.Info("Checking skills to change back");
            switch (skillLocator.primary.skillNameToken)
            {
                case prefix + "PRIMARY_SLASH_NAME":
                    // swap the skills out
                    break;
                case prefix + "PRIMARY_GUN_NAME":
                    // swapt the skills out
                    break;
            }
            switch (skillLocator.secondary.skillNameToken)
            {
                case prefix + "FOLLOWUP_GUN_NAME":
                    int followUpSpiritGun = 0;
                    skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.spiritGunFollowUp, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                    Log.Info("SpiritGun was reverted");
                    Log.Info("MoveID: " +moveID);
                    Log.Info("followUpGun: " + followUpSpiritGun);
                    if (moveID != 4)
                        if (moveID == 2)
                            if (followUpSpiritGun == 0) FollowUpSettings(true, 2, 2);  //spirit gun was used
                    break;
                case prefix + "FOLLOWUP_SHOTGUN_NAME":
                    int followUpShotgun = 1;
                    /*base.skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritShotgun, GenericSkill.SkillOverridePriority.Contextual);
                    base.skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.spiritGunFollowUp, GenericSkill.SkillOverridePriority.Contextual);*/
                    if (moveID != 4)
                        if (moveID == 2)
                            if (followUpShotgun == 1) FollowUpSettings(true, 2, 3);  //shotgun was used
                    break;

            }
        }

        public void FollowUpSettings(bool isFollowUpActive, int skillSlot, int ID)
        {
            Log.Info("FollowUpActivation: " + isFollowUpActive);
            /* if (skillSlot == 1) //skillLocator.primary.DeductStock(1);
                if (skillSlot == 2) //skillLocator.secondary.DeductStock(1);*/

            if (stateMachine == null)
            {
                Log.Error("No State machine found");
            }
            else
            {
                Type currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    Log.Info("Yusuke State machine found");
                    YusukeMain targetState = (YusukeMain)stateMachine.state;
                    Log.Info("Starting cooldown");
                    if (ID == 1) targetState.StartCoolDown(1);
                    if (ID == 2) targetState.StartCoolDown(2);
                    if (ID == 3) targetState.StartCoolDown(3);

                    switchSkills = true;
                }
                else
                {
                    Log.Error("This is not the YusukeMain state.");

                }


            }


        }


    }
}
