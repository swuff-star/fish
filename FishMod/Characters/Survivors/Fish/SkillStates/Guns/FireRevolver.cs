using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.Fish.Guns
{
    public class FireRevolver : BaseShootProjectile
    {
        public override void OnEnter()
        {
            baseDuration = 0.17f;
            baseForce = baseDamageCoefficient * 100f;
            base.OnEnter();
        }
    }
}
