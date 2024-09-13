
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
            Modules.Content.AddEntityState(typeof(ChargeSpiritGunMega));
            Modules.Content.AddEntityState(typeof(ChargeSpiritShotgun));
            Modules.Content.AddEntityState(typeof(MultiTracking));


            Modules.Content.AddEntityState(typeof(FireSpiritShotgun));
            Modules.Content.AddEntityState(typeof(FireSpiritMega));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));
        }
    }
}
