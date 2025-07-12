using FishMod.Survivors.Fish.SkillStates;
using EntityStates.Fish;
using EntityStates.Fish.Guns;

namespace FishMod.Survivors.Fish
{
    public static class FishStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));
            Modules.Content.AddEntityState(typeof(Shoot));
            Modules.Content.AddEntityState(typeof(Roll));
            Modules.Content.AddEntityState(typeof(ThrowBomb));

            // base states
            Modules.Content.AddEntityState(typeof(SwapWeapons));

            // gun states
            Modules.Content.AddEntityState(typeof(FireRevolver));
            Modules.Content.AddEntityState(typeof(FireMachinegun));
        }
    }
}
