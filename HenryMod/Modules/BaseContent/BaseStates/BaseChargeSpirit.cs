using EntityStates;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{

    public class BaseChargeSpirit : BaseSkillState
    {

        public float chargeDuration;
        public float baseChargeDuration;


        // starting value, max value and how fast to increment
        public float chargeValue;
        public float chargeIncrement;
        public float chargeLimit;

        public override void OnEnter()
        {
            base.OnEnter();

        }
        public override void OnExit()
        {
            base.OnExit();
            
        }

        public float RoundTheFloat(float value)
        {
            return Mathf.Round(value);
        }

        



    }


}
