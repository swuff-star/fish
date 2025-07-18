﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.Fish.Guns
{
    public class FireMachinegun : BaseShootProjectile
    {
        public override void OnEnter()
        {
            baseDuration = 0.2f;
            maxSpread = 1.5f;
            baseForce = baseDamageCoefficient * 100f;
            base.OnEnter();
        }
    }
}
