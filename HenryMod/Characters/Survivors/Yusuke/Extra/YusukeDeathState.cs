using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    internal class YusukeDeathState : GenericCharacterDeath
    {

        public override void OnEnter()
        {
            base.OnEnter();

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

        }



        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
