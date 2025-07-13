using FishMod.Modules.Weapons;
using FishMod.Modules.Weapons.Guns;
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
        private Inventory inventory;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();

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
        }

        public int GetAmmoTypePickupAmount(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case FishWeaponDef.AmmoType.Bullet:
                    return bulletsPerPickup;
                case FishWeaponDef.AmmoType.Shell:
                    return shellsPerPickup;
                case FishWeaponDef.AmmoType.Explosive:
                    return explosivesPerPickup;
                case FishWeaponDef.AmmoType.Bolt:
                    return boltsPerPickup;
                case FishWeaponDef.AmmoType.Laser:
                    return lasersPerPickup;
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

        public float CalculateCurrentDropMultiplier()
        {
            float primaryMultiplier = 0.5f;
            float secondaryMultiplier = 0.5f;

            if (weaponData[0].weaponDef != null)
            {
                float primaryCount = GetAmmoTypeCount(GetPrimaryAmmoType());
                float primaryMax = GetAmmoTypeMax(GetPrimaryAmmoType());

                primaryMultiplier = GetAmmoMultiplier(primaryCount / primaryMax);
            }

            if (weaponData[1].weaponDef != null)
            {
                float secondaryCount = GetAmmoTypeCount(GetSecondaryAmmoType());
                float secondaryMax = GetAmmoTypeMax(GetSecondaryAmmoType());

                secondaryMultiplier = GetAmmoMultiplier(secondaryCount / secondaryMax);
            }

            return (primaryMultiplier + secondaryMultiplier);
        }

        public float GetAmmoMultiplier(float ammoPercentage)
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

        private void OnDestroy()
        {
            if (inventory != null)
            {
                
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
                /*weaponData = new FishWeaponData[]
                {
                    new FishWeaponData
                    {
                        weaponDef = Revolver.instance.weaponDef
                    }
                };

                weaponData = new FishWeaponData[]
                {
                    new FishWeaponData
                    {
                        weaponDef = Machinegun.instance.weaponDef
                    }
                };*/

                AddWeaponItem(Revolver.instance.weaponDef);
                AddWeaponItem(Machinegun.instance.weaponDef);
                currentBullets = 120;
                ValidateWeapons();

                Debug.Log("FishWeaponTracker.Init : Initializing with current ammo counts: Bullets " + currentBullets + " | Shells " + currentShells + " | Explosives " + currentExplosives + " | Bolts " + currentBolts + " | Lasers " + currentLasers);
            }

            hasInit = true;
        }

        private void FinishInit()
        {
            hasInit = true;
        }

        public void SwapWeapons()
        {
            if (weaponData.Length < 2)
            {
                Debug.LogError("FishWeaponTracker.SwapWeapon : Attempting to swtich with nothing to switch to!! Aborted");
                return;
            }

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
                    pickupIndex = PickupCatalog.FindPickupIndex(weaponDef.itemDef.itemIndex)
                    // to-do: add duplicated tag
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
            if (inventory.GetItemCount(weaponDef.itemDef) <= 0) inventory.GiveItem(weaponDef.itemDef);
        }

        public void ValidateWeapons()
        {
            foreach (FishWeaponDef fwd in FishWeaponCatalog.weaponDefs)
            {
                if (inventory.GetItemCount(fwd.itemDef) > 0)
                {
                    AddWeapon(fwd);
                }

                if (inventory.GetItemCount(fwd.itemDef) <= 0)
                {
                    RemoveWeapon(fwd);
                }
            }
        }
    }
}
