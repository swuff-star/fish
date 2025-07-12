using FishMod.Characters.Survivors.Fish.Components;
using FishMod.Modules.Weapons;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.Fish
{
    public class BaseFishState : BaseSkillState
    {
        protected FishWeaponController weaponController;
        protected FishWeaponDef cachedWeaponDef;

        public override void OnEnter()
        {
            weaponController = GetComponent<FishWeaponController>();
            if (weaponController != null) cachedWeaponDef = weaponController.weaponDef;

            base.OnEnter();
        }
    }
}
