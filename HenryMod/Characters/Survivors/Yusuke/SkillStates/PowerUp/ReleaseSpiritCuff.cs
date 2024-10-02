using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Modules;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp
{
    public class ReleaseSpiritCuff : BaseSkillState
    {
        public static float baseDuration = 1.25f;

        private float duration = 1f;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", duration);

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
            SpiritCuffComponent cuffComponent = gameObject.GetComponent<SpiritCuffComponent>();
            if (cuffComponent != null)
            {
                cuffComponent.hasReleased = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
