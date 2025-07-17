using FishMod.Modules.Weapons;
using RoR2.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static FishMod.Modules.Weapons.FishWeaponDef;
using FishMod.Survivors.Fish;

namespace FishMod.Characters.Survivors.Fish.Components
{
    public class FishWeaponController : MonoBehaviour
    {
        public ushort syncedWeapon;
        public NetworkInstanceId netId;

        public Action<FishWeaponController> onWeaponUpdate;
        public static Action<Inventory> onInventoryUpdate;

        public CharacterBody characterBody { get; private set; }
        public ChildLocator childLocator { get; private set; }
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        private SkillDef skillOverride;
        public ChestBehavior chestBehavior = new ChestBehavior();

        // current + offhand weapons
        public FishWeaponDef weaponDef;

        public int currentAmmo;
        public int currentMaxAmmo;

        private FishWeaponTracker _weaponTracker;


        private GameObject currentWeaponMdl;
        private Transform heldTransform;

        private Inventory inventory
        {
            get
            {
                if (characterBody != null && characterBody.inventory != null) return characterBody.inventory;
                if (weaponTracker != null && weaponTracker.TryGetComponent(out CharacterMaster master)) return master.inventory;
                return null;
            }
        }

        public FishWeaponTracker weaponTracker
        {
            get
            {
                if (_weaponTracker != null) return _weaponTracker;

                if (characterBody != null && characterBody.master != null)
                {
                    FishWeaponTracker fwt = characterBody.master.GetComponent<FishWeaponTracker>();

                    if (fwt == null) fwt = characterBody.master.gameObject.AddComponent<FishWeaponTracker>();
                    fwt.SetFish(this);
                    _weaponTracker = fwt;
                    return fwt;
                }
                return null;
            }
        }

        public void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            skillLocator = GetComponent<SkillLocator>();

            ModelLocator modelLocator = GetComponent<ModelLocator>();

            childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();

            heldTransform = childLocator.FindChild("GunTransform");

            Invoke("SetInventoryHook", 0.5f);
        }

        private void SetInventoryHook()
        {
            if (inventory)
            {
                inventory.onInventoryChanged += Inventory_onInventoryChanged;
                // inventory.onItemAddedClient += Inventory_onItemAddedClient;
            }
        }

        private void Inventory_onInventoryChanged()
        {
            if (onInventoryUpdate != null)
            {
                onInventoryUpdate(inventory);
            }
        }

        private void Start()
        {
            Invoke("Init", 0.3f);
        }

        private void Init()
        {
            EquipWeapon(weaponTracker.equippedIndex);
        }

        public void EquipWeapon(int index)
        {
            if (index >= weaponTracker.weaponData.Length)
            {
                weaponTracker.ValidateWeapons();
            }

            weaponTracker.equippedIndex = index;

            weaponDef = weaponTracker.weaponData[index].weaponDef;

            float cooldownRemaining = 0f;
            if (skillLocator.primary.skillDef is FishWeaponSkillDef fwsd) cooldownRemaining = fwsd.pseudoCooldownRemaining;

            if (skillOverride != null)
            {
                skillLocator.primary.UnsetSkillOverride(gameObject, skillOverride, GenericSkill.SkillOverridePriority.Network);
            }

            skillOverride = weaponDef.primarySkillDef;

            skillLocator.primary.SetSkillOverride(gameObject, skillOverride, GenericSkill.SkillOverridePriority.Network);

            weaponTracker.SetAmmoType(weaponDef.ammoType);

            Debug.Log("FishWeaponController.EquipWeapon : Current weapon ammo type should be " + weaponDef.ammoType + ", and is currently " + weaponTracker.activeAmmo);

            Debug.Log("FishWeaponController.EquipWeapon : Equipping " + weaponDef.name + " with " + weaponTracker.GetCurrentAmmoTypeRemaining() + "/" + weaponTracker.GetAmmoTypeMax(weaponDef.ammoType) + " of type " + weaponTracker.activeAmmo);

            skillLocator.primary.maxStock = weaponTracker.GetAmmoTypeMax(weaponDef.ammoType);
            skillLocator.primary.stock = weaponTracker.GetCurrentAmmoTypeRemaining();
            if (skillLocator.primary.skillDef is FishWeaponSkillDef fwsd2) fwsd2.pseudoCooldownRemaining = weaponTracker.offhandRemainingCooldown;

            Debug.Log("FishWeaponController.EquipWeapon : Equipped weapon currently has " + weaponTracker.offhandRemainingCooldown + " remaining reload time");

            weaponTracker.offhandRemainingCooldown = cooldownRemaining;

            weaponTracker.SyncStocksAndAmmo();

            Debug.Log("FishWeaponController.EquipWeapon : Secondary currently has " + weaponTracker.offhandRemainingCooldown + " remaining reload time");

            Debug.Log("FishWeaponController.EquipWeapon : Primary override has " + skillLocator.primary.stock + " / " + skillLocator.primary.maxStock);

            if (currentWeaponMdl != null)
            {
                Destroy(currentWeaponMdl);
            }

            // this shit will NOT network
            // hopefully can reimplement using item displays?
            // not hard to network if needed but this implementation isnt ideal given we already have systems for adding meshes when items are owned..
            if (heldTransform != null)
            {
                currentWeaponMdl = Instantiate(weaponDef.modelPrefab, heldTransform);
                currentWeaponMdl.layer = LayerIndex.noCollision.intVal;
                currentWeaponMdl.transform.SetParent(heldTransform);
            }

            if (onWeaponUpdate == null) return;
            onWeaponUpdate(this);
        }

