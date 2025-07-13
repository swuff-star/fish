using FishMod.Modules.Weapons;
using FishMod.Modules.Weapons.Guns;
using FishMod.Survivors.Fish;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static FishMod.Modules.Weapons.FishWeaponDef;
using static RoR2.GenericPickupController;

namespace FishMod.Characters.Survivors.Fish.Components
{
    public struct FishWeaponData
    {
        public FishWeaponDef weaponDef;
    }

    public struct FishStoredWeaponData
    {
        public FishWeaponDef weaponDef;
    }

    public class FishWeaponTracker : MonoBehaviour
    {
        public FishWeaponData[] weaponData = new FishWeaponData[0];
        public FishWeaponData[] storedWeaponData = new FishWeaponData[0];
        public int equippedIndex = 0;
        public int offhandIndex = 1;
        public int nextWeapon;

        public float offhandRemainingCooldown = 0f;

        // ammo types
        public int currentBullets = 0;
        public int maxBullets = 255;
        public int bulletsPerPickup = 32;

        public int currentShells = 0;
        public int maxShells = 55;
        public int shellsPerPickup = 8;

        public int currentExplosives = 0;
        public int maxExplosives = 55;
        public int explosivesPerPickup = 6;

        public int currentBolts = 0;
        public int maxBolts = 55;
        public int boltsPerPickup = 7;

        public int currentLasers = 0;
        public int maxLasers = 55;
        public int lasersPerPickup = 10;

        public FishWeaponDef.AmmoType activeAmmo = FishWeaponDef.AmmoType.None;

        private bool hasInit = false;
        private FishWeaponController _fishWeaponController;
        private SkillLocator _bodySkllLocator;
        private Inventory inventory;

        private bool isValidatingWeapons = false;
        private bool suppressValidation = false;

        bool isFish = false;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();

            isFish = TryGetComponent(out CharacterMaster master) && BodyCatalog.FindBodyIndex(master.GetBody()) == BodyCatalog.FindBodyIndex(FishSurvivor.instance.bodyPrefab);

            Init();
        }

        public void SetAmmoType(AmmoType type)
        {
            activeAmmo = type;
        }

