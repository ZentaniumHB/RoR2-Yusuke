using EntityStates;
using Rewired.Demos;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp
{
    internal class BeginMazokuTransformation : BaseSkillState
    {
        public static float baseDuration = 1.25f;

        private float duration = 12f;

        public override void OnEnter()
        {
            base.OnEnter();
            // if the transform count is greater than 2 (meaning raizen has passed) then do the other animation
            PlayAnimation("FullBody, Override", "MazokuTransformRaizen", "ThrowBomb.playbackRate", duration);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextState(SkillSwitch(1));
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
            if (maz != null)
            {
                maz.hasTransformed = true;
            }

            // AFTER switch to the other animation set (mazoku)
        }

        protected virtual EntityState SkillSwitch(int ID)
        {
            // use of an ID is needed to decide which move gets swapped in
            return new SwitchSkills
            {
                switchID = ID,
            };
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
