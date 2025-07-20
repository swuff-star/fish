using FishMod.Survivors.Fish;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.Fish.Guns
{
    public class FireShotgun : BaseShootProjectile
    {
        public override void OnEnter()
        {
            baseDuration = 0.57f;
            baseForce = baseDamageCoefficient * 100f;
            shotsToFire = 7f;
            baseTimeBetweenShots = 0f;
            maxSpread = 5f;
            spreadShots = true;
            projectilePrefab = FishAssets.shellPrefab;
            base.OnEnter();
        }
    }
}