        public bool MaxAmmo()
        {
            return currentBullets >= maxBullets && currentShells >= maxShells && currentExplosives >= maxExplosives && currentBolts >= maxBolts && currentLasers >= maxLasers;  
        }
        public int GetCurrentAmmoTypeRemaining()
        {
            switch (activeAmmo)
            {
                case FishWeaponDef.AmmoType.Bullet:
                    return currentBullets;
                case FishWeaponDef.AmmoType.Shell:
                    return currentShells;
                case FishWeaponDef.AmmoType.Explosive:
                    return currentExplosives;
                case FishWeaponDef.AmmoType.Bolt:
                    return currentBolts;
                case FishWeaponDef.AmmoType.Laser:
                    return currentLasers;
                case FishWeaponDef.AmmoType.None:
                    return 1;
                default:
                    return 0;
            }
        }
        public int GetAmmoTypeMax(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case FishWeaponDef.AmmoType.Bullet:
                    return maxBullets;
                case FishWeaponDef.AmmoType.Shell:
                    return maxShells;
                case FishWeaponDef.AmmoType.Explosive:
                    return maxExplosives;
                case FishWeaponDef.AmmoType.Bolt:
                    return maxBolts;
                case FishWeaponDef.AmmoType.Laser:
                    return maxLasers;
                case FishWeaponDef.AmmoType.None:
                    return -1;
                default:
                    return 0;
            }
        }
        public int GetAmmoTypeCount(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case FishWeaponDef.AmmoType.Bullet:
                    return currentBullets;
                case FishWeaponDef.AmmoType.Shell:
                    return currentShells;
                case FishWeaponDef.AmmoType.Explosive:
                    return currentExplosives;
                case FishWeaponDef.AmmoType.Bolt:
                    return currentBolts;
                case FishWeaponDef.AmmoType.Laser:
                    return currentLasers;
                case FishWeaponDef.AmmoType.None:
                    return -1;
                default:
                    return 0;
            }
        }
        public void SetCurrentAmmo(int amount)
        {
            switch (activeAmmo)
            {
                case AmmoType.Bullet:
                    currentBullets = amount;
                    break;
                case AmmoType.Shell:
                    currentShells = amount;
                    break;
                case AmmoType.Explosive:
                    currentExplosives = amount;
                    break;
                case AmmoType.Bolt:
                    currentBolts = amount;
                    break;
                case AmmoType.Laser:
                    currentLasers = amount;
                    break;
            }
        }
        public void SpendCurrentAmmo(int amount)
        {
            switch (activeAmmo)
            {
                case AmmoType.Bullet:
                    currentBullets -= amount;
                    currentBullets = Mathf.Max(currentBullets, 0);
                    break;
                case AmmoType.Shell:
                    currentShells -= amount;
                    currentShells = Mathf.Max(currentShells, 0);
                    break;
                case AmmoType.Explosive:
                    currentExplosives -= amount;
                    currentExplosives = Mathf.Max(currentExplosives, 0);
                    break;
                case AmmoType.Bolt:
                    currentBolts -= amount;
                    currentBolts = Mathf.Max(currentBolts, 0);
                    break;
                case AmmoType.Laser:
                    currentLasers -= amount;
                    currentLasers = Mathf.Max(currentLasers, 0);
                    break;
            }
        }

        public void GiveAmmo(int amount, AmmoType ammoType)
        {
            Debug.Log("FishWeaponTracker.GiveAmmo: Giving " + amount + " to " + ammoType);

            switch (ammoType)
            {
                case AmmoType.Bullet:
                    currentBullets += amount;
                    currentBullets = Mathf.Min(currentBullets, maxBullets);
                    break;
                case AmmoType.Shell:
                    currentShells += amount;
                    currentShells = Mathf.Min(currentShells, maxShells);
                    break;
                case AmmoType.Explosive:
                    currentExplosives += amount;
                    currentExplosives = Mathf.Min(currentExplosives, maxExplosives);
                    break;
                case AmmoType.Bolt:
                    currentBolts += amount;
                    currentBolts = Mathf.Min(currentBolts, maxBolts);
                    break;
                case AmmoType.Laser:
                    currentLasers += amount;
                    currentLasers = Mathf.Min(currentLasers, maxLasers);
                    break;
                default:
                    break;
            }

            if (ammoType == activeAmmo)
            {
                bodySkillLocator.primary.stock = GetAmmoTypeCount(activeAmmo);
            }
        }

        public int GetFishAdjustedAmmoCount(int amount)
        {
            if (isFish)
            {
                return (int)MathF.Ceiling(1.25f * amount);
            }
            return amount;
        }

        public int GetAmmoTypePickupAmount(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case FishWeaponDef.AmmoType.Bullet:
                    return GetFishAdjustedAmmoCount(bulletsPerPickup);
                case FishWeaponDef.AmmoType.Shell:
                    return GetFishAdjustedAmmoCount(shellsPerPickup);
                case FishWeaponDef.AmmoType.Explosive:
                    return GetFishAdjustedAmmoCount(explosivesPerPickup);
                case FishWeaponDef.AmmoType.Bolt:
                    return GetFishAdjustedAmmoCount(boltsPerPickup);
                case FishWeaponDef.AmmoType.Laser:
                    return GetFishAdjustedAmmoCount(lasersPerPickup);
                default:
                    return 0;
            }
        }

        public AmmoType GetPrimaryAmmoType()
        {
            if (weaponData[0].weaponDef != null)
                return weaponData[0].weaponDef.ammoType;

            return AmmoType.None;
        }

        public AmmoType GetSecondaryAmmoType()
        {
            if (weaponData[1].weaponDef != null)
            {
                return weaponData[1].weaponDef.ammoType;
            }

            return AmmoType.None;
        }

        public float CalculateCurrentDropRateMultiplier()
        {
            float primaryMultiplier = 0.5f;
            float secondaryMultiplier = 0.5f;

            if (weaponData[0].weaponDef != null)
            {
                float primaryCount = GetAmmoTypeCount(GetPrimaryAmmoType());
                float primaryMax = GetAmmoTypeMax(GetPrimaryAmmoType());

                primaryMultiplier = GetDropRateAmmoThresholdMultiplier(primaryCount / primaryMax);
            }

            if (weaponData[1].weaponDef != null)
            {
                float secondaryCount = GetAmmoTypeCount(GetSecondaryAmmoType());
                float secondaryMax = GetAmmoTypeMax(GetSecondaryAmmoType());

                secondaryMultiplier = GetDropRateAmmoThresholdMultiplier(secondaryCount / secondaryMax);
            }

            return (primaryMultiplier + secondaryMultiplier);
        }

        public float GetDropRateAmmoThresholdMultiplier(float ammoPercentage)
        {
            if (ammoPercentage <= 0.2f)
                return 0.8f;

            else if (ammoPercentage >= 0.6f)
                return 0.15f;

            else
                return 0.5f;
        }

        private FishWeaponController fishWeaponController
        {
            get
            {
                if (_fishWeaponController != null) return _fishWeaponController;

                if (TryGetComponent(out CharacterMaster master))
                {
                    if (master.GetBody() != null)
                    {
                        _fishWeaponController = master.GetBody().GetComponent<FishWeaponController>();
                        return _fishWeaponController;
                    }
                }

                return null;
            }
        }

        private SkillLocator bodySkillLocator
        {
            get
            {
                if (_bodySkllLocator != null) return _bodySkllLocator;
                
                if (TryGetComponent(out CharacterMaster master))
                {
                    if (master.GetBody() != null)
                    {
                        _bodySkllLocator = master.GetBody().GetComponent<SkillLocator>();
                        return _bodySkllLocator;
                    }
                }

                return null;
            }
        }

        private void Inventory_onInventoryChanged()
        {
            if (!suppressValidation)
                ValidateWeapons();
        }

        private void Start()
        {
            inventory.onInventoryChanged += Inventory_onInventoryChanged;
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.onInventoryChanged -= Inventory_onInventoryChanged;
            }
        }

        public void SetFish(FishWeaponController fwt)
        {
            _fishWeaponController = fwt;
        }

        private void Init()
        {
            if (fishWeaponController != null)
            {
                FishWeaponDef startingWeapon = null;

                // add starting gun based on primary skilldef choice
                if (bodySkillLocator != null && bodySkillLocator.primary.skillDef is FishWeaponSkillDef fwsd && fwsd.weaponDef != null)
                {
                    startingWeapon = fwsd.weaponDef;
                }
                // if we can't find one from that, just give revolver
                else
                {
                    startingWeapon = Revolver.instance.weaponDef;
                }

                if (startingWeapon == null)
                {
                    // if you see this   what the fuck
                    Debug.LogError("FishWeaponTracker.Init : Starting weaponDef was null!! Is the revolver disabled?");
                    return;
                }
                else
                {
                    // give starting gun and 3 ammo packs of ammo

                    weaponData = new FishWeaponData[2];
                    weaponData[0] = new FishWeaponData { weaponDef = null };
                    weaponData[1] = new FishWeaponData { weaponDef = null };

                    AddWeaponItem(startingWeapon);

                    // otherwise we get a magical dupe that fucks everything up
                    // nightmare
                    // suppressValidation = true;
                    GiveAmmo(GetAmmoTypePickupAmount(startingWeapon.ammoType) * 3, startingWeapon.ammoType);
                    activeAmmo = startingWeapon.ammoType;
                    // suppressValidation = false;
                }

                SyncStocksAndAmmo();
                ValidateWeapons();

                Debug.Log("FishWeaponTracker.Init : Initializing with current ammo counts: Bullets " + currentBullets + " | Shells " + currentShells + " | Explosives " + currentExplosives + " | Bolts " + currentBolts + " | Lasers " + currentLasers);
            }

            hasInit = true;
        }
        
        public void SyncStocksAndAmmo()
        {
            if (bodySkillLocator != null)
            {
                bodySkillLocator.primary.stock = GetCurrentAmmoTypeRemaining();
            }
        }

        private void FinishInit()
        {
            hasInit = true;
        }

        public void SwapWeapons()
        {
            if (weaponData[offhandIndex].weaponDef == null)
            {
                Debug.LogError("FishWeaponTracker.SwapWeapon : Attempting to swtich with nothing to switch to!! Aborted");
                return;
            }

            // this is stupid and over-engineered as fuck for when i was thinking we'd want more than 2 weapons
            // that should just never happen. go play hunk
            int current = equippedIndex;
            equippedIndex = offhandIndex;
            offhandIndex = current;
        }

        public void SwapToWeapon(int index)
        {
            offhandIndex = equippedIndex;
            equippedIndex = index;
        }

        public void AddWeapon(FishWeaponDef weaponDef)
        {
            for (int i = 0; i < weaponData.Length; i++)
            {
                if (weaponData[i].weaponDef == weaponDef) return;
            }

            Array.Resize(ref weaponData, weaponData.Length + 1);

            bool hasStoredData = false;
            foreach (FishWeaponData fwd in storedWeaponData)
            {
                if (weaponDef == fwd.weaponDef)
                {
                    hasStoredData = true;
                    RemoveStoredData(weaponDef);

                    weaponData[weaponData.Length - 1] = new FishWeaponData
                    {
                        weaponDef = weaponDef
                    };
                }
            }

            if (!hasStoredData)
            {
                weaponData[weaponData.Length - 1] = new FishWeaponData
                {
                    weaponDef = weaponDef
                };
            }

            AddWeaponItem(weaponDef);
        }

        public void RemoveWeapon(FishWeaponDef weaponDef)
        {
            for (int i = 0; i < weaponData.Length; i++)
            {
                if (weaponData[i].weaponDef == weaponDef)
                {
                    DropWeapon(i, false);
                    return;
                }
            }
        }

        public void SwapWeapon(int index, bool createPickup = true, FishWeaponDef newWeaponDef = null)
        {
            if (index >= weaponData.Length || newWeaponDef == null) return;
            if (weaponData[index].weaponDef == null) return;

            foreach (FishWeaponData fwd in storedWeaponData)
            {
                if (fwd.weaponDef == weaponData[index].weaponDef) return;
            }

            FishWeaponDef weaponDef = weaponData[index].weaponDef;

            weaponData[index] = new FishWeaponData
            {
                weaponDef = newWeaponDef
            };

            // create pickup of dropped weapon
            if (NetworkServer.active && createPickup)
            {
                CreatePickupInfo pickupInfo = new CreatePickupInfo
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    pickupIndex = PickupCatalog.FindPickupIndex(weaponDef.itemDef.itemIndex)
                    // to-do: add duplicated tag
                };

                PickupDropletController.CreatePickupDroplet(pickupInfo, transform.position, Vector3.up);

                inventory.RemoveItem(weaponDef.itemDef, 100);
            }

            AddWeaponItem(newWeaponDef);
        }

        public void DropWeapon(int index, bool createPickup = true)
        {
            if (index >= weaponData.Length) return;
            if (weaponData[index].weaponDef == null) return;

            foreach (FishWeaponData fwd in storedWeaponData)
            {
                if (fwd.weaponDef == weaponData[index].weaponDef) return;
            }

            FishWeaponDef weaponDef = weaponData[index].weaponDef;

            Array.Resize(ref storedWeaponData, storedWeaponData.Length + 1);

            storedWeaponData[storedWeaponData.Length - 1] = new FishWeaponData
            {
                weaponDef = weaponDef
            };

            for (int i = index; i < weaponData.Length - 1; i++)
            {
                // move down
                weaponData[i] = weaponData[i + 1];
            }
            Array.Resize(ref weaponData, weaponData.Length - 1);

            // create pickup
            if (NetworkServer.active && createPickup)
            {
                CreatePickupInfo pickupInfo = new CreatePickupInfo
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    pickupIndex = PickupCatalog.FindPickupIndex(weaponDef.itemDef.itemIndex),
                    chest = fishWeaponController.chestBehavior,
                    // lol??
                };

                PickupDropletController.CreatePickupDroplet(pickupInfo, transform.position, Vector3.up);

                inventory.RemoveItem(weaponDef.itemDef, 100);
            }

            if (index < equippedIndex) equippedIndex--;

            if (offhandIndex == index)
            {
                offhandIndex = 0;
                for (int i = 0; i < weaponData.Length; i++)
                {
                    if (i != equippedIndex) offhandIndex = i;
                }
            }
            else
            {
                if (index < offhandIndex) offhandIndex--;
            }
        }

        public void RemoveStoredData(FishWeaponDef weaponDef)
        {
            int index = 0;
            for (int i = 0; i < weaponData.Length; i++)
            {
                if (storedWeaponData[i].weaponDef && storedWeaponData[i].weaponDef == weaponDef) index = i;
            }

            for (int i = 0; i < storedWeaponData.Length - 1; i++)
            {
                storedWeaponData[i] = storedWeaponData[i + 1];
            }

            Array.Resize(ref storedWeaponData, storedWeaponData.Length - 1);
        }

        private void AddWeaponItem(FishWeaponDef weaponDef)
        {
            if (!NetworkServer.active) return;

            suppressValidation = true;
            try
            {
                if (inventory.GetItemCount(weaponDef.itemDef) <= 0 && inventory.GetItemCount(weaponDef.itemDef) < 2)
                {
                    inventory.GiveItem(weaponDef.itemDef);
                }
                if (inventory.GetItemCount(weaponDef.itemDef) > 2)
                {
                    inventory.RemoveItem(weaponDef.itemDef, -(2 - inventory.GetItemCount(weaponDef.itemDef)));
                }

                for (int i = 0; i < weaponData.Length; i++)
                {
                    if (weaponData[i].weaponDef != null)
                    {
                        Debug.Log("FishWeaponTracker.AddWeaponItem : Weapon " + i + " is " + weaponData[i].weaponDef.name);
                    }
                    else
                    {
                        Debug.Log("FishWeaponTracker.AddWeaponItem : Weapon " + i + " is null");
                    }
                }
            }
            finally
            {
                suppressValidation = false;
            }
        }

        public void TryAddWeapon(FishWeaponDef newWeapon)
        {
            int newWeaponInstances = 0;
            for (int i = 0; i < weaponData.Length; i++)
            {
                if (weaponData[i].weaponDef == newWeapon)
                {
                    newWeaponInstances++;
                }
            }
            if (newWeaponInstances >= inventory.GetItemCount(newWeapon.itemDef))
            {
                return;
            }


            if (weaponData.Length < 2)
            {
                Array.Resize(ref weaponData, 2);
            }

            //for (int i = 0; i < weaponData.Length; i++)
            //{
            //    if (weaponData[i].weaponDef != null)
            //    {
            //        Debug.Log("FishWeaponTracker.TryAddWeapon : Weapon " + i + " is " + weaponData[i].weaponDef.name);
            //    }
            //    else
            //    {
            //        Debug.Log("FishWeaponTracker.TryAddWeapon : Weapon " + i + " is null");
            //    }
            //}

            // seek empty slots first
            for (int i = 0; i < weaponData.Length; i++)
            {
                if (weaponData[i].weaponDef == null)
                {
                    weaponData[i].weaponDef = newWeapon;
                    AddWeaponItem(newWeapon);
                    return;
                }
            }

            // i guess explicitly check that the offhand is empty and try to force it there :/
            if (weaponData[offhandIndex].weaponDef == null)
            {
                weaponData[offhandIndex].weaponDef = newWeapon;
                AddWeaponItem(newWeapon);
                return;
            }

            // drop primary and replace with new if both full and its actually a new gun
            // TO-DO: you should be able to pick up the same gun from the ground and swap it with held, even if the same
            // and each gun gives ammo the first time its picked up. but that might be ugly to track without a tag
            if (weaponData[equippedIndex].weaponDef != newWeapon)
            {
                FishWeaponDef droppedWeapon = weaponData[equippedIndex].weaponDef;
                weaponData[equippedIndex].weaponDef = newWeapon;

                if (NetworkServer.active)
                {
                    CreatePickupInfo pickupInfo = new CreatePickupInfo
                    {
                        position = fishWeaponController.transform.position,
                        rotation = fishWeaponController.transform.rotation,
                        pickupIndex = PickupCatalog.FindPickupIndex(droppedWeapon.itemDef.itemIndex),
                        chest = fishWeaponController.chestBehavior
                    };
                    PickupDropletController.CreatePickupDroplet(pickupInfo, fishWeaponController.transform.position, Vector3.up);
                    inventory.RemoveItem(droppedWeapon.itemDef, 1);
                }

                AddWeaponItem(newWeapon);
                fishWeaponController.EquipWeapon(equippedIndex);
            }
            else
            {
                // replace

                if (NetworkServer.active)
                {
                    CreatePickupInfo pickupInfo = new CreatePickupInfo
                    {
                        position = fishWeaponController.transform.position,
                        rotation = fishWeaponController.transform.rotation,
                        pickupIndex = PickupCatalog.FindPickupIndex(newWeapon.itemDef.itemIndex)
                    };
                    PickupDropletController.CreatePickupDroplet(pickupInfo, fishWeaponController.transform.position, Vector3.up);
                    inventory.RemoveItem(newWeapon.itemDef, 1);
                }
            }
        }

        public void ValidateWeapons()
        {
            if (isValidatingWeapons) return;
            isValidatingWeapons = true;

            try
            {
                foreach (FishWeaponDef fwd in FishWeaponCatalog.weaponDefs)
                {
                    if (inventory.GetItemCount(fwd.itemDef) > 0)
                    {
                        TryAddWeapon(fwd);
                    }
                    if (inventory.GetItemCount(fwd.itemDef) < 1)
                    {
                        RemoveWeapon(fwd);
                    }
                }
            }
            finally
            {
                isValidatingWeapons = false;
            }
        }
    }
}