        public void CycleWeapon()
        {
            for (int i = 0; i < weaponTracker.weaponData.Length; ++i)
            {
                if (weaponTracker.weaponData[i].weaponDef == null)
                {
                    Debug.LogWarning("FishWeaponController.EquipWeapon : Attempting to switch weapons with no current secondary!");
                    return;
                }
            }
            weaponTracker.SwapWeapons();
            EquipWeapon(weaponTracker.equippedIndex);
        }

        public float GetCurrentDropMultiplier()
        {
            return weaponTracker.CalculateCurrentDropRateMultiplier();
        }

        public bool MaxAmmo()
        {
            return weaponTracker.MaxAmmo();
        }

        public void ConsumeAmmo(int amount = 1)
        {
            // Debug.Log("FishWeaponController.ConsumeAmmo : " + weaponDef.name + " firing " + amount + " " + weaponTracker.activeAmmo);
            // no cooldowns = infinite ammo >:)
            if (characterBody.HasBuff(RoR2Content.Buffs.NoCooldowns)) return;

            amount = Mathf.Max(amount, 0);

            weaponTracker.SpendCurrentAmmo(amount);

            skillLocator.primary.DeductStock(amount);
        }

        public void GiveAmmoOfType(int amount = 1, AmmoType type = AmmoType.None)
        {
            weaponTracker.GiveAmmo(amount, type);
        }

        public void GiveAmmoPackOfType(AmmoType type = AmmoType.None)
        {
            weaponTracker.GiveAmmo(weaponTracker.GetAmmoTypePickupAmount(type), type);
        }

