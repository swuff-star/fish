using FishMod.Survivors.Fish;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FishMod.Characters.Survivors.Fish.Components
{
    public class ShellBehavior : MonoBehaviour
    {
        private ProjectileDamage damage;
        private float falloffTimer = 0f;
        private float baseDamage = 0f;
        private bool hasAppliedFalloff = false;
        public void OnStart()
        {
            if (gameObject.TryGetComponent(out ProjectileDamage pd))
            {
                damage = pd;
                baseDamage = pd.damage;
            }
            else
            {
                Log.Error("ShellBehavior.OnStart : Failed to find projectile damage on shell! Destroying ShellBehavior.");
                Destroy(this);
            }
        }

        public void FixedUpdate()
        {
            if (falloffTimer < FishStaticValues.shellFalloffDelay)
            {
                if (hasAppliedFalloff == false)
                {
                    hasAppliedFalloff = true;

                    damage.damage *= FishStaticValues.shellFalloffMultiplier;
                }
            }
            else
            {
                falloffTimer += Time.fixedDeltaTime;
            }
        }

        public void ResetDamage()
        {
            falloffTimer -= falloffTimer;
            damage.damage = baseDamage;
        }
    }
}
