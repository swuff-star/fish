using EntityStates;
using FishMod.Modules.Guns;
using FishMod.Survivors.Fish;
using RoR2.Skills;
using UnityEngine;

namespace FishMod.Modules.Weapons.Guns
{
    public class Shotgun : BaseWeapon<Shotgun>
    {
        public override string weaponNameToken => "Shotgun";
        public override string weaponName => "Shotgun";
        public override string weaponDesc => "Basic shotgun. Fires every 0.57s";
        public override string iconName => "texIconShotgun";
        public override GameObject crosshairPrefab => null;
        public override int magSize => 7;
        public override float magPickupMultiplier => 1;
        public override int firstAvailableStage => 0;
        public override int pickupAmmo => 32;
        public override float reloadDuration => 0.57f;
        public override string ammoName => "Shells";
        public override GameObject modelPrefab => FishSurvivor.instance.assetBundle.LoadAsset<GameObject>("mdlShotgun");
        public override FishWeaponDef.AnimationSet animationSet => FishWeaponDef.AnimationSet.Pistol;
        public override FishWeaponDef.AmmoType ammoType => FishWeaponDef.AmmoType.Shell;
        public override bool storedOnBack => false;

        public override FishWeaponSkillDef primarySkillDef => Skills.CreateFishWeaponSkillDef<FishWeaponSkillDef>(new SkillDefInfo
            {
                skillName = "FishShotgun",
                skillNameToken = FishSurvivor.FISH_PREFIX + "SHOTGUN_NAME",
                skillDescriptionToken = FishSurvivor.FISH_PREFIX + "SHOTGUN_DESCRIPTION",
                skillIcon = FishSurvivor.instance.assetBundle.LoadAsset<Sprite>(iconName),

                activationState = new SerializableEntityStateType(typeof(EntityStates.Fish.Guns.FireShotgun)),
                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Any,

                baseRechargeInterval = reloadDuration,
                baseMaxStock = 255,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 0,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,
                attackSpeedBuffsRestockSpeed = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,

                
            }, weaponDef);

        public override void Init()
        {
            base.Init();
        }
    }
}
