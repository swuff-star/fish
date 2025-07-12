using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FishMod.Modules.Weapons
{
    public static class FishWeaponCatalog
    {
        public static Dictionary<string, FishWeaponDef> weaponDrops = new Dictionary<string, FishWeaponDef>();
        public static FishWeaponDef[] weaponDefs = new FishWeaponDef[0];
        public static List<ItemDef> itemDefs = new List<ItemDef>(0);

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
    }
}
