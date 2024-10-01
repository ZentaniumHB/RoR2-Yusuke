using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using YusukeMod.Modules;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp
{
    public class ReleaseSpiritCuff : BaseSkillState
    {

        public override void OnEnter()
        {
            base.OnEnter();
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
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
