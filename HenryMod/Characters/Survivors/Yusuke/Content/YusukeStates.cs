
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.SkillStates;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.Survivors.Yusuke
{
    public static class YusukeStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));


            Modules.Content.AddEntityState(typeof(ChargeSpiritGun));
            Modules.Content.AddEntityState(typeof(ChargeSpiritGunPrimary));
            Modules.Content.AddEntityState(typeof(ChargeSpiritGunMega));
            Modules.Content.AddEntityState(typeof(ChargeSpiritShotgun));
            Modules.Content.AddEntityState(typeof(MultiTracking));
            Modules.Content.AddEntityState(typeof(ChargeSpiritWave));


            Modules.Content.AddEntityState(typeof(FireSpiritShotgun));
            Modules.Content.AddEntityState(typeof(FireSpiritMega));
            Modules.Content.AddEntityState(typeof(ChargeSpiritWave));
            Modules.Content.AddEntityState(typeof(Shoot));
            Modules.Content.AddEntityState(typeof(SpiritWave));
            Modules.Content.AddEntityState(typeof(ReleaseSpiritCuff));

            Modules.Content.AddEntityState(typeof(ChargeDemonGun));
            Modules.Content.AddEntityState(typeof(MazBackToBackStrikes));
            Modules.Content.AddEntityState(typeof(FireDemonGunBarrage));

            Modules.Content.AddEntityState(typeof(SpiritGunDouble));
            Modules.Content.AddEntityState(typeof(SpiritGunFollowUp));
            Modules.Content.AddEntityState(typeof(DivePunch));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));
        }
    }
}
