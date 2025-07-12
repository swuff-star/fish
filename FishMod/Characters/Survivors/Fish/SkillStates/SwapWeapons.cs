using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.Fish
{
    public class SwapWeapons : BaseFishState
    {
        public float baseDuration = 0.15f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            weaponController.CycleWeapon();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (duration >= fixedAge)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
