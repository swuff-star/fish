using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FishMod.Modules.Weapons
{
    public static class FishWeaponCatalog
    {
        public static Dictionary<string, FishWeaponDef> weaponDrops = new Dictionary<string, FishWeaponDef>();
        public static Dictionary<ItemDef, FishWeaponDef> itemWeaponKVPs = new Dictionary<ItemDef, FishWeaponDef>();
        public static FishWeaponDef[] weaponDefs = new FishWeaponDef[0];
        public static List<ItemDef> itemDefs = new List<ItemDef>(0);

        public static List<FishWeaponDef> availableWeapons = new List<FishWeaponDef>(0);
        public static List<FishWeaponDef> availableWeaponsMonster = new List<FishWeaponDef>(0);
        public static List<FishWeaponDef> availableWeaponsRobot = new List<FishWeaponDef>(0);
        public static List<FishWeaponDef> availableWeaponsCurse = new List<FishWeaponDef>(0);
        public static int scenesCleared = 0;

        public static void AddWeapon(FishWeaponDef weaponDef, bool addItem = true)
        {
            Array.Resize(ref weaponDefs, weaponDefs.Length + 1);

            int index = weaponDefs.Length - 1;
            weaponDef.index = (ushort)index;

            weaponDefs[index] = weaponDef;
            weaponDef.index = (ushort)index;

            if (addItem) itemDefs.Add(weaponDef.itemDef);

            Debug.Log("FishWeaponCatalog.AddWeapon : Added " + weaponDef.nameToken + " to Fish weapon catalog with index: " + weaponDef.index);
        }

        public static void AddWeaponDrop(string bodyName, FishWeaponDef weaponDef, bool autoComplete = true)
        {
            if (autoComplete)
            {
                if (!bodyName.Contains("Body")) bodyName += "Body";
                if (!bodyName.Contains("(Clone)")) bodyName += "(Clone)";
            }

            weaponDrops.Add(bodyName, weaponDef);
        }

        public static FishWeaponDef GetWeaponDefFromIndex(int index)
        {
            return weaponDefs[index];
        }

        public static FishWeaponDef GetWeaponDefFromSkillDef(FishWeaponSkillDef skillDef)
        {
            return skillDef.weaponDef;
        }

        public static FishWeaponDef GetWeaponDefFromItemDef(ItemDef itemDef)
        {
            if (itemWeaponKVPs.TryGetValue(itemDef, out FishWeaponDef fwd))
            {
                return fwd;
            }
            return null;
        }

        public static void RefreshAvailableWeapons()
        {
            availableWeapons.Clear();
            availableWeaponsMonster.Clear();
            availableWeaponsRobot.Clear();
            availableWeaponsCurse.Clear();

            Debug.Log("FishWeaponCatalog.RefreshAvailableOptions : Beginning refresh of catalog with " + scenesCleared + " scenes cleared");

            foreach (FishWeaponDef fwd in weaponDefs)
            {
                int stage = fwd.firstAvailableStage;

                if (stage <= scenesCleared)
                    availableWeapons.Add(fwd);

                if (stage < Math.Max(scenesCleared, 2))
                    availableWeaponsMonster.Add(fwd);

                if (stage <= scenesCleared + 1)
                    availableWeaponsRobot.Add(fwd);

                if (stage <= scenesCleared + 2)
                    availableWeaponsCurse.Add(fwd);
            }

            Debug.Log("FishWeaponCatalog.RefreshAvailableOptions : Finished refresh of catalog with a total of " + availableWeapons.Count + " weapons in pool");
        }

        public static FishWeaponDef RandomAvailableWeaponDef(int offset = 0)
        {
            // ??
            if (availableWeapons == null || availableWeapons.Count == 0)
            {
                Debug.LogError("FishWeaponCatalog.RandomAvailableWeaponDef : Failed to find a suitable weapon!");
                return null;
            }

            // because we're TOTALLY gonna have robot some day
            // cursed weapons maybe more feasible
            // (iirc enemy weapon drops are a tier below the current stage which is much more relevant than either)
            
            // weapons dropped by monsters are a tier below the current pool
            if (offset == -1)
            {
                return availableWeaponsMonster[UnityEngine.Random.Range(0, availableWeaponsMonster.Count)];
            }

            // when playing as robot (HAHAHAHAHA) weapons are a tier above 
            if (offset == 1)
            {
                return availableWeaponsRobot[UnityEngine.Random.Range(0, availableWeaponsRobot.Count)];
            }

            // if the weapon is cursed its two tiers above
            if (offset == 2)
            {
                return availableWeaponsCurse[UnityEngine.Random.Range(0, availableWeaponsCurse.Count)];
            }

            // to-do: version weighted towards higher tier weapons?
            // i think nuclear throne just gives everything the same one out of however many total chance
            return availableWeapons[UnityEngine.Random.Range(0, availableWeapons.Count)];
        }

        // gun gun
        public static FishWeaponDef RandomWeaponDef()
        {
            // what the fuckkkkk are you doing
            if (weaponDefs == null || weaponDefs.Count() == 0) return null; Debug.LogError("FishWeaponCatalog.RandomAvailableWeaponDef : Failed to find any weapons at all. Huh?");

            return weaponDefs[UnityEngine.Random.Range(0, weaponDefs.Length)];
        }
    }
}
