using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using YusukeMod.SkillStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack
{
    internal class ChargeSpiritGunPrimary : ChargeSpiritGun
    {
        public override int attackID { get; set; } = 1;

        protected override EntityState SpiritNextState()
        {
            return new Shoot
            {
                charge = totalCharge,
                isPrimary = true
        };
        }

        protected override EntityState DoubleNextState()
        {
            return new SpiritGunDouble
            {
                charge = totalCharge,
                isMaxCharge = isMaxCharge,
                isPrimary = true

            };
        }

        protected override EntityState BeamNextState()
        {

            return new FireSpiritBeam
            {
                charge = totalCharge,
                isPrimary = true
};
        }
    }
}
