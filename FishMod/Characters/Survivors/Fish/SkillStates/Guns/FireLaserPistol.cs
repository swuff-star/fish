using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.Fish.Guns
{
    public class FireLaserPistol : BaseShootHitscan
    {
        public override void OnEnter()
        {
            baseDuration = 0.3f;
            baseTimeBetweenShots = 0.01f;
            minShotsToFire = 5;
            maxShotsToFire = 5;

            base.OnEnter();
        }
    }
}
