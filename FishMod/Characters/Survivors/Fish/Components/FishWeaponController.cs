using FishMod.Modules.Weapons;
using RoR2.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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

        // current + offhand weapons
        public FishWeaponDef weaponDef;
        private FishWeaponDef lastWeaponDef;

        public int currentAmmo;
        public int currentMaxAmmo;

        private FishWeaponTracker _weaponTracker;

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

        public void EquipWeapon(int index, bool setAmmo = true)
        {
            if (index >= weaponTracker.weaponData.Length)
            {
                weaponTracker.ValidateWeapons();
            }

            weaponTracker.equippedIndex = index;

            weaponDef = weaponTracker.weaponData[index].weaponDef;

            if (skillOverride != null)
            {
                skillLocator.primary.UnsetSkillOverride(gameObject, skillOverride, GenericSkill.SkillOverridePriority.Network);
            }

            skillOverride = weaponDef.primarySkillDef;

            skillLocator.primary.SetSkillOverride(gameObject, skillOverride, GenericSkill.SkillOverridePriority.Network);

            weaponTracker.SetAmmoType(weaponDef.ammoType);

            Debug.Log("FishWeaponController.EquipWeapon : Current weapon ammo type should be " + weaponDef.ammoType + ", and is currently " + weaponTracker.activeAmmo);

            Debug.Log("FishWeaponController.EquipWeapon : Equipping " + weaponDef.name + " with " + weaponTracker.GetCurrentAmmoTypeRemaining() + "/" + weaponTracker.GetCurrentAmmoTypeMax() + " of type " + weaponTracker.activeAmmo);

            skillLocator.primary.maxStock = weaponTracker.GetCurrentAmmoTypeMax();
            skillLocator.primary.stock = weaponTracker.GetCurrentAmmoTypeRemaining();

            Debug.Log("FishWeaponController.EquipWeapon : Primary override has " + skillLocator.primary.stock + " / " + skillLocator.primary.maxStock);

            if (onWeaponUpdate == null) return;
            onWeaponUpdate(this);
        }

        public void CycleWeapon()
        {
            weaponTracker.SwapWeapons();
            EquipWeapon(weaponTracker.equippedIndex);
        }

        public void ConsumeAmmo(int amount = 1)
        {
            Debug.Log("FishWeaponController.ConsumeAmmo : " + weaponDef.name + " firing " + amount + " " + weaponTracker.activeAmmo);
            if (characterBody.HasBuff(RoR2Content.Buffs.NoCooldowns)) return;

            if (currentAmmo <= weaponTracker.GetCurrentAmmoTypeRemaining()) weaponTracker.SpendCurrentAmmo(amount);

            currentAmmo -= amount;

            if (currentAmmo <= 0) currentAmmo = 0;

            if (weaponTracker.GetCurrentAmmoTypeRemaining() <= 0) weaponTracker.SetCurrentAmmo(0);

            skillLocator.primary.DeductStock(amount);
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
