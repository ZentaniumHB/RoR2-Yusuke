using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp
{
    internal class SacredEnergyRelease : BaseSkillState
    {

        private float duration = 1f;

        YusukeWeaponComponent yusukeWeapon;
        HealthComponent yusukeHealth;

        public override void OnEnter()
        {
            base.OnEnter();

            yusukeWeapon = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeHealth = characterBody.GetComponent<HealthComponent>();

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

            if (yusukeHealth) yusukeHealth.health = yusukeHealth.fullHealth;

            if (characterMotor)
            {
                characterMotor.enabled = true;

            }

            if (NetworkServer.active)
            {
                if (yusukeHealth)
                {
                    yusukeHealth.godMode = false;
                }

            }

            if (yusukeWeapon) yusukeWeapon.SetKnockedBoolean(false);

            base.OnExit();

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
