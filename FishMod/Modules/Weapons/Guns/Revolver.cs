using FishMod.Modules.Guns;
using FishMod.Survivors.Fish;
using RoR2.Skills;
using UnityEngine;

namespace FishMod.Modules.Weapons.Guns
{
    public class Revolver : BaseWeapon<Revolver>
    {
        public override string weaponNameToken => "Revolver";
        public override string weaponName => "Revolver";
        public override string weaponDesc => "Basic pistol. Fires every 0.2s";
        public override string iconName => "texRevolverIcon";
        public override GameObject crosshairPrefab => null;
        public override int magSize => 7;
        public override float magPickupMultiplier => 1;
        public override int pickupAmmo => 32;
        public override float reloadDuration => 0.2f;
        public override string ammoName => "Bullets";
        public override GameObject modelPrefab => FishSurvivor.instance.assetBundle.LoadAsset<GameObject>("mdlRevolver");
        public override FishWeaponDef.AnimationSet animationSet => FishWeaponDef.AnimationSet.Pistol;
        public override bool storedOnBack => false;

        public override SkillDef primarySkillDef => Skills.CreatePrimarySkillDef(
            new EntityStates.SerializableEntityStateType(typeof(EntityStates.Fish.Guns.FireRevolver)),
            "Weapon",
            "SWUFF_FISH_BODY_SHOOT_REVOLVER_NAME",
            "SWUFF_FISH_BODY_SHOOT_REVOLVER_DESCRIPTION",
            FishSurvivor.instance.assetBundle.LoadAsset<Sprite>("texShootIcon"),
            false);

        public override void Init()
        {
            base.Init();
        }
    }
}
