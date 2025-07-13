using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace FishMod.Modules.Weapons
{
    public class FishWeaponSkillDef : SkillDef
    {
        // ughhhhhhh
        public float basePseudoCooldown;
        public float pseudoCooldownRemaining = 0f;

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            basePseudoCooldown = baseRechargeInterval;
            return base.OnAssigned(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return skillSlot.stock >= requiredStock && pseudoCooldownRemaining <= 0f;
        }

        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
        {
            base.OnFixedUpdate(skillSlot, deltaTime);

            if (pseudoCooldownRemaining > 0f)
            {
                pseudoCooldownRemaining -= Time.fixedDeltaTime;
            }
            
            if (pseudoCooldownRemaining < 0f)
            {
                pseudoCooldownRemaining = 0f;
            }
        }
    }
}
