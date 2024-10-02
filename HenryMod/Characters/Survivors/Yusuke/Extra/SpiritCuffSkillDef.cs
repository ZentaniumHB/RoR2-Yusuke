using RoR2.Skills;
using JetBrains.Annotations;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using RoR2;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    public class SpiritCuffSkillDef : SkillDef
    {

        // grabbed the method from HuntressSkillDef and referenced the Henry Fury skill
        protected class InstanceData : BaseSkillInstanceData
        {
            public SpiritCuffComponent cuffComponent;
        }

        
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                cuffComponent = skillSlot.GetComponent<SpiritCuffComponent>()
            };
        }

        private static bool HasEnoughSpiritEnergy([NotNull] GenericSkill skillSlot)
        {
            if (!(((InstanceData)skillSlot.skillInstanceData).cuffComponent?.currentSpiritValue >= (float)skillSlot.rechargeStock))
            {
                return false;
            }
            return true;


        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            if (!HasEnoughSpiritEnergy(skillSlot))
            {
                return false;
            }
            return base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            if (base.IsReady(skillSlot))
            {
                return HasEnoughSpiritEnergy(skillSlot);
            }
            return false;
        }
    }
}
