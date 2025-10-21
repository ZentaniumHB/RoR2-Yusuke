using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace YusukeMod.Modules.BaseContent.BaseStates
{
    public class YusukeBeginSpawn : BaseSkillState
    {

        public static float animationDuration = 9.1f;


        private Transform modelTransform;
        private AimAnimator aimAnim;
        private float originalGiveUpDuration;

        public override void OnEnter()
        {
            base.OnEnter();
            modelTransform = GetModelTransform();

            if (modelTransform != null) 
            {
                aimAnim = modelTransform.GetComponent<AimAnimator>();
                originalGiveUpDuration = aimAnim.giveupDuration;
                aimAnim.giveupDuration = 0f;
                aimAnim.enabled = false;
            }

            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            PlayAnimation("Body", "Spawn", "animSpawn.playbackRate", animationDuration);
        }

        public override void OnExit()
        {

            if (modelTransform != null)
            {
                aimAnim = modelTransform.GetComponent<AimAnimator>();
                aimAnim.giveupDuration = originalGiveUpDuration;
                aimAnim.enabled = true;
            }

            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= animationDuration)
            {
                outer.SetNextStateToMain(); 
            }
        }

    }
}
