using RoR2;
using HenryMod.Modules.Achievements;

namespace HenryMod.Survivors.Fish.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class FishMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = FishSurvivor.FISH_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = FishSurvivor.FISH_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => FishSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}