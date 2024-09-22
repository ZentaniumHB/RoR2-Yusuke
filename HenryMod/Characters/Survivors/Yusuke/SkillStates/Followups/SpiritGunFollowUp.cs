using EntityStates;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    internal class SpiritGunFollowUp : BaseSkillState
    {

        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        public float charge;

        public int ID;

        public override void OnEnter()
        {
            base.OnEnter();

            Log.Info("ENTERED SPIRIT FOLLOW UP");
            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
            Log.Info("LEAVING SPIRIT FOLLOW UP");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (ID != 0)
            {
                if (fixedAge >= fireTime)
                {
                    Fire();
                }

                if (fixedAge >= duration && isAuthority)
                {

                    Log.Info("ID IN Spirit follow up: " + ID);
                    outer.SetNextState(new RevertSkills
                    {
                        moveID = ID

                    });

                    

                }
            }
            else
            {
                outer.SetNextStateToMain();     // this is only because for some reason this state gets called more than once, dunno why yet.
            }



        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}
