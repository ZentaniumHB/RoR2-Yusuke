using YusukeMod.Survivors.Yusuke.Achievements;
using RoR2;
using UnityEngine;

namespace YusukeMod.Survivors.Yusuke
{
    public static class YusukeUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                YusukeMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(YusukeMasteryAchievement.identifier),
                YusukeSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
