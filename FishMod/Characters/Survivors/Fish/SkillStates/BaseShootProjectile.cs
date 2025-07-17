using FishMod.Modules.Weapons;
using FishMod.Survivors.Fish;
using IL.RoR2.Projectile;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.Fish.Guns
{
    public class BaseShootProjectile : BaseFishState
    {
        public static GameObject muzzleEffectPrefab = Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab;
        public static GameObject projectilePrefab = FishAssets.bulletPrefab;

        public static float baseDuration = 0.2f;
        public static float baseDamageCoefficient = FishStaticValues.bulletDamageCoefficient;
        public static float baseForce = 100f;
        public static float firePercentTime = 0.0f;
        public static float shotsToFire = 1f;
        public static float baseTimeBetweenShots = 1f;
        public static string fireSoundString = "HenryShootPistol";
        public static float recoilAmplitude = 1f;
        public static float minSpread = 0f;
        public static float maxSpread = 1f;
        public static float spreadYawScale = 1f;
        public static float spreadPitchScale = 1f;
        public static float spreadBloomValue = 0.3f;
        public static string muzzleName = "Muzzle";

        private float fireTime;
        private float timeSinceLastShot;
        private float timeBetweenShots;
        private float shotsFired;
        private float stateDuration;
        private float reloadDuration;
        private Ray aimRay;
        private bool isCrit;

        private FishWeaponSkillDef fwsd;

        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            stateDuration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * stateDuration;
            timeBetweenShots = baseTimeBetweenShots / attackSpeedStat;

            // ensures first shot comes out immediately. a little jank but im tired.
            timeSinceLastShot = timeBetweenShots;

            if (skillLocator.primary.skillDef is FishWeaponSkillDef fishWeaponSkillDef)
            {
                fwsd = fishWeaponSkillDef;
                reloadDuration = fwsd.GetRechargeInterval(skillLocator.primary);
            }
            else
            {
                reloadDuration = baseDuration / attackSpeedStat;
            }

            aimRay = GetAimRay();
            isCrit = RollCrit();

            aimRay.direction = Util.ApplySpread(aimRay.direction, minSpread, maxSpread, spreadYawScale, spreadPitchScale);

            StartAimMode(aimRay, stateDuration * 1.5f);

            // PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", stateDuration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timeSinceLastShot += Time.fixedDeltaTime;
            if (Time.fixedDeltaTime >= fireTime && timeSinceLastShot >= timeBetweenShots)
            {
                Fire();
            }

            // this generally shouldn't be hit, since Fire() calls outer.SetNextStateToMain().
            if (fixedAge >= stateDuration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            if (!isAuthority) return;

            Util.PlayAttackSpeedSound(fireSoundString, gameObject, attackSpeedStat);

            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleName, false);
            }

            AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);

            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                owner = gameObject,
                damage = damageStat * baseDamageCoefficient,
                force = baseForce,
                crit = isCrit,
            };

            RoR2.Projectile.ProjectileManager.instance.FireProjectile(fireProjectileInfo);

            characterBody.AddSpreadBloom(spreadBloomValue);

            shotsFired++;

            if (shotsFired < shotsToFire)
            {
                timeSinceLastShot -= timeBetweenShots;
            }

            else
            {
                outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (weaponController != null) weaponController.ConsumeAmmo();

            if (skillLocator.primary.skillDef is FishWeaponSkillDef fishWeaponSkillDef)
            {
                fishWeaponSkillDef.pseudoCooldownRemaining = reloadDuration;
            }
        }
    }
}
