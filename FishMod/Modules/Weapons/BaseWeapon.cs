using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using FishMod.Survivors.Fish;
using static FishMod.Modules.Weapons.FishWeaponDef;
using FishMod.Modules.Weapons;

namespace FishMod.Modules.Guns
{
    public abstract class BaseWeapon<T> : BaseWeapon where T : BaseWeapon<T>
    {
        public static T instance { get; private set; }

        public BaseWeapon()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting BaseWeapon was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class BaseWeapon
    {
        public FishWeaponDef weaponDef { get; private set; }
        public ItemDef itemDef { get; private set; }
        public abstract string weaponNameToken { get; }
        public abstract string weaponName { get; }
        public abstract string weaponDesc { get; }
        public abstract string iconName { get; }
        public abstract GameObject crosshairPrefab { get; }
        public abstract int magSize { get; }
        public abstract float magPickupMultiplier { get; }
        public abstract int pickupAmmo { get; }
        public abstract float reloadDuration { get; }
        public abstract string ammoName { get; }
        public abstract SkillDef primarySkillDef { get; }
        public abstract GameObject modelPrefab { get; }
        public abstract FishWeaponDef.AnimationSet animationSet { get; }
        public abstract FishWeaponDef.AmmoType ammoType { get; }
        public abstract bool storedOnBack { get; }

        public virtual string weaponNameTokenFull
        {
            get
            {
                return "SWUFF_FISH_WEAPON_" + weaponNameToken + "_NAME";
            }
        }

        public virtual string weaponDescToken
        {
            get
            {
                return "SWUFF_FISH_WEAPON_" + weaponNameToken + "_DESC";
            }
        }

        public virtual void Init()
        {
            CreateLang();
            CreateWeapon();
        }

        protected void CreateLang()
        {
            LanguageAPI.Add("SWUFF_FISH_WEAPON_" + weaponNameToken + "_NAME", weaponName);
            LanguageAPI.Add("SWUFF_FISH_WEAPON_" + weaponNameToken + "_DESC", weaponDesc);
        }

        protected void CreateWeapon()
        {
            Sprite icon = null;
            if (iconName != "") icon = FishSurvivor.instance.assetBundle.LoadAsset<Sprite>(iconName);

            weaponDef = CreateWeaponDefFromInfo(new FishWeaponDefInfo
            {
                nameToken = "SWUFF_FISH_WEAPON_" + weaponNameToken + "_NAME",
                descriptionToken = "SWUFF_FISH_WEAPON_" + weaponNameToken + "_DESC",
                icon = icon,
                crosshairPrefab = crosshairPrefab,
                magSize = magSize,
                magPickupMultiplier = magPickupMultiplier,
                pickupAmmo = pickupAmmo,
                reloadDuration = reloadDuration,
                ammoName = ammoName,
                primarySkillDef = primarySkillDef,
                modelPrefab = modelPrefab,
                animationSet = animationSet,
                ammoType = ammoType,
                storedOnBack = storedOnBack,
            });

            // this really should have worked man i fucking hate the solution
            //itemDef = (ItemDef)ScriptableObject.CreateInstance(typeof(ItemDef));
            itemDef = ItemDef.Instantiate(Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/CloverVoid/CloverVoid.asset").WaitForCompletion());
            itemDef.name = "SWUFF_FISH_WEAPON_" + weaponNameToken + "_NAME";
            itemDef.nameToken = "SWUFF_FISH_WEAPON_" + weaponNameToken + "_NAME";
            itemDef.descriptionToken = "SWUFF_FISH_WEAPON_" + weaponNameToken + "_DESC";
            itemDef.pickupToken = "SWUFF_FISH_WEAPON_" + weaponNameToken + "_DESC";
            itemDef.loreToken = "SWUFF_FISH_WEAPON_" + weaponNameToken + "_DESC";
            itemDef.canRemove = false;
            itemDef.hidden = false;
            itemDef.pickupIconSprite = weaponDef.icon;
            itemDef.requiredExpansion = null;
            itemDef.tags = new ItemTag[]
            {
                ItemTag.AIBlacklist,
                ItemTag.BrotherBlacklist,
                ItemTag.CannotCopy,
                ItemTag.CannotDuplicate,
                ItemTag.CannotSteal,
                ItemTag.WorldUnique
            };
            itemDef.unlockableDef = null;

            if (modelPrefab)
            {
                itemDef.pickupModelPrefab = FishSurvivor.instance.assetBundle.LoadAsset<GameObject>(weaponDef.modelPrefab.name + "Pickup");
                Asset.ConvertAllRenderersToHopooShader(itemDef.pickupModelPrefab);
            }

            // tell me why i can't set this tell me why this breaks the entire weapon when i do it this way
            //itemDef.tier = ItemTier.NoTier;

            weaponDef.itemDef = itemDef;
            FishWeaponCatalog.AddWeapon(weaponDef);

            if (modelPrefab) Asset.ConvertAllRenderersToHopooShader(modelPrefab);
        }
    }
}
