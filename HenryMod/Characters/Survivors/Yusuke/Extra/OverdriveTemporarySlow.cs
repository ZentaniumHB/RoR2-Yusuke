using EntityStates;
using On.RoR2.CharacterAI;
using RoR2;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    public class OverdriveTemporarySlow : BaseState
    {
        public float duration;

        Animator animator;
        internal float previousAttackSpeedStat;
        private Animator modelAnimator;

        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            if (modelAnimator)
            {
                modelAnimator.enabled = false;
            }
            if (rigidbody && !rigidbody.isKinematic)
            {
                rigidbody.velocity = Vector3.zero;
                if (rigidbodyMotor)
                {
                    rigidbodyMotor.moveVector = Vector3.zero;
                }
            }
            if (characterDirection)
            {
                characterDirection.moveVector = characterDirection.forward;
            }

            
        }
        public override void OnExit()
        {
            if (modelAnimator)
            {
                modelAnimator.enabled = true;
            }
            CharacterModel model = GetModelTransform().GetComponent<CharacterModel>();
            if (model)
            {
                model.forceUpdate = true;
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            attackSpeedStat = 0f;


            if (characterDirection)
            {
                characterDirection.moveVector = characterDirection.forward;
            }

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(duration);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            duration = reader.ReadSingle();
        }
    }
}