        public void GiveAmmoPack()
        {
            Debug.Log("FishWeaponController.ApplyAmmoPack : Attempting to apply ammo..");

            // get the currently in-use weapon types
            AmmoType primaryType = weaponTracker.GetPrimaryAmmoType();
            AmmoType secondaryType = weaponTracker.GetSecondaryAmmoType();

            Debug.Log("FishWeaponController.ApplyAmmoPack : Primary is " + primaryType + " | Secondary is " + secondaryType);
            List<AmmoType> activeTypes = new List<AmmoType>();

            // make a list of all weapon types that can get ammo
            // if a weapon is at full ammo, or a weapon is melee, it'll roll for a weapon type that's independent of the other
            // so we want a list of ammo types that AREN'T in use-
            // to do that, we initialize a list of all of them and then subtract later
            List<AmmoType> otherAmmoTypes = new List<AmmoType>();
            otherAmmoTypes.Add(AmmoType.Bullet);
            otherAmmoTypes.Add(AmmoType.Shell);
            otherAmmoTypes.Add(AmmoType.Explosive);
            otherAmmoTypes.Add(AmmoType.Bolt);
            otherAmmoTypes.Add(AmmoType.Laser);

            bool invalidPrimary = false;
            bool invalidSecondary = false;

            if (primaryType != AmmoType.None && weaponTracker.GetAmmoTypeCount(primaryType) < weaponTracker.GetAmmoTypeMax(primaryType))
            {
                // add primary to the list of types in-use
                activeTypes.Add(primaryType);
            }
            else
            {
                // primary either uses no ammo, or is full
                invalidPrimary = true;
            }

            // remove the primary type from the list of other ammo types
            if (otherAmmoTypes.Contains(primaryType)) otherAmmoTypes.Remove(primaryType);

            if (secondaryType != AmmoType.None && weaponTracker.GetAmmoTypeCount(secondaryType) < weaponTracker.GetAmmoTypeMax(secondaryType))
            {
                // add secondary to the list of types in-use
                if (activeTypes.Contains(secondaryType) == false)
                {
                    activeTypes.Add(secondaryType);
                }
            }
            else
            {
                // secondary either uses no ammo, or is full
                invalidSecondary = true;
            }

            // remove the secondary type from the list of other ammo types
            if (otherAmmoTypes.Contains(secondaryType)) otherAmmoTypes.Remove(secondaryType);

            // final type to give ammo for
            AmmoType chosenType;

            // if both are valid, roll between them exclusively
            if (invalidPrimary == false && invalidSecondary == false)
            {
                chosenType = activeTypes[UnityEngine.Random.Range(0, activeTypes.Count)];

                Debug.Log("FishWeaponController.ApplyAmmoPack : Both ammo types were valid.");
            }

            // if both are invalid (none/full), roll another ammo type
            else if (invalidPrimary && invalidSecondary)
            {
                chosenType = otherAmmoTypes[UnityEngine.Random.Range(0, otherAmmoTypes.Count)];

                Debug.Log("FishWeaponController.ApplyAmmoPack : Neither ammo types were valid.");
            }

            // if only one is invalid, 50% chance to give to the valid, and 50% to give a different one instead
            else
            {
                if (UnityEngine.Random.value < 0.5f)
                {
                    chosenType = activeTypes[UnityEngine.Random.Range(0, activeTypes.Count)];
                }

                else
                {
                    chosenType = otherAmmoTypes[UnityEngine.Random.Range(0, otherAmmoTypes.Count)];
                }

                if (invalidPrimary)
                {
                    Debug.Log("FishWeaponController.ApplyAmmoPack : Primary ammo type was invalid.");
                }

                if (invalidSecondary)
                {
                    Debug.Log("FishWeaponController.ApplyAmmoPack : Secondary ammo type was invalid.");
                }
            }

            Debug.Log("FishWeaponController.ApplyAmmoPack : Selected ammo type is " + chosenType);

            int ammoAmount = weaponTracker.GetAmmoTypePickupAmount(chosenType);

            if (chosenType == primaryType)
            {
                // >
                skillLocator.primary.DeductStock(-ammoAmount);
            }

            weaponTracker.GiveAmmo(ammoAmount, chosenType);

            Debug.Log("FishWeaponController.ApplyAmmoPack : End of ApplyAmmoPack. Current ammo is now: ");
            Debug.Log("FishWeaponController.ApplyAmmoPack :     Bullets " + weaponTracker.currentBullets);
            Debug.Log("FishWeaponController.ApplyAmmoPack :     Shells " + weaponTracker.currentShells);
            Debug.Log("FishWeaponController.ApplyAmmoPack :     Explosives " + weaponTracker.currentExplosives);
            Debug.Log("FishWeaponController.ApplyAmmoPack :     Bolts " + weaponTracker.currentBolts);
            Debug.Log("FishWeaponController.ApplyAmmoPack :     Lasers " + weaponTracker.currentLasers);
        }

        /*public void ServerGetStoredWeapon(FishWeaponDef newWeapon, float ammo, FishWeaponController fwc)
        {
            if (fwc.gameObject.TryGetComponent(out NetworkIdentity identity))
            {
                new SyncStoredWeapon(identity.netId, newWeapon.index, ammo).Send(NetworkDestination.Clients);
            }
        }

        public void ServerDropWeapon(int index)
        {
            if (!NetworkServer.active) return;

            if (TryGetComponent(out NetworkIdentity identity))
            {
                new SyncGunDrop2(identity.netId, index).Send(NetworkDestination.Clients);
            }
        }*/

        public void ClientDropWeapon(int index)
        {
            weaponTracker.DropWeapon(index);
        }

        public void ServerSetWeapon(int index)
        {
            if (!NetworkServer.active) return;

            weaponTracker.nextWeapon = index;
        }
    }
}
