using FishMod;
using FishMod.Modules.Weapons;
using FishMod.Survivors.Fish;
using RoR2;
using RoR2BepInExPack.GameAssetPaths;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.Fish
{
    public class BaseShootHitscan : BaseFishState
    {
        public static GameObject muzzleEffectPrefab = Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab;
        public static GameObject hitEffectPrefab = Commando.CommandoWeapon.FirePistol2.hitEffectPrefab;
        public static GameObject tracerEffectPrefab = FishAssets.laserTracerPrefab;

        public static float damageCoefficient = 1f;
        public static float procCoefficient = 1f;
        public static float force = 100f;
        public static float radius = 0.5f;
        public static float range = 500f;
        public static int minShotsToFire = 1;
        public static int maxShotsToFire = 1;
        public static float baseDuration = 1f;
        public static float baseTimeBetweenShots = 1f;
        public static float firePercentTime = 0.0f;
        public static string fireSoundString = "HenryShootPistol";
        public static float recoilAmplitude = 1f;
        public static float minSpread = 0f;
        public static float maxSpread = 1f;
        public static float spreadYawScale = 1f;
        public static float spreadPitchScale = 1f;
        public static float spreadBloomValue = 0.3f;
        public static float trajectoryAimAssistMultiplier = 1f;
        public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
        public static LayerMask hitMask = LayerIndex.CommonMasks.laser;
        public static string muzzleName = "Muzzle";
        public static bool lockAimRay = true;
        public static bool repeatSound = false;

        private float timeSinceLastShot;
        private float timeBetweenShots;
        private int shotsFired;
        private int shotsToFire;
        private float fireTime;
        private float stateDuration;
        private float reloadDuration;
        private Ray aimRay;
        private bool isCrit;
        private bool hasPlayedSound;

        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            stateDuration = baseDuration / attackSpeedStat;
            reloadDuration = baseDuration / attackSpeedStat;
            timeBetweenShots = baseTimeBetweenShots / attackSpeedStat;
            fireTime = firePercentTime * stateDuration;

            shotsToFire = UnityEngine.Random.Range(minShotsToFire, maxShotsToFire);

            // ensures first shot comes out immediately. a little jank but im tired.
            timeSinceLastShot = timeBetweenShots;

            aimRay = GetAimRay();
            isCrit = RollCrit();

            StartAimMode(aimRay, stateDuration * 1.5f);

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timeSinceLastShot += Time.fixedDeltaTime;
            if (Time.fixedDeltaTime >= fireTime && timeSinceLastShot >= timeBetweenShots)
            {
                Fire();
            }

            if (fixedAge >= stateDuration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            if (!hasPlayedSound || repeatSound)
            {
                Util.PlayAttackSpeedSound(fireSoundString, gameObject, attackSpeedStat);
                hasPlayedSound = true;
            }

            if (lockAimRay == false)
            {
                aimRay = GetAimRay();
            }

            aimRay.direction = Util.ApplySpread(aimRay.direction, minSpread, maxSpread, spreadYawScale, spreadPitchScale);

            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleName, false);
            }

            AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);

            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = maxSpread;
                bulletAttack.maxDistance = range;
                bulletAttack.damage = damageCoefficient * damageStat;
                bulletAttack.procCoefficient = procCoefficient;
                bulletAttack.force = force;
                bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
                bulletAttack.muzzleName = muzzleName;
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.hitMask = LayerIndex.CommonMasks.laser;
                bulletAttack.isCrit = isCrit;
                bulletAttack.radius = radius;
                bulletAttack.smartCollision = true;
                bulletAttack.trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier;
                bulletAttack.damageType = DamageTypeCombo.GenericPrimary;

                bulletAttack.Fire();

                bulletAttack.hitCallback = (BulletAttack bullet, ref BulletAttack.BulletHit hitInfo) =>
                {
                    if (hitInfo.hitHurtBox != null)
                    {
                        Log.Debug("BaseShootHitscan.Fire : Hitscan callback");
                        radius *= 0.8f;
                    }
                    return true;
                };
            }
            characterBody.AddSpreadBloom(spreadBloomValue);

            shotsFired++;

            if (shotsFired < maxShotsToFire)
            {
                timeSinceLastShot -= timeBetweenShots;
            }

            else
            {
                outer.SetNextStateToMain();
            }
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
