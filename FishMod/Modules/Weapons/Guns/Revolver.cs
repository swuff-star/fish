using EntityStates;
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
        public override string iconName => "texIconRevolver";
        public override GameObject crosshairPrefab => null;
        public override int magSize => 7;
        public override float magPickupMultiplier => 1;
        public override int pickupAmmo => 32;
        public override float reloadDuration => 0.2f;
        public override string ammoName => "Bullets";
        public override GameObject modelPrefab => FishSurvivor.instance.assetBundle.LoadAsset<GameObject>("mdlRevolver");
        public override FishWeaponDef.AnimationSet animationSet => FishWeaponDef.AnimationSet.Pistol;
        public override FishWeaponDef.AmmoType ammoType => FishWeaponDef.AmmoType.Bullet;
        public override bool storedOnBack => false;

        public override FishWeaponSkillDef primarySkillDef => Skills.CreateFishWeaponSkillDef<FishWeaponSkillDef>(new SkillDefInfo
            {
                skillName = "FishRevolver",
                skillNameToken = FishSurvivor.FISH_PREFIX + "REVOLVER_NAME",
                skillDescriptionToken = FishSurvivor.FISH_PREFIX + "REVOLVER_DESCRIPTION",
                skillIcon = FishSurvivor.instance.assetBundle.LoadAsset<Sprite>(iconName),

                activationState = new SerializableEntityStateType(typeof(EntityStates.Fish.Guns.FireRevolver)),
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
