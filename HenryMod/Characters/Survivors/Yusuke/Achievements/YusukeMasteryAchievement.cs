using RoR2;
using YusukeMod.Modules.Achievements;

namespace YusukeMod.Survivors.Yusuke.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class YusukeMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = YusukeSurvivor.YUSUKE_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = YusukeSurvivor.YUSUKE_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => YusukeSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}