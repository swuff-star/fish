using EntityStates;
using FishMod.Modules.Guns;
using FishMod.Survivors.Fish;
using RoR2.Skills;
using UnityEngine;

namespace FishMod.Modules.Weapons.Guns
{
    public class Machinegun : BaseWeapon<Machinegun>
    {
        public override string weaponNameToken => "Machinegun";
        public override string weaponName => "Machinegun";
        public override string weaponDesc => "Basic machine gun. Fires every 0.2s. Full auto";
        public override string iconName => "texIconMachinegun";
        public override GameObject crosshairPrefab => null;
        public override int magSize => 7;
        public override float magPickupMultiplier => 1;
        public override int pickupAmmo => 32;
        public override float reloadDuration => 0.17f;
        public override string ammoName => "Bullets";
        public override GameObject modelPrefab => FishSurvivor.instance.assetBundle.LoadAsset<GameObject>("mdlMachinegun");
        public override FishWeaponDef.AnimationSet animationSet => FishWeaponDef.AnimationSet.SMG;
        public override FishWeaponDef.AmmoType ammoType => FishWeaponDef.AmmoType.Bullet;
        public override bool storedOnBack => false;

        public override FishWeaponSkillDef primarySkillDef => Skills.CreateSkillDef<FishWeaponSkillDef>(new SkillDefInfo
            {
                skillName = "FishMachinegun",
                skillNameToken = FishSurvivor.FISH_PREFIX + "MACHINEGUN_NAME",
                skillDescriptionToken = FishSurvivor.FISH_PREFIX + "MACHINEGUN_DESCRIPTION",
                skillIcon = FishSurvivor.instance.assetBundle.LoadAsset<Sprite>(iconName),

                activationState = new SerializableEntityStateType(typeof(EntityStates.Fish.Guns.FireMachinegun)),
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
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,
                attackSpeedBuffsRestockSpeed = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            });

        public override void Init()
        {
            base.Init();
        }
    }
}
