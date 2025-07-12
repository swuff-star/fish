using FishMod.Survivors.Fish.Achievements;
using RoR2;
using UnityEngine;

namespace FishMod.Survivors.Fish
{
    public static class FishUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                FishMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(FishMasteryAchievement.identifier),
                FishSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
