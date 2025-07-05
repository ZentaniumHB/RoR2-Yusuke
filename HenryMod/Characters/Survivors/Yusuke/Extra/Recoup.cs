using EntityStates;
using RoR2;
using System;


namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    // This is simply used for visual animation fatigue, might use it for other conditions too
    internal class Recoup : BaseSkillState
    {
        public byte recoupID;
        public float animDuration;
        public float animationStopwatch;

        public override void OnEnter()
        {
            // depending on the ID determins the anim
            if (recoupID == 1) 
            {
                PlayAnimation("RightArm, Override", "WaveGroundedRecoup", "Slide.playbackRate", 1f);
                animDuration = 1f;
            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = false;
            animationStopwatch += GetDeltaTime();
            Log.Info("AnimStopwatch in RECOUP: " + animationStopwatch);
            if (animationStopwatch > animDuration) 
            { 
                outer.SetNextStateToMain();
                return;
            }
                
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Vehicle;
        }

    }
}
