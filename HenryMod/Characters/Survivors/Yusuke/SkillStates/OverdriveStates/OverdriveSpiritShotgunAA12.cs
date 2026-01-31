using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    internal class OverdriveSpiritShotgunAA12 : BaseSkillState
    {

        private float duration = 1f;

        public override void OnEnter()
        {
            base.OnEnter();

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }

        }

        public override void OnExit()
        {

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
