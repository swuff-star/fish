using UnityEngine;
using RoR2;
using RoR2.Skills;

namespace FishMod.Modules.Weapons
{
    [CreateAssetMenu(fileName = "wpn", menuName = "ScriptableObjects/WeaponDef", order = 1)]
    public class FishWeaponDef : ScriptableObject
    {
        // just taking what you had. ty rob.
        public enum AnimationSet
        {
            Pistol,
            SMG,
            Rocket,
            Unarmed,
            Throwable,
            PistolAlt,
            Railgun
        }

        public enum AmmoType
        {
            Bullet,
            Shell,
            Explosive,
            Bolt,
            Laser,
            None
        }

        [Header("General")]
        public string nameToken = "";
        public string descriptionToken = "";
        public Sprite icon = null;
        public GameObject crosshairPrefab = null;
        public int magSize = 8;
        public float magPickupMultiplier = 1f;
        public int pickupAmmo = 1;
        public float reloadDuration = 2.4f;
        public string ammoName = "";
        public bool allowAutoReload = true;
        public bool exposeWeakPoints = true;
        public bool roundReload = false;
        public bool canPickUpAmmo = true;
        public AmmoType ammoType = AmmoType.None;
        public int firstAvailableStage = 0;

        [Header("Skills")]
        public SkillDef primarySkillDef;

        [Header("Visuals")]
        public GameObject modelPrefab;
        public AnimationSet animationSet = AnimationSet.SMG;
        public bool storedOnBack = true;

        [HideInInspector]
        public ushort index;

        [HideInInspector]
        public ItemDef itemDef;

        public static FishWeaponDef CreateWeaponDefFromInfo(FishWeaponDefInfo weaponDefInfo)
        {
            FishWeaponDef weaponDef = (FishWeaponDef)CreateInstance(typeof(FishWeaponDef));
            weaponDef.name = weaponDefInfo.nameToken;

            weaponDef.nameToken = weaponDefInfo.nameToken;
            weaponDef.descriptionToken = weaponDefInfo.descriptionToken;
            weaponDef.icon = weaponDefInfo.icon;
            weaponDef.crosshairPrefab = weaponDefInfo.crosshairPrefab;
            weaponDef.magSize = weaponDefInfo.magSize;
            weaponDef.magPickupMultiplier = weaponDefInfo.magPickupMultiplier;
            weaponDef.pickupAmmo = weaponDefInfo.pickupAmmo;
            weaponDef.reloadDuration = weaponDefInfo.reloadDuration;
            weaponDef.ammoName = weaponDefInfo.ammoName;
            weaponDef.ammoType = weaponDefInfo.ammoType;
            weaponDef.firstAvailableStage = weaponDefInfo.firstAvailableStage;

            weaponDef.primarySkillDef = weaponDefInfo.primarySkillDef;

            weaponDef.modelPrefab = weaponDefInfo.modelPrefab;
            weaponDef.animationSet = weaponDefInfo.animationSet;
            weaponDef.storedOnBack = weaponDefInfo.storedOnBack;

            return weaponDef;
        }

        [System.Serializable]
        public struct FishWeaponDefInfo
        {
            public string nameToken;
            public string descriptionToken;
            public Sprite icon;
            public GameObject crosshairPrefab;
            public int magSize;
            public float magPickupMultiplier;
            public int pickupAmmo;
            public float reloadDuration;
            public string ammoName;
            public AmmoType ammoType;
            public int firstAvailableStage;

            public SkillDef primarySkillDef;

            public GameObject modelPrefab;
            public AnimationSet animationSet;
            public bool storedOnBack;
        }
    }
}
