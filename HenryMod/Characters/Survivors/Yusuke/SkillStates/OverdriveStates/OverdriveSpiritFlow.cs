using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    internal class OverdriveSpiritFlow : BaseSkillState
    {

        private float duration = 1f;
        private float overdriveTimeDuration;
        private float overdriveFullDuration = 5f;

        public override void OnEnter()
        {
            base.OnEnter();

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            overdriveTimeDuration += GetDeltaTime();

            if (isAuthority && fixedAge >= duration)
            {
                if (overdriveTimeDuration > overdriveFullDuration)
                {
                    outer.SetNextStateToMain();
                }
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
