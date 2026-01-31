
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Modules.BaseContent.BaseStates;
using YusukeMod.Modules.BaseStates;
using YusukeMod.SkillStates;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.Survivors.Yusuke
{
    public static class YusukeStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));
            Modules.Content.AddEntityState(typeof(PunchCombo));

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
            Modules.Content.AddEntityState(typeof(SpiritWave2));
            Modules.Content.AddEntityState(typeof(ReleaseSpiritCuff));

            Modules.Content.AddEntityState(typeof(ChargeDemonGun));
            Modules.Content.AddEntityState(typeof(ChargeDemonGunPrimary));
            Modules.Content.AddEntityState(typeof(MazBackToBackStrikes));
            Modules.Content.AddEntityState(typeof(FireDemonGunBarrage));
            Modules.Content.AddEntityState(typeof(SwingCombo));
            Modules.Content.AddEntityState(typeof(ChargeDemonGunMega));

            Modules.Content.AddEntityState(typeof(SpiritGunDouble));
            Modules.Content.AddEntityState(typeof(SpiritGunFollowUp));
            Modules.Content.AddEntityState(typeof(DivePunch));

            Modules.Content.AddEntityState(typeof(Roll));
            Modules.Content.AddEntityState(typeof(BlinkDash));

            Modules.Content.AddEntityState(typeof(ThrowBomb));

            Modules.Content.AddEntityState(typeof(YusukeBeginSpawn));
            Modules.Content.AddEntityState(typeof(KnockedState));
            Modules.Content.AddEntityState(typeof(YusukeDeathState));
            Modules.Content.AddEntityState(typeof(MazokuResurrect));
            Modules.Content.AddEntityState(typeof(SacredEnergyRelease));

            Modules.Content.AddEntityState(typeof(OverdriveSpiritSnipe));
            Modules.Content.AddEntityState(typeof(OverdriveSpiritShotgunAA12));
            Modules.Content.AddEntityState(typeof(OverdriveSpiritWaveImpactFist));
            Modules.Content.AddEntityState(typeof(Overdrive12Hooks));
            Modules.Content.AddEntityState(typeof(OverdriveSpiritFlow));

            Modules.Content.AddEntityState(typeof(SwitchSkills));
            Modules.Content.AddEntityState(typeof(YusukeMain));
        }
    }
}
