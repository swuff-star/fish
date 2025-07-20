using FishMod.Survivors.Fish;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.Fish.Guns
{
    public class FireDoubleShotgun : BaseShootProjectile
    {
        public override void OnEnter()
        {
            baseDuration = 0.93f;
            baseForce = baseDamageCoefficient * 100f;
            shotsToFire = 14f;
            baseTimeBetweenShots = 0f;
            maxSpread = 7.5f;
            ammoToComsume = 2;
            spreadShots = true;
            projectilePrefab = FishAssets.shellPrefab;
            base.OnEnter();
        }
    }
}
