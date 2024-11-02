using EntityStates;
using Rewired.Demos;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.SkillStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    public class ChargeDemonGunPrimary : ChargeDemonGun
    {
        public override int attackID { get; set; } = 1;

        protected override EntityState SpiritNextState()
        {


            if (bullets == 0) bullets = 1;   // just so a bullet is shot when a player just taps the skill
            return new FireDemonGunBarrage
            {
                charge = totalCharge,
                totalBullets = bullets,
                isPrimary = true,
            };
        }


    }
}
